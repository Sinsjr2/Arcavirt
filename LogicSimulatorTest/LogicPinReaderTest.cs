using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

[TestFixture]
public class LogicPinReaderTest {
    [Test]
    public void ReadBit_ReturnsCorrectPinValue() {
        var pins = new LogicPins {
            Pins = [LogicSignal.High, LogicSignal.Low, LogicSignal.High, LogicSignal.Low],
            NumOfPins = [(0, 2), (2, 2)],
            PinNumberToLogicNumber = [0, 0, 1, 1]
        };

        var changedPins = new List<int> { 0, 2 };
        var isExecutedLogicNumbers = new[] { false, false };

        var reader = new LogicPinReader(pins, changedPins, isExecutedLogicNumbers, 0);

        using (Assert.EnterMultipleScope()) {
            Assert.That(reader.ReadBit(0, 0), Is.EqualTo(LogicSignal.High));
            Assert.That(reader.ReadBit(0, 1), Is.EqualTo(LogicSignal.Low));
            Assert.That(reader.ReadBit(1, 0), Is.EqualTo(LogicSignal.High));
            Assert.That(reader.ReadBit(1, 1), Is.EqualTo(LogicSignal.Low));
        }

    }

    [Test]
    public void GetPinsLength_ReturnsCorrectLength() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low, LogicSignal.High ],
            NumOfPins = [(0, 2), (2, 1)],
            PinNumberToLogicNumber = [0, 0, 1]
        };

        var reader = new LogicPinReader(pins, [], [false, false], 0);

        using (Assert.EnterMultipleScope()) {
            Assert.That(reader.GetPinsLength(0), Is.EqualTo(2));
            Assert.That(reader.GetPinsLength(1), Is.EqualTo(1));
        }

    }

    [Test]
    public void TryGetNextChangedLogicNumber_ReturnsChangedLogicNumbers() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low, LogicSignal.High, LogicSignal.Low ],
            NumOfPins = [(0, 2), (2, 2)],
            PinNumberToLogicNumber = [0, 0, 1, 1]
        };

        var changedPins = new List<int> { 0, 2 };
        var isExecutedLogicNumbers = new[] { false, false };

        var reader = new LogicPinReader(pins, changedPins, isExecutedLogicNumbers, 0);

        using (Assert.EnterMultipleScope()) {
            Assert.That(reader.TryGetNextChangedLogicNumber(out int firstLogicNo), Is.True);
            Assert.That(firstLogicNo, Is.EqualTo(0));

            Assert.That(reader.TryGetNextChangedLogicNumber(out int secondLogicNo), Is.True);
            Assert.That(secondLogicNo, Is.EqualTo(1));
            Assert.That(reader.TryGetNextChangedLogicNumber(out int thirdLogicNo), Is.False);
        }

    }

    [Test]
    public void TryGetNextChangedLogicNumber_SkipsDuplicates() {
        var pins = new LogicPins {
            Pins = [ LogicSignal.High, LogicSignal.Low, LogicSignal.High],
            NumOfPins = [(0, 2), (2, 1)],
            PinNumberToLogicNumber = [0, 0, 1]
        };

        var changedPins = new List<int> { 0, 1 };
        var isExecutedLogicNumbers = new[] { false, false };

        var reader = new LogicPinReader(pins, changedPins, isExecutedLogicNumbers, 0);

        using (Assert.EnterMultipleScope()) {
            Assert.That(reader.TryGetNextChangedLogicNumber(out int firstLogicNo), Is.True);
            Assert.That(firstLogicNo, Is.EqualTo(0));
            Assert.That(reader.TryGetNextChangedLogicNumber(out int secondLogicNo), Is.False);
        }

    }
}
