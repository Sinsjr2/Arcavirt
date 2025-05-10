namespace Gdb.Thread.MultiThread;

public interface IGDBMultiThread : IGDBThread {

    void ReadRegister(int i, int tid);

    void WriteRegister(int i, int tid);

}