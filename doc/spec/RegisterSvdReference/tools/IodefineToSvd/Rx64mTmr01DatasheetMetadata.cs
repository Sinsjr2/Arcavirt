namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 29章(8-Bit Timer (TMRb)、p.1377-1404の29.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。iodefine.h上のst_tmr01は
/// TMR01(0x88204)・TMR23(0x88214)の2インスタンスに対応する。
///
/// TMR01/TMR23は、TMR0(TMR2)とTMR1(TMR3)の2つの8bitタイマーユニットのTCORA/TCORB/
/// TCNT/TCCRを1組の16bitレジスタとしてまとめてアクセスするための別名アドレスであり、
/// データシートにTMR01/TMR23専用の個別のレジスタ節・ビット表は存在しない。TCR/TCSR/
/// TCSTRに相当する16bitレジスタは存在しない。
///
/// TCORA(29.2.2節)・TCORB(29.2.3節)・TCNT(29.2.1節)の3レジスタは、各節内に
/// TMR0(TMR2)側を上位8bit・TMR1(TMR3)側を下位8bitとする合成ビット図が併記されており、
/// TMR01/TMR23の"Value after reset"を本文から直接読み取れる(2026-07-30確認済み)。
/// TCCR(29.2.5節)にはこの合成ビット図が無く、8bit版のビット図のみだが、本文に
/// 「Two TCCR registers can be accessed simultaneously by accessing the address of
/// the even channel TCCR register in 16-bit units.」との記載があるため、TMR0(TMR2).
/// TCCR(上位8bit)とTMR1(TMR3).TCCR(下位8bit)のリセット値を連結して導出した。
/// </summary>
public static class Rx64mTmr01DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 29.2.2 TCORA - TMR0(TMR2).TCORA=0xFF(上位8bit) / TMR1(TMR3).TCORA=0xFF(下位8bit)
        ["TCORA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 29.2.3 TCORB - TMR0(TMR2).TCORB=0xFF(上位8bit) / TMR1(TMR3).TCORB=0xFF(下位8bit)
        ["TCORB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 29.2.1 TCNT - TMR0(TMR2).TCNT=0x00(上位8bit) / TMR1(TMR3).TCNT=0x00(下位8bit)
        ["TCNT"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 29.2.5 TCCR - TMR0(TMR2).TCCR=0x00(上位8bit) / TMR1(TMR3).TCCR=0x00(下位8bit)
        ["TCCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
    };
}