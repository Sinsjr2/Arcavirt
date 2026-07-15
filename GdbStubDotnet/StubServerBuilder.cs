using Pidgin;

namespace GdbStubDotnet;

public sealed class StubServerBuilder {
    private readonly List<Parser<byte, IDispatchedCommand>> _entries = [];
    private ITransport? _transport;

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ResponseWriter<SyncResponse>> handler) {
        _entries.Add(Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<TCmd>(cmd, handler)));
        return this;
    }

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ExecutionResponder> handler) {
        _entries.Add(Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new ExecDispatchedCommand<TCmd>(cmd, handler)));
        return this;
    }

    public StubServerBuilder UseTransport(ITransport transport) {
        _transport = transport;
        return this;
    }

    public StubServer Build() {
        if (_transport is null) {
            throw new InvalidOperationException("UseTransport が呼ばれていません。");
        }
        var dispatcher = new Dispatcher(_entries);
        return new StubServer(_transport, dispatcher);
    }
}