namespace Gdb.Thread.SingleThread;

public interface ISingleThreadStep {
    void RangeStep(ulong start, ulong end);
}