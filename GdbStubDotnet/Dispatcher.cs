using System.Buffers;
using Pidgin;

namespace GdbStubDotnet;

internal interface IDispatchedCommand {
    void Execute(IBufferWriter<byte> output);
}

internal sealed class SyncDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ResponseWriter<SyncResponse>> handler) : IDispatchedCommand {
    public void Execute(IBufferWriter<byte> output) {
        var writer = new ResponseWriter<SyncResponse>(output);
        handler(command, writer);
    }
}

internal sealed class Dispatcher {
    private readonly Parser<byte, IDispatchedCommand> _router;

    public Dispatcher(IReadOnlyList<Parser<byte, IDispatchedCommand>> entries) {
        _router = Parser.OneOf(entries);
    }

    public byte[]? Route(ReadOnlySpan<byte> payload) {
        var result = _router.Parse(payload);
        if (!result.Success) {
            return null;
        }
        var buffer = new ArrayBufferWriter<byte>();
        result.Value.Execute(buffer);
        return buffer.WrittenSpan.ToArray();
    }
}