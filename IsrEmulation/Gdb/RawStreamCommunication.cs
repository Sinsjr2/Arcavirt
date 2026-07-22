
namespace Gdb;

/// <summary>
/// Stream に対して読み書きを行います。
/// </summary>
public class RawStreamCommunication : IStreamCommunication {
    readonly Stream targetReadStream;
    readonly Stream targetWriteStream;

    public RawStreamCommunication(Stream targetStream, Stream targetWriteStream) {
        targetReadStream = targetStream;
        this.targetWriteStream = targetWriteStream;
    }

    public async ValueTask<int> Read(Memory<byte> dest, CancellationToken token) {
        return await targetReadStream.ReadAsync(dest, token);
    }

    public async ValueTask Write(ReadOnlyMemory<byte> src, CancellationToken token) {
        await targetWriteStream.WriteAsync(src, token);
    }
}