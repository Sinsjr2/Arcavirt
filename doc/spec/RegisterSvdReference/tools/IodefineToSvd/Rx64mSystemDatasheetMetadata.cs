namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 複数章(3章 Operating Modes・6章 Resets・8章 Voltage Detection Circuit (LVDA)・
/// 9章 Clock Generation Circuit・11章 Low Power Consumption・
/// 13章 Register Write Protection Function)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。アドレス・ビット構成は代表的なレジスタ
/// (RSTSR0/RSTSR1/LVD1CR0/MSTPCRA/DPSBKR等)についてマニュアル本文と個別に
/// 突き合わせて確認済み(2026-07-28)。全57レジスタの網羅的な逐一照合は
/// 行っていない。
///
/// SYSTEMは「レジスタ全体で1つの固定リセット値」というRegisterDatasheetMetadataの
/// 前提が崩れる3種類のケースを含む(いずれもnull(PIDR/c0i.8と同じ仕組み)で表現し、
/// 理由をコード内コメントで区別する。詳細はc0i.7の知見取りまとめにも反映予定):
/// 1. リセット要因依存(RSTSR0/RSTSR1/RSTSR2): どのリセット要因で起動したかにより
///    値が変わる。データシートに「The value after reset depends on the reset
///    source.」と明記。
/// 2. オプション設定メモリ(OFS)依存(HOCOCR.HCSTP、OSCOVFSR.ILCOVF/HCOVF):
///    フラッシュ書き込み時に設定するOFS0/OFS1のビット値により、リセット後の値が
///    変わる。リセット要因ではなくチップ設定によって決まる点でRSTSR系とは異なるが、
///    「単一の固定値がない」という意味では同じ扱いにした。
/// 3. 起動時の外部要因依存(MDMONR.MD=MDピンレベル、MDSR.UBTS=起動モード、
///    SYSCR0.ROME/EXBE=MD・PC7/UBピンの組み合わせで選択される動作モード):
///    リセット解除時の外部ピン状態・起動モードにより値が変わる。
///    SYSCR0は表3.1(Selection of Operating Modes by the Mode-Setting Pins on
///    Release from the Reset State)で「SYSCR0 Initial State」としてROME/EXBEが
///    モード設定ピンの組み合わせごとに列挙されており、単一の固定値ではない。
/// 4. 真に未定義(DPSBKR): PIDR(c0i.8)と同じ、電源オン直後は不定というだけで
///    上記1-3のような特定の決定要因は明記されていない。
///
/// 上記1-4のいずれについても、null採用によりResetMaskは出力されない
/// (SvdDocumentBuilder.BuildRegisterElementはResetValueがnullの場合resetMaskも
/// 出力しないため)。そのためResetMaskは意味を持たない0を設定している
/// (PIDRと同じ扱い)。
///
/// これらとは別に、LVD1CR0/LVD2CR0のb3のように「特定の決定要因が示されない
/// 単純な未定義ビット」は、CMCR(c0i.3)と同じ「値+部分マスク」の経路
/// (該当ビットを除いたResetMask)で表現している。
/// </summary>
public static class Rx64mSystemDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 3章 Operating Modes / 6章 Resets
        ["MDMONR"] = new RegisterDatasheetMetadata(null, 0x0000, "read-only"),
        ["MDSR"] = new RegisterDatasheetMetadata(null, 0x0000, "read-only"),
        ["SYSCR0"] = new RegisterDatasheetMetadata(null, 0x0000, "read-write"),
        ["SYSCR1"] = new RegisterDatasheetMetadata(0x00FF, 0xFFFF, "read-write"),
        ["RSTSR0"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RSTSR1"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RSTSR2"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["SWRR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 8章 Voltage Detection Circuit (LVDA)
        ["LVD1CR0"] = new RegisterDatasheetMetadata(0x82, 0xF7, "read-write"),
        ["LVD1CR1"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        ["LVD1SR"] = new RegisterDatasheetMetadata(0x02, 0xFF, "read-write"),
        ["LVD2CR0"] = new RegisterDatasheetMetadata(0x82, 0xF7, "read-write"),
        ["LVD2CR1"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        ["LVD2SR"] = new RegisterDatasheetMetadata(0x02, 0xFF, "read-write"),
        ["LVCMPCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["LVDLVLR"] = new RegisterDatasheetMetadata(0xBB, 0xFF, "read-write"),

        // 9章 Clock Generation Circuit
        ["SCKCR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["SCKCR2"] = new RegisterDatasheetMetadata(0x0011, 0xFFFF, "read-write"),
        ["SCKCR3"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PLLCR"] = new RegisterDatasheetMetadata(0x1D00, 0xFFFF, "read-write"),
        ["PLLCR2"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        ["BCKCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["MOSCCR"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        ["SOSCCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["LOCOCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ILOCOCR"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        // HCSTP(b0)はOFS1.HOCOENに依存(上記コメント参照)
        ["HOCOCR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["HOCOCR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // ILCOVF(b4)はOFS0.IWDTSTRT、HCOVF(b3)はOFS1.HOCOENに依存
        ["OSCOVFSR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["OSTDCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["OSTDSR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // マニュアル本文にビット単位のR/W表が無く、アクセス権限は文脈
        // (書込み手順の記述)からの推定(SCIのBRR/MDDRと同様の状況)
        ["MOSCWTCR"] = new RegisterDatasheetMetadata(0x53, 0xFF, "read-write"),
        ["SOSCWTCR"] = new RegisterDatasheetMetadata(0x21, 0xFF, "read-write"),
        ["MOFCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["HOCOPCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 11章 Low Power Consumption / 13章 Register Write Protection Function
        ["SBYCR"] = new RegisterDatasheetMetadata(0x4000, 0xFFFF, "read-write"),
        ["MSTPCRA"] = new RegisterDatasheetMetadata(0x46FFFFFF, 0xFFFFFFFF, "read-write"),
        ["MSTPCRB"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
        ["MSTPCRC"] = new RegisterDatasheetMetadata(0xFFFF0000, 0xFFFFFFFF, "read-write"),
        ["MSTPCRD"] = new RegisterDatasheetMetadata(0xFFFFFF00, 0xFFFFFFFF, "read-write"),
        ["OPCCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["RSTCKCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSBYCR"] = new RegisterDatasheetMetadata(0x01, 0xFF, "read-write"),
        ["DPSIER0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIER1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIER2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIER3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIFR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIFR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIFR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIFR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIEGR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIEGR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIEGR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["DPSIEGR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 全ビット未定義(PIDRと同じ扱い、電源オン直後は不定)
        ["DPSBKR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["PRCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
    };
}