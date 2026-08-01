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
        new("RX64M_WDT_Pilot", "rx64m-wdt", "rx64m-wdt-pilot.svd", new HashSet<string> { "st_wdt" }),
        new("RX64M_IWDT_Pilot", "rx64m-iwdt", "rx64m-iwdt-pilot.svd", new HashSet<string> { "st_iwdt" }),
        new("RX64M_MPU_Pilot", "rx64m-mpu", "rx64m-mpu-pilot.svd", new HashSet<string> { "st_mpu" }),
        new("RX64M_ECCRAM_Pilot", "rx64m-eccram", "rx64m-eccram-pilot.svd", new HashSet<string> { "st_eccram" }),
        new("RX64M_ELC_Pilot", "rx64m-elc", "rx64m-elc-pilot.svd", new HashSet<string> { "st_elc" }),
        new("RX64M_BSC_Pilot", "rx64m-bsc", "rx64m-bsc-pilot.svd", new HashSet<string> { "st_bsc" }),
        new("RX64M_PDC_Pilot", "rx64m-pdc", "rx64m-pdc-pilot.svd", new HashSet<string> { "st_pdc" }),
        new("RX64M_SSI_Pilot", "rx64m-ssi", "rx64m-ssi-pilot.svd", new HashSet<string> { "st_ssi" }),
        new("RX64M_SRC_Pilot", "rx64m-src", "rx64m-src-pilot.svd", new HashSet<string> { "st_src" }),
        new("RX64M_PPG_Pilot", "rx64m-ppg", "rx64m-ppg-pilot.svd", new HashSet<string> { "st_ppg0", "st_ppg1" }),
        new("RX64M_GPT_Pilot", "rx64m-gpt", "rx64m-gpt-pilot.svd", new HashSet<string> { "st_gpt", "st_gpt0" }),
        new("RX64M_RIIC_Pilot", "rx64m-riic", "rx64m-riic-pilot.svd", new HashSet<string> { "st_riic" }),
        new("RX64M_ETHERC_Pilot", "rx64m-etherc", "rx64m-etherc-pilot.svd", new HashSet<string> { "st_etherc" }),
        new("RX64M_MMCIF_Pilot", "rx64m-mmcif", "rx64m-mmcif-pilot.svd", new HashSet<string> { "st_mmcif" }),
        new("RX64M_RSPI_Pilot", "rx64m-rspi", "rx64m-rspi-pilot.svd", new HashSet<string> { "st_rspi" }),
        new("RX64M_SDHI_Pilot", "rx64m-sdhi", "rx64m-sdhi-pilot.svd", new HashSet<string> { "st_sdhi" }),
        new("RX64M_DMAC_Pilot", "rx64m-dmac", "rx64m-dmac-pilot.svd", new HashSet<string> { "st_dmac" }),
        new("RX64M_EXDMAC_Pilot", "rx64m-exdmac", "rx64m-exdmac-pilot.svd", new HashSet<string> { "st_exdmac" }),
        new("RX64M_POE_Pilot", "rx64m-poe", "rx64m-poe-pilot.svd", new HashSet<string> { "st_poe" }),
        new("RX64M_PTPEDMAC_Pilot", "rx64m-ptpedmac", "rx64m-ptpedmac-pilot.svd", new HashSet<string> { "st_ptpedmac" }),
        new("RX64M_TMR0_Pilot", "rx64m-tmr0", "rx64m-tmr0-pilot.svd", new HashSet<string> { "st_tmr0" }),
        new("RX64M_TMR01_Pilot", "rx64m-tmr01", "rx64m-tmr01-pilot.svd", new HashSet<string> { "st_tmr01" }),
        new("RX64M_TMR1_Pilot", "rx64m-tmr1", "rx64m-tmr1-pilot.svd", new HashSet<string> { "st_tmr1" }),
        new("RX64M_USB_Pilot", "rx64m-usb", "rx64m-usb-pilot.svd", new HashSet<string> { "st_usb" }),
        new("RX64M_USB0_Pilot", "rx64m-usb0", "rx64m-usb0-pilot.svd", new HashSet<string> { "st_usb0" }),
        new("RX64M_MPC_Pilot", "rx64m-mpc", "rx64m-mpc-pilot.svd", new HashSet<string> { "st_mpc" }),
        new("RX64M_DMAC0_Pilot", "rx64m-dmac0", "rx64m-dmac0-pilot.svd", new HashSet<string> { "st_dmac0" }),
        new("RX64M_DMAC1_Pilot", "rx64m-dmac1", "rx64m-dmac1-pilot.svd", new HashSet<string> { "st_dmac1" }),
        new("RX64M_DTC_Pilot", "rx64m-dtc", "rx64m-dtc-pilot.svd", new HashSet<string> { "st_dtc" }),
        new("RX64M_EDMAC_Pilot", "rx64m-edmac", "rx64m-edmac-pilot.svd", new HashSet<string> { "st_edmac" }),
        new("RX64M_EXDMAC0_Pilot", "rx64m-exdmac0", "rx64m-exdmac0-pilot.svd", new HashSet<string> { "st_exdmac0" }),
        new("RX64M_EXDMAC1_Pilot", "rx64m-exdmac1", "rx64m-exdmac1-pilot.svd", new HashSet<string> { "st_exdmac1" }),
        new("RX64M_RTC_Pilot", "rx64m-rtc", "rx64m-rtc-pilot.svd", new HashSet<string> { "st_rtc" }),
        new("RX64M_QSPI_Pilot", "rx64m-qspi", "rx64m-qspi-pilot.svd", new HashSet<string> { "st_qspi" }),
        new("RX64M_S12AD_Pilot", "rx64m-s12ad", "rx64m-s12ad-pilot.svd", new HashSet<string> { "st_s12ad", "st_s12ad1" }),
        new("RX64M_USBA_Pilot", "rx64m-usba", "rx64m-usba-pilot.svd", new HashSet<string> { "st_usba" }),
        new("RX64M_TPU_Pilot", "rx64m-tpu", "rx64m-tpu-pilot.svd", new HashSet<string> { "st_tpu0", "st_tpu1", "st_tpu2", "st_tpu3", "st_tpu4", "st_tpu5", "st_tpua" }),
        new("RX64M_MTU_Pilot", "rx64m-mtu", "rx64m-mtu-pilot.svd", new HashSet<string> {
            "st_mtu", "st_mtu0", "st_mtu1", "st_mtu2", "st_mtu3", "st_mtu4", "st_mtu5", "st_mtu6", "st_mtu7",
        }),
        new("RX64M_MTU8_Pilot", "rx64m-mtu8", "rx64m-mtu8-pilot.svd", new HashSet<string> { "st_mtu8" }),
        new("RX64M_EPTPC_Pilot", "rx64m-eptpc", "rx64m-eptpc-pilot.svd", new HashSet<string> { "st_eptpc", "st_eptpc0" }),
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
        ["rx64m-wdt"] = Rx64mWdtDatasheetMetadata.Registers,
        ["rx64m-iwdt"] = Rx64mIwdtDatasheetMetadata.Registers,
        ["rx64m-mpu"] = Rx64mMpuDatasheetMetadata.Registers,
        ["rx64m-eccram"] = Rx64mEccramDatasheetMetadata.Registers,
        ["rx64m-elc"] = Rx64mElcDatasheetMetadata.Registers,
        ["rx64m-bsc"] = Rx64mBscDatasheetMetadata.Registers,
        ["rx64m-pdc"] = Rx64mPdcDatasheetMetadata.Registers,
        ["rx64m-ssi"] = Rx64mSsiDatasheetMetadata.Registers,
        ["rx64m-src"] = Rx64mSrcDatasheetMetadata.Registers,
        ["rx64m-ppg"] = Rx64mPpgDatasheetMetadata.Registers,
        ["rx64m-gpt"] = Rx64mGptDatasheetMetadata.Registers,
        ["rx64m-riic"] = Rx64mRiicDatasheetMetadata.Registers,
        ["rx64m-etherc"] = Rx64mEthercDatasheetMetadata.Registers,
        ["rx64m-mmcif"] = Rx64mMmcifDatasheetMetadata.Registers,
        ["rx64m-rspi"] = Rx64mRspiDatasheetMetadata.Registers,
        ["rx64m-sdhi"] = Rx64mSdhiDatasheetMetadata.Registers,
        ["rx64m-dmac"] = Rx64mDmacDatasheetMetadata.Registers,
        ["rx64m-exdmac"] = Rx64mExdmacDatasheetMetadata.Registers,
        ["rx64m-poe"] = Rx64mPoeDatasheetMetadata.Registers,
        ["rx64m-ptpedmac"] = Rx64mPtpedmacDatasheetMetadata.Registers,
        ["rx64m-tmr0"] = Rx64mTmr0DatasheetMetadata.Registers,
        ["rx64m-tmr01"] = Rx64mTmr01DatasheetMetadata.Registers,
        ["rx64m-tmr1"] = Rx64mTmr1DatasheetMetadata.Registers,
        ["rx64m-usb"] = Rx64mUsbDatasheetMetadata.Registers,
        ["rx64m-usb0"] = Rx64mUsb0DatasheetMetadata.Registers,
        ["rx64m-mpc"] = Rx64mMpcDatasheetMetadata.Registers,
        ["rx64m-dmac0"] = Rx64mDmac0DatasheetMetadata.Registers,
        ["rx64m-dmac1"] = Rx64mDmac1DatasheetMetadata.Registers,
        ["rx64m-dtc"] = Rx64mDtcDatasheetMetadata.Registers,
        ["rx64m-edmac"] = Rx64mEdmacDatasheetMetadata.Registers,
        ["rx64m-exdmac0"] = Rx64mExdmac0DatasheetMetadata.Registers,
        ["rx64m-exdmac1"] = Rx64mExdmac1DatasheetMetadata.Registers,
        ["rx64m-rtc"] = Rx64mRtcDatasheetMetadata.Registers,
        ["rx64m-qspi"] = Rx64mQspiDatasheetMetadata.Registers,
        ["rx64m-s12ad"] = Rx64mS12adDatasheetMetadata.Registers,
        ["rx64m-usba"] = Rx64mUsbaDatasheetMetadata.Registers,
        ["rx64m-tpu"] = Rx64mTpuDatasheetMetadata.Registers,
        ["rx64m-mtu"] = Rx64mMtuDatasheetMetadata.Registers,
        ["rx64m-mtu8"] = Rx64mMtu8DatasheetMetadata.Registers,
        ["rx64m-eptpc"] = Rx64mEptpcDatasheetMetadata.Registers,
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