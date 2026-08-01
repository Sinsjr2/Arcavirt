using System.Runtime.CompilerServices;
using System.Xml.Linq;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mTpuIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_tpu0", "st_tpu1", "st_tpu2", "st_tpu3", "st_tpu4", "st_tpu5", "st_tpua"];

    /// <summary>
    /// 実物のiodefine.hからst_tpu0〜st_tpu5(6チャネル)とst_tpua(共通レジスタ)の
    /// 7構造体全てを同時にパースしてSVDドキュメントを生成でき、
    /// "TPU0"〜"TPU5"・"TPUA"という7個のperipheralが生成されることを確認する。
    /// なお、iodefine.hの#define群ではTPU0/TPU1、TPU2/TPU3、TPU4/TPU5がそれぞれ
    /// 同一ベースアドレスを共有しており、後発側がalternatePeripheralとして
    /// 関連付けられるが、それでも7個全てが個別のperipheral要素として
    /// 出力されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineTpu_GeneratesSevenPeripheralInstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_TPU_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "TPU0", "TPU1", "TPU2", "TPU3", "TPU4", "TPU5", "TPUA" }));
    }

    /// <summary>
    /// 実物のst_tpu0のTCRが、Rx64mTpuDatasheetMetadataのメタデータ(リセット値0x00・
    /// read-write)を反映して、SVDのaccess/resetValue要素に正しく解決されることを
    /// 確認する(resetMaskは全ビット既知のため出力されない)。
    /// </summary>
    [Test]
    public void Build_RealTpu0Tcr_ResolvesDatasheetAccessAndResetValue() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_TPU_Pilot", "pilot", instances, structs, Rx64mTpuDatasheetMetadata.Registers);

        var tpu0 = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Single(p => p.Element("name")!.Value == "TPU0");
        var tcr = tpu0.Element("registers")!.Elements("register")
            .Single(r => r.Element("name")!.Value == "TCR");

        Assert.That(
            (tcr.Element("access")!.Value, tcr.Element("resetValue")!.Value, tcr.Element("resetMask")),
            Is.EqualTo(("read-write", "0x0", (XElement?)null)));
    }
}