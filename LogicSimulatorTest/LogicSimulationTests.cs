using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

/// <summary>
/// カウントシナリオ1つ分（開始値、クロック回数、ラベル、カウント方向）
/// </summary>
public record CountScenario(
    int StartValue,
    int ClockCount,
    bool IsUp,      // true=カウントアップ, false=カウントダウン
    string Label
);

/// <summary>
/// UDCounter データ駆動テスト用 Config
/// </summary>
public record UDCounterTestConfig(
    int BitCount,
    // null → Enumerable.Range(0, 1<<BitCount) で全件生成
    int[]? ExplicitPresetValues,
    CountScenario[] CountUpScenarios,
    CountScenario[] CountDownScenarios
);

public record PinValue(string PinName, bool Value);

/// <summary>
/// 1フレームで入力されるデータとそのフレームを計算したときの出力の期待値を表します。
/// </summary>
public record SignalFrame(IReadOnlyList<PinValue> Inputs, IReadOnlyList<PinValue> Expecteds);

/// <summary>
/// 一連のフレームを処理する信号のテストパターン
/// </summary>
public record SignalTestPattern(IReadOnlyList<SignalFrame> Frames);

/// <summary>
/// よく使う回路を定義しています。
/// </summary>
public class BuiltInCircuit {

    public static readonly Circuit JK_FFMasterSlavePresetClear = new([
            new("~PRE~", new InputConnector(1)),
            new("J", new InputConnector(1)),
            new("K", new InputConnector(1)),
            new("CLK", new InputConnector(1)),
            new("~CLR~", new InputConnector(1)),
            new("and1", new AndLogic(2)),
            new("and2", new AndLogic(2)),
            new("and3", new AndLogic(2)),
            new("and4", new AndLogic(2)),
            new("and5", new AndLogic(2)),
            new("and6", new AndLogic(2)),
            new("nand1", new NAndLogic(2)),
            new("nand2", new NAndLogic(2)),
            new("nand3", new NAndLogic(2)),
            new("nand4", new NAndLogic(2)),
            new("nand5", new NAndLogic(2)),
            new("nand6", new NAndLogic(2)),
            new("nand7", new NAndLogic(2)),
            new("nand8", new NAndLogic(2)),
            new("not1", new NotLogic()),
            new("Q", new OutputConnector(1)),
            new("~Q~", new OutputConnector(1))
        ],
        [
            new(new LogicConnector("~PRE~", "out"), new LogicConnector("and3", "in[0]")),
            new(new LogicConnector("~PRE~", "out"), new LogicConnector("and5", "in[0]")),
            new(new LogicConnector("J", "out"), new LogicConnector("and1", "in[0]")),
            new(new LogicConnector("K", "out"), new LogicConnector("and2", "in[0]")),
            new(new LogicConnector("CLK", "out"), new LogicConnector("and1", "in[1]")),
            new(new LogicConnector("CLK", "out"), new LogicConnector("and2", "in[1]")),
            new(new LogicConnector("CLK", "out"), new LogicConnector("not1", "in")),
            new(new LogicConnector("not1", "out"), new LogicConnector("nand5", "in[1]")),
            new(new LogicConnector("not1", "out"), new LogicConnector("nand6", "in[0]")),
            new(new LogicConnector("~CLR~", "out"), new LogicConnector("and4", "in[1]")),
            new(new LogicConnector("~CLR~", "out"), new LogicConnector("and6", "in[1]")),
            new(new LogicConnector("and1", "out"), new LogicConnector("nand1", "in[1]")),
            new(new LogicConnector("and2", "out"), new LogicConnector("nand2", "in[0]")),
            new(new LogicConnector("nand1", "out"), new LogicConnector("and3", "in[1]")),
            new(new LogicConnector("and3", "out"), new LogicConnector("nand3", "in[0]")),
            new(new LogicConnector("nand2", "out"), new LogicConnector("and4", "in[0]")),
            new(new LogicConnector("and4", "out"), new LogicConnector("nand4", "in[1]")),
            new(new LogicConnector("nand3", "out"), new LogicConnector("nand4", "in[0]")),
            new(new LogicConnector("nand3", "out"), new LogicConnector("nand5", "in[0]")),
            new(new LogicConnector("nand4", "out"), new LogicConnector("nand3", "in[1]")),
            new(new LogicConnector("nand4", "out"), new LogicConnector("nand6", "in[1]")),
            new(new LogicConnector("nand5", "out"), new LogicConnector("and5", "in[1]")),
            new(new LogicConnector("nand6", "out"), new LogicConnector("and6", "in[0]")),
            new(new LogicConnector("and5", "out"), new LogicConnector("nand7", "in[0]")),
            new(new LogicConnector("and6", "out"), new LogicConnector("nand8", "in[1]")),
            new(new LogicConnector("nand7", "out"), new LogicConnector("Q", "in")),
            new(new LogicConnector("nand7", "out"), new LogicConnector("nand8", "in[0]")),
            new(new LogicConnector("nand7", "out"), new LogicConnector("nand2", "in[1]")),
            new(new LogicConnector("nand8", "out"), new LogicConnector("~Q~", "in")),
            new(new LogicConnector("nand8", "out"), new LogicConnector("nand7", "in[1]")),
            new(new LogicConnector("nand8", "out"), new LogicConnector("nand1", "in[0]")),
        ]);

    /// <summary>
    /// D型フリップフロップ（マスタースレーブ JK-FF を使用した実装）。
    /// クロック C の立ち下がりエッジ（High → Low）で入力 D の値を Q に取り込みます。
    ///
    /// ピン一覧:
    /// - ~Clr~ : クリア（アクティブLow）。Low のとき Q=Low に強制リセットします。
    /// - C     : クロック入力。立ち下がりエッジ（High → Low）で D の値を取り込みます。
    /// - D     : データ入力。クロック立ち下がり時にこの値が Q に転送されます。
    /// - ~Set~ : セット（アクティブLow）。Low のとき Q=High に強制セットします。
    /// - Q     : 出力。
    /// - ~Q~   : Q の反転出力。
    ///
    /// 真理値表:
    /// <code>
    /// ~Set~ | ~Clr~ | D | C  | Q | ~Q~ | 説明
    /// ------|-------|---|----|---|-----|--------------------------------------------------
    ///   H   |   H   | X | ↓  | D | ~D~ | 通常動作（立ち下がりエッジでラッチ）
    ///   L   |   H   | X | X  | H |  L  | セット有効（~Set~=Low）
    ///   H   |   L   | X | X  | L |  H  | クリア有効（~Clr~=Low）
    ///   L   |   L   | X | X  | H |  H  | 禁止状態（両方有効、発振なし）
    /// </code>
    /// </summary>
    public static readonly Circuit D_FF = new([
            new("~Clr~", new InputConnector(1)),
            new("C",     new InputConnector(1)),
            new("D",     new InputConnector(1)),
            new("~Set~", new InputConnector(1)),
            new("not_d", new NotLogic()),
            new("jkff1", new CustomCircuit("jk_ff_preset_clear")),
            new("Q",     new OutputConnector(1)),
            new("~Q~",   new OutputConnector(1))
        ],
        [
            new(new LogicConnector("~Clr~", "out"), new LogicConnector("jkff1",  "~CLR~")),
            new(new LogicConnector("~Set~", "out"), new LogicConnector("jkff1",  "~PRE~")),
            new(new LogicConnector("C",     "out"), new LogicConnector("jkff1",  "CLK")),
            new(new LogicConnector("D",     "out"), new LogicConnector("jkff1",  "J")),
            new(new LogicConnector("D",     "out"), new LogicConnector("not_d",  "in")),
            new(new LogicConnector("not_d", "out"), new LogicConnector("jkff1",  "K")),
            new(new LogicConnector("jkff1", "Q"),   new LogicConnector("Q",      "in")),
            new(new LogicConnector("jkff1", "~Q~"), new LogicConnector("~Q~",    "in")),
        ]);

