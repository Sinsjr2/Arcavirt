using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserNestedAliasUnionPatternTest {
    static readonly string rtcFixture = string.Join("\n", new[] {
        "struct st_rtc {",
        "    unsigned char  R64CNT;",
        "    union {",
        "        union {",
        "            unsigned char BYTE;",
        "            struct {",
        "                unsigned char SEC1 : 4;",
        "                unsigned char SEC10 : 3;",
        "            } BIT;",
        "        } RSECCNT;",
        "        union {",
        "            unsigned char BYTE;",
        "            struct {",
        "                unsigned char BCNT : 8;",
        "            } BIT;",
        "        } BCNT0;",
        "    };",
        "    unsigned char  RMINCNT;",
        "};",
        "",
        "#define RTC (*(volatile struct st_rtc *)0x8C400)",
    });

    /// <summary>
    /// RX64MのRTC(st_rtc)のRSECCNT/BCNT0のように、無名union直下のメンバーが
    /// プレーンな値ではなく、それ自体がビットフィールドを持つ名前付き入れ子union
    /// (BCD時刻カウンタ/バイナリカウンタの2つの解釈を同一アドレスに持つ)である
    /// 場合でも、例外を投げずにパースできることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_NestedAnonymousUnionOfNamedUnions_ParsesWithoutException() {
        var structs = IodefineHeaderParser.ParseStructs(rtcFixture, new HashSet<string> { "st_rtc" });

        Assert.That(structs["st_rtc"].Members.Select(m => m.Name), Is.EqualTo(new[] { "R64CNT", "RSECCNT", "RMINCNT" }));
    }

    /// <summary>
    /// 入れ子エイリアスunionの最初のメンバー(RSECCNT)が実体として解決され、
    /// 2番目以降のメンバー(BCNT0)はRegisterLayoutBuilder.Resolveで同一アドレス
    /// オフセットの別レジスタとして生成されること、かつSCIFAのBRR/MDDR
    /// (フィールド無し)とは異なり、BCNT0が自身の固有ビットフィールド(BCNT)を
    /// 保持したまま出力されることを確認する(RSECCNTのSEC1/SEC10とは異なる
    /// フィールド構成)。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_NestedAliasUnion_AliasKeepsItsOwnDistinctFields() {
        var structs = IodefineHeaderParser.ParseStructs(rtcFixture, new HashSet<string> { "st_rtc" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_rtc"]);

        var rseccnt = registers.Single(r => r.Name == "RSECCNT");
        var bcnt0 = registers.Single(r => r.Name == "BCNT0");

        Assert.That(
            (rseccnt.AddressOffset, rseccnt.AlternateRegister, rseccnt.Fields.Select(f => f.Name),
             bcnt0.AddressOffset, bcnt0.AlternateRegister, bcnt0.Fields.Select(f => f.Name)),
            Is.EqualTo(
                (1, (string?)null, (IEnumerable<string>)new[] { "SEC1", "SEC10" },
                 1, "RSECCNT", (IEnumerable<string>)new[] { "BCNT" })));
    }
}