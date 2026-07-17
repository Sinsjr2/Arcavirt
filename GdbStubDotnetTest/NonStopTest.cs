using System.Net;
using System.Net.Sockets;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class NonStopTest {
    /// <summary>
    /// QNonStop:1 で non-stop を有効化した後、vCont;c に対して即時 OK が
    /// 返り、対象スレッドの ReportStop で %Stop 通知(Stop:T05)が非同期に
    /// 送出され、続く vStopped で(保留無しのため)OK が返ることを確認する
    /// (§4.3手順5・§4.5の一連の流れ、Arcavirt-o3e.10.3の受け入れ基準)。
    /// QNonStop/vStopped はライブラリ組込みの既定処理であり、利用者は
    /// Map で何も登録していない(non-stop対応に利用者コードは不要)。
    /// </summary>
    [Test]
    public async Task NonStopFlow_VContThenReportStop_SendsImmediateOkThenStopNotificationThenVStoppedOk() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var handlerInvoked = new TaskCompletionSource<ExecutionResponder>();

        using var server = new StubServerBuilder()
            .Map(Commands.VCont, (VContCommand cmd, ExecutionResponder responder) => handlerInvoked.SetResult(responder))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("QNonStop:1"u8));
        byte[]? nonStopAck = await ReadOnePacket(clientStream);
        Assert.That(nonStopAck, Is.EqualTo("OK"u8.ToArray()));

        await clientStream.WriteAsync(Framer.Encode("vCont;c"u8));
        byte[]? immediateOk = await ReadOnePacket(clientStream);
        Assert.That(immediateOk, Is.EqualTo("OK"u8.ToArray()));

        ExecutionResponder responder = await handlerInvoked.Task;
        responder.ReportStop(new StopEvent(default, StopReason.Signal, 5, 0));

        byte[]? notification = await ReadOnePacket(clientStream);
        Assert.That(notification, Is.EqualTo("Stop:T05"u8.ToArray()));

        await clientStream.WriteAsync(Framer.Encode("vStopped"u8));
        byte[]? drainReply = await ReadOnePacket(clientStream);
        Assert.That(drainReply, Is.EqualTo("OK"u8.ToArray()));
    }

    /// <summary>
    /// non-stopで、ハンドラが同期的にResponder.Rejectを呼んだ場合、
    /// vCont;c の応答は即時OKではなくReject由来のエラー応答1回だけに
    /// なることを確認する(コードレビューで判明した不具合の回帰防止:
    /// 修正前は即時OKとエラー応答が二重に送出されていた)。
    /// </summary>
    [Test]
    public async Task VContCommand_SynchronousRejectInNonStop_ReturnsOnlyRejectErrorNotDoubleResponse() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));

        using var server = new StubServerBuilder()
            .Map(Commands.VCont, (VContCommand cmd, ExecutionResponder responder) => responder.Reject(new RspError(14, null)))
            .Map(Commands.HaltReason, (cmd, res) => res.Text("S05"u8))
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("QNonStop:1"u8));
        byte[]? nonStopAck = await ReadOnePacket(clientStream);
        Assert.That(nonStopAck, Is.EqualTo("OK"u8.ToArray()));

        await clientStream.WriteAsync(Framer.Encode("vCont;c"u8));
        byte[]? rejectReply = await ReadOnePacket(clientStream);
        Assert.That(rejectReply, Is.EqualTo("E0e"u8.ToArray()));

        await clientStream.WriteAsync(Framer.Encode("?"u8));
        byte[]? haltReply = await ReadOnePacket(clientStream);
        Assert.That(haltReply, Is.EqualTo("S05"u8.ToArray()));
    }

    /// <summary>
    /// 1回のstream.ReadAsyncチャンクに複数パケットが含まれる場合(ackの
    /// 直後に即座に応答が来る等、TCPが複数の送出をまとめて配送すること
    /// がある)、Framer.ProcessBytesは1回のProcessBytes呼び出し内で
    /// コールバックを複数回発火させる。最後に見つかったパケットで
    /// 上書きしてしまうと「本来先に届くはずの余分な応答」を見失い、
    /// 二重応答バグ等を検出できなくなる(コードレビューで判明)。
    /// received が既に確定していれば以降のPacketイベントは無視し、
    /// 最初の1件だけを返す。
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