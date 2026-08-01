namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 24章(Multi-Function Timer Pulse Unit 3 (MTU3a)、24.2 Register Descriptions、
/// 24.2.1〜24.2.42、p.856-932)から手動で書き写したレジスタごとのリセット値・
/// アクセス権限。
///
/// RX64MのMTU3aはMTU(共通レジスタ)/MTU0〜MTU8の10チャンネル(iodefine.hでは
/// st_mtu/st_mtu0〜st_mtu8の10構造体)から成る。データシートはTCR・TMDR1・TIOR・
/// TIER・TSR・NFCRnなど多くのレジスタを複数チャンネルにまたがる1つの節で
/// まとめて説明しており(節ごとに"MTUn.xxx"のアドレスが並記される)、
/// Address(es)欄に複数チャンネルが列挙されている場合はビット構成・Value after
/// resetが1つの図・1つのビット表として共有されている。このファイルはレジスタ名を
/// キーとする単一の辞書を提供し、Program.csのdatasheetProvidersには
/// st_mtu/st_mtu0〜st_mtu8の10構造体すべてに対して同じ辞書を割り当てる設計とする。
/// チャンネルごとにビットの意味(シンボル名)は異なっていても、リセット値・
/// ResetMask・アクセス権限が節内で明示的に同一と確認できたレジスタ名のみ、
/// この辞書に単一エントリとして登録した。
///
/// ●●● TCNT・TGRA・TGRB・TGRC・TGRDの幅違い(TMR0/TMR01/TMR1と同じ対応方針) ●●●
/// データシート24.2.13(TCNT)・24.2.15(TGRm)は、MTU0〜MTU4,MTU6,MTU7の
/// TCNT/TGRA/TGRB/TGRC/TGRDを16ビットレジスタ(リセット値TCNT=0000h、
/// TGRm=FFFFh)、MTU8のTCNT/TGRA/TGRB/TGRC/TGRDを32ビットレジスタ
/// (リセット値TCNT=00000000h、TGRm=FFFFFFFFh)と明記しており、iodefine.hでも
/// st_mtu0〜st_mtu4/st_mtu6/st_mtu7ではunsigned short(2バイト)、st_mtu8では
/// unsigned long(4バイト)とメンバー型が異なる。レジスタ名だけをキーとする
/// フラットな辞書では、この2つの幅を同時に正しく表現できない(SvdDocumentBuilder.
/// BuildRegisterElementはregister.ByteSizeからfullMaskを計算し、
/// datasheetMetadataのResetMaskと比較して<resetMask>要素の要否を決めるため、
/// 幅の異なるレジスタに同じエントリを適用すると誤ったResetValue/ResetMaskを
/// 出力してしまう)。
/// このファイル(Rx64mMtuDatasheetMetadata)は16ビット版(MTU0〜4,6,7,共通MTU)
/// 向けの辞書とし、TCNT/TGRA/TGRB/TGRC/TGRDを16ビット値で登録する。MTU8
/// (32ビット版)向けには、既存のTMR0/TMR01/TMR1と同じ「幅ごとに別プロバイダ」
/// 方針に倣い、Rx64mMtu8DatasheetMetadata(このファイルの辞書をベースに5
/// レジスタだけ32ビット値で上書きする)を別途用意し、Program.csで
/// st_mtu8には"rx64m-mtu8"、それ以外のMTU構造体には"rx64m-mtu"を割り当てる。
///
/// ●●● JSONL自動抽出結果からの訂正 ●●●
/// TCSYSTR(24.2.19)は抽出スクリプトの候補でaccess_candidate="read-only"と
/// 判定されていたが、実際のビット表では8ビット中7ビット(SCH0〜SCH4,SCH6,SCH7)が
/// "R/(W)*1"(1のみ書き込み可能、対応するカウンタ起動で自動クリア)、残り1ビット
/// (b2予約)のみ"R"である。1ビットでも書込み可能なビットがあれば規約上
/// read-writeに集約するため、read-writeに訂正した(DMSTS等で確認済みの既知の
/// 誤検出パターンと同型)。
///
/// ●●● 判断に迷った箇所 ●●●
/// TITCNT1A/TITCNT1B(24.2.40)は、本文中に「TITCNT1AとTITCNT1Bは8ビットの
/// リード/ライト可能なカウンタである」という記述があるが、ビット単位のR/W表では
/// 全ビットが"R"のみで、書き込み可能を示す記述はどこにもない(クリアは
/// TITCR1m.T3AEN/T4VEN等の間接操作でのみ行われる)。ビット表を優先しread-only
/// として扱った。なお同じ構造を持つTITCNT2A/TITCNT2B(24.2.42)にはこの
/// 「リード/ライト可能」という記述自体が存在せず(単に「値をセットし
/// カウントダウンする」という説明のみ)、矛盾なくread-onlyとして扱える。
///
/// TCNTSA/TCNTSB(24.2.26)はビット表を持たないプレーンなデータレジスタだが、
/// 本文冒頭に"TCNTSA and TCNTSB are 16-bit read-only counters"と明示されている
/// ためread-onlyとした(ADDRy等と同じ、本文の明示を優先するパターン)。
///
/// TSR(24.2.9)はbit6が「読み出すと1」・bit7(TCFD)が実際のカウント方向を示す
/// フラグで、"Value after reset"図はbit6=1・bit7=1と明記している。bit0〜5は
/// 本文に"The read value is undefined."と明記されているためResetMaskから除外し
/// (0xC0)、bit6・bit7は図の記載どおり既知として扱った(bit7の値がリセット後に
/// 本当に1になるかは、TCFDの意味の説明文自体には記載がなく図のみが根拠だが、
/// 他ビットのような"undefined"の注記が無いため図を採用した)。
/// </summary>
public static class Rx64mMtuDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 24.2.1 Timer Control Register (TCR) / MTU5.TCRU,TCRV,TCRW
        ["TCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TCRU"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TCRV"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TCRW"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.2 Timer Control Register 2 (TCR2) / MTU5.TCR2U,TCR2V,TCR2W
        ["TCR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TCR2U"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TCR2V"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TCR2W"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.3 Timer Mode Register 1 (TMDR1)
        ["TMDR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.4 Timer Mode Register 2m (TMDR2m) (m = A, B) - MTU共通
        ["TMDR2A"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TMDR2B"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.5 Timer Mode Register 3 (TMDR3) - MTU1のみ。PHCKSEL=1が既定値。
        ["TMDR3"] = new RegisterDatasheetMetadata(0x02, 0xFF, "read-write"),

        // 24.2.6 Timer I/O Control Register (TIOR) / MTU5.TIORU,TIORV,TIORW
        ["TIORH"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIORL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIOR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIORU"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIORV"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIORW"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.7 Timer Compare Match Clear Register (TCNTCMPCLR) - MTU5のみ。
        ["TCNTCMPCLR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.8 Timer Interrupt Enable Register (TIER) / MTU0.TIER2
        ["TIER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIER2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.9 Timer Status Register (TSR) - bit0〜5は読み出し不定(undefined)のためResetMaskから除外。
        ["TSR"] = new RegisterDatasheetMetadata(0xC0, 0xC0, "read-write"),

        // 24.2.10 Timer Buffer Operation Transfer Mode Register (TBTM)
        ["TBTM"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.11 Timer Input Capture Control Register (TICCR) - MTU1のみ。
        ["TICCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.12 Timer Synchronous Clear Register (TSYCR) - MTU6のみ。
        ["TSYCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.13 Timer Counter (TCNT) - MTU0〜4,6,7,共通MTUは16bit。MTU8(32bit)はRx64mMtu8DatasheetMetadata参照。
        ["TCNT"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["TCNTU"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["TCNTV"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["TCNTW"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 24.2.14 Timer Longword Counter (TCNTLW) - MTU1のみ(32bit)。
        ["TCNTLW"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),

        // 24.2.15 Timer General Register m (TGRm) - MTU0〜4,6,7,共通MTUは16bit。MTU8(32bit)はRx64mMtu8DatasheetMetadata参照。
        ["TGRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRC"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRD"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRE"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRF"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRU"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRV"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRW"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),

        // 24.2.16 Timer Longword General Register m (TGRmLW) (m = A, B) - MTU1のみ(32bit)。
        ["TGRALW"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
        ["TGRBLW"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),

        // 24.2.17 Timer Start Registers (TSTRA, TSTRB, TSTR) - TSTRA/TSTRBはMTU共通、TSTRはMTU5のみ。
        ["TSTRA"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TSTRB"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TSTR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.18 Timer Synchronous Register m (TSYRm) (m = A, B) - MTU共通。
        ["TSYRA"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TSYRB"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.19 Timer Counter Synchronous Start Register (TCSYSTR) - MTU共通。
        // JSONL抽出候補はread-onlyだったが、7/8ビットがR/(W)*1のためread-writeに訂正(docコメント参照)。
        ["TCSYSTR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.20 Timer Read/Write Enable Register m (TRWERm) (m = A, B) - MTU共通。
        ["TRWERA"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        ["TRWERB"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),

        // 24.2.21 Timer Output Master Enable Register m (TOERm) (m = A, B) - MTU共通。
        ["TOERA"] = new RegisterDatasheetMetadata(0xC0, 0xFF, "read-write"),
        ["TOERB"] = new RegisterDatasheetMetadata(0xC0, 0xFF, "read-write"),

        // 24.2.22 Timer Output Control Register 1m (TOCR1m) (m = A, B) - MTU共通。
        ["TOCR1A"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TOCR1B"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.23 Timer Output Control Register 2m (TOCR2m) (m = A, B) - MTU共通。
        ["TOCR2A"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TOCR2B"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.24 Timer Output Level Buffer Register m (TOLBRm) (m = A, B) - MTU共通。
        ["TOLBRA"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TOLBRB"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.25 Timer Gate Control Register A (TGCRA) - MTU共通。bit7予約は"read as 1"。
        ["TGCRA"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),

        // 24.2.26 Timer Subcounter m (TCNTSm) (m = A, B) - MTU共通。本文冒頭でread-onlyと明示。
        ["TCNTSA"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["TCNTSB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),

        // 24.2.27 Timer Period Data Register m (TCDRm) (m = A, B) - MTU共通。
        ["TCDRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TCDRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),

        // 24.2.28 Timer Period Buffer Register m (TCBRm) (m = A, B) - MTU共通。
        ["TCBRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TCBRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),

        // 24.2.29 Timer Dead Time Data Register m (TDDRm) (m = A, B) - MTU共通。
        ["TDDRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TDDRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),

        // 24.2.30 Timer Dead Time Enable Register m (TDERm) (m = A, B) - MTU共通。
        ["TDERA"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        ["TDERB"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),

        // 24.2.31 Timer Buffer Transfer Set Register m (TBTERm) (m = A, B) - MTU共通。
        ["TBTERA"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TBTERB"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.32 Timer Waveform Control Register m (TWCRm) (m = A, B) - MTU共通。
        ["TWCRA"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TWCRB"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.33 Noise Filter Control Register n (NFCRn) (n = 0 to 4, 6, 7, 8, C)
        ["NFCR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCR4"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCR6"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCR7"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCR8"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["NFCRC"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.34 Noise Filter Control Register 5 (NFCR5) - MTU5のみ。
        ["NFCR5"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.35 Timer A/D Conversion Start Request Control Register (TADCR) - MTU4,MTU7のみ。
        ["TADCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 24.2.36 Timer A/D Conversion Start Request Cycle Set Register m (TADCORm) - MTU4,MTU7のみ。
        ["TADCORA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TADCORB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),

        // 24.2.37 Timer A/D Conversion Start Request Cycle Set Buffer Register m (TADCOBRm) - MTU4,MTU7のみ。
        ["TADCOBRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TADCOBRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),

        // 24.2.38 Timer Interrupt Skipping Mode Register m (TITMRm) (m = A, B) - MTU共通。
        ["TITMRA"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TITMRB"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.39 Timer Interrupt Skipping Set Register 1m (TITCR1m) (m = A, B) - MTU共通。
        ["TITCR1A"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TITCR1B"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.40 Timer Interrupt Skipping Counter 1m (TITCNT1m) (m = A, B) - MTU共通。
        // 本文は「リード/ライト可能」と記載するが、ビット表は全ビットRのみ。ビット表を優先しread-onlyとした(docコメント参照)。
        ["TITCNT1A"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        ["TITCNT1B"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),

        // 24.2.41 Timer Interrupt Skipping Set Register 2m (TITCR2m) (m = A, B) - MTU共通。
        ["TITCR2A"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TITCR2B"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 24.2.42 Timer Interrupt Skipping Counter 2m (TITCNT2m) (m = A, B) - MTU共通。全ビットRのみ。
        ["TITCNT2A"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        ["TITCNT2B"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
    };
}