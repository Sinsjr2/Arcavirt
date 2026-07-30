namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 19章(EXDMA Controller (EXDMACa)、p.678-679の19.2.16/19.2.17)から手動で書き写した
/// レジスタごとのリセット値・アクセス権限。EXDMAC0-1のチャネル別レジスタ
/// (EDMSAR等)はstruct st_exdmac0/st_exdmac1が別途あり、void*ポインタ型メンバーの
/// ためパーサー未対応・別issueのスコープ外。st_exdmacはEXDMAC0-1に共通の
/// モジュール開始レジスタ(EDMAST)とクラスタ転送用バッファレジスタ(CLSBR0-7)
/// のみを含む。
///
/// CLSBR0-7(19.2.17)は「Cluster Buffer Register y (y = 0 to 7)」として単一の節・
/// 単一のリセット値表・単一のビット説明表でまとめて記載されており、8レジスタとも
/// 同一のリセット値・アクセス権限を採用した。
/// </summary>
public static class Rx64mExdmacDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 19.2.16 EXDMAC Module Start Register (EDMAST)
        ["EDMAST"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.17 Cluster Buffer Register y (CLSBRy) (y = 0 to 7) - 8レジスタとも同一
        ["CLSBR0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CLSBR1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CLSBR2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CLSBR3"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CLSBR4"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CLSBR5"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CLSBR6"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["CLSBR7"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}