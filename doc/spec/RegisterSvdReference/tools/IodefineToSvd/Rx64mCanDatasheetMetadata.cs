namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 43章(CAN Module (CAN)、43.2 Register Descriptions、43.2.1〜43.2.24、
/// p.2244-2277)から手動で書き写したレジスタごとのリセット値・アクセス権限。
/// st_canの全メンバー(MB[32]・MKR[8]・FIDCR0/FIDCR1・MKIVLR・MIER・MCTL[32]・
/// CTLR・STR・BCR・RFCR・RFPCR・TFCR・TFPCR・EIER・EIFR・RECR・TECR・ECSR・
/// CSSR・MSSR・MSMR・TSR・AFSR・TCR)がこのマニュアルの43.2.1〜43.2.24節と
/// 1対1で一致することを確認済み。
///
/// MB[32](43.2.6)はCMSIS-SVDの&lt;cluster dim="32"&gt;として表現するため
/// (RegisterLayoutBuilder/SvdDocumentBuilder参照)、キーはMB自体ではなく
/// クラスター内の子レジスタ名(ID/DLC/DATA/TS)。iodefine.hのDATA[8](1byte×8)は
/// 既存のdimレジスタ機構(ICUのIR[256]と同じ仕組み)でDATA%sとして表現されるため、
/// DATA0〜7を個別キーに分ける必要はない。
///
/// MB内のID/DLC/DATA/TSは、マニュアル本文で「The value after reset of the CANi
/// mailbox is undefined.」と明記されており、ビット単位の表でも全ビットが
/// "x: Undefined"かつR/Wであるため、ResetValue: nullとした。
/// MKR/FIDCR0/FIDCR1/MKIVLR/MIER/CSSR/AFSRも同様に、本文の"Value after reset"表で
/// 該当ビット全体が"x: Undefined"であるため、ResetValue: nullとした
/// (RFPCR/TFPCRは全ビット未定義かつ「R/W」ではなく「W」のみ(CPU側ポインタを
/// インクリメントする書き込み専用トリガレジスタ)のため、Accessをwrite-onlyとした)。
///
/// MSSR(43.2.15)は、MBNST[4:0]/SESTの実体ビットは"R"のみだが、予約ビット(b6,b5)が
/// 明示的に"R/W"(読み出しは0固定、書き込み値は0とすべき)と記載されているため、
/// 規約通り「ビット単位でR・R/Wが混在する場合はレジスタ全体をread-writeとして
/// 扱う」ルールに従いread-writeとした(抽出スクリプトの候補はread-onlyだったが、
/// 本文の予約ビット記載を優先して訂正)。
/// STR(43.2.13)は予約ビット(b15)を含む全ビットが"R"のみで、MSSRと異なり
/// R/Wビットが存在しないため、素直にread-onlyとした。
/// RECR/TECR/TSR(43.2.20/43.2.21/43.2.23)も同様に全ビット"R"のみのため
/// read-onlyとした。
/// </summary>
public static class Rx64mCanDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers { get; } =
        new Dictionary<string, RegisterDatasheetMetadata> {
            // 43.2.1 Control Register (CTLR)
            ["CTLR"] = new RegisterDatasheetMetadata(0x0500, 0xFFFF, "read-write"),
            // 43.2.2 Bit Configuration Register (BCR)
            ["BCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
            // 43.2.3 Mask Register k (MKRk) (k = 0 to 7) - リセット後未定義
            ["MKR"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
            // 43.2.4 FIFO Received ID Compare Registers 0 and 1 (FIDCR0 and FIDCR1) - リセット後未定義
            ["FIDCR0"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
            ["FIDCR1"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
            // 43.2.5 Mask Invalid Register (MKIVLR) - リセット後未定義
            ["MKIVLR"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
            // 43.2.6 Mailbox Register j (MBj) - <cluster dim="32">内の子レジスタ(ID/DLC/DATA/TS)。
            // いずれもリセット後未定義。
            ["ID"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
            ["DLC"] = new RegisterDatasheetMetadata(null, 0x0000, "read-write"),
            ["DATA"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
            ["TS"] = new RegisterDatasheetMetadata(null, 0x0000, "read-write"),
            // 43.2.7 Mailbox Interrupt Enable Register (MIER) - リセット後未定義
            ["MIER"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
            // 43.2.8 Message Control Register j (MCTLj)
            ["MCTL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            // 43.2.9 Receive FIFO Control Register (RFCR)
            ["RFCR"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
            // 43.2.10 Receive FIFO Pointer Control Register (RFPCR) - 全ビット未定義・書き込み専用
            ["RFPCR"] = new RegisterDatasheetMetadata(null, 0x00, "write-only"),
            // 43.2.11 Transmit FIFO Control Register (TFCR)
            ["TFCR"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
            // 43.2.12 Transmit FIFO Pointer Control Register (TFPCR) - 全ビット未定義・書き込み専用
            ["TFPCR"] = new RegisterDatasheetMetadata(null, 0x00, "write-only"),
            // 43.2.13 Status Register (STR) - 全ビットRのみ
            ["STR"] = new RegisterDatasheetMetadata(0x0500, 0xFFFF, "read-only"),
            // 43.2.14 Mailbox Search Mode Register (MSMR)
            ["MSMR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            // 43.2.15 Mailbox Search Status Register (MSSR) - 予約ビットがR/Wのためread-write
            ["MSSR"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
            // 43.2.16 Channel Search Support Register (CSSR) - 全ビット未定義
            ["CSSR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
            // 43.2.17 Acceptance Filter Support Register (AFSR) - 全ビット未定義
            ["AFSR"] = new RegisterDatasheetMetadata(null, 0x0000, "read-write"),
            // 43.2.18 Error Interrupt Enable Register (EIER)
            ["EIER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            // 43.2.19 Error Interrupt Factor Judge Register (EIFR)
            ["EIFR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            // 43.2.20 Receive Error Count Register (RECR) - 全ビットRのみ
            ["RECR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
            // 43.2.21 Transmit Error Count Register (TECR) - 全ビットRのみ
            ["TECR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
            // 43.2.22 Error Code Store Register (ECSR)
            ["ECSR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            // 43.2.23 Time Stamp Register (TSR) - 全ビットRのみ
            ["TSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
            // 43.2.24 Test Control Register (TCR)
            ["TCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        };
}