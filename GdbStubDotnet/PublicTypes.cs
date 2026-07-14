namespace GdbStubDotnet;

public readonly record struct ThreadId(int Pid, int Tid);

public enum ActionKind {
    Continue,
    Step,
    Stop,
    Signal,
}

public readonly record struct ResumeAction(ThreadId Thread, ActionKind Kind, int Signal);

public enum ResumeMode {
    AllStop,
    NonStop,
}

public enum StopReason {
    SwBreak,
    HwBreak,
    Watch,
    Signal,
    Exited,
    Terminated,
}

public enum BpType {
    Soft,
    Hard,
    Write,
    Read,
    Access,
}

public readonly record struct StopEvent(ThreadId Thread, StopReason Reason, int SignalOrExit, ulong WatchAddr);

public readonly record struct RspError(int Code, string? Detail);

public readonly record struct StubFault(Exception Error, string Context);