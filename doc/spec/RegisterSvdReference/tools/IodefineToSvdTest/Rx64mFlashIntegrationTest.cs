using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mFlashIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_flashを実際にパースしてSVDドキュメントを生成でき、
    /// "FLASH"という1つのperipheralが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineFlash_GeneratesSingleFlashInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_flash" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_FLASH_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "FLASH" }));
    }

    /// <summary>
    /// 実物のst_flashのFCMDR(Flash Memory User's Manual: Hardware Interfaceの
    /// 4.13節でPCMDR/CMDRが全ビットRのみと明記されているレジスタ)が、
    /// Rx64mFlashDatasheetMetadataのメタデータ(リセット値0xFFFF・read-only)を
    /// 反映して解決されることを確認する(resetMaskは全ビット既知のため出力されない)。
    /// </summary>
    [Test]
    public void Build_RealFlashFcmdr_ResolvesDatasheetAccessAndResetValueAsReadOnly() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_flash" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build(
            "RX64M_FLASH_Pilot", "pilot", instances, structs, Rx64mFlashDatasheetMetadata.Registers);

        var flash = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Single(p => p.Element("name")!.Value == "FLASH");
        var fcmdr = flash.Element("registers")!.Elements("register")
            .Single(r => r.Element("name")!.Value == "FCMDR");

        Assert.That(
            (fcmdr.Element("access")!.Value, fcmdr.Element("resetValue")!.Value, fcmdr.Element("resetMask")),
            Is.EqualTo(("read-only", "0xFFFF", (System.Xml.Linq.XElement?)null)));
    }
}