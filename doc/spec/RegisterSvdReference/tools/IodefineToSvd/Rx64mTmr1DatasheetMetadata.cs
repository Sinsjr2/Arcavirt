namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 29章(8-Bit Timer (TMRb)、p.1377-1404の29.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。iodefine.h上のst_tmr1は
/// TMR1(0x88201)・TMR3(0x88211)の2インスタンスに対応する。
///
/// TCNT/TCORA/TCORB/TCR/TCCRの5レジスタは、データシート本文がTMR0.TCR 8200h,
/// TMR1.TCR 8201h, TMR2.TCR 8210h, TMR3.TCR 8211hのようにTMR0-TMR3の4チャネル分の
/// アドレスを1つの節・1つの"Value after reset"行にまとめて記載しており、
/// 4チャネル共通の値であることを確認済み(2026-07-30)。TCSRのみ、
/// 「TMR0.TCSR, TMR2.TCSR」と「TMR1.TCSR, TMR3.TCSR」で節・表が分かれている
/// (下記参照)。
///
/// TCSRのみ、st_tmr1(TMR1/TMR3)とst_tmr0(TMR0/TMR2)でビット構成が異なる。
/// st_tmr1側のTCSRはADTEビットを持たず、該当するb4は「読み出しは常に1固定・
/// 書き込み値は1にすること」という予約ビットである(iodefine.hのst_tmr1.TCSRにも
/// ADTEフィールドが無く、この差異と一致する、設計上の正規の差異であることを確認済み)。
/// (このためst_tmr0/st_tmr1を1つのDatasheetMetadataクラスにまとめず、レジスタ名の
/// 重複を避けるためにも別ファイルとした。)
/// </summary>
public static class Rx64mTmr1DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 29.2.4 Timer Control Register (TCR) - TMR1/TMR3共通
        ["TCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 29.2.6 Timer Control/Status Register (TCSR) - TMR1.TCSR, TMR3.TCSR
        // b4は「読み出しは1固定・書き込み値は1にすること」の予約ビット(定義済み値のためマスクに含める)
        // b7-b5はUndefinedのため部分マスクで除外(ResetMask=0x1F)
        ["TCSR"] = new RegisterDatasheetMetadata(0x10, 0x1F, "read-write"),
        // 29.2.2 Time Constant Register A (TCORA) - TMR1/TMR3共通
        ["TCORA"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 29.2.3 Time Constant Register B (TCORB) - TMR1/TMR3共通
        ["TCORB"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 29.2.1 Timer Counter (TCNT) - TMR1/TMR3共通
        ["TCNT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 29.2.5 Timer Counter Control Register (TCCR) - TMR1/TMR3共通
        ["TCCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 29.2.7 Timer Counter Start Register (TCSTR) - TMR1/TMR3共通
        // b7-b1はUndefinedのため部分マスクで除外(ResetMask=0x01)
        ["TCSTR"] = new RegisterDatasheetMetadata(0x00, 0x01, "read-write"),
    };
}