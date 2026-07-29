namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 28章(Programmable Pulse Generator (PPG)、p.1348-1358の28.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。アドレス・ビット構成は
/// 代表的なレジスタ(PTRSLR/PCR/PMR/NDERH/NDRH等)についてマニュアル本文と個別に
/// 突き合わせて確認済み(2026-07-29)。
///
/// iodefine.h上はPPG0(st_ppg0、10レジスタ)・PPG1(st_ppg1、PTRSLRを含む11レジスタ)の
/// 2つの別structとして展開されているが、データシート上はPTRSLR以外の全レジスタ
/// (PCR/PMR/NDERH/NDERL/PODRH/PODRL/NDRH/NDRL/NDRH2/NDRL2)がPPGn(n = 0, 1)という
/// 1つの汎用記載でまとめられている。PPG0とPPG1でビットが表す出力グループ・ピンの
/// 対応関係は異なる(PPG0はグループ0-3/PO0-15、PPG1はグループ4-7/PO16-31)が、
/// アドレスオフセット・リセット値・アクセス権限は全レジスタでPPG0/PPG1同一であることを
/// 本文の"Value after reset"行で確認済み。そのためRegisterDatasheetMetadataの辞書
/// キー(レジスタ名のみ)がPPG0/PPG1間で衝突する問題は無い。
///
/// PTRSLR(PPGトリガ選択レジスタ)はPPG1にのみ存在し、PPG0側に対応するレジスタは無い。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、NDRH/NDRL/NDRH2/NDRL2は、出力グループの
/// トリガ設定によって一部ビットが「読み出し値はFFh固定・書き込み禁止」になる旨の記載が
/// あるが、これは実行時の動作条件による見かけ上の制約であり、リセット直後の値・
/// レジスタ定義上のアクセス権限自体はビット表の通りR/Wで統一されているため、
/// read-writeとして扱った。
/// </summary>
public static class Rx64mPpgDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 28.2.1 PPG Trigger Select Register (PTRSLR) - PPG1のみ、PTRSL(b0)=1がリセット値
        ["PTRSLR"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        // 28.2.5 PPG Output Control Register (PCR) - PPG0/PPG1共通、全ビット1がリセット値
        ["PCR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 28.2.6 PPG Output Mode Register (PMR) - PPG0/PPG1共通
        // G0INV-G3INV(b4-b7)=1、G0NOV-G3NOV(b0-b3)=0がリセット値
        ["PMR"] = new RegisterDatasheetMetadata(0xF0, 0xFF, "read-write"),
        // 28.2.2 Next Data Enable Register H/L (NDERH/NDERL) - PPG0/PPG1共通
        ["NDERH"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NDERL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 28.2.3 Output Data Register H/L (PODRH/PODRL) - PPG0/PPG1共通
        ["PODRH"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["PODRL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 28.2.4 Next Data Register H/L/H2/L2 (NDRH/NDRL/NDRH2/NDRL2) - PPG0/PPG1共通
        ["NDRH"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NDRL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NDRH2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NDRL2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}