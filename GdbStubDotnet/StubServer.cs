using System.Text;

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

            List<byte[]> syncResponses = [];
            List<System.Threading.Channels.ChannelReader<ExecOutcome>> execWaits = [];
            _framer.ProcessBytes(buffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    RouteResult? routed = _dispatcher.Route(evt.Payload!);
                    if (routed is null) {
                        syncResponses.Add(Framer.Encode(ReadOnlySpan<byte>.Empty));
                    } else if (routed.Value.ExecWait is { } execWait) {
                        execWaits.Add(execWait);
                    } else {
                        syncResponses.Add(Framer.Encode(routed.Value.SyncResponse!));
                    }
                }
            });

            foreach (byte[] response in syncResponses) {
                await _transport.WriteAsync(response);
            }

            foreach (var execWait in execWaits) {
                ExecOutcome outcome = await execWait.ReadAsync();
                byte[] replyPayload = outcome.IsReject
                    ? EncodeError(outcome.RejectError)
                    : EncodeStopReply(outcome.Stop);
                await _transport.WriteAsync(Framer.Encode(replyPayload));
            }
        }
    }

    private static byte[] EncodeStopReply(StopEvent stop) {
        string text = "T" + stop.SignalOrExit.ToString("x2");
        return Encoding.ASCII.GetBytes(text);
    }

    private static byte[] EncodeError(RspError error) {
        string text = "E" + error.Code.ToString("x2");
        return Encoding.ASCII.GetBytes(text);
    }

    public void Stop() {
        _transport.Close();
    }

    public void Dispose() {
        Stop();
    }
}