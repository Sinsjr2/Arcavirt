using System.Threading.Channels;

namespace GdbStubDotnet;

/// <summary>
/// resume(vCont/c/s)の停止報告を仲介する内部コンポーネント。
/// ExecutionResponder.ReportStop/Reject の唯一の入口であり、報告が
/// 「どの resume にスコープされたものか」を token で判定する(§4.4:
/// resume 外の ReportStop は無視する規律)。
/// Mode(ResumeMode)に応じて OnReportStop の配送先が切り替わる
/// (Arcavirt-o3e.10.3):
///   - AllStop: 接続共有の ExecOutcome チャネルへ書き込み、単一 stop reply
///     として StubServer の select ループへ届く(Arcavirt-o3e.10.1の経路)。
///   - NonStop: NotificationQueue へ %Stop 通知として Enqueue する(§4.5)。
/// OnReject は Mode を問わず常に ExecOutcome チャネル経由(resume要求自体が
/// 不正なときの同期応答であり、%Stop通知の対象ではないため)。
/// 現行スコープ(Arcavirt-o3e.10.3)は単一スレッド・単一 outstanding resume
/// を前提とする(all-stop時代の10.1からの前提を維持)。複数スレッドの
/// 同時 resume・相関は Arcavirt-o3e.10.4 で対応する。
/// </summary>
internal sealed class ExecutionCoordinator {
    private readonly ChannelWriter<ExecOutcome> _writer;
    private readonly NotificationQueue _notificationQueue;
    private int _tokenSeed;
    private int _activeToken;
    private volatile ResumeMode _mode;

    internal ExecutionCoordinator(ChannelWriter<ExecOutcome> writer, NotificationQueue notificationQueue) {
        _writer = writer;
        _notificationQueue = notificationQueue;
    }

    internal ResumeMode Mode => _mode;

    /// <summary>
    /// QNonStop:1/0 受信時に呼ばれ、以後の OnReportStop の配送先を切り替える。
    /// </summary>
    internal void SetMode(ResumeMode mode) {
        _mode = mode;
    }

    /// <summary>
    /// resume を1つ開始し、この resume にスコープされた ExecutionResponder を
    /// 返す。token は Interlocked.Increment により一意に払い出され、
    /// 以後この resume からの報告だけが _activeToken との CAS に成功する。
    /// </summary>
    internal ExecutionResponder BeginResume() {
        int token = Interlocked.Increment(ref _tokenSeed);
        Interlocked.Exchange(ref _activeToken, token);
        return new ExecutionResponder(this, token);
    }

    /// <summary>
    /// token が現在アクティブな resume の token と一致する場合のみ、結果を
    /// 配送して resume を消費済みにする。一致しない場合(既に消費済み、
    /// または別の resume に切り替わった後の遅延・重複呼び出し)は§4.4の
    /// 規律に従い無視する。この CAS 判定が本コンポーネントの不変条件であり、
    /// 簡略化・省略してはならない。
    /// </summary>
    internal void OnReportStop(int token, in StopEvent stop) {
        if (Interlocked.CompareExchange(ref _activeToken, 0, token) != token) {
            return;
        }
        if (_mode == ResumeMode.NonStop) {
            _notificationQueue.Enqueue(new StopNotification(stop));
            return;
        }
        _writer.TryWrite(new ExecOutcome(false, stop, default));
    }

    /// <summary>
    /// OnReportStop と同じ token 判定を Reject 経路に適用する。Reject は
    /// resume要求自体が不正なときの同期応答であり、Mode を問わず常に
    /// ExecOutcome チャネル経由で伝える(%Stop通知の対象ではない)。
    /// </summary>
    internal void OnReject(int token, RspError error) {
        if (Interlocked.CompareExchange(ref _activeToken, 0, token) != token) {
            return;
        }
        _writer.TryWrite(new ExecOutcome(true, default, error));
    }
}