using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mPpgIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_ppg0", "st_ppg1"];

    /// <summary>
    /// 実物のiodefine.hからst_ppg0・st_ppg1を実際にパースしてSVDドキュメントを
    /// 生成でき、PPG0・PPG1の2インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefinePpg_GeneratesPpg0AndPpg1InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_PPG_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "PPG0", "PPG1" }));
    }

    /// <summary>
    /// 実物のst_ppg0が、10個のレジスタ(PCR/PMR/NDERH/NDERL/PODRH/PODRL/NDRH/NDRL/
    /// NDRH2/NDRL2)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealPpg0_Produces10Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_ppg0"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "PCR", "PMR", "NDERH", "NDERL", "PODRH", "PODRL", "NDRH", "NDRL", "NDRH2", "NDRL2",
        }));
    }

    /// <summary>
    /// 実物のst_ppg1が、11個のレジスタ(PTRSLR/PCR/PMR/NDERH/NDERL/PODRH/PODRL/NDRH/
    /// NDRL/NDRH2/NDRL2)として解決されることを確認する。PPG0に対してPTRSLRが
    /// 1つ多いことを確認する意味も持つ。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealPpg1_Produces11Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_ppg1"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "PTRSLR", "PCR", "PMR", "NDERH", "NDERL", "PODRH", "PODRL", "NDRH", "NDRL", "NDRH2", "NDRL2",
        }));
    }
}