    /// <summary>
    /// N ビット UD カウンタを動的に生成します
    /// </summary>
    public static Circuit CreateUDCounter(int bitCount)
    {
        if (bitCount < 1 || bitCount > 31) {
            throw new ArgumentException($"bitCount must be between 1 and 31, but was {bitCount}.", nameof(bitCount));
        }

        if (bitCount == 1) {
            // 1ビットカウンタ: 直接配線（CustomCircuit を使わない）
            return new([
                new("LOW", new InputConnector(1)),
                new("SET", new InputConnector(1)),
                new("CLK", new InputConnector(1)),
                new("DIR", new InputConnector(1)),
                new("INITIAL0", new InputConnector(1)),
                new("HI", new OutputConnector(1)),
                new("D0", new OutputConnector(1)),
                new("jkff1", new CustomCircuit("jk_ff_preset_clear")),
                new("not1", new NotLogic()),
                new("nand1", new NAndLogic(2)),
                new("nand2", new NAndLogic(2)),
                new("and1", new AndLogic(2)),
                new("xor1", new XOrLogic(2)),
            ],
            [
                new(new LogicConnector("LOW", "out"), new LogicConnector("and1", "in[0]")),
                new(new LogicConnector("and1", "out"), new LogicConnector("HI", "in")),
                new(new LogicConnector("LOW", "out"), new LogicConnector("jkff1", "J")),
                new(new LogicConnector("LOW", "out"), new LogicConnector("jkff1", "K")),
                new(new LogicConnector("INITIAL0", "out"), new LogicConnector("nand1", "in[0]")),
                new(new LogicConnector("INITIAL0", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("SET", "out"), new LogicConnector("nand1", "in[1]")),
                new(new LogicConnector("SET", "out"), new LogicConnector("nand2", "in[0]")),
                new(new LogicConnector("not1", "out"), new LogicConnector("nand2", "in[1]")),
                new(new LogicConnector("nand2", "out"), new LogicConnector("jkff1", "~CLR~")),
                new(new LogicConnector("CLK", "out"), new LogicConnector("jkff1", "CLK")),
                new(new LogicConnector("nand1", "out"), new LogicConnector("jkff1", "~PRE~")),
                new(new LogicConnector("DIR", "out"), new LogicConnector("xor1", "in[1]")),
                new(new LogicConnector("jkff1", "Q"), new LogicConnector("xor1", "in[0]")),
                new(new LogicConnector("jkff1", "Q"), new LogicConnector("D0", "in")),
                new(new LogicConnector("xor1", "out"), new LogicConnector("and1", "in[1]")),
            ]);
        }
        else
        {
            // N>1ビットカウンタ: ud_counter_1bit の CustomCircuit を N個直列接続
            var nodes = new List<LogicNode>();

            // 入力・出力ピンの追加
            nodes.Add(new("LOW", new InputConnector(1)));
            nodes.Add(new("SET", new InputConnector(1)));
            nodes.Add(new("CLK", new InputConnector(1)));
            nodes.Add(new("DIR", new InputConnector(1)));
            for (int i = 0; i < bitCount; i++) {
                nodes.Add(new($"INITIAL{i}", new InputConnector(1)));
            }
            nodes.Add(new("HI", new OutputConnector(1)));
            for (int i = 0; i < bitCount; i++) {
                nodes.Add(new($"D{i}", new OutputConnector(1)));
            }

            // ud_counter_1bit カスタム回路の追加
            for (int i = 1; i <= bitCount; i++) {
                nodes.Add(new($"udc1bit_{i}", new CustomCircuit("ud_counter_1bit")));
            }

            var wires = new List<LogicConnection>();

            // LOW → udc1bit_1.LOW
            wires.Add(new(new LogicConnector("LOW", "out"), new LogicConnector("udc1bit_1", "LOW")));

            // SET, CLK, DIR → 全カウンタ共通接続
            for (int i = 1; i <= bitCount; i++) {
                wires.Add(new(new LogicConnector("SET", "out"), new LogicConnector($"udc1bit_{i}", "SET")));
                wires.Add(new(new LogicConnector("CLK", "out"), new LogicConnector($"udc1bit_{i}", "CLK")));
                wires.Add(new(new LogicConnector("DIR", "out"), new LogicConnector($"udc1bit_{i}", "DIR")));
            }

            // udc1bit_i.HI → udc1bit_(i+1).LOW (i=1..N-1)
            for (int i = 1; i < bitCount; i++) {
                wires.Add(new(new LogicConnector($"udc1bit_{i}", "HI"), new LogicConnector($"udc1bit_{i + 1}", "LOW")));
            }

            // udc1bit_N.HI → HI出力
            wires.Add(new(new LogicConnector($"udc1bit_{bitCount}", "HI"), new LogicConnector("HI", "in")));

            // INITIALi → udc1bit_(i+1).INITIAL0 (i=0..N-1)
            for (int i = 0; i < bitCount; i++) {
                wires.Add(new(new LogicConnector($"INITIAL{i}", "out"), new LogicConnector($"udc1bit_{i+1}", "INITIAL0")));
            }

            // udc1bit_i.D0 → D(i-1) 出力
            for (int i = 1; i <= bitCount; i++) {
                wires.Add(new(new LogicConnector($"udc1bit_{i}", "D0"), new LogicConnector($"D{i-1}", "in")));
            }

            return new(nodes, wires);
        }
    }

    public static Circuit CreateComparator(int bitCount) {
        if (bitCount < 1 || bitCount > 31) {
            throw new ArgumentException($"bitCount must be between 1 and 31, but was {bitCount}.", nameof(bitCount));
        }
        if (bitCount == 1) {
            return new([
                new("A0", new InputConnector(1)),
                new("B0", new InputConnector(1)),
                new("GT", new OutputConnector(1)),
                new("EQ", new OutputConnector(1)),
                new("LT", new OutputConnector(1)),
                new("not1", new NotLogic()),
                new("not2", new NotLogic()),
                new("and1", new AndLogic(2)),
                new("and2", new AndLogic(2)),
                new("and3", new AndLogic(2)),
                new("and4", new AndLogic(2)),
                new("or1", new OrLogic(2)),
            ],
            [
                new(new LogicConnector("A0", "out"), new LogicConnector("and1", "in[0]")),
                new(new LogicConnector("A0", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("A0", "out"), new LogicConnector("and3", "in[0]")),
                new(new LogicConnector("B0", "out"), new LogicConnector("not2", "in")),
                new(new LogicConnector("B0", "out"), new LogicConnector("and3", "in[1]")),
                new(new LogicConnector("B0", "out"), new LogicConnector("and4", "in[1]")),
                new(new LogicConnector("not1", "out"), new LogicConnector("and2", "in[0]")),
                new(new LogicConnector("not1", "out"), new LogicConnector("and4", "in[0]")),
                new(new LogicConnector("not2", "out"), new LogicConnector("and1", "in[1]")),
                new(new LogicConnector("not2", "out"), new LogicConnector("and2", "in[1]")),
                new(new LogicConnector("and1", "out"), new LogicConnector("GT", "in")),
                new(new LogicConnector("and2", "out"), new LogicConnector("or1", "in[0]")),
                new(new LogicConnector("and3", "out"), new LogicConnector("or1", "in[1]")),
                new(new LogicConnector("or1", "out"), new LogicConnector("EQ", "in")),
                new(new LogicConnector("and4", "out"), new LogicConnector("LT", "in")),
            ]);
        }

        var nodes = new List<LogicNode>();
        var wires = new List<LogicConnection>();

        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"A{i}", new InputConnector(1)));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"B{i}", new InputConnector(1)));
        }
        nodes.Add(new("GT", new OutputConnector(1)));
        nodes.Add(new("EQ", new OutputConnector(1)));
        nodes.Add(new("LT", new OutputConnector(1)));

        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"comp_{i}", new CustomCircuit("comparator_1bit")));
        }
        for (int k = 1; k < bitCount; k++) {
            nodes.Add(new($"and_gt_{k}", new AndLogic(2)));
            nodes.Add(new($"or_gt_{k}", new OrLogic(2)));
            nodes.Add(new($"and_le_{k}", new AndLogic(2)));
            nodes.Add(new($"or_le_{k}", new OrLogic(2)));
        }
        nodes.Add(new("and_eq", new AndLogic(bitCount)));

        for (int i = 0; i < bitCount; i++) {
            wires.Add(new(new LogicConnector($"A{i}", "out"), new LogicConnector($"comp_{i}", "A0")));
            wires.Add(new(new LogicConnector($"B{i}", "out"), new LogicConnector($"comp_{i}", "B0")));
        }
        for (int i = 0; i < bitCount; i++) {
            wires.Add(new(new LogicConnector($"comp_{i}", "EQ"), new LogicConnector("and_eq", $"in[{i}]")));
        }
        for (int k = 1; k < bitCount; k++) {
            string prevGtNode = k == 1 ? "comp_0" : $"or_gt_{k - 1}";
            string prevGtPin  = k == 1 ? "GT" : "out";
            string prevLeNode = k == 1 ? "comp_0" : $"or_le_{k - 1}";
            string prevLePin  = k == 1 ? "LT" : "out";

            wires.Add(new(new LogicConnector(prevGtNode, prevGtPin), new LogicConnector($"and_gt_{k}", "in[0]")));
            wires.Add(new(new LogicConnector($"comp_{k}", "EQ"), new LogicConnector($"and_gt_{k}", "in[1]")));
            wires.Add(new(new LogicConnector($"and_gt_{k}", "out"), new LogicConnector($"or_gt_{k}", "in[0]")));
            wires.Add(new(new LogicConnector($"comp_{k}", "GT"), new LogicConnector($"or_gt_{k}", "in[1]")));

            wires.Add(new(new LogicConnector(prevLeNode, prevLePin), new LogicConnector($"and_le_{k}", "in[0]")));
            wires.Add(new(new LogicConnector($"comp_{k}", "EQ"), new LogicConnector($"and_le_{k}", "in[1]")));
            wires.Add(new(new LogicConnector($"and_le_{k}", "out"), new LogicConnector($"or_le_{k}", "in[0]")));
            wires.Add(new(new LogicConnector($"comp_{k}", "LT"), new LogicConnector($"or_le_{k}", "in[1]")));
        }
        wires.Add(new(new LogicConnector($"or_gt_{bitCount - 1}", "out"), new LogicConnector("GT", "in")));
        wires.Add(new(new LogicConnector("and_eq", "out"), new LogicConnector("EQ", "in")));
        wires.Add(new(new LogicConnector($"or_le_{bitCount - 1}", "out"), new LogicConnector("LT", "in")));

        return new(nodes, wires);
    }

    public static readonly Circuit Comparator1Bit = CreateComparator(1);

    public static readonly Circuit Comparator4Bit = CreateComparator(4);

    public static readonly Circuit Comparator8Bit = CreateComparator(8);

    public static readonly Circuit Comparator16Bit = CreateComparator(16);

    public static readonly Circuit UDCounter1Bit = CreateUDCounter(1);
    public static readonly Circuit UDCounter2Bit = CreateUDCounter(2);
    public static readonly Circuit UDCounter4Bit = CreateUDCounter(4);

    public static readonly Circuit UDCounter8Bit = CreateUDCounter(8);

    public static readonly Circuit UDCounter16Bit = CreateUDCounter(16);

    /// <summary>
    /// 16ビット レンジカウンタを生成します。
    ///
    /// ピン仕様:
    /// 入力: LOW, ZERO, CLK, SET, DIR, INITIAL{i} (i=0..15), A{i} (i=0..15), B{i} (i=0..15), MAX{i} (i=0..15)
    /// 出力: RANGE, D{i} (i=0..15)
    ///
    /// 動作:
    /// - カウンタが A <= D <= B の範囲内にあるとき RANGE=1
    /// - SET=1 中は RANGE を強制的に High に
    /// - オーバーフロー時に MAX 値をラップアラウンド値として使用
    /// </summary>
    public static Circuit CreateRangeCounter16Bit()
    {
        var nodes = new List<LogicNode>();
        var wires = new List<LogicConnection>();

        int bitCount = 16;

        nodes.Add(new("LOW", new InputConnector(1)));
        nodes.Add(new("ZERO", new InputConnector(1)));
        nodes.Add(new("CLK", new InputConnector(1)));
        nodes.Add(new("SET", new InputConnector(1)));
        nodes.Add(new("DIR", new InputConnector(1)));

        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"INITIAL{i}", new InputConnector(1)));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"A{i}", new InputConnector(1)));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"B{i}", new InputConnector(1)));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"MAX{i}", new InputConnector(1)));
        }

        nodes.Add(new("RANGE", new OutputConnector(1)));
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"D{i}", new OutputConnector(1)));
        }

        nodes.Add(new("counter", new CustomCircuit("ud_counter_16bit")));
        nodes.Add(new("comp_a", new CustomCircuit("comparator_16bit")));
        nodes.Add(new("comp_b", new CustomCircuit("comparator_16bit")));
        nodes.Add(new("dff", new CustomCircuit("d_ff")));

        nodes.Add(new("not_dir", new NotLogic()));
        nodes.Add(new("not_set", new NotLogic()));
        nodes.Add(new("not_hi", new NotLogic()));

        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"and_wrap_{i}", new AndLogic(2)));
            nodes.Add(new($"and_init_wrap_{i}", new AndLogic(2)));
            nodes.Add(new($"and_init_user_{i}", new AndLogic(2)));
            nodes.Add(new($"or_init_{i}", new OrLogic(2)));
        }

        nodes.Add(new("or_comp_a_eq_lt", new OrLogic(2)));
        nodes.Add(new("or_comp_b_gt_eq", new OrLogic(2)));
        nodes.Add(new("and_range_comb", new AndLogic(2)));
        nodes.Add(new("or_range_final", new OrLogic(2)));
        nodes.Add(new("or_hi_set", new OrLogic(2)));

        wires.Add(new(new LogicConnector("DIR", "out"), new LogicConnector("not_dir", "in")));
        wires.Add(new(new LogicConnector("SET", "out"), new LogicConnector("not_set", "in")));

        for (int i = 0; i < bitCount; i++) {
            wires.Add(new(new LogicConnector("not_dir", "out"), new LogicConnector($"and_wrap_{i}", "in[0]")));
            wires.Add(new(new LogicConnector($"MAX{i}", "out"), new LogicConnector($"and_wrap_{i}", "in[1]")));
            wires.Add(new(new LogicConnector($"and_wrap_{i}", "out"), new LogicConnector($"and_init_wrap_{i}", "in[0]")));
            wires.Add(new(new LogicConnector("not_set", "out"), new LogicConnector($"and_init_wrap_{i}", "in[1]")));
            wires.Add(new(new LogicConnector("SET", "out"), new LogicConnector($"and_init_user_{i}", "in[0]")));
            wires.Add(new(new LogicConnector($"INITIAL{i}", "out"), new LogicConnector($"and_init_user_{i}", "in[1]")));
            wires.Add(new(new LogicConnector($"and_init_wrap_{i}", "out"), new LogicConnector($"or_init_{i}", "in[0]")));
            wires.Add(new(new LogicConnector($"and_init_user_{i}", "out"), new LogicConnector($"or_init_{i}", "in[1]")));
            wires.Add(new(new LogicConnector($"or_init_{i}", "out"), new LogicConnector("counter", $"INITIAL{i}")));
        }

        wires.Add(new(new LogicConnector("LOW", "out"), new LogicConnector("counter", "LOW")));
        wires.Add(new(new LogicConnector("SET", "out"), new LogicConnector("or_hi_set", "in[1]")));
        wires.Add(new(new LogicConnector("counter", "HI"), new LogicConnector("not_hi", "in")));
        wires.Add(new(new LogicConnector("counter", "HI"), new LogicConnector("or_hi_set", "in[0]")));
        wires.Add(new(new LogicConnector("or_hi_set", "out"), new LogicConnector("counter", "SET")));
        wires.Add(new(new LogicConnector("CLK", "out"), new LogicConnector("counter", "CLK")));
        wires.Add(new(new LogicConnector("DIR", "out"), new LogicConnector("counter", "DIR")));

        for (int i = 0; i < bitCount; i++) {
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector("comp_a", $"B{i}")));
            wires.Add(new(new LogicConnector($"A{i}", "out"), new LogicConnector("comp_a", $"A{i}")));
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector("comp_b", $"A{i}")));
            wires.Add(new(new LogicConnector($"B{i}", "out"), new LogicConnector("comp_b", $"B{i}")));
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector($"D{i}", "in")));
        }

        wires.Add(new(new LogicConnector("comp_a", "EQ"), new LogicConnector("or_comp_a_eq_lt", "in[0]")));
        wires.Add(new(new LogicConnector("comp_a", "LT"), new LogicConnector("or_comp_a_eq_lt", "in[1]")));
        wires.Add(new(new LogicConnector("comp_b", "EQ"), new LogicConnector("or_comp_b_gt_eq", "in[0]")));
        wires.Add(new(new LogicConnector("comp_b", "LT"), new LogicConnector("or_comp_b_gt_eq", "in[1]")));

        wires.Add(new(new LogicConnector("or_comp_a_eq_lt", "out"), new LogicConnector("and_range_comb", "in[0]")));
        wires.Add(new(new LogicConnector("or_comp_b_gt_eq", "out"), new LogicConnector("and_range_comb", "in[1]")));

        wires.Add(new(new LogicConnector("and_range_comb", "out"), new LogicConnector("or_range_final", "in[0]")));
        wires.Add(new(new LogicConnector("dff", "Q"), new LogicConnector("or_range_final", "in[1]")));
        wires.Add(new(new LogicConnector("or_range_final", "out"), new LogicConnector("RANGE", "in")));

        wires.Add(new(new LogicConnector("not_set", "out"), new LogicConnector("dff", "~Set~")));
        wires.Add(new(new LogicConnector("LOW", "out"), new LogicConnector("dff", "~Clr~")));
        wires.Add(new(new LogicConnector("ZERO", "out"), new LogicConnector("dff", "D")));
        wires.Add(new(new LogicConnector("CLK", "out"), new LogicConnector("dff", "C")));

        return new(nodes, wires);
    }

    public static readonly Circuit RangeCounter16Bit = CreateRangeCounter16Bit();

    /// <summary>
    /// N ビット マルチプレクサを動的に生成します。
    ///
    /// セレクタビット S[0..M-1] (M = numOfSelectorBit) で、C = 2^M 個のチャンネルうち1つを選択し、
    /// そのチャンネルの入力データ D[ch][0..dataBit-1] を出力 Y[0..dataBit-1] に通します。
    ///
    /// ピン仕様:
    /// 入力:
    ///   - S{i} (i=0..M-1): セレクタビット i
    ///   - D{ch}_{b} (ch=0..C-1, b=0..dataBit-1): チャンネル ch のビット b のデータ入力
    /// 出力:
    ///   - Y{b} (b=0..dataBit-1): 選択されたチャンネルのデータ出力ビット b
    /// </summary>
    /// <param name="dataBit">各チャンネルのデータビット数。1 以上を指定。</param>
    /// <param name="numOfSelectorBit">セレクタビット数。1 以上 8 以下を指定。</param>
    /// <returns>マルチプレクサ回路。</returns>
    public static Circuit CreateMultiplexer(int dataBit, int numOfSelectorBit) {
        if (dataBit < 1) {
            throw new ArgumentException($"dataBit must be at least 1, but was {dataBit}.", nameof(dataBit));
        }
        if (numOfSelectorBit < 1 || numOfSelectorBit > 8) {
            throw new ArgumentException($"numOfSelectorBit must be between 1 and 8, but was {numOfSelectorBit}.", nameof(numOfSelectorBit));
        }

        int numOfChannel = 1 << numOfSelectorBit;
        var nodes = new List<LogicNode>();
        var wires = new List<LogicConnection>();

        // セレクタ入力 S0..S{M-1}
        for (int i = 0; i < numOfSelectorBit; i++) {
            nodes.Add(new($"S{i}", new InputConnector(1)));
        }

        // データ入力 D{ch}_{b} (チャンネル順、ビット順)
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                nodes.Add(new($"D{ch}_{b}", new InputConnector(1)));
            }
        }

        // 出力 Y{b}
        for (int b = 0; b < dataBit; b++) {
            nodes.Add(new($"Y{b}", new OutputConnector(1)));
        }

        // NOT ゲート not_s{i}
        for (int i = 0; i < numOfSelectorBit; i++) {
            nodes.Add(new($"not_s{i}", new NotLogic()));
        }

        // デコード AND ゲート dec_{ch} (M >= 2 のみ)
        if (numOfSelectorBit >= 2) {
            for (int ch = 0; ch < numOfChannel; ch++) {
                nodes.Add(new($"dec_{ch}", new AndLogic(numOfSelectorBit)));
            }
        }

        // データ AND ゲート and_{ch}_{b}
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                nodes.Add(new($"and_{ch}_{b}", new AndLogic(2)));
            }
        }

        // 出力 OR ゲート or_{b}
        for (int b = 0; b < dataBit; b++) {
            nodes.Add(new($"or_{b}", new OrLogic(numOfChannel)));
        }

        // 配線

        // NOT: S{i} → not_s{i}.in
        for (int i = 0; i < numOfSelectorBit; i++) {
            wires.Add(new(new LogicConnector($"S{i}", "out"), new LogicConnector($"not_s{i}", "in")));
        }

        // デコード AND (M >= 2 のみ)
        if (numOfSelectorBit >= 2) {
            for (int ch = 0; ch < numOfChannel; ch++) {
                for (int i = 0; i < numOfSelectorBit; i++) {
                    bool bitIsOne = (ch & (1 << i)) != 0;
                    string sourceNode = bitIsOne ? $"S{i}" : $"not_s{i}";
                    wires.Add(new(new LogicConnector(sourceNode, "out"), new LogicConnector($"dec_{ch}", $"in[{i}]")));
                }
            }
        }

        // データ AND: (M==1 なら ch==0 は not_s0、ch==1 は S0；M>=2 なら dec_{ch}.out) → and_{ch}_{b}.in[0]
        for (int ch = 0; ch < numOfChannel; ch++) {
            string decodeSource;
            if (numOfSelectorBit == 1) {
                decodeSource = ch == 0 ? "not_s0" : "S0";
            } else {
                decodeSource = $"dec_{ch}";
            }

            for (int b = 0; b < dataBit; b++) {
                wires.Add(new(new LogicConnector(decodeSource, "out"), new LogicConnector($"and_{ch}_{b}", "in[0]")));
            }
        }

        // データ AND: D{ch}_{b}.out → and_{ch}_{b}.in[1]
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                wires.Add(new(new LogicConnector($"D{ch}_{b}", "out"), new LogicConnector($"and_{ch}_{b}", "in[1]")));
            }
        }

        // OR: 全チャンネルの and_{ch}_{b}.out を or_{b}.in[{ch}] へ
        for (int b = 0; b < dataBit; b++) {
            for (int ch = 0; ch < numOfChannel; ch++) {
                wires.Add(new(new LogicConnector($"and_{ch}_{b}", "out"), new LogicConnector($"or_{b}", $"in[{ch}]")));
            }
        }

        // 出力: or_{b}.out → Y{b}.in
        for (int b = 0; b < dataBit; b++) {
            wires.Add(new(new LogicConnector($"or_{b}", "out"), new LogicConnector($"Y{b}", "in")));
        }

        return new(nodes, wires);
    }

    /// <summary>
    /// N ビット デマルチプレクサを動的に生成します。
    ///
    /// セレクタビット S[0..M-1] (M = numOfSelectorBit) で、C = 2^M 個のチャンネルうち1つを選択し、
    /// 入力データ D[0..dataBit-1] をそのチャンネルの出力 Y[ch][0..dataBit-1] に通します。
    /// その他のチャンネルの出力は全て Low に固定されます。
    ///
    /// ピン仕様:
    /// 入力:
    ///   - S{i} (i=0..M-1): セレクタビット i
    ///   - D{b} (b=0..dataBit-1): データ入力ビット b
    /// 出力:
    ///   - Y{ch}_{b} (ch=0..C-1, b=0..dataBit-1): チャンネル ch のビット b のデータ出力
    /// </summary>
    /// <param name="dataBit">データのビット数。1 以上を指定。</param>
    /// <param name="numOfSelectorBit">セレクタビット数。1 以上 8 以下を指定。</param>
    /// <returns>デマルチプレクサ回路。</returns>
    public static Circuit CreateDemultiplexer(int dataBit, int numOfSelectorBit) {
        if (dataBit < 1) {
            throw new ArgumentException($"dataBit must be at least 1, but was {dataBit}.", nameof(dataBit));
        }
        if (numOfSelectorBit < 1 || numOfSelectorBit > 8) {
            throw new ArgumentException($"numOfSelectorBit must be between 1 and 8, but was {numOfSelectorBit}.", nameof(numOfSelectorBit));
        }

        int numOfChannel = 1 << numOfSelectorBit;
        var nodes = new List<LogicNode>();
        var wires = new List<LogicConnection>();

        // セレクタ入力 S0..S{M-1}
        for (int i = 0; i < numOfSelectorBit; i++) {
            nodes.Add(new($"S{i}", new InputConnector(1)));
        }

        // データ入力 D{b}
        for (int b = 0; b < dataBit; b++) {
            nodes.Add(new($"D{b}", new InputConnector(1)));
        }

        // 出力 Y{ch}_{b}
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                nodes.Add(new($"Y{ch}_{b}", new OutputConnector(1)));
            }
        }

        // NOT ゲート not_s{i}
        for (int i = 0; i < numOfSelectorBit; i++) {
            nodes.Add(new($"not_s{i}", new NotLogic()));
        }

        // デコード AND ゲート dec_{ch} (M >= 2 のみ)
        if (numOfSelectorBit >= 2) {
            for (int ch = 0; ch < numOfChannel; ch++) {
                nodes.Add(new($"dec_{ch}", new AndLogic(numOfSelectorBit)));
            }
        }

        // データ AND ゲート and_{ch}_{b}
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                nodes.Add(new($"and_{ch}_{b}", new AndLogic(2)));
            }
        }

        // 配線

        // NOT: S{i} → not_s{i}.in
        for (int i = 0; i < numOfSelectorBit; i++) {
            wires.Add(new(new LogicConnector($"S{i}", "out"), new LogicConnector($"not_s{i}", "in")));
        }

        // デコード AND (M >= 2 のみ)
        if (numOfSelectorBit >= 2) {
            for (int ch = 0; ch < numOfChannel; ch++) {
                for (int i = 0; i < numOfSelectorBit; i++) {
                    bool bitIsOne = (ch & (1 << i)) != 0;
                    string sourceNode = bitIsOne ? $"S{i}" : $"not_s{i}";
                    wires.Add(new(new LogicConnector(sourceNode, "out"), new LogicConnector($"dec_{ch}", $"in[{i}]")));
                }
            }
        }

        // データ AND: (M==1 なら ch==0 は not_s0、ch==1 は S0；M>=2 なら dec_{ch}.out) → and_{ch}_{b}.in[0]
        for (int ch = 0; ch < numOfChannel; ch++) {
            string decodeSource;
            if (numOfSelectorBit == 1) {
                decodeSource = ch == 0 ? "not_s0" : "S0";
            } else {
                decodeSource = $"dec_{ch}";
            }

            for (int b = 0; b < dataBit; b++) {
                wires.Add(new(new LogicConnector(decodeSource, "out"), new LogicConnector($"and_{ch}_{b}", "in[0]")));
            }
        }

        // データ AND: D{b}.out → and_{ch}_{b}.in[1]（全チャンネル共通）
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                wires.Add(new(new LogicConnector($"D{b}", "out"), new LogicConnector($"and_{ch}_{b}", "in[1]")));
            }
        }

        // 出力: and_{ch}_{b}.out → Y{ch}_{b}.in
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                wires.Add(new(new LogicConnector($"and_{ch}_{b}", "out"), new LogicConnector($"Y{ch}_{b}", "in")));
            }
        }

        return new(nodes, wires);
    }

    public static readonly IReadOnlyDictionary<string, Circuit> Circuits = new Dictionary<string, Circuit> {
        { "jk_ff_preset_clear", JK_FFMasterSlavePresetClear },
        { "d_ff", D_FF },
        { "comparator_1bit", Comparator1Bit },
        { "comparator_4bit", Comparator4Bit },
        { "comparator_8bit", Comparator8Bit },
        { "comparator_16bit", Comparator16Bit },
        { "ud_counter_1bit", UDCounter1Bit },
        { "ud_counter_2bit", UDCounter2Bit },
        { "ud_counter_4bit", UDCounter4Bit },
        { "ud_counter_8bit", UDCounter8Bit },
        { "ud_counter_16bit", UDCounter16Bit }
    };
}

