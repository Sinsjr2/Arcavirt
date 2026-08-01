namespace IodefineToSvd;

// ByteSizeは1要素分のサイズ(dim付きレジスタでも配列全体ではなく1要素分)。
// AlternateRegisterが非nullの場合、同名の実体レジスタと同一アドレスを共有する
// 別名ビュー(CMSIS-SVDのalternateRegister、SCIFAのBRR/MDDR等)であることを示す。
public record ResolvedRegister(string Name, int AddressOffset, int ByteSize, IReadOnlyList<CBitField> Fields, int? ArrayCount = null, string? AlternateRegister = null);

// AddressOffsetはクラスター1要素目の先頭オフセット、ByteSizeはクラスター1要素分の
// 合計サイズ(dimIncrement)。Registersのaddress offsetはクラスター先頭からの相対値
// (CMSIS-SVDのcluster内register配置と同じ意味)。
public record ResolvedCluster(string Name, int AddressOffset, int ByteSize, int ArrayCount, IReadOnlyList<ResolvedRegister> Registers);

public static class RegisterLayoutBuilder {
    public static IReadOnlyList<ResolvedRegister> Resolve(CStruct structDef) {
        return ResolveMembers(structDef.Members, out _);
    }

    // RX64MのCAN(st_can)のMB[32]のように、無名構造体そのものが配列になっている
    // メンバー(CStructMember.Cluster)を<cluster dim>として解決する。
    // Resolveはクラスターメンバーをフラットなレジスタとして扱わずスキップする
    // (下のResolveMembers参照)ため、こちらは別枠として呼び出す。
    public static IReadOnlyList<ResolvedCluster> ResolveClusters(CStruct structDef) {
        var clusters = new List<ResolvedCluster>();
        var offset = 0;
        foreach (var member in structDef.Members) {
            if (member.Cluster is { } clusterMembers) {
                var childRegisters = ResolveMembers(clusterMembers, out var elementByteSize);
                clusters.Add(new ResolvedCluster(member.Name, offset, elementByteSize, member.ArrayCount ?? 1, childRegisters));
            }
            offset += member.ByteSize * (member.ArrayCount ?? 1);
        }
        return clusters;
    }

    static IReadOnlyList<ResolvedRegister> ResolveMembers(IReadOnlyList<CStructMember> members, out int totalByteSize) {
        var registers = new List<ResolvedRegister>();
        var offset = 0;
        foreach (var member in members) {
            if (member.Cluster is null && !member.IsPadding) {
                registers.Add(new ResolvedRegister(member.Name, offset, member.ByteSize, member.Fields ?? [], member.ArrayCount));
                foreach (var alias in member.Aliases ?? []) {
                    registers.Add(new ResolvedRegister(alias.Name, offset, alias.ByteSize, alias.Fields ?? [], AlternateRegister: member.Name));
                }
            }
            offset += member.ByteSize * (member.ArrayCount ?? 1);
        }
        totalByteSize = offset;
        return registers;
    }

    public static int TotalByteSize(CStruct structDef) {
        return structDef.Members.Sum(m => m.ByteSize * (m.ArrayCount ?? 1));
    }
}