namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 10章(Clock Frequency Accuracy Measurement Circuit (CAC), p.337-344)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。全8レジスタについて
/// マニュアル本文の個別レジスタ節(10.2.1〜10.2.8)と突き合わせて確認済み(2026-07-29)。
///
/// CASTRはビット単位でR/R/Wが混在するため、レジスタ全体ではread-writeとして扱う
/// (SCI/ICU/SYSTEM等の既存モジュールと同じ規約)。CACNTBRは本文に
/// 「16-bit read-only register」と明記されている。
/// </summary>
public static class Rx64mCacDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["CACR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CACR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CACR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CAICR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CASTR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CAULVR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CALLVR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CACNTBR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
    };
}