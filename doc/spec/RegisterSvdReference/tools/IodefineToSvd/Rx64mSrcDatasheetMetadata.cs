namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 48章(Sample Rate Converter (SRC)、p.2463-2472の48.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。アドレス・ビット構成は
/// 全7レジスタ(SRCFCTR/SRCID/SRCOD/SRCIDCTRL/SRCODCTRL/SRCCTRL/SRCSTAT)についてマニュアル
/// 本文と個別に突き合わせて確認済み(2026-07-29)。
///
/// iodefine.h上のst_srcは実際には7レジスタで、うちSRCFCTR[5552]は配列レジスタ
/// (フィルタ係数テーブルRAM、ICU(c0i.11)のIR[256]と同じ配列レジスタパターン)。
/// 辞書のキーはインデックス無しの"SRCFCTR"(RegisterLayoutBuilderが配列を1エントリに
/// 集約するため)。
///
/// SRCFCTRnは、下位22ビット(SRCFCOE[21:0])がデータシートに"x: Undefined"と明記されて
/// いる一方、上位10ビット(予約ビット、b31-b22)はリセット後0固定と明記されているため、
/// MPU(c0i.20)のRSPAGEn/REPAGEnと同じ「値+部分マスク」で上位10ビットのみを
/// マスク対象とした(ResetValue=0x00000000、ResetMask=0xFFC00000)。
///
/// SRCOD(出力データレジスタ)は本文に「32-bit read-only register」と明記されているため
/// read-onlyとした。SRCID(入力データレジスタ)は「32-bit readable/writable register」と
/// 明記されているためread-writeとした。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、SRCSTATはOINT/IINT/OVF/UDF/CEF
/// (「1確認後の0書込みのみ有効」のR/(W))・FLF/IFDN/OFDN(R)・予約ビット(R/W)が
/// 混在するがread-writeとして扱った。
///
/// SRCSTATのリセット値はIINT(b1)=1、他は0のため0x0002とした。
/// </summary>
public static class Rx64mSrcDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 48.2.7 Filter Coefficient Table n (SRCFCTRn) (n = 0 to 5551)
        // 下位22ビット(SRCFCOE[21:0])はUndefinedのため部分マスクで除外
        ["SRCFCTR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFC00000, "read-write"),
        // 48.2.1 Input Data Register (SRCID) - 32-bit readable/writable
        ["SRCID"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 48.2.2 Output Data Register (SRCOD) - 32-bit read-only
        ["SRCOD"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 48.2.3 Input Data Control Register (SRCIDCTRL)
        ["SRCIDCTRL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 48.2.4 Output Data Control Register (SRCODCTRL)
        ["SRCODCTRL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 48.2.5 Control Register (SRCCTRL)
        ["SRCCTRL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 48.2.6 Status Register (SRCSTAT) - IINT(b1)=1がリセット値
        ["SRCSTAT"] = new RegisterDatasheetMetadata(0x0002, 0xFFFF, "read-write"),
    };
}