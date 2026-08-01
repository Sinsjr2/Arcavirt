namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 37章(DMA Controller for the Ethernet Controller (EDMACa)、p.1695-1721の
/// 37.2.1-37.2.24)から手動で書き写した、st_edmac(インスタンスEDMAC0/EDMAC1)の
/// レジスタごとのリセット値・アクセス権限。この章にはEDMACn向けの節とPTPEDMAC向け
/// の節が混在しており(例: 37.2.6 EDMACn.EESR / 37.2.7 PTPEDMAC.EESR)、PTPEDMAC向け
/// の節はArcavirt-c0i.38で別途対応済みのため対象外とした。TDLAR・RDLAR・RBWAR・
/// RDFAR・TBRAR・TDFARはvoid*ポインタ型メンバー(32ビットのアドレス関連レジスタ)。
///
/// 抽出スクリプトの「たたき台」から修正・補完した箇所:
/// - TDLAR・RDLAR(37.2.4/37.2.5節)はBit/Symbol表を持つが「アドレスを設定する」旨の
///   記述のみでread-write。
/// - RBWAR・RDFAR・TBRAR・TDFAR(37.2.21-37.2.24節)はBit/Symbol表を持たず
///   access_candidate=nullだったが、各節の本文に「The xxx register is read only.
///   Do not write to this register.」と明記されているため、read-onlyとした。
/// </summary>
public static class Rx64mEdmacDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 37.2.1 EDMAC Mode Register (EDMR)
        ["EDMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.2 EDMAC Transmit Request Register (EDTRR)
        ["EDTRR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.3 EDMAC Receive Request Register (EDRRR)
        ["EDRRR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.4 Transmit Descriptor List Start Address Register (TDLAR)
        ["TDLAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.5 Receive Descriptor List Start Address Register (RDLAR)
        ["RDLAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.6 ETHERC/EDMAC Status Register (EDMACn.EESR) - PTPEDMAC.EESR(37.2.7)は対象外
        ["EESR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.8 ETHERC/EDMAC Status Interrupt Enable Register (EDMACn.EESIPR) - PTPEDMAC.EESIPR(37.2.9)は対象外
        ["EESIPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.10 ETHERC/EDMAC Transmit/Receive Status Copy Enable Register (EDMACn.TRSCER)
        ["TRSCER"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.11 Missed-Frame Counter Register (RMFCR)
        ["RMFCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.12 Transmit FIFO Threshold Register (TFTR)
        ["TFTR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.13 FIFO Depth Register (FDR)
        ["FDR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.14 Receive Method Control Register (RMCR)
        ["RMCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.15 Transmit FIFO Underflow Counter (TFUCR)
        ["TFUCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.16 Receive FIFO Overflow Counter (RFOCR)
        ["RFOCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.17 Independent Output Signal Setting Register (IOSR)
        ["IOSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.18 Flow Control Start FIFO Threshold Setting Register (FCFTR) - RFDO/RFFOの初期値111b
        ["FCFTR"] = new RegisterDatasheetMetadata(0x00070007, 0xFFFFFFFF, "read-write"),
        // 37.2.19 Receive Data Padding Insert Register (RPADIR)
        ["RPADIR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.20 Transmit Interrupt Setting Register (TRIMD)
        ["TRIMD"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.21 Receive Buffer Write Address Register (RBWAR) - 上記コメント参照(たたき台補完)
        ["RBWAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 37.2.22 Receive Descriptor Fetch Address Register (RDFAR) - 上記コメント参照(たたき台補完)
        ["RDFAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 37.2.23 Transmit Buffer Read Address Register (TBRAR) - 上記コメント参照(たたき台補完)
        ["TBRAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 37.2.24 Transmit Descriptor Fetch Address Register (TDFAR) - 上記コメント参照(たたき台補完)
        ["TDFAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
    };
}