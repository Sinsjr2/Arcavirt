namespace GdbStubDotnet;

internal readonly record struct ExecOutcome(bool IsReject, StopEvent Stop, RspError RejectError);

/// <summary>
/// 実行コマンドの長命・スレッドセーフな応答器。ターゲットスレッドから
/// 呼ばれることを前提に、ReportStop/Reject は ExecutionCoordinator へ
/// token 付きで通知するのみで、報告が有効かどうかの判定(§4.4のスコープ規律)
/// と共有チャネルへの書込は ExecutionCoordinator 側が行う。
/// </summary>
public sealed class ExecutionResponder {
    private readonly ExecutionCoordinator _coordinator;
    private readonly int _token;

    internal ExecutionResponder(ExecutionCoordinator coordinator, int token) {
        _coordinator = coordinator;
        _token = token;
    }

    /// <summary>
    /// この responder がスコープされている resume の token。Dispatcher が
    /// 例外発生時の resume 中断判定・non-stop即時OK可否判定に用いる
    /// (internal専用、公開APIではない)。
    /// </summary>
    internal int Token => _token;

    public void ReportStop(in StopEvent stop) {
        _coordinator.OnReportStop(_token, in stop);
    }

    public void Reject(RspError error) {
        _coordinator.OnReject(_token, error);
    }
}