namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 17章(Memory-Protection Unit (MPU), p.597-611)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。アドレス・ビット構成は代表的なレジスタ
/// (MPEN/MPESTS/MPDEA/MHITD等)についてマニュアル本文と個別に突き合わせて
/// 確認済み(2026-07-29)。全26レジスタの網羅的な逐一照合は行っていない。
///
/// RSPAGEn/REPAGEn(n = 0 to 7)は、iodefine.h上は番号ごとに個別のstructメンバーとして
/// 展開されているが、データシート上は「RSPAGEn」「REPAGEn」のようにn=0〜7をまとめて
/// 1回だけ汎用的に記載されており、全インスタンスで同一のリセット値・アクセス権限を
/// 持つことを確認済み。
///
/// RSPAGEn/REPAGEnは「値+部分マスク」(CMCR(c0i.3)と同じ経路)で表現している:
/// レジスタ全体としては、下位4ビット(RSPAGEnの予約ビットb3-b0、REPAGEnのUAC[2:0]・V)
/// はリセット後0に固定される一方、上位28ビット(RSPN[27:0]/REPN[27:0])は
/// データシートに"x: Undefined"と明記されている。そのため下位4ビットのみを
/// ResetValue=0・ResetMask=0x0000000Fとして表現し、上位28ビットはマスク外とした。
///
/// MPDEA(データメモリ保護エラーアドレスレジスタ)・MPSA(リージョンサーチアドレス
/// レジスタ)は、全ビットがデータシートに"x: Undefined"と明記されており単一の
/// 固定リセット値が無いため、null(PIDR/c0i.8と同じ仕組み)で表現している。
/// ResetMaskはnull時に意味を持たないため0を設定している。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、ビットごとにR/RWが混在するレジスタ
/// (MHITI/MHITD等、データビットはR・予約ビットはR/W)は"read-write"として扱う。
/// MPDEA(DEA[31:0]の全ビットがRのみ、MHITI/MHITDと異なり予約ビットも含め
/// 全てRのみ)は例外的にread-onlyとした。
/// </summary>
public static class Rx64mMpuDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 17.2.1 Region-n Start Page Number Register (RSPAGEn) (n = 0 to 7)
        // 上位28ビット(RSPN[27:0])はUndefinedのため部分マスクで除外
        ["RSPAGE0"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["RSPAGE1"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["RSPAGE2"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["RSPAGE3"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["RSPAGE4"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["RSPAGE5"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["RSPAGE6"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["RSPAGE7"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),

        // 17.2.2 Region-n End Page Number Register (REPAGEn) (n = 0 to 7)
        // 上位28ビット(REPN[27:0])はUndefinedのため部分マスクで除外
        ["REPAGE0"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["REPAGE1"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["REPAGE2"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["REPAGE3"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["REPAGE4"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["REPAGE5"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["REPAGE6"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),
        ["REPAGE7"] = new RegisterDatasheetMetadata(0x00000000, 0x0000000F, "read-write"),

        // 17.2.3-17.2.12 その他のMPUレジスタ
        ["MPEN"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["MPBAC"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["MPECLR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["MPESTS"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // DEA[31:0]は全ビットUndefinedかつ全ビットR(書込み不可)のため
        // null+read-only
        ["MPDEA"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-only"),
        // SA[31:0]は全ビットUndefinedだが全ビットR/Wのためnull+read-write
        ["MPSA"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
        ["MPOPS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["MPOPI"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["MHITI"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["MHITD"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}