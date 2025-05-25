
using System.Threading.Channels;
using IsrEmulationTest.Gdb;

namespace Gdb;

/// <summary>
/// 読み書きを async await 形式で行うために使用します。
/// </summary>
public class ChannelCommunication : IStreamCommunication {
    static UnboundedChannelOptions option = new() { AllowSynchronousContinuations = true };

    readonly Channel<Memory<byte>> readChannel = Channel.CreateUnbounded<Memory<byte>>(option);
    readonly Channel<int> readCompleteNotify = Channel.CreateUnbounded<int>(option);

    readonly Channel<ReadOnlyMemory<byte>> writeChannel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>(option);
    readonly Channel<Unit> writeCompleteNotify = Channel.CreateUnbounded<Unit>(option);

    public async ValueTask<int> Read(Memory<byte> dest, CancellationToken token) {
        await readChannel.Writer.WriteAsync(dest, token);
        return await readCompleteNotify.Reader.ReadAsync(token);
    }

    public async ValueTask<Memory<byte>> WaitReadBuffer(CancellationToken token) {
        return await readChannel.Reader.ReadAsync(token);
    }

    public void CompleteReadNotify(int readLength) {
        readCompleteNotify.Writer.TryWrite(readLength);
    }

    public async ValueTask Write(ReadOnlyMemory<byte> src, CancellationToken token) {
        await writeChannel.Writer.WriteAsync(src, token);
        await writeCompleteNotify.Reader.ReadAsync(token);
    }

    public bool TryGetWriteBuffer(out ReadOnlyMemory<byte> result) {
        return writeChannel.Reader.TryRead(out result);
    }

    public async ValueTask<ReadOnlyMemory<byte>> WaitWriteBuffer(CancellationToken token) {
        return await writeChannel.Reader.ReadAsync(token);
    }

    public void CompleteWriteNotify() {
        writeCompleteNotify.Writer.TryWrite(Unit.Default);
    }
}