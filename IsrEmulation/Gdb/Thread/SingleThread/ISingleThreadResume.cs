namespace Gdb.Thread.SingleThread;

public interface ISingleThreadResume {

    ISingleThreadStep? StepObject { get; }

    ISingleThreadRangeStep? RangeStepObject { get; }

    void Resume(ulong? address, byte? signal);
}