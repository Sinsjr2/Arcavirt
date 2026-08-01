using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mRtcIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// 実物のiodefine.hからst_rtcを実際にパースしてSVDドキュメントを生成できることと、
    /// RTCは複数インスタンスではなく単一のペリフェラル("RTC")として生成されることを
    /// 確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineRtc_GeneratesSingleRtcInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_rtc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_RTC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "RTC" }));
    }

    /// <summary>
    /// 実物のst_rtcのRSECCNT/BCNT0が、同一アドレスオフセットを共有し、BCNT0側に
    /// AlternateRegister="RSECCNT"が設定された2つのレジスタとして解決されることを
    /// 確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealRtcRseccntBcnt0_ShareSameOffsetWithAlternateRegister() {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structNames = new HashSet<string> { "st_rtc" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_rtc"]);

        var rseccnt = registers.Single(r => r.Name == "RSECCNT");
        var bcnt0 = registers.Single(r => r.Name == "BCNT0");

        Assert.That((rseccnt.AddressOffset, bcnt0.AddressOffset, bcnt0.AlternateRegister), Is.EqualTo((rseccnt.AddressOffset, rseccnt.AddressOffset, "RSECCNT")));
    }
}