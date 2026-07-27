using System.Runtime.CompilerServices;
using System.Xml.Linq;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class RealIodefineIntegrationTest {
    static string GetRealIodefinePath([CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(
            testProjectDir,
            "..", "..", "..", "..", "..",
            "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));
    }

    /// <summary>
    /// リポジトリ実物のRX64M iodefine.hからCMT系struct(st_cmt/st_cmt0/st_cmtw)を
    /// 実際にパースしてSVDドキュメントを生成し、CMT0.CMCRのCKS/CMIEビット位置が
    /// iodefine.hの定義と一致することを確認する。
    /// </summary>
    [Test]
    public void Build_RealRx64mIodefine_GeneratesCmt0CmcrFieldsMatchingHeader() {
        var path = GetRealIodefinePath();
        var source = File.ReadAllText(path);
        var structNames = new HashSet<string> { "st_cmt", "st_cmt0", "st_cmtw" };

        var structs = IodefineHeaderParser.ParseStructs(source, structNames);
        var instances = IodefineHeaderParser.ParseInstances(source, structNames);
        var document = SvdDocumentBuilder.Build("RX64M_CMT_Pilot", "pilot", instances, structs);

        var cmt0Cmcr = document.Root!
            .Element("peripherals")!
            .Elements("peripheral")
            .Single(p => p.Element("name")!.Value == "CMT0")
            .Element("registers")!
            .Elements("register")
            .Single(r => r.Element("name")!.Value == "CMCR");

        var fields = cmt0Cmcr.Element("fields")!.Elements("field")
            .ToDictionary(
                f => f.Element("name")!.Value,
                f => (
                    BitOffset: int.Parse(f.Element("bitOffset")!.Value),
                    BitWidth: int.Parse(f.Element("bitWidth")!.Value)));

        Assert.That(fields["CKS"], Is.EqualTo((BitOffset: 0, BitWidth: 2)));
        Assert.That(fields["CMIE"], Is.EqualTo((BitOffset: 6, BitWidth: 1)));
    }
}