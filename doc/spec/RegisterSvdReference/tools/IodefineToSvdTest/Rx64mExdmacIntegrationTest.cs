using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mExdmacIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_exdmac"];

    /// <summary>
    /// 実物のiodefine.hからst_exdmacを実際にパースしてSVDドキュメントを生成でき、
    /// EXDMACの1インスタンスが生成されることを確認する。EXDMAC0-1のチャネル別
    /// レジスタ(struct st_exdmac0/st_exdmac1、void*メンバーのためパーサー未対応)は
    /// 対象外。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineExdmac_GeneratesExdmacInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_EXDMAC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "EXDMAC" }));
    }

    /// <summary>
    /// 実物のst_exdmacが、9個のレジスタ(EDMAST/CLSBR0-CLSBR7)として
    /// 解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealExdmac_Produces9Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_exdmac"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "EDMAST", "CLSBR0", "CLSBR1", "CLSBR2", "CLSBR3", "CLSBR4", "CLSBR5", "CLSBR6", "CLSBR7",
        }));
    }
}