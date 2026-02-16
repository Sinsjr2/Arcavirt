using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

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
    /// 1ビット比較器、<、==、> を判定する
    /// </summary>
    public static readonly Circuit Comparator1Bit = new([
            new("A", new InputConnector(1)),
            new("B", new InputConnector(1)),
            new("GT", new OutputConnector(1)),
            new("EQ", new OutputConnector(1)),
            new("LE", new OutputConnector(1)),
            new("not1", new NotLogic()),
            new("not2", new NotLogic()),
            new("and1", new AndLogic(2)),
            new("and2", new AndLogic(2)),
            new("and3", new AndLogic(2)),
            new("and4", new AndLogic(2)),
            new("or1", new OrLogic(2)),
        ],
        [
            new(new LogicConnector("A", "out"), new LogicConnector("and1", "in[0]")),
            new(new LogicConnector("A", "out"), new LogicConnector("not1", "in")),
            new(new LogicConnector("A", "out"), new LogicConnector("and3", "in[0]")),
            new(new LogicConnector("B", "out"), new LogicConnector("not2", "in")),
            new(new LogicConnector("B", "out"), new LogicConnector("and3", "in[1]")),
            new(new LogicConnector("B", "out"), new LogicConnector("and4", "in[1]")),
            new(new LogicConnector("not1", "out"), new LogicConnector("and2", "in[0]")),
            new(new LogicConnector("not1", "out"), new LogicConnector("and4", "in[0]")),
            new(new LogicConnector("not2", "out"), new LogicConnector("and1", "in[1]")),
            new(new LogicConnector("not2", "out"), new LogicConnector("and2", "in[1]")),
            new(new LogicConnector("and1", "out"), new LogicConnector("GT", "in")),
            new(new LogicConnector("and2", "out"), new LogicConnector("or1", "in[0]")),
            new(new LogicConnector("and3", "out"), new LogicConnector("or1", "in[1]")),
            new(new LogicConnector("or1", "out"), new LogicConnector("EQ", "in")),
            new(new LogicConnector("and4", "out"), new LogicConnector("LE", "in")),
        ]);

    public static readonly IReadOnlyDictionary<string, Circuit> Circuits = new Dictionary<string, Circuit> {
        { "jk_ff_preset_clear", JK_FFMasterSlavePresetClear }
    };
}

[TestFixture]
public class LogicSimulationTests {

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

    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.X, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.X, LogicSignal.High, LogicSignal.X)]
    [TestCase(LogicSignal.Low, LogicSignal.X, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.X, LogicSignal.X)]
    public void AndLogicTest(LogicSignal input1, LogicSignal input2, LogicSignal expected) {
        var circuit = new Circuit([
                new("input1", new InputConnector(1)),
                new("input2", new InputConnector(1)),
                new("and1", new AndLogic(2)),
                new("output", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("input1", "out"), new LogicConnector("and1", "in[0]")),
                new(new LogicConnector("input2", "out"), new LogicConnector("and1", "in[1]")),
                new(new LogicConnector("and1", "out"), new LogicConnector("output", "in"))
            ]);

        var simulation = new LogicSimulation(circuit);

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.X, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.Low, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.Low, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.High, LogicSignal.X, LogicSignal.High)]
    public void OrLogicTest(LogicSignal input1, LogicSignal input2, LogicSignal expected) {
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

        var simulation = new LogicSimulation(circuit);

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
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

    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.X, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.X, LogicSignal.High, LogicSignal.X)]
    [TestCase(LogicSignal.Low, LogicSignal.X, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.X, LogicSignal.X)]
    public void NAndLogicTest(LogicSignal input1, LogicSignal input2, LogicSignal expected) {
        var circuit = new Circuit([
                new("input1", new InputConnector(1)),
                new("input2", new InputConnector(1)),
                new("nand1", new NAndLogic(2)),
                new("output", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("input1", "out"), new LogicConnector("nand1", "in[0]")),
                new(new LogicConnector("input2", "out"), new LogicConnector("nand1", "in[1]")),
                new(new LogicConnector("nand1", "out"), new LogicConnector("output", "in"))
            ]);

        var simulation = new LogicSimulation(circuit);

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.X, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.Low, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.High, LogicSignal.X, LogicSignal.Low)]
    public void NOrLogicTest(LogicSignal input1, LogicSignal input2, LogicSignal expected) {
        var circuit = new Circuit([
                new("input1", new InputConnector(1)),
                new("input2", new InputConnector(1)),
                new("nor", new NOrLogic(2)),
                new("output", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("input1", "out"), new LogicConnector("nor", "in[0]")),
                new(new LogicConnector("input2", "out"), new LogicConnector("nor", "in[1]")),
                new(new LogicConnector("nor", "out"), new LogicConnector("output", "in"))
            ]);

        var simulation = new LogicSimulation(circuit);

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.X, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.Low, LogicSignal.X)]
    [TestCase(LogicSignal.X, LogicSignal.High, LogicSignal.X)]
    [TestCase(LogicSignal.Low, LogicSignal.X, LogicSignal.X)]
    [TestCase(LogicSignal.High, LogicSignal.X, LogicSignal.X)]
    public void XOrLogicTest(LogicSignal input1, LogicSignal input2, LogicSignal expected) {
        var circuit = new Circuit([
                new("input1", new InputConnector(1)),
                new("input2", new InputConnector(1)),
                new("xor", new XOrLogic(2)),
                new("output", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("input1", "out"), new LogicConnector("xor", "in[0]")),
                new(new LogicConnector("input2", "out"), new LogicConnector("xor", "in[1]")),
                new(new LogicConnector("xor", "out"), new LogicConnector("output", "in"))
            ]);

        var simulation = new LogicSimulation(circuit);

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
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

    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.Low, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.Low, LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.High, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.Low, LogicSignal.High, LogicSignal.Low)]
    public void Comparator1Bit(LogicSignal a, LogicSignal b, LogicSignal expectedGT, LogicSignal expectedEQ, LogicSignal expectedLE) {
        var simulation = new LogicSimulation(BuiltInCircuit.Comparator1Bit);
        
        simulation.SetInput("A", 0, a);
        simulation.SetInput("B", 0, b);
        simulation.Step();
        
        Assert.That(simulation.GetOutput("GT", 0), Is.EqualTo(expectedGT));
        Assert.That(simulation.GetOutput("EQ", 0), Is.EqualTo(expectedEQ));
        Assert.That(simulation.GetOutput("LE", 0), Is.EqualTo(expectedLE));
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