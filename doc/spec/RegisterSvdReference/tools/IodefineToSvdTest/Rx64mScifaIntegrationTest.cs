using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mScifaIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_scifaを実際にパースしてSVDドキュメントを生成できる
    /// (BRR/MDDRの無名エイリアスunionパターンに対応済みであることの実機データによる
    /// 検証)ことと、SCIFA8-11の4インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineScifa_GeneratesFourScifaInstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_scifa" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_SCIFA_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "SCIFA8", "SCIFA9", "SCIFA10", "SCIFA11" }));
    }

    /// <summary>
    /// 実物のst_scifaのBRR/MDDRが、同一アドレスオフセットを共有し、MDDR側に
    /// AlternateRegister="BRR"が設定された2つのレジスタとして解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealScifaBrrMddr_ShareSameOffsetWithAlternateRegister() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_scifa" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_scifa"]);

        var brr = registers.Single(r => r.Name == "BRR");
        var mddr = registers.Single(r => r.Name == "MDDR");

        Assert.That((brr.AddressOffset, mddr.AddressOffset, mddr.AlternateRegister), Is.EqualTo((brr.AddressOffset, brr.AddressOffset, "BRR")));
    }
}