namespace GdbStubDotnet;

public interface ITransport {
    ValueTask<int> ReadAsync(Memory<byte> buffer);
    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer);
    void Close();
}