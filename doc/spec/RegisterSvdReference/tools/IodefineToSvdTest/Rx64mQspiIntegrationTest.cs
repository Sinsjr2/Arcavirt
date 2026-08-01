using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mQspiIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_qspiを実際にパースしてSVDドキュメントを生成できる
    /// (SPDRの複数視点union統合パターンに対応済みであることの実機データによる検証)
    /// ことと、QSPIという1つのperipheralが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineQspi_GeneratesSingleQspiInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_qspi" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_QSPI_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "QSPI" }));
    }

    /// <summary>
    /// 実物のst_qspiのSPDR(LONG/WORD.H/BYTE.HHの複数視点union)が、フィールド無しの
    /// 1つのレジスタ"SPDR"として解決され、AlternateRegisterがnullであることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealQspiSpdr_ResolvesToSingleRegisterWithoutAlternate() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_qspi" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_qspi"]);

        var spdrAlternateRegisters = registers.Where(r => r.Name == "SPDR").Select(r => r.AlternateRegister);

        Assert.That(spdrAlternateRegisters, Is.EqualTo(new string?[] { null }));
    }
}