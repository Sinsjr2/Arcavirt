namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 18章(DMA Controller (DMACAa)、p.619-632の18.2.1-18.2.12)から手動で書き写した、
/// チャネル0のレジスタ群(st_dmac0、インスタンスDMAC0)のレジスタごとのリセット値・
/// アクセス権限。DMSAR・DMDAR・DMCRAはvoid*ポインタ型メンバー(32ビットのアドレス
/// 設定レジスタ)で、本文にビット単位のBit/Symbol表があり全ビットR/Wと明記されている
/// ため、通常のレジスタと同様にread-writeとした。DMOFRはDMAC0にのみ存在する
/// レジスタ(18.2.8節)。
///
/// 抽出スクリプトの「たたき台」から修正した箇所:
/// - DMSTS(18.2.11節): 「たたき台」はaccess_candidate="read-only"としていたが、
///   本文のBit/Symbol表ではESIF・DTIFビットが"R/W*1"(Note 1: 0書き込みでのみ
///   クリア可能)であり、書込み可能なビットが存在するためread-writeに修正した
///   (ACT・予約ビットはRのみ)。
/// </summary>
public static class Rx64mDmac0DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 18.2.1 DMA Source Address Register (DMSAR)
        ["DMSAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 18.2.2 DMA Destination Address Register (DMDAR)
        ["DMDAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 18.2.3 DMA Transfer Count Register (DMCRA)
        ["DMCRA"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 18.2.4 DMA Block Transfer Count Register (DMCRB)
        ["DMCRB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 18.2.5 DMA Transfer Mode Register (DMTMD)
        ["DMTMD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 18.2.6 DMA Interrupt Setting Register (DMINT)
        ["DMINT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.7 DMA Address Mode Register (DMAMD)
        ["DMAMD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 18.2.8 DMA Offset Register (DMOFR) - DMAC0にのみ存在
        ["DMOFR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 18.2.9 DMA Transfer Enable Register (DMCNT)
        ["DMCNT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.10 DMA Software Start Register (DMREQ)
        ["DMREQ"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.11 DMA Status Register (DMSTS) - 上記コメント参照(たたき台修正)
        ["DMSTS"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.12 DMA Request Source Flag Control Register (DMCSL)
        ["DMCSL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}