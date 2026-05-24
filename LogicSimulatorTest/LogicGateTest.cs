using NUnit.Framework;
using LogicSimulator;
using LogicSimulator.Gates;

namespace LogicSimulatorTest;

[TestFixture]
public class LogicGateTest {
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
        TestHelpers.TestLogicGate(new AndLogic(inputs.Length), inputs, expected);
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
        TestHelpers.TestLogicGate(new OrLogic(inputs.Length), inputs, expected);
    }

    [TestCase(LogicSignal.Low, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.X, LogicSignal.X)]
    public void NotLogicTest(LogicSignal input, LogicSignal expected) {
        var circuit = new Circuit([
                new("input", new InputConnector()),
                new("not1", new NotLogic()),
                new("output", new OutputConnector())
            ],
            [
                new(new LogicConnector("input", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("output", "in"))
            ]);

        var simulation = TestHelpers.Build(circuit);

        simulation.SetInput("input", 0, input);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", "in"), Is.EqualTo(expected));
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
        TestHelpers.TestLogicGate(new NAndLogic(inputs.Length), inputs, expected);
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
        TestHelpers.TestLogicGate(new NOrLogic(inputs.Length), inputs, expected);
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
        TestHelpers.TestLogicGate(new XOrLogic(inputs.Length), inputs, expected);
    }
}
