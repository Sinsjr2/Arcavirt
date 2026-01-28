using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

[TestFixture]
public class LogicSimulationTests {

    [TestCase(false, false, false)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, true, true)]
    public void CustomOrGateExtensibilityTest(bool input1, bool input2, bool expected) {
        var nodes = new LogicNode[] {
            new("input1", new InputConnector(1)),
            new("input2", new InputConnector(1)),
            new("or", new OrLogic(2)),
            new("output", new OutputConnector(1))
        };

        var connections = new LogicConnection[] {
            new(new LogicConnector("input1", "out"), new LogicConnector("or", "in[0]")),
            new(new LogicConnector("input2", "out"), new LogicConnector("or", "in[1]")),
            new(new LogicConnector("or", "out"), new LogicConnector("output", "in"))
        };

        // カスタムファクトリを指定してLogicSimulationをインスタンス化
        var factories = new Dictionary<Type, ILogicExecutorFactory> {
            { typeof(OrLogic), new LogicExecutorFactory<OrLogic>(new OrLogicExecutorFactory()) },
            { typeof(InputConnector), new LogicExecutorFactory<InputConnector>(new InputConnectorExecutorFactory()) },
            { typeof(OutputConnector), new LogicExecutorFactory<OutputConnector>(new OutputConnectorExecutorFactory()) },
        };

        var simulation = new LogicSimulation(nodes, connections, factories);

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();
        
        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(false, false, false)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, true, true)]
    public void AndGateSimulationTest(bool input1, bool input2, bool expected) {
        var nodes = new LogicNode[] {
            new("input1", new InputConnector(1)),
            new("input2", new InputConnector(1)),
            new("and1", new AndLogic(2)),
            new("output", new OutputConnector(1))
        };

        var connections = new LogicConnection[] {
            new(new LogicConnector("input1", "out"), new LogicConnector("and1", "in[0]")),
            new(new LogicConnector("input2", "out"), new LogicConnector("and1", "in[1]")),
            new(new LogicConnector("and1", "out"), new LogicConnector("output", "in"))
        };

        var simulation = new LogicSimulation(nodes, connections);

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();
        
        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public void NotGateSimulationTest(bool input, bool expected) {
        var nodes = new LogicNode[] {
            new("input", new InputConnector(1)),
            new("not1", new NotLogic()),
            new("output", new OutputConnector(1))
        };

        var connections = new LogicConnection[] {
            new(new LogicConnector("input", "out"), new LogicConnector("not1", "in")),
            new(new LogicConnector("not1", "out"), new LogicConnector("output", "in"))
        };

        var simulation = new LogicSimulation(nodes, connections);

        // 最初は反対の値を設定して、次に目的の値に設定
        simulation.SetInput("input", 0, !input);
        simulation.Step();
        
        simulation.SetInput("input", 0, input);
        simulation.Step();
        
        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(false, false, true)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, true, false)]
    public void NAndGateSimulationTest(bool input1, bool input2, bool expected) {
        var nodes = new LogicNode[] {
            new("input1", new InputConnector(1)),
            new("input2", new InputConnector(1)),
            new("nand1", new NAndLogic(2)),
            new("output", new OutputConnector(1))
        };

        // 接続の定義
        var connections = new LogicConnection[] {
            new(new LogicConnector("input1", "out"), new LogicConnector("nand1", "in[0]")),
            new(new LogicConnector("input2", "out"), new LogicConnector("nand1", "in[1]")),
            new(new LogicConnector("nand1", "out"), new LogicConnector("output", "in"))
        };

        var simulation = new LogicSimulation(nodes, connections);

        // 最初は反対の値を設定して、値が伝播するようにしてから、目的の値に変更
        simulation.SetInput("input1", 0, !input1);
        simulation.SetInput("input2", 0, !input2);
        simulation.Step();
        
        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(false, false, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, true, false)]
    public void NOrGateSimulationTest(bool input1, bool input2, bool expected) {
        var nodes = new LogicNode[] {
            new("input1", new InputConnector(1)),
            new("input2", new InputConnector(1)),
            new("nor", new NOrLogic(2)),
            new("output", new OutputConnector(1))
        };

        var connections = new LogicConnection[] {
            new(new LogicConnector("input1", "out"), new LogicConnector("nor", "in[0]")),
            new(new LogicConnector("input2", "out"), new LogicConnector("nor", "in[1]")),
            new(new LogicConnector("nor", "out"), new LogicConnector("output", "in"))
        };

        var simulation = new LogicSimulation(nodes, connections);

        // 最初は反対の値を設定して、値が伝播するようにしてから、目的の値に変更
        simulation.SetInput("input1", 0, !input1);
        simulation.SetInput("input2", 0, !input2);
        simulation.Step();

        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(false, false, false)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, true, false)]
    public void XOrGateSimulationTest(bool input1, bool input2, bool expected) {
        var nodes = new LogicNode[] {
            new("input1", new InputConnector(1)),
            new("input2", new InputConnector(1)),
            new("xor", new XOrLogic(2)),
            new("output", new OutputConnector(1))
        };

        var connections = new LogicConnection[] {
            new(new LogicConnector("input1", "out"), new LogicConnector("xor", "in[0]")),
            new(new LogicConnector("input2", "out"), new LogicConnector("xor", "in[1]")),
            new(new LogicConnector("xor", "out"), new LogicConnector("output", "in"))
        };

        var simulation = new LogicSimulation(nodes, connections);

        // 最初は反対の値を設定して、値が伝播するようにしてから、目的の値に変更
        simulation.SetInput("input1", 0, !input1);
        simulation.SetInput("input2", 0, !input2);
        simulation.Step();
        
        simulation.SetInput("input1", 0, input1);
        simulation.SetInput("input2", 0, input2);
        simulation.Step();

        Assert.That(simulation.GetOutput("output", 0), Is.EqualTo(expected));
    }

    [TestCase(true , false, false, true )] // 出力:保持
    [TestCase(false, false, false, false)] // 出力:保持
    [TestCase(true , false, true , false)] // 出力:0
    [TestCase(false, false, true , false)] // 出力:0
    [TestCase(true , true , false, true )] // 出力:1 (Set状態)
    [TestCase(false, true , false, true )] // 出力:1 (Set状態)
    // 回路が発振したときの無限ループ対策
    [CancelAfter(1000)]
    public void NandSrFFLatchSimulationTest(bool prevQ, bool set, bool reset, bool expectedQ) {
        var nodes = new LogicNode[] {
            new("set", new InputConnector(1)),
            new("reset", new InputConnector(1)),
            new("not1", new NotLogic()),
            new("not2", new NotLogic()),
            new("nand1", new NAndLogic(2)),
            new("nand2", new NAndLogic(2)),
            new("outputQ", new OutputConnector(1)),
            new("outputQnot", new OutputConnector(1))
        };
        var connections = new LogicConnection[] {
            new(new LogicConnector("set", "out"), new LogicConnector("not1", "in")),
            new(new LogicConnector("not1", "out"), new LogicConnector("nand1", "in[0]")),
            new(new LogicConnector("reset", "out"), new LogicConnector("not2", "in")),
            new(new LogicConnector("not2", "out"), new LogicConnector("nand2", "in[0]")),
            new(new LogicConnector("nand1", "out"), new LogicConnector("nand2", "in[1]")),
            new(new LogicConnector("nand1", "out"), new LogicConnector("outputQ", "in")),
            new(new LogicConnector("nand2", "out"), new LogicConnector("nand1", "in[1]")),
            new(new LogicConnector("nand2", "out"), new LogicConnector("outputQnot", "in"))
        };

        var simulation = new LogicSimulation(nodes, connections);

        // 前の出力を設定する
        if (prevQ) {
            simulation.SetInput("set", 0, true);
            simulation.SetInput("reset", 0, false);
        } else {
            simulation.SetInput("set", 0, false);
            simulation.SetInput("reset", 0, true);
        }
        // 初回実行で初期出力を生成する
        simulation.MarkAllInputPinChanged();
        simulation.Step();
        // 前の出力が期待通り反映されていることを確認する
        Assert.That(simulation.GetOutput("outputQ", 0), Is.EqualTo(prevQ));
        Assert.That(simulation.GetOutput("outputQnot", 0), Is.EqualTo(!prevQ));

        simulation.SetInput("set", 0, set);
        simulation.SetInput("reset", 0, reset);
        simulation.Step();
        Assert.That(simulation.GetOutput("outputQ", 0), Is.EqualTo(expectedQ));
        Assert.That(simulation.GetOutput("outputQnot", 0), Is.EqualTo(!expectedQ));
    }

    public void JKFFMasterSlavePresetClear(bool pre_, bool j, bool k, bool clk, bool clr) {
        var nodes = new LogicNode[] {
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
        };
        var connections = new LogicConnection[] {
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
            new(new LogicConnector("nand7", "out"), new LogicConnector("and2", "in[1]")),
            new(new LogicConnector("nand8", "out"), new LogicConnector("~Q~", "in")),
            new(new LogicConnector("nand8", "out"), new LogicConnector("nand7", "in[1]")),
            new(new LogicConnector("nand8", "out"), new LogicConnector("nand1", "in[0]")),
        };

        var simulation = new LogicSimulation(nodes, connections);
    }

    /// <summary>
    /// 1つの出力ピンから複数の入力ピンに状態が正しくコピーされることを確認するテスト
    /// </summary>
    [Test]
    public void MultipleConnectionCopyTest() {
        // 1つのOR素子の出力を3つの異なる素子の入力に接続する複数接続テスト
        var nodes = new LogicNode[] {
            new("input", new InputConnector(1)),
            new("or1", new OrLogic(1)),
            new("and1", new AndLogic(2)),
            new("and2", new AndLogic(2)),
            new("and3", new AndLogic(2)),
            new("outputA", new OutputConnector(1)),
            new("outputB", new OutputConnector(1)),
            new("outputC", new OutputConnector(1))
        };

        var connections = new LogicConnection[] {
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
        };

        var simulation = new LogicSimulation(nodes, connections);

        // ステップ1: 入力=FALSE で初期化
        simulation.SetInput("input", 0, false);
        simulation.Step();
        
        bool outA1 = simulation.GetOutput("outputA", 0);
        bool outB1 = simulation.GetOutput("outputB", 0);
        bool outC1 = simulation.GetOutput("outputC", 0);

        Assert.That(outA1, Is.EqualTo(false));
        Assert.That(outB1, Is.EqualTo(false));
        Assert.That(outC1, Is.EqualTo(false));

        // ステップ2: 入力=TRUE に変更
        simulation.SetInput("input", 0, true);
        simulation.Step();

        // すべての出力がTRUEであることを確認（複数接続がすべて正しくコピーされたことを検証）
        Assert.That(simulation.GetOutput("outputA", 0), Is.EqualTo(true));
        Assert.That(simulation.GetOutput("outputB", 0), Is.EqualTo(true));
        Assert.That(simulation.GetOutput("outputC", 0), Is.EqualTo(true));
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
            Pins = new[] { true, false, true, false },
            NumOfPins = new[] { (0, 2), (2, 2) },
            PinNumberToLogicNumber = new[] { 0, 0, 1, 1 }
        };

        var changedPins = new List<int> { 0, 2 };
        var isExecutedLogicNumbers = new[] { false, false };

        var reader = new LogicPinReader(pins, changedPins, isExecutedLogicNumbers, 0);

        Assert.That(reader.ReadBit(0, 0), Is.EqualTo(true));
        Assert.That(reader.ReadBit(0, 1), Is.EqualTo(false));
        Assert.That(reader.ReadBit(1, 0), Is.EqualTo(true));
        Assert.That(reader.ReadBit(1, 1), Is.EqualTo(false));
    }

    [Test]
    public void GetPinsLength_ReturnsCorrectLength() {
        // Arrange
        var pins = new LogicPins {
            Pins = new[] { true, false, true },
            NumOfPins = new[] { (0, 2), (2, 1) },
            PinNumberToLogicNumber = new[] { 0, 0, 1 }
        };

        var reader = new LogicPinReader(pins, new List<int>(), new[] { false, false }, 0);

        Assert.That(reader.GetPinsLength(0), Is.EqualTo(2));
        Assert.That(reader.GetPinsLength(1), Is.EqualTo(1));
    }

    [Test]
    public void TryGetNextChangedLogicNumber_ReturnsChangedLogicNumbers() {
        // Logic 0のPin 0, Logic 1のPin 0が変化した
        var pins = new LogicPins {
            Pins = new[] { true, false, true, false },
            NumOfPins = new[] { (0, 2), (2, 2) },
            PinNumberToLogicNumber = new[] { 0, 0, 1, 1 }
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
            Pins = new[] { true, false, true },
            NumOfPins = new[] { (0, 2), (2, 1) },
            PinNumberToLogicNumber = new[] { 0, 0, 1 }
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
            Pins = new[] { false, false, false, false },
            NumOfPins = new[] { (0, 2), (2, 2) },
            PinNumberToLogicNumber = new[] { 0, 0, 1, 1 }
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        writer.WriteBit(0, 0, true);
        writer.WriteBit(1, 1, true);

        Assert.That(pins.Pins[0], Is.True);
        Assert.That(pins.Pins[1], Is.False);
        Assert.That(pins.Pins[2], Is.False);
        Assert.That(pins.Pins[3], Is.True);
    }

    [Test]
    public void WriteBit_TracksChangedPins() {
        // Arrange
        var pins = new LogicPins {
            Pins = new[] { false, false },
            NumOfPins = new[] { (0, 2) },
            PinNumberToLogicNumber = new[] { 0, 0 }
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        writer.WriteBit(0, 0, true);  // 変更あり
        writer.WriteBit(0, 1, false); // 変更なし（既にfalse）
        writer.WriteBit(0, 0, false); // 変更あり

        Assert.That(changedPins.Count, Is.EqualTo(2));
        Assert.That(changedPins[0], Is.EqualTo(0));
        Assert.That(changedPins[1], Is.EqualTo(0));
    }

    [Test]
    public void GetPinsLength_ReturnsCorrectLength() {
        var pins = new LogicPins {
            Pins = new[] { false, false, false },
            NumOfPins = new[] { (0, 2), (2, 1) },
            PinNumberToLogicNumber = new[] { 0, 0, 1 }
        };

        var writer = new LogicPinsWriter(pins, new List<int>());

        Assert.That(writer.GetPinsLength(0), Is.EqualTo(2));
        Assert.That(writer.GetPinsLength(1), Is.EqualTo(1));
    }

    [Test]
    public void ReadBit_ReturnsCurrentValue() {
        var pins = new LogicPins {
            Pins = new[] { true, false },
            NumOfPins = new[] { (0, 2) },
            PinNumberToLogicNumber = new[] { 0, 0 }
        };

        var writer = new LogicPinsWriter(pins, new List<int>());

        Assert.That(writer.ReadBit(0, 0), Is.True);
        Assert.That(writer.ReadBit(0, 1), Is.False);
    }

    [Test]
    public void WriteBit_MultipleWrites_TrackEachChange() {
        var pins = new LogicPins {
            Pins = new[] { false, false, false, false },
            NumOfPins = new[] { (0, 2), (2, 2) },
            PinNumberToLogicNumber = new[] { 0, 0, 1, 1 }
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        // 複数の異なるピンに書き込み
        writer.WriteBit(0, 0, true);
        writer.WriteBit(0, 1, true);
        writer.WriteBit(1, 0, true);
        writer.WriteBit(1, 1, true);

        Assert.That(changedPins.Count, Is.EqualTo(4));
        Assert.That(pins.Pins.All(p => p), Is.True);
    }
}