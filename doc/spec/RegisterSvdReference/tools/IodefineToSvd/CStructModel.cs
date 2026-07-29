namespace IodefineToSvd;

public record CBitField(string Name, int BitOffset, int BitWidth);

// ByteSizeは1要素分のサイズ。ArrayCountが非nullの場合(例: union配列 IR[256])、
// メンバー全体の占有バイト数はByteSize*ArrayCountになる(RegisterLayoutBuilder参照)。
// AliasNamesはSCIFAのBRR/MDDRのように、ビットフィールドを持たない無名union内で
// 複数のプレーンメンバーが同一オフセットを共有するケース(このメンバー自身が実体、
// AliasNamesが同一オフセットの別名register群)を表す。
public record CStructMember(
    string Name,
    int ByteSize,
    bool IsPadding,
    IReadOnlyList<CBitField>? Fields,
    int? ArrayCount = null,
    IReadOnlyList<string>? AliasNames = null);

public record CStruct(string Name, IReadOnlyList<CStructMember> Members);

public record PeripheralInstance(string Name, string StructTypeName, ulong BaseAddress, string? AlternatePeripheral = null);