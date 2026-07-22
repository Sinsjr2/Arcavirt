using System.Net;
using System.Net.Sockets;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class SetThreadTest {
    /// <summary>
    /// Hg1 でスレッド1を選択した後、g(ReadRegisters)コマンドを送ると、
    /// ハンドラが受け取る ReadRegistersCommand.Thread に選択したスレッドが
    /// 充填されていることを確認する(§6.6、Arcavirt-o3e.14)。
    /// H は利用者が Map で何も登録しなくても既定ハンドラが動作する。
    /// </summary>
    [Test]
    public async Task SetThread_ThenReadRegisters_FillsSelectedThreadIntoCommand() {
        using var transport = new TcpTransport(new IPEndPoint(IPAddress.Loopback, 0));
        var capturedThread = new TaskCompletionSource<ThreadId>();

        using var server = new StubServerBuilder()
            .Map(Commands.ReadRegisters, (cmd, res) => {
                capturedThread.SetResult(cmd.Thread);
                res.HexBytes([0x01]);
            })
            .UseTransport(transport)
            .Build();
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, transport.Port);
        var clientStream = client.GetStream();

        await clientStream.WriteAsync(Framer.Encode("Hg1"u8));
        byte[]? setThreadAck = await ReadOnePacket(clientStream);
        Assert.That(setThreadAck, Is.EqualTo("OK"u8.ToArray()));

        await clientStream.WriteAsync(Framer.Encode("g"u8));
        byte[]? readRegistersReply = await ReadOnePacket(clientStream);

        ThreadId thread = await capturedThread.Task;
        Assert.Multiple(() => {
            Assert.That(thread, Is.EqualTo(new ThreadId(0, 1)));
            Assert.That(readRegistersReply, Is.EqualTo("01"u8.ToArray()));
        });
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