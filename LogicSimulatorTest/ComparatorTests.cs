using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

[TestFixture]
public class ComparatorTests {
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    public void Comparator_DataDriven(int bitCount) {
        int maxVal = (1 << bitCount) - 1;
        for (int a = 0; a <= maxVal; a++) {
            for (int b = 0; b <= maxVal; b++) {
                var simulation = TestHelpers.Build(
                    BuiltInCircuits.CreateComparator(bitCount), BuiltInCircuits.Circuits);
                for (int i = 0; i < bitCount; i++) {
                    simulation.SetInput($"A{i}", 0, ((a >> i & 1) != 0).ToSignal());
                    simulation.SetInput($"B{i}", 0, ((b >> i & 1) != 0).ToSignal());
                }
                simulation.Step();
                using (Assert.EnterMultipleScope()) {
                    Assert.That(simulation.GetOutput("GT", 0), Is.EqualTo((a > b).ToSignal()), $"A=0x{a:X} > B=0x{b:X}");
                    Assert.That(simulation.GetOutput("EQ", 0), Is.EqualTo((a == b).ToSignal()), $"A=0x{a:X} == B=0x{b:X}");
                    Assert.That(simulation.GetOutput("LT", 0), Is.EqualTo((a < b).ToSignal()), $"A=0x{a:X} < B=0x{b:X}");
                }
            }
        }
    }
}
