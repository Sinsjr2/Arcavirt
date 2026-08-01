using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserMultiViewUnionPatternTest {
    static readonly string qspiFixture = string.Join("\n", new[] {
        "struct st_qspi {",
        "    unsigned char  SPCR;",
        "    union {",
        "        unsigned long LONG;",
        "        struct {",
        "            unsigned short H;",
        "        } WORD;",
        "        struct {",
        "            unsigned char HH;",
        "        } BYTE;",
        "    } SPDR;",
        "    unsigned char  SPBR;",
        "};",
        "",
        "#define QSPI (*(volatile struct st_qspi *)0xD0100)",
    });

    /// <summary>
    /// RX64MのQSPI(st_qspi)のSPDRのように、1つのunion内に"BIT"以外の
    /// ビットフィールドを持たない複数のstruct視点(WORD/BYTE、いずれもコロン無しの
    /// 単一メンバー)が並ぶ場合でも、例外を投げずにパースできることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_MultiViewUnionWithoutBitFields_ParsesWithoutException() {
        var structs = IodefineHeaderParser.ParseStructs(qspiFixture, new HashSet<string> { "st_qspi" });

        Assert.That(structs["st_qspi"].Members.Select(m => m.Name), Is.EqualTo(new[] { "SPCR", "SPDR", "SPBR" }));
    }

    /// <summary>
    /// WORD/BYTEのいずれの視点もビットフィールド(コロン付きメンバー)を持たないため、
    /// SPDRはフィールド無し(空リスト)のレジスタとして解決され、LONG型(4byte)分の
    /// サイズでオフセットが進むことを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_MultiViewUnionWithoutBitFields_ProducesEmptyFieldsRegister() {
        var structs = IodefineHeaderParser.ParseStructs(qspiFixture, new HashSet<string> { "st_qspi" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_qspi"]);

        var spdr = registers.Single(r => r.Name == "SPDR");
        Assert.That((spdr.AddressOffset, spdr.ByteSize, spdr.Fields.Count), Is.EqualTo((1, 4, 0)));
    }
}