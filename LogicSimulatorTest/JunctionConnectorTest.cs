using NUnit.Framework;
using LogicSimulator;
using LogicSimulator.Gates;

namespace LogicSimulatorTest;

[TestFixture]
public class JunctionConnectorTest {
    [Test]
    public void BypassTest_SingleBit() {
        var circuit = new Circuit(
            [
                new("input", new InputConnector()),
                new("junc", new JunctionConnector([8], [8])),
                new("output", new OutputConnector())
            ],
            [
                new(new("input", "out"), new("junc", "in[0]")),
                new(new("junc", "out[0]"), new("output", "in"))
            ]
        );

        var sim = TestHelpers.Build(circuit);
        sim.SetInput("input", 0, LogicSignal.Low);
        for (int i = 1; i < 8; i++) {
            sim.SetInput("input", i, i % 2 == 0 ? LogicSignal.Low : LogicSignal.High);
        }
        sim.Step();

        Assert.That(sim.GetOutput("output", "in", 0), Is.EqualTo(LogicSignal.Low));
        for (int i = 1; i < 8; i++) {
            var expected = i % 2 == 0 ? LogicSignal.Low : LogicSignal.High;
            Assert.That(sim.GetOutput("output", "in", i), Is.EqualTo(expected));
        }
    }

    [Test]
    public void MergeTest_8Plus8To16() {
        var circuit = new Circuit(
            [
                new("in0", new InputConnector()),
                new("in1", new InputConnector()),
                new("junc", new JunctionConnector([8, 8], [16])),
                new("out", new OutputConnector())
            ],
            [
                new(new("in0", "out"), new("junc", "in[0]")),
                new(new("in1", "out"), new("junc", "in[1]")),
                new(new("junc", "out[0]"), new("out", "in"))
            ]
        );

        var sim = TestHelpers.Build(circuit);

        for (int i = 0; i < 8; i++) {
            sim.SetInput("in0", i, LogicSignal.Low);
            sim.SetInput("in1", i, LogicSignal.Low);
        }
        sim.Step();
        for (int i = 0; i < 16; i++) {
            Assert.That(sim.GetOutput("out", "in", i), Is.EqualTo(LogicSignal.Low),
                $"Test case 0: bit {i} should be Low");
        }

        for (int i = 0; i < 8; i++) {
            sim.SetInput("in0", i, LogicSignal.High);
            sim.SetInput("in1", i, LogicSignal.Low);
        }
        sim.Step();
        for (int i = 0; i < 8; i++) {
            Assert.That(sim.GetOutput("out", "in", i), Is.EqualTo(LogicSignal.Low),
                $"Test case 1 (0xFF in[0], 0x00 in[1]): bit {i} should be Low");
        }
        for (int i = 8; i < 16; i++) {
            Assert.That(sim.GetOutput("out", "in", i), Is.EqualTo(LogicSignal.High),
                $"Test case 1 (0xFF in[0], 0x00 in[1]): bit {i} should be High");
        }

        for (int i = 0; i < 8; i++) {
            sim.SetInput("in0", i, LogicSignal.Low);
            sim.SetInput("in1", i, LogicSignal.High);
        }
        sim.Step();
        for (int i = 0; i < 8; i++) {
            Assert.That(sim.GetOutput("out", "in", i), Is.EqualTo(LogicSignal.High),
                $"Test case 2 (0x00 in[0], 0xFF in[1]): bit {i} should be High");
        }
        for (int i = 8; i < 16; i++) {
            Assert.That(sim.GetOutput("out", "in", i), Is.EqualTo(LogicSignal.Low),
                $"Test case 2 (0x00 in[0], 0xFF in[1]): bit {i} should be Low");
        }

        for (int i = 0; i < 8; i++) {
            var in0Val = (i & 1) == 0 ? LogicSignal.Low : LogicSignal.High;
            var in1Val = (i & 2) == 0 ? LogicSignal.Low : LogicSignal.High;
            sim.SetInput("in0", i, in0Val);
            sim.SetInput("in1", i, in1Val);
        }
        sim.Step();
        for (int i = 0; i < 8; i++) {
            var expectedIn1 = (i & 2) == 0 ? LogicSignal.Low : LogicSignal.High;
            Assert.That(sim.GetOutput("out", "in", i), Is.EqualTo(expectedIn1),
                $"Test case 3 (0xAB in[0], 0xCD in[1]): bit {i} should be {expectedIn1}");
        }
        for (int i = 8; i < 16; i++) {
            var expectedIn0 = ((i - 8) & 1) == 0 ? LogicSignal.Low : LogicSignal.High;
            Assert.That(sim.GetOutput("out", "in", i), Is.EqualTo(expectedIn0),
                $"Test case 3 (0xAB in[0], 0xCD in[1]): bit {i} should be {expectedIn0}");
        }
    }

