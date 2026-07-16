using System.Buffers;
using Pidgin;

namespace GdbStubDotnet;

internal interface IDispatchedCommand {
    /// <summary>
    /// 同期コマンドなら output にバイト列を書き込み false を返す。
    /// 実行コマンドなら、all-stop では何も書き込まず true を返す(結果は
    /// ExecutionCoordinator 経由で接続共有チャネルへ後刻届く)。non-stop
    /// では resume 開始の直後に即時 OK を書き込み false を返す(§4.3手順5)。
    /// </summary>
    bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator);
}

internal sealed class SyncDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ResponseWriter<SyncResponse>> handler) : IDispatchedCommand {
    /// <summary>
    /// TCmd が IThreadScoped を実装していれば、ハンドラ呼び出し
    /// 直前に H(SetThread)で選択済みの現在スレッドをThreadフィールドへ
    /// 差し替える(§6.6)。実装していないコマンドは素通しする。
    /// </summary>
    public bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator) {
        TCmd effective = command is IThreadScoped scoped ? (TCmd)scoped.WithThread(coordinator.CurrentThread) : command;
        var writer = new ResponseWriter<SyncResponse>(output);
        handler(effective, writer);
        return false;
    }
}

internal sealed class ExecDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ExecutionResponder> handler) : IDispatchedCommand {
    public bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator) {
        ExecutionResponder responder = coordinator.BeginResume();
        handler(command, responder);
        if (coordinator.Mode == ResumeMode.NonStop) {
            new ResponseWriter<SyncResponse>(output).Ok();
            return false;
        }
        return true;
    }
}

internal readonly record struct RouteResult(byte[]? SyncResponse, bool ExecStarted);

internal sealed class Dispatcher {
    private readonly Parser<byte, IDispatchedCommand> _router;
    private readonly ExecutionCoordinator _coordinator;

    public Dispatcher(IReadOnlyList<Parser<byte, IDispatchedCommand>> entries, ExecutionCoordinator coordinator) {
        _router = Parser.OneOf(entries);
        _coordinator = coordinator;
    }

    public RouteResult? Route(ReadOnlySpan<byte> payload) {
        var result = _router.Parse(payload);
        if (!result.Success) {
            return null;
        }
        var buffer = new ArrayBufferWriter<byte>();
        bool execStarted = result.Value.Execute(buffer, _coordinator);
        return execStarted
            ? new RouteResult(null, true)
            : new RouteResult(buffer.WrittenSpan.ToArray(), false);
    }
}