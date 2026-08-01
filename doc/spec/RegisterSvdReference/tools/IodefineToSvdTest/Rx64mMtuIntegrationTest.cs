using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mMtuIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = [
        "st_mtu", "st_mtu0", "st_mtu1", "st_mtu2", "st_mtu3",
        "st_mtu4", "st_mtu5", "st_mtu6", "st_mtu7", "st_mtu8",
    ];

    /// <summary>
    /// 実物のiodefine.hからst_mtu(共通レジスタ)とst_mtu0〜st_mtu8(9チャネル)の
    /// 10構造体全てを同時にパースしてSVDドキュメントを生成でき、
    /// "MTU"・"MTU0"〜"MTU8"という10個のperipheralが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineMtu_GeneratesTenPeripheralInstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_MTU_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "MTU", "MTU0", "MTU1", "MTU2", "MTU3", "MTU4", "MTU5", "MTU6", "MTU7", "MTU8" }));
    }

    /// <summary>
    /// 実物のst_mtu0のTCNT(16ビット)が、Rx64mMtuDatasheetMetadataのメタデータ
    /// (リセット値0x0000・read-write)を反映して、SVDのaccess/resetValue要素に
    /// 正しく解決されることを確認する(resetMaskは全ビット既知のため出力されない)。
    /// </summary>
    [Test]
    public void Build_RealMtu0Tcnt_ResolvesDatasheetAccessAndResetValueAs16Bit() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_MTU_Pilot", "pilot", instances, structs, Rx64mMtuDatasheetMetadata.Registers);

        var mtu0 = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Single(p => p.Element("name")!.Value == "MTU0");
        var tcnt = mtu0.Element("registers")!.Elements("register")
            .Single(r => r.Element("name")!.Value == "TCNT");

        Assert.That(
            (tcnt.Element("access")!.Value, tcnt.Element("resetValue")!.Value, tcnt.Element("resetMask")),
            Is.EqualTo(("read-write", "0x0", (System.Xml.Linq.XElement?)null)));
    }

    /// <summary>
    /// 実物のst_mtu8のTCNT(32ビット、他チャネルと同名だが幅が異なる)が、
    /// Rx64mMtu8DatasheetMetadataの32ビット専用メタデータ(リセット値0x00000000・
    /// read-write)を反映して解決されることを確認する(resetMaskは全ビット既知の
    /// ため出力されない)。TCNT/TGRA〜TGRDはMTU0〜7では16ビットだがMTU8のみ
    /// 32ビットであり、フラットな名前ベース辞書ではこの幅の違いを1つの辞書で
    /// 正しく表現できないため、rx64m-mtu8という別プロバイダを用意している
    /// (Rx64mMtu8DatasheetMetadataのdocコメント参照)。
    /// </summary>
    [Test]
    public void Build_RealMtu8Tcnt_ResolvesDatasheetAccessAndResetValueAs32Bit() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_MTU_Pilot", "pilot", instances, structs, Rx64mMtu8DatasheetMetadata.Registers);

        var mtu8 = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Single(p => p.Element("name")!.Value == "MTU8");
        var tcnt = mtu8.Element("registers")!.Elements("register")
            .Single(r => r.Element("name")!.Value == "TCNT");

        Assert.That(
            (tcnt.Element("access")!.Value, tcnt.Element("resetValue")!.Value, tcnt.Element("resetMask")),
            Is.EqualTo(("read-write", "0x0", (System.Xml.Linq.XElement?)null)));
    }
}