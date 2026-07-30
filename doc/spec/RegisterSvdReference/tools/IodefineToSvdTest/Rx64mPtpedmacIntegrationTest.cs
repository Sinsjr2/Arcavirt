using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mPtpedmacIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_ptpedmac"];

    /// <summary>
    /// 実物のiodefine.hからst_ptpedmacを実際にパースしてSVDドキュメントを生成でき、
    /// PTPEDMACの1インスタンスが生成されることを確認する。EDMAC0/EDMAC1
    /// (ETHERC0/ETHERC1用チャネル)は別structのため対象外。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefinePtpedmac_GeneratesPtpedmacInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_PTPEDMAC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "PTPEDMAC" }));
    }

    /// <summary>
    /// 実物のst_ptpedmacが、22個のレジスタ(EDMR/EDTRR/EDRRR/TDLAR/RDLAR/EESR/
    /// EESIPR/TRSCER/RMFCR/TFTR/FDR/RMCR/TFUCR/RFOCR/IOSR/FCFTR/RPADIR/TRIMD/
    /// RBWAR/RDFAR/TBRAR/TDFAR)として解決されることを確認する。TRSCER・IOSRは
    /// データシート上PTPEDMAC向けのアドレス・ビット説明表を持たないため
    /// Rx64mPtpedmacDatasheetMetadata.Registersには含まれないが、iodefine.h上の
    /// 物理レイアウトとしては存在するため、このレイアウト解決には含まれる。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealPtpedmac_Produces22Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_ptpedmac"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "EDMR", "EDTRR", "EDRRR", "TDLAR", "RDLAR", "EESR", "EESIPR", "TRSCER", "RMFCR", "TFTR", "FDR", "RMCR",
            "TFUCR", "RFOCR", "IOSR", "FCFTR", "RPADIR", "TRIMD", "RBWAR", "RDFAR", "TBRAR", "TDFAR",
        }));
    }
}