using System;
using System.Collections.Generic;

namespace LogicSimulator;

/// <summary>
/// よく使う回路を定義しています。
/// </summary>
public class BuiltInCircuits {

    public static readonly Circuit JK_FFMasterSlavePresetClear = new([
            new("~PRE~", new InputConnector()),
            new("J", new InputConnector()),
            new("K", new InputConnector()),
            new("CLK", new InputConnector()),
            new("~CLR~", new InputConnector()),
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
            new("Q", new OutputConnector()),
            new("~Q~", new OutputConnector())
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
    /// D型フリップフロップ。
    ///
    /// ピン一覧:
    /// - Clr : クリア。
    /// - C     : クロック入力。立ち上がりエッジで D の値を取り込みます。
    /// - D     : データ入力。クロック立ち上がり時にこの値が Q に転送されます。
    /// - Set : セット。
    /// - Q     : 出力。
    /// - ~Q~   : Q の反転出力。
    ///
    /// 真理値表:
    /// <code>
    /// Set | Clr | D | C  | Q | ~Q~ | 説明
    /// ----|-----|---|----|---|-----|--------------------------------------
    ///   L |  L  | X | ↑ | D | ~D~ | 通常動作(立ち上がりエッジでラッチ)
    ///   H |  X  | X | X  | H |  L  | セット有効
    ///   L |  H  | X | X  | L |  H  | クリア有効
    /// </code>
    /// </summary>
    public static readonly Circuit D_FF = new([
            new("Clr",     new InputConnector()),
            new("C",       new InputConnector()),
            new("D",       new InputConnector()),
            new("Set",     new InputConnector()),
            new("not_set", new NotLogic()),
            new("not_clr", new NotLogic()),
            new("not_d",   new NotLogic()),
            new("not_clk", new NotLogic()),
            new("or_clr",  new OrLogic(2)),
            new("jkff1",   new CustomCircuit("jk_ff_preset_clear")),
            new("Q",       new OutputConnector()),
            new("~Q~",     new OutputConnector())
        ],
        [
            new(new LogicConnector("Set",     "out"), new LogicConnector("not_set", "in")),
            new(new LogicConnector("not_set", "out"), new LogicConnector("jkff1",   "~PRE~")),
            new(new LogicConnector("Clr",     "out"), new LogicConnector("not_clr", "in")),
            new(new LogicConnector("not_clr", "out"), new LogicConnector("or_clr",  "in[0]")),
            new(new LogicConnector("Set",     "out"), new LogicConnector("or_clr",  "in[1]")),
            new(new LogicConnector("or_clr",  "out"), new LogicConnector("jkff1",   "~CLR~")),
            new(new LogicConnector("C",       "out"), new LogicConnector("not_clk", "in")),
            new(new LogicConnector("not_clk", "out"), new LogicConnector("jkff1",   "CLK")),
            new(new LogicConnector("D",       "out"), new LogicConnector("jkff1",   "J")),
            new(new LogicConnector("D",       "out"), new LogicConnector("not_d",   "in")),
            new(new LogicConnector("not_d",   "out"), new LogicConnector("jkff1",   "K")),
            new(new LogicConnector("jkff1",   "Q"),   new LogicConnector("Q",       "in")),
            new(new LogicConnector("jkff1",   "~Q~"), new LogicConnector("~Q~",     "in")),
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
            // 1ビットカウンタ: 直接配線(CustomCircuit を使わない)
            return new([
                new("LOW", new InputConnector()),
                new("SET", new InputConnector()),
                new("CLK", new InputConnector()),
                new("DIR", new InputConnector()),
                new("INITIAL0", new InputConnector()),
                new("HI", new OutputConnector()),
                new("D0", new OutputConnector()),
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
            nodes.Add(new("LOW", new InputConnector()));
            nodes.Add(new("SET", new InputConnector()));
            nodes.Add(new("CLK", new InputConnector()));
            nodes.Add(new("DIR", new InputConnector()));
            for (int i = 0; i < bitCount; i++) {
                nodes.Add(new($"INITIAL{i}", new InputConnector()));
            }
            nodes.Add(new("HI", new OutputConnector()));
            for (int i = 0; i < bitCount; i++) {
                nodes.Add(new($"D{i}", new OutputConnector()));
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
                new("A0", new InputConnector()),
                new("B0", new InputConnector()),
                new("GT", new OutputConnector()),
                new("EQ", new OutputConnector()),
                new("LT", new OutputConnector()),
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
            nodes.Add(new($"A{i}", new InputConnector()));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"B{i}", new InputConnector()));
        }
        nodes.Add(new("GT", new OutputConnector()));
        nodes.Add(new("EQ", new OutputConnector()));
        nodes.Add(new("LT", new OutputConnector()));

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
    /// 入力: CLK, SET, DIR, INITIAL{i} (i=0..15), A{i} (i=0..15), B{i} (i=0..15), MAX{i} (i=0..15)
    /// 出力: RANGE, D{i} (i=0..15)
    ///
    /// 動作:
    /// - カウンタが A <= D <= B の範囲内にあるとき RANGE=1
    /// - SET=1 中は RANGE を強制的に High に
    /// - オーバーフロー時に MAX 値をラップアラウンド値として使用
    /// </summary>
    public static Circuit CreateRangeCounter16Bit() {
        var nodes = new List<LogicNode>();
        var wires = new List<LogicConnection>();

        int bitCount = 16;

        nodes.Add(new("LOW",  ConstValueLogic.FromBool(true)));
        nodes.Add(new("ZERO", ConstValueLogic.FromBool(false)));
        nodes.Add(new("CLK", new InputConnector()));
        nodes.Add(new("SET", new InputConnector()));
        nodes.Add(new("DIR", new InputConnector()));

        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"INITIAL{i}", new InputConnector()));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"A{i}", new InputConnector()));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"B{i}", new InputConnector()));
        }
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"MAX{i}", new InputConnector()));
        }

        nodes.Add(new("RANGE", new OutputConnector()));
        for (int i = 0; i < bitCount; i++) {
            nodes.Add(new($"D{i}", new OutputConnector()));
        }

        nodes.Add(new("not1", new NotLogic()));
        nodes.Add(new("counter", new CustomCircuit("ud_counter_16bit")));
        nodes.Add(new("comp1", new CustomCircuit("comparator_16bit")));
        nodes.Add(new("comp2", new CustomCircuit("comparator_16bit")));
        nodes.Add(new("comp3", new CustomCircuit("comparator_16bit")));
        nodes.Add(new("comp4", new CustomCircuit("comparator_16bit")));
        nodes.Add(new("dff", new CustomCircuit("d_ff")));

        nodes.Add(new($"mux1", new CustomCircuit("mux_16bit_1sel")));
        nodes.Add(new($"mux2", new CustomCircuit("mux_16bit_1sel")));
        nodes.Add(new("mux3", new CustomCircuit("mux_1bit_1sel")));

        nodes.Add(new("and1", new AndLogic(2)));
        nodes.Add(new("and2", new AndLogic(2)));

        nodes.Add(new("or1", new OrLogic(2)));
        nodes.Add(new("or2", new OrLogic(2)));
        nodes.Add(new("or3", new OrLogic(2)));
        nodes.Add(new("or4", new OrLogic(2)));

        wires.Add(new(new LogicConnector("LOW", "out[0]"), new LogicConnector("counter", "LOW")));
        wires.Add(new(new LogicConnector("CLK", "out"), new LogicConnector("counter", "CLK")));
        wires.Add(new(new LogicConnector("CLK", "out"), new LogicConnector("not1", "in")));
        wires.Add(new(new LogicConnector("CLK", "out"), new LogicConnector("dff", "C")));

        wires.Add(new(new LogicConnector("DIR", "out"), new LogicConnector("counter", "DIR")));
        wires.Add(new(new LogicConnector("SET", "out"), new LogicConnector("or1", "in[1]")));
        wires.Add(new(new LogicConnector("SET", "out"), new LogicConnector("dff", "Clr")));

        wires.Add(new(new LogicConnector("not1", "out"), new LogicConnector("and1", "in[0]")));
        wires.Add(new(new LogicConnector("and1", "out"), new LogicConnector("or1", "in[0]")));
        wires.Add(new(new LogicConnector("or1", "out"), new LogicConnector("counter", "SET")));

        for (int i = 0; i < bitCount; i++) {
            wires.Add(new(new LogicConnector("ZERO", "out[0]"), new LogicConnector("mux1", $"D0_{i}")));
            wires.Add(new(new LogicConnector($"MAX{i}", "out"), new LogicConnector("mux1", $"D1_{i}")));
            wires.Add(new(new LogicConnector("mux1", $"Y{i}"), new LogicConnector($"mux2", $"D0_{i}")));
            wires.Add(new(new LogicConnector($"INITIAL{i}", "out"), new LogicConnector("mux2", $"D1_{i}")));
            wires.Add(new(new LogicConnector("mux2", $"Y{i}"), new LogicConnector("counter", $"INITIAL{i}")));
        }
        wires.Add(new(new LogicConnector("DIR", "out"), new LogicConnector($"mux1", "S0")));
        wires.Add(new(new LogicConnector("SET", "out"), new LogicConnector("mux2", "S0")));

        for (int i = 0; i < bitCount; i++) {
            wires.Add(new(new LogicConnector($"A{i}", "out"), new LogicConnector("comp1", $"A{i}")));
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector("comp1", $"B{i}")));
            wires.Add(new(new LogicConnector($"B{i}", "out"), new LogicConnector("comp2", $"A{i}")));
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector("comp2", $"B{i}")));
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector($"D{i}", "in")));
        }

        wires.Add(new(new LogicConnector("comp1", "EQ"), new LogicConnector("or2", "in[0]")));
        wires.Add(new(new LogicConnector("comp1", "LT"), new LogicConnector("or2", "in[1]")));
        wires.Add(new(new LogicConnector("comp2", "GT"), new LogicConnector("or3", "in[0]")));
        wires.Add(new(new LogicConnector("comp2", "EQ"), new LogicConnector("or3", "in[1]")));

        wires.Add(new(new LogicConnector("or2", "out"), new LogicConnector("and2", "in[0]")));
        wires.Add(new(new LogicConnector("or3", "out"), new LogicConnector("and2", "in[1]")));

        for (int i = 0; i < bitCount; i++) {
            wires.Add(new(new LogicConnector($"MAX{i}", "out"), new LogicConnector("comp3", $"A{i}")));
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector("comp3", $"B{i}")));
            wires.Add(new(new LogicConnector("ZERO", "out[0]"), new LogicConnector("comp4", $"A{i}")));
            wires.Add(new(new LogicConnector("counter", $"D{i}"), new LogicConnector("comp4", $"B{i}")));
        }

        wires.Add(new(new LogicConnector("comp3", "EQ"), new LogicConnector("or4", "in[0]")));
        wires.Add(new(new LogicConnector("comp3", "LT"), new LogicConnector("or4", "in[1]")));
        wires.Add(new(new LogicConnector("or4", "out"), new LogicConnector("mux3", "D0_0")));
        wires.Add(new(new LogicConnector("comp4", "EQ"), new LogicConnector("mux3", "D1_0")));
        wires.Add(new(new LogicConnector("DIR", "out"), new LogicConnector("mux3", "S0")));

        wires.Add(new(new LogicConnector("and2", "out"), new LogicConnector("RANGE", "in")));
        wires.Add(new(new LogicConnector("dff", "Q"), new LogicConnector("and1", "in[1]")));

        wires.Add(new(new LogicConnector("ZERO", "out[0]"), new LogicConnector("dff", "Set")));
        wires.Add(new(new LogicConnector("mux3", "Y0"), new LogicConnector("dff", "D")));

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
            nodes.Add(new($"S{i}", new InputConnector()));
        }

        // データ入力 D{ch}_{b} (チャンネル順、ビット順)
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                nodes.Add(new($"D{ch}_{b}", new InputConnector()));
            }
        }

        // 出力 Y{b}
        for (int b = 0; b < dataBit; b++) {
            nodes.Add(new($"Y{b}", new OutputConnector()));
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
            nodes.Add(new($"S{i}", new InputConnector()));
        }

        // データ入力 D{b}
        for (int b = 0; b < dataBit; b++) {
            nodes.Add(new($"D{b}", new InputConnector()));
        }

        // 出力 Y{ch}_{b}
        for (int ch = 0; ch < numOfChannel; ch++) {
            for (int b = 0; b < dataBit; b++) {
                nodes.Add(new($"Y{ch}_{b}", new OutputConnector()));
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

        // データ AND: D{b}.out → and_{ch}_{b}.in[1](全チャンネル共通)
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
        { "ud_counter_16bit", UDCounter16Bit },
        { "mux_1bit_1sel", CreateMultiplexer(1, 1) },
        { "mux_2bit_1sel", CreateMultiplexer(2, 1) },
        { "mux_16bit_1sel", CreateMultiplexer(16, 1) },
    };
}
