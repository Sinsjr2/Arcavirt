namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 58章(12-Bit D/A Converter (R12DA), p.2725-2731)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。全7レジスタについてマニュアル本文の個別レジスタ節
/// (58.2.1〜58.2.6)と突き合わせて確認済み(2026-07-29)。
///
/// DACRはb4-b0(Reserved)が「read as 1」と明記されており、リセット値0x1Fに
/// この1固定のReservedビットを反映している。
/// </summary>
public static class Rx64mDaDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["DADR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DADR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DACR"] = new RegisterDatasheetMetadata(0x1F, 0xFF, "read-write"),
        ["DADPR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DAADSCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DAAMPCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DAADUSR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}