    [Test]
    public void SplitTest_16To8Plus8() {
        var circuit = new Circuit(
            [
                new("input", new InputConnector()),
                new("junc", new JunctionConnector([16], [8, 8])),
                new("out0", new OutputConnector()),
                new("out1", new OutputConnector())
            ],
            [
                new(new("input", "out"), new("junc", "in[0]")),
                new(new("junc", "out[0]"), new("out0", "in")),
                new(new("junc", "out[1]"), new("out1", "in"))
            ]
        );

        var sim = TestHelpers.Build(circuit);

        for (int i = 0; i < 8; i++) {
            var val = (i & 2) == 0 ? LogicSignal.Low : LogicSignal.High;
            sim.SetInput("input", i, val);
        }
        for (int i = 8; i < 16; i++) {
            var val = ((i - 8) & 1) == 0 ? LogicSignal.Low : LogicSignal.High;
            sim.SetInput("input", i, val);
        }
        sim.Step();

        for (int i = 0; i < 8; i++) {
            var expected = ((i + 8) & 1) == 0 ? LogicSignal.Low : LogicSignal.High;
            Assert.That(sim.GetOutput("out0", "in", i), Is.EqualTo(expected),
                $"out[0] bit {i} should be {expected}");
        }
        for (int i = 0; i < 8; i++) {
            var expected = (i & 2) == 0 ? LogicSignal.Low : LogicSignal.High;
            Assert.That(sim.GetOutput("out1", "in", i), Is.EqualTo(expected),
                $"out[1] bit {i} should be {expected}");
        }
    }

    [Test]
    public void ErrorTest_JunctionBitSumMismatch() {
        var circuit = new Circuit(
            [
                new("input", new InputConnector()),
                new("junc", new JunctionConnector([8], [7])),
                new("output", new OutputConnector())
            ],
            [
                new(new("input", "out"), new("junc", "in[0]")),
                new(new("junc", "out[0]"), new("output", "in"))
            ]
        );

        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors.Any(e => e.Kind == CircuitErrorKind.JunctionBitSumMismatch), Is.True);
    }

    [Test]
    public void ErrorTest_UnconnectedInput_AllPins() {
        var circuit = new Circuit(
            [
                new("junc", new JunctionConnector([8], [8])),
                new("output", new OutputConnector())
            ],
            [
                new(new("junc", "out[0]"), new("output", "in"))
            ]
        );

        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors.Any(e => e.Kind == CircuitErrorKind.UnconnectedInput), Is.True);
    }

    [Test]
    public void ErrorTest_UnconnectedInput_PartialPins() {
        var circuit = new Circuit(
            [
                new("input", new InputConnector()),
                new("junc", new JunctionConnector([8], [8])),
                new("output", new OutputConnector())
            ],
            [
                new(new("input", "out"), new("junc", "in[0][0]")),
                new(new("junc", "out[0]"), new("output", "in"))
            ]
        );

        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors.Any(e => e.Kind == CircuitErrorKind.UnconnectedInput), Is.True);
    }

    [Test]
    public void ErrorTest_UnconnectedInput_ScalarPin() {
        var circuit = new Circuit(
            [
                new("junc", new JunctionConnector([1], [1])),
                new("output", new OutputConnector())
            ],
            [
                new(new("junc", "out[0]"), new("output", "in"))
            ]
        );

        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors.Any(e => e.Kind == CircuitErrorKind.UnconnectedInput), Is.True);
    }

    [Test]
    public void ErrorTest_BitWidthMismatch() {
        var circuit = new Circuit(
            [
                new("input", new InputConnector(8)),
                new("junc", new JunctionConnector([16], [16])),
                new("output", new OutputConnector())
            ],
            [
                new(new("input", "out"), new("junc", "in[0]")),
                new(new("junc", "out[0]"), new("output", "in"))
            ]
        );

        Assert.That(LogicSimulation.TryBuild(circuit, out _, out var errors), Is.False);
        Assert.That(errors.Any(e => e.Kind == CircuitErrorKind.BitWidthMismatch), Is.True);
    }

    [Test]
    public void ChainedSwapTest_100Patterns() {
        for (int patternIdx = 0; patternIdx < 100; patternIdx++) {
            int width = (patternIdx % 10) + 1;
            int swapType = patternIdx / 10;

            var nodes = new List<LogicNode> {
                new("in0", new InputConnector()),
                new("in1", new InputConnector()),
                new("junc0", new JunctionConnector([width, width], [width, width])),
                new("junc1", new JunctionConnector([width, width], [width, width])),
                new("out0", new OutputConnector()),
                new("out1", new OutputConnector())
            };

            var connections = new List<LogicConnection> {
                new(new("in0", "out"), new("junc0", "in[0]")),
                new(new("in1", "out"), new("junc0", "in[1]"))
            };

            if ((swapType & 1) == 0) {
                connections.Add(new(new("junc0", "out[0]"), new("junc1", "in[0]")));
                connections.Add(new(new("junc0", "out[1]"), new("junc1", "in[1]")));
            } else {
                connections.Add(new(new("junc0", "out[0]"), new("junc1", "in[1]")));
                connections.Add(new(new("junc0", "out[1]"), new("junc1", "in[0]")));
            }

            if ((swapType & 2) == 0) {
                connections.Add(new(new("junc1", "out[0]"), new("out0", "in")));
                connections.Add(new(new("junc1", "out[1]"), new("out1", "in")));
            } else {
                connections.Add(new(new("junc1", "out[0]"), new("out1", "in")));
                connections.Add(new(new("junc1", "out[1]"), new("out0", "in")));
            }

            var circuit = new Circuit(nodes, connections);
            var sim = TestHelpers.Build(circuit);

            for (int i = 0; i < width; i++) {
                sim.SetInput("in0", i, (i % 2 == 0) ? LogicSignal.Low : LogicSignal.High);
                sim.SetInput("in1", i, (i % 3 == 0) ? LogicSignal.Low : LogicSignal.High);
            }
            sim.Step();

            var swapAtJunc0 = (swapType & 1) == 1;
            var swapAtJunc1 = (swapType & 2) == 2;

            for (int i = 0; i < width; i++) {
                var in0Val = (i % 2 == 0) ? LogicSignal.Low : LogicSignal.High;
                var in1Val = (i % 3 == 0) ? LogicSignal.Low : LogicSignal.High;

                var afterJunc0Out0 = swapAtJunc0 ? in1Val : in0Val;
                var afterJunc0Out1 = swapAtJunc0 ? in0Val : in1Val;

                var afterJunc1Out0 = swapAtJunc1 ? afterJunc0Out1 : afterJunc0Out0;
                var afterJunc1Out1 = swapAtJunc1 ? afterJunc0Out0 : afterJunc0Out1;

                Assert.That(sim.GetOutput("out0", "in", i), Is.EqualTo(afterJunc1Out0),
                    $"Pattern {patternIdx}: out0[{i}] mismatch");
                Assert.That(sim.GetOutput("out1", "in", i), Is.EqualTo(afterJunc1Out1),
                    $"Pattern {patternIdx}: out1[{i}] mismatch");
            }
        }
    }
}
