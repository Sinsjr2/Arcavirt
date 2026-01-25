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
    [TestCase(true , true , false, false)] // 出力:1
    [TestCase(false, true , false, false)] // 出力:1
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
}