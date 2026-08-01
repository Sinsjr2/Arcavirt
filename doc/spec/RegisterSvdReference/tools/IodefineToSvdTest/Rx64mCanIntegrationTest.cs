using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mCanIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_canを実際にパースしてSVDドキュメントを生成でき、
    /// "CAN0"/"CAN1"/"CAN2"の3つのperipheralが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineCan_GeneratesThreeCanInstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_can" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_CAN_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "CAN0", "CAN1", "CAN2" }));
    }

    /// <summary>
    /// 実物のst_canのMB[32](無名構造体の配列)が、フラットなレジスタではなく
    /// dim="32"のclusterとして生成され、その中にID/DLC/DATA/TSの4レジスタが
    /// 相対addressOffset(ID:0x0, DLC:0x4, DATA:0x6(dim=8), TS:0xE)で
    /// 含まれることを確認する。
    /// </summary>
    [Test]
    public void Build_RealCanMb_ResolvesAsClusterWithIdDlcDataTsChildren() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_can" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build(
            "RX64M_CAN_Pilot", "pilot", instances, structs, Rx64mCanDatasheetMetadata.Registers);

        var can0 = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Single(p => p.Element("name")!.Value == "CAN0");
        var registersElement = can0.Element("registers")!;

        Assert.That(registersElement.Elements("register").Any(r => r.Element("name")!.Value == "MB%s"), Is.False);

        var cluster = registersElement.Elements("cluster").Single();
        var childNames = cluster.Elements("register").Select(r => r.Element("name")!.Value).ToArray();
        var childOffsets = cluster.Elements("register").Select(r => r.Element("addressOffset")!.Value).ToArray();

        Assert.That(
            (cluster.Element("dim")!.Value, cluster.Element("dimIncrement")!.Value, cluster.Element("name")!.Value, childNames, childOffsets),
            Is.EqualTo(("32", "0x10", "MB%s", new[] { "ID", "DLC", "DATA%s", "TS" }, new[] { "0x0", "0x4", "0x6", "0xE" })));
    }
}