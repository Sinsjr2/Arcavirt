using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mDmac1IntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_dmac1"];

    /// <summary>
    /// 実物のiodefine.hからst_dmac1を実際にパースしてSVDドキュメントを生成でき、
    /// DMAC1-DMAC7の7インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineDmac1_GeneratesDmac1To7InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_DMAC1_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] {
            "DMAC1", "DMAC2", "DMAC3", "DMAC4", "DMAC5", "DMAC6", "DMAC7",
        }));
    }

    /// <summary>
    /// 実物のst_dmac1が、11個のレジスタ(DMSAR-DMCSL、DMOFRを含まない、
    /// アドレスオフセット順)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealDmac1_Produces11Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_dmac1"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "DMSAR", "DMDAR", "DMCRA", "DMCRB", "DMTMD", "DMINT", "DMAMD",
            "DMCNT", "DMREQ", "DMSTS", "DMCSL",
        }));
    }
}