namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 37章(DMA Controller for the Ethernet Controller (EDMACa)、p.1695-1721の
/// 37.2 Register Descriptions)から手動で書き写したレジスタごとのリセット値・
/// アクセス権限。iodefine.h側の"PTPEDMAC"という名前はデータシート側の"EDMAC"に
/// 対応する。このMCUにはEDMAC0/EDMAC1(ETHERC0/ETHERC1用)とPTPEDMAC(EPTPC用)の
/// 3チャネルがあり、本文中でチャネル番号は"n (n = 0, 1)"と表記される
/// (37.1節)。PTPEDMACはEDMAC0/EDMAC1と共通のレジスタセットを持つが、以下の
/// 2レジスタのみ例外で、st_ptpedmacに存在するにもかかわらずEDMACn(n = 0, 1)側の
/// アドレスのみが本文に明記されており、PTPEDMAC側のアドレス・ビット説明表が
/// 存在しない:
/// - TRSCER(37.2.10 "ETHERC/EDMAC Transmit/Receive Status Copy Enable Register
///   (EDMACn.TRSCER)"): RRFCE/RMAFCEビットはEDMACn.EESRのRRF/RMAFフラグに対応する
///   が、PTPEDMAC.EESR(37.2.7)にはRRF/RMAFに相当するビットが存在しない
///   (PVER/RPORT/MACEに置き換わっている)ため、PTPEDMACでは意味を持たない。
/// - IOSR(37.2.17 "Independent Output Signal Setting Register"): ELBビットは
///   ETHERC外部出力ピン(ETn_EXOUT)向けの設定であり、PTPEDMACには対応する
///   外部ピンがない。
/// この2レジスタについては、EDMACn側の値を類推適用せず、意図的にこの
/// Registers辞書から除外している。SvdDocumentBuilderは、datasheetMetadataの
/// 辞書に存在しないレジスタについてはaccess/resetValue要素を一切出力しない
/// (辞書にキーがあってResetValue=nullの場合はaccess要素のみ出力される
/// PORTモジュールのPIDRとは異なる扱い)。CMSIS-SVD.xsd上はregisterType内の
/// access要素がminOccurs="0"のため許容されるが、このプロジェクトで辞書が
/// struct上のレジスタ数より意図的に少ないのはこのモジュールが初めてである。
/// 生成時にこの判断を上書きしたい場合は、この2レジスタの扱いを個別に検討すること。
///
/// EESR(37.2.7)・EESIPR(37.2.9)はPTPEDMAC固有の節として本文に個別記載されており
/// (EDMACn.EESR/EESIPRの37.2.6/37.2.8とはビット構成が異なる。例: PTPEDMAC.EESRは
/// TYPE[3:0]/PVER/RPORT/MACEを持つがEDMACn.EESRはCERF/PRE/RTSF/RTLF/RRF/RMAF/
/// TRO/CD/DLC/CND/ECIを持つ)、この節の値を採用した。
/// </summary>
public static class Rx64mPtpedmacDatasheetMetadata {
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
        // 37.2.7 PTP/EDMAC Status Register (PTPEDMAC.EESR) - 上記コメント参照
        ["EESR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.9 PTP/EDMAC Status Interrupt Enable Register (PTPEDMAC.EESIPR) - 上記コメント参照
        ["EESIPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.10 TRSCERはPTPEDMACでは非対応(上記コメント参照、意図的に未登録)
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
        // 37.2.17 IOSRはPTPEDMACでは非対応(上記コメント参照、意図的に未登録)
        // 37.2.18 Flow Control Start FIFO Threshold Setting Register (FCFTR)
        ["FCFTR"] = new RegisterDatasheetMetadata(0x00070007, 0xFFFFFFFF, "read-write"),
        // 37.2.19 Receive Data Padding Insert Register (RPADIR)
        ["RPADIR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.20 Transmit Interrupt Setting Register (TRIMD)
        ["TRIMD"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 37.2.21 Receive Buffer Write Address Register (RBWAR) - 本文に"read only"と明記
        ["RBWAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 37.2.22 Receive Descriptor Fetch Address Register (RDFAR) - 本文に"read only"と明記
        ["RDFAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 37.2.23 Transmit Buffer Read Address Register (TBRAR) - 本文に"read only"と明記
        ["TBRAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 37.2.24 Transmit Descriptor Fetch Address Register (TDFAR) - 本文に"read only"と明記
        ["TDFAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
    };
}