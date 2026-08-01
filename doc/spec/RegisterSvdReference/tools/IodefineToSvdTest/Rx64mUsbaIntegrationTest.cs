using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mUsbaIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_usba"];

    /// <summary>
    /// 実物のiodefine.hからst_usbaを実際にパースしてSVDドキュメントを生成でき、
    /// USBAの1インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineUsba_GeneratesUsbaInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_USBA_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "USBA" }));
    }

    /// <summary>
    /// 実物のst_usbaが、74個のレジスタ(SYSCFG-DPUSR1R、アドレスオフセット順)として
    /// 解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealUsba_Produces74Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_usba"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "SYSCFG", "BUSWAIT", "SYSSTS0", "PLLSTA", "DVSTCTR0", "CFIFO", "D0FIFO", "D1FIFO",
            "CFIFOSEL", "CFIFOCTR", "D0FIFOSEL", "D0FIFOCTR", "D1FIFOSEL", "D1FIFOCTR", "INTENB0",
            "INTENB1", "BRDYENB", "NRDYENB", "BEMPENB", "SOFCFG", "PHYSET", "INTSTS0", "INTSTS1",
            "BRDYSTS", "NRDYSTS", "BEMPSTS", "FRMNUM", "USBADDR", "USBREQ", "USBVAL", "USBINDX",
            "USBLENG", "DCPCFG", "DCPMAXP", "DCPCTR", "PIPESEL", "PIPECFG", "PIPEBUF", "PIPEMAXP",
            "PIPEPERI", "PIPE1CTR", "PIPE2CTR", "PIPE3CTR", "PIPE4CTR", "PIPE5CTR", "PIPE6CTR",
            "PIPE7CTR", "PIPE8CTR", "PIPE9CTR", "PIPE1TRE", "PIPE1TRN", "PIPE2TRE", "PIPE2TRN",
            "PIPE3TRE", "PIPE3TRN", "PIPE4TRE", "PIPE4TRN", "PIPE5TRE", "PIPE5TRN", "DEVADD0",
            "DEVADD1", "DEVADD2", "DEVADD3", "DEVADD4", "DEVADD5", "LPCTRL", "LPSTS", "BCCTRL",
            "PL1CTRL1", "PL1CTRL2", "HL1CTRL1", "HL1CTRL2", "DPUSR0R", "DPUSR1R",
        }));
    }
}