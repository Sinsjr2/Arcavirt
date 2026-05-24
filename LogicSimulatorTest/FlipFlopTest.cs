using NUnit.Framework;
using LogicSimulator;
using LogicSimulator.Gates;

namespace LogicSimulatorTest;

[TestFixture]
public class FlipFlopTest {
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
                new("S", new InputConnector()),
                new("R", new InputConnector()),
                new("not1", new NotLogic()),
                new("not2", new NotLogic()),
                new("nand1", new NAndLogic(2)),
                new("nand2", new NAndLogic(2)),
                new("Q", new OutputConnector()),
                new("~Q", new OutputConnector())
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

        var simulation = TestHelpers.Build(circuit);
        simulation.SetInput("S", 0, LogicSignal.Low);
        simulation.SetInput("R", 0, LogicSignal.Low);

        foreach (var frame in pattern.Frames) {
            foreach (var input in frame.Inputs) {
                simulation.SetInput(input.PinName, 0, input.Value.ToSignal());
            }
            simulation.Step();
            foreach (var expected in frame.Expecteds) {
                Assert.That(simulation.GetOutput(expected.PinName, "in"), Is.EqualTo(expected.Value.ToSignal()));
            }
        }
    }

    public static readonly IReadOnlyList<SignalTestPattern> JKFFMasterSlavePresetClear_Data = [
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
        var circuit = BuiltInCircuits.JK_FFMasterSlavePresetClear;

        var simulation = TestHelpers.Build(circuit);

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
                Assert.That(simulation.GetOutput(expected.PinName, "in"), Is.EqualTo(expected.Value.ToSignal()));
            }
        }
    }

    public static readonly IReadOnlyList<SignalTestPattern> DFF_Data = [
        // Clr=High で非同期クリア → Q=Low, ~Q~=High
        new([
            new([ new("Clr", true) ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // Set=High で非同期セット → Q=High, ~Q~=Low
        new([
            new([ new("Set", true) ], [ new("Q", true), new("~Q~", false) ])
        ]),
        // Set=High はクロックに無関係（C なしで即時セット）
        new([
            new([ new("Clr", true)  ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("Clr", false) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("Set", true)  ], [ new("Q", true),  new("~Q~", false) ])
        ]),
        // Set を解除しても Q 保持（クロックエッジなし）
        new([
            new([ new("Set", true)  ], [ new("Q", true), new("~Q~", false) ]),
            new([ new("Set", false) ], [ new("Q", true), new("~Q~", false) ])
        ]),
        // Clr=High はクロックに無関係（C なしで即時クリア）
        new([
            new([ new("Set", true)  ], [ new("Q", true),  new("~Q~", false) ]),
            new([ new("Set", false) ], [ new("Q", true),  new("~Q~", false) ]),
            new([ new("Clr", true)  ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // Set が Clr より優先される（Set=High, Clr=High → Q=High, ~Q~=Low）
        new([
            new([ new("Set", true), new("Clr", true) ], [ new("Q", true), new("~Q~", false) ])
        ]),
        // 通常動作: D=High、C 立ち上がりエッジ（Low→High）で Q=High
        new([
            new([ new("Clr", true)  ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("Clr", false) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("D", true)    ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("C", true)    ], [ new("Q", true),  new("~Q~", false) ])
        ]),
        // 通常動作: D=Low、C 立ち上がりエッジで Q=Low
        new([
            new([ new("Set", true)  ], [ new("Q", true),  new("~Q~", false) ]),
            new([ new("Set", false) ], [ new("Q", true),  new("~Q~", false) ]),
            new([ new("C", true)    ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // C 立ち下がりエッジ（High→Low）では Q は変化しない
        new([
            new([ new("Clr", true)  ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("Clr", false) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("D", true)    ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("C", true)    ], [ new("Q", true),  new("~Q~", false) ]),
            new([ new("C", false)   ], [ new("Q", true),  new("~Q~", false) ])
        ]),
        // C=High 保持中に D が変化しても Q 不変
        new([
            new([ new("Clr", true)  ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("Clr", false) ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("C", true)    ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("D", true)    ], [ new("Q", false), new("~Q~", true) ]),
            new([ new("D", false)   ], [ new("Q", false), new("~Q~", true) ])
        ]),
        // C=Low 保持中に D が変化しても Q 不変
        new([
            new([ new("Set", true)  ], [ new("Q", true), new("~Q~", false) ]),
            new([ new("Set", false) ], [ new("Q", true), new("~Q~", false) ]),
            new([ new("D", false)   ], [ new("Q", true), new("~Q~", false) ]),
            new([ new("D", true)    ], [ new("Q", true), new("~Q~", false) ])
        ]),
    ];

    [CancelAfter(1000)]
    [TestCaseSource(nameof(DFF_Data))]
    public void DFF_SignalTest(SignalTestPattern testPattern) {
        var simulation = TestHelpers.Build(BuiltInCircuits.D_FF, BuiltInCircuits.Circuits);
        simulation.SetInput("Clr", 0, LogicSignal.Low);
        simulation.SetInput("Set", 0, LogicSignal.Low);
        simulation.SetInput("C",   0, LogicSignal.Low);
        simulation.SetInput("D",   0, LogicSignal.Low);
        simulation.Step();

        foreach (var frame in testPattern.Frames) {
            foreach (var input in frame.Inputs) {
                simulation.SetInput(input.PinName, 0, input.Value.ToSignal());
            }
            simulation.Step();
            foreach (var expected in frame.Expecteds) {
                Assert.That(simulation.GetOutput(expected.PinName, "in"), Is.EqualTo(expected.Value.ToSignal()));
            }
        }
    }
}
