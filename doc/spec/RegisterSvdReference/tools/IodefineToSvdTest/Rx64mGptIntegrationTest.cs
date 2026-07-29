using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mGptIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_gpt", "st_gpt0"];

    /// <summary>
    /// 実物のiodefine.hからst_gpt・st_gpt0を実際にパースしてSVDドキュメントを
    /// 生成でき、共有制御ブロックGPTと4チャネル分(GPT0-GPT3)の計5インスタンスが
    /// 生成されることを確認する。GPTA(General PWM Timer)はデータシート本文
    /// (26.1節)に「four-channel 16-bit timer」と明記されている4チャネル構成であり、
    /// iodefine.h上もGPT0-GPT3の4インスタンスが定義されている。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineGpt_GeneratesGptAndFourChannelInstancesWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_GPT_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "GPT", "GPT0", "GPT1", "GPT2", "GPT3" }));
    }

    /// <summary>
    /// 実物のst_gpt(共有制御ブロック)が、11個のレジスタ(GTSTR/NFCR/GTHSCR/GTHCCR/
    /// GTHSSR/GTHPSR/GTWP/GTSYNC/GTETINT/GTBDR/GTSWP)として解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealGpt_Produces11Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_gpt"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "GTSTR", "NFCR", "GTHSCR", "GTHCCR", "GTHSSR", "GTHPSR", "GTWP", "GTSYNC", "GTETINT", "GTBDR", "GTSWP",
        }));
    }

    /// <summary>
    /// 実物のst_gpt0(チャネルごとのブロック)が、31個のレジスタとして解決される
    /// ことを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealGpt0_Produces31Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_gpt0"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "GTIOR", "GTINTAD", "GTCR", "GTBER", "GTUDC", "GTITC", "GTST", "GTCNT",
            "GTCCRA", "GTCCRB", "GTCCRC", "GTCCRD", "GTCCRE", "GTCCRF",
            "GTPR", "GTPBR", "GTPDBR",
            "GTADTRA", "GTADTBRA", "GTADTDBRA", "GTADTRB", "GTADTBRB", "GTADTDBRB",
            "GTONCR", "GTDTCR", "GTDVU", "GTDVD", "GTDBU", "GTDBD", "GTSOS", "GTSOTR",
        }));
    }
}