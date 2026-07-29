using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mMpuIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_mpuを実際にパースしてSVDドキュメントを生成でき、
    /// MPUインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineMpu_GeneratesSingleMpuInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_mpu" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_MPU_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "MPU" }));
    }

    /// <summary>
    /// 実物のst_mpuが、パディング(wk0/wk1/wk2)を除いた26個のレジスタ(RSPAGE0〜7/
    /// REPAGE0〜7/MPEN/MPBAC/MPECLR/MPESTS/MPDEA/MPSA/MPOPS/MPOPI/MHITI/MHITD)
    /// として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealMpu_Produces26Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_mpu" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_mpu"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "RSPAGE0", "REPAGE0", "RSPAGE1", "REPAGE1", "RSPAGE2", "REPAGE2", "RSPAGE3", "REPAGE3",
            "RSPAGE4", "REPAGE4", "RSPAGE5", "REPAGE5", "RSPAGE6", "REPAGE6", "RSPAGE7", "REPAGE7",
            "MPEN", "MPBAC", "MPECLR", "MPESTS", "MPDEA", "MPSA", "MPOPS", "MPOPI", "MHITI", "MHITD",
        }));
    }
}