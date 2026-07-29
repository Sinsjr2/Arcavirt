namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 33章(Watchdog Timer (WDTA), p.1490-1494)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。全4レジスタについてマニュアル本文の個別レジスタ節
/// (33.2.1〜33.2.4)と突き合わせて確認済み(2026-07-29)。33.2.5のOption Function
/// Select Register 0 (OFS0)はフラッシュ設定領域(iodefine.hのst_wdt構造体には
/// 含まれない)のため対象外。
/// </summary>
public static class Rx64mWdtDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["WDTRR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["WDTCR"] = new RegisterDatasheetMetadata(0x33F3, 0xFFFF, "read-write"),
        ["WDTSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["WDTRCR"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
    };
}