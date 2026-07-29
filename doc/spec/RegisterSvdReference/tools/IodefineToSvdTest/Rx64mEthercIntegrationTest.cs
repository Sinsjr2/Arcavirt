using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mEthercIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_etherc"];

    /// <summary>
    /// 実物のiodefine.hからst_ethercを実際にパースしてSVDドキュメントを生成でき、
    /// ETHERC0・ETHERC1の2インスタンスが生成されることを確認する。ETHERCは
    /// データシート(35.1節)に「two-channel Ethernet controller」と明記されている
    /// 2チャネル構成であり、iodefine.h上もETHERC0・ETHERC1の2インスタンスが
    /// 定義されている。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineEtherc_GeneratesTwoChannelInstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_ETHERC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "ETHERC0", "ETHERC1" }));
    }

    /// <summary>
    /// 実物のst_ethercが、26個のレジスタ(ECMR/RFLR/ECSR/ECSIPR/PIR/PSR/RDMLR/IPGR/APR/
    /// MPR/RFCF/TPAUSER/TPAUSECR/BCFRR/MAHR/MALR/TROCR/CDCR/LCCR/CNDCR/CEFCR/FRECR/
    /// TSFRCR/TLFRCR/RFCR/MAFCR)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealEtherc_Produces26Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_etherc"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "ECMR", "RFLR", "ECSR", "ECSIPR", "PIR", "PSR", "RDMLR", "IPGR", "APR",
            "MPR", "RFCF", "TPAUSER", "TPAUSECR", "BCFRR", "MAHR", "MALR", "TROCR",
            "CDCR", "LCCR", "CNDCR", "CEFCR", "FRECR", "TSFRCR", "TLFRCR", "RFCR", "MAFCR",
        }));
    }
}