using System.Net;
using System.Net.Sockets;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class StubServerTest {
    /// <summary>
    /// gコマンドを実際にTCP経由で送信し、登録したハンドラが返すレジスタ値が
    /// 正しいRSPフレーム($...#cs)としてクライアントに返ってくることを確認する。
    /// </summary>
    [Test]
    public async Task GCommand_RoundTripOverTcp_ReturnsHexEncodedRegisters() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        byte[] registerValues = [0x01, 0x02, 0x03, 0x04];

        using var server = new StubServerBuilder()
            .Map(Commands.ReadRegisters, (cmd, res) => res.HexBytes(registerValues))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        byte[] request = Framer.Encode("g"u8);
        await clientStream.WriteAsync(request);

        var framer = new Framer();
        byte[]? receivedPayload = null;
        byte[] readBuffer = new byte[256];
        while (receivedPayload is null) {
            int n = await clientStream.ReadAsync(readBuffer);
            framer.ProcessBytes(readBuffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    receivedPayload = evt.Payload;
                }
            });
        }

        Assert.That(receivedPayload, Is.EqualTo("01020304"u8.ToArray()));
    }

    /// <summary>
    /// g パーサーは "g" に厳密一致し、"gX" のような接頭辞一致だけではマッチ
    /// しないことを確認する。入力全体を消費できないコマンドは未対応として
    /// 空パケット($#00)にフォールバックする(end-of-input アンカリングの回帰防止)。
    /// </summary>
    [Test]
    public async Task UnregisteredCommandWithMatchingPrefix_RoundTripOverTcp_FallsBackToEmptyPacket() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));

        using var server = new StubServerBuilder()
            .Map(Commands.ReadRegisters, (cmd, res) => res.HexBytes([0x01]))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        byte[] request = Framer.Encode("gX"u8);
        await clientStream.WriteAsync(request);

        var framer = new Framer();
        byte[]? receivedPayload = null;
        byte[] readBuffer = new byte[256];
        while (receivedPayload is null) {
            int n = await clientStream.ReadAsync(readBuffer);
            framer.ProcessBytes(readBuffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    receivedPayload = evt.Payload;
                }
            });
        }

        Assert.That(receivedPayload, Is.Empty);
    }

    /// <summary>
    /// qC(特定コマンド)と汎用 q クエリの両方を登録した状態で "qC" を送ると、
    /// 汎用クエリではなく qC 専用ハンドラにルーティングされることを確認する。
    /// 汎用クエリのパーサーは "qC" にも部分一致してしまうため、OneOf の登録順
    /// (特定を先、汎用を後)と Try によるバックトラックの両方が効いて
    /// 初めて成立する(接頭辞衝突の回帰防止)。
    /// </summary>
    [Test]
    public async Task QCCommand_WithGenericQueryAlsoRegistered_RoutesToCurrentThreadHandler() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));

        using var server = new StubServerBuilder()
            .Map(Commands.CurrentThread, (cmd, res) => res.Ok())
            .Map(Commands.Query, (cmd, res) => res.Empty())
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        byte[] request = Framer.Encode("qC"u8);
        await clientStream.WriteAsync(request);

        var framer = new Framer();
        byte[]? receivedPayload = null;
        byte[] readBuffer = new byte[256];
        while (receivedPayload is null) {
            int n = await clientStream.ReadAsync(readBuffer);
            framer.ProcessBytes(readBuffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    receivedPayload = evt.Payload;
                }
            });
        }

        Assert.That(receivedPayload, Is.EqualTo("OK"u8.ToArray()));
    }

    /// <summary>
    /// vCont;c(実行系)と vCont?(問い合わせ、Sync)の両方を登録した状態で
    /// "vCont?" を送ると、vCont; 系のExecハンドラではなく vCont? 専用の
    /// Syncハンドラにルーティングされることを確認する。
    /// "vCont;c" と "vCont?" は "vCont" の接頭辞を共有するため、Try による
    /// バックトラックが機能して初めて成立する(接頭辞衝突の回帰防止)。
    /// </summary>
    [Test]
    public async Task VContQuery_WithVContAlsoRegistered_RoutesToVContQueryHandler() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var vContInvoked = new TaskCompletionSource<bool>();

        using var server = new StubServerBuilder()
            .Map(Commands.VCont, (VContCommand cmd, ExecutionResponder responder) => vContInvoked.SetResult(true))
            .Map(Commands.VContQuery, (cmd, res) => res.Ok())
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        byte[] request = Framer.Encode("vCont?"u8);
        await clientStream.WriteAsync(request);

        var framer = new Framer();
        byte[]? receivedPayload = null;
        byte[] readBuffer = new byte[256];
        while (receivedPayload is null) {
            int n = await clientStream.ReadAsync(readBuffer);
            framer.ProcessBytes(readBuffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    receivedPayload = evt.Payload;
                }
            });
        }

        Assert.Multiple(() => {
            Assert.That(receivedPayload, Is.EqualTo("OK"u8.ToArray()));
            Assert.That(vContInvoked.Task.IsCompleted, Is.False);
        });
    }

    /// <summary>
    /// ? コマンドを送信すると、登録した Sync ハンドラの応答がそのまま
    /// 返ってくることを確認する。
    /// </summary>
    [Test]
    public async Task HaltReasonCommand_RoundTripOverTcp_ReturnsHandlerResponse() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));

        using var server = new StubServerBuilder()
            .Map(Commands.HaltReason, (cmd, res) => res.HexBytes([0x05]))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        byte[] request = Framer.Encode("?"u8);
        await clientStream.WriteAsync(request);

        var framer = new Framer();
        byte[]? receivedPayload = null;
        byte[] readBuffer = new byte[256];
        while (receivedPayload is null) {
            int n = await clientStream.ReadAsync(readBuffer);
            framer.ProcessBytes(readBuffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    receivedPayload = evt.Payload;
                }
            });
        }

        Assert.That(receivedPayload, Is.EqualTo("05"u8.ToArray()));
    }

    /// <summary>
    /// ハンドラが Error(RspError) を呼ぶと、E NN が16進数2桁で整形されて
    /// 返ることを確認する(Code=10 は E0a になり、10進数の E10 にはならない)。
    /// </summary>
    [Test]
    public async Task SyncCommand_ThenError_ReturnsHexEncodedErrorReply() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));

        using var server = new StubServerBuilder()
            .Map(Commands.HaltReason, (cmd, res) => res.Error(new RspError(10, null)))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("?"u8));

        var framer = new Framer();
        byte[]? receivedPayload = null;
        byte[] readBuffer = new byte[256];
        while (receivedPayload is null) {
            int n = await clientStream.ReadAsync(readBuffer);
            framer.ProcessBytes(readBuffer.AsSpan(0, n), evt => {
                if (evt.Kind == FramerEventKind.Packet) {
                    receivedPayload = evt.Payload;
                }
            });
        }

        Assert.That(receivedPayload, Is.EqualTo("E0a"u8.ToArray()));
    }
}