using System.Buffers;

namespace GdbStubDotnet;

public readonly struct SyncResponse;
public readonly struct ExecResponse;

public readonly ref struct ResponseWriter<TKind> {
    private readonly IBufferWriter<byte> _output;

    internal ResponseWriter(IBufferWriter<byte> output) {
        _output = output;
    }

    public void Ok() {
        var span = _output.GetSpan(2);
        span[0] = (byte)'O';
        span[1] = (byte)'K';
        _output.Advance(2);
    }

    public void HexBytes(ReadOnlySpan<byte> data) {
        var span = _output.GetSpan(data.Length * 2);
        for (int i = 0; i < data.Length; i++) {
            data[i].TryFormat(span.Slice(i * 2, 2), out _, "x2");
        }
        _output.Advance(data.Length * 2);
    }

    public void Error(RspError error) {
        byte[] bytes = HexUtil.EncodeError(error);
        var span = _output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        _output.Advance(bytes.Length);
    }

    public void Empty() {
    }
}