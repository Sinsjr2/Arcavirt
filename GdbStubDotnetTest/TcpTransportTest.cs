using System.Net;
using System.Net.Sockets;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class TcpTransportTest {
    /// <summary>
    /// サーバー側(TcpTransport)が WriteAsync で送信したバイト列を、
    /// クライアント側(TcpClient)が同一内容で受信できることを確認する。
    /// </summary>
    [Test]
    public async Task WriteAsync_AfterAccept_ClientReceivesSameBytes() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var acceptTask = transport.AcceptAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        await acceptTask;

        byte[] sent = [0x01, 0x02, 0x03, 0x04];
        await transport.WriteAsync(sent);

        byte[] received = new byte[sent.Length];
        int totalRead = 0;
        var clientStream = client.GetStream();
        while (totalRead < received.Length) {
            int n = await clientStream.ReadAsync(received.AsMemory(totalRead));
            totalRead += n;
        }

        Assert.That(received, Is.EqualTo(sent));
    }

    /// <summary>
    /// クライアント側(TcpClient)が送信したバイト列を、
    /// サーバー側(TcpTransport)の ReadAsync で同一内容で受信できることを確認する。
    /// </summary>
    [Test]
    public async Task ReadAsync_AfterClientWrite_ServerReceivesSameBytes() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var acceptTask = transport.AcceptAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        await acceptTask;

        byte[] sent = [0xAA, 0xBB, 0xCC];
        await client.GetStream().WriteAsync(sent);

        byte[] received = new byte[sent.Length];
        int totalRead = 0;
        while (totalRead < received.Length) {
            int n = await transport.ReadAsync(received.AsMemory(totalRead));
            totalRead += n;
        }

        Assert.That(received, Is.EqualTo(sent));
    }

    /// <summary>
    /// Close で現在のクライアント接続を閉じた後、AcceptAsync を再度呼ぶと
    /// 新しいクライアント接続を受け入れられることを確認する
    /// (「切断後の再 listen による再接続可」という仕様上の要件)。
    /// </summary>
    [Test]
    public async Task AcceptAsync_AfterClose_AcceptsNewConnection() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var firstAccept = transport.AcceptAsync();

        using (var firstClient = new TcpClient()) {
            await firstClient.ConnectAsync(IPAddress.Loopback, transport.Port);
            await firstAccept;
        }

        transport.Close();

        var secondAccept = transport.AcceptAsync();
        using var secondClient = new TcpClient();
        await secondClient.ConnectAsync(IPAddress.Loopback, transport.Port);
        await secondAccept;

        byte[] sent = [0x42];
        await transport.WriteAsync(sent);

        byte[] received = new byte[1];
        int n = await secondClient.GetStream().ReadAsync(received);

        Assert.Multiple(() => {
            Assert.That(n, Is.EqualTo(1));
            Assert.That(received, Is.EqualTo(sent));
        });
    }
}