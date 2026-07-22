using System.Net;
using System.Net.Sockets;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class ExecutionTest {
    /// <summary>
    /// c コマンド送信後、ハンドラが捕捉した ExecutionResponder に ReportStop すると、
    /// クライアントに stop reply(T05, SIGTRAP=5)が返り、その後ループが通常モードに
    /// 戻って後続の g コマンドにも正しく応答できることを確認する
    /// (exec-wait に詰まったままにならないことの検証)。
    /// </summary>
    [Test]
    public async Task ContinueCommand_ThenReportStop_ReturnsStopReplyAndLoopResumesNormalHandling() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();

        using var server = new StubServerBuilder()
            .Map(Commands.Continue, (cmd, responder) => handlerInvoked.SetResult(responder))
            .Map(Commands.ReadRegisters, (cmd, res) => res.HexBytes([0x01]))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("c"u8));

        ExecutionResponder responder = await handlerInvoked.Task;
        responder.ReportStop(new StopEvent(default, StopReason.Signal, 5, 0));

        byte[]? stopReply = await ReadOnePacket(clientStream);
        Assert.That(stopReply, Is.EqualTo("T05"u8.ToArray()));

        await clientStream.WriteAsync(Framer.Encode("g"u8));
        byte[]? gReply = await ReadOnePacket(clientStream);
        Assert.That(gReply, Is.EqualTo("01"u8.ToArray()));
    }

    /// <summary>
    /// ExecutionResponder.ReportStop がターゲット自身のスレッド(I/Oループとは
    /// 別スレッド)から呼ばれても、正しく stop reply が送出されることを確認する
    /// (実運用でターゲット実装が別スレッドで resume を駆動する想定を反映)。
    /// </summary>
    [Test]
    public async Task ContinueCommand_ReportStopFromDifferentThread_ReturnsStopReply() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();

        using var server = new StubServerBuilder()
            .Map(Commands.Continue, (cmd, responder) => handlerInvoked.SetResult(responder))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("c"u8));

        ExecutionResponder responder = await handlerInvoked.Task;

        var targetThread = new Thread(() => {
            responder.ReportStop(new StopEvent(default, StopReason.Signal, 5, 0));
        });
        targetThread.Start();
        targetThread.Join();

        byte[]? stopReply = await ReadOnePacket(clientStream);
        Assert.That(stopReply, Is.EqualTo("T05"u8.ToArray()));
    }

    /// <summary>
    /// c コマンド送信後、ハンドラが Reject を呼ぶと、クライアントに
    /// エラー応答(E NN, 16進数)が返ることを確認する。
    /// </summary>
    [Test]
    public async Task ContinueCommand_ThenReject_ReturnsHexErrorReply() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();

        using var server = new StubServerBuilder()
            .Map(Commands.Continue, (cmd, responder) => handlerInvoked.SetResult(responder))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("c"u8));

        ExecutionResponder responder = await handlerInvoked.Task;
        responder.Reject(new RspError(14, null));

        byte[]? errorReply = await ReadOnePacket(clientStream);
        Assert.That(errorReply, Is.EqualTo("E0e"u8.ToArray()));
    }

    /// <summary>
    /// s(Step)コマンド送信後、ハンドラが捕捉した ExecutionResponder に
    /// ReportStop すると、クライアントに stop reply(T05)が返ることを確認する
    /// (c コマンドと同型の Exec パスであることの確認)。
    /// </summary>
    [Test]
    public async Task StepCommand_ThenReportStop_ReturnsStopReply() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();

        using var server = new StubServerBuilder()
            .Map(Commands.Step, (cmd, responder) => handlerInvoked.SetResult(responder))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("s"u8));

        ExecutionResponder responder = await handlerInvoked.Task;
        responder.ReportStop(new StopEvent(default, StopReason.Signal, 5, 0));

        byte[]? stopReply = await ReadOnePacket(clientStream);
        Assert.That(stopReply, Is.EqualTo("T05"u8.ToArray()));
    }

    static async Task<byte[]?> ReadOnePacket(NetworkStream stream) {
        var framer = new Framer();
        byte[]? received = null;
        byte[] buffer = new byte[256];
        while (received is null) {
            int n = await stream.ReadAsync(buffer);
            framer.ProcessBytes(buffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    received = evt.Payload;
                }
            });
        }
        return received;
    }
}