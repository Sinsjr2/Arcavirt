using System.Collections.Concurrent;
using System.Threading.Channels;

namespace GdbStubDotnet;

/// <summary>
/// resume(vCont/c/s)の停止報告を仲介する内部コンポーネント。
/// ExecutionResponder.ReportStop/Reject の唯一の入口であり、報告が
/// 「どの resume にスコープされたものか」を token で判定する(§4.4:
/// resume 外の ReportStop は無視する規律)。
/// Mode(ResumeMode)に応じて OnReportStop の配送先・合流方針が切り替わる:
///   - AllStop: token を CAS で消費し、最初に届いた1件だけを接続共有の
///     ExecOutcome チャネルへ単一 stop reply として配送する(§4.3手順7
///     「多スレッド停止の合流(代表1件)」。他スレッドからの後続の報告は
///     token 消費済みのため無視される)。
///   - NonStop: token は消費せず(このresume episode中は複数スレッドが
///     独立に停止しうるため)、episode内で既に報告済みのスレッドを
///     _reportedThreadsInEpisode で重複排除しつつ、スレッドごとに
///     NotificationQueue へ %Stop 通知として Enqueue する
///     (Arcavirt-o3e.10.4)。
/// OnReject は Mode を問わず常に ExecOutcome チャネル経由(resume要求自体が
/// 不正なときの同期応答であり、%Stop通知の対象ではない)。
/// CurrentThread は H(SetThread、Arcavirt-o3e.14)による現在スレッド選択
/// 状態を保持する。接続ごとに単一のI/Oループ上でのみ読み書きされる
/// (§4.2)ため、追加の同期は不要。
/// DetailedErrors/OnError は §7.2/§7.3(Arcavirt-o3e.18)のエラー処理設定を
/// 保持する(Build()時に確定する読み取り専用設定)。
/// </summary>
internal sealed class ExecutionCoordinator {
    private readonly ChannelWriter<ExecOutcome> _writer;
    private readonly NotificationQueue _notificationQueue;
    private readonly ConcurrentDictionary<(int Token, ThreadId Thread), byte> _reportedThreadsInEpisode = new();
    private int _tokenSeed;
    private int _activeToken;
    private volatile ResumeMode _mode;
    private ThreadId _currentThread;

    internal ExecutionCoordinator(ChannelWriter<ExecOutcome> writer, NotificationQueue notificationQueue, bool detailedErrors, Action<StubFault>? onError) {
        _writer = writer;
        _notificationQueue = notificationQueue;
        DetailedErrors = detailedErrors;
        OnError = onError;
    }

    internal ResumeMode Mode => _mode;

    internal ThreadId CurrentThread => _currentThread;

    /// <summary>
    /// §7.3。true でハンドラエラーを E.&lt;text&gt;(人間可読)、false
    /// (既定)で E NN として応答する。
    /// </summary>
    internal bool DetailedErrors { get; }

    /// <summary>
    /// ハンドラ例外の診断通知先(§7.2)。GDB へは安全な E NN 応答を送りつつ、
    /// 例外自体はこちらへのみ通知する(ILogger 依存なし)。
    /// </summary>
    internal Action<StubFault>? OnError { get; }

    /// <summary>
    /// QNonStop:1/0 受信時に呼ばれ、以後の OnReportStop の配送先を切り替える。
    /// </summary>
    internal void SetMode(ResumeMode mode) {
        _mode = mode;
    }

    /// <summary>
    /// H(SetThread)受信時に呼ばれ、以後の IThreadScoped コマンドの
    /// Thread フィールドへ自動的に充填される現在スレッドを更新する(§6.6)。
    /// </summary>
    internal void SetCurrentThread(ThreadId thread) {
        _currentThread = thread;
    }

