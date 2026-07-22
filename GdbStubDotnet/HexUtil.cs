using System.Buffers;
using System.Text;

namespace GdbStubDotnet;

static class HexUtil {
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
    /// stop-reply本体("T"+signal/exitコードの16進数2桁+任意のthread:フィールド)を
    /// 書き込む。all-stopの単一stop reply(StubServer)とnon-stopの%Stop通知
    /// (NotificationQueue.StopNotification)の双方が同じ書式を必要とする
    /// ため、ここへ一本化している(重複実装の回避、Arcavirt-y1r)。
    /// thread == default(ThreadId)(Pid==0 かつ Tid==0、H未使用の単一
    /// スレッドターゲットで最も一般的なケース)のときは thread フィールド
    /// 自体を省略し、旧来どおりの裸の "Txx" を書く。real gdbはスレッドid
    /// 0を「無効/any」の特殊値として扱う実装がある(例: QEMU gdbstub)ため、
    /// 単一スレッド運用という最も広く使われる経路で "thread:0;" を送ると
    /// gdb側の解釈を壊すリスクがあり、これはL3(実gdb結合テスト)でも
    /// 検証できない(ReportStopを経由しないため)。安全側に倒し、デフォルト
    /// 値のときだけ省略する。
    /// thread != default の場合、Pid==0 なら "thread:&lt;tid&gt;;"(裸のtid
    /// 形式)、Pid!=0 なら "thread:p&lt;pid&gt;.&lt;tid&gt;;"(multiprocess
    /// 拡張形式)を書く。GDB-RPのmultiprocess拡張("p&lt;pid&gt;.&lt;tid&gt;")
    /// はgdb側がqSupportedで"multiprocess+"を受け取っていないと解釈を
    /// 保証されない(本ライブラリはQNonStop+のみ広告し、multiprocess+は
    /// 広告していない)。ThreadIdRefパーサ(H/vContの受信側)もPid未指定時は
    /// Pid=0として扱う対称設計のため、Pid==0の裸形式はどのgdbでも安全に
    /// 解釈できる最小前提のケースに一致させている。Pid!=0(利用者が明示的に
    /// マルチプロセスを使う場合)は素直にp形式で書く。
    /// </summary>
    public static void WriteStopReplyText(IBufferWriter<byte> writer, int signalOrExit, ThreadId thread) {
        if (thread == default) {
            var bareSpan = writer.GetSpan(3);
            bareSpan[0] = (byte)'T';
            signalOrExit.TryFormat(bareSpan.Slice(1, 2), out _, "x2");
            writer.Advance(3);
            return;
        }
        Span<byte> pidHex = stackalloc byte[16];
        Span<byte> tidHex = stackalloc byte[16];
        thread.Tid.TryFormat(tidHex, out int tidLen, "x");
        bool includePid = thread.Pid != 0;
        int pidLen = 0;
        if (includePid) {
            thread.Pid.TryFormat(pidHex, out pidLen, "x");
        }
        int threadFieldLen = "thread:"u8.Length + (includePid ? 1 + pidLen + 1 : 0) + tidLen + 1;
        var span = writer.GetSpan(3 + threadFieldLen);
        span[0] = (byte)'T';
        signalOrExit.TryFormat(span.Slice(1, 2), out _, "x2");
        int pos = 3;
        "thread:"u8.CopyTo(span[pos..]);
        pos += "thread:"u8.Length;
        if (includePid) {
            span[pos++] = (byte)'p';
            pidHex[..pidLen].CopyTo(span[pos..]);
            pos += pidLen;
            span[pos++] = (byte)'.';
        }
        tidHex[..tidLen].CopyTo(span[pos..]);
        pos += tidLen;
        span[pos++] = (byte)';';
        writer.Advance(pos);
    }
}