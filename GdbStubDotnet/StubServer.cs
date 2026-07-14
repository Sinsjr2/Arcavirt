namespace GdbStubDotnet;

public sealed class StubServer : IDisposable {
    private readonly ITransport _transport;
    private readonly Dispatcher _dispatcher;
    private readonly Framer _framer = new();
    private Task? _loopTask;

    internal StubServer(ITransport transport, Dispatcher dispatcher) {
        _transport = transport;
        _dispatcher = dispatcher;
    }

    public void Start() {
        _loopTask = RunLoopAsync();
    }

    private async Task RunLoopAsync() {
        byte[] buffer = new byte[4096];
        while (true) {
            int n;
            try {
                n = await _transport.ReadAsync(buffer);
            } catch {
                return;
            }
            if (n == 0) {
                return;
            }

            List<byte[]> responses = [];
            _framer.ProcessBytes(buffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    byte[]? responsePayload = _dispatcher.Route(evt.Payload!);
                    byte[] encoded = responsePayload is null
                        ? Framer.Encode(ReadOnlySpan<byte>.Empty)
                        : Framer.Encode(responsePayload);
                    responses.Add(encoded);
                }
            });

            foreach (byte[] response in responses) {
                await _transport.WriteAsync(response);
            }
        }
    }

    public void Stop() {
        _transport.Close();
    }

    public void Dispose() {
        Stop();
    }
}