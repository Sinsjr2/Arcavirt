using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mPortIntegrationTest {
    static readonly HashSet<string> portStructNames = [
        "st_port0", "st_port1", "st_port2", "st_port3", "st_port4",
        "st_port5", "st_port6", "st_port7", "st_port8", "st_port9",
        "st_porta", "st_portb", "st_portc", "st_portd", "st_porte",
        "st_portf", "st_portg", "st_portj",
    ];

    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// PORT0とPORTEのODR0レジスタで、iodefine.hから自動抽出したフィールド名集合が
    /// 異なる(PORTEのみB3=PE1用ビットが存在する)ことを確認する。
    /// データシートのODR0記述にある「PORTE.ODR0以外は奇数ビットが予約」という
    /// 特記事項が、iodefine.h側にも反映されていることの実証。
    /// </summary>
    [Test]
    public void ParseStructs_Odr0OnPort0AndPortE_DiffersByPe1SpecialCase() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, portStructNames);

        var port0Odr0 = structs["st_port0"].Members.Single(m => m.Name == "ODR0");
        var porteOdr0 = structs["st_porte"].Members.Single(m => m.Name == "ODR0");

        Assert.That(
            (port0Odr0.Fields!.Select(f => f.Name), porteOdr0.Fields!.Select(f => f.Name)),
            Is.EqualTo((new[] { "B0", "B2", "B4", "B6" }, new[] { "B0", "B2", "B3", "B4", "B6" })));
    }

    /// <summary>
    /// 18ポート全てについて、iodefine.hから実際にSVDドキュメントを生成できる
    /// (レジスタ集合・ビットフィールドがポートごとに異なっていても、
    /// 既存のIodefineToSvdツールが無変更で動作する)ことを確認する。
    /// </summary>
    [Test]
    public void ParseInstances_AllPortStructNames_Produces18Instances() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var instances = IodefineHeaderParser.ParseInstances(source, portStructNames);

        Assert.That(instances.Select(i => i.Name), Is.EquivalentTo(new[] {
            "PORT0", "PORT1", "PORT2", "PORT3", "PORT4", "PORT5", "PORT6", "PORT7", "PORT8", "PORT9",
            "PORTA", "PORTB", "PORTC", "PORTD", "PORTE", "PORTF", "PORTG", "PORTJ",
        }));
    }
}