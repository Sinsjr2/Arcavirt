namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 61章「RAM」内のECC RAM関連の節(61.2.1-61.2.9、p.2751-2755)から手動で書き写した
/// レジスタごとのリセット値・アクセス権限。アドレス・ビット構成は代表的なレジスタ
/// (ECCRAMMODE/ECCRAMPRCR/ECCRAM2ECAD等)についてマニュアル本文と個別に
/// 突き合わせて確認済み(2026-07-29)。全9レジスタの網羅的な逐一照合は行っていない。
///
/// ECCRAMは、SYSTEM(c0i.7)やMPU(c0i.20)と異なり、9レジスタすべてについて
/// データシートに単一の固定リセット値が明記されているため、null表現・部分マスク
/// 表現のいずれも使用していない。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、ビットごとにR/R(/W)が混在する
/// レジスタ(ECCRAM2STS/ECCRAM1STS等)は"read-write"として扱う。ECCRAM2ECAD/
/// ECCRAM1ECADは、データビット・予約ビットを含め全ビットがR(読み出し専用)のため
/// 例外的にread-onlyとした。
/// </summary>
public static class Rx64mEccramDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 61.2.1 ECCRAM Operating Mode Control Register (ECCRAMMODE)
        ["ECCRAMMODE"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 61.2.2 ECCRAM 2-Bit Error Status Register (ECCRAM2STS)
        // ECC2ERRはR/(W)(0のみ書込み可)、予約ビットはR/Wのためread-write
        ["ECCRAM2STS"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 61.2.3 ECCRAM 1-Bit Error Information Update Enable Register (ECCRAM1STSEN)
        ["ECCRAM1STSEN"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 61.2.4 ECCRAM 1-Bit Error Status Register (ECCRAM1STS)
        // ECC1ERRはR/(W)(0のみ書込み可)、予約ビットはR/Wのためread-write
        ["ECCRAM1STS"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 61.2.5 ECCRAM Protection Register (ECCRAMPRCR)
        ["ECCRAMPRCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 61.2.6 ECCRAM 2-Bit Error Address Capture Register (ECCRAM2ECAD)
        // 全ビットR(読み出し専用)のためread-only。リセット値は
        // b31-b24=0・b23-b16=1・b15=1・b14-b0(ECC2EAD含む)=0
        ["ECCRAM2ECAD"] = new RegisterDatasheetMetadata(0x00FF8000, 0xFFFFFFFF, "read-only"),
        // 61.2.7 ECCRAM 1-Bit Error Address Capture Register (ECCRAM1ECAD)
        // ECCRAM2ECADと同一のビット構成・リセット値
        ["ECCRAM1ECAD"] = new RegisterDatasheetMetadata(0x00FF8000, 0xFFFFFFFF, "read-only"),
        // 61.2.8 ECCRAM Protection Register 2 (ECCRAMPRCR2)
        ["ECCRAMPRCR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 61.2.9 ECCRAM Test Control Register (ECCRAMETST)
        ["ECCRAMETST"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}