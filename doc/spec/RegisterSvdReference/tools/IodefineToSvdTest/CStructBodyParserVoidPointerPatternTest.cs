using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class CStructBodyParserVoidPointerPatternTest {
    static readonly string dmacFixture = string.Join("\n", new[] {
        "struct st_dmac0 {",
        "    void          *DMSAR;",
        "    void          *DMDAR;",
        "    unsigned long  DMCRA;",
        "};",
        "",
        "#define DMAC0 (*(volatile struct st_dmac0 *)0x82000)",
    });

    /// <summary>
    /// RX64MのDMAC0/DTC/EDMAC/EXDMAC0/EXDMAC1(DMSAR/DTCVBR/TDLAR/EDMSAR等)で使われる
    /// "void *NAME;"というポインタ型メンバーを、例外を投げずにパースできることを確認する。
    /// </summary>
    [Test]
    public void ParseStructs_VoidPointerMember_ParsesWithoutException() {
        var structs = IodefineHeaderParser.ParseStructs(dmacFixture, new HashSet<string> { "st_dmac0" });

        Assert.That(structs["st_dmac0"].Members.Select(m => m.Name), Is.EqualTo(new[] { "DMSAR", "DMDAR", "DMCRA" }));
    }

    /// <summary>
    /// void*メンバーは、RX(32bitアーキテクチャ)のポインタとして常に4byte固定で
    /// 解決され、後続メンバーのオフセットが正しく4byte分進むことを確認する
    /// (指す先の型がvoidであることに影響されない)。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_VoidPointerMembers_AdvanceOffsetByFourBytesEach() {
        var structs = IodefineHeaderParser.ParseStructs(dmacFixture, new HashSet<string> { "st_dmac0" });
        var registers = RegisterLayoutBuilder.Resolve(structs["st_dmac0"]);

        Assert.That(registers.Select(r => (r.Name, r.AddressOffset, r.ByteSize)), Is.EqualTo(new[] {
            ("DMSAR", 0, 4),
            ("DMDAR", 4, 4),
            ("DMCRA", 8, 4),
        }));
    }
}