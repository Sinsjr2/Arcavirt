namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 42章(I2C-bus Interface (RIICa)、p.2167-2195の42.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。RIICは2チャネル構成
/// (RIIC0、RIIC2。RIIC1は欠番)で、iodefine.h上ではRIIC0(0x0008 8300)・
/// RIIC2(0x0008 8340)の2インスタンスが同一struct(st_riic)で定義されている。
/// データシートの各レジスタはRIIC0/RIIC2共通の1つの表として記載されており、
/// 代表的なレジスタ(ICCR1/ICCR2/ICSR2/ICBRL等)についてチャネル間でリセット値・
/// アクセス権限に差異がないことを確認済み(2026-07-29)。
///
/// なお、ICFER(I2C-bus Function Enable Register)のb7(FMPE、Fast-Mode Plus Enable)は、
/// 本文の脚注に「The Fast-mode Plus enable bit (FMPE) is only supported by RIIC0.
/// In RIIC2, bit 7 is reserved.」と明記されており、RIIC2ではこのビットの機能が
/// 無効(予約扱い)になる。ただし、ビット説明表・リセット値表ともにRIIC0/RIIC2で
/// 同一の記載(R/W、リセット値0)であり、レジスタ単位のAccess・ResetValue・
/// ResetMaskには差異が無いため、共通の1エントリで表現した。
///
/// iodefine.h上のst_riicは20レジスタ(ICCR1/ICCR2/ICMR1/ICMR2/ICMR3/ICFER/ICSER/
/// ICIER/ICSR1/ICSR2/SARL0/SARU0/SARL1/SARU1/SARL2/SARU2/ICBRL/ICBRH/ICDRT/ICDRR)
/// であり、データシートのI2C-bus Shift Register (ICDRS、42.2.17)はメモリマップド
/// レジスタとして展開されていないため対象外。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとにR/R/(W)混在だが
/// read-writeとして扱った:
/// - ICCR1: SDAI/SCLI(ラインモニタ)はR、他はR/W
/// - ICCR2: BBSY(バス状態フラグ)はR、他はR/W
/// - ICMR3: ACKBR(受信確認応答ビット)はR、他はR/W
/// - ICSR1: AAS0-2/GCA/DID/HOA(検出フラグ、「1確認後の0書込みのみ有効」のR/(W))は
///   混在、予約ビットはR/W
/// - ICSR2: TMOF/AL/START/STOP/NACKF/RDRF/TEND(「1確認後の0書込みのみ有効」の
///   R/(W))は混在、TDRE(送信データエンプティフラグ)はR
///
/// ICDRT(送信データレジスタ)は「The ICDRT register can always be read and written」と
/// 明記され、リセット値も0xFF(全ビット1)の固定値のため、read-writeとして表現した。
/// ICDRR(受信データレジスタ)は「The ICDRR register cannot be written」と明記されて
/// いるため、read-onlyとした(リセット値は0x00)。
/// </summary>
public static class Rx64mRiicDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 42.2.1 I2C-bus Control Register 1 (ICCR1) - SOWP/SCLO/SDAO/SCLI/SDAI=1がリセット値
        ["ICCR1"] = new RegisterDatasheetMetadata(0x1F, 0xFF, "read-write"),
        // 42.2.2 I2C-bus Control Register 2 (ICCR2)
        ["ICCR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 42.2.3 I2C-bus Mode Register 1 (ICMR1) - BCWP(b3)=1がリセット値
        ["ICMR1"] = new RegisterDatasheetMetadata(0x08, 0xFF, "read-write"),
        // 42.2.4 I2C-bus Mode Register 2 (ICMR2) - TMOH(b2)/TMOL(b1)=1がリセット値
        ["ICMR2"] = new RegisterDatasheetMetadata(0x06, 0xFF, "read-write"),
        // 42.2.5 I2C-bus Mode Register 3 (ICMR3)
        ["ICMR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 42.2.6 I2C-bus Function Enable Register (ICFER)
        // SCLE(b6)/NFE(b5)/NACKE(b4)/MALE(b1)=1がリセット値
        ["ICFER"] = new RegisterDatasheetMetadata(0x72, 0xFF, "read-write"),
        // 42.2.7 I2C-bus Status Enable Register (ICSER) - GCAE(b3)/SAR0E(b0)=1がリセット値
        ["ICSER"] = new RegisterDatasheetMetadata(0x09, 0xFF, "read-write"),
        // 42.2.8 I2C-bus Interrupt Enable Register (ICIER)
        ["ICIER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 42.2.9 I2C-bus Status Register 1 (ICSR1)
        ["ICSR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 42.2.10 I2C-bus Status Register 2 (ICSR2)
        ["ICSR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 42.2.11 Slave Address Register Ly (SARLy) (y = 0 to 2)
        ["SARL0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SARL1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SARL2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 42.2.12 Slave Address Register Uy (SARUy) (y = 0 to 2)
        ["SARU0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SARU1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SARU2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 42.2.13 I2C-bus Bit Rate Low-Level Register (ICBRL) - 予約ビット(b7-b5)は「読み出し1」固定
        ["ICBRL"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 42.2.14 I2C-bus Bit Rate High-Level Register (ICBRH) - 予約ビット(b7-b5)は「読み出し1」固定
        ["ICBRH"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 42.2.15 I2C-bus Transmit Data Register (ICDRT) - read-write
        ["ICDRT"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 42.2.16 I2C-bus Receive Data Register (ICDRR) - read-only
        ["ICDRR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
    };
}