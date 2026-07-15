using System.Buffers;
using Pidgin;

namespace GdbStubDotnet;

internal interface IDispatchedCommand {
    /// <summary>
    /// 同期コマンドなら output にバイト列を書き込み false を返す。
    /// 実行コマンドなら何も書き込まず、coordinator 経由で resume を開始して
    /// true を返す(結果は ExecutionCoordinator 経由で接続共有チャネルへ
    /// 後刻届く)。
    /// </summary>
    bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator);
}

internal sealed class SyncDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ResponseWriter<SyncResponse>> handler) : IDispatchedCommand {
    public bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator) {
        var writer = new ResponseWriter<SyncResponse>(output);
        handler(command, writer);
        return false;
    }
}

internal sealed class ExecDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ExecutionResponder> handler) : IDispatchedCommand {
    public bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator) {
        ExecutionResponder responder = coordinator.BeginResume();
        handler(command, responder);
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