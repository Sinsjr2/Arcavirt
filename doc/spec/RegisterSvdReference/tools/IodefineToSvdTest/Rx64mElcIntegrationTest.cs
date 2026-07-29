using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mElcIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_elcを実際にパースしてSVDドキュメントを生成でき、
    /// ELCインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineElc_GeneratesSingleElcInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_elc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_ELC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "ELC" }));
    }

    /// <summary>
    /// 実物のst_elcが、パディング(wk0〜wk10)を除いた51個のレジスタ(ELCR/ELSR0/ELSR3/
    /// ELSR4/ELSR7/ELSR10〜13/ELSR15/ELSR16/ELSR18〜28/ELOPA〜ELOPD/PGR1/PGR2/PGC1/
    /// PGC2/PDBF1/PDBF2/PEL0〜3/ELSEGR/ELSR33/ELSR35〜38/ELSR41〜45/ELOPF/ELOPH/
    /// ELOPI/ELOPJ)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealElc_Produces51Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_elc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_elc"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "ELCR", "ELSR0", "ELSR3", "ELSR4", "ELSR7", "ELSR10", "ELSR11", "ELSR12", "ELSR13",
            "ELSR15", "ELSR16", "ELSR18", "ELSR19", "ELSR20", "ELSR21", "ELSR22", "ELSR23", "ELSR24",
            "ELSR25", "ELSR26", "ELSR27", "ELSR28", "ELOPA", "ELOPB", "ELOPC", "ELOPD",
            "PGR1", "PGR2", "PGC1", "PGC2", "PDBF1", "PDBF2", "PEL0", "PEL1", "PEL2", "PEL3",
            "ELSEGR", "ELSR33", "ELSR35", "ELSR36", "ELSR37", "ELSR38", "ELSR41", "ELSR42", "ELSR43",
            "ELSR44", "ELSR45", "ELOPF", "ELOPH", "ELOPI", "ELOPJ",
        }));
    }
}