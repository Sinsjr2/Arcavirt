namespace IodefineToSvd;

public record CBitField(string Name, int BitOffset, int BitWidth);

public record CStructMember(
    string Name,
    int ByteSize,
    bool IsPadding,
    IReadOnlyList<CBitField>? Fields);

public record CStruct(string Name, IReadOnlyList<CStructMember> Members);

public record PeripheralInstance(string Name, string StructTypeName, ulong BaseAddress);