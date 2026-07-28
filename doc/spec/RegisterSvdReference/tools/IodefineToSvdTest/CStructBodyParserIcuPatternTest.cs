using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserIcuPatternTest {
    static readonly string lineCommentFixture = string.Join("\n", new[] {
        "struct st_icu {",
        "    union {",
        "        unsigned char BYTE;",
        "        struct {",
        "",
        "#ifdef __RX_LITTLE_ENDIAN__",
        "            unsigned char PIR0 : 1;",
        "//          unsigned char PIR1 : 1;",
        "            unsigned char  : 6;",
        "#else",
        "            unsigned char  : 6;",
        "//          unsigned char PIR1 : 1;",
        "            unsigned char PIR0 : 1;",
        "#endif",
        "        } BIT;",
        "    } PIBR0;",
        "};",
        "",
        "#define ICU (*(volatile struct st_icu *)0x87000)",
    });

    /// <summary>
    /// RX64MのICUのように、ビットフィールド定義の一部が"//"で行コメントアウト
    /// されている場合、例外を投げずにコメント部分を無視して残りのビットフィールドを
    /// 正しく解決できることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_LineCommentedOutBitField_IgnoresCommentAndResolvesRemainingFields() {
        var structs = IodefineHeaderParser.ParseStructs(lineCommentFixture, new HashSet<string> { "st_icu" });

        var pibr0 = structs["st_icu"].Members.Single(m => m.Name == "PIBR0");

        Assert.That(pibr0.Fields!.Select(f => (f.Name, f.BitOffset, f.BitWidth)), Is.EqualTo(new[] {
            ("PIR0", 0, 1),
        }));
    }

    static readonly string emptyUnionFixture = string.Join("\n", new[] {
        "struct st_icu {",
        "    union {",
        "        unsigned char BYTE;",
        "//      struct {",
        "//          unsigned char PIR7:1;",
        "//      } BIT;",
        "    } PIBR0;",
        "    unsigned char  DMRSR0;",
        "};",
        "",
        "#define ICU (*(volatile struct st_icu *)0x87000)",
    });

    /// <summary>
    /// RX64MのICU.PIBR0のように、ビットフィールドを説明するstruct部分が
    /// 丸ごとコメントアウトされ、union内にサイズ型メンバーしか残っていない
    /// 場合でも、フィールド無しの1レジスタとして解決できることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_UnionWithoutInnerStruct_ResolvesAsRegisterWithNoFields() {
        var structs = IodefineHeaderParser.ParseStructs(emptyUnionFixture, new HashSet<string> { "st_icu" });

        var pibr0 = structs["st_icu"].Members.Single(m => m.Name == "PIBR0");

        Assert.That((pibr0.ByteSize, pibr0.Fields!.Count), Is.EqualTo((1, 0)));
    }

    /// <summary>
    /// PIBR0(1byte、フィールド無し)の直後にあるDMRSR0が、正しいオフセット
    /// (1byte分だけ進んだ位置)で解決されることを確認する
    /// (union内structが空でもRegisterLayoutBuilderのオフセット累積計算が
    /// 壊れていないことの確認)。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_Dmrsr0AfterEmptyUnion_AdvancesOffsetByOneByte() {
        var structs = IodefineHeaderParser.ParseStructs(emptyUnionFixture, new HashSet<string> { "st_icu" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_icu"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset)), Is.EqualTo(new[] {
            ("PIBR0", 0),
            ("DMRSR0", 1),
        }));
    }

    static readonly string arrayUnionFixture = string.Join("\n", new[] {
        "struct st_icu {",
        "    union {",
        "        unsigned char BYTE;",
        "        struct {",
        "",
        "#ifdef __RX_LITTLE_ENDIAN__",
        "            unsigned char IR : 1;",
        "            unsigned char  : 7;",
        "#else",
        "            unsigned char  : 7;",
        "            unsigned char IR : 1;",
        "#endif",
        "        } BIT;",
        "    } IR[256];",
        "    unsigned char  SWINTR;",
        "};",
        "",
        "#define ICU (*(volatile struct st_icu *)0x87000)",
    });

    /// <summary>
    /// RX64MのICU.IR[256]のように、union全体が配列になっているレジスタが、
    /// 例外を投げずに1要素分のByteSizeとArrayCount=256を持つメンバーとして
    /// 解決できることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_UnionArrayMember_ResolvesElementByteSizeAndArrayCount() {
        var structs = IodefineHeaderParser.ParseStructs(arrayUnionFixture, new HashSet<string> { "st_icu" });

        var ir = structs["st_icu"].Members.Single(m => m.Name == "IR");

        Assert.That((ir.ByteSize, ir.ArrayCount), Is.EqualTo((1, 256)));
    }

    /// <summary>
    /// IR[256](1byte×256要素=256byte)の直後にあるSWINTRが、配列全体の
    /// バイト数だけオフセットを進めた位置(0x100)で解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_SwintrAfterIrArray_AdvancesOffsetByArrayTotalSize() {
        var structs = IodefineHeaderParser.ParseStructs(arrayUnionFixture, new HashSet<string> { "st_icu" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_icu"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset)), Is.EqualTo(new[] {
            ("IR", 0),
            ("SWINTR", 0x100),
        }));
    }
}