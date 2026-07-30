using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mTmr01IntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_tmr01"];

    /// <summary>
    /// 実物のiodefine.hからst_tmr01を実際にパースしてSVDドキュメントを生成でき、
    /// TMR01・TMR23の2インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineTmr01_GeneratesTmr01AndTmr23InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_TMR01_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "TMR01", "TMR23" }));
    }

    /// <summary>
    /// 実物のst_tmr01が、4個のレジスタ(TCORA/TCORB/TCNT/TCCR)として解決される
    /// ことを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealTmr01_Produces4Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_tmr01"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "TCORA", "TCORB", "TCNT", "TCCR",
        }));
    }
}