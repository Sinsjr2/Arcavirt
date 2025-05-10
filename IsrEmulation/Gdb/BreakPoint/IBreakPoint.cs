namespace Gdb.BreakPoint;

public interface IBreakPoint {
    IHardWareBreakPoint? HwBreakPointObject { get; }
    ISoftWareBreakPoint? SwBreakPointObject { get; }
    IHardWareWatchPoint? HwWatchPointObject { get; }
}
