namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 29章(8-Bit Timer (TMRb)、p.1377-1404の29.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。iodefine.h上のst_tmr0は
/// TMR0(0x88200)・TMR2(0x88210)の2インスタンスに対応する。
///
/// TCNT/TCORA/TCORB/TCR/TCCRの5レジスタは、データシート本文がTMR0.TCR 8200h,
/// TMR1.TCR 8201h, TMR2.TCR 8210h, TMR3.TCR 8211hのようにTMR0-TMR3の4チャネル分の
/// アドレスを1つの節・1つの"Value after reset"行にまとめて記載しており、
/// 4チャネル共通の値であることを確認済み(2026-07-30)。TCSRのみ、
/// 「TMR0.TCSR, TMR2.TCSR」と「TMR1.TCSR, TMR3.TCSR」で節・表が分かれている
/// (下記参照)。
///
/// TCSRのみ、st_tmr0(TMR0/TMR2)とst_tmr1(TMR1/TMR3)でビット構成が異なる
/// (st_tmr0側のみADTEビット(b4)を持つ)。これは本文の29.2.6節が
/// 「TMR0.TCSR, TMR2.TCSR」と「TMR1.TCSR, TMR3.TCSR」を別の表として記載しており、
/// iodefine.hのstruct定義(st_tmr0のTCSRにADTEフィールドがあり、st_tmr1には無い)と
/// 一致する、設計上の正規の差異であることを確認済み。
/// (このためst_tmr0/st_tmr1を1つのDatasheetMetadataクラスにまとめず、レジスタ名の
/// 重複を避けるためにも別ファイルとした。)
/// </summary>
public static class Rx64mTmr0DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 29.2.4 Timer Control Register (TCR) - TMR0/TMR2共通
        ["TCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 29.2.6 Timer Control/Status Register (TCSR) - TMR0.TCSR, TMR2.TCSR
        // b7-b5はUndefinedのため部分マスクで除外(ResetMask=0x1F)
        ["TCSR"] = new RegisterDatasheetMetadata(0x00, 0x1F, "read-write"),
        // 29.2.2 Time Constant Register A (TCORA) - TMR0/TMR2共通
        ["TCORA"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 29.2.3 Time Constant Register B (TCORB) - TMR0/TMR2共通
        ["TCORB"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 29.2.1 Timer Counter (TCNT) - TMR0/TMR2共通
        ["TCNT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 29.2.5 Timer Counter Control Register (TCCR) - TMR0/TMR2共通
        ["TCCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 29.2.7 Timer Counter Start Register (TCSTR) - TMR0/TMR2共通
        // b7-b1はUndefinedのため部分マスクで除外(ResetMask=0x01)
        ["TCSTR"] = new RegisterDatasheetMetadata(0x00, 0x01, "read-write"),
    };
}