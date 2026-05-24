using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

public static class TestHelpers {
    public static LogicSimulation Build(Circuit c, IReadOnlyDictionary<string, Circuit>? lib = null) {
        Assert.That(LogicSimulation.TryBuild(c, out var s, out var e, lib), Is.True,
            () => string.Join(", ", e.Select(x => x.Message)));
        return s!;
    }

    public static void TestLogicGate(ILogicElement gate, LogicSignal[] inputs, LogicSignal expected) {
        int n = inputs.Length;
        var nodes = Enumerable.Range(0, n)
            .Select(i => new LogicNode($"input{i}", new InputConnector()))
            .Append(new LogicNode("gate", gate))
            .Append(new LogicNode("output", new OutputConnector()))
            .ToArray();

        var connections = Enumerable.Range(0, n)
            .Select(i => new LogicConnection(new($"input{i}", "out"), new("gate", $"in[{i}]")))
            .Append(new LogicConnection(new("gate", "out"), new("output", "in")))
            .ToArray();

        var sim = Build(new Circuit(nodes, connections));
        for (int i = 0; i < n; i++) {
            sim.SetInput($"input{i}", 0, inputs[i]);
        }
        sim.Step();
        Assert.That(sim.GetOutput("output", "in"), Is.EqualTo(expected));
    }
}
