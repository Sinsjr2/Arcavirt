using Gdb.BreakPoint;
using Gdb.Thread;

namespace Gdb;

public interface IGdbStub {

    IBreakPoint? BreakpointObject { get; }

    IGDBThread? ThreadObject { get; }

    string TargetDescriptionXML { get; }

    event Action OnBreak;

    void HandleCtrlC();
}