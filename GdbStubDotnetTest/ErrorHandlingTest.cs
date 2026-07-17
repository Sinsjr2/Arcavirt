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

    /// <summary>
    /// 実行コマンドのハンドラが例外を投げても、Route の例外境界により
    /// 安全な E01 応答が返ることを確認する(§7.2の例外境界が同期コマンドに
    /// 限らず実行コマンドにも適用されることの確認)。ハンドラ例外前に
    /// 捕まえたresponderが後から呼ばれても無視されるべき、というtoken
    /// 無効化の不変条件そのものは、ソケット越しの非同期タイミングに
    /// 依存せず決定論的に検証できる ExecutionCoordinatorTest 側で確認する。
    /// </summary>
    [Test]
    public async Task ExecHandlerThrows_ReturnsGenericErrorOverTheWire() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));

        using var server = new StubServerBuilder()
            .Map(Commands.Continue, (ContinueCommand cmd, ExecutionResponder responder) => throw new InvalidOperationException("boom"))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("c"u8));
        byte[]? errorReply = await ReadOnePacket(clientStream);
        Assert.That(errorReply, Is.EqualTo("E01"u8.ToArray()));
    }

    /// <summary>
    /// 1回のstream.ReadAsyncチャンクに複数パケットが含まれる場合、
    /// Framer.ProcessBytesは1回の呼び出し内でコールバックを複数回
    /// 発火させる。最後に見つかったパケットで上書きすると、本来先に
    /// 届くはずの余分な応答を見失う(コードレビューで判明)。received
    /// が既に確定していれば以降のPacketイベントは無視し、最初の1件だけ
    /// を返す。
    /// </summary>
    private static async Task<byte[]?> ReadOnePacket(NetworkStream stream) {
        var framer = new Framer();
        byte[]? received = null;
        byte[] buffer = new byte[256];
        while (received is null) {
            int n = await stream.ReadAsync(buffer);
            framer.ProcessBytes(buffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet && received is null) {
                    received = evt.Payload;
                }
            });
        }
        return received;
    }
}