    /// <summary>
    /// resume を1つ開始し、この resume にスコープされた ExecutionResponder を
    /// 返す。token は Interlocked.Increment により一意に払い出され、
    /// 以後この resume からの報告だけが token 判定に成功する。
    /// _reportedThreadsInEpisode は (token, Thread) の複合キーで管理して
    /// おり、新しい token は過去のどの (token, Thread) エントリとも一致
    /// しないため、ここで明示的にクリアする必要はない(クリアを挟むと
    /// 「古いtokenの判定通過後・新episodeのクリア後」という極小window で
    /// 古い報告が新episodeの重複排除表へ紛れ込む競合状態が生じるため、
    /// 意図的にクリアしない設計にしている)。
    /// </summary>
    internal ExecutionResponder BeginResume() {
        int token = Interlocked.Increment(ref _tokenSeed);
        Interlocked.Exchange(ref _activeToken, token);
        return new ExecutionResponder(this, token);
    }

    /// <summary>
    /// token が現在アクティブな resume の token と一致する場合のみ CAS で
    /// 消費する(§4.4)。OnReportStop の AllStop 経路・OnReject の両方が
    /// 使う共通判定であり、本コンポーネントの不変条件。簡略化・省略しては
    /// ならない。
    /// </summary>
    private bool TryConsumeToken(int token) {
        return Interlocked.CompareExchange(ref _activeToken, 0, token) == token;
    }

    /// <summary>
    /// token が現在アクティブな resume の token と一致するかだけを判定する
    /// (消費はしない)。ExecDispatchedCommand が、resume 開始直後に
    /// ハンドラが同期的に Reject を呼んだかどうかを判定するために使う
    /// (Reject は Mode を問わず TryConsumeToken で token を消費するため、
    /// ReportStop(NonStopでは token を消費しない)の有無に関わらず、
    /// Reject が起きたかどうかだけを正確に反映する)。
    /// </summary>
    internal bool IsPending(int token) {
        return Volatile.Read(ref _activeToken) == token;
    }

    /// <summary>
    /// resume を異常終了させる(ハンドラが例外を投げた場合)。token が
    /// 現在アクティブなら消費して以後の報告を無効化する。既に
    /// ReportStop/Reject 済みなら何もしない(CASが失敗するだけで安全)。
    /// </summary>
    internal void AbortResume(int token) {
        Interlocked.CompareExchange(ref _activeToken, 0, token);
    }

    /// <summary>
    /// AllStop では token が現在アクティブな resume の token と一致する
    /// 場合のみ CAS で消費し、最初の1件を単一 stop reply として配送する
    /// (§4.3手順7、代表1件による合流)。一致しない場合(既に消費済み、
    /// 他スレッドの後続報告、または別の resume に切り替わった後の遅延・
    /// 重複呼び出し)は§4.4の規律に従い無視する。
    /// NonStop では token を消費せず(複数スレッドが独立に停止しうる
    /// ため)、episode内のスレッド単位で重複排除しつつ複数回配送を許す
    /// (Arcavirt-o3e.10.4)。判定は Volatile.Read の読み取り専用チェックで
    /// 行い、_reportedThreadsInEpisode のキーに token 自体を含めることで、
    /// 「チェック通過後に別の BeginResume が割り込む」タイミング依存の
    /// すり抜けを構造的に防ぐ(旧・単純なThreadIdキー+Clear()方式では
    /// この窓が存在した)。
    /// </summary>
    internal void OnReportStop(int token, in StopEvent stop) {
        if (_mode == ResumeMode.NonStop) {
            if (Volatile.Read(ref _activeToken) != token) {
                return;
            }
            if (_reportedThreadsInEpisode.TryAdd((token, stop.Thread), 0)) {
                _notificationQueue.Enqueue(new StopNotification(stop));
            }
            return;
        }
        if (!TryConsumeToken(token)) {
            return;
        }
        _writer.TryWrite(new ExecOutcome(false, stop, default));
    }

    /// <summary>
    /// OnReportStop の AllStop 経路と同じ token 判定を Reject 経路に適用する。
    /// Reject は resume要求自体が不正なときの同期応答であり、Mode を問わず
    /// 常に ExecOutcome チャネル経由で伝える(%Stop通知の対象ではない)。
    /// </summary>
    internal void OnReject(int token, RspError error) {
        if (!TryConsumeToken(token)) {
            return;
        }
        _writer.TryWrite(new ExecOutcome(true, default, error));
    }
}