[TestFixture]
public class LogicSimulationTests {

    static void TestLogicGate(ILogicElement gate, LogicSignal[] inputs, LogicSignal expected) {
        int n = inputs.Length;
        var nodes = Enumerable.Range(0, n)
            .Select(i => new LogicNode($"input{i}", new InputConnector(1)))
            .Append(new LogicNode("gate", gate))
            .Append(new LogicNode("output", new OutputConnector(1)))
            .ToArray();

        var connections = Enumerable.Range(0, n)
            .Select(i => new LogicConnection(new($"input{i}", "out"), new("gate", $"in[{i}]")))
            .Append(new LogicConnection(new("gate", "out"), new("output", "in")))
            .ToArray();

        var sim = new LogicSimulation(new Circuit(nodes, connections));
        for (int i = 0; i < n; i++) {
            sim.SetInput($"input{i}", 0, inputs[i]);
        }
        sim.Step();
        Assert.That(sim.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(false, false, false)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, true, true)]
    public void OrGateExtensibilityTest(bool input1, bool input2, bool expected) {
        var circuit = new Circuit([
                new("input1", new InputConnector(1)),
                new("input2", new InputConnector(1)),
                new("or", new OrLogic(2)),
                new("output", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("input1", "out"), new LogicConnector("or", "in[0]")),
                new(new LogicConnector("input2", "out"), new LogicConnector("or", "in[1]")),
                new(new LogicConnector("or", "out"), new LogicConnector("output", "in"))
            ]);

        // カスタムファクトリを指定してLogicSimulationをインスタンス化
        var factories = new Dictionary<Type, ILogicExecutorFactory> {
            { typeof(OrLogic), new LogicExecutorFactory<OrLogic>(new OrLogicExecutorFactory()) },
            { typeof(InputConnector), new LogicExecutorFactory<InputConnector>(new InputConnectorExecutorFactory()) },
            { typeof(OutputConnector), new LogicExecutorFactory<OutputConnector>(new OutputConnectorExecutorFactory()) },
        };

        var simulation = new LogicSimulation(circuit, factories);

        simulation.SetInput("input1", 0, input1.ToSignal());
        simulation.SetInput("input2", 0, input2.ToSignal());
        simulation.Step();
        
        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected.ToSignal()));
    }

    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.X }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.X }, LogicSignal.X)]
    // 3入力テストケース: 全入力High → High、Low あり → Low
    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High, LogicSignal.High }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low, LogicSignal.High }, LogicSignal.Low)]
    public void AndLogicTest(LogicSignal[] inputs, LogicSignal expected) {
        TestLogicGate(new AndLogic(inputs.Length), inputs, expected);
    }

    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.X }, LogicSignal.High)]
    // 3入力テストケース: High あり → High、全Low → Low
    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.Low, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low, LogicSignal.Low }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High, LogicSignal.Low }, LogicSignal.High)]
    public void OrLogicTest(LogicSignal[] inputs, LogicSignal expected) {
        TestLogicGate(new OrLogic(inputs.Length), inputs, expected);
    }

    [TestCase(LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.X, LogicSignal.X)]
    public void NotLogicTest(LogicSignal input, LogicSignal expected) {
        var circuit = new Circuit([
                new("input", new InputConnector(1)),
                new("not1", new NotLogic()),
                new("output", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("input", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("output", "in"))
            ]);

        var simulation = new LogicSimulation(circuit);
        
        simulation.SetInput("input", 0, input);
        simulation.Step();
        
        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.X }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.X }, LogicSignal.X)]
    // 3入力テストケース: 全入力High → Low、それ以外 → High
    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High, LogicSignal.High }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low, LogicSignal.High }, LogicSignal.High)]
    public void NAndLogicTest(LogicSignal[] inputs, LogicSignal expected) {
        TestLogicGate(new NAndLogic(inputs.Length), inputs, expected);
    }

    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.X }, LogicSignal.Low)]
    // 3入力テストケース: 全Low → High、High あり → Low
    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.Low, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low, LogicSignal.Low }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High, LogicSignal.Low }, LogicSignal.Low)]
    public void NOrLogicTest(LogicSignal[] inputs, LogicSignal expected) {
        TestLogicGate(new NOrLogic(inputs.Length), inputs, expected);
    }

    // 2入力テスト
    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.Low }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.High }, LogicSignal.High)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.High }, LogicSignal.Low)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.X, LogicSignal.High }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.Low, LogicSignal.X }, LogicSignal.X)]
    [TestCase(new[] { LogicSignal.High, LogicSignal.X }, LogicSignal.X)]
    // 3入力テスト
    [TestCase(new[] { LogicSignal.Low, LogicSignal.Low, LogicSignal.Low }, LogicSignal.Low)] // 全Low → Low (0^0^0=0)
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.High }, LogicSignal.High)] // 全High → High (1^1^1=1)
    [TestCase(new[] { LogicSignal.High, LogicSignal.High, LogicSignal.Low }, LogicSignal.Low)] // 偶数個のHigh → Low (1^1^0=0)
    [TestCase(new[] { LogicSignal.X, LogicSignal.Low, LogicSignal.Low }, LogicSignal.X)] // X混じりは常に不定
    [TestCase(new[] { LogicSignal.X, LogicSignal.High, LogicSignal.Low }, LogicSignal.X)] // XOR固有の特性確認：X→確定しない
    public void XOrLogicTest(LogicSignal[] inputs, LogicSignal expected) {
        TestLogicGate(new XOrLogic(inputs.Length), inputs, expected);
    }

    public static readonly IReadOnlyList<SignalTestPattern> NandSrFFLatchSimulationTest_Data = [
        new([ new( [new("S", true) ], [ new("Q", true), new("~Q", false)]) ]), // // 出力:保持(1)
        // NOTE: S=0 R=0 は保持であるので、初期状態は不定になる
        // 出力:保持(0)
        new([
            new([ new("R", true) ], [ new("Q", false), new("~Q", true)]),
            new([ new("R", false) ], [ new("Q", false), new("~Q", true)])
        ]),
        // // 初期:1 出力:0
        new([
            new([ new("S", true) ], []),
            new([ new("S", false), new("R", true) ], [ new("Q", false), new("~Q", true) ])
        ]),
        // 初期:0 出力0
        new([
            new([ new("R", true) ], [ new("Q", false), new("~Q", true) ])
        ]),
        // 初期:1 出力:1
        new([
            new([ new("S", true) ], []),
            new([ new("S", true) ], [ new("Q", true), new("~Q", false) ])
        ]),
        // 初期:0 出力1
        new([
            new([ new("R", true) ], [ ]),
            new([ new("R", false), new("S", true) ], [new("Q", true), new("~Q", false)])
        ]),
    ];

    // 回路が発振したときの無限ループ対策
    [CancelAfter(1000)]
    [TestCaseSource(nameof(NandSrFFLatchSimulationTest_Data))]
    public void NandSrFFLatchSimulationTest(SignalTestPattern pattern) {
        var circuit = new Circuit([
                new("S", new InputConnector(1)),
                new("R", new InputConnector(1)),
                new("not1", new NotLogic()),
                new("not2", new NotLogic()),
                new("nand1", new NAndLogic(2)),
                new("nand2", new NAndLogic(2)),
                new("Q", new OutputConnector(1)),
                new("~Q", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("S", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("nand1", "in[0]")),
                new(new LogicConnector("R", "out"), new LogicConnector("not2", "in")),
                new(new LogicConnector("not2", "out"), new LogicConnector("nand2", "in[0]")),
                new(new LogicConnector("nand1", "out"), new LogicConnector("nand2", "in[1]")),
                new(new LogicConnector("nand1", "out"), new LogicConnector("Q", "in")),
                new(new LogicConnector("nand2", "out"), new LogicConnector("nand1", "in[1]")),
                new(new LogicConnector("nand2", "out"), new LogicConnector("~Q", "in"))
            ]);

        var simulation = new LogicSimulation(circuit);
        simulation.SetInput("S", 0, LogicSignal.Low);
        simulation.SetInput("R", 0, LogicSignal.Low);

        foreach (var frame in pattern.Frames) {
            foreach (var input in frame.Inputs) {
                simulation.SetInput(input.PinName, 0, input.Value.ToSignal());
            }
            simulation.Step();
            foreach (var expected in frame.Expecteds) {
                Assert.That(simulation.GetOutput(expected.PinName, 0), Is.EqualTo(expected.Value.ToSignal()));
            }
        }
    }

    public static IReadOnlyList<SignalTestPattern> JKFFMasterSlavePresetClear_Data = [
        // 初期状態で、 PRE CLR の信号が出力されることを考える
        new([ new([], [ new("Q", true), new("~Q~", true) ]) ]),
        new([ new([ new("~PRE~", true) ], [ new("Q", false), new("~Q~", true) ]) ]),
        new([ new([ new("~CLR~", true) ], [ new("Q", true), new("~Q~", false) ]) ]),
        new([
            // NOTE: Q と ~Q~ の両方が 1 のときに、PRE と CLR を1にすると発振する
            new([ new("~PRE~", true) ],  [ ]),
            new([ new("~CLR~", true) ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // クロックを入れても出力が変化しない J=0, K=0
        new([
            new([ new("~PRE~", true) ],  [ ]), new([ new("~CLR~", true) ], [ ]),
            new([ new("CLK", true) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("CLK", false) ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // クロックを入れると出力トグルする 初期: Q=0 J=1, K=1
        new([
            new([ new("~PRE~", true) ],  [ ]), new([ new("~CLR~", true) ], []),
            new([ new("J", true), new("K", true) ],  [ new("Q", false), new("~Q~", true) ]),
            new([ new("CLK", true) ], [ new("Q", false), new("~Q~", true) ]),
            // 1回目
            new([ new("CLK", false) ], [ new("Q", true), new("~Q~", false) ]),
            new([ new("CLK", true) ], [ new("Q", true), new("~Q~", false) ]),
            // 2回目
            new([ new("CLK", false) ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // クロックを入れると出力トグルする 初期: Q=1 J=1, K=1
        new([
            new([ new("~CLR~", true) ],  [ ]), new([ new("~PRE~", true) ], []),
            new([ new("J", true), new("K", true) ],  [ new("Q", true), new("~Q~", false) ]),
            new([ new("CLK", true) ], [ new("Q", true), new("~Q~", false) ]),
            // 1回目
            new([ new("CLK", false) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("CLK", true) ], [ new("Q", false), new("~Q~", true) ]),
            // 2回目
            new([ new("CLK", false) ], [ new("Q", true), new("~Q~", false) ]),
            new([ new("CLK", true) ], [ new("Q", true), new("~Q~", false) ])
        ]),
        // 初期:Q=1 Q=0で固定される
        new([
            new([ new("~CLR~", true) ],  []), new([ new("~PRE~", true) ], []),
            new([ new("K", true) ],  [ new("Q", true), new("~Q~", false) ]),
            new([ new("CLK", true) ],  [ new("Q", true), new("~Q~", false) ]),
            // 1回目
            new([ new("CLK", false) ],  [ new("Q", false), new("~Q~", true) ]),
            new([ new("CLK", true) ],  [ new("Q", false), new("~Q~", true) ]),
            // 2回目
            new([ new("CLK", false) ],  [ new("Q", false), new("~Q~", true) ]),
            new([ new("CLK", true) ],  [ new("Q", false), new("~Q~", true) ]),
        ]),
        // 初期:Q=0 Q=1で固定される
        new([
            new([ new("~PRE~", true) ],  [ ]), new([ new("~CLR~", true) ], []),
            new([ new("J", true) ],  [ new("Q", false), new("~Q~", true) ]),
            new([ new("CLK", true) ],  [ new("Q", false), new("~Q~", true) ]),
            // 1回目
            new([ new("CLK", false) ],  [ new("Q", true), new("~Q~", false) ]),
            new([ new("CLK", true) ],  [ new("Q", true), new("~Q~", false) ]),
            // 2回目
            new([ new("CLK", false) ],  [ new("Q", true), new("~Q~", false) ]),
            new([ new("CLK", true) ],  [ new("Q", true), new("~Q~", false) ]),
        ])
    ];

    [CancelAfter(1000)]
    [TestCaseSource(nameof(JKFFMasterSlavePresetClear_Data))]
    public void JKFFMasterSlavePresetClear(SignalTestPattern testPattern) {
        var circuit = BuiltInCircuit.JK_FFMasterSlavePresetClear;

        var simulation = new LogicSimulation(circuit);

        simulation.SetInput("CLK", 0, LogicSignal.Low);
        simulation.SetInput("J", 0, LogicSignal.Low);
        simulation.SetInput("K", 0, LogicSignal.Low);
        simulation.SetInput("~PRE~", 0, LogicSignal.Low);
        simulation.SetInput("~CLR~", 0, LogicSignal.Low);
        simulation.Step();

        foreach (var frame in testPattern.Frames) {
            foreach (var input in frame.Inputs) {
                simulation.SetInput(input.PinName, 0, input.Value.ToSignal());
            }
            simulation.Step();
            foreach (var expected in frame.Expecteds) {
                Assert.That(simulation.GetOutput(expected.PinName, 0), Is.EqualTo(expected.Value.ToSignal()));
            }
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    public void Comparator_DataDriven(int bitCount) {
        int maxVal = (1 << bitCount) - 1;
        for (int a = 0; a <= maxVal; a++) {
            for (int b = 0; b <= maxVal; b++) {
                var simulation = new LogicSimulation(
                    BuiltInCircuit.CreateComparator(bitCount), BuiltInCircuit.Circuits);
                for (int i = 0; i < bitCount; i++) {
                    simulation.SetInput($"A{i}", 0, ((a >> i & 1) != 0).ToSignal());
                    simulation.SetInput($"B{i}", 0, ((b >> i & 1) != 0).ToSignal());
                }
                simulation.Step();
                using (Assert.EnterMultipleScope()) {
                    Assert.That(simulation.GetOutput("GT", 0), Is.EqualTo((a > b).ToSignal()), $"A=0x{a:X} > B=0x{b:X}");
                    Assert.That(simulation.GetOutput("EQ", 0), Is.EqualTo((a == b).ToSignal()), $"A=0x{a:X} == B=0x{b:X}");
                    Assert.That(simulation.GetOutput("LT", 0), Is.EqualTo((a < b).ToSignal()), $"A=0x{a:X} < B=0x{b:X}");
                }
            }
        }
    }

    /// <summary>
    /// 回路の展開が入れ子になっていた場合展開できるかを確認します。
    /// </summary>
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.Low)]
    public void CustomCircuitExpandTest(LogicSignal input1, LogicSignal input2, LogicSignal expected) {
        var andCircuit = new Circuit([
                new("a", new InputConnector(1)),
                new("b", new InputConnector(1)),
                new("and1", new AndLogic(2)),
                new("y", new OutputConnector(1))
            ],
            [
                new(new("a", "out"), new("and1", "in[0]")),
                new(new("b", "out"), new("and1", "in[1]")),
                new(new("and1", "out"), new ("y", "in"))
            ]);

        var library = new Dictionary<string, Circuit> {
            { "andCircuit", andCircuit }
        };

        var testCircuit = new Circuit([
                new("x1", new InputConnector(1)),
                new("x2", new InputConnector(1)),
                new("and100", new CustomCircuit("andCircuit")),
                new("result", new OutputConnector(1))
            ],
            [
                new(new("x1", "out"), new("and100", "a")),
                new(new("x2", "out"), new("and100", "b")),
                new(new("and100", "y"), new("result", "in")),
            ]);
        
        var simulation = new LogicSimulation(testCircuit, library);
        simulation.SetInput("x1", 0, input1);
        simulation.SetInput("x2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("result", 0), Is.EqualTo(expected));
    }

    /// <summary>
    /// 1つの出力ピンから複数の入力ピンに状態が正しくコピーされることを確認するテスト
    /// </summary>
    [Test]
    public void MultipleConnectionCopyTest() {
        // 1つのOR素子の出力を3つの異なる素子の入力に接続する複数接続テスト
        var circuit = new Circuit(new LogicNode[] {
                new("input", new InputConnector(1)),
                new("or1", new OrLogic(1)),
                new("and1", new AndLogic(2)),
                new("and2", new AndLogic(2)),
                new("and3", new AndLogic(2)),
                new("outputA", new OutputConnector(1)),
                new("outputB", new OutputConnector(1)),
                new("outputC", new OutputConnector(1))
            },
            new LogicConnection[] {
                // 入力 → OR[0]
                new(new LogicConnector("input", "out"), new LogicConnector("or1", "in[0]")),
                
                // OR出力 → 複数のAND素子の入力[0]（複数接続）
                new(new LogicConnector("or1", "out"), new LogicConnector("and1", "in[0]")),
                new(new LogicConnector("or1", "out"), new LogicConnector("and2", "in[0]")),
                new(new LogicConnector("or1", "out"), new LogicConnector("and3", "in[0]")),
                
                // 入力をAND素子の入力[1]にも接続（全ANDが同じ入力を受け取る）
                new(new LogicConnector("input", "out"), new LogicConnector("and1", "in[1]")),
                new(new LogicConnector("input", "out"), new LogicConnector("and2", "in[1]")),
                new(new LogicConnector("input", "out"), new LogicConnector("and3", "in[1]")),
                
                // AND出力 → 出力コネクタ
                new(new LogicConnector("and1", "out"), new LogicConnector("outputA", "in")),
                new(new LogicConnector("and2", "out"), new LogicConnector("outputB", "in")),
                new(new LogicConnector("and3", "out"), new LogicConnector("outputC", "in"))
            });

        var simulation = new LogicSimulation(circuit);

        // ステップ1: 入力=FALSE で初期化
        simulation.SetInput("input", 0, LogicSignal.Low);
        simulation.Step();

        Assert.That(simulation.GetOutput("outputA", 0), Is.EqualTo(LogicSignal.Low));
        Assert.That(simulation.GetOutput("outputB", 0), Is.EqualTo(LogicSignal.Low));
        Assert.That(simulation.GetOutput("outputC", 0), Is.EqualTo(LogicSignal.Low));

        // ステップ2: 入力=TRUE に変更
        simulation.SetInput("input", 0, LogicSignal.High);
        simulation.Step();

        // すべての出力がTRUEであることを確認（複数接続がすべて正しくコピーされたことを検証）
        Assert.That(simulation.GetOutput("outputA", 0), Is.EqualTo(LogicSignal.High));
        Assert.That(simulation.GetOutput("outputB", 0), Is.EqualTo(LogicSignal.High));
        Assert.That(simulation.GetOutput("outputC", 0), Is.EqualTo(LogicSignal.High));
    }

    public static IEnumerable<TestCaseData> UDCounterTestCases => [
        // 1bit
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 1,
            ExplicitPresetValues: null,
            CountUpScenarios: [ new(StartValue: 0, ClockCount: 4, IsUp: true, Label: "countup_full") ],
            CountDownScenarios: [ new(StartValue: 1, ClockCount: 4, IsUp: false, Label: "countdown_full") ]
        )).SetName("1bit"),

        // 2bit
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 2,
            ExplicitPresetValues: null,
            CountUpScenarios: [ new(StartValue: 0, ClockCount: 8, IsUp: true, Label: "countup_full") ],
            CountDownScenarios: [ new(StartValue: 3, ClockCount: 8, IsUp: false, Label: "countdown_full") ]
        )).SetName("2bit"),

        // 4bit
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 4,
            ExplicitPresetValues: null,
            CountUpScenarios: [ new(StartValue: 0, ClockCount: 32, IsUp: true, Label: "countup_full") ],
            CountDownScenarios: [ new(StartValue: 15, ClockCount: 32, IsUp: false, Label: "countdown_full") ]
        )).SetName("4bit"),

        // 8bit（修正4適用: StartValue: 0x01, ClockCount: 3）
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 8,
            ExplicitPresetValues: [0x00, 0x01, 0x7F, 0x80, 0xFF],
            CountUpScenarios: [
                new(StartValue: 0x00, ClockCount: 16, IsUp: true,  Label: "countup_boundary"),
                new(StartValue: 0xFF, ClockCount: 3,  IsUp: true,  Label: "overflow"),
                new(StartValue: 0x80, ClockCount: 4,  IsUp: true,  Label: "midvalue"),
            ],
            CountDownScenarios: [
                new(StartValue: 0x01, ClockCount: 3, IsUp: false, Label: "countdown_underflow"),
            ]
        )).SetName("8bit"),

        // 16bit（修正4適用: StartValue: 0x0001, ClockCount: 3）
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 16,
            ExplicitPresetValues: [0x0000, 0x0001, 0x8000, 0xFFFF],
            CountUpScenarios: [
                new(StartValue: 0x0000, ClockCount: 2, IsUp: true,  Label: "countup_boundary"),
                new(StartValue: 0xFFFF, ClockCount: 4, IsUp: true,  Label: "overflow"),
                new(StartValue: 0x8000, ClockCount: 2, IsUp: true,  Label: "midvalue"),
            ],
            CountDownScenarios: [
                new(StartValue: 0x0001, ClockCount: 3, IsUp: false, Label: "countdown_underflow"),
            ]
        )).SetName("16bit"),
    ];

    [TestCaseSource(nameof(UDCounterTestCases))]
    [CancelAfter(10000)] // 動的設定不可のため最大値を固定（1/2/4bit全件でも余裕あり）
    public void UDCounter_DataDriven(UDCounterTestConfig cfg) {
        int bitCount = cfg.BitCount;
        int maxValue = (1 << bitCount) - 1;
        int modulus  = maxValue + 1;

        // Phase 0: 初期化
        var circuit    = BuiltInCircuit.CreateUDCounter(bitCount);
        var simulation = new LogicSimulation(circuit, BuiltInCircuit.Circuits);
        simulation.SetInput("CLK", 0, LogicSignal.Low);
        simulation.SetInput("DIR", 0, LogicSignal.Low);
        simulation.SetInput("LOW", 0, LogicSignal.Low);
        simulation.SetInput("SET", 0, LogicSignal.Low);
        for (int j = 0; j < bitCount; j++) {
            simulation.SetInput($"INITIAL{j}", 0, LogicSignal.Low);
        }
        simulation.Step();

        // Phase 1: リセット確認
        simulation.SetInput("SET", 0, LogicSignal.High);
        simulation.Step();
        for (int j = 0; j < bitCount; j++) {
            Assert.That(simulation.GetOutput($"D{j}", 0), Is.EqualTo(LogicSignal.Low), $"reset: D{j}");
        }

        // Phase 2: プリセット確認
        var presetValues = cfg.ExplicitPresetValues ?? Enumerable.Range(0, modulus).ToArray();
        foreach (int preset in presetValues) {
            for (int j = 0; j < bitCount; j++) {
                simulation.SetInput($"INITIAL{j}", 0, ((preset & (1 << j)) != 0).ToSignal());
            }
            simulation.SetInput("SET", 0, LogicSignal.High);
            simulation.Step();
            // 2a. SET=High中の確認
            for (int j = 0; j < bitCount; j++) {
                Assert.That(simulation.GetOutput($"D{j}", 0),
                    Is.EqualTo(((preset & (1 << j)) != 0).ToSignal()),
                    $"preset=0x{preset:X}: D{j}");
            }
            // 2b. SET解除後のホールド確認
            simulation.SetInput("SET", 0, LogicSignal.Low);
            simulation.Step();
            for (int j = 0; j < bitCount; j++) {
                Assert.That(simulation.GetOutput($"D{j}", 0),
                    Is.EqualTo(((preset & (1 << j)) != 0).ToSignal()),
                    $"after preset clear, preset=0x{preset:X}: D{j}");
            }
        }

        // Phase 3: カウントアップシナリオ群
        foreach (var scenario in cfg.CountUpScenarios) {
            SetupCountScenario(simulation, bitCount, scenario.StartValue, isUp: true);
            RunCountScenario(simulation, bitCount, maxValue, modulus, scenario);
        }

        // Phase 4: カウントダウンシナリオ群
        foreach (var scenario in cfg.CountDownScenarios) {
            SetupCountScenario(simulation, bitCount, scenario.StartValue, isUp: false);
            RunCountScenario(simulation, bitCount, maxValue, modulus, scenario);
        }
    }

    private static void SetupCountScenario(LogicSimulation simulation, int bitCount,
        int startValue, bool isUp) {
        for (int j = 0; j < bitCount; j++) {
            simulation.SetInput($"INITIAL{j}", 0, ((startValue & (1 << j)) != 0).ToSignal());
        }
        simulation.SetInput("SET", 0, LogicSignal.High);
        simulation.Step();

        simulation.SetInput("SET", 0, LogicSignal.Low);
        // LOW=High: 上位ビットからのキャリー入力をHighにし、カウンタを動作可能にする
        simulation.SetInput("LOW", 0, LogicSignal.High);
        simulation.SetInput("DIR", 0, isUp ? LogicSignal.Low : LogicSignal.High);
        simulation.Step();
        // この時点でHIが組み合わせ論理として確定
    }

    private static void RunCountScenario(LogicSimulation simulation, int bitCount,
        int maxValue, int modulus, CountScenario scenario) {
        int startValue = scenario.StartValue;
        bool isUp      = scenario.IsUp;
        string label   = scenario.Label;

        // 初期HI確認（クロック前）
        bool hiInitial = (isUp && startValue == maxValue) || (!isUp && startValue == 0);
        Assert.That(simulation.GetOutput("HI", 0), Is.EqualTo(hiInitial.ToSignal()),
            $"{label} pre-clk HI");

        for (int i = 0; i < scenario.ClockCount; i++) {
            simulation.SetInput("CLK", 0, LogicSignal.High);
            simulation.Step();
            simulation.SetInput("CLK", 0, LogicSignal.Low);
            simulation.Step();

            int expectedValue = isUp
                ? (startValue + i + 1) % modulus
                : ((startValue - (i + 1)) & maxValue);

            using (Assert.EnterMultipleScope()) {
                for (int j = 0; j < bitCount; j++) {
                    bool expectedBit = (expectedValue & (1 << j)) != 0;
                    Assert.That(simulation.GetOutput($"D{j}", 0), Is.EqualTo(expectedBit.ToSignal()),
                        $"{label} i={i}: D{j}");
                }
                bool hiExpected = (isUp && expectedValue == maxValue) || (!isUp && expectedValue == 0);
                Assert.That(simulation.GetOutput("HI", 0), Is.EqualTo(hiExpected.ToSignal()),
                    $"{label} i={i}: HI");
            }
        }
    }

    public static IReadOnlyList<SignalTestPattern> DFF_Data = [
        // 1. ~Clr~=Low でクリア → Q=Low, ~Q~=High
        new([
            new([ new("~Clr~", false) ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // 2. ~Set~=Low でセット → Q=High, ~Q~=Low
        new([
            new([ new("~Set~", false) ], [ new("Q", true), new("~Q~", false) ])
        ]),
        // 3. 通常動作: D=High、C 立ち下がり → Q=High
        new([
            new([ new("~Clr~", false) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("~Clr~", true) ], []),
            new([ new("D", true), new("C", true) ], []),
            new([ new("C", false) ], [ new("Q", true), new("~Q~", false) ])
        ]),
        // 4. 通常動作: D=Low、C 立ち下がり → Q=Low
        new([
            new([ new("~Set~", false) ], [ new("Q", true), new("~Q~", false) ]),
            new([ new("~Set~", true) ], []),
            new([ new("D", false), new("C", true) ], []),
            new([ new("C", false) ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // 5. 禁止パターン: ~Set~=Low かつ ~Clr~=Low → Q=High, ~Q~=High（両方High）
        new([
            new([ new("~Set~", false), new("~Clr~", false) ], [ new("Q", true), new("~Q~", true) ])
        ]),
        // 6. クロック保持中（C=High）は D 変化しても Q 不変
        new([
            new([ new("~Clr~", false) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("~Clr~", true) ], []),
            new([ new("C", true) ], []),
            new([ new("D", true)  ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("D", false) ], [ new("Q", false), new("~Q~", true) ])
        ]),
    ];

    [CancelAfter(1000)]
    [TestCaseSource(nameof(DFF_Data))]
    public void DFF_SignalTest(SignalTestPattern testPattern) {
        var simulation = new LogicSimulation(BuiltInCircuit.D_FF, BuiltInCircuit.Circuits);
        simulation.SetInput("~Clr~", 0, LogicSignal.High);
        simulation.SetInput("~Set~", 0, LogicSignal.High);
        simulation.SetInput("C",     0, LogicSignal.Low);
        simulation.SetInput("D",     0, LogicSignal.Low);
        simulation.Step();

        foreach (var frame in testPattern.Frames) {
            foreach (var input in frame.Inputs) {
                simulation.SetInput(input.PinName, 0, input.Value.ToSignal());
            }
            simulation.Step();
            foreach (var expected in frame.Expecteds) {
                Assert.That(simulation.GetOutput(expected.PinName, 0), Is.EqualTo(expected.Value.ToSignal()));
            }
        }
    }

    public static IEnumerable<TestCaseData> MultiplexerTestCases => [
        new TestCaseData(1, 1).SetName("dataBit=1, selector=1 (2ch)"),
        new TestCaseData(1, 2).SetName("dataBit=1, selector=2 (4ch)"),
        new TestCaseData(4, 1).SetName("dataBit=4, selector=1 (2ch)"),
        new TestCaseData(4, 2).SetName("dataBit=4, selector=2 (4ch)"),
    ];

    [TestCaseSource(nameof(MultiplexerTestCases))]
    [CancelAfter(10000)]
    public void Multiplexer_DataDriven(int dataBit, int numOfSelectorBit) {
        int numOfChannel = 1 << numOfSelectorBit;

        // テスト: 全セレクタ値 × 全データ値
        for (int sel = 0; sel < numOfChannel; sel++) {
            for (int dataVal = 0; dataVal < (1 << dataBit); dataVal++) {
                var sim = new LogicSimulation(
                    BuiltInCircuit.CreateMultiplexer(dataBit, numOfSelectorBit),
                    BuiltInCircuit.Circuits);

                // 選択されるチャンネルに dataVal をセット、その他は反転値をセット
                // これによりチャンネル選択ミスを確実に検出できる
                for (int ch = 0; ch < numOfChannel; ch++) {
                    for (int b = 0; b < dataBit; b++) {
                        int val = (ch == sel)
                            ? (dataVal >> b) & 1
                            : ~(dataVal >> b) & 1;  // 選択チャンネルと必ず異なる値
                        sim.SetInput($"D{ch}_{b}", 0, (val != 0).ToSignal());
                    }
                }

                // セレクタをセット
                for (int i = 0; i < numOfSelectorBit; i++) {
                    sim.SetInput($"S{i}", 0, (((sel >> i) & 1) != 0).ToSignal());
                }

                sim.Step();

                // 出力確認: Y{b} == D{sel}_{b}
                using (Assert.EnterMultipleScope()) {
                    for (int b = 0; b < dataBit; b++) {
                        Assert.That(
                            sim.GetOutput($"Y{b}", 0),
                            Is.EqualTo((((dataVal >> b) & 1) != 0).ToSignal()),
                            $"sel={sel}, dataVal=0x{dataVal:X}: Y{b}");
                    }
                }
            }
        }
    }

    public static IEnumerable<TestCaseData> DemultiplexerTestCases => [
        new TestCaseData(1, 1).SetName("dataBit=1, selector=1 (2ch)"),
        new TestCaseData(1, 2).SetName("dataBit=1, selector=2 (4ch)"),
        new TestCaseData(4, 1).SetName("dataBit=4, selector=1 (2ch)"),
        new TestCaseData(4, 2).SetName("dataBit=4, selector=2 (4ch)"),
    ];

    [TestCaseSource(nameof(DemultiplexerTestCases))]
    [CancelAfter(10000)]
    public void Demultiplexer_DataDriven(int dataBit, int numOfSelectorBit) {
        int numOfChannel = 1 << numOfSelectorBit;

        // テスト: 全セレクタ値 × 全データ値
        for (int sel = 0; sel < numOfChannel; sel++) {
            for (int dataVal = 0; dataVal < (1 << dataBit); dataVal++) {
                var sim = new LogicSimulation(
                    BuiltInCircuit.CreateDemultiplexer(dataBit, numOfSelectorBit),
                    BuiltInCircuit.Circuits);

                // データ入力
                for (int b = 0; b < dataBit; b++) {
                    sim.SetInput($"D{b}", 0, (((dataVal >> b) & 1) != 0).ToSignal());
                }

                // セレクタ
                for (int i = 0; i < numOfSelectorBit; i++) {
                    sim.SetInput($"S{i}", 0, (((sel >> i) & 1) != 0).ToSignal());
                }

                sim.Step();

                // 出力確認: Y{sel}_{b} == D{b}、その他チャンネルは全 Low
                using (Assert.EnterMultipleScope()) {
                    for (int ch = 0; ch < numOfChannel; ch++) {
                        for (int b = 0; b < dataBit; b++) {
                            LogicSignal expected = (ch == sel)
                                ? (((dataVal >> b) & 1) != 0).ToSignal()
                                : LogicSignal.Low;
                            Assert.That(
                                sim.GetOutput($"Y{ch}_{b}", 0),
                                Is.EqualTo(expected),
                                $"sel={sel}, dataVal=0x{dataVal:X}: Y{ch}_{b}");
                        }
                    }
                }
            }
        }
    }

    public record RangeCounter16BitTestCase(
        int A,
        int B,
        int Initial,
        int Max,
        int ExpectedRangeInitial,
        string Label
    );

    public static IEnumerable<TestCaseData> RangeCounter16BitTestCases => [
        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 15,
            Max: 65535,
            ExpectedRangeInitial: 1,
            Label: "in_range_middle"
        )).SetName("in_range_middle"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 5,
            Max: 65535,
            ExpectedRangeInitial: 0,
            Label: "out_range_below"
        )).SetName("out_range_below"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 25,
            Max: 65535,
            ExpectedRangeInitial: 0,
            Label: "out_range_above"
        )).SetName("out_range_above"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 10,
            Max: 65535,
            ExpectedRangeInitial: 1,
            Label: "at_lower_bound"
        )).SetName("at_lower_bound"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 20,
            Max: 65535,
            ExpectedRangeInitial: 1,
            Label: "at_upper_bound"
        )).SetName("at_upper_bound"),
    ];

    [TestCaseSource(nameof(RangeCounter16BitTestCases))]
    [CancelAfter(10000)]
    public void RangeCounter16Bit_DataDriven(RangeCounter16BitTestCase testCase) {
        var sim = new LogicSimulation(
            BuiltInCircuit.RangeCounter16Bit,
            BuiltInCircuit.Circuits);

        InitializeRangeCounter(sim);

        SetupRangeCounterInputs(sim, testCase.A, testCase.B, testCase.Initial, testCase.Max);

        sim.SetInput("SET", 0, LogicSignal.High);
        sim.Step();

        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.Step();

        CheckRangeOutput(sim, testCase.ExpectedRangeInitial, testCase.Label);

        CheckCounterOutput(sim, testCase.Initial, testCase.Label);
    }

    private static void InitializeRangeCounter(LogicSimulation sim) {
        sim.SetInput("LOW", 0, LogicSignal.Low);
        sim.SetInput("ZERO", 0, LogicSignal.Low);
        sim.SetInput("CLK", 0, LogicSignal.Low);
        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.SetInput("DIR", 0, LogicSignal.Low);

        for (int i = 0; i < 16; i++) {
            sim.SetInput($"INITIAL{i}", 0, LogicSignal.Low);
            sim.SetInput($"A{i}", 0, LogicSignal.Low);
            sim.SetInput($"B{i}", 0, LogicSignal.Low);
            sim.SetInput($"MAX{i}", 0, LogicSignal.Low);
        }

        sim.Step();

        sim.SetInput("SET", 0, LogicSignal.High);
        sim.Step();
        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.Step();
    }

    private static void SetupRangeCounterInputs(LogicSimulation sim, int a, int b, int initial, int max) {
        for (int i = 0; i < 16; i++) {
            sim.SetInput($"INITIAL{i}", 0, ((initial & (1 << i)) != 0).ToSignal());
            sim.SetInput($"A{i}", 0, ((a & (1 << i)) != 0).ToSignal());
            sim.SetInput($"B{i}", 0, ((b & (1 << i)) != 0).ToSignal());
            sim.SetInput($"MAX{i}", 0, ((max & (1 << i)) != 0).ToSignal());
        }
    }

    private static void CheckRangeOutput(LogicSimulation sim, int expectedRange, string label) {
        LogicSignal expected = (expectedRange != 0) ? LogicSignal.High : LogicSignal.Low;
        Assert.That(
            sim.GetOutput("RANGE", 0),
            Is.EqualTo(expected),
            $"{label}: RANGE");
    }

    private static void CheckCounterOutput(LogicSimulation sim, int expectedValue, string label) {
        for (int i = 0; i < 16; i++) {
            LogicSignal expected = ((expectedValue & (1 << i)) != 0) ? LogicSignal.High : LogicSignal.Low;
            Assert.That(
                sim.GetOutput($"D{i}", 0),
                Is.EqualTo(expected),
                $"{label}: D{i}");
        }
    }

    private static void SimulateSetRelease(LogicSimulation sim) {
        sim.SetInput("SET", 0, LogicSignal.High);
        sim.Step();
        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.Step();
    }

    private static void SimulateCountUp(LogicSimulation sim, int count) {
        for (int i = 0; i < count; i++) {
            sim.SetInput("CLK", 0, LogicSignal.High);
            sim.Step();
            sim.SetInput("CLK", 0, LogicSignal.Low);
            sim.Step();
        }
    }
}

/// <summary>
/// LogicPinReader 単体テスト
/// 入力ピンの変化検出と素子番号の取得機能を検証
/// </summary>
[TestFixture]
public class LogicPinReaderTests {
    [Test]
    public void ReadBit_ReturnsCorrectPinValue() {
        // Arrange: 基本的なLogicPinsを作成
        var pins = new LogicPins {
            Pins = [LogicSignal.High, LogicSignal.Low, LogicSignal.High, LogicSignal.Low],
            NumOfPins = [(0, 2), (2, 2)],
            PinNumberToLogicNumber = [0, 0, 1, 1]
        };

        var changedPins = new List<int> { 0, 2 };
        var isExecutedLogicNumbers = new[] { false, false };

        var reader = new LogicPinReader(pins, changedPins, isExecutedLogicNumbers, 0);

        Assert.That(reader.ReadBit(0, 0), Is.EqualTo(LogicSignal.High));
        Assert.That(reader.ReadBit(0, 1), Is.EqualTo(LogicSignal.Low));
        Assert.That(reader.ReadBit(1, 0), Is.EqualTo(LogicSignal.High));
        Assert.That(reader.ReadBit(1, 1), Is.EqualTo(LogicSignal.Low));
    }

    [Test]
    public void GetPinsLength_ReturnsCorrectLength() {
        // Arrange
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low, LogicSignal.High ],
            NumOfPins = [(0, 2), (2, 1)],
            PinNumberToLogicNumber = [0, 0, 1]
        };

        var reader = new LogicPinReader(pins, [], [false, false], 0);

        Assert.That(reader.GetPinsLength(0), Is.EqualTo(2));
        Assert.That(reader.GetPinsLength(1), Is.EqualTo(1));
    }

    [Test]
    public void TryGetNextChangedLogicNumber_ReturnsChangedLogicNumbers() {
        // Logic 0のPin 0, Logic 1のPin 0が変化した
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low, LogicSignal.High, LogicSignal.Low ], 
            NumOfPins = [(0, 2), (2, 2)],
            PinNumberToLogicNumber = [0, 0, 1, 1]
        };

        var changedPins = new List<int> { 0, 2 }; // Logic 0のPin 0, Logic 1のPin 0
        var isExecutedLogicNumbers = new[] { false, false };

        var reader = new LogicPinReader(pins, changedPins, isExecutedLogicNumbers, 0);

        Assert.That(reader.TryGetNextChangedLogicNumber(out int firstLogicNo), Is.True);
        Assert.That(firstLogicNo, Is.EqualTo(0));
        
        Assert.That(reader.TryGetNextChangedLogicNumber(out int secondLogicNo), Is.True);
        Assert.That(secondLogicNo, Is.EqualTo(1));
        Assert.That(reader.TryGetNextChangedLogicNumber(out int thirdLogicNo), Is.False);
    }

    [Test]
    public void TryGetNextChangedLogicNumber_SkipsDuplicates() {
        // Arrange: Logic 0のPin 0と1が両方変化（Logic 0は1回だけ返す）
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low, LogicSignal.High],
            NumOfPins = [(0, 2), (2, 1)],
            PinNumberToLogicNumber = [0, 0, 1]
        };

        var changedPins = new List<int> { 0, 1 }; // Logic 0の両方のピン
        var isExecutedLogicNumbers = new[] { false, false };

        var reader = new LogicPinReader(pins, changedPins, isExecutedLogicNumbers, 0);

        Assert.That(reader.TryGetNextChangedLogicNumber(out int firstLogicNo), Is.True);
        Assert.That(firstLogicNo, Is.EqualTo(0));
        Assert.That(reader.TryGetNextChangedLogicNumber(out int secondLogicNo), Is.False);
    }
}

