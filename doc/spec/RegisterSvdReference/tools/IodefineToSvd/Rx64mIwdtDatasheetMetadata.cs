namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 34章(Independent Watchdog Timer (IWDTa), p.1504-1511)から手動で書き写したレジスタ
/// ごとのリセット値・アクセス権限。全5レジスタについてマニュアル本文の個別レジスタ節
/// (34.2.1〜34.2.5)と突き合わせて確認済み(2026-07-29)。34.2.6のOption Function
/// Select Register 0 (OFS0)はフラッシュ設定領域(iodefine.hのst_iwdt構造体には
/// 含まれない)のため対象外。
/// </summary>
public static class Rx64mIwdtDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["IWDTRR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["IWDTCR"] = new RegisterDatasheetMetadata(0x33F3, 0xFFFF, "read-write"),
        ["IWDTSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["IWDTRCR"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
        ["IWDTCSTPR"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
    };
}