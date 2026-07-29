namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 47章(Serial Sound Interface (SSI)、p.2432-2444の47.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。アドレス・ビット構成は
/// 全7レジスタ(SSICR/SSISR/SSIFCR/SSIFSR/SSIFTDR/SSIFRDR/SSITDMR)についてマニュアル
/// 本文と個別に突き合わせて確認済み(2026-07-29)。SSI0(0008 A500h)・SSI1(0008 A540h)は
/// 同一構成(st_ssi)で、レジスタごとのリセット値・アクセス権限もマニュアル本文で
/// SSI0/SSI1共通の1つの表として記載されており、両インスタンスで同一であることを
/// 確認済み。
///
/// iodefine.h上のst_ssiは実際には7レジスタ(SSICR/SSISR/SSIFCR/SSIFSR/SSIFTDR/
/// SSIFRDR/SSITDMR、SSICR-SSISR間の8バイトはwk0パディング)であり、SSIFTDR/SSIFRDRは
/// unionではなくunsigned long型の素のメンバーとして展開されている。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、以下はビットごとにR/R(/W)混在だが
/// read-writeとして扱った:
/// - SSISR: IDST/RSWNO/RCHNO/TSWNO/TCHNO(状態フラグ)はR、IIRQはR、ROIRQ/RUIRQ/
///   TOIRQ/TUIRQ(割り込みステータスフラグ)は「1確認後の0書込みのみ有効」のR/(W)、
///   予約ビットはR/W
/// - SSIFSR: RDF/TDE(データフル/エンプティフラグ)は「1確認後の0書込みのみ有効」の
///   R/(W)、RDC/TDC(データ個数表示)はR、予約ビットはR/W
///
/// SSIFCRの予約ビット(b15-b8, b30-b17)は個別のビット説明表で「These bits are read
/// as undefined」と明記されている。この章(47章)のValue after reset行はSSIFTDR/
/// SSIFRDRのような不定レジスタでも"x"ではなく"—"で表記しており、MPU(c0i.20)や
/// SRC(c0i.26)のような"x: Undefined"の凡例自体を使っていないため、SSIFCRの
/// Value after reset行に数値"0"が書かれていることは、この2つの予約ビット範囲が
/// 実際に0固定であることの根拠にならないと判断した。ビット説明表の記載に従い、
/// MPU(c0i.20)のRSPAGEn/REPAGEnと同じ「値+部分マスク」で、定義済みビット
/// (b7-b0: RFRST/TFRST/RIE/TIE/RTRG[1:0]/TTRG[1:0]、b16: SSIRST、
/// b31: AUCKE)のみをResetMaskに含め、undefinedと明記されたb15-b8・b30-b17は
/// マスク対象外とした(ResetValue=0x00000000、ResetMask=0x800100FF)。
///
/// SSIFTDR(送信FIFOデータレジスタ)・SSIFRDR(受信FIFOデータレジスタ)は、本文にそれぞれ
/// 「write-only FIFO register」「read-only FIFO register」と明記され、"Value after
/// reset"の行も全ビット"—"(不定値、SCIFA(c0i.10)のFTDR/FRDRと同じ表記)のため、
/// null(ResetValue)で表現した。
/// </summary>
public static class Rx64mSsiDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 47.2.1 Control Register (SSICR)
        ["SSICR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 47.2.2 Status Register (SSISR) - IDST(b0)/RSWNO(b1)/TSWNO(b4)/IIRQ(b25)=1がリセット値
        ["SSISR"] = new RegisterDatasheetMetadata(0x02000013, 0xFFFFFFFF, "read-write"),
        // 47.2.3 FIFO Control Register (SSIFCR)
        // b15-b8・b30-b17はUndefinedのため部分マスクで除外
        ["SSIFCR"] = new RegisterDatasheetMetadata(0x00000000, 0x800100FF, "read-write"),
        // 47.2.4 FIFO Status Register (SSIFSR) - TDE(b16)=1がリセット値
        ["SSIFSR"] = new RegisterDatasheetMetadata(0x00010000, 0xFFFFFFFF, "read-write"),
        // 47.2.5 Transmit FIFO Data Register (SSIFTDR) - write-only、リセット値は不定
        ["SSIFTDR"] = new RegisterDatasheetMetadata(null, 0x00000000, "write-only"),
        // 47.2.6 Receive FIFO Data Register (SSIFRDR) - read-only、リセット値は不定
        ["SSIFRDR"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-only"),
        // 47.2.7 TDM Mode Register (SSITDMR)
        ["SSITDMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}