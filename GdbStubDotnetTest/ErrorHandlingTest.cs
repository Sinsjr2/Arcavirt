using System.Net;
using System.Net.Sockets;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class ErrorHandlingTest {
    /// <summary>
    /// ハンドラ内で例外が発生すると、OnError へ StubFault として通知され、
    /// GDB へは安全な E01 応答が返り、かつ I/O ループが継続して後続コマンド
    /// にも正常応答できることを確認する(§7.2、Arcavirt-o3e.18)。
    /// </summary>
    [Test]
    public async Task HandlerThrows_InvokesOnErrorAndReturnsGenericErrorWithoutCrashingLoop() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var faultCaptured = new TaskCompletionSource<StubFault>();

        using var server = new StubServerBuilder()
            .Map(Commands.ReadRegisters, (ReadRegistersCommand cmd, ResponseWriter<SyncResponse> res) => throw new InvalidOperationException("boom"))
            .Map(Commands.HaltReason, (cmd, res) => res.Text("S05"u8))
            .OnError(fault => faultCaptured.SetResult(fault))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("g"u8));
        byte[]? errorReply = await ReadOnePacket(clientStream);
        StubFault fault = await faultCaptured.Task;

        Assert.Multiple(() => {
            Assert.That(errorReply, Is.EqualTo("E01"u8.ToArray()));
            Assert.That(fault.Error, Is.InstanceOf<InvalidOperationException>());
        });

        await clientStream.WriteAsync(Framer.Encode("?"u8));
        byte[]? haltReply = await ReadOnePacket(clientStream);
        Assert.That(haltReply, Is.EqualTo("S05"u8.ToArray()));
    }

    /// <summary>
    /// EnableDetailedErrors() を有効にすると、Detail 付き RspError が
    /// E.&lt;text&gt; 形式(人間可読)で応答されることを確認する(§7.3)。
    /// </summary>
    [Test]
    public async Task EnableDetailedErrors_TrueWithDetailText_ReturnsHumanReadableErrorFormat() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));

        using var server = new StubServerBuilder()
            .Map(Commands.ReadRegisters, (cmd, res) => res.Error(new RspError(5, "bad register")))
            .EnableDetailedErrors()
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("g"u8));
        byte[]? reply = await ReadOnePacket(clientStream);

        Assert.That(reply, Is.EqualTo("E.bad register"u8.ToArray()));
    }

    /// <summary>
    /// クライアントが切断すると OnDisconnect ハンドラが呼ばれることを
    /// 確認する(§4.6)。
    /// </summary>
    [Test]
    public async Task ClientDisconnects_InvokesOnDisconnect() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var disconnected = new TaskCompletionSource<bool>();

        using var server = new StubServerBuilder()
            .OnDisconnect(() => disconnected.SetResult(true))
            .UseTransport(transport)
            .Build();
        server.Start();

        var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        client.Close();

        bool result = await disconnected.Task;
        Assert.That(result, Is.True);
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