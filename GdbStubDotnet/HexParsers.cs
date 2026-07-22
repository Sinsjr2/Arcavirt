using Pidgin;

namespace GdbStubDotnet;

/// <summary>
/// RSP のワイヤ上で頻出する16進数表記を byte トークンストリームから
/// 解析するための共有コンビネータ。GDB-RP のパケット構文は各コマンド
/// パーサーが直接参照し、本ファイルはその共通部品のみを提供する。
/// </summary>
static class HexParsers {
    static readonly Parser<byte, byte> hexDigit =
        Parser<byte>.Token(HexUtil.IsHexDigit);

    public static readonly Parser<byte, ulong> HexULong =
        hexDigit.AtLeastOnce().Select(static digits => {
            ulong value = 0;
            foreach (byte d in digits) {
                value = (value << 4) | (ulong)(uint)HexUtil.NibbleValue(d);
            }
            return value;
        });

    static readonly Parser<byte, byte> hexBytePair =
        hexDigit.Then(hexDigit, static (hi, lo) => (byte)(((uint)HexUtil.NibbleValue(hi) << 4) | (uint)HexUtil.NibbleValue(lo)));

    public static readonly Parser<byte, byte[]> HexBytesRest =
        hexBytePair.Many().Select(static bytes => bytes.ToArray());
}