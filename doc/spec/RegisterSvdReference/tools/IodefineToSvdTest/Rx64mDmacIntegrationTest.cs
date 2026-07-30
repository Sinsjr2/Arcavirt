using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mDmacIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_dmac"];

    /// <summary>
    /// 実物のiodefine.hからst_dmacを実際にパースしてSVDドキュメントを生成でき、
    /// DMACの1インスタンスが生成されることを確認する。DMAC0-7のチャネル別
    /// レジスタ(struct st_dmac0、void*メンバーのためパーサー未対応)は対象外。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineDmac_GeneratesDmacInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_DMAC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "DMAC" }));
    }

    /// <summary>
    /// 実物のst_dmacが、2個のレジスタ(DMAST/DMIST)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealDmac_Produces2Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_dmac"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] { "DMAST", "DMIST" }));
    }
}