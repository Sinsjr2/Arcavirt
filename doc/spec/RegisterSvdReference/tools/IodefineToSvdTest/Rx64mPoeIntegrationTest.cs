using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mPoeIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_poe"];

    /// <summary>
    /// 実物のiodefine.hからst_poeを実際にパースしてSVDドキュメントを生成でき、
    /// POE3の1インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefinePoe_GeneratesPoe3InstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_POE_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "POE3" }));
    }

    /// <summary>
    /// 実物のst_poeが、26個のレジスタ(ICSR1/OCSR1/ICSR2/OCSR2/ICSR3/SPOER/
    /// POECR1-6/ICSR4/ICSR5/ALR1/ICSR6/G0SELR-G3SELR/M0SELR1/M0SELR2/M3SELR/
    /// M4SELR1/M4SELR2/MGSELR)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealPoe_Produces26Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_poe"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "ICSR1", "OCSR1", "ICSR2", "OCSR2", "ICSR3", "SPOER", "POECR1", "POECR2", "POECR3", "POECR4", "POECR5", "POECR6",
            "ICSR4", "ICSR5", "ALR1", "ICSR6", "G0SELR", "G1SELR", "G2SELR", "G3SELR", "M0SELR1", "M0SELR2", "M3SELR",
            "M4SELR1", "M4SELR2", "MGSELR",
        }));
    }
}