using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserSciPatternTest {
    static readonly string sci0Fixture = string.Join("\n", new[] {
        "struct st_sci0 {",
        "    unsigned char  TDR;",
        "    union {",
        "        unsigned short WORD;",
        "        struct {",
        "            unsigned char TDRH;",
        "            unsigned char TDRL;",
        "        } BYTE;",
        "    } TDRHL;",
        "    unsigned char  MDDR;",
        "};",
        "",
        "#define SCI0 (*(volatile struct st_sci0 *)0x8A000)",
    });

    /// <summary>
    /// RX64MのSCI0.TDRHLのように、union内がビットフィールドではなく
    /// バイト単位の名前付きサブメンバー("unsigned char TDRH;"のようにコロンが無い)
    /// を持ち、内部structの名前も"BIT"ではなく"BYTE"であるパターンを、
    /// 例外を投げずに1つの16bitレジスタ(フィールドなし)として解決できることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_UnionWithNamedByteHalves_ResolvesAsSingleRegisterWithoutFields() {
        var structs = IodefineHeaderParser.ParseStructs(sci0Fixture, new HashSet<string> { "st_sci0" });

        var tdrhl = structs["st_sci0"].Members.Single(m => m.Name == "TDRHL");

        Assert.That((tdrhl.ByteSize, tdrhl.Fields!.Count), Is.EqualTo((2, 0)));
    }

    /// <summary>
    /// TDRHL(2byte)の前後にあるTDR/MDDRが、TDRHLのバイトサイズ分だけ
    /// 正しくオフセットをずらして解決されることを確認する
    /// (union内配列非対応の修正がRegisterLayoutBuilderのオフセット累積計算を
    /// 壊していないことの確認)。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_TdrhlBetweenPlainMembers_AdvancesOffsetByTwoBytes() {
        var structs = IodefineHeaderParser.ParseStructs(sci0Fixture, new HashSet<string> { "st_sci0" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_sci0"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset)), Is.EqualTo(new[] {
            ("TDR", 0),
            ("TDRHL", 1),
            ("MDDR", 3),
        }));
    }
}