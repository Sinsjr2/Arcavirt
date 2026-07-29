using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mEccramIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_eccramを実際にパースしてSVDドキュメントを生成でき、
    /// ECCRAMインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineEccram_GeneratesSingleEccramInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_eccram" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_ECCRAM_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "ECCRAM" }));
    }

    /// <summary>
    /// 実物のst_eccramが、パディング(wk0/wk1)を除いた9個のレジスタ(ECCRAMMODE/
    /// ECCRAM2STS/ECCRAM1STSEN/ECCRAM1STS/ECCRAMPRCR/ECCRAM2ECAD/ECCRAM1ECAD/
    /// ECCRAMPRCR2/ECCRAMETST)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealEccram_Produces9Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_eccram" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_eccram"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "ECCRAMMODE", "ECCRAM2STS", "ECCRAM1STSEN", "ECCRAM1STS", "ECCRAMPRCR",
            "ECCRAM2ECAD", "ECCRAM1ECAD", "ECCRAMPRCR2", "ECCRAMETST",
        }));
    }
}