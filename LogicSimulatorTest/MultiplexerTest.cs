using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

[TestFixture]
public class MultiplexerTest {

    public static IEnumerable<TestCaseData> MultiplexerTestCases => [
        new TestCaseData(1, 1).SetName("dataBit=1, selector=1 (2ch)"),
        new TestCaseData(1, 2).SetName("dataBit=1, selector=2 (4ch)"),
        new TestCaseData(4, 1).SetName("dataBit=4, selector=1 (2ch)"),
        new TestCaseData(4, 2).SetName("dataBit=4, selector=2 (4ch)"),
    ];

    [TestCaseSource(nameof(MultiplexerTestCases))]
    [CancelAfter(10000)]
    public void Multiplexer_DataDriven(int dataBit, int numOfSelectorBit) {
        int numOfChannel = 1 << numOfSelectorBit;

        for (int sel = 0; sel < numOfChannel; sel++) {
            for (int dataVal = 0; dataVal < (1 << dataBit); dataVal++) {
                var sim = TestHelpers.Build(
                    BuiltInCircuits.CreateMultiplexer(dataBit, numOfSelectorBit),
                    BuiltInCircuits.Circuits);

                for (int ch = 0; ch < numOfChannel; ch++) {
                    for (int b = 0; b < dataBit; b++) {
                        int val = (ch == sel)
                            ? (dataVal >> b) & 1
                            : ~(dataVal >> b) & 1;
                        sim.SetInput($"D{ch}_{b}", 0, (val != 0).ToSignal());
                    }
                }

                for (int i = 0; i < numOfSelectorBit; i++) {
                    sim.SetInput($"S{i}", 0, (((sel >> i) & 1) != 0).ToSignal());
                }

                sim.Step();

                using (Assert.EnterMultipleScope()) {
                    for (int b = 0; b < dataBit; b++) {
                        Assert.That(
                            sim.GetOutput($"Y{b}", 0),
                            Is.EqualTo((((dataVal >> b) & 1) != 0).ToSignal()),
                            $"sel={sel}, dataVal=0x{dataVal:X}: Y{b}");
                    }
                }
            }
        }
    }

    public static IEnumerable<TestCaseData> DemultiplexerTestCases => [
        new TestCaseData(1, 1).SetName("dataBit=1, selector=1 (2ch)"),
        new TestCaseData(1, 2).SetName("dataBit=1, selector=2 (4ch)"),
        new TestCaseData(4, 1).SetName("dataBit=4, selector=1 (2ch)"),
        new TestCaseData(4, 2).SetName("dataBit=4, selector=2 (4ch)"),
    ];

    [TestCaseSource(nameof(DemultiplexerTestCases))]
    [CancelAfter(10000)]
    public void Demultiplexer_DataDriven(int dataBit, int numOfSelectorBit) {
        int numOfChannel = 1 << numOfSelectorBit;

        for (int sel = 0; sel < numOfChannel; sel++) {
            for (int dataVal = 0; dataVal < (1 << dataBit); dataVal++) {
                var sim = TestHelpers.Build(
                    BuiltInCircuits.CreateDemultiplexer(dataBit, numOfSelectorBit),
                    BuiltInCircuits.Circuits);

                for (int b = 0; b < dataBit; b++) {
                    sim.SetInput($"D{b}", 0, (((dataVal >> b) & 1) != 0).ToSignal());
                }

                for (int i = 0; i < numOfSelectorBit; i++) {
                    sim.SetInput($"S{i}", 0, (((sel >> i) & 1) != 0).ToSignal());
                }

                sim.Step();

                using (Assert.EnterMultipleScope()) {
                    for (int ch = 0; ch < numOfChannel; ch++) {
                        for (int b = 0; b < dataBit; b++) {
                            LogicSignal expected = (ch == sel)
                                ? (((dataVal >> b) & 1) != 0).ToSignal()
                                : LogicSignal.Low;
                            Assert.That(
                                sim.GetOutput($"Y{ch}_{b}", 0),
                                Is.EqualTo(expected),
                                $"sel={sel}, dataVal=0x{dataVal:X}: Y{ch}_{b}");
                        }
                    }
                }
            }
        }
    }
}
