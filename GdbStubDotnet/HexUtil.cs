namespace GdbStubDotnet;

internal static class HexUtil {
    public static int NibbleValue(byte c) {
        return c switch {
            >= (byte)'0' and <= (byte)'9' => c - (byte)'0',
            >= (byte)'a' and <= (byte)'f' => c - (byte)'a' + 10,
            >= (byte)'A' and <= (byte)'F' => c - (byte)'A' + 10,
            _ => -1,
        };
    }

    public static bool IsHexDigit(byte c) {
        return NibbleValue(c) >= 0;
    }
}