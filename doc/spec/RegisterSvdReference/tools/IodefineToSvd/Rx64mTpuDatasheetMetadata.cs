namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 27章(16-Bit Timer Pulse Unit (TPUa)、27.2 Register Descriptions、
/// 27.2.1〜27.2.10、p.1272-1294)から手動で書き写したレジスタごとのリセット値・
/// アクセス権限。
///
/// TPUは6チャネル(TPU0〜TPU5)構成で、iodefine.hではst_tpu0〜st_tpu5という6個の
/// 別々の構造体(共通レジスタはst_tpua)になっているが、データシートは27.2.1〜
/// 27.2.10の各節で6チャネル分(TPUAの節のみ単独)のAddress(es)を並記し、
/// "Value after reset"行を1つだけ共有する構成になっている。TCR/TMDR/TIER/TSR/
/// NFCRはチャネルにより内部ビット構成が異なる(下記参照)が、以下の理由により
/// 数値としてのリセット値・アクセス権限はどの節でも全チャネル共通であることを
/// 本文で確認できたため、このファイルはS12ADと同様にレジスタ名をキーとする
/// 単一の辞書を提供する設計とした。
///
/// - TCR(27.2.1): b7(CCLR[2])はTPU0/TPU3のみ意味を持つビットで、TPU1/TPU2/
///   TPU4/TPU5では予約(read as 0)と注記されている。どちらの場合もリセット時は
///   0であるため、"Value after reset"の1行(全ビット0)がそのまま全チャネルに
///   適用できる。アクセス権限もR/Wで統一。
/// - TMDR(27.2.2): b4(BFA)/b5(BFB)/b7(ICSELD)はTPU0/TPU3のみ有効で、
///   TPU1/TPU2/TPU4/TPU5では予約(read as 0)と注記。TCRと同様にリセット値0で
///   共通。
/// - TIORH/TIORL/TIOR(27.2.3): レジスタ名自体がチャネルにより異なる
///   (TPU0/TPU3はTIORH+TIORLの2レジスタ、TPU1/TPU2/TPU4/TPU5はTIOR1レジスタ)
///   ため、そもそも同名レジスタが複数チャネルで異なる値を持つという問題は
///   発生しない。3レジスタともリセット値0x00・R/Wで共通。
/// - TIER(27.2.4): b6は全チャネル共通で予約(read as 1)。b2(TGIEC)/b3(TGIED)は
///   TPU0/TPU3のみ有効で他チャネルでは予約(read as 0)、b5(TCIEU)は逆に
///   TPU1/TPU2/TPU4/TPU5のみ有効でTPU0/TPU3では予約(read as 0)、b7(TTGE)は
///   TPU5のみ予約(read as 0、他チャネルでは機能ビットだがリセット値は0)。
///   これらの機能ビット/予約ビットの初期値はいずれも0で一致するため、
///   "Value after reset"の1行(0x40 = b6のみ1)がそのまま全チャネルに適用できる。
/// - TSR(27.2.5): b6は全チャネル共通で予約(read as 1)。b2(TGFC)/b3(TGFD)は
///   TPU0/TPU3のみ有効、b5(TCFU)はTPU1/TPU2/TPU4/TPU5のみ有効(TPU0/TPU3では
///   予約read as 0)、b7(TCFD)はTPU1/TPU2/TPU4/TPU5では機能ビット(初期値1)、
///   TPU0/TPU3では予約(read as 1)。どちらの場合もb7は1で一致するため、
///   "Value after reset"の1行(0xC0 = b6,b7が1)がそのまま全チャネルに適用できる。
///   ビット単位ではR/(W)(0書き込みでのみクリア可能な状態フラグ)とR(TCFD)が
///   混在するが、規約に従いレジスタ全体はread-writeとして扱った。
/// - NFCR(27.2.10): b2(NFCEN)/b3(NFDEN)はTPU0/TPU3のみ有効で、
///   TPU1/TPU2/TPU4/TPU5では予約(read as 0、書き込み不可)。リセット値は
///   全チャネル0x00で共通。b6/b7は全チャネル共通で予約(read as 0、書き込み
///   不可、R)だが、他ビットはR/Wのため規約に従いレジスタ全体をread-writeとした。
/// - TCNT/TGRA/TGRB/TGRC/TGRD(27.2.6〜27.2.7): 全チャネル共通で
///   read/writeレジスタ。TCNTはリセット値0x0000、TGRA〜TGRDはリセット値
///   0xFFFF(全ビット1)である点に注意(compare/capture用レジスタのため、
///   意図しない一致を避けるために全ビット1が初期値になっている)。
/// - TSTR/TSYR(27.2.8〜27.2.9): TPUAの共通レジスタで、チャネルをまたぐ問題は
///   そもそも存在しない。リセット値0x00・read-write。
///
/// 抽出スクリプトの「たたき台」(tpu_extract.jsonl)との差異:
/// - TIER/TSRについて、たたき台はTPU0.TIER/TPU1.TIER/TPU2.TIER...の並びの
///   Address(es)しか候補として拾っておらず、TPU3〜TPU5分は本文を直接確認して
///   同一節にまとめて記載されていることを確認した(たたき台の抽出漏れではなく、
///   本文側がTPU0〜TPU5を1つのAddress(es)欄に列挙しているため問題ない)。
/// - その他の値(TCR/TMDR/TIOR系/TCNT/TGR系/TSTR/TSYR/NFCR)はたたき台の
///   reset_value_candidate/access_candidateをそのまま本文と突き合わせて採用した
///   (相違なし)。
///
/// なお、iodefine.hの#define群ではTPU0とTPU1、TPU2とTPU3、TPU4とTPU5がそれぞれ
/// 同一ベースアドレス(例: TPU0とTPU1はともに0x88108)を指しており、
/// IodefineHeaderParser.ParseInstancesにより後発側(TPU1/TPU3/TPU5)が
/// AlternatePeripheralとして先発側に関連付けられる。これはハードウェア上の
/// レジスタ配置(奇数/偶数チャネルの1バイトオフセット違い)を反映したもので、
/// このメタデータファイルの設計(レジスタ名をキーとする単一辞書)には影響しない。
/// </summary>
public static class Rx64mTpuDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 27.2.1 Timer Control Register (TCR) - 全チャネル共通
        ["TCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 27.2.2 Timer Mode Register (TMDR) - 全チャネル共通
        ["TMDR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 27.2.3 Timer I/O Control Register (TIORH, TIORL, TIOR)
        // TIORH/TIORLはTPU0・TPU3、TIORはTPU1/TPU2/TPU4/TPU5専用のレジスタ名。
        ["TIORH"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIORL"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TIOR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 27.2.4 Timer Interrupt Enable Register (TIER) - 全チャネル共通(b6予約=1)
        ["TIER"] = new RegisterDatasheetMetadata(0x40, 0xFF, "read-write"),

        // 27.2.5 Timer Status Register (TSR) - 全チャネル共通(b6予約=1, b7=1)
        ["TSR"] = new RegisterDatasheetMetadata(0xC0, 0xFF, "read-write"),

        // 27.2.6 Timer Counter (TCNT) - 全チャネル共通
        ["TCNT"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 27.2.7 Timer General Registers (TGRA〜TGRD) - 全チャネル共通、リセット値0xFFFF
        ["TGRA"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRB"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRC"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["TGRD"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),

        // 27.2.8 Timer Start Register (TSTR) - TPUA共通レジスタ
        ["TSTR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 27.2.9 Timer Synchronous Register (TSYR) - TPUA共通レジスタ
        ["TSYR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 27.2.10 Noise Filter Control Register (NFCR) - 全チャネル共通
        ["NFCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}