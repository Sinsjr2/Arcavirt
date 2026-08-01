using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mExdmac1IntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_exdmac1"];

    /// <summary>
    /// 実物のiodefine.hからst_exdmac1を実際にパースしてSVDドキュメントを生成でき、
    /// EXDMAC1の1インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineExdmac1_GeneratesExdmac1InstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_EXDMAC1_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "EXDMAC1" }));
    }

    /// <summary>
    /// 実物のst_exdmac1が、14個のレジスタ(EDMSAR-EDMPRF、EDMOFRを含まない、
    /// アドレスオフセット順)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealExdmac1_Produces14Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_exdmac1"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "EDMSAR", "EDMDAR", "EDMCRA", "EDMCRB", "EDMTMD", "EDMOMD", "EDMINT",
            "EDMAMD", "EDMCNT", "EDMREQ", "EDMSTS", "EDMRMD", "EDMERF", "EDMPRF",
        }));
    }
}