using System.Threading.Channels;

namespace GdbStubDotnet;

/// <summary>
/// resume(vCont/c/s)の停止報告を仲介する内部コンポーネント。
/// ExecutionResponder.ReportStop/Reject の唯一の入口であり、報告が
/// 「どの resume にスコープされたものか」を token で判定する(§4.4:
/// resume 外の ReportStop は無視する規律)。
/// 現行スコープ(Arcavirt-o3e.10.1)は all-stop のみを対象とし、接続につき
/// outstanding な resume は高々1つという前提のもとで、判定を通った報告を
/// 接続共有の ExecOutcome チャネルへそのまま書き込む。non-stop・複数スレッド
/// 相関(Arcavirt-o3e.10.2以降)は Mode に応じた分岐をここへ追加する。
/// </summary>
internal sealed class ExecutionCoordinator {
    private readonly ChannelWriter<ExecOutcome> _writer;
    private int _tokenSeed;
    private int _activeToken;

    internal ExecutionCoordinator(ChannelWriter<ExecOutcome> writer) {
        _writer = writer;
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
    /// 共有チャネルへ書き込んで resume を消費済みにする。一致しない場合
    /// (既に消費済み、または別の resume に切り替わった後の遅延・重複呼び出し)
    /// は§4.4の規律に従い無視する。この CAS 判定が本コンポーネントの
    /// 唯一の不変条件であり、簡略化・省略してはならない。
    /// </summary>
    internal void OnReportStop(int token, in StopEvent stop) {
        if (Interlocked.CompareExchange(ref _activeToken, 0, token) != token) {
            return;
        }
        _writer.TryWrite(new ExecOutcome(false, stop, default));
    }

    /// <summary>
    /// OnReportStop と同じ token 判定を Reject 経路に適用する。
    /// </summary>
    internal void OnReject(int token, RspError error) {
        if (Interlocked.CompareExchange(ref _activeToken, 0, token) != token) {
            return;
        }
        _writer.TryWrite(new ExecOutcome(true, default, error));
    }
}