using Clock;

namespace CPU;

public class InstructionLoop<TTarget> where TTarget : IInstructionStep {
    public readonly TTarget Target;

    readonly SimulationClock clock;

    readonly Action<Action> runOnLoop;

    readonly Action execute;

    public bool IsRunning { get; private set; } = false;

    /// <summary>
    /// 一度の <see cref="Execute"/> の呼び出しで実行するクロック数 
    /// </summary>
    int oneExecuteClockCount;

    public InstructionLoop(
        Action<Action> runOnLoop,
        TTarget target,
        SimulationClock clock,
        int oneExecuteClockCount = 1000000)
    {
        execute = Execute;
        this.oneExecuteClockCount = oneExecuteClockCount;
        this.clock = clock;
        Target = target;
        this.runOnLoop = runOnLoop;
    }

    public void Start() {
        IsRunning = true;
        runOnLoop(execute);
    }

    void Execute() {
        if (!IsRunning) {
            return;
        }
        for (int i = 0; i < oneExecuteClockCount && IsRunning; i++) {
            if (Target.Waiting) {
                var nanosToNextAlarm = clock.Nanos;
                clock.Tick(nanosToNextAlarm);
                i += (int)(nanosToNextAlarm / Target.CycleNanoSec);
            } else {
                var cycles = Target.NextStep();
                clock.Tick(cycles * Target.CycleNanoSec);
                i += cycles;
            }
        }
        if (IsRunning) {
            runOnLoop(execute);
        }
    }

    public void Stop() {
        IsRunning = false;
    }
}