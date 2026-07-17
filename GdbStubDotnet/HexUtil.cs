using System.Buffers;
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

    /// <summary>
    /// stop-reply本体("T"+signal/exitコードの16進数2桁)を書き込む。
    /// all-stopの単一stop reply(StubServer)とnon-stopの%Stop通知
    /// (NotificationQueue.StopNotification)の双方が同じ書式を必要とする
    /// ため、ここへ一本化している(重複実装の回避)。
    /// </summary>
    public static void WriteStopReplyText(IBufferWriter<byte> writer, int signalOrExit) {
        var span = writer.GetSpan(3);
        span[0] = (byte)'T';
        signalOrExit.TryFormat(span.Slice(1, 2), out _, "x2");
        writer.Advance(3);
    }
}