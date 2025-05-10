namespace Gdb;

public interface IStreamCommunication {
    ValueTask<int> Read(Memory<byte> dest, CancellationToken token);
    ValueTask Write(ReadOnlyMemory<byte> src, CancellationToken token);
}