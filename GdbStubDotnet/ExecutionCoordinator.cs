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
/// </summary>
internal sealed class ExecutionCoordinator {
    private readonly ChannelWriter<ExecOutcome> _writer;
    private readonly NotificationQueue _notificationQueue;
    private readonly ConcurrentDictionary<ThreadId, byte> _reportedThreadsInEpisode = new();
    private int _tokenSeed;
    private int _activeToken;
    private volatile ResumeMode _mode;
    private ThreadId _currentThread;

    internal ExecutionCoordinator(ChannelWriter<ExecOutcome> writer, NotificationQueue notificationQueue) {
        _writer = writer;
        _notificationQueue = notificationQueue;
    }

    internal ResumeMode Mode => _mode;

    internal ThreadId CurrentThread => _currentThread;

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
    /// 以後この resume からの報告だけが token 判定に成功する。新しい
    /// episode の開始として、直前の episode の重複排除状態もクリアする。
    /// </summary>
    internal ExecutionResponder BeginResume() {
        int token = Interlocked.Increment(ref _tokenSeed);
        Interlocked.Exchange(ref _activeToken, token);
        _reportedThreadsInEpisode.Clear();
        return new ExecutionResponder(this, token);
    }

    /// <summary>
    /// AllStop では token が現在アクティブな resume の token と一致する
    /// 場合のみ CAS で消費し、最初の1件を単一 stop reply として配送する
    /// (§4.3手順7、代表1件による合流)。一致しない場合(既に消費済み、
    /// 他スレッドの後続報告、または別の resume に切り替わった後の遅延・
    /// 重複呼び出し)は§4.4の規律に従い無視する。この CAS 判定は
    /// 本コンポーネントの不変条件であり、簡略化・省略してはならない。
    /// NonStop では token を消費せず、episode内のスレッド単位で重複排除
    /// しつつ複数回配送を許す(Arcavirt-o3e.10.4)。
    /// </summary>
    internal void OnReportStop(int token, in StopEvent stop) {
        if (_mode == ResumeMode.NonStop) {
            if (Volatile.Read(ref _activeToken) != token) {
                return;
            }
            if (_reportedThreadsInEpisode.TryAdd(stop.Thread, 0)) {
                _notificationQueue.Enqueue(new StopNotification(stop));
            }
            return;
        }
        if (Interlocked.CompareExchange(ref _activeToken, 0, token) != token) {
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
        if (Interlocked.CompareExchange(ref _activeToken, 0, token) != token) {
            return;
        }
        _writer.TryWrite(new ExecOutcome(true, default, error));
    }
}