using System.Buffers;

namespace GdbStubDotnet;

public readonly struct SyncResponse;
public readonly struct ExecResponse;

public readonly ref struct ResponseWriter<TKind> {
    readonly IBufferWriter<byte> output;
    readonly bool detailedErrors;

    internal ResponseWriter(IBufferWriter<byte> output, bool detailedErrors) {
        this.output = output;
        this.detailedErrors = detailedErrors;
    }

    public void Ok() {
        var span = output.GetSpan(2);
        span[0] = (byte)'O';
        span[1] = (byte)'K';
        output.Advance(2);
    }

    public void HexBytes(ReadOnlySpan<byte> data) {
        var span = output.GetSpan(data.Length * 2);
        for (int i = 0; i < data.Length; i++) {
            data[i].TryFormat(span.Slice(i * 2, 2), out _, "x2");
        }
        output.Advance(data.Length * 2);
    }

    /// <summary>
    /// hex エンコードを行わない生テキスト応答(qSupported の機能一覧等)。
    /// </summary>
    public void Text(ReadOnlySpan<byte> text) {
        var span = output.GetSpan(text.Length);
        text.CopyTo(span);
        output.Advance(text.Length);
    }

    public void Error(RspError error) {
        byte[] bytes = HexUtil.EncodeError(error, detailedErrors);
        var span = output.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        output.Advance(bytes.Length);
    }

    public void Empty() {
    }
}