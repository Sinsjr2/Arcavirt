using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mSrcIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_srcを実際にパースしてSVDドキュメントを生成でき、
    /// SRCインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineSrc_GeneratesSingleSrcInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_src" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_SRC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "SRC" }));
    }

    /// <summary>
    /// 実物のst_srcが、7個のレジスタ(SRCFCTR/SRCID/SRCOD/SRCIDCTRL/SRCODCTRL/
    /// SRCCTRL/SRCSTAT)として解決されることを確認する。SRCFCTR[5552]が配列レジスタ
    /// (ICU(c0i.11)のIR[256]と同じパターン)として1エントリに集約されることも
    /// 併せて確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealSrc_Produces7Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_src" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_src"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "SRCFCTR", "SRCID", "SRCOD", "SRCIDCTRL", "SRCODCTRL", "SRCCTRL", "SRCSTAT",
        }));
    }
}