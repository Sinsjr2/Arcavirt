namespace GdbStubDotnet;

public enum FramerEventKind {
    Packet,
    ChecksumMismatch,
    Ack,
    Nak,
}

public readonly record struct FramerEvent(FramerEventKind Kind, byte[]? Payload = null);

/// <summary>
/// RSP のフレーミング状態機械。$payload#cs の組立とチェックサム検証のみを担う
/// (RLE・} エスケープのデコードは対象外。ソケットに依存せずバイト列だけで完結する)。
/// 通知パケット('%payload#cs')も同じ状態機械でデコードする('$'と'%'を同様に
/// ペイロード開始として扱う。実 gdb クライアントは通知を送信しないため、
/// この受信側対応は主にテストハーネスが本ライブラリ自身の通知送出を
/// 検証するためのもの)。
/// </summary>
public sealed class Framer {
    enum State {
        Idle,
        InPayload,
        InChecksumHigh,
        InChecksumLow,
    }

    State state = State.Idle;
    readonly List<byte> payloadBuffer = [];
    byte computedChecksum;
    byte checksumHighNibble;

    public void ProcessBytes(ReadOnlySpan<byte> chunk, Action<FramerEvent> onEvent) {
        foreach (byte b in chunk) {
            ProcessByte(b, onEvent);
        }
    }

    void ProcessByte(byte b, Action<FramerEvent> onEvent) {
        switch (state) {
            case State.Idle:
                if (b == (byte)'$' || b == (byte)'%') {
                    payloadBuffer.Clear();
                    computedChecksum = 0;
                    state = State.InPayload;
                } else if (b == (byte)'+') {
                    onEvent(new FramerEvent(FramerEventKind.Ack));
                } else if (b == (byte)'-') {
                    onEvent(new FramerEvent(FramerEventKind.Nak));
                }
                // 0x03(interrupt)等、上記以外のバイトは本層のスコープ外として無視する。
                break;

            case State.InPayload:
                if (b == (byte)'#') {
                    state = State.InChecksumHigh;
                } else {
                    payloadBuffer.Add(b);
                    unchecked { computedChecksum += b; }
                }
                break;

            case State.InChecksumHigh:
                checksumHighNibble = b;
                state = State.InChecksumLow;
                break;

            case State.InChecksumLow:
                int received = (HexUtil.NibbleValue(checksumHighNibble) << 4) | HexUtil.NibbleValue(b);
                onEvent(received == computedChecksum
                    ? new FramerEvent(FramerEventKind.Packet, payloadBuffer.ToArray())
                    : new FramerEvent(FramerEventKind.ChecksumMismatch));
                state = State.Idle;
                break;
        }
    }

    public static byte[] Encode(ReadOnlySpan<byte> payload) {
        return EncodeFramed((byte)'$', payload);
    }

    /// <summary>
    /// 非同期通知('%payload#cs')を組み立てる(§4.5)。チェックサム計算は
    /// 通常応答と同じで、先頭マーカーのみ異なる。
    /// </summary>
    public static byte[] EncodeNotification(ReadOnlySpan<byte> payload) {
        return EncodeFramed((byte)'%', payload);
    }

    static byte[] EncodeFramed(byte marker, ReadOnlySpan<byte> payload) {
        byte checksum = 0;
        foreach (byte b in payload) {
            unchecked { checksum += b; }
        }

        byte[] result = new byte[payload.Length + 4];
        result[0] = marker;
        payload.CopyTo(result.AsSpan(1));
        result[^3] = (byte)'#';
        string hex = checksum.ToString("x2");
        result[^2] = (byte)hex[0];
        result[^1] = (byte)hex[1];
        return result;
    }
}