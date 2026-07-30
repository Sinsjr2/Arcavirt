namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 44章(Serial Peripheral Interface (RSPIa)、p.2296-2376の44.2 Register Descriptions)
/// から手動で書き写したレジスタごとのリセット値・アクセス権限。RSPIは1チャネル構成
/// (RSPI0)であり、iodefine.h上もst_rspiの1インスタンス(RSPI0)のみが定義されている。
///
/// SPDR(44.2.5)は、データシート上は32bitアクセス(SPDR)と16bitアクセス(SPDR.H)の
/// 両方の表記があるが、iodefine.h側は32bit union(SPDR)のみでSPDR.Hに対応する
/// 別名メンバーは存在しないため、32bit版の値のみを採用した(いずれもリセット値は
/// 全ビット0で一致)。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとにR/R(/W)混在だが
/// read-writeとして扱った:
/// - SPSR: OVRF/MODF/PERFはR/(W)(1を読み出した後に0を書き込むことでのみクリア
///   可能)、IDLNFはR、SPTEF/SPRFは"R*2"(書き込みは1のみ有効)。SPTEF(b5)は
///   リセット値表で1固定であることを確認済み。
///
/// SPSSR(44.2.7)は全ビットR(参照専用のステータスレジスタ、SPCMDmのうちどれが
/// 参照されているかを示す)のためread-onlyとした。
/// </summary>
public static class Rx64mRspiDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 44.2.1 RSPI Control Register (SPCR)
        ["SPCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.2 RSPI Slave Select Polarity Register (SSLP)
        ["SSLP"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.3 RSPI Pin Control Register (SPPCR)
        ["SPPCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.4 RSPI Status Register (SPSR) - SPTEF(b5)=1がリセット値
        ["SPSR"] = new RegisterDatasheetMetadata(0x20, 0xFF, "read-write"),
        // 44.2.5 RSPI Data Register (SPDR) - 32bit版の値を採用(上記コメント参照)
        ["SPDR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 44.2.6 RSPI Sequence Control Register (SPSCR)
        ["SPSCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.7 RSPI Sequence Status Register (SPSSR) - read-only
        ["SPSSR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        // 44.2.8 RSPI Bit Rate Register (SPBR)
        ["SPBR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        // 44.2.9 RSPI Data Control Register (SPDCR)
        ["SPDCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.10 RSPI Clock Delay Register (SPCKD)
        ["SPCKD"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.11 RSPI Slave Select Negation Delay Register (SSLND)
        ["SSLND"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.12 RSPI Next-Access Delay Register (SPND)
        ["SPND"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.13 RSPI Control Register 2 (SPCR2)
        ["SPCR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 44.2.14 RSPI Command Register m (SPCMDm) (m = 0 to 7)
        // 8レジスタとも同一のリセット値・アクセス権限であることを本文で確認済み。
        ["SPCMD0"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
        ["SPCMD1"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
        ["SPCMD2"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
        ["SPCMD3"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
        ["SPCMD4"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
        ["SPCMD5"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
        ["SPCMD6"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
        ["SPCMD7"] = new RegisterDatasheetMetadata(0x070D, 0xFFFF, "read-write"),
    };
}