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
/// </summary>
public sealed class Framer {
    private enum State {
        Idle,
        InPayload,
        InChecksumHigh,
        InChecksumLow,
    }

    private State _state = State.Idle;
    private readonly List<byte> _payloadBuffer = [];
    private byte _computedChecksum;
    private byte _checksumHighNibble;

    public void ProcessBytes(ReadOnlySpan<byte> chunk, Action<FramerEvent> onEvent) {
        foreach (byte b in chunk) {
            ProcessByte(b, onEvent);
        }
    }

    private void ProcessByte(byte b, Action<FramerEvent> onEvent) {
        switch (_state) {
            case State.Idle:
                if (b == (byte)'$') {
                    _payloadBuffer.Clear();
                    _computedChecksum = 0;
                    _state = State.InPayload;
                } else if (b == (byte)'+') {
                    onEvent(new FramerEvent(FramerEventKind.Ack));
                } else if (b == (byte)'-') {
                    onEvent(new FramerEvent(FramerEventKind.Nak));
                }
                // 0x03(interrupt)等、上記以外のバイトは本層のスコープ外として無視する。
                break;

            case State.InPayload:
                if (b == (byte)'#') {
                    _state = State.InChecksumHigh;
                } else {
                    _payloadBuffer.Add(b);
                    unchecked { _computedChecksum += b; }
                }
                break;

            case State.InChecksumHigh:
                _checksumHighNibble = b;
                _state = State.InChecksumLow;
                break;

            case State.InChecksumLow:
                int received = (HexNibble(_checksumHighNibble) << 4) | HexNibble(b);
                onEvent(received == _computedChecksum
                    ? new FramerEvent(FramerEventKind.Packet, _payloadBuffer.ToArray())
                    : new FramerEvent(FramerEventKind.ChecksumMismatch));
                _state = State.Idle;
                break;
        }
    }

    private static int HexNibble(byte c) {
        return c switch {
            >= (byte)'0' and <= (byte)'9' => c - (byte)'0',
            >= (byte)'a' and <= (byte)'f' => c - (byte)'a' + 10,
            >= (byte)'A' and <= (byte)'F' => c - (byte)'A' + 10,
            _ => 0,
        };
    }

    public static byte[] Encode(ReadOnlySpan<byte> payload) {
        byte checksum = 0;
        foreach (byte b in payload) {
            unchecked { checksum += b; }
        }

        byte[] result = new byte[payload.Length + 4];
        result[0] = (byte)'$';
        payload.CopyTo(result.AsSpan(1));
        result[^3] = (byte)'#';
        string hex = checksum.ToString("x2");
        result[^2] = (byte)hex[0];
        result[^1] = (byte)hex[1];
        return result;
    }
}