/// <summary>
/// LogicPinsWriter 単体テスト
/// ピンへの書き込みと変更追跡機能を検証
/// </summary>
[TestFixture]
public class LogicPinsWriterTests {
    [Test]
    public void WriteBit_UpdatesPinValue() {
        // Arrange
        var pins = new LogicPins {
            Pins = [ LogicSignal.Low, LogicSignal.Low, LogicSignal.Low, LogicSignal.Low],
            NumOfPins = [(0, 2), (2, 2)],
            PinNumberToLogicNumber = [0, 0, 1, 1]
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        writer.WriteBit(0, 0, LogicSignal.High);
        writer.WriteBit(1, 1, LogicSignal.High);

        Assert.That(pins.Pins[0], Is.EqualTo(LogicSignal.High));
        Assert.That(pins.Pins[1], Is.EqualTo(LogicSignal.Low));
        Assert.That(pins.Pins[2], Is.EqualTo(LogicSignal.Low));
        Assert.That(pins.Pins[3], Is.EqualTo(LogicSignal.High));
    }

    [Test]
    public void WriteBit_TracksChangedPins() {
        // Arrange
        var pins = new LogicPins {
            Pins = [ LogicSignal.Low, LogicSignal.Low ],
            NumOfPins = [(0, 2)],
            PinNumberToLogicNumber = [0, 0]
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        writer.WriteBit(0, 0, LogicSignal.High);  // 変更あり
        writer.WriteBit(0, 1, LogicSignal.Low); // 変更なし（既にfalse）
        writer.WriteBit(0, 0, LogicSignal.Low); // 変更あり

        Assert.That(changedPins.Count, Is.EqualTo(2));
        Assert.That(changedPins[0], Is.EqualTo(0));
        Assert.That(changedPins[1], Is.EqualTo(0));
    }

    [Test]
    public void GetPinsLength_ReturnsCorrectLength() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.Low, LogicSignal.Low, LogicSignal.Low ],
            NumOfPins = [(0, 2), (2, 1)],
            PinNumberToLogicNumber = [0, 0, 1]
        };

