namespace IodefineToSvd;

// ByteSizeは1要素分のサイズ(dim付きレジスタでも配列全体ではなく1要素分)。
// AlternateRegisterが非nullの場合、同名の実体レジスタと同一アドレスを共有する
// 別名ビュー(CMSIS-SVDのalternateRegister、SCIFAのBRR/MDDR等)であることを示す。
public record ResolvedRegister(string Name, int AddressOffset, int ByteSize, IReadOnlyList<CBitField> Fields, int? ArrayCount = null, string? AlternateRegister = null);

public static class RegisterLayoutBuilder {
    public static IReadOnlyList<ResolvedRegister> Resolve(CStruct structDef) {
        var registers = new List<ResolvedRegister>();
        var offset = 0;
        foreach (var member in structDef.Members) {
            if (!member.IsPadding) {
                registers.Add(new ResolvedRegister(member.Name, offset, member.ByteSize, member.Fields ?? [], member.ArrayCount));
                foreach (var aliasName in member.AliasNames ?? []) {
                    registers.Add(new ResolvedRegister(aliasName, offset, member.ByteSize, [], AlternateRegister: member.Name));
                }
            }
            offset += member.ByteSize * (member.ArrayCount ?? 1);
        }
        return registers;
    }

    public static int TotalByteSize(CStruct structDef) {
        return structDef.Members.Sum(m => m.ByteSize * (m.ArrayCount ?? 1));
    }
}