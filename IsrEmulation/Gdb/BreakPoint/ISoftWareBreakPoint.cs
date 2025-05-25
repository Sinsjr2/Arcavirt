namespace Gdb.BreakPoint;

public interface ISoftWareBreakPoint {

    bool AddSwBreakPoint(ulong address, uint length);

    bool RemoveSwBreakPoint(ulong address, uint length);
}