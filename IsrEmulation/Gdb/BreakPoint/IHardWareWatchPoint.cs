namespace Gdb.BreakPoint;

public interface IHardWareWatchPoint {
    bool AddHwWatchPoint(ulong address, ulong length, BreakWatchKind kind);
    bool RemoveHwWatchPoint(ulong address, ulong length, BreakWatchKind kind);
}