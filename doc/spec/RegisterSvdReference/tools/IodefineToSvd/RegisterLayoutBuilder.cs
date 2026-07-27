namespace IodefineToSvd;

public record ResolvedRegister(string Name, int AddressOffset, int ByteSize, IReadOnlyList<CBitField> Fields);

public static class RegisterLayoutBuilder {
    public static IReadOnlyList<ResolvedRegister> Resolve(CStruct structDef) {
        var registers = new List<ResolvedRegister>();
        var offset = 0;
        foreach (var member in structDef.Members) {
            if (!member.IsPadding) {
                registers.Add(new ResolvedRegister(member.Name, offset, member.ByteSize, member.Fields ?? []));
            }
            offset += member.ByteSize;
        }
        return registers;
    }

    public static int TotalByteSize(CStruct structDef) {
        return structDef.Members.Sum(m => m.ByteSize);
    }
}