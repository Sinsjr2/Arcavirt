using System.Net;
using System.Net.Sockets;

namespace GdbStubDotnet;

public sealed class TcpTransport : ITransport, IDisposable {
    private readonly TcpListener _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;

    public TcpTransport(IPEndPoint endpoint) {
        _listener = new TcpListener(endpoint);
        _listener.Start();
    }

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public async Task AcceptAsync() {
        _client = await _listener.AcceptTcpClientAsync();
        _stream = _client.GetStream();
    }

    public async ValueTask<int> ReadAsync(Memory<byte> buffer) {
        if (_stream is null) {
            await AcceptAsync();
        }
        return await _stream!.ReadAsync(buffer);
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer) {
        if (_stream is null) {
            await AcceptAsync();
        }
        await _stream!.WriteAsync(buffer);
    }

    public void Close() {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
    }

    public void Dispose() {
        Close();
        _listener.Stop();
    }
}