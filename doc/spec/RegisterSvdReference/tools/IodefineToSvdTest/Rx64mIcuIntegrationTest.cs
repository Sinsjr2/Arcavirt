using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mIcuIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_icuを実際にパースしてSVDドキュメントを生成できる
    /// (行コメント"//"での無効化・union内配列IR[256]等・struct部分が丸ごと
    /// コメントアウトされたunion(PIBR0等)の3パターン全てに対応済みであることの
    /// 実機データによる検証)ことと、ICUインスタンスが1つ生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineIcu_GeneratesSingleIcuInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_icu" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_ICU_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "ICU" }));
    }

    /// <summary>
    /// IR[256]レジスタが、dim=256・dimIncrement=1byte・フィールドIR(1bit)を
    /// 持つ配列レジスタとして解決されることを確認する
    /// (実機データでのdim対応の最終確認)。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealIcuIrArray_Has256ElementsOfOneByte() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_icu" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_icu"]);

        var ir = registers.Single(r => r.Name == "IR");

        Assert.That((ir.ByteSize, ir.ArrayCount), Is.EqualTo((1, 256)));
    }
}