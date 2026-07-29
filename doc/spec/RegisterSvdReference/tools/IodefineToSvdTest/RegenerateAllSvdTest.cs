using System.Runtime.CompilerServices;
using System.Xml.Linq;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

/// <summary>
/// パーサー/ジェネレーター(CStructBodyParser・RegisterLayoutBuilder・
/// SvdDocumentBuilder等)を変更すると、既にコミット済みのsvdファイルの内容が
/// 気付かないうちに変わってしまうことがある(SvdXsdValidatorTestはディスク上の
/// スナップショットを読むだけで、ライブ生成結果とは比較していないため)。
/// このテストは、コミット済みの全モジュールについて実物のiodefine.hから
/// その場で再生成した結果が、ディスク上のsvdファイルと完全一致することを
/// 確認することで、この「スナップショットの陳腐化」を機械的に検出する。
/// 新しいモジュールを追加したら、このテーブルにも追加すること。
/// </summary>
public class RegenerateAllSvdTest {
    public record Module(string DeviceName, string DatasheetProviderName, string SvdFileName, IReadOnlySet<string> StructNames);

    static readonly IReadOnlyList<Module> modules = [
        new("RX64M_CMT_Pilot", "rx64m-cmt", "rx64m-cmt-pilot.svd", new HashSet<string> { "st_cmt", "st_cmt0", "st_cmtw" }),
        new("RX64M_PORT_Pilot", "rx64m-port", "rx64m-port-pilot.svd", new HashSet<string> {
            "st_port0", "st_port1", "st_port2", "st_port3", "st_port4",
            "st_port5", "st_port6", "st_port7", "st_port8", "st_port9",
            "st_porta", "st_portb", "st_portc", "st_portd", "st_porte",
            "st_portf", "st_portg", "st_portj",
        }),
        new("RX64M_SCI_Pilot", "rx64m-sci", "rx64m-sci-pilot.svd", new HashSet<string> { "st_sci0", "st_sci12", "st_smci0" }),
        new("RX64M_ICU_Pilot", "rx64m-icu", "rx64m-icu-pilot.svd", new HashSet<string> { "st_icu" }),
        new("RX64M_SYSTEM_Pilot", "rx64m-system", "rx64m-system-pilot.svd", new HashSet<string> { "st_system" }),
        new("RX64M_SCIFA_Pilot", "rx64m-scifa", "rx64m-scifa-pilot.svd", new HashSet<string> { "st_scifa" }),
        new("RX64M_CAC_Pilot", "rx64m-cac", "rx64m-cac-pilot.svd", new HashSet<string> { "st_cac" }),
        new("RX64M_CRC_Pilot", "rx64m-crc", "rx64m-crc-pilot.svd", new HashSet<string> { "st_crc" }),
        new("RX64M_DA_Pilot", "rx64m-da", "rx64m-da-pilot.svd", new HashSet<string> { "st_da" }),
        new("RX64M_DOC_Pilot", "rx64m-doc", "rx64m-doc-pilot.svd", new HashSet<string> { "st_doc" }),
        new("RX64M_TEMPS_Pilot", "rx64m-temps", "rx64m-temps-pilot.svd", new HashSet<string> { "st_temps" }),
    ];

    static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, RegisterDatasheetMetadata>> datasheetProviders = new Dictionary<string, IReadOnlyDictionary<string, RegisterDatasheetMetadata>> {
        ["rx64m-cmt"] = Rx64mCmtDatasheetMetadata.Registers,
        ["rx64m-port"] = Rx64mPortDatasheetMetadata.Registers,
        ["rx64m-sci"] = Rx64mSciDatasheetMetadata.Registers,
        ["rx64m-icu"] = Rx64mIcuDatasheetMetadata.Registers,
        ["rx64m-system"] = Rx64mSystemDatasheetMetadata.Registers,
        ["rx64m-scifa"] = Rx64mScifaDatasheetMetadata.Registers,
        ["rx64m-cac"] = Rx64mCacDatasheetMetadata.Registers,
        ["rx64m-crc"] = Rx64mCrcDatasheetMetadata.Registers,
        ["rx64m-da"] = Rx64mDaDatasheetMetadata.Registers,
        ["rx64m-doc"] = Rx64mDocDatasheetMetadata.Registers,
        ["rx64m-temps"] = Rx64mTempsDatasheetMetadata.Registers,
    };

    static string GetRepoPath(string relativePath, [CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        var repoRoot = Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "..", "..", ".."));
        return Path.Combine(repoRoot, relativePath);
    }

    static string GetRealIodefinePath() => GetRepoPath(Path.Combine(
        "RenesasRXNative", "test", "src", "smc_gen", "r_bsp", "mcu", "rx64m", "register_access", "gnuc", "iodefine.h"));

    static IEnumerable<Module> ModuleTestCases => modules;

    /// <summary>
    /// 各モジュールについて実物のiodefine.hからその場で再生成したSVDドキュメントが、
    /// コミット済みのsvdファイルと構造的に完全一致することを確認する
    /// (パーサー/ジェネレーター変更後の再生成忘れ・意図しない副作用の検出)。
    /// </summary>
    [TestCaseSource(nameof(ModuleTestCases))]
    public void Build_CommittedModule_MatchesCommittedSvdFile(Module module) {
        var source = File.ReadAllText(GetRealIodefinePath());
        var structs = IodefineHeaderParser.ParseStructs(source, module.StructNames);
        var instances = IodefineHeaderParser.ParseInstances(source, module.StructNames);
        var datasheetMetadata = datasheetProviders[module.DatasheetProviderName];

        var regenerated = SvdDocumentBuilder.Build(
            module.DeviceName,
            $"{module.DeviceName} peripheral registers generated from iodefine.h (pilot).",
            instances,
            structs,
            datasheetMetadata);

        var committed = XDocument.Load(GetRepoPath(Path.Combine("doc", "spec", "RegisterSvdReference", module.SvdFileName)));

        Assert.That(XNode.DeepEquals(regenerated, committed), Is.True,
            $"{module.SvdFileName}の再生成結果がコミット済みファイルと一致しません。パーサー/ジェネレーター変更後の再生成を忘れていないか確認してください。");
    }
}