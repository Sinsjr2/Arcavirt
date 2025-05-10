namespace Gdb.BreakPoint;

public interface IHardWareBreakPoint {

    bool AddHwBreakPoint(ulong address, uint kind);

    bool RemoveHwBreakPoint(ulong address, uint kind);
}