using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Gdb;

public class GDBTCPServer {
    readonly TcpListener socketServer;

    readonly IGdbStub targetStub;

    readonly ILogger logger;

    public int Port { get; }

    Action<object?, Func<object?, ValueTask>> runOnLoop;

    public GDBTCPServer(
        Action<object?, Func<object?, ValueTask>> runOnLoop, ILogger logger, int port, IGdbStub targetStub) {
        this.runOnLoop = runOnLoop;
        this.Port = port;
        socketServer = new TcpListener(IPAddress.Any, port);
        this.logger = logger;
        this.targetStub = targetStub;
    }

    public async ValueTask ConnectionLoop(CancellationToken token) {
        token.ThrowIfCancellationRequested();
        socketServer.Start();

        while (true) {
            token.ThrowIfCancellationRequested();
            try {
                var listner = await socketServer.AcceptTcpClientAsync(token);
                logger.LogInformation("GDB connected");
                runOnLoop(this, obj => ((GDBTCPServer?)obj)!.CommandAnalizeLoop(listner, token));
            } catch (Exception ex) {
                logger.LogError("GDB connection error: {message}", ex);
            }
        }
    }

    async ValueTask CommandAnalizeLoop(TcpClient listner, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        using var client = listner;
        using var scopedClient = client;
        using var stream = scopedClient.GetStream();
        var packetAnalizer = new GdbPacketAnalizerCStubLike(targetStub, logger, runOnLoop, stream, stream);
        // var communication = new StreamCommunication(stream, stream);
        // await packetAnalizer.CommandAnalizeLoop(communicasttion, token);
        await packetAnalizer.MessageLoop(token);
    }
}
