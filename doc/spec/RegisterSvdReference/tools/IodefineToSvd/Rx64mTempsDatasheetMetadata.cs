namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 59章(Temperature Sensor (TEMPS), p.2736-2737)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。データシート59.2節にはTSCR/TSCDRの2レジスタが
/// 記載されているが、iodefine.hのst_temps構造体にはTSCRのみが含まれ、TSCDR
/// (工場出荷時較正データ、フラッシュ領域0x7FB17Cに配置)は含まれないため対象外
/// (2026-07-29確認済み)。
/// </summary>
public static class Rx64mTempsDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["TSCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}