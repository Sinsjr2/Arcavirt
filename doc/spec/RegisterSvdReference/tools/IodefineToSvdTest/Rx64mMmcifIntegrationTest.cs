using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mMmcifIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_mmcif"];

    /// <summary>
    /// 実物のiodefine.hからst_mmcifを実際にパースしてSVDドキュメントを生成でき、
    /// MMCIFインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineMmcif_GeneratesSingleMmcifInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_MMCIF_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "MMCIF" }));
    }

    /// <summary>
    /// 実物のst_mmcifが、21個のレジスタ(CECMDSET/CEARG/CEARGCMD12/CECMDCTRL/
    /// CEBLOCKSET/CECLKCTRL/CEBUFACC/CERESP3/CERESP2/CERESP1/CERESP0/CERESPCMD12/
    /// CEDATA/CEBOOT/CEINT/CEINTEN/CEHOSTSTS1/CEHOSTSTS2/CEDETECT/CEADDMODE/
    /// CEVERSION)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealMmcif_Produces21Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_mmcif"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "CECMDSET", "CEARG", "CEARGCMD12", "CECMDCTRL", "CEBLOCKSET", "CECLKCTRL",
            "CEBUFACC", "CERESP3", "CERESP2", "CERESP1", "CERESP0", "CERESPCMD12",
            "CEDATA", "CEBOOT", "CEINT", "CEINTEN", "CEHOSTSTS1", "CEHOSTSTS2",
            "CEDETECT", "CEADDMODE", "CEVERSION",
        }));
    }
}