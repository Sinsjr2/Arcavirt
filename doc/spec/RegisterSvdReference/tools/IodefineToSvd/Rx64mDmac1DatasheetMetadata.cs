namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 18章(DMA Controller (DMACAa)、p.619-632の18.2.1-18.2.12)から手動で書き写した、
/// チャネル1-7のレジスタ群(st_dmac1、インスタンスDMAC1-DMAC7)のレジスタごとの
/// リセット値・アクセス権限。DMAC0(st_dmac0)と同一節・同一リセット値/アクセス
/// 権限のレジスタのみで構成される(DMOFRが無い点のみst_dmac0と異なる)ため、
/// Rx64mDmac0DatasheetMetadata.csの値をそのまま踏襲した。
///
/// 抽出スクリプトの「たたき台」から修正した箇所はRx64mDmac0DatasheetMetadata.csの
/// DMSTSに関するコメントを参照(本ファイルのDMSTSも同一の理由でread-writeとした)。
/// </summary>
public static class Rx64mDmac1DatasheetMetadata {
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
        // 18.2.9 DMA Transfer Enable Register (DMCNT)
        ["DMCNT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.10 DMA Software Start Register (DMREQ)
        ["DMREQ"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.11 DMA Status Register (DMSTS) - Rx64mDmac0DatasheetMetadata参照(たたき台修正)
        ["DMSTS"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.12 DMA Request Source Flag Control Register (DMCSL)
        ["DMCSL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}