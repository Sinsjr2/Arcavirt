using NUnit.Framework;
using LogicSimulator;
using LogicSimulator.Gates;

namespace LogicSimulatorTest;

[TestFixture]
public class LogicSimulationTest {

    /// <summary>
    /// 回路の展開が入れ子になっていた場合展開できるかを確認します。
    /// </summary>
    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.Low)]
    public void CustomCircuitExpandTest(LogicSignal input1, LogicSignal input2, LogicSignal expected) {
        var andCircuit = new Circuit([
                new("a", new InputConnector()),
                new("b", new InputConnector()),
                new("and1", new AndLogic(2)),
                new("y", new OutputConnector())
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
                new("x1", new InputConnector()),
                new("x2", new InputConnector()),
                new("and100", new CustomCircuit("andCircuit")),
                new("result", new OutputConnector())
            ],
            [
                new(new("x1", "out"), new("and100", "a")),
                new(new("x2", "out"), new("and100", "b")),
                new(new("and100", "y"), new("result", "in")),
            ]);

        var simulation = TestHelpers.Build(testCircuit, library);
        simulation.SetInput("x1", 0, input1);
        simulation.SetInput("x2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("result", "in"), Is.EqualTo(expected));
    }

    /// <summary>
    /// 1つの出力ピンから複数の入力ピンに状態が正しくコピーされることを確認するテスト
    /// </summary>
    [Test]
    public void MultipleConnectionCopyTest() {
        // 1つのOR素子の出力を3つの異なる素子の入力に接続する複数接続テスト
        var circuit = new Circuit(new LogicNode[] {
                new("input", new InputConnector()),
                new("or1", new OrLogic(1)),
                new("and1", new AndLogic(2)),
                new("and2", new AndLogic(2)),
                new("and3", new AndLogic(2)),
                new("outputA", new OutputConnector()),
                new("outputB", new OutputConnector()),
                new("outputC", new OutputConnector())
            },
            new LogicConnection[] {
                // 入力 → OR[0]
                new(new LogicConnector("input", "out"), new LogicConnector("or1", "in[0]")),

                // OR出力 → 複数のAND素子の入力[0](複数接続)
                new(new LogicConnector("or1", "out"), new LogicConnector("and1", "in[0]")),
                new(new LogicConnector("or1", "out"), new LogicConnector("and2", "in[0]")),
                new(new LogicConnector("or1", "out"), new LogicConnector("and3", "in[0]")),

                // 入力をAND素子の入力[1]にも接続(全ANDが同じ入力を受け取る)
                new(new LogicConnector("input", "out"), new LogicConnector("and1", "in[1]")),
                new(new LogicConnector("input", "out"), new LogicConnector("and2", "in[1]")),
                new(new LogicConnector("input", "out"), new LogicConnector("and3", "in[1]")),

                // AND出力 → 出力コネクタ
                new(new LogicConnector("and1", "out"), new LogicConnector("outputA", "in")),
                new(new LogicConnector("and2", "out"), new LogicConnector("outputB", "in")),
                new(new LogicConnector("and3", "out"), new LogicConnector("outputC", "in"))
            });

        var simulation = TestHelpers.Build(circuit);

        // ステップ1: 入力=FALSE で初期化
        simulation.SetInput("input", 0, LogicSignal.Low);
        simulation.Step();

        Assert.That(simulation.GetOutput("outputA", "in"), Is.EqualTo(LogicSignal.Low));
        Assert.That(simulation.GetOutput("outputB", "in"), Is.EqualTo(LogicSignal.Low));
        Assert.That(simulation.GetOutput("outputC", "in"), Is.EqualTo(LogicSignal.Low));

        // ステップ2: 入力=TRUE に変更
        simulation.SetInput("input", 0, LogicSignal.High);
        simulation.Step();

        // すべての出力がTRUEであることを確認(複数接続がすべて正しくコピーされたことを検証)
        Assert.That(simulation.GetOutput("outputA", "in"), Is.EqualTo(LogicSignal.High));
        Assert.That(simulation.GetOutput("outputB", "in"), Is.EqualTo(LogicSignal.High));
        Assert.That(simulation.GetOutput("outputC", "in"), Is.EqualTo(LogicSignal.High));
    }

    [TestCase(LogicSignal.High, LogicSignal.High, LogicSignal.High)]
    [TestCase(LogicSignal.High, LogicSignal.Low, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.High, LogicSignal.Low)]
    [TestCase(LogicSignal.Low, LogicSignal.Low, LogicSignal.Low)]
    public void InputOutputConnector_AutoInfer_WorksWithAndGate(LogicSignal a, LogicSignal b, LogicSignal expected) {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("B", "out"), new LogicConnector("and", "in[1]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("A", 0, a);
        sim.SetInput("B", 0, b);
        sim.Step();
        Assert.That(sim.GetOutput("out", "in"), Is.EqualTo(expected));
    }

    [Test]
    public void InputOutputConnector_ExplicitDataBits_WorksWithAndGate() {
        var circuit = new Circuit([
                new("A", new InputConnector(1)),
                new("and", new AndLogic(1)),
                new("out", new OutputConnector(1))
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("A", 0, LogicSignal.High);
        sim.Step();
        Assert.That(sim.GetOutput("out", "in"), Is.EqualTo(LogicSignal.High));
    }

    [Test]
    public void InputConnector_NoConnection_ReturnsFalseWithUnresolvableError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("and", new AndLogic(1)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "A" && e.Kind == CircuitErrorKind.UnresolvableConnector));
    }

    [Test]
    public void InputConnector_ExplicitDataBits_MismatchWithConnectedPin_ReturnsFalseWithBitWidthMismatch() {
        var circuit = new Circuit([
                new("A", new InputConnector(2)),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "A" && e.Kind == CircuitErrorKind.BitWidthMismatch));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void InputConnector_InvalidDataBits_Throws(int dataBits) {
        Assert.Throws<ArgumentException>(() => new InputConnector(dataBits));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void OutputConnector_InvalidDataBits_Throws(int dataBits) {
        Assert.Throws<ArgumentException>(() => new OutputConnector(dataBits));
    }

    static Circuit SimpleAndCircuit() {
        return new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("B", "out"), new LogicConnector("and", "in[1]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
    }

    [Test]
    public void TryBuild_ValidCircuit_ReturnsTrue() {
        var result = LogicSimulation.TryBuild(SimpleAndCircuit(), out var sim, out var errors);
        Assert.That(result, Is.True);
        Assert.That(sim, Is.Not.Null);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void TryBuild_ValidCircuit_SimulationWorksCorrectly() {
        LogicSimulation.TryBuild(SimpleAndCircuit(), out var sim, out _);
        sim!.SetInput("A", 0, LogicSignal.High);
        sim.SetInput("B", 0, LogicSignal.High);
        sim.Step();
        Assert.That(sim.GetOutput("out", "in"), Is.EqualTo(LogicSignal.High));
    }

    [Test]
    public void TryBuild_UnconnectedOutputConnector_ReturnsFalseWithUnconnectedInputError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "out" && e.Kind == CircuitErrorKind.UnconnectedInput));
    }

    [Test]
    public void TryBuild_MultipleSourcesOnOutputConnector_ReturnsFalseWithMultipleSourceError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("out", "in")),
                new(new LogicConnector("B", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "out" && e.PinName == "in" && e.Kind == CircuitErrorKind.MultipleSourceConnections));
    }

    [Test]
    public void TryBuild_UnresolvableInputConnector_ReturnsFalseWithUnresolvableError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("out", new OutputConnector())
            ],
            []);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "A" && e.Kind == CircuitErrorKind.UnresolvableConnector));
    }

    [Test]
    public void TryBuild_MultipleSourcesOnGateInput_ReturnsFalseWithMultipleSourceError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("B", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[1]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "and" && e.PinName == "in[0]" && e.Kind == CircuitErrorKind.MultipleSourceConnections));
    }

    [Test]
    public void TryBuild_UnconnectedGateInput_ReportsUnconnectedInput() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "and" && e.PinName == "in[1]" && e.Kind == CircuitErrorKind.UnconnectedInput));
    }

    [Test]
    public void TryBuild_BothGateInputsUnconnected_ReportsBothPins() {
        var circuit = new Circuit([
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "and" && e.PinName == "in[0]" && e.Kind == CircuitErrorKind.UnconnectedInput));
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "and" && e.PinName == "in[1]" && e.Kind == CircuitErrorKind.UnconnectedInput));
    }

    [Test]
    public void TryBuild_InputConnectorWithNoOutgoing_NotReportedAsUnconnectedInput() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("out", new OutputConnector())
            ],
            []);
        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors, Has.None.Matches<CircuitError>(e =>
            e.NodeId == "A" && e.Kind == CircuitErrorKind.UnconnectedInput));
    }

    [Test]
    public void TryBuild_MultipleErrors_AllReported() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("out1", new OutputConnector()),
                new("out2", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("out1", "in")),
                new(new LogicConnector("B", "out"), new LogicConnector("out1", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out _, out var errors);
        Assert.That(result, Is.False);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "out1" && e.Kind == CircuitErrorKind.MultipleSourceConnections));
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "out2" && e.Kind == CircuitErrorKind.UnconnectedInput));
    }

    [Test]
    public void TryBuild_WithCircuitLibrary_ValidCircuit_ReturnsTrue() {
        var lib = new Dictionary<string, Circuit> {
            { "and_gate", SimpleAndCircuit() }
        };
        var circuit = new Circuit([
                new("X", new InputConnector()),
                new("Y", new InputConnector()),
                new("sub", new CustomCircuit("and_gate")),
                new("result", new OutputConnector())
            ],
            [
                new(new LogicConnector("X", "out"), new LogicConnector("sub", "A")),
                new(new LogicConnector("Y", "out"), new LogicConnector("sub", "B")),
                new(new LogicConnector("sub", "out"), new LogicConnector("result", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors, lib);
        Assert.That(result, Is.True);
        Assert.That(sim, Is.Not.Null);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void TryBuild_WithCircuitLibrary_InvalidCircuit_ReturnsFalseWithErrors() {
        var lib = new Dictionary<string, Circuit> {
            { "and_gate", SimpleAndCircuit() }
        };
        var circuit = new Circuit([
                new("X", new InputConnector()),
                new("sub", new CustomCircuit("and_gate")),
                new("result", new OutputConnector())
            ],
            [
                new(new LogicConnector("X", "out"), new LogicConnector("sub", "A"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors, lib);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Is.Not.Empty);
    }

    [Test]
    public void TryBuild_MultipleSourcesOnNotGateInput_ReturnsFalseWithMultipleSourceError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("not", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not", "in")),
                new(new LogicConnector("B", "out"), new LogicConnector("not", "in")),
                new(new LogicConnector("not", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "not" && e.Kind == CircuitErrorKind.MultipleSourceConnections));
    }

    [Test]
    public void TryBuild_UnconnectedOutputConnector_ReturnsErrorWithNodeId() {
        var circuit = new Circuit([
                new("myOut", new OutputConnector())
            ],
            []);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "myOut" && e.Kind == CircuitErrorKind.UnconnectedInput));
    }

    [Test]
    public void TryBuild_ScalarPinAccessedWithIndex_ReturnsInvalidPinAccess() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not", "in")),
                new(new LogicConnector("not", "out[0]"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(errors.Count(e => e.Kind == CircuitErrorKind.InvalidPinAccess), Is.EqualTo(1));
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "not" && e.PinName == "out[0]" && e.Kind == CircuitErrorKind.InvalidPinAccess));
    }

    [Test]
    public void TryBuild_BusPinAccessedWithOutOfRangeIndex_ReturnsInvalidPinAccess() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("B", "out"), new LogicConnector("and", "in[2]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "and" && e.PinName == "in[2]" && e.Kind == CircuitErrorKind.InvalidPinAccess));
    }

    [Test]
    public void TryBuild_OneBitBusPinAccessedWithIndex0_IsValid() {
        var circuit = new Circuit([
                new("A", new InputConnector(1)),
                new("not", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out[0]"), new LogicConnector("not", "in")),
                new(new LogicConnector("not", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.True);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void TryBuild_BusAndBitConnectionsMixed_ReturnsMultipleSourceConnections() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("and", new AndLogic(1)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in")),
                new(new LogicConnector("B", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "and" && e.PinName == "in" && e.Kind == CircuitErrorKind.MultipleSourceConnections));
    }

    [Test]
    public void TryBuild_DuplicateNodeId_ReturnsError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("A", new InputConnector()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "A" && e.Kind == CircuitErrorKind.DuplicateNodeId));
    }

    record UnknownLogic : ILogicElement;

    [Test]
    public void TryBuild_UnregisteredLogicElement_ReturnsError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("custom", new UnknownLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "custom" && e.Kind == CircuitErrorKind.UnregisteredLogicElement));
    }

    [Test]
    public void TryBuild_InvalidSourceLogicId_ReturnsInvalidNodeReferenceError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not1", new NotLogic()),
                new("not2", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("out", "in")),
                new(new LogicConnector("NONEXISTENT", "out"), new LogicConnector("not2", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "NONEXISTENT" && e.Kind == CircuitErrorKind.InvalidNodeReference));
    }

    [Test]
    public void TryBuild_InvalidSourcePinName_ReturnsInvalidNodeReferenceError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not1", new NotLogic()),
                new("not2", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("out", "in")),
                new(new LogicConnector("not1", "INVALID_PIN"), new LogicConnector("not2", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "not1" && e.PinName == "INVALID_PIN" && e.Kind == CircuitErrorKind.InvalidNodeReference));
    }

    [Test]
    public void TryBuild_UnknownCustomCircuitName_ReturnsInvalidNodeReferenceError() {
        var lib = new Dictionary<string, Circuit>();
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("sub", new CustomCircuit("no_such_circuit")),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("sub", "in")),
                new(new LogicConnector("sub", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors, lib);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "sub" && e.Kind == CircuitErrorKind.InvalidNodeReference));
    }

    [Test]
    public void TryBuild_WithLibrary_InvalidSourceLogicId_ReturnsErrorNotThrow() {
        var lib = new Dictionary<string, Circuit> { { "and_gate", SimpleAndCircuit() } };
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("sub", new CustomCircuit("and_gate")),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("sub", "A")),
                new(new LogicConnector("B", "out"), new LogicConnector("sub", "B")),
                new(new LogicConnector("sub", "out"), new LogicConnector("out", "in")),
                new(new LogicConnector("NONEXISTENT", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors, lib);
        Assert.That(result, Is.False);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "NONEXISTENT" && e.Kind == CircuitErrorKind.InvalidNodeReference));
    }

    [Test]
    public void ResolveConnectorBits_InputConnectorConnectedToConflictingWidths_ReturnsBitWidthMismatch() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("and", new AndLogic(2)),
                new("or2", new OrLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("A", "out"), new LogicConnector("or2", "in")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Is.False);
            Assert.That(sim, Is.Null);
            Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
                e.NodeId == "A" && e.Kind == CircuitErrorKind.BitWidthMismatch));
        }
    }

    [Test]
    public void OutputConnector_ExplicitDataBits_MismatchWithConnectedPin_ReturnsFalseWithBitWidthMismatch() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector(2))
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("B", "out"), new LogicConnector("and", "in[1]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Is.False);
            Assert.That(sim, Is.Null);
            Assert.That(errors, Has.Some.Matches<CircuitError>(static e =>
                e.NodeId == "out" && e.Kind == CircuitErrorKind.BitWidthMismatch));
        }
    }

    [Test]
    public void ResolveConnectorBits_ConnectorOnlyConnectedToUnresolvedConnectors_ReturnsUnresolvableConnector() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Is.False);
            Assert.That(sim, Is.Null);
            Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
                e.NodeId == "A" && e.Kind == CircuitErrorKind.UnresolvableConnector));
        }
    }

    [Test]
    public void TryBuild_InvalidTargetPinName_ReturnsInvalidNodeReferenceError() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not1", new NotLogic()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("and", "INVALID_PIN")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var result = LogicSimulation.TryBuild(circuit, out var sim, out var errors);
        Assert.That(result, Is.False);
        Assert.That(sim, Is.Null);
        Assert.That(errors, Has.Some.Matches<CircuitError>(e =>
            e.NodeId == "and" && e.Kind == CircuitErrorKind.InvalidNodeReference));
    }

    [Test]
    public void BusConnection_BusToBus_PropagatesAllBits() {
        var circuit = new Circuit([
                new("in0", new InputConnector()),
                new("in1", new InputConnector()),
                new("or", new OrLogic(2)),
                new("out0", new OutputConnector()),
                new("out1", new OutputConnector())
            ],
            [
                new(new LogicConnector("in0", "out"), new LogicConnector("or", "in[0]")),
                new(new LogicConnector("in1", "out"), new LogicConnector("or", "in[1]")),
                new(new LogicConnector("or", "out"), new LogicConnector("out0", "in")),
                new(new LogicConnector("or", "out"), new LogicConnector("out1", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("in0", 0, LogicSignal.High);
        sim.SetInput("in1", 0, LogicSignal.High);
        sim.Step();
        Assert.That(sim.GetOutput("out0", "in"), Is.EqualTo(LogicSignal.High));
        Assert.That(sim.GetOutput("out1", "in"), Is.EqualTo(LogicSignal.High));
    }

    [Test]
    public void BusConnection_BusToIndexedBit_RoutesCorrectBit() {
        var circuit = new Circuit([
                new("in0", new InputConnector()),
                new("in1", new InputConnector()),
                new("not0", new NotLogic()),
                new("not1", new NotLogic()),
                new("out0", new OutputConnector()),
                new("out1", new OutputConnector())
            ],
            [
                new(new LogicConnector("in0", "out"), new LogicConnector("not0", "in")),
                new(new LogicConnector("in1", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not0", "out"), new LogicConnector("out0", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("out1", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("in0", 0, LogicSignal.High);
        sim.SetInput("in1", 0, LogicSignal.Low);
        sim.Step();
        Assert.That(sim.GetOutput("out0", "in"), Is.EqualTo(LogicSignal.Low));
        Assert.That(sim.GetOutput("out1", "in"), Is.EqualTo(LogicSignal.High));
    }

    [Test]
    public void SetInput_OutOfRangeBitIndex_ThrowsException() {
        var circuit = new Circuit([
                new("A", new InputConnector(1)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        Assert.That(() => sim.SetInput("A", 1, LogicSignal.High), Throws.TypeOf<IndexOutOfRangeException>());
    }

    [Test]
    public void Step_MultipleSteps_StateTransitionsCorrectly() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not", "in")),
                new(new LogicConnector("not", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("A", 0, LogicSignal.Low);
        sim.Step();
        Assert.That(sim.GetOutput("out", "in"), Is.EqualTo(LogicSignal.High));
        sim.SetInput("A", 0, LogicSignal.High);
        sim.Step();
        Assert.That(sim.GetOutput("out", "in"), Is.EqualTo(LogicSignal.Low));
    }

    [Test]
    public void Step_OscillatingCircuit_WithCombinationalFeedback() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not1", new NotLogic()),
                new("not2", new NotLogic()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not1", "in")),
                new(new LogicConnector("not1", "out"), new LogicConnector("not2", "in")),
                new(new LogicConnector("not2", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[1]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("A", 0, LogicSignal.High);
        sim.Step();
        Assert.That(sim.GetOutput("out", "in"), Is.EqualTo(LogicSignal.High));
    }

    [Test]
    public void GetOutput_ArbitraryNodeAndPin_ReturnsCorrectSignal() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("B", new InputConnector()),
                new("and", new AndLogic(2)),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("and", "in[0]")),
                new(new LogicConnector("B", "out"), new LogicConnector("and", "in[1]")),
                new(new LogicConnector("and", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("A", 0, LogicSignal.High);
        sim.SetInput("B", 0, LogicSignal.High);
        sim.Step();
        Assert.That(sim.GetOutput("and", "out"), Is.EqualTo(LogicSignal.High));
    }

    [Test]
    public void GetOutput_NonexistentLogicId_ThrowsArgumentException() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not", "in")),
                new(new LogicConnector("not", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        Assert.That(() => sim.GetOutput("nonexistent", "in"), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void GetOutput_NonexistentPinName_ThrowsArgumentException() {
        var circuit = new Circuit([
                new("A", new InputConnector()),
                new("not", new NotLogic()),
                new("out", new OutputConnector())
            ],
            [
                new(new LogicConnector("A", "out"), new LogicConnector("not", "in")),
                new(new LogicConnector("not", "out"), new LogicConnector("out", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        Assert.That(() => sim.GetOutput("not", "nonexistent"), Throws.TypeOf<ArgumentException>());
    }
}
