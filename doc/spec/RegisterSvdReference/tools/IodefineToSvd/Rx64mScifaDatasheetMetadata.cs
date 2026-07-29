namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 41章(FIFO Embedded Serial Communications Interface (SCIFA), p.2115-2240)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。全13レジスタについて
/// マニュアル本文の個別レジスタ節(41.2.1〜41.2.15)と突き合わせて確認済み(2026-07-29)。
///
/// BRR/MDDRはst_scifaの無名union(CStructBodyParserのAliasNames対応、SVD上は
/// alternateRegisterで表現)で同一アドレスを共有する別名レジスタ。SEMR.MDDRSビットで
/// どちらがアクセスされるか選択する。SCI(c0i.9)のBRR/MDDRと同じ0xFF/read-writeの扱い。
///
/// SPTRはSPB2DT/SCKDT/CTS2DT/RTS2DT(各"DT"系ビット、b0/b2/b4/b6)がリセット後
/// 未定義("x")と明記されている一方、"IO"系ビット(b1/b3/b5/b7)と上位byte(Reserved)は
/// 0固定であるため、CMCR(c0i.3)と同じ「値+部分マスク」で表現する
/// (該当4ビットを除いたResetMask=0xFFAA)。
///
/// FTDRは「write-only register」、FRDRは「read-only register」と本文に明記されている
/// (他はビット単位でR/(W)・R・R/Wが混在するため、レジスタ全体ではread-writeとして扱う)。
/// </summary>
public static class Rx64mScifaDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["SMR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["BRR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["MDDR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["SCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["FTDR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "write-only"),
        ["FSR"] = new RegisterDatasheetMetadata(0x0020, 0xFFFF, "read-write"),
        ["FRDR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        ["FCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["FDR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["SPTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFAA, "read-write"),
        ["LSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["FTCR"] = new RegisterDatasheetMetadata(0x1F1F, 0xFFFF, "read-write"),
        ["SEMR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}