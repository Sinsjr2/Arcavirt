using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mUsbIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_usb"];

    /// <summary>
    /// 実物のiodefine.hからst_usbを実際にパースしてSVDドキュメントを生成でき、
    /// USBの1インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineUsb_GeneratesUsbInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_USB_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "USB" }));
    }

    /// <summary>
    /// 実物のst_usbが、2個のレジスタ(DPUSR0R/DPUSR1R)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealUsb_Produces2Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_usb"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] { "DPUSR0R", "DPUSR1R" }));
    }
}