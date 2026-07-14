using Pidgin;

namespace GdbStubDotnet;

public readonly record struct ReadRegistersCommand(ThreadId Thread);

public static class Commands {
    public static readonly Parser<byte, ReadRegistersCommand> ReadRegisters =
        Parser<byte>.Token((byte)'g').ThenReturn(new ReadRegistersCommand(default));
}