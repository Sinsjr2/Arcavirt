using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mUsb0IntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_usb0"];

    /// <summary>
    /// 実物のiodefine.hからst_usb0を実際にパースしてSVDドキュメントを生成でき、
    /// USB0の1インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineUsb0_GeneratesUsb0InstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_USB0_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "USB0" }));
    }

    /// <summary>
    /// 実物のst_usb0が、63個のレジスタ(SYSCFG-PHYSLEW、アドレスオフセット順)として
    /// 解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealUsb0_Produces63Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_usb0"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "SYSCFG", "SYSSTS0", "DVSTCTR0", "CFIFO", "D0FIFO", "D1FIFO", "CFIFOSEL", "CFIFOCTR",
            "D0FIFOSEL", "D0FIFOCTR", "D1FIFOSEL", "D1FIFOCTR", "INTENB0", "INTENB1", "BRDYENB",
            "NRDYENB", "BEMPENB", "SOFCFG", "INTSTS0", "INTSTS1", "BRDYSTS", "NRDYSTS", "BEMPSTS",
            "FRMNUM", "DVCHGR", "USBADDR", "USBREQ", "USBVAL", "USBINDX", "USBLENG", "DCPCFG",
            "DCPMAXP", "DCPCTR", "PIPESEL", "PIPECFG", "PIPEMAXP", "PIPEPERI", "PIPE1CTR", "PIPE2CTR",
            "PIPE3CTR", "PIPE4CTR", "PIPE5CTR", "PIPE6CTR", "PIPE7CTR", "PIPE8CTR", "PIPE9CTR",
            "PIPE1TRE", "PIPE1TRN", "PIPE2TRE", "PIPE2TRN", "PIPE3TRE", "PIPE3TRN", "PIPE4TRE",
            "PIPE4TRN", "PIPE5TRE", "PIPE5TRN", "DEVADD0", "DEVADD1", "DEVADD2", "DEVADD3",
            "DEVADD4", "DEVADD5", "PHYSLEW",
        }));
    }
}