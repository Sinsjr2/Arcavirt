namespace Gdb.BreakPoint;

public interface IBreakPoint {
    IHardWareBreakPoint? HwBreakPointObject { get; }
    ISoftWareBreakPoint? SwBreakPointObject { get; }
    IWatchPoint? WatchPointObject { get; }
}
