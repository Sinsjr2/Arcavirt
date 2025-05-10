namespace Gdb.Thread.MultiThread;

public interface IMultiThreadStep {
    void Step(byte? signal = null);
}