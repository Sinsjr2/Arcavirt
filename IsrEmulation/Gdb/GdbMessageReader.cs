namespace Gdb;

public struct GdbMessageReader {
    readonly ReadOnlyMemory<char> buffer;

    int position;

    /// <summary>
    /// 残り読み取り可能なcharの長さ
    /// </summary>
    public int RemainingLength => buffer.Length - position;

    public GdbMessageReader(ReadOnlyMemory<char> buffer) {
        this.buffer = buffer;
        this.position = 0;
    }

    /// <summary>
    /// 指定した長さ(インデックス分)読み飛ばします。
    /// </summary>
    public bool TrySkip(int length) {
        var nextPos = position + length;
        if (buffer.Length <= nextPos) {
            return false;
        }
        position = nextPos;
        return true;
    }

    public bool TryReadChar(out char result) {
        if (buffer.Length <= position) {
            result = '\0';
            return false;
        }
        result = buffer.Span[position];
        position++;
        return true;
    }

    public bool TryReadIfExpChar(char expected) {
        if (buffer.Length <= position) {
            return false;
        }
        if (expected != buffer.Span[position]) {
            return false;
        }
        position++;
        return true;
    }

    /// <summary>
    /// 期待する文字の場合読み込みます。
    /// expectedが空のときはtrueを返します。
    /// </summary>
    public bool TryReadIfExpString(ReadOnlySpan<char> expected) {
        var remaingStr = buffer[position..];
        if (remaingStr.Length < expected.Length) {
            return false;
        }
        bool equals = remaingStr.Slice(0, expected.Length).Span.SequenceEqual(expected);
        if (!equals) {
            return false;
        }
        position += expected.Length;
        return true;
    }

    public bool TryReadHexUIntegerBE(out ulong result) {
        if (!GDBUtils.TryDecoeHexUIntegerBE(buffer.Span[position..], out var value, out var readLength)) {
            result = 0;
            return false;
        }
        position += readLength;
        result = value;
        return true;
    }

    /// <summary>
    /// 指定されたchar分読み込みます。
    /// 指定された長さよりも残りの長さが短かった場合はfalseを返します。
    /// </summary>
    public bool TryReadExpectedLength(int length, out ReadOnlyMemory<char> result) {
        if (RemainingLength < length) {
            result = ReadOnlyMemory<char>.Empty;
            return false;
        }
        result = buffer.Slice(position, length);
        position += length;
        return true;
    }
}
