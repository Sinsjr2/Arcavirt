using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mRspiIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_rspi"];

    /// <summary>
    /// 実物のiodefine.hからst_rspiを実際にパースしてSVDドキュメントを生成でき、
    /// RSPI0の1インスタンスが生成されることを確認する。RSPI(Serial Peripheral
    /// Interface)はデータシート本文(44.1節)に「one channel」と明記されている
    /// 1チャネル構成であり、iodefine.h上もRSPI0の1インスタンスのみが定義されている。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineRspi_GeneratesRspi0InstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_RSPI_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "RSPI0" }));
    }

    /// <summary>
    /// 実物のst_rspiが、21個のレジスタ(SPCR/SSLP/SPPCR/SPSR/SPDR/SPSCR/SPSSR/SPBR/
    /// SPDCR/SPCKD/SSLND/SPND/SPCR2/SPCMD0-SPCMD7)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealRspi_Produces21Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_rspi"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "SPCR", "SSLP", "SPPCR", "SPSR", "SPDR", "SPSCR", "SPSSR", "SPBR", "SPDCR", "SPCKD", "SSLND", "SPND", "SPCR2",
            "SPCMD0", "SPCMD1", "SPCMD2", "SPCMD3", "SPCMD4", "SPCMD5", "SPCMD6", "SPCMD7",
        }));
    }
}