        var writer = new LogicPinsWriter(pins, new List<int>());

        Assert.That(writer.GetPinsLength(0), Is.EqualTo(2));
        Assert.That(writer.GetPinsLength(1), Is.EqualTo(1));
    }

    [Test]
    public void ReadBit_ReturnsCurrentValue() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low ],
            NumOfPins = [(0, 2)],
            PinNumberToLogicNumber = [0, 0]
        };

        var writer = new LogicPinsWriter(pins, []);

        Assert.That(writer.ReadBit(0, 0), Is.EqualTo(LogicSignal.High));
        Assert.That(writer.ReadBit(0, 1), Is.EqualTo(LogicSignal.Low));
    }

    [Test]
    public void WriteBit_MultipleWrites_TrackEachChange() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.Low, LogicSignal.Low, LogicSignal.Low, LogicSignal.Low ],
            NumOfPins = [(0, 2), (2, 2)],
            PinNumberToLogicNumber = [0, 0, 1, 1]
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        // 複数の異なるピンに書き込み
        writer.WriteBit(0, 0, LogicSignal.High);
        writer.WriteBit(0, 1, LogicSignal.High);
        writer.WriteBit(1, 0, LogicSignal.High);
        writer.WriteBit(1, 1, LogicSignal.High);

        Assert.That(changedPins.Count, Is.EqualTo(4));
        Assert.That(pins.Pins.All(p => p == LogicSignal.High), Is.True);
    }
}