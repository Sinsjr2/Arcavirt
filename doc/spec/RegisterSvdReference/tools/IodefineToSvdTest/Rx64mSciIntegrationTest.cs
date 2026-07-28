using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mSciIntegrationTest {
    static readonly HashSet<string> sciStructNames = ["st_sci0", "st_sci12", "st_smci0"];

    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// SCI0-7・SCI12・SMCI0-7,12の全18インスタンスについて、実物のiodefine.hから
    /// 例外を投げずにインスタンス一覧を抽出できることを確認する
    /// (st_sci0/st_sci12のTDRHL/RDRHLパターンにParseStructsが対応済みであることの
    /// 実機データによる検証)。
    /// </summary>
    [Test]
    public void ParseInstances_AllSciAndSmciStructNames_Produces18Instances() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var instances = IodefineHeaderParser.ParseInstances(source, sciStructNames);

        Assert.That(instances.Select(i => i.Name), Is.EquivalentTo(new[] {
            "SCI0", "SCI1", "SCI2", "SCI3", "SCI4", "SCI5", "SCI6", "SCI7", "SCI12",
            "SMCI0", "SMCI1", "SMCI2", "SMCI3", "SMCI4", "SMCI5", "SMCI6", "SMCI7", "SMCI12",
        }));
    }

    /// <summary>
    /// SMCI0はSCI0と同一ベースアドレス(スマートカードモード用の別ビュー)であるため、
    /// SMCI0のAlternatePeripheralがSCI0に設定され、SCI0自身は
    /// AlternatePeripheralを持たない(先発側であるため)ことを確認する。
    /// </summary>
    [Test]
    public void ParseInstances_Smci0SharesAddressWithSci0_SetsAlternatePeripheralOnSmciOnly() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var instances = IodefineHeaderParser.ParseInstances(source, sciStructNames);

        var sci0 = instances.Single(i => i.Name == "SCI0");
        var smci0 = instances.Single(i => i.Name == "SMCI0");

        Assert.That((sci0.AlternatePeripheral, smci0.AlternatePeripheral), Is.EqualTo(((string?)null, "SCI0")));
    }
}