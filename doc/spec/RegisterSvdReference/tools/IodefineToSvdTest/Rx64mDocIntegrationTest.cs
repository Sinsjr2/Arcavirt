using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mDocIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_docを実際にパースしてSVDドキュメントを生成でき、
    /// DOCインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineDoc_GeneratesSingleDocInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_doc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_DOC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "DOC" }));
    }

    /// <summary>
    /// 実物のst_docが、DOCR/DODIR/DODSRの3レジスタとして解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealDoc_Produces3Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_doc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_doc"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] { "DOCR", "DODIR", "DODSR" }));
    }
}