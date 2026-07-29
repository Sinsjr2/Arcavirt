namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 26章(General PWM Timer (GPTA)、p.1141-1176の26.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。GPTA全体は4チャネル
/// 構成(GPT0-GPT3)であり、iodefine.h上ではGPT0(0x000C 2100)・GPT1(0x000C 2180)・
/// GPT2(0x000C 2200)・GPT3(0x000C 2280)の4インスタンスが定義されている(共有制御
/// ブロックGPT(0x000C 2000)を含めて5インスタンス)。データシートの各レジスタは
/// GPTn(n = 0 to 3)という表記でGPT0-GPT3共通の1つの表として記載されており、
/// 代表的なレジスタ(GTSTR/GTHSCR/GTIOR/GTCR/GTONCR等)についてチャネル間で
/// リセット値・アクセス権限に差異がないことを確認済み(2026-07-29)。
///
/// iodefine.h上のst_gptは共有制御ブロック(GPT、11レジスタ: GTSTR/NFCR/GTHSCR/
/// GTHCCR/GTHSSR/GTHPSR/GTWP/GTSYNC/GTETINT/GTBDR/GTSWP)、st_gpt0はチャネルごとの
/// ブロック(GPT0-GPT3、31レジスタ)である。両struct間でレジスタ名の重複はない。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとにR/R(/W)混在だが
/// read-writeとして扱った:
/// - GTHCCR: CCSWn(GPTn.GTCNT Counter Clear、書き込みで1、自動的に0へ戻り「読み出しは
///   0」)はビット説明表上R/Wと明記されているため、他ビットと合わせてread-writeとした
/// - GTST: TUCF/ITCNT[2:0]/DTEF(状態フラグ)はR、予約ビット(b14-b12)はR/W
///
/// GTETINT(b9, b8)は、リセット値表で"x: Undefined"と明記され、ビット説明表でも
/// 「The read value is undefined」と記載されているため、「値+部分マスク」で表現した
/// (ResetValue=0x0000、ResetMask=0xFCFF、b9-b8を除外)。
///
/// GTST(b7-b0)も同様に、リセット値表で"x: Undefined"、ビット説明表で「The read value
/// is undefined」と明記されているため、「値+部分マスク」で表現した
/// (ResetValue=0x8000、ResetMask=0xFF00、b7-b0を除外)。TUCF(b15)=1がリセット値。
///
/// GTSOS(b9, b8)も同様の理由で「値+部分マスク」とした(ResetValue=0x0000、
/// ResetMask=0xFCFF、b9-b8を除外)。GTSOSは全ビットがR(読み出し専用のステータス
/// レジスタ)のため、Accessはread-onlyとした。
/// </summary>
public static class Rx64mGptDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // --- st_gpt (GPT共有制御ブロック) ---
        // 26.2.1 General PWM Timer Software Start Register (GTSTR)
        ["GTSTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.2 Noise Filter Control Register (NFCR)
        ["NFCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.3 General PWM Timer Hardware Source Start/Stop Control Register (GTHSCR)
        ["GTHSCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.4 General PWM Timer Hardware Source Clear Control Register (GTHCCR)
        ["GTHCCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.5 General PWM Timer Hardware Start Source Select Register (GTHSSR)
        ["GTHSSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.6 General PWM Timer Hardware Stop/Clear Source Select Register (GTHPSR)
        ["GTHPSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.7 General PWM Timer Write-Protection Register (GTWP)
        ["GTWP"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.8 General PWM Timer Sync Register (GTSYNC)
        ["GTSYNC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.9 General PWM Timer External Trigger Input Interrupt Register (GTETINT)
        // b9, b8はUndefinedのため部分マスクで除外
        ["GTETINT"] = new RegisterDatasheetMetadata(0x0000, 0xFCFF, "read-write"),
        // 26.2.10 General PWM Timer Buffer Operation Disable Register (GTBDR)
        ["GTBDR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.11 General PWM Timer Start Write-Protection Register (GTSWP)
        ["GTSWP"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // --- st_gpt0 (GPTnチャネルごとのブロック、n = 0 to 3) ---
        // 26.2.12 General PWM Timer I/O Control Register (GTIOR)
        ["GTIOR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.13 General PWM Timer Interrupt Output Setting Register (GTINTAD)
        ["GTINTAD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.14 General PWM Timer Control Register (GTCR)
        ["GTCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.15 General PWM Timer Buffer Enable Register (GTBER)
        ["GTBER"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.16 General PWM Timer Count Direction Register (GTUDC) - UD(b0)=1がリセット値
        ["GTUDC"] = new RegisterDatasheetMetadata(0x0001, 0xFFFF, "read-write"),
        // 26.2.17 General PWM Timer Interrupt and A/D Converter Start Request Skipping Setting Register (GTITC)
        ["GTITC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.18 General PWM Timer Status Register (GTST) - TUCF(b15)=1がリセット値
        // b7-b0はUndefinedのため部分マスクで除外
        ["GTST"] = new RegisterDatasheetMetadata(0x8000, 0xFF00, "read-write"),
        // 26.2.19 General PWM Timer Counter (GTCNT)
        ["GTCNT"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.20 General PWM Timer Compare Capture Register m (GTCCRm) (m = A to F)
        ["GTCCRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTCCRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTCCRC"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTCCRD"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTCCRE"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTCCRF"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.21 General PWM Timer Period Setting Register (GTPR)
        ["GTPR"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.22 General PWM Timer Period Setting Buffer Register (GTPBR)
        ["GTPBR"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.23 General PWM Timer Period Setting Double Buffer Register (GTPDBR)
        ["GTPDBR"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.24 A/D Converter Start Request Timing Register m (GTADTRm) (m = A, B)
        ["GTADTRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTADTRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.25 A/D Converter Start Request Timing Buffer Register m (GTADTBRm) (m = A, B)
        ["GTADTBRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTADTBRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.26 A/D Converter Start Request Timing Double Buffer Register m (GTADTDBRm) (m = A, B)
        ["GTADTDBRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTADTDBRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.27 General PWM Timer Output Negate Control Register (GTONCR) - NFV(b8)=1がリセット値
        ["GTONCR"] = new RegisterDatasheetMetadata(0x0100, 0xFFFF, "read-write"),
        // 26.2.28 General PWM Timer Dead Time Control Register (GTDTCR)
        ["GTDTCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 26.2.29 General PWM Timer Dead Time Value Register m (GTDVm) (m = U, D)
        ["GTDVU"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTDVD"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.30 General PWM Timer Dead Time Buffer Register m (GTDBm) (m = U, D)
        ["GTDBU"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["GTDBD"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        // 26.2.31 General PWM Timer Output Protection Function Status Register (GTSOS) - read-only
        // b9, b8はUndefinedのため部分マスクで除外
        ["GTSOS"] = new RegisterDatasheetMetadata(0x0000, 0xFCFF, "read-only"),
        // 26.2.32 General PWM Timer Output Protection Function Temporary Release Register (GTSOTR)
        ["GTSOTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
    };
}