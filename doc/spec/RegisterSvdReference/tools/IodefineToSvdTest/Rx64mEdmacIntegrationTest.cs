using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mEdmacIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_edmac"];

    /// <summary>
    /// 実物のiodefine.hからst_edmacを実際にパースしてSVDドキュメントを生成でき、
    /// EDMAC0・EDMAC1の2インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineEdmac_GeneratesEdmac0And1InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_EDMAC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "EDMAC0", "EDMAC1" }));
    }

    /// <summary>
    /// 実物のst_edmacが、22個のレジスタ(EDMR-TDFAR、アドレスオフセット順)として
    /// 解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealEdmac_Produces22Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_edmac"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "EDMR", "EDTRR", "EDRRR", "TDLAR", "RDLAR", "EESR", "EESIPR", "TRSCER",
            "RMFCR", "TFTR", "FDR", "RMCR", "TFUCR", "RFOCR", "IOSR", "FCFTR",
            "RPADIR", "TRIMD", "RBWAR", "RDFAR", "TBRAR", "TDFAR",
        }));
    }
}