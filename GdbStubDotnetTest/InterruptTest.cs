using System.Net;
using System.Net.Sockets;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class InterruptTest {
    /// <summary>
    /// c コマンド実行中(exec-wait で停止待ちの間)に生の 0x03 バイトを送信すると、
    /// I/O ループが transport の読み取りを exec-wait と同時に待ち受けているため、
    /// OnInterrupt ハンドラが(exec の完了を待たずに)呼ばれることを確認する。
    /// さらにその後 ReportStop すると通常どおり stop reply が返り、ループが
    /// 詰まらないことも確認する(select 構造の往復整合性)。
    /// </summary>
    [Test]
    public async Task RawInterruptByte_DuringOutstandingContinue_InvokesOnInterruptHandlerAndLoopContinues() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();
        var interruptInvoked = new TaskCompletionSource<bool>();

        using var server = new StubServerBuilder()
            .Map(Commands.Continue, (cmd, responder) => handlerInvoked.SetResult(responder))
            .OnInterrupt(() => interruptInvoked.SetResult(true))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("c"u8));
        ExecutionResponder responder = await handlerInvoked.Task;

        await clientStream.WriteAsync(new byte[] { 0x03 });
        bool interrupted = await interruptInvoked.Task;

        responder.ReportStop(new StopEvent(default, StopReason.Signal, 2, 0));
        byte[]? stopReply = await ReadOnePacket(clientStream);

        Assert.Multiple(() => {
            Assert.That(interrupted, Is.True);
            Assert.That(stopReply, Is.EqualTo("T02"u8.ToArray()));
        });
    }

    /// <summary>
    /// c コマンド実行中に vCtrlC パケット(0x03 のパケット版)を送信すると、
    /// 内部で raw 0x03 と同じ interrupt 経路に集約され OnInterrupt ハンドラが
    /// 呼ばれることを確認する。vCtrlC 自体には(フレーミング ack '+' 以外の)
    /// 直接の内容応答が発生しないことも合わせて確認する(§4.6)。
    /// </summary>
    [Test]
    public async Task VCtrlCPacket_DuringOutstandingContinue_InvokesOnInterruptHandlerWithoutDirectReply() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();
        var interruptInvoked = new TaskCompletionSource<bool>();

        using var server = new StubServerBuilder()
            .Map(Commands.Continue, (cmd, responder) => handlerInvoked.SetResult(responder))
            .OnInterrupt(() => interruptInvoked.SetResult(true))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("c"u8));
        ExecutionResponder responder = await handlerInvoked.Task;

        await clientStream.WriteAsync(Framer.Encode("vCtrlC"u8));
        bool interrupted = await interruptInvoked.Task;

        responder.ReportStop(new StopEvent(default, StopReason.Signal, 2, 0));
        byte[]? stopReply = await ReadOnePacket(clientStream);

        Assert.Multiple(() => {
            Assert.That(interrupted, Is.True);
            Assert.That(stopReply, Is.EqualTo("T02"u8.ToArray()));
        });
    }

    /// <summary>
    /// interrupt を経て exec が完了した後、後続の同期コマンドにも通常どおり
    /// 応答できることを確認する(select 構造導入後も通常ループへ正しく
    /// 戻ることの回帰防止)。
    /// </summary>
    [Test]
    public async Task AfterInterruptAndReportStop_SubsequentSyncCommand_RespondsNormally() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();
        var interruptInvoked = new TaskCompletionSource<bool>();

        using var server = new StubServerBuilder()
            .Map(Commands.Continue, (cmd, responder) => handlerInvoked.SetResult(responder))
            .Map(Commands.ReadRegisters, (cmd, res) => res.HexBytes([0x01]))
            .OnInterrupt(() => interruptInvoked.SetResult(true))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("c"u8));
        ExecutionResponder responder = await handlerInvoked.Task;

        await clientStream.WriteAsync(new byte[] { 0x03 });
        await interruptInvoked.Task;

        responder.ReportStop(new StopEvent(default, StopReason.Signal, 2, 0));
        await ReadOnePacket(clientStream);

        await clientStream.WriteAsync(Framer.Encode("g"u8));
        byte[]? gReply = await ReadOnePacket(clientStream);

        Assert.That(gReply, Is.EqualTo("01"u8.ToArray()));
    }

    private static async Task<byte[]?> ReadOnePacket(NetworkStream stream) {
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