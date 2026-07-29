using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mRiicIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_riic"];

    /// <summary>
    /// 実物のiodefine.hからst_riicを実際にパースしてSVDドキュメントを生成でき、
    /// RIIC0・RIIC2の2インスタンスが生成されることを確認する(RIIC1は欠番)。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineRiic_GeneratesRiic0AndRiic2InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_RIIC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "RIIC0", "RIIC2" }));
    }

    /// <summary>
    /// 実物のst_riicが、20個のレジスタ(ICCR1/ICCR2/ICMR1/ICMR2/ICMR3/ICFER/ICSER/
    /// ICIER/ICSR1/ICSR2/SARL0/SARU0/SARL1/SARU1/SARL2/SARU2/ICBRL/ICBRH/ICDRT/ICDRR)
    /// として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealRiic_Produces20Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_riic"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "ICCR1", "ICCR2", "ICMR1", "ICMR2", "ICMR3", "ICFER", "ICSER", "ICIER",
            "ICSR1", "ICSR2", "SARL0", "SARU0", "SARL1", "SARU1", "SARL2", "SARU2",
            "ICBRL", "ICBRH", "ICDRT", "ICDRR",
        }));
    }
}