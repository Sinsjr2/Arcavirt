namespace IodefineToSvd;

public record CBitField(string Name, int BitOffset, int BitWidth);

// ByteSizeは1要素分のサイズ。ArrayCountが非nullの場合(例: union配列 IR[256])、
// メンバー全体の占有バイト数はByteSize*ArrayCountになる(RegisterLayoutBuilder参照)。
// Aliasesは、ビットフィールドを持たない無名union内で複数のプレーンメンバーが
// 同一オフセットを共有するケース(SCIFAのBRR/MDDR)や、RTCのRSECCNT/BCNT0のように
// 各エイリアスがそれ自体ビットフィールドを持つ名前付き入れ子unionであるケースの
// 両方を表す。このメンバー自身が実体、Aliasesが同一オフセットの別名register群
// (それぞれ自身のFields/ByteSizeを持つ)。
// Clusterは、RX64MのCAN(st_can)のMB[32]のように、複数レジスタをまとめた無名構造体
// 自体が配列になっているケース(CMSIS-SVDの<cluster dim>)を表す。非nullの場合、
// ByteSizeはクラスター1要素分の合計サイズ(dimIncrement)、ArrayCountがdim、
// Clusterがクラスター内の子メンバー(それぞれ独立したレジスタとして解決される)。
public record CStructMember(
    string Name,
    int ByteSize,
    bool IsPadding,
    IReadOnlyList<CBitField>? Fields,
    int? ArrayCount = null,
    IReadOnlyList<CStructMember>? Aliases = null,
    IReadOnlyList<CStructMember>? Cluster = null);

public record CStruct(string Name, IReadOnlyList<CStructMember> Members);

public record PeripheralInstance(string Name, string StructTypeName, ulong BaseAddress, string? AlternatePeripheral = null);