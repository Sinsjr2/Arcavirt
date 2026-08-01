using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserNestedFieldViewUnionPatternTest {
    static readonly string s12adFixture = string.Join("\n", new[] {
        "struct st_s12ad {",
        "    unsigned char  ADCSR;",
        "    union {",
        "        unsigned short WORD;",
        "        union {",
        "            struct {",
        "                unsigned short AD : 12;",
        "                unsigned short  : 2;",
        "                unsigned short DIAGST : 2;",
        "            } RIGHT;",
        "            struct {",
        "                unsigned short DIAGST : 2;",
        "                unsigned short  : 2;",
        "                unsigned short AD : 12;",
        "            } LEFT;",
        "        } BIT;",
        "    } ADRD;",
        "    unsigned short ADDR0;",
        "};",
        "",
        "#define S12AD (*(volatile struct st_s12ad *)0x89000)",
    });

    /// <summary>
    /// RX64MのS12AD(st_s12ad)のADRDのように、"BIT"視点自体がさらに入れ子の
    /// unionになっており、その中に"RIGHT"/"LEFT"(右詰め/左詰め、同一データの
    /// 異なるビット割り当て解釈)のような複数のstructが並ぶ場合でも、例外を
    /// 投げずにパースできることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_NestedUnionOfAlternateFieldViews_ParsesWithoutException() {
        var structs = IodefineHeaderParser.ParseStructs(s12adFixture, new HashSet<string> { "st_s12ad" });

        Assert.That(structs["st_s12ad"].Members.Select(m => m.Name), Is.EqualTo(new[] { "ADCSR", "ADRD", "ADDR0" }));
    }

    /// <summary>
    /// RIGHT/LEFTはRTCのRSECCNT/BCNT0のような別名レジスタではなく、同一データの
    /// 異なる解釈にすぎないため、ADRDは1つのレジスタとして解決され、後方の
    /// (LEFT視点の)ビットフィールド構成が採用されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_NestedUnionOfAlternateFieldViews_ProducesSingleRegisterUsingLastView() {
        var structs = IodefineHeaderParser.ParseStructs(s12adFixture, new HashSet<string> { "st_s12ad" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_s12ad"]);

        Assert.That(registers.Count(r => r.Name == "ADRD"), Is.EqualTo(1));
        var adrd = registers.Single(r => r.Name == "ADRD");
        Assert.That(
            (adrd.AlternateRegister, adrd.Fields.Select(f => (f.Name, f.BitOffset, f.BitWidth))),
            Is.EqualTo(
                ((string?)null, (IEnumerable<(string, int, int)>)new[] { ("DIAGST", 0, 2), ("AD", 4, 12) })));
    }
}