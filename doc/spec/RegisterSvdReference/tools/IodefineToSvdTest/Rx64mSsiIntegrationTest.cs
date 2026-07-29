using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mSsiIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_ssiを実際にパースしてSVDドキュメントを生成でき、
    /// SSI0・SSI1の2インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineSsi_GeneratesSsi0AndSsi1InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_ssi" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_SSI_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "SSI0", "SSI1" }));
    }

    /// <summary>
    /// 実物のst_ssiが、7個のレジスタ(SSICR/SSISR/SSIFCR/SSIFSR/SSIFTDR/SSIFRDR/
    /// SSITDMR)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealSsi_Produces7Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_ssi" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_ssi"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "SSICR", "SSISR", "SSIFCR", "SSIFSR", "SSIFTDR", "SSIFRDR", "SSITDMR",
        }));
    }
}