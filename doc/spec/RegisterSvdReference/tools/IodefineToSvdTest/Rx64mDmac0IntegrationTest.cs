using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mDmac0IntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_dmac0"];

    /// <summary>
    /// 実物のiodefine.hからst_dmac0を実際にパースしてSVDドキュメントを生成でき、
    /// DMAC0の1インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineDmac0_GeneratesDmac0InstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_DMAC0_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "DMAC0" }));
    }

    /// <summary>
    /// 実物のst_dmac0が、12個のレジスタ(DMSAR-DMCSL、アドレスオフセット順)として
    /// 解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealDmac0_Produces12Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_dmac0"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "DMSAR", "DMDAR", "DMCRA", "DMCRB", "DMTMD", "DMINT", "DMAMD", "DMOFR",
            "DMCNT", "DMREQ", "DMSTS", "DMCSL",
        }));
    }
}