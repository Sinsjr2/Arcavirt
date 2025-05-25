namespace Gdb.Thread.SingleThread;

public interface ISingleThreadStep {
    void Step(ulong? address, byte? signal = null);
}