using System.Text;

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

    /// <summary>
    /// エラー応答を整形する。detailed が false、または error.Detail が
    /// null の場合は "E NN"(16進数2桁)。detailed が true かつ Detail が
    /// 設定されている場合は "E.&lt;text&gt;"(人間可読、§7.3)。
    /// </summary>
    public static byte[] EncodeError(RspError error, bool detailed) {
        if (detailed && error.Detail is not null) {
            return Encoding.ASCII.GetBytes("E." + error.Detail);
        }
        return Encoding.ASCII.GetBytes("E" + error.Code.ToString("x2"));
    }
}