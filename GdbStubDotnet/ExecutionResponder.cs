using System.Threading.Channels;

namespace GdbStubDotnet;

internal readonly record struct ExecOutcome(bool IsReject, StopEvent Stop, RspError RejectError);

/// <summary>
/// 実行コマンドの長命・スレッドセーフな応答器。ターゲットスレッドから
/// 呼ばれることを前提に、ReportStop/Reject は Channel への書き込みのみを行い、
/// コーディネーター状態の変更は一切しない(I/Oループ側でのみ状態を変更する)。
/// </summary>
public sealed class ExecutionResponder {
    private readonly ChannelWriter<ExecOutcome> _writer;

    internal ExecutionResponder(ChannelWriter<ExecOutcome> writer) {
        _writer = writer;
    }

    public void ReportStop(in StopEvent stop) {
        _writer.TryWrite(new ExecOutcome(false, stop, default));
    }

    public void Reject(RspError error) {
        _writer.TryWrite(new ExecOutcome(true, default, error));
    }
}