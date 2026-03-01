
namespace Gdb;

/// <summary>
/// 読み書きを Stream に対して行います。
/// </summary>
public class StreamCommunication : IStreamCommunication {

    readonly Stream readStream;
    readonly Stream writeStream;

    public StreamCommunication(Stream readStream, Stream writeStream) {
        this.readStream = readStream;
        this.writeStream = writeStream;
    }

    public async ValueTask<int> Read(Memory<byte> dest, CancellationToken token) {
        return await readStream.ReadAsync(dest, token);
    }

    public async ValueTask Write(ReadOnlyMemory<byte> src, CancellationToken token) {
        await writeStream.WriteAsync(src, token);
    }
}