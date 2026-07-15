using System.Buffers;
using System.Threading.Channels;
using Pidgin;

namespace GdbStubDotnet;

internal interface IDispatchedCommand {
    /// <summary>
    /// 同期コマンドなら output にバイト列を書き込み null を返す。
    /// 実行コマンドなら何も書き込まず、stop 結果を受け取る ChannelReader を返す。
    /// </summary>
    ChannelReader<ExecOutcome>? Execute(IBufferWriter<byte> output);
}

internal sealed class SyncDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ResponseWriter<SyncResponse>> handler) : IDispatchedCommand {
    public ChannelReader<ExecOutcome>? Execute(IBufferWriter<byte> output) {
        var writer = new ResponseWriter<SyncResponse>(output);
        handler(command, writer);
        return null;
    }
}

internal sealed class ExecDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ExecutionResponder> handler) : IDispatchedCommand {
    public ChannelReader<ExecOutcome>? Execute(IBufferWriter<byte> output) {
        var channel = Channel.CreateBounded<ExecOutcome>(1);
        var responder = new ExecutionResponder(channel.Writer);
        handler(command, responder);
        return channel.Reader;
    }
}

internal readonly record struct RouteResult(byte[]? SyncResponse, ChannelReader<ExecOutcome>? ExecWait);

internal sealed class Dispatcher {
    private readonly Parser<byte, IDispatchedCommand> _router;

    public Dispatcher(IReadOnlyList<Parser<byte, IDispatchedCommand>> entries) {
        _router = Parser.OneOf(entries);
    }

    public RouteResult? Route(ReadOnlySpan<byte> payload) {
        var result = _router.Parse(payload);
        if (!result.Success) {
            return null;
        }
        var buffer = new ArrayBufferWriter<byte>();
        var execWait = result.Value.Execute(buffer);
        return execWait is null
            ? new RouteResult(buffer.WrittenSpan.ToArray(), null)
            : new RouteResult(null, execWait);
    }
}