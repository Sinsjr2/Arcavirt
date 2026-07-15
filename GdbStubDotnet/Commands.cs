using Pidgin;

namespace GdbStubDotnet;

public readonly record struct ReadRegistersCommand(ThreadId Thread);

public readonly record struct ReadMemoryCommand(ulong Addr, int Len, ThreadId Thread);

public readonly record struct WriteMemoryCommand(ulong Addr, int Len, byte[] Data, ThreadId Thread);

public readonly record struct CurrentThreadCommand;

public readonly record struct ThreadInfoStartCommand;

public readonly record struct SupportedCommand(byte[] Features);

public readonly record struct QueryCommand(byte[] Name, byte[] Args);

public readonly record struct WriteRegistersCommand(byte[] Data, ThreadId Thread);

public readonly record struct ReadRegisterCommand(int Number, ThreadId Thread);

public readonly record struct WriteRegisterCommand(int Number, byte[] Data, ThreadId Thread);

public readonly record struct InsertBreakpointCommand(BpType Type, ulong Addr, int Kind);

public readonly record struct RemoveBreakpointCommand(BpType Type, ulong Addr, int Kind);

public readonly record struct ContinueCommand(ulong? Addr);

public readonly record struct HaltReasonCommand;

public readonly record struct StepCommand(ulong? Addr);

public readonly record struct VContCommand(ResumeAction[] Actions);

public readonly record struct VContQueryCommand;

public static class Commands {
    public static readonly Parser<byte, ReadRegistersCommand> ReadRegisters =
        Parser<byte>.Token((byte)'g').ThenReturn(new ReadRegistersCommand(default));

    public static readonly Parser<byte, ReadMemoryCommand> ReadMemory =
        Parser<byte>.Token((byte)'m')
            .Then(HexParsers.HexULong)
            .Before(Parser<byte>.Token((byte)','))
            .Then(HexParsers.HexULong, static (addr, len) => new ReadMemoryCommand(addr, (int)len, default));

    public static readonly Parser<byte, WriteMemoryCommand> WriteMemory =
        Parser<byte>.Token((byte)'M')
            .Then(HexParsers.HexULong)
            .Before(Parser<byte>.Token((byte)','))
            .Then(HexParsers.HexULong, static (addr, len) => (addr, len))
            .Before(Parser<byte>.Token((byte)':'))
            .Then(HexParsers.HexBytesRest, static (t, data) => new WriteMemoryCommand(t.addr, (int)t.len, data, default));

    public static readonly Parser<byte, CurrentThreadCommand> CurrentThread =
        Parser<byte>.Sequence("qC"u8.ToArray()).ThenReturn(new CurrentThreadCommand());

    public static readonly Parser<byte, ThreadInfoStartCommand> ThreadInfoStart =
        Parser<byte>.Sequence("qfThreadInfo"u8.ToArray()).ThenReturn(new ThreadInfoStartCommand());

    public static readonly Parser<byte, SupportedCommand> Supported =
        Parser<byte>.Sequence("qSupported"u8.ToArray())
            .Then(
                Parser<byte>.Token((byte)':').Then(Parser<byte>.Any.Many().Select(static bs => bs.ToArray()))
                    .Or(Parser<byte>.Return(Array.Empty<byte>())))
            .Select(static features => new SupportedCommand(features));

    public static readonly Parser<byte, QueryCommand> Query =
        Parser<byte>.Token((byte)'q')
            .Then(Parser<byte>.Any.AtLeastOnce().Select(static bs => bs.ToArray()))
            .Select(static name => new QueryCommand(name, []));

    public static readonly Parser<byte, WriteRegistersCommand> WriteRegisters =
        Parser<byte>.Token((byte)'G')
            .Then(HexParsers.HexBytesRest, static (_, data) => new WriteRegistersCommand(data, default));

    public static readonly Parser<byte, ReadRegisterCommand> ReadRegister =
        Parser<byte>.Token((byte)'p')
            .Then(HexParsers.HexULong, static (_, number) => new ReadRegisterCommand((int)number, default));

    public static readonly Parser<byte, WriteRegisterCommand> WriteRegister =
        Parser<byte>.Token((byte)'P')
            .Then(HexParsers.HexULong)
            .Before(Parser<byte>.Token((byte)'='))
            .Then(HexParsers.HexBytesRest, static (number, data) => new WriteRegisterCommand((int)number, data, default));

    public static readonly Parser<byte, InsertBreakpointCommand> InsertBreakpoint =
        Parser<byte>.Token((byte)'Z')
            .Then(HexParsers.HexULong)
            .Before(Parser<byte>.Token((byte)','))
            .Then(HexParsers.HexULong, static (type, addr) => (type, addr))
            .Before(Parser<byte>.Token((byte)','))
            .Then(HexParsers.HexULong, static (t, kind) => new InsertBreakpointCommand(ParseBpType(t.type), t.addr, (int)kind));

    public static readonly Parser<byte, RemoveBreakpointCommand> RemoveBreakpoint =
        Parser<byte>.Token((byte)'z')
            .Then(HexParsers.HexULong)
            .Before(Parser<byte>.Token((byte)','))
            .Then(HexParsers.HexULong, static (type, addr) => (type, addr))
            .Before(Parser<byte>.Token((byte)','))
            .Then(HexParsers.HexULong, static (t, kind) => new RemoveBreakpointCommand(ParseBpType(t.type), t.addr, (int)kind));

    public static readonly Parser<byte, ContinueCommand> Continue =
        Parser<byte>.Token((byte)'c')
            .Then(
                HexParsers.HexULong.Select(static addr => (ulong?)addr)
                    .Or(Parser<byte>.Return((ulong?)null)))
            .Select(static maybeAddr => new ContinueCommand(maybeAddr));

    public static readonly Parser<byte, HaltReasonCommand> HaltReason =
        Parser<byte>.Token((byte)'?').ThenReturn(new HaltReasonCommand());

    public static readonly Parser<byte, StepCommand> Step =
        Parser<byte>.Token((byte)'s')
            .Then(
                HexParsers.HexULong.Select(static addr => (ulong?)addr)
                    .Or(Parser<byte>.Return((ulong?)null)))
            .Select(static maybeAddr => new StepCommand(maybeAddr));

    public static readonly Parser<byte, VContCommand> VCont =
        Parser<byte>.Sequence("vCont;c"u8.ToArray())
            .ThenReturn(new VContCommand([new ResumeAction(default, ActionKind.Continue, 0)]));

    public static readonly Parser<byte, VContQueryCommand> VContQuery =
        Parser<byte>.Sequence("vCont?"u8.ToArray()).ThenReturn(new VContQueryCommand());

    private static BpType ParseBpType(ulong value) {
        return value switch {
            0 => BpType.Soft,
            1 => BpType.Hard,
            2 => BpType.Write,
            3 => BpType.Read,
            4 => BpType.Access,
            _ => BpType.Soft,
        };
    }
}