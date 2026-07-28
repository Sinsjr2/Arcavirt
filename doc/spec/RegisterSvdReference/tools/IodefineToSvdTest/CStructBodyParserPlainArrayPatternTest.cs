using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserPlainArrayPatternTest {
    static readonly string dpsbkrFixture = string.Join("\n", new[] {
        "struct st_system {",
        "    unsigned char  DPSBYCR;",
        "    unsigned char  DPSBKR[32];",
        "    unsigned char  MOFCR;",
        "};",
        "",
        "#define SYSTEM (*(volatile struct st_system *)0x80000)",
    });

    /// <summary>
    /// RX64MのSYSTEM.DPSBKR[32]のように、ビットフィールドを持たない素の配列
    /// メンバー(union経由でない、"unsigned char DPSBKR[32];")が、
    /// 1要素分のByteSizeとArrayCount=32を持つメンバーとして解決できることを
    /// 確認する(修正前は配列全体を1つの32byteレジスタとして誤認識していた)。
    /// </summary>
    [Test]
    public void ParseStructs_PlainArrayMember_ResolvesElementByteSizeAndArrayCount() {
        var structs = IodefineHeaderParser.ParseStructs(dpsbkrFixture, new HashSet<string> { "st_system" });

        var dpsbkr = structs["st_system"].Members.Single(m => m.Name == "DPSBKR");

        Assert.That((dpsbkr.ByteSize, dpsbkr.ArrayCount), Is.EqualTo((1, 32)));
    }

    /// <summary>
    /// DPSBKR[32](1byte×32要素=32byte)の直後にあるMOFCRが、配列全体の
    /// バイト数だけオフセットを進めた位置で解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_MofcrAfterDpsbkrArray_AdvancesOffsetByArrayTotalSize() {
        var structs = IodefineHeaderParser.ParseStructs(dpsbkrFixture, new HashSet<string> { "st_system" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_system"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset)), Is.EqualTo(new[] {
            ("DPSBYCR", 0),
            ("DPSBKR", 1),
            ("MOFCR", 33),
        }));
    }
}