using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mDaIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_daを実際にパースしてSVDドキュメントを生成でき、
    /// DAインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineDa_GeneratesSingleDaInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_da" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_DA_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "DA" }));
    }

    /// <summary>
    /// 実物のst_daが、DADR0/DADR1/DACR/DADPR/DAADSCR/DAAMPCR/DAADUSRの
    /// 7レジスタ(途中の巨大パディングwk1を除く)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealDa_Produces7Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_da" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_da"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "DADR0", "DADR1", "DACR", "DADPR", "DAADSCR", "DAAMPCR", "DAADUSR",
        }));
    }
}