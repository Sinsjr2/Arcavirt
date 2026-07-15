using Pidgin;

namespace GdbStubDotnet;

public sealed class StubServerBuilder {
    private readonly List<Parser<byte, IDispatchedCommand>> _entries = [];
    private ITransport? _transport;
    private Action? _onInterrupt;

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ResponseWriter<SyncResponse>> handler) {
        _entries.Add(Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<TCmd>(cmd, handler)));
        return this;
    }

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ExecutionResponder> handler) {
        _entries.Add(Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new ExecDispatchedCommand<TCmd>(cmd, handler)));
        return this;
    }

    /// <summary>
    /// 生 0x03 または vCtrlC パケットを受信したときに呼ばれるハンドラを登録する(§4.6)。
    /// 直接の応答は発生しない。実行中の ExecutionResponder が停止検知後に
    /// stop reply(SIGINT 相当)を返す想定(利用者側の resume 実装が対応する)。
    /// </summary>
    public StubServerBuilder OnInterrupt(Action handler) {
        _onInterrupt = handler;
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
        return new StubServer(_transport, dispatcher, _onInterrupt);
    }
}