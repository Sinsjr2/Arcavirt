namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 51章(Parallel Data Capture Unit (PDC)、p.2577-2587の51.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。アドレス・ビット構成は
/// 全7レジスタ(PCCR0/PCCR1/PCSR/PCMONR/PCDR/VCR/HCR)についてマニュアル本文と
/// 個別に突き合わせて確認済み(2026-07-29)。
///
/// iodefine.h上のst_pdcは実際には7レジスタ(PCCR0/PCCR1/PCSR/PCMONR/PCDR/VCR/HCR)で
/// あり、パディングメンバーは無い。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、以下はビットごとにR/R(/W)混在だが
/// read-writeとして扱った:
/// - PCCR0: PRST(b3)は「1書込みのみ有効(自動的に0へ戻る)」のR/(W)、他はR/W
/// - PCSR: FEF/OVRF/UDRF/VERF/HERF(ステータスフラグ)は「1確認後の0書込みのみ有効」の
///   R/(W)、FBSY/FEMPFはR(読み出し専用)、予約ビットはR
///
/// PCMONRは全ビットがR(VSYNC/HSYNC信号状態の読み出し専用、予約ビットもR)のため、
/// 例外的にread-onlyとした。
///
/// PCDRは、本文に個別のビット表が無く、本文中に「captured data are read from this
/// register」等、読み出しに関する記述しかなく書き込みに関する記述が一切無い
/// (22段FIFOのデータレジスタ)。SRCOD(c0i.26)の「32-bit read-only register」や
/// SSIFRDR(c0i.25)の「read-only FIFO register」のように"read-only"と明記する文は
/// 無いため、read-onlyという判断は書き込み経路の記述が存在しないことからの推定で
/// あり、本文に明記された事実ではない。リセット値は、この章(51章)がSSI(47章)の
/// ような"—"/"x"表記を一切使っておらず"Value after reset"行が全ビット明示的な
/// 数値"0"であるため、0x00000000とした。
///
/// PCSRのリセット値はFEMPF(b1、FIFO Empty Flag)が1、他は0のため0x00000002とした
/// (リセット直後はFIFOが空であることに対応)。
/// </summary>
public static class Rx64mPdcDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 51.2.1 PDC Control Register 0 (PCCR0)
        ["PCCR0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 51.2.2 PDC Control Register 1 (PCCR1)
        ["PCCR1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 51.2.3 PDC Status Register (PCSR) - FEMPF(b1)=1がリセット値
        ["PCSR"] = new RegisterDatasheetMetadata(0x00000002, 0xFFFFFFFF, "read-write"),
        // 51.2.4 PDC Pin Monitor Register (PCMONR) - 全ビットRのみ
        ["PCMONR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 51.2.5 PDC Receive Data Register (PCDR) - 読み出し専用のFIFOデータレジスタ
        ["PCDR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 51.2.6 Vertical Capture Register (VCR)
        ["VCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 51.2.7 Horizontal Capture Register (HCR)
        ["HCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}