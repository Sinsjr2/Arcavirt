using System.Xml.Linq;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class SvdDocumentBuilderDatasheetTest {
    static readonly string cmt0Fixture = string.Join("\n", new[] {
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
        "#define CMT0 (*(volatile struct st_cmt0 *)0x88002)",
    });

    /// <summary>
    /// データシートメタデータを与えた場合、CMCR(リセット時にb7が未定義)は
    /// access/resetValueに加えてresetMaskも出力されることを確認する
    /// (0xFF7F: 全16bit中b7のみリセット値が未定義のため)。
    /// </summary>
    [Test]
    public void Build_CmcrWithUndefinedBit_EmitsAccessResetValueAndResetMask() {
        var structs = IodefineHeaderParser.ParseStructs(cmt0Fixture, new HashSet<string> { "st_cmt0" });
        var instances = IodefineHeaderParser.ParseInstances(cmt0Fixture, new HashSet<string> { "st_cmt0" });

        var document = SvdDocumentBuilder.Build("Test", "test", instances, structs, Rx64mCmtDatasheetMetadata.Registers);

        var cmcr = document.Root!.Element("peripherals")!.Elements("peripheral").Single()
            .Element("registers")!.Elements("register").Single(r => r.Element("name")!.Value == "CMCR");

        Assert.That(
            (cmcr.Element("access")!.Value, cmcr.Element("resetValue")!.Value, cmcr.Element("resetMask")!.Value),
            Is.EqualTo(("read-write", "0x0", "0xFF7F")));
    }

    /// <summary>
    /// 全ビットのリセット値が既知のレジスタ(CMCNT)では、resetMaskが
    /// (冗長なため)出力されないことを確認する。
    /// </summary>
    [Test]
    public void Build_CmcntWithFullyKnownResetValue_OmitsResetMask() {
        var structs = IodefineHeaderParser.ParseStructs(cmt0Fixture, new HashSet<string> { "st_cmt0" });
        var instances = IodefineHeaderParser.ParseInstances(cmt0Fixture, new HashSet<string> { "st_cmt0" });

        var document = SvdDocumentBuilder.Build("Test", "test", instances, structs, Rx64mCmtDatasheetMetadata.Registers);

        var cmcnt = document.Root!.Element("peripherals")!.Elements("peripheral").Single()
            .Element("registers")!.Elements("register").Single(r => r.Element("name")!.Value == "CMCNT");

        Assert.That(cmcnt.Element("resetMask"), Is.Null);
    }

    static readonly string sci0Fixture = string.Join("\n", new[] {
        "struct st_sci0 {",
        "    union {",
        "        unsigned char BYTE;",
        "        struct {",
        "",
        "#ifdef __RX_LITTLE_ENDIAN__",
        "            unsigned char IICACKR : 1;",
        "            unsigned char  : 7;",
        "#else",
        "            unsigned char  : 7;",
        "            unsigned char IICACKR : 1;",
        "#endif",
        "        } BIT;",
        "    } SISR;",
        "};",
        "",
        "#define SCI0 (*(volatile struct st_sci0 *)0x8A000)",
    });

    /// <summary>
    /// SISR(b2/b4/b5のみリセット値がデータシートで"Undefined"と明記、
    /// 残りのb0/b1/b3/b6/b7は0が明記)を与えた場合、CMCRと同様に
    /// access/resetValueに加えて、定義済みビットのみを1とするresetMaskが
    /// 出力されることを確認する。
    /// </summary>
    [Test]
    public void Build_SisrWithPartiallyUndefinedResetValue_EmitsAccessResetValueAndResetMask() {
        var structs = IodefineHeaderParser.ParseStructs(sci0Fixture, new HashSet<string> { "st_sci0" });
        var instances = IodefineHeaderParser.ParseInstances(sci0Fixture, new HashSet<string> { "st_sci0" });

        var document = SvdDocumentBuilder.Build("Test", "test", instances, structs, Rx64mSciDatasheetMetadata.Registers);

        var sisr = document.Root!.Element("peripherals")!.Elements("peripheral").Single()
            .Element("registers")!.Elements("register").Single(r => r.Element("name")!.Value == "SISR");

        Assert.That(
            (sisr.Element("access")!.Value, sisr.Element("resetValue")!.Value, sisr.Element("resetMask")!.Value),
            Is.EqualTo(("read-write", "0x0", "0xCB")));
    }
}