using System.Net;
using System.Net.Sockets;

namespace GdbStubDotnet;

public sealed class TcpTransport : ITransport, IDisposable {
    readonly TcpListener listener;
    TcpClient? client;
    NetworkStream? stream;

    public TcpTransport(IPEndPoint endpoint) {
        listener = new TcpListener(endpoint);
        listener.Start();
    }

    public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    public async Task AcceptAsync() {
        client = await listener.AcceptTcpClientAsync();
        stream = client.GetStream();
    }

    public async ValueTask<int> ReadAsync(Memory<byte> buffer) {
        if (stream is null) {
            await AcceptAsync();
        }
        return await stream!.ReadAsync(buffer);
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer) {
        if (stream is null) {
            await AcceptAsync();
        }
        await stream!.WriteAsync(buffer);
    }

    public void Close() {
        stream?.Dispose();
        client?.Dispose();
        stream = null;
        client = null;
    }

    public void Dispose() {
        Close();
        listener.Stop();
    }
}