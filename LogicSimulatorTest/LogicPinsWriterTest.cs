using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

[TestFixture]
public class LogicPinsWriterTest {
    [Test]
    public void WriteBit_UpdatesPinValue() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.Low, LogicSignal.Low, LogicSignal.Low, LogicSignal.Low],
            NumOfPins = [(0, 2), (2, 2)],
            PinNumberToLogicNumber = [0, 0, 1, 1]
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        writer.WriteBit(0, 0, LogicSignal.High);
        writer.WriteBit(1, 1, LogicSignal.High);

        using (Assert.EnterMultipleScope()) {
            Assert.That(pins.Pins[0], Is.EqualTo(LogicSignal.High));
            Assert.That(pins.Pins[1], Is.EqualTo(LogicSignal.Low));
            Assert.That(pins.Pins[2], Is.EqualTo(LogicSignal.Low));
            Assert.That(pins.Pins[3], Is.EqualTo(LogicSignal.High));
        }
    }

    [Test]
    public void WriteBit_TracksChangedPins() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.Low, LogicSignal.Low ],
            NumOfPins = [(0, 2)],
            PinNumberToLogicNumber = [0, 0]
        };

        var changedPins = new List<int>();
        var writer = new LogicPinsWriter(pins, changedPins);

        writer.WriteBit(0, 0, LogicSignal.High);
        writer.WriteBit(0, 1, LogicSignal.Low);
        writer.WriteBit(0, 0, LogicSignal.Low);

        using (Assert.EnterMultipleScope()) {
            Assert.That(changedPins.Count, Is.EqualTo(2));
            Assert.That(changedPins[0], Is.EqualTo(0));
            Assert.That(changedPins[1], Is.EqualTo(0));
        }
    }

    [Test]
    public void GetPinsLength_ReturnsCorrectLength() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.Low, LogicSignal.Low, LogicSignal.Low ],
            NumOfPins = [(0, 2), (2, 1)],
            PinNumberToLogicNumber = [0, 0, 1]
        };

        var writer = new LogicPinsWriter(pins, new List<int>());

        using (Assert.EnterMultipleScope()) {
            Assert.That(writer.GetPinsLength(0), Is.EqualTo(2));
            Assert.That(writer.GetPinsLength(1), Is.EqualTo(1));
        }

    }

    [Test]
    public void ReadBit_ReturnsCurrentValue() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low ],
            NumOfPins = [(0, 2)],
            PinNumberToLogicNumber = [0, 0]
        };

        var writer = new LogicPinsWriter(pins, []);

        using (Assert.EnterMultipleScope()) {
            Assert.That(writer.ReadBit(0, 0), Is.EqualTo(LogicSignal.High));
            Assert.That(writer.ReadBit(0, 1), Is.EqualTo(LogicSignal.Low));
        }

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

        writer.WriteBit(0, 0, LogicSignal.High);
        writer.WriteBit(0, 1, LogicSignal.High);
        writer.WriteBit(1, 0, LogicSignal.High);
        writer.WriteBit(1, 1, LogicSignal.High);

        using (Assert.EnterMultipleScope()) {
            Assert.That(changedPins.Count, Is.EqualTo(4));
            Assert.That(pins.Pins.All(p => p == LogicSignal.High), Is.True);
        }

    }
}
