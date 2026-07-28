using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mSystemIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_systemを実際にパースしてSVDドキュメントを
    /// 生成できることと、SYSTEMインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineSystem_GeneratesSingleSystemInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_system" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_SYSTEM_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "SYSTEM" }));
    }

    /// <summary>
    /// DPSBKR[32](ビットフィールドを持たない素の配列レジスタ)が、
    /// dim=32・1要素1byteのレジスタとして解決されることを確認する
    /// (実機データでの素の配列dim対応の最終確認)。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealSystemDpsbkrArray_Has32ElementsOfOneByte() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_system" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_system"]);

        var dpsbkr = registers.Single(r => r.Name == "DPSBKR");

        Assert.That((dpsbkr.ByteSize, dpsbkr.ArrayCount), Is.EqualTo((1, 32)));
    }
}