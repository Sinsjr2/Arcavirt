using System.Collections.Concurrent;
using System.Threading.Channels;

namespace GdbStubDotnet;

/// <summary>
/// resume(vCont/c/s)の停止報告を仲介する内部コンポーネント。
/// ExecutionResponder.ReportStop/Reject の唯一の入口であり、報告が
/// 「どの resume にスコープされたものか」を token で判定する(§4.4:
/// resume 外の ReportStop は無視する規律)。
/// _activeTokens は「現在有効な resume episode の token 集合」を表す。
/// BeginResume 時点の Mode によって集合の扱いが変わる(Arcavirt-cwm):
///   - AllStop: 1本勝負(実gdbはstop reply受信前に次のresumeを送らない
///     前提)。新しい resume が始まったら集合を Clear してから新token を
///     追加し、直前までの token を即座に無効化する(古い resume からの
///     遅延報告は§4.4の規律により無視されるべきため)。
///   - NonStop: 複数スレッドが「別々のvCont呼び出し」で独立に resume
///     されうる(例: threadAをcontinueさせたまま、後からthreadBだけを
///     個別にcontinueさせる)。この場合、新しい BeginResume は既存の
///     token を無効化してはならない。集合はクリアせず単純に追加する。
/// Mode(ResumeMode)に応じて OnReportStop の配送先・合流方針も切り替わる:
///   - AllStop: token を集合から CAS 相当(TryRemove)で消費し、最初に
///     届いた1件だけを接続共有の ExecOutcome チャネルへ単一 stop reply
///     として配送する(§4.3手順7「多スレッド停止の合流(代表1件)」。
///     他スレッドからの後続の報告は token 消費済みのため無視される)。
///   - NonStop: token は消費せず(このresume episode中は複数スレッドが
///     独立に停止しうるため)、episode内で既に報告済みのスレッドを
///     _reportedThreadsInEpisode で重複排除しつつ、スレッドごとに
///     NotificationQueue へ %Stop 通知として Enqueue する
///     (Arcavirt-o3e.10.4)。NonStopのtokenはReportStopで消費されない
///     ため _activeTokens に残り続ける(_reportedThreadsInEpisode と
///     同様、接続が有限寿命であることを前提に無制限増加を許容している。
///     Arcavirt-o3e.10.4で既に受容済みの特性と同じ考え方)。
/// OnReject は Mode を問わず常に ExecOutcome チャネル経由(resume要求自体が
/// 不正なときの同期応答であり、%Stop通知の対象ではない)。
/// CurrentThread は H(SetThread、Arcavirt-o3e.14)による現在スレッド選択
/// 状態を保持する。接続ごとに単一のI/Oループ上でのみ読み書きされる
/// (§4.2)ため、追加の同期は不要。BeginResume も同じI/Oループ上でのみ
/// 呼ばれるため、「BeginResume時点でのMode」が一貫して観測できる。
/// DetailedErrors/OnError は §7.2/§7.3(Arcavirt-o3e.18)のエラー処理設定を
/// 保持する(Build()時に確定する読み取り専用設定)。
/// </summary>
internal sealed class ExecutionCoordinator {
    private readonly ChannelWriter<ExecOutcome> _writer;
    private readonly NotificationQueue _notificationQueue;
    private readonly ConcurrentDictionary<(int Token, ThreadId Thread), byte> _reportedThreadsInEpisode = new();
    private readonly ConcurrentDictionary<int, byte> _activeTokens = new();
    private int _tokenSeed;
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
    /// 返す。token は Interlocked.Increment により一意に払い出される。
    /// AllStop では「1本勝負」の不変条件を保つため、新token追加前に
    /// _activeTokens をクリアし、直前までの token を即座に無効化する
    /// (実gdbはstop reply受信前に次のresumeを送らないため通常は到達
    /// しないが、防御的に「最新のみ有効」を維持する)。NonStop では
    /// 別々のvCont呼び出しが異なるスレッドを独立に resume しうるため
    /// (Arcavirt-cwm)、クリアせず単純に token を追加する。
    /// </summary>
    internal ExecutionResponder BeginResume() {
        int token = Interlocked.Increment(ref _tokenSeed);
        if (_mode != ResumeMode.NonStop) {
            _activeTokens.Clear();
        }
        _activeTokens.TryAdd(token, 0);
        return new ExecutionResponder(this, token);
    }

    /// <summary>
    /// token が現在アクティブな resume 集合に含まれる場合のみ、それを
    /// 集合から取り除いて消費する(§4.4)。ConcurrentDictionary.TryRemove
    /// は単一key に対して原子的なため、CASと同じ「一度だけ成功する」
    /// 保証を持つ。OnReportStop の AllStop 経路・OnReject の両方が使う
    /// 共通判定であり、本コンポーネントの不変条件。簡略化・省略しては
    /// ならない。
    /// </summary>
    private bool TryConsumeToken(int token) {
        return _activeTokens.TryRemove(token, out _);
    }

    /// <summary>
    /// token が現在アクティブな resume 集合に含まれるかだけを判定する
    /// (消費はしない)。ExecDispatchedCommand が、resume 開始直後に
    /// ハンドラが同期的に Reject を呼んだかどうかを判定するために使う
    /// (Reject は Mode を問わず TryConsumeToken で token を消費するため、
    /// ReportStop(NonStopでは token を消費しない)の有無に関わらず、
    /// Reject が起きたかどうかだけを正確に反映する)。
    /// </summary>
    internal bool IsPending(int token) {
        return _activeTokens.ContainsKey(token);
    }

    /// <summary>
    /// resume を異常終了させる(ハンドラが例外を投げた場合)。token が
    /// 現在アクティブなら集合から取り除いて以後の報告を無効化する。既に
    /// ReportStop/Reject 済みなら何もしない(TryRemoveが失敗するだけで
    /// 安全)。
    /// </summary>
    internal void AbortResume(int token) {
        _activeTokens.TryRemove(token, out _);
    }

    /// <summary>
    /// AllStop では token が現在アクティブな resume 集合に含まれる場合
    /// のみ消費し、最初の1件を単一 stop reply として配送する(§4.3手順7、
    /// 代表1件による合流)。含まれない場合(既に消費済み、他スレッドの
    /// 後続報告、または別の resume に切り替わった後の遅延・重複呼び出し)
    /// は§4.4の規律に従い無視する。AllStopでは BeginResume 側で集合を
    /// 都度クリアしているため、実質的に「最新の resume の token だけが
    /// 有効」という従来どおりの1本勝負が保たれる。
    /// NonStop では token を消費せず(複数スレッドが独立に停止しうる
    /// ため)、episode内のスレッド単位で重複排除しつつ複数回配送を許す
    /// (Arcavirt-o3e.10.4)。加えて token 自体が _activeTokens に残り
    /// 続けるため、「別々のvCont呼び出し」で始まった複数の resume
    /// episode が互いを無効化せず並行して有効であり続ける(Arcavirt-cwm)。
    /// 判定は _reportedThreadsInEpisode のキーに token 自体を含めることで、
    /// 「チェック通過後に別の BeginResume が割り込む」タイミング依存の
    /// すり抜けを構造的に防ぐ(旧・単純なThreadIdキー+Clear()方式では
    /// この窓が存在した)。
    /// </summary>
    internal void OnReportStop(int token, in StopEvent stop) {
        if (_mode == ResumeMode.NonStop) {
            if (!_activeTokens.ContainsKey(token)) {
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