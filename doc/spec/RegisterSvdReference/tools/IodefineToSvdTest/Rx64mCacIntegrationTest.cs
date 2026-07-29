using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mCacIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_cacを実際にパースしてSVDドキュメントを生成でき、
    /// CACインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineCac_GeneratesSingleCacInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_cac" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_CAC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "CAC" }));
    }

    /// <summary>
    /// 実物のst_cacが、パディング(wk0)を除いた8個のレジスタ(CACR0/CACR1/CACR2/
    /// CAICR/CASTR/CAULVR/CALLVR/CACNTBR)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealCac_Produces8Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_cac" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_cac"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "CACR0", "CACR1", "CACR2", "CAICR", "CASTR", "CAULVR", "CALLVR", "CACNTBR",
        }));
    }
}