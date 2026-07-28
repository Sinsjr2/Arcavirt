namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 22章(I/O Ports, 22.3 Register Descriptions, p.789-796)から手動で書き写した
/// レジスタごとのリセット値・アクセス権限。
/// 22.3の各レジスタ表はポート個別ではなく「PORTm.XXX」として汎用的に
/// (m = 0 to 9, A to G, and J)1回だけ記載されており、ビット単位の意味・
/// アクセス権限はどのポートでも同じ(存在しないピンに対応するビットも
/// 予約ビットとして同じアクセス権限を持つ)。そのためCMT/CMTWと同じ
/// レジスタ名キーの辞書構造がそのまま使える(存在ピンの違いはiodefine.h側の
/// フィールド名集合の違いとして自動的に表現される)。
/// PIDR(ポート入力レジスタ)はリセット時の値が全ビット未定義("x")と
/// 明記されているため、resetValueを持たせず(捏造しない)access=read-onlyのみ
/// 設定する。
/// </summary>
public static class Rx64mPortDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["PDR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["PODR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["PIDR"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["PMR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ODR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ODR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["PCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DSCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}