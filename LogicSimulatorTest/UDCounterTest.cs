using NUnit.Framework;
using LogicSimulator;
using LogicSimulator.Gates;

namespace LogicSimulatorTest;

[TestFixture]
public class UDCounterTest {
    /// <summary>
    /// カウントシナリオ1つ分(開始値、クロック回数、ラベル、カウント方向)
    /// </summary>
    public record CountScenario(
        int StartValue,
        int ClockCount,
        bool IsUp,      // true=カウントアップ, false=カウントダウン
        string Label
    );

    /// <summary>
    /// UDCounter データ駆動テスト用 Config
    /// </summary>
    public record UDCounterTestConfig(
        int BitCount,
        // null → Enumerable.Range(0, 1<<BitCount) で全件生成
        int[]? ExplicitPresetValues,
        CountScenario[] CountUpScenarios,
        CountScenario[] CountDownScenarios
    );

    public static IEnumerable<TestCaseData> UDCounterTestCases => [
        // 1bit
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 1,
            ExplicitPresetValues: null,
            CountUpScenarios: [ new(StartValue: 0, ClockCount: 4, IsUp: true, Label: "countup_full") ],
            CountDownScenarios: [ new(StartValue: 1, ClockCount: 4, IsUp: false, Label: "countdown_full") ]
        )).SetDescription("1bit"),

        // 2bit
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 2,
            ExplicitPresetValues: null,
            CountUpScenarios: [ new(StartValue: 0, ClockCount: 8, IsUp: true, Label: "countup_full") ],
            CountDownScenarios: [ new(StartValue: 3, ClockCount: 8, IsUp: false, Label: "countdown_full") ]
        )).SetDescription("2bit"),

        // 4bit
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 4,
            ExplicitPresetValues: null,
            CountUpScenarios: [ new(StartValue: 0, ClockCount: 32, IsUp: true, Label: "countup_full") ],
            CountDownScenarios: [ new(StartValue: 15, ClockCount: 32, IsUp: false, Label: "countdown_full") ]
        )).SetDescription("4bit"),

        // 8bit(修正4適用: StartValue: 0x01, ClockCount: 3)
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 8,
            ExplicitPresetValues: [0x00, 0x01, 0x7F, 0x80, 0xFF],
            CountUpScenarios: [
                new(StartValue: 0x00, ClockCount: 16, IsUp: true,  Label: "countup_boundary"),
                new(StartValue: 0xFF, ClockCount: 3,  IsUp: true,  Label: "overflow"),
                new(StartValue: 0x80, ClockCount: 4,  IsUp: true,  Label: "midvalue"),
            ],
            CountDownScenarios: [
                new(StartValue: 0x01, ClockCount: 3, IsUp: false, Label: "countdown_underflow"),
            ]
        )).SetDescription("8bit"),

        // 16bit(修正4適用: StartValue: 0x0001, ClockCount: 3)
        new TestCaseData(new UDCounterTestConfig(
            BitCount: 16,
            ExplicitPresetValues: [0x0000, 0x0001, 0x8000, 0xFFFF],
            CountUpScenarios: [
                new(StartValue: 0x0000, ClockCount: 2, IsUp: true,  Label: "countup_boundary"),
                new(StartValue: 0xFFFF, ClockCount: 4, IsUp: true,  Label: "overflow"),
                new(StartValue: 0x8000, ClockCount: 2, IsUp: true,  Label: "midvalue"),
            ],
            CountDownScenarios: [
                new(StartValue: 0x0001, ClockCount: 3, IsUp: false, Label: "countdown_underflow"),
            ]
        )).SetDescription("16bit"),
    ];

    [TestCaseSource(nameof(UDCounterTestCases))]
    [CancelAfter(10000)] // 動的設定不可のため最大値を固定(1/2/4bit全件でも余裕あり)
    public void UDCounter_DataDriven(UDCounterTestConfig cfg) {
        int bitCount = cfg.BitCount;
        int maxValue = (1 << bitCount) - 1;
        int modulus  = maxValue + 1;

        // Phase 0: 初期化
        var circuit    = BuiltInCircuits.CreateUDCounter(bitCount);
        var simulation = TestHelpers.Build(circuit, BuiltInCircuits.Circuits);
        simulation.SetInput("CLK", 0, LogicSignal.Low);
        simulation.SetInput("DIR", 0, LogicSignal.Low);
        simulation.SetInput("LOW", 0, LogicSignal.Low);
        simulation.SetInput("SET", 0, LogicSignal.Low);
        for (int j = 0; j < bitCount; j++) {
            simulation.SetInput($"INITIAL{j}", 0, LogicSignal.Low);
        }
        simulation.Step();

        // Phase 1: リセット確認
        simulation.SetInput("SET", 0, LogicSignal.High);
        simulation.Step();
        for (int j = 0; j < bitCount; j++) {
            Assert.That(simulation.GetOutput($"D{j}", "in"), Is.EqualTo(LogicSignal.Low), $"reset: D{j}");
        }

        // Phase 2: プリセット確認
        var presetValues = cfg.ExplicitPresetValues ?? Enumerable.Range(0, modulus).ToArray();
        foreach (int preset in presetValues) {
            for (int j = 0; j < bitCount; j++) {
                simulation.SetInput($"INITIAL{j}", 0, ((preset & (1 << j)) != 0).ToSignal());
            }
            simulation.SetInput("SET", 0, LogicSignal.High);
            simulation.Step();
            // 2a. SET=High中の確認
            for (int j = 0; j < bitCount; j++) {
                Assert.That(simulation.GetOutput($"D{j}", "in"),
                    Is.EqualTo(((preset & (1 << j)) != 0).ToSignal()),
                    $"preset=0x{preset:X}: D{j}");
            }
            // 2b. SET解除後のホールド確認
            simulation.SetInput("SET", 0, LogicSignal.Low);
            simulation.Step();
            for (int j = 0; j < bitCount; j++) {
                Assert.That(simulation.GetOutput($"D{j}", "in"),
                    Is.EqualTo(((preset & (1 << j)) != 0).ToSignal()),
                    $"after preset clear, preset=0x{preset:X}: D{j}");
            }
        }

        // Phase 3: カウントアップシナリオ群
        foreach (var scenario in cfg.CountUpScenarios) {
            SetupCountScenario(simulation, bitCount, scenario.StartValue, isUp: true);
            RunCountScenario(simulation, bitCount, maxValue, modulus, scenario);
        }

        // Phase 4: カウントダウンシナリオ群
        foreach (var scenario in cfg.CountDownScenarios) {
            SetupCountScenario(simulation, bitCount, scenario.StartValue, isUp: false);
            RunCountScenario(simulation, bitCount, maxValue, modulus, scenario);
        }
    }

    static void SetupCountScenario(LogicSimulation simulation, int bitCount,
        int startValue, bool isUp) {
        for (int j = 0; j < bitCount; j++) {
            simulation.SetInput($"INITIAL{j}", 0, ((startValue & (1 << j)) != 0).ToSignal());
        }
        simulation.SetInput("SET", 0, LogicSignal.High);
        simulation.Step();

        simulation.SetInput("SET", 0, LogicSignal.Low);
        // LOW=High: 上位ビットからのキャリー入力をHighにし、カウンタを動作可能にする
        simulation.SetInput("LOW", 0, LogicSignal.High);
        simulation.SetInput("DIR", 0, isUp ? LogicSignal.Low : LogicSignal.High);
        simulation.Step();
        // この時点でHIが組み合わせ論理として確定
    }

    static void RunCountScenario(LogicSimulation simulation, int bitCount,
        int maxValue, int modulus, CountScenario scenario) {
        int startValue = scenario.StartValue;
        bool isUp      = scenario.IsUp;
        string label   = scenario.Label;

        // 初期HI確認(クロック前)
        bool hiInitial = (isUp && startValue == maxValue) || (!isUp && startValue == 0);
        Assert.That(simulation.GetOutput("HI", "in"), Is.EqualTo(hiInitial.ToSignal()),
            $"{label} pre-clk HI");

        for (int i = 0; i < scenario.ClockCount; i++) {
            simulation.SetInput("CLK", 0, LogicSignal.High);
            simulation.Step();
            simulation.SetInput("CLK", 0, LogicSignal.Low);
            simulation.Step();

            int expectedValue = isUp
                ? (startValue + i + 1) % modulus
                : ((startValue - (i + 1)) & maxValue);

            using (Assert.EnterMultipleScope()) {
                for (int j = 0; j < bitCount; j++) {
                    bool expectedBit = (expectedValue & (1 << j)) != 0;
                    Assert.That(simulation.GetOutput($"D{j}", "in"), Is.EqualTo(expectedBit.ToSignal()),
                        $"{label} i={i}: D{j}");
                }
                bool hiExpected = (isUp && expectedValue == maxValue) || (!isUp && expectedValue == 0);
                Assert.That(simulation.GetOutput("HI", "in"), Is.EqualTo(hiExpected.ToSignal()),
                    $"{label} i={i}: HI");
            }
        }
    }
}
