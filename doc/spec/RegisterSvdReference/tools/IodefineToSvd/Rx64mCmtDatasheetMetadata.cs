namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 30章(Compare Match Timer, p.1405-1412)・31章(Compare Match Timer W, p.1413-1420)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。
/// 各レジスタのビットフィールド名の集合は、iodefine.hから自動抽出した結果と
/// 完全一致することを確認済み(2026-07-28)。
/// </summary>
public static class Rx64mCmtDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["CMSTR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CMSTR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CMCR"] = new RegisterDatasheetMetadata(0x0000, 0xFF7F, "read-write"),
        ["CMCNT"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CMCOR"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["CMWSTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CMWCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CMWIOR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CMWCNT"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CMWCOR"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
        ["CMWICR0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["CMWICR1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["CMWOCR0"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
        ["CMWOCR1"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
    };
}