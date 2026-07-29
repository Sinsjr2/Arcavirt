namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 60章(Data Operation Circuit (DOC), p.2744-2745)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。全3レジスタについてマニュアル本文の個別レジスタ節
/// (60.2.1〜60.2.3)と突き合わせて確認済み(2026-07-29)。
/// </summary>
public static class Rx64mDocDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["DOCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DODIR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DODSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
    };
}