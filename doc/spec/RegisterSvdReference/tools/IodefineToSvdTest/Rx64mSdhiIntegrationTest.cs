using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mSdhiIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_sdhi"];

    /// <summary>
    /// 実物のiodefine.hからst_sdhiを実際にパースしてSVDドキュメントを生成でき、
    /// SDHIの1インスタンスが生成されることを確認する。SDHI(SD Host Interface)は
    /// iodefine.h上でSDHIの1インスタンスのみが定義されている。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineSdhi_GeneratesSdhiInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_SDHI_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "SDHI" }));
    }

    /// <summary>
    /// 実物のst_sdhiが、25個のレジスタ(SDCMD/SDARG/SDSTOP/SDBLKCNT/SDRSP10/SDRSP32/
    /// SDRSP54/SDRSP76/SDSTS1/SDSTS2/SDIMSK1/SDIMSK2/SDCLKCR/SDSIZE/SDOPT/SDERSTS1/
    /// SDERSTS2/SDBUFR/SDIOMD/SDIOSTS/SDIOIMSK/SDDMAEN/SDRST/SDVER/SDSWAP)として
    /// 解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealSdhi_Produces25Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_sdhi"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "SDCMD", "SDARG", "SDSTOP", "SDBLKCNT", "SDRSP10", "SDRSP32", "SDRSP54", "SDRSP76",
            "SDSTS1", "SDSTS2", "SDIMSK1", "SDIMSK2", "SDCLKCR", "SDSIZE", "SDOPT", "SDERSTS1", "SDERSTS2",
            "SDBUFR", "SDIOMD", "SDIOSTS", "SDIOIMSK", "SDDMAEN", "SDRST", "SDVER", "SDSWAP",
        }));
    }
}