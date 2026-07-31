using System.Runtime.CompilerServices;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class Rx64mMpcIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    static readonly HashSet<string> structNames = ["st_mpc"];

    /// <summary>
    /// 実物のiodefine.hからst_mpcを実際にパースしてSVDドキュメントを生成でき、
    /// MPCの1インスタンスが生成されることを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefineMpc_GeneratesMpcInstanceWithoutException() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_MPC_Pilot", "pilot", instances, structs);

        var peripheralNames = document.Root!.Element("peripherals")!.Elements("peripheral")
            .Select(p => p.Element("name")!.Value);

        Assert.That(peripheralNames, Is.EqualTo(new[] { "MPC" }));
    }

    /// <summary>
    /// 実物のst_mpcが、128個のレジスタ(PFCSE-PJ5PFS、アドレスオフセット順)として
    /// 解決されることを確認する。
    /// </summary>
    [Test]
    public void ResolveRegisterLayout_RealMpc_Produces128Registers() {
        var source = File.ReadAllText(GetRealIodefinePath());

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var registers = RegisterLayoutBuilder.Resolve(structs["st_mpc"]);

        Assert.That(registers.Select(r => r.Name), Is.EqualTo(new[] {
            "PFCSE", "PFCSS0", "PFCSS1", "PFAOE0", "PFAOE1", "PFBCR0", "PFBCR1", "PFENET", "PWPR",
            "P00PFS", "P01PFS", "P02PFS", "P03PFS", "P05PFS", "P07PFS",
            "P10PFS", "P11PFS", "P12PFS", "P13PFS", "P14PFS", "P15PFS", "P16PFS", "P17PFS",
            "P20PFS", "P21PFS", "P22PFS", "P23PFS", "P24PFS", "P25PFS", "P26PFS", "P27PFS",
            "P30PFS", "P31PFS", "P32PFS", "P33PFS", "P34PFS",
            "P40PFS", "P41PFS", "P42PFS", "P43PFS", "P44PFS", "P45PFS", "P46PFS", "P47PFS",
            "P50PFS", "P51PFS", "P52PFS", "P54PFS", "P55PFS", "P56PFS",
            "P60PFS", "P66PFS", "P67PFS",
            "P71PFS", "P72PFS", "P73PFS", "P74PFS", "P75PFS", "P76PFS", "P77PFS",
            "P80PFS", "P81PFS", "P82PFS", "P83PFS", "P86PFS", "P87PFS",
            "P90PFS", "P91PFS", "P92PFS", "P93PFS", "P94PFS", "P95PFS", "P96PFS", "P97PFS",
            "PA0PFS", "PA1PFS", "PA2PFS", "PA3PFS", "PA4PFS", "PA5PFS", "PA6PFS", "PA7PFS",
            "PB0PFS", "PB1PFS", "PB2PFS", "PB3PFS", "PB4PFS", "PB5PFS", "PB6PFS", "PB7PFS",
            "PC0PFS", "PC1PFS", "PC2PFS", "PC3PFS", "PC4PFS", "PC5PFS", "PC6PFS", "PC7PFS",
            "PD0PFS", "PD1PFS", "PD2PFS", "PD3PFS", "PD4PFS", "PD5PFS", "PD6PFS", "PD7PFS",
            "PE0PFS", "PE1PFS", "PE2PFS", "PE3PFS", "PE4PFS", "PE5PFS", "PE6PFS", "PE7PFS",
            "PF0PFS", "PF1PFS", "PF2PFS", "PF5PFS",
            "PG0PFS", "PG1PFS", "PG2PFS", "PG3PFS", "PG4PFS", "PG5PFS", "PG6PFS", "PG7PFS",
            "PJ3PFS", "PJ5PFS",
        }));
        Assert.That(registers, Has.Count.EqualTo(128));
    }
}