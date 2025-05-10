using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

public enum GDBMessageKind {
    /// <summary>
    /// バッファを溜め込み中
    /// </summary>
    StoreBuffer,

    /// <summary>
    /// 解析可能なGDBMessageあり
    /// </summary>
    HasMessage,

    /// <summary>
    /// チェックサムが不正なため読み飛ばした
    /// </summary>
    InvalidChecksum,
}

/// <summary>
/// GDMMessageの解析結果
/// </summary>
/// <param name="HasValue">GDBMessageがあるかどうか。ない場合は、</param>
/// <param name="ValueArea">#の次から$の前までのインデックス</param>
/// <param name="NextIndex">読み飛ばす長さ</param>
public record struct GDBMessageParseResult(
    GDBMessageKind ResultKind,
    Range ValueArea,
    int NextIndex
) {

    /// <summary>
    /// GDBMessageとして解析する範囲を返します。
    /// データが揃っていない場合(長さが足りない場合)は、Range(0, 0)を返します。
    /// </summary>
    public Range MessageArea =>
        ResultKind is GDBMessageKind.HasMessage or GDBMessageKind.InvalidChecksum
            ? new Range(ValueArea.Start.Value - 1, ValueArea.End.Value + 3)
            : new Range(0, 0);
}

public static class GDBUtils {

    /// <summary>
    /// 指定したオフセット位置からその後のデータを前に移動させます。
    /// </summary>
    public static void MoveAhead(ArrayBufferWriter<char> writer, int begin) {
        var bufferLength = writer.WrittenCount;
        writer.ResetWrittenCount();
        var span = writer.GetSpan(bufferLength)[..bufferLength];
        var newSpan = span[begin..];
        newSpan.CopyTo(span);
        writer.Advance(newSpan.Length);
    } 

    /// <summary>
    /// ビッグエンディアン ASCII 16進数に変換し、書き込んだ文字数(1)を返します。
    /// </summary>
    public static int EncodeHexByte(StringBuilder buf, byte value) {
        buf.Append($"{value:x2}");
        return 1;
    }

    // public static void EncodeHexBuf(StringBuilder buf, ReadOnlySpan<byte> values) {
    //     foreach (var value in values) {
    //         EncodeHexByte(buf, value);
    //     }
    // }

    /// <summary>
    /// ビッグエンディアン ASCII 16進数に変換し、書き込んだ文字数(4)を返します。
    /// </summary>
    public static int EncodeHexUInt32BE(StringBuilder buf, uint value) {
        buf.Append($"{value:x8}");
        return 4;
    }

    /// <summary>
    /// リトルエンディアン ASCII 16進数に変換し、書き込んだ文字数(4)を返します。
    /// </summary>
    public static int EncodeHexUInt32LE(StringBuilder buf, uint value) {
        uint swapped = BinaryPrimitives.ReverseEndianness(value);
        return EncodeHexUInt32BE(buf, value);
    }

    /// <summary>
    /// ビッグエンディアン ASCII 16進数に変換し、書き込んだ文字数(8)を返します。
    /// </summary>
    public static int EncodeHexUInt64BE(StringBuilder buf, ulong value) {
        buf.Append($"{value:x64}");
        return 8;
    }

    /// <summary>
    /// リトルエンディアン ASCII 16進数に変換し、書き込んだ文字数(8)を返します。
    /// </summary>
    public static int EncodeHexUInt64LE(StringBuilder buf, ulong value) {
        ulong swapped = BinaryPrimitives.ReverseEndianness(value);
        return EncodeHexUInt64BE(buf, value);
    }

    /// <summary>
    ///  ASCII 16進数形式で最大8バイトのデータを書き込みます。
    ///  上位の0は消去するので可変長になります。
    /// </summary>
    /// <param name="buf">書き込み先</param>
    /// <param name="value">ビックエンディアンで書き込む値</param>
    /// <returns>書き込んだ文字数</returns>
    public static int EncodeHexUIntegerBE(StringBuilder buf, ulong value) {
        var prevSize = buf.Length;
        buf.Append($"{value:x}");
        return buf.Length - prevSize;
    }

