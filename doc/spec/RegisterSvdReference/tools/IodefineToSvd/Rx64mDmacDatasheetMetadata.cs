namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 18章(DMA Controller (DMACAa)、p.632-634の18.2.13/18.2.14)から手動で書き写した
/// レジスタごとのリセット値・アクセス権限。DMAC0-7のチャネル別レジスタ(DMSAR等)は
/// struct st_dmac0が別途あり、void*ポインタ型メンバーのためパーサー未対応・別issueの
/// スコープ外。st_dmacはDMAC0-7に共通のモジュール開始レジスタ(DMAST)と
/// DMAC4-7専用の割り込みステータスモニタレジスタ(DMIST)のみを含む。
/// </summary>
public static class Rx64mDmacDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 18.2.13 DMAC Module Start Register (DMAST)
        ["DMAST"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 18.2.14 DMAC74 Interrupt Status Monitor Register (DMIST) - read-only
        ["DMIST"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
    };
}