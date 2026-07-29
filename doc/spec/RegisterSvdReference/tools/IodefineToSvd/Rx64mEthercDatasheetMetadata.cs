namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 35章(Ethernet Controller (ETHERC)、p.1523-1542の35.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。ETHERCは2チャネル構成
/// (ETHERC0 @ 000C 0100h、ETHERC1 @ 000C 0300h)であり、iodefine.h上でも
/// ETHERC0・ETHERC1の2インスタンスが同一struct(st_etherc)で定義されている。
/// データシートは全レジスタについてETHERC0/ETHERC1双方のアドレスを1つの表に
/// 併記しており(例: "ETHERC0.ECMR 000C 0100h, ETHERC1.ECMR 000C 0300h")、
/// 両アドレスの差(0x200)がiodefine.h上のオフセット差と一致することを代表的な
/// レジスタで確認した(2026-07-29)。35.2.1(ECMR)から35.2.26(MAFCR)まで26節が
/// 存在し、iodefine.h上のst_ethercが持つ26レジスタとちょうど一致する。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとに異なるR/W区分が
/// 混在するがread-writeとして扱った:
/// - ECSR: 各フラグビットは「R/W *1(書き込みで1をクリア)」、予約ビットはR/W
/// - CECLKCTRLに相当するものはETHERCには無いが、同様の考え方でCECLKCTRL等
///   (MMCIF側)も参照
///
/// 一部ビットのみ未定義のケースは「値+部分マスク」で表現した:
/// - PIR: b3(MDI)がリセット値表で"x: Undefined"と明記されているため、
///   ResetValue=0x00000000、ResetMask=0xFFFFFFF7(b3を除外)。他ビットはR/W。
/// - PSR: b0(LMON)がリセット値表で"x: Undefined"と明記されているため、
///   ResetValue=0x00000000、ResetMask=0xFFFFFFFE(b0を除外)。全ビットRのため
///   Accessはread-onlyとした。
/// - MPR: リセット値表の数値自体は"0"だが、ビット説明表でMP[15:0]について
///   「The read value is undefined.」と明記されている(GPT/SSIで実際に発生した
///   「リセット値表の数値を鵜呑みにする」罠と同種のケース)。そのため
///   ResetValue=0x00000000、ResetMask=0xFFFF0000(MP[15:0]を除外)とした。
///
/// RFCF・TPAUSECRはビット説明表でRPAUSE[7:0]/TXP[7:0]・予約ビットともRのため
/// read-onlyとした。
///
/// TROCR・CDCR・LCCR・CNDCR・CEFCR・FRECR・TSFRCR・TLFRCR・RFCR・MAFCRは
/// ビット単位のR/W表が無いカウンタレジスタだが、本文に「The counter value
/// becomes 0 by writing any value to the ... register.」と明記されており、
/// 読み出し(カウント値)・書き込み(クリア)の両方が可能なためread-writeとした
/// (ドキュメント上に明示のR/W表が無い推測であることに注意)。MAHR・MALRも
/// 同様にビット単位R/W表は無いが、MAC アドレス設定用のR/Wレジスタとして
/// read-writeとした。
/// </summary>
public static class Rx64mEthercDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 35.2.1 ETHERC Mode Register (ECMR)
        ["ECMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.2 Receive Frame Maximum Length Register (RFLR)
        ["RFLR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.3 ETHERC Status Register (ECSR) - フラグはR/W*1(書き込みで1をクリア)、予約ビットはR/W
        ["ECSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.4 ETHERC Interrupt Enable Register (ECSIPR)
        ["ECSIPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.5 PHY Interface Register (PIR) - b3(MDI)がUndefinedのため部分マスクで除外
        ["PIR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFF7, "read-write"),
        // 35.2.6 PHY Status Register (PSR) - read-only、b0(LMON)がUndefinedのため部分マスクで除外
        ["PSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFE, "read-only"),
        // 35.2.7 Random Number Generation Counter Limit Setting Register (RDMLR)
        ["RDMLR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.8 Interpacket Gap Register (IPGR) - IPG[4:0]=14h(96 bit time)が初期値
        ["IPGR"] = new RegisterDatasheetMetadata(0x00000014, 0xFFFFFFFF, "read-write"),
        // 35.2.9 Automatic PAUSE Frame Register (APR)
        ["APR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.10 Manual PAUSE Frame Register (MPR) - MP[15:0]は「読み出し値はUndefined」と
        // 明記されているため部分マスクで除外(リセット値表の数値は0だが鵜呑みにしない)
        ["MPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFF0000, "read-write"),
        // 35.2.11 Received PAUSE Frame Counter (RFCF) - read-only
        ["RFCF"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 35.2.12 PAUSE Frame Retransmit Count Setting Register (TPAUSER)
        ["TPAUSER"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.13 PAUSE Frame Retransmit Counter (TPAUSECR) - read-only
        ["TPAUSECR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 35.2.14 Broadcast Frame Receive Count Setting Register (BCFRR)
        ["BCFRR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.15 MAC Address Upper Bit Register (MAHR) - ビット単位R/W表は無いが設定用レジスタ
        ["MAHR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.16 MAC Address Lower Bit Register (MALR)
        ["MALR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.17 Transmit Retry Over Counter Register (TROCR) - 書き込みで0クリア可能なカウンタ
        ["TROCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.18 Late Collision Detect Counter Register (CDCR)
        ["CDCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.19 Lost Carrier Counter Register (LCCR)
        ["LCCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.20 Carrier Not Detect Counter Register (CNDCR)
        ["CNDCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.21 CRC Error Frame Receive Counter Register (CEFCR)
        ["CEFCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.22 Frame Receive Error Counter Register (FRECR)
        ["FRECR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.23 Too-Short Frame Receive Counter Register (TSFRCR)
        ["TSFRCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.24 Too-Long Frame Receive Counter Register (TLFRCR)
        ["TLFRCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.25 Received Alignment Error Frame Counter Register (RFCR)
        ["RFCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 35.2.26 Multicast Address Frame Receive Counter Register (MAFCR)
        ["MAFCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}