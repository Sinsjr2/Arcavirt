namespace Gdb.BreakPoint;

public interface IHardWareBreakPoint {

    bool AddHwBreakPoint(ulong address, uint length);

    bool RemoveHwBreakPoint(ulong address, uint length);
}