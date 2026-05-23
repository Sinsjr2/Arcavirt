using NUnit.Framework;
using LogicSimulator;

namespace LogicSimulatorTest;

[TestFixture]
public class RangeCounterTest {
    public record RangeCounter16BitTestCase(
        int A,
        int B,
        int Initial,
        int Max,
        int ExpectedRangeInitial
    );

    public record RangeCounter16BitCountTestCase(
        int Initial,
        int Max,
        int Dir,
        int ClockCount,
        int ExpectedD,
        int? A,
        int? B,
        int? ExpectedRangeAfter
    );

    public record RangeCounter16BitRangeTransitionTestCase(
        int A,
        int B,
        int Initial,
        int Max,
        int Dir,
        int ClockCount,
        int ExpectedD,
        int ExpectedRangeInitial,
        int ExpectedRangeFinal
    );

    public static IEnumerable<TestCaseData> RangeCounter16BitTestCases => [
        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 15,
            Max: 65535,
            ExpectedRangeInitial: 1
        )).SetDescription("in_range_middle"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 5,
            Max: 65535,
            ExpectedRangeInitial: 0
        )).SetDescription("out_range_below"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 25,
            Max: 65535,
            ExpectedRangeInitial: 0
        )).SetDescription("out_range_above"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 10,
            Max: 65535,
            ExpectedRangeInitial: 1
        )).SetDescription("at_lower_bound"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 10,
            B: 20,
            Initial: 20,
            Max: 65535,
            ExpectedRangeInitial: 1
        )).SetDescription("at_upper_bound"),

        // グループ8: 16ビット幅の確認
        new TestCaseData(new RangeCounter16BitTestCase(
            A: 0,
            B: 65535,
            Initial: 0x5555,
            Max: 65535,
            ExpectedRangeInitial: 1
        )).SetDescription("G8-01: 交互ビット（0x5555）が正しくロードされる"),

        new TestCaseData(new RangeCounter16BitTestCase(
            A: 0,
            B: 65535,
            Initial: 0xAAAA,
            Max: 65535,
            ExpectedRangeInitial: 1
        )).SetDescription("G8-02: 交互ビット（0xAAAA）が正しくロードされる"),
    ];

    [TestCaseSource(nameof(RangeCounter16BitTestCases))]
    [CancelAfter(10000)]
    public void RangeCounter16Bit_DataDriven(RangeCounter16BitTestCase testCase) {
        var sim = TestHelpers.Build(
            BuiltInCircuits.RangeCounter16Bit,
            BuiltInCircuits.Circuits);

        InitializeRangeCounter(sim);

        SetupRangeCounterInputs(sim, testCase.A, testCase.B, testCase.Initial, testCase.Max);

        sim.SetInput("SET", 0, LogicSignal.High);
        sim.Step();

        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.Step();

        CheckRangeOutput(sim, testCase.ExpectedRangeInitial);

        CheckCounterOutput(sim, testCase.Initial);
    }

    public static IEnumerable<TestCaseData> RangeCounter16BitCountTestCases => [
        // グループ2: カウントアップ（DIR=0）
        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 1,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G2-01: 1回カウント"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 65535,
            Dir: 0,
            ClockCount: 5,
            ExpectedD: 5,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G2-02: 複数回カウント（5回）"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 9,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 10,
            A: 10,
            B: 20,
            ExpectedRangeAfter: 1
        )).SetDescription("G2-03: 下限境界到達"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 20,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 21,
            A: 10,
            B: 20,
            ExpectedRangeAfter: 0
        )).SetDescription("G2-04: 上限超過"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 65535,
            Dir: 0,
            ClockCount: -1,
            ExpectedD: 0,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G2-05: CLK継続High時にカウントなし"),

        // グループ3: カウントダウン（DIR=1）
        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 10,
            Max: 65535,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 9,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G3-01: 1回カウントダウン"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 10,
            Max: 65535,
            Dir: 1,
            ClockCount: 5,
            ExpectedD: 5,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G3-02: 複数回カウントダウン（5回）"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 21,
            Max: 65535,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 20,
            A: 10,
            B: 20,
            ExpectedRangeAfter: 1
        )).SetDescription("G3-03: 上限境界到達"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 10,
            Max: 65535,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 9,
            A: 10,
            B: 20,
            ExpectedRangeAfter: 0
        )).SetDescription("G3-04: 下限超過"),

        // グループ5: ラップアラウンド（Down方向）
        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 100,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 100,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G5-01: D=0→ラップしてMAX（=100）"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 65535,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 65535,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G5-02: D=0, MAX=65535→ラップして65535"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 1,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 1,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G5-03: MAX=1でラップ"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 5,
            Max: 100,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 4,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G5-04: D>0はラップしない"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 10,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 10,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G5-05: MAX=10でラップ"),

        // グループ4: ラップアラウンド（Up方向）
        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 65535,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G4-01: D=MAX→ラップして0"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 100,
            Max: 100,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G4-02: 任意MAX（100）でラップ"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 65535,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G4-03: MAX=65535（全ビット1）でラップ"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 1,
            Max: 1,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G4-04: MAX=1でラップ（最小限のカウント幅）"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0,
            Max: 0,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G4-05: MAX=0でラップ→0のまま（極端）"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 5,
            Max: 10,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 6,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G4-06: D<MAXはラップしない"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 65535,
            Max: 65535,
            Dir: 0,
            ClockCount: 3,
            ExpectedD: 2,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G4-07: ラップ後も継続してカウント（3CLK）"),

        // グループ8: 16ビット幅の確認
        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0x00FF,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0x0100,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G8-03: 繰り上がりを跨ぐカウント（0x00FF→0x0100）"),

        new TestCaseData(new RangeCounter16BitCountTestCase(
            Initial: 0xFF00,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0xFF01,
            A: null,
            B: null,
            ExpectedRangeAfter: null
        )).SetDescription("G8-04: 上位8ビット境界（0xFF00→0xFF01）"),
    ];

    public static IEnumerable<TestCaseData> RangeCounter16BitRangeTransitionTestCases => [
        new TestCaseData(new RangeCounter16BitRangeTransitionTestCase(
            A: 10,
            B: 20,
            Initial: 9,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 10,
            ExpectedRangeInitial: 0,
            ExpectedRangeFinal: 1
        )).SetDescription("G7-01: Up、D=A-1→Aでレンジ入り"),

        new TestCaseData(new RangeCounter16BitRangeTransitionTestCase(
            A: 10,
            B: 20,
            Initial: 20,
            Max: 65535,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 21,
            ExpectedRangeInitial: 1,
            ExpectedRangeFinal: 0
        )).SetDescription("G7-02: Up、D=B→B+1でレンジ外"),

        new TestCaseData(new RangeCounter16BitRangeTransitionTestCase(
            A: 10,
            B: 20,
            Initial: 21,
            Max: 65535,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 20,
            ExpectedRangeInitial: 0,
            ExpectedRangeFinal: 1
        )).SetDescription("G7-03: Down、D=B+1→Bでレンジ入り"),

        new TestCaseData(new RangeCounter16BitRangeTransitionTestCase(
            A: 10,
            B: 20,
            Initial: 10,
            Max: 65535,
            Dir: 1,
            ClockCount: 1,
            ExpectedD: 9,
            ExpectedRangeInitial: 1,
            ExpectedRangeFinal: 0
        )).SetDescription("G7-04: Down、D=A→A-1でレンジ外"),

        new TestCaseData(new RangeCounter16BitRangeTransitionTestCase(
            A: 0,
            B: 5,
            Initial: 3,
            Max: 3,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0,
            ExpectedRangeInitial: 1,
            ExpectedRangeFinal: 1
        )).SetDescription("G7-05: ラップ後の値がRange内"),

        new TestCaseData(new RangeCounter16BitRangeTransitionTestCase(
            A: 5,
            B: 10,
            Initial: 3,
            Max: 3,
            Dir: 0,
            ClockCount: 1,
            ExpectedD: 0,
            ExpectedRangeInitial: 0,
            ExpectedRangeFinal: 0
        )).SetDescription("G7-06: ラップ後の値がRange外"),
    ];

    [TestCaseSource(nameof(RangeCounter16BitCountTestCases))]
    [CancelAfter(10000)]
    public void RangeCounter16Bit_Count_DataDriven(RangeCounter16BitCountTestCase testCase) {
        var sim = TestHelpers.Build(
            BuiltInCircuits.RangeCounter16Bit,
            BuiltInCircuits.Circuits);

        InitializeRangeCounter(sim);

        SetupRangeCounterInputs(sim, testCase.A ?? 0, testCase.B ?? 0, testCase.Initial, testCase.Max);

        sim.SetInput("SET", 0, LogicSignal.High);
        sim.Step();
        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.Step();

        // DIR信号を設定（0:カウントアップ, 1:カウントダウン）
        sim.SetInput("DIR", 0, testCase.Dir == 0 ? LogicSignal.Low : LogicSignal.High);
        sim.Step();

        SimulateClockPulses(sim, testCase.ClockCount);
        CheckCounterOutput(sim, testCase.ExpectedD);

        if (testCase.ExpectedRangeAfter.HasValue) {
            CheckRangeOutput(sim, testCase.ExpectedRangeAfter.Value);
        }
    }

    [TestCaseSource(nameof(RangeCounter16BitRangeTransitionTestCases))]
    [CancelAfter(10000)]
    public void RangeCounter16Bit_RangeTransition_DataDriven(RangeCounter16BitRangeTransitionTestCase testCase) {
        var sim = TestHelpers.Build(
            BuiltInCircuits.RangeCounter16Bit,
            BuiltInCircuits.Circuits);

        InitializeRangeCounter(sim);

        SetupRangeCounterInputs(sim, testCase.A, testCase.B, testCase.Initial, testCase.Max);

        sim.SetInput("SET", 0, LogicSignal.High);
        sim.Step();
        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.Step();

        // RANGE初期値を確認
        CheckRangeOutput(sim, testCase.ExpectedRangeInitial);

        // DIR信号を設定（0:カウントアップ, 1:カウントダウン）
        sim.SetInput("DIR", 0, testCase.Dir == 0 ? LogicSignal.Low : LogicSignal.High);
        sim.Step();

        // クロックパルスを実行
        SimulateClockPulses(sim, testCase.ClockCount);

        // D値を確認
        CheckCounterOutput(sim, testCase.ExpectedD);

        // RANGE最終値を確認
        CheckRangeOutput(sim, testCase.ExpectedRangeFinal);
    }

    private static void InitializeRangeCounter(LogicSimulation sim) {
        sim.SetInput("CLK", 0, LogicSignal.Low);
        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.SetInput("DIR", 0, LogicSignal.Low);

        for (int i = 0; i < 16; i++) {
            sim.SetInput($"INITIAL{i}", 0, LogicSignal.Low);
            sim.SetInput($"A{i}", 0, LogicSignal.Low);
            sim.SetInput($"B{i}", 0, LogicSignal.Low);
            sim.SetInput($"MAX{i}", 0, LogicSignal.Low);
        }

        sim.Step();

        sim.SetInput("SET", 0, LogicSignal.High);
        sim.Step();
        sim.SetInput("SET", 0, LogicSignal.Low);
        sim.Step();
    }

    private static void SetupRangeCounterInputs(LogicSimulation sim, int a, int b, int initial, int max) {
        for (int i = 0; i < 16; i++) {
            sim.SetInput($"INITIAL{i}", 0, ((initial & (1 << i)) != 0).ToSignal());
            sim.SetInput($"A{i}", 0, ((a & (1 << i)) != 0).ToSignal());
            sim.SetInput($"B{i}", 0, ((b & (1 << i)) != 0).ToSignal());
            sim.SetInput($"MAX{i}", 0, ((max & (1 << i)) != 0).ToSignal());
        }
    }

    private static void CheckRangeOutput(LogicSimulation sim, int expectedRange) {
        LogicSignal expected = (expectedRange != 0) ? LogicSignal.High : LogicSignal.Low;
        Assert.That(
            sim.GetOutput("RANGE", 0),
            Is.EqualTo(expected));
    }

    private static void CheckCounterOutput(LogicSimulation sim, int expectedValue) {
        for (int i = 0; i < 16; i++) {
            LogicSignal expected = ((expectedValue & (1 << i)) != 0) ? LogicSignal.High : LogicSignal.Low;
            Assert.That(
                sim.GetOutput($"D{i}", 0),
                Is.EqualTo(expected),
                $"D{i}");
        }
    }

    private static void SimulateClockPulses(LogicSimulation sim, int count) {
        for (int i = 0; i < count; i++) {
            sim.SetInput("CLK", 0, LogicSignal.High);
            sim.Step();
            sim.SetInput("CLK", 0, LogicSignal.Low);
            sim.Step();
        }
    }
}