    static bool TryParseHexToByte(char ch, [NotNullWhen(true)] out byte value) {
        if (ch >= '0' && ch <= '9') {
            value = (byte)(ch - '0');
            return true;
        }
        if (ch >= 'A' && ch <= 'F') {
            value = (byte)(ch - 'A' + 10);
            return true;
        }
        if (ch >= 'a' && ch <= 'f') {
            value = (byte)(ch - 'a' + 10);
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// 指定した文字列(ASCII16進数)をバイトに変換します。
    /// 変換失敗した場合はfalseを返します。
    /// </summary>
    /// <param name="readLength">読み込んだ文字数(2)失敗(0)</param>
    public static bool TryDecodeHexU8(ReadOnlySpan<char> encoded, [NotNullWhen(true)] out byte value, out int readLength) {
        if (encoded.Length < 2 ||
            !TryParseHexToByte(encoded[0], out var a) ||
            !TryParseHexToByte(encoded[1], out var b)) { 
            value = 0;
            readLength = 0;
            return false;
        }
        value = (byte)(a << 4 | b);
        readLength = 2;
        return true;
    }

    /// <summary>
    /// 指定したビッグエンディアン文字列(ASCII16進数)をuintに変換します。
    /// 8文字以上ない場合や解析できない場合はfalseを返します。
    /// </summary>
    /// <param name="readLength">読み込んだ文字数(8)失敗(0)</param>
    public static bool TryDecodeHexUInt32BE(ReadOnlySpan<char> encoded, out uint value, out int readLength) {
        if (encoded.Length < 8 ||
            !TryDecodeHexU8(encoded, out var a, out _) ||
            !TryDecodeHexU8(encoded[2..], out var b, out _) ||
            !TryDecodeHexU8(encoded[4..], out var c, out _) ||
            !TryDecodeHexU8(encoded[6..], out var d, out _)) { 
            value = 0;
            readLength = 0;
            return false;
        }
        value = unchecked((uint)(
            (a << (8 * 3)) |
            (b << (8 * 2)) |
            (c << (8 * 1)) |
            (d << (8 * 0))));
        readLength = 8;
        return true;
    }

    /// <summary>
    /// 指定したリトルエンディアン文字列(ASCII16進数)をuintに変換します。
    /// 8文字以上ない場合や解析できない場合はfalseを返します。
    /// </summary>
    /// <param name="readLength">読み込んだ文字数(8)失敗(0)</param>
    public static bool TryDecodeHexUInt32LE(ReadOnlySpan<char> encoded, out uint value, out int readLength) {
        if (!TryDecodeHexUInt32BE(encoded, out value, out readLength)) {
            return false;
        }
        value = BinaryPrimitives.ReverseEndianness(value);
        return true;
    }

    /// <summary>
    /// 指定したビッグエンディアン文字列(ASCII16進数)をulongに変換します。
    /// 16文字以上ない場合や解析できない場合はfalseを返します。
    /// </summary>
    /// <param name="readLength">読み込んだ文字数(16)失敗(0)</param>
    public static bool TryDecodeHexUInt64BE(ReadOnlySpan<char> encoded, out ulong value, out int readLength) {
        if (encoded.Length < 16 ||
            !TryDecodeHexU8(encoded, out var a, out _) ||
            !TryDecodeHexU8(encoded[2..], out var b, out _) ||
            !TryDecodeHexU8(encoded[4..], out var c, out _) ||
            !TryDecodeHexU8(encoded[6..], out var d, out _) ||
            !TryDecodeHexU8(encoded[8..], out var e, out _) ||
            !TryDecodeHexU8(encoded[10..], out var f, out _) ||
            !TryDecodeHexU8(encoded[12..], out var g, out _) ||
            !TryDecodeHexU8(encoded[14..], out var h, out _)) { 
            value = 0;
            readLength = 0;
            return false;
        }
        value = unchecked((uint)(
            (a << (8 * 7)) |
            (b << (8 * 6)) |
            (c << (8 * 5)) |
            (d << (8 * 4)) |
            (e << (8 * 3)) |
            (f << (8 * 2)) |
            (g << (8 * 1)) |
            (h << (8 * 0))));
        readLength = 16;
        return true;
    }

    /// <summary>
    /// 指定したリトルエンディアン文字列(ASCII16進数)をulongに変換します。
    /// 16文字以上ない場合や解析できない場合はfalseを返します。
    /// </summary>
    /// <param name="readLength">読み込んだ文字数(16)失敗(0)</param>
    public static bool TryDecodeHexUInt64LE(ReadOnlySpan<char> encoded, out ulong value, out int readLength) {
        if (!TryDecodeHexUInt64BE(encoded, out value, out readLength)) {
            return false;
        }
        value = BinaryPrimitives.ReverseEndianness(value);
        return true;
    }

    /// <summary>
    /// 16進数として読み込めるだけ値を読み込みます。
    /// </summary>
    public static bool TryDecoeHexUIntegerBE(ReadOnlySpan<char> encoded, out ulong value, out int readLength) {
        int i = 0;
        ulong decoded = 0;
        bool successDecode = true;
        for (; i < encoded.Length && i < 17; i++) {
            successDecode = TryParseHexToByte(encoded[i], out var x);
            if (!successDecode) {
                break;
            }
            decoded = (decoded << 4) | x;
        }

        if (i == 0 || 16 <= i && successDecode) {
            // 短すぎたり、長すぎる場合は数字としてデコードできない
            value = 0;
            readLength = 0;
            return false;
        }
        value = decoded;
        readLength = i;
        return true;
    }

    // /// <summary>
    // /// 指定した文字列(ASCII16進数)をバイト配列に変換します。
    // /// 読み込んだ文字数を返します。
    // /// 長さが2の倍数でない場合やデコードに失敗した場合は0を返します。
    // /// また、失敗した場合は、ライターにデータは書き込んだままですが、位置は進めません。
    // /// </summary>
    // public static int TryDecodeHexBytes(IBufferWriter<byte> writer, ReadOnlySpan<char> encoded) {
    //     if (encoded.Length % 2 != 0) {
    //         return 0;
    //     }
    //     // 配列の長さを繰り上げる
    //     var bufLength = encoded.Length / 2;
    //     var buf = writer.GetSpan(bufLength)[..bufLength];
    //     for (int i = 0; i < buf.Length; i++) {
    //         if (TryDecodeHexByte(encoded.Slice(2 * i, 2), out var decoded) != 0) {
    //             return 0;
    //         }
    //         buf[i] = decoded;
    //     }
    //     writer.Advance(bufLength);
    //     return encoded.Length;
    // }

    // public static uint[] DecodeHexUInt32Array(string encoded) {
    //     var buf = DecodeHexBuf(encoded);
    //     // TODO 修正する
    //     var result = new uint[buf.Length / 4];
    //     for (int i = 0; i < result.Length; i++) {
    //         result[i] = BitConverter.ToUInt32(buf, i * 4);
    //     }
    //     return result;
    // }

    // public static uint DecodeHexUint32(string encoded) {
    //     return DecodeHexUInt32Array(encoded)[0];
    // }

    /// <summary>
    /// GDBMessageを解析します。
    public static GDBMessageParseResult ParseGDBMessage(ReadOnlySpan<char> buf) {
        int dolla = buf.IndexOf('$');
        if (dolla < 0) {
            // $ が見つからなかったので、全データを読み飛ばす
            return new GDBMessageParseResult(GDBMessageKind.StoreBuffer, new Range(0, 0), buf.Length);
        }
        var afterDolla = buf[(dolla + 1)..];
        int hash = afterDolla.IndexOf('#');
        if (hash < 0) {
            // # が見つからなかったので、$よりも前を捨て、後ろのデータを貯める
            return new GDBMessageParseResult(GDBMessageKind.StoreBuffer, new Range(0, 0), dolla);
        }
        var afterHash = afterDolla[(hash + 1)..];
        if (afterHash.Length < 2) {
            // $よりも前を捨てる
            // チェックサムが足らないので、貯める
            return new GDBMessageParseResult(GDBMessageKind.StoreBuffer, new Range(0, 0), dolla);
        }

        var cmd = afterDolla[..hash];
        var isValidCheckSum =
            TryDecodeHexU8(afterHash[..2], out var cksum, out _) &&
            GDBChecksum(cmd) == cksum;
        var nextIndex = dolla + 1 + hash + 3;
        if (!isValidCheckSum) {
            return new GDBMessageParseResult(GDBMessageKind.InvalidChecksum, new Range(0, 0), nextIndex);
        } else {
            var range = new Range(dolla + 1, dolla + 1 + hash);
            return new GDBMessageParseResult(GDBMessageKind.HasMessage, range, nextIndex);
        }
    }

    public static byte GDBChecksum(ReadOnlySpan<char> text, uint initialChecksum = 0) {
        unchecked {
            uint checksum = initialChecksum;
            foreach (var c in text) {
                checksum += c;
            }
            checksum &= 0xFF;
            return (byte)checksum;
        }
    }

    public static byte GDBChecksum(StringBuilder.ChunkEnumerator text, uint initialChecksum = 0) {
        unchecked {
            uint checksum = initialChecksum;
            foreach (var xs in text) {
                checksum += GDBChecksum(xs.Span, checksum);
            }
            return (byte)(checksum & 0xFF);
        }
    }

    public static void GDBMessage(StringBuilder buf, StringBuilder value) {
        buf.Append('$');
        buf.Append(value);
        buf.Append('#');
        EncodeHexByte(buf, GDBChecksum(value.GetChunks()));
    }

    public static void GDBMessage(StringBuilder buf, ReadOnlySpan<char> value) {
        buf.Append('$');
        buf.Append(value);
        buf.Append('#');
        EncodeHexByte(buf, GDBChecksum(value));
    }
}
