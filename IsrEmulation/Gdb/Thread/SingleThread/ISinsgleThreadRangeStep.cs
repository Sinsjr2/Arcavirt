namespace Gdb.Thread.SingleThread;

public interface ISingleThreadRangeStep {
    void Step(byte? signal = null);
}