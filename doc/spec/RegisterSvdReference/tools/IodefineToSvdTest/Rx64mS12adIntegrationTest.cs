using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mS12adIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_s12ad", "st_s12ad1"];

    /// <summary>
    /// 実物のiodefine.hからst_s12ad(ユニット0)とst_s12ad1(ユニット1)を同時に
    /// パースしてSVDドキュメントを生成でき、"S12AD"と"S12AD1"という2つの
    /// peripheralが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineS12ad_GeneratesS12adAndS12ad1InstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_S12AD_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "S12AD", "S12AD1" }));
    }

    /// <summary>
    /// 実物のst_s12adのADRDが、RIGHT/LEFT(同一データの右詰め/左詰めの異なる解釈)
    /// を別名レジスタとしてではなく1つのレジスタとして解決し、
    /// AlternateRegisterがnullであることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealS12adAdrd_ResolvesAsSingleRegisterWithoutAlternateRegister() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_s12ad"]);

        var adrd = registers.Single(r => r.Name == "ADRD");
        Assert.That(adrd.AlternateRegister, Is.Null);
    }
}