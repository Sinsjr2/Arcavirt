namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 20章(Data Transfer Controller (DTCa)、p.730-734の20.2.7-20.2.11)から手動で
/// 書き写した、st_dtc(インスタンスDTC)のレジスタごとのリセット値・アクセス権限。
/// MRA/MRB/SAR/DAR/CRA/CRB(20.2.1-20.2.6節)は本文に「CPUから直接アクセスできない
/// DTC内部レジスタで、RAM上の転送情報として保持される」旨が明記されており、
/// iodefine.hのst_dtcにもメモリマップドレジスタとして存在しないため対象外とした。
///
/// - DTCCR: b3(予約)はリセット時1固定・書込み値も1固定と明記されているため、
///   ResetValueに反映した(0x08)。
/// - DTCVBR: void*ポインタ型メンバー(32ビットのアドレス設定レジスタ)。本文には
///   ビット単位のBit/Symbol表が無いが、「上位4ビットは書込み不可でb27の値を反映する」
///   「下位10ビットは予約でリセット・書込みとも0固定」という記述があり、他の
///   ポインタ型レジスタ(DMSAR等)と同様に読み書き可能なアドレス設定レジスタと
///   判断してread-writeとした。
/// - DTCSTS: Bit/Symbol表の全ビット(VECN[7:0]・予約・ACT)がRのみでread-writeな
///   ビットが無いため、read-onlyとした。
/// </summary>
public static class Rx64mDtcDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 20.2.7 DTC Control Register (DTCCR) - b3(予約)はリセット時1固定
        ["DTCCR"] = new RegisterDatasheetMetadata(0x08, 0xFF, "read-write"),
        // 20.2.8 DTC Vector Base Register (DTCVBR) - 上記コメント参照
        ["DTCVBR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 20.2.9 DTC Address Mode Register (DTCADMOD)
        ["DTCADMOD"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 20.2.10 DTC Module Start Register (DTCST)
        ["DTCST"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 20.2.11 DTC Status Register (DTCSTS) - 全ビットR、上記コメント参照
        ["DTCSTS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
    };
}