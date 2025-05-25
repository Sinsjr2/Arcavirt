namespace Gdb.Thread.SingleThread;

public interface ISingleThreadRangeStep {
    void RangeStep(ulong start, ulong end);
}