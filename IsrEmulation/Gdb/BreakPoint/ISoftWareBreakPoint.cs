namespace Gdb.BreakPoint;

public interface ISoftWareBreakPoint {

    bool AddSwBreakPoint(ulong address, uint kind);

    bool RemoveSwBreakPoint(ulong address, uint kind);
}