using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mEptpcIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_eptpc", "st_eptpc0"];

    /// <summary>
    /// 実物のiodefine.hからst_eptpc(EPTPCの1インスタンスのみ)とst_eptpc0
    /// (EPTPC0・EPTPC1の2インスタンス)を同時にパースしてSVDドキュメントを生成でき、
    /// "EPTPC"・"EPTPC0"・"EPTPC1"という3つのperipheralが生成されることを確認する。
    /// st_eptpc0という型名から一見EPTPC/EPTPC0の2インスタンスかと誤解しやすいが、
    /// 実際にはst_eptpc0型がEPTPC0とEPTPC1の2つのベースアドレス(0xC4800/0xC4C00)に
    /// インスタンス化されており、EPTPC(st_eptpc型、0xC0500)と合わせて3インスタンス
    /// になる。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineEptpc_GeneratesEptpcEptpc0Eptpc1InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_EPTPC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "EPTPC", "EPTPC0", "EPTPC1" }));
    }

    /// <summary>
    /// 実物のst_eptpcのMIESR(MINT割り込み要因ステータスレジスタ)が、
    /// Rx64mEptpcDatasheetMetadataのメタデータ(リセット値0x00000000・read-write)を
    /// 反映して、SVDのaccess/resetValue要素に正しく解決されることを確認する
    /// (resetMaskは全ビット既知のため出力されない)。MIESRは抽出スクリプトが
    /// CYC0〜5ビットのR/W*1注記を見落としread-onlyと誤検出していた
    /// (本文のBit/Symbol表で正しくはCYC0〜5がR/W*1のためread-write)ため、
    /// 訂正内容を確認する回帰的な意味も持つ。
    /// </summary>
    [Test]
    public void Build_RealEptpcMiesr_ResolvesDatasheetAccessAndResetValueAsReadWrite() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build(
            "RX64M_EPTPC_Pilot", "pilot", instances, structs, Rx64mEptpcDatasheetMetadata.Registers);

        var eptpc = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Single(p => p.Element("name")!.Value == "EPTPC");
        var miesr = eptpc.Element("registers")!.Elements("register")
            .Single(r => r.Element("name")!.Value == "MIESR");

        Assert.That(
            (miesr.Element("access")!.Value, miesr.Element("resetValue")!.Value, miesr.Element("resetMask")),
            Is.EqualTo(("read-write", "0x0", (System.Xml.Linq.XElement?)null)));
    }
}