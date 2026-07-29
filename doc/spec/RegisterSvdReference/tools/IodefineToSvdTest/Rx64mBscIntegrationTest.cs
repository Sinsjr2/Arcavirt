using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mBscIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_bscを実際にパースしてSVDドキュメントを生成でき、
    /// BSCインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineBsc_GeneratesSingleBscInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_bsc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_BSC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "BSC" }));
    }

    /// <summary>
    /// 実物のst_bscが、パディング(wk0〜wk36)を除いた58個のレジスタ(BERCLR/BEREN/
    /// BERSR1/BERSR2/BUSPRI、CSnMOD/CSnWCR1/CSnWCR2/CSnCR/CSnREC(n = 0 to 7)、
    /// CSRECEN、SDCCR/SDCMOD/SDAMOD/SDSELF/SDRFCR/SDRFEN/SDICR/SDIR/SDADR/SDTR/
    /// SDMOD/SDSR)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealBsc_Produces58Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_bsc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_bsc"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "BERCLR", "BEREN", "BERSR1", "BERSR2", "BUSPRI",
            "CS0MOD", "CS0WCR1", "CS0WCR2", "CS1MOD", "CS1WCR1", "CS1WCR2",
            "CS2MOD", "CS2WCR1", "CS2WCR2", "CS3MOD", "CS3WCR1", "CS3WCR2",
            "CS4MOD", "CS4WCR1", "CS4WCR2", "CS5MOD", "CS5WCR1", "CS5WCR2",
            "CS6MOD", "CS6WCR1", "CS6WCR2", "CS7MOD", "CS7WCR1", "CS7WCR2",
            "CS0CR", "CS0REC", "CS1CR", "CS1REC", "CS2CR", "CS2REC", "CS3CR", "CS3REC",
            "CS4CR", "CS4REC", "CS5CR", "CS5REC", "CS6CR", "CS6REC", "CS7CR", "CS7REC",
            "CSRECEN",
            "SDCCR", "SDCMOD", "SDAMOD", "SDSELF", "SDRFCR", "SDRFEN", "SDICR", "SDIR",
            "SDADR", "SDTR", "SDMOD", "SDSR",
        }));
    }
}