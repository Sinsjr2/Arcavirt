using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class IodefineHeaderParserTest {
    static readonly string cmtFixture = string.Join("\n", new[] {
        "struct st_cmt {",
        "    union {",
        "        unsigned short WORD;",
        "        struct {",
        "",
        "#ifdef __RX_LITTLE_ENDIAN__",
        "            unsigned short STR0 : 1;",
        "            unsigned short STR1 : 1;",
        "            unsigned short  : 14;",
        "#else",
        "            unsigned short  : 14;",
        "            unsigned short STR1 : 1;",
        "            unsigned short STR0 : 1;",
        "#endif",
        "        } BIT;",
        "    } CMSTR0;",
        "    char wk0[14];",
        "    union {",
        "        unsigned short WORD;",
        "        struct {",
        "",
        "#ifdef __RX_LITTLE_ENDIAN__",
        "            unsigned short STR2 : 1;",
        "            unsigned short STR3 : 1;",
        "            unsigned short  : 14;",
        "#else",
        "            unsigned short  : 14;",
        "            unsigned short STR3 : 1;",
        "            unsigned short STR2 : 1;",
        "#endif",
        "        } BIT;",
        "    } CMSTR1;",
        "};",
        "",
        "struct st_cmt0 {",
        "    union {",
        "        unsigned short WORD;",
        "        struct {",
        "",
        "#ifdef __RX_LITTLE_ENDIAN__",
        "            unsigned short CKS : 2;",
        "            unsigned short  : 4;",
        "            unsigned short CMIE : 1;",
        "            unsigned short  : 9;",
        "#else",
        "            unsigned short  : 9;",
        "            unsigned short CMIE : 1;",
        "            unsigned short  : 4;",
        "            unsigned short CKS : 2;",
        "#endif",
        "        } BIT;",
        "    } CMCR;",
        "    unsigned short CMCNT;",
        "    unsigned short CMCOR;",
        "};",
        "",
        "#define CMT (*(volatile struct st_cmt *)0x88000)",
        "#define CMT0 (*(volatile struct st_cmt0 *)0x88002)",
        "#define CMT1 (*(volatile struct st_cmt0 *)0x88008)",
    });

    /// <summary>
    /// CMCRのビットフィールドが、無名パディングを挟んでもLSBファーストで
    /// 正しいbitOffsetに解決されることを確認する
    /// (CKS: bitOffset0 width2、CMIE: bitOffset6 width1)。
    /// </summary>
    [Test]
    public void ParseStructs_CmcrWithPadding_ResolvesBitOffsetsAfterPadding() {
        var structs = IodefineHeaderParser.ParseStructs(cmtFixture, new HashSet<string> { "st_cmt0" });
        var cmcr = structs["st_cmt0"].Members.Single(m => m.Name == "CMCR");

        Assert.That(cmcr.Fields!.Select(f => (f.Name, f.BitOffset, f.BitWidth)), Is.EqualTo(new[] {
            ("CKS", 0, 2),
            ("CMIE", 6, 1),
        }));
    }

    /// <summary>
    /// st_cmtのCMSTR0/CMSTR1が、間のchar配列パディング(wk0[14])を
    /// バイトオフセットとして加算した上でレジスタオフセットに
    /// 反映されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_CmtWithCharArrayPadding_AdvancesOffsetPastPadding() {
        var structs = IodefineHeaderParser.ParseStructs(cmtFixture, new HashSet<string> { "st_cmt" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_cmt"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset)), Is.EqualTo(new[] {
            ("CMSTR0", 0),
            ("CMSTR1", 0x10),
        }));
    }

    /// <summary>
    /// st_cmt0のビットフィールドを持たない素のスカラメンバ(CMCNT/CMCOR)が、
    /// レジスタとして正しいバイトオフセットで解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_PlainScalarMembers_ResolvesSequentialByteOffsets() {
        var structs = IodefineHeaderParser.ParseStructs(cmtFixture, new HashSet<string> { "st_cmt0" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_cmt0"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset, r.ByteSize)), Is.EqualTo(new[] {
            ("CMCR", 0, 2),
            ("CMCNT", 2, 2),
            ("CMCOR", 4, 2),
        }));
    }

    /// <summary>
    /// #define行から、対象struct型に一致するインスタンス名とベースアドレスが
    /// 正しく抽出されることを確認する。
    /// </summary>
    [Test]
    public void ParseInstances_MatchingStructTypes_ExtractsNameAndBaseAddress() {
        var instances = IodefineHeaderParser.ParseInstances(cmtFixture, new HashSet<string> { "st_cmt", "st_cmt0" });

        Assert.That(instances.Select(i => (i.Name, i.StructTypeName, i.BaseAddress)), Is.EqualTo(new[] {
            ("CMT", "st_cmt", 0x88000UL),
            ("CMT0", "st_cmt0", 0x88002UL),
            ("CMT1", "st_cmt0", 0x88008UL),
        }));
    }
}