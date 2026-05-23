using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

[TestFixture]
public class ConstValueLogicTest {

    [TestCase(true, LogicSignal.High)]
    [TestCase(false, LogicSignal.Low)]
    public void FromBool_OutputsCorrectSignal(bool v, LogicSignal expected) {
        var circuit = new Circuit([
                new("cv", ConstValueLogic.FromBool(v)),
                new("out0", new OutputConnector())
            ],
            [
                new(new LogicConnector("cv", "out[0]"), new LogicConnector("out0", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.Step();
        Assert.That(sim.GetOutput("out0", 0), Is.EqualTo(expected));
    }

    [Test]
    public void FromU8_4bit_OutputsCorrectBits() {
        var circuit = new Circuit([
                new("cv", ConstValueLogic.FromU8(4, 0b1010)),
                new("out0", new OutputConnector()),
                new("out1", new OutputConnector()),
                new("out2", new OutputConnector()),
                new("out3", new OutputConnector())
            ],
            [
                new(new LogicConnector("cv", "out[0]"), new LogicConnector("out0", "in")),
                new(new LogicConnector("cv", "out[1]"), new LogicConnector("out1", "in")),
                new(new LogicConnector("cv", "out[2]"), new LogicConnector("out2", "in")),
                new(new LogicConnector("cv", "out[3]"), new LogicConnector("out3", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.Step();
        using (Assert.EnterMultipleScope()) {
            Assert.That(sim.GetOutput("out0", 0), Is.EqualTo(LogicSignal.Low));
            Assert.That(sim.GetOutput("out1", 0), Is.EqualTo(LogicSignal.High));
            Assert.That(sim.GetOutput("out2", 0), Is.EqualTo(LogicSignal.Low));
            Assert.That(sim.GetOutput("out3", 0), Is.EqualTo(LogicSignal.High));
        }

    }

    [Test]
    public void FromI8_Negative1_4bit_OutputsAllHigh() {
        var circuit = new Circuit([
                new("cv", ConstValueLogic.FromI8(4, -1)),
                new("out0", new OutputConnector()),
                new("out1", new OutputConnector()),
                new("out2", new OutputConnector()),
                new("out3", new OutputConnector())
            ],
            [
                new(new LogicConnector("cv", "out[0]"), new LogicConnector("out0", "in")),
                new(new LogicConnector("cv", "out[1]"), new LogicConnector("out1", "in")),
                new(new LogicConnector("cv", "out[2]"), new LogicConnector("out2", "in")),
                new(new LogicConnector("cv", "out[3]"), new LogicConnector("out3", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.Step();
        using (Assert.EnterMultipleScope()) {
            Assert.That(sim.GetOutput("out0", 0), Is.EqualTo(LogicSignal.High));
            Assert.That(sim.GetOutput("out1", 0), Is.EqualTo(LogicSignal.High));
            Assert.That(sim.GetOutput("out2", 0), Is.EqualTo(LogicSignal.High));
            Assert.That(sim.GetOutput("out3", 0), Is.EqualTo(LogicSignal.High));
        }
    }

    [Test]
    public void ConstValueLogic_PropagatesTo_AndLogic() {
        var circuit = new Circuit([
                new("cv", ConstValueLogic.FromBool(true)),
                new("input", new InputConnector()),
                new("and1", new AndLogic(2)),
                new("result", new OutputConnector())
            ],
            [
                new(new LogicConnector("cv", "out[0]"), new LogicConnector("and1", "in[0]")),
                new(new LogicConnector("input", "out"), new LogicConnector("and1", "in[1]")),
                new(new LogicConnector("and1", "out"), new LogicConnector("result", "in"))
            ]);
        var sim = TestHelpers.Build(circuit);
        sim.SetInput("input", 0, LogicSignal.High);
        sim.Step();
        Assert.That(sim.GetOutput("result", 0), Is.EqualTo(LogicSignal.High));

        sim.SetInput("input", 0, LogicSignal.Low);
        sim.Step();
        Assert.That(sim.GetOutput("result", 0), Is.EqualTo(LogicSignal.Low));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(65)]
    public void FromU64_InvalidBitLength_ThrowsArgumentException(int bitLength) {
        Assert.Throws<ArgumentException>(() => ConstValueLogic.FromU64(bitLength, 0));
    }

    [Test]
    public void FromU8_ValueTooLarge_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => ConstValueLogic.FromU8(3, 255));
    }

    [Test]
    public void FromI8_ValueOutOfRange_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => ConstValueLogic.FromI8(3, -5));
    }

    [Test]
    public void FromU64_BitLength64_MaxValue_Succeeds() {
        var cv = ConstValueLogic.FromU64(64, ulong.MaxValue);
        Assert.That(cv.Value, Is.EqualTo(ulong.MaxValue));
    }

    [Test]
    public void FromI64_BitLength64_MinValue_Succeeds() {
        var cv = ConstValueLogic.FromI64(64, long.MinValue);
        Assert.That(cv.Value, Is.EqualTo(unchecked((ulong)long.MinValue)));
    }
}
