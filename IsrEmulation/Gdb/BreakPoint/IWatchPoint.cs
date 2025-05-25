namespace Gdb.BreakPoint;

public interface IWatchPoint {
    bool AddWatchPoint(ulong address, ulong length, BreakWatchKind kind);
    bool RemoveWatchPoint(ulong address, ulong length, BreakWatchKind kind);
}