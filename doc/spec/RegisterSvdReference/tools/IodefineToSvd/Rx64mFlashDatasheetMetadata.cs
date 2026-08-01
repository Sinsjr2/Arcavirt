namespace IodefineToSvd;

/// <summary>
/// RX64M Group, RX71M Group Flash Memory User's Manual: Hardware Interface
/// (R01UH0435EJ0121 Rev.1.21, Oct 28, 2022)の4章(Registers、4.1〜4.19、p.10-30)
/// から手動で書き写したレジスタごとのリセット値・アクセス権限。
///
/// RX64M Group User's Manual: Hardware(本プロジェクトの他モジュールが参照している
/// メインのハードウェアマニュアル)の63章(Flash Memory)は、FACI制御レジスタ
/// (本ファイルが扱う19レジスタ)について「Refer to Flash Memory User's Manual:
/// Hardware Interface」とだけ記載しビット構成・リセット値を記載していないため、
/// このFlash専用マニュアル(別途Webから取得)を情報源とした。st_flashの全19
/// レジスタ(iodefine.hの6327〜6641行目)がこのマニュアルの4.1〜4.19節と1対1で
/// 一致することを確認済み。
///
/// FCMDR(4.13)は本文のBit/Symbol表でPCMDR[7:0]・CMDR[7:0]がいずれも"R"のみ
/// (書き込み不可)であるため、他のレジスタと異なりread-onlyとした(抽出スクリプトの
/// 候補もread-onlyで一致)。リセット後の値は全ビット1(0xFFFF、コマンド未受信を
/// 示す既定パターン)。
///
/// FPCKAR(4.19)のPCKA[7:0](b7-b0)は、"Value after reset"の表記が"0/1"という
/// 特殊な形式になっている。これは本文に「The initial value is set to the maximum
/// operating frequency of the FCLK」と説明されている通り、製品・動作条件に応じて
/// 変わる値であり、単一の固定値としては表現できないため、単なる"x"(不定)とは
/// 区別しつつも、本メタデータではResetMaskから除外して扱った(KEY[7:0]側は
/// 「書き込み値は保持されず常に0として読み出される」と明記されているため0固定・
/// 既知として扱う)。
///
/// FSADDR/FEADDR(4.5/4.6)・FCURAME/FENTRYR/FPROTR/FSUINITR/FPCKAR等に頻出する
/// 「R/W*n」(Note参照、FRDY=1の時のみ書き込み可・特定のKEYコード書き込み時のみ
/// 有効・書き込み値が保持されずreadは常に0等)は、いずれも本文中に固定の
/// リセット値(大半は0)が明記されているため、Note の内容自体はAccess判定
/// (read-write/read-only)には影響させず、規約通り「ビット単位でR/(W)・R・R/Wが
/// 混在する場合はレジスタ全体をread-writeとして扱う」ルールに従った。
/// </summary>
public static class Rx64mFlashDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 4.1 Flash P/E Protect Register (FWEPROR)
        ["FWEPROR"] = new RegisterDatasheetMetadata(0x02, 0xFF, "read-write"),
        // 4.2 Flash Access Status Register (FASTAT) - ECRCT/CMDLKはR、DFAE/CFAEはR/W*1、予約ビットはR/W
        ["FASTAT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 4.3 Flash Access Error Interrupt Enable Register (FAEINT)
        ["FAEINT"] = new RegisterDatasheetMetadata(0x99, 0xFF, "read-write"),
        // 4.4 Flash Ready Interrupt Enable Register (FRDYIE)
        ["FRDYIE"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 4.5 FACI Command Start Address Register (FSADDR) - 32bit、b0/b1のみread-onlyだがレジスタ全体はread-write
        ["FSADDR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 4.6 FACI Command End Address Register (FEADDR) - 32bit
        ["FEADDR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 4.7 FCURAM Enable Register (FCURAME)
        ["FCURAME"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 4.8 Flash Status Register (FSTATR) - 32bit、FRDY(b15)のみリセット後1、他は0
        ["FSTATR"] = new RegisterDatasheetMetadata(0x00008000, 0xFFFFFFFF, "read-write"),
        // 4.9 Flash P/E Mode Entry Register (FENTRYR)
        ["FENTRYR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 4.10 Flash Protection Register (FPROTR)
        ["FPROTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 4.11 Flash Sequencer Set-Up Initialization Register (FSUINITR)
        ["FSUINITR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 4.12 Lock Bit Status Register (FLKSTAT)
        ["FLKSTAT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 4.13 FACI Command Register (FCMDR) - 全ビットRのみのためread-only、リセット後は全ビット1
        ["FCMDR"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-only"),
        // 4.14 Flash P/E Status Register (FPESTAT)
        ["FPESTAT"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 4.15 Data Flash Blank Check Control Register (FBCCNT)
        ["FBCCNT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 4.16 Data Flash Blank Check Status Register (FBCSTAT)
        ["FBCSTAT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 4.17 Data Flash Programming Start Address Register (FPSADDR) - 32bit、PSADR[18:0]はR、予約ビットはR/W
        ["FPSADDR"] = new RegisterDatasheetMetadata(0x00008000, 0xFFFFFFFF, "read-write"),
        // 4.18 Flash Sequencer Processing Switching Register (FCPSR)
        ["FCPSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 4.19 Flash Sequencer Processing Clock Frequency Notification Register (FPCKAR)
        // PCKA[7:0](b7-b0)は動作周波数依存で単一値を持たないためResetMaskから除外(docコメント参照)。
        ["FPCKAR"] = new RegisterDatasheetMetadata(0x0000, 0xFF00, "read-write"),
    };
}