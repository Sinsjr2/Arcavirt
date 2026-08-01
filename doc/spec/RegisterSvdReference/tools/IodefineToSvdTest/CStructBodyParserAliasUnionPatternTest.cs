using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserAliasUnionPatternTest {
    static readonly string scifaFixture = string.Join("\n", new[] {
        "struct st_scifa {",
        "    unsigned char  SMR;",
        "    union {",
        "        unsigned char  BRR;",
        "        unsigned char  MDDR;",
        "    };",
        "    unsigned char  SCR;",
        "};",
        "",
        "#define SCIFA8 (*(volatile struct st_scifa *)0xD0000)",
    });

    /// <summary>
    /// RX64MのSCIFA(st_scifa)のBRR/MDDRのように、ビットフィールドを持たない
    /// プレーンなメンバーが複数並ぶ無名union("union { unsigned char BRR; unsigned
    /// char MDDR; };"、閉じ括弧の直後にレジスタ名を持たない)を、例外を投げずに
    /// パースできることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_UnnamedUnionOfPlainAliasMembers_ParsesWithoutException() {
        var structs = IodefineHeaderParser.ParseStructs(scifaFixture, new HashSet<string> { "st_scifa" });

        Assert.That(structs["st_scifa"].Members.Select(m => m.Name), Is.EqualTo(new[] { "SMR", "BRR", "SCR" }));
    }

    /// <summary>
    /// 無名エイリアスunionの最初のメンバー(BRR)がAliasesに2番目以降のメンバー名
    /// (MDDR)を保持した実体として解決され、RegisterLayoutBuilder.Resolveで
    /// BRRとMDDRの両方が同一アドレスオフセットのレジスタとして生成され、
    /// MDDR側にAlternateRegister="BRR"が設定されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_UnnamedAliasUnion_ProducesTwoRegistersAtSameOffset() {
        var structs = IodefineHeaderParser.ParseStructs(scifaFixture, new HashSet<string> { "st_scifa" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_scifa"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset, r.AlternateRegister)), Is.EqualTo(new[] {
            ("SMR", 0, (string?)null),
            ("BRR", 1, (string?)null),
            ("MDDR", 1, "BRR"),
            ("SCR", 2, (string?)null),
        }));
    }
}