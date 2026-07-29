using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mIwdtIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_iwdtを実際にパースしてSVDドキュメントを生成でき、
    /// IWDTインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineIwdt_GeneratesSingleIwdtInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_iwdt" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_IWDT_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "IWDT" }));
    }

    /// <summary>
    /// 実物のst_iwdtが、IWDTRR/IWDTCR/IWDTSR/IWDTRCR/IWDTCSTPRの
    /// 5レジスタとして解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealIwdt_Produces5Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_iwdt" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_iwdt"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "IWDTRR", "IWDTCR", "IWDTSR", "IWDTRCR", "IWDTCSTPR",
        }));
    }
}