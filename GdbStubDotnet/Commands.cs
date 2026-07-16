using Pidgin;

namespace GdbStubDotnet;

/// <summary>
/// H(SetThread)による現在スレッド選択状態を、後続コマンドのThreadフィールド
/// へ充填するための内部インタフェース(§6.6)。SyncDispatchedCommand が
/// ハンドラ呼び出し直前にWithThreadで差し替える。TCmdを型引数に取らない
/// 非ジェネリックインタフェースとし、呼び出し側でTCmdへキャストし直す
/// (TCmdは制約なしのため、自己参照ジェネリック制約は形成できない)。
/// </summary>
internal interface IThreadScoped {
    ThreadId Thread { get; }
    object WithThread(ThreadId thread);
}

public readonly record struct ReadRegistersCommand(ThreadId Thread) : IThreadScoped {
    public object WithThread(ThreadId thread) => this with { Thread = thread };
}

public readonly record struct ReadMemoryCommand(ulong Addr, int Len, ThreadId Thread) : IThreadScoped {
    public object WithThread(ThreadId thread) => this with { Thread = thread };
}

public readonly record struct WriteMemoryCommand(ulong Addr, int Len, byte[] Data, ThreadId Thread) : IThreadScoped {
    public object WithThread(ThreadId thread) => this with { Thread = thread };
}

public readonly record struct CurrentThreadCommand;

public readonly record struct ThreadInfoStartCommand;

public readonly record struct SupportedCommand(byte[] Features);

public readonly record struct QueryCommand(byte[] Name, byte[] Args);

public readonly record struct WriteRegistersCommand(byte[] Data, ThreadId Thread) : IThreadScoped {
    public object WithThread(ThreadId thread) => this with { Thread = thread };
}

public readonly record struct ReadRegisterCommand(int Number, ThreadId Thread) : IThreadScoped {
    public object WithThread(ThreadId thread) => this with { Thread = thread };
}

public readonly record struct WriteRegisterCommand(int Number, byte[] Data, ThreadId Thread) : IThreadScoped {
    public object WithThread(ThreadId thread) => this with { Thread = thread };
}

public readonly record struct InsertBreakpointCommand(BpType Type, ulong Addr, int Kind);

public readonly record struct RemoveBreakpointCommand(BpType Type, ulong Addr, int Kind);

public readonly record struct ContinueCommand(ulong? Addr);

public readonly record struct HaltReasonCommand;

public readonly record struct StepCommand(ulong? Addr);

public readonly record struct VContCommand(ResumeAction[] Actions);

public readonly record struct VContQueryCommand;

public readonly record struct SetThreadCommand(char Op, ThreadId Thread);

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

    /// <summary>
    /// vCont のスレッド指定("p&lt;pid&gt;.&lt;tid&gt;" または裸の "&lt;tid&gt;")。
    /// GDB-RP の "-1"(全スレッド)センチネルは10進リテラルであり16進ではないため、
    /// 先に literal "-1" を Try で試してから通常の16進数へフォールバックする。
    /// 裸の tid のみの形式では Pid=0(未指定)として扱う。SetThread(H)からも
    /// 共用する。
    /// </summary>
    private static readonly Parser<byte, int> ThreadIdComponent =
        Parser.Try(Parser<byte>.Sequence("-1"u8.ToArray()).ThenReturn(-1))
            .Or(HexParsers.HexULong.Select(static v => (int)v));

    private static readonly Parser<byte, ThreadId> ThreadIdRef =
        Parser.Try(
            Parser<byte>.Token((byte)'p')
                .Then(ThreadIdComponent)
                .Before(Parser<byte>.Token((byte)'.'))
                .Then(ThreadIdComponent, static (pid, tid) => new ThreadId(pid, tid)))
            .Or(ThreadIdComponent.Select(static tid => new ThreadId(0, tid)));

    /// <summary>
    /// vCont の1アクション分("C" sig 以外は signal は0固定)。
    /// "S sig"(ステップ+シグナル)は ActionKind に対応する値が存在しないため
    /// 未対応(仕様確定済みDTOの範囲外)。該当パケットはこのパーサ全体が
    /// 不一致になり、FW既定の空応答(§5.1)にフォールバックする。
    /// </summary>
    private static readonly Parser<byte, (ActionKind Kind, int Signal)> VContActionSpec =
        Parser.OneOf(
            Parser<byte>.Token((byte)'C').Then(HexParsers.HexULong, static (_, sig) => (ActionKind.Signal, (int)sig)),
            Parser<byte>.Token((byte)'c').ThenReturn((ActionKind.Continue, 0)),
            Parser<byte>.Token((byte)'s').ThenReturn((ActionKind.Step, 0)),
            Parser<byte>.Token((byte)'t').ThenReturn((ActionKind.Stop, 0)));

    private static readonly Parser<byte, ResumeAction> VContSegment =
        Parser<byte>.Token((byte)';')
            .Then(VContActionSpec)
            .Then(
                Parser<byte>.Token((byte)':').Then(ThreadIdRef).Select(static t => (ThreadId?)t)
                    .Or(Parser<byte>.Return((ThreadId?)null)),
                static (spec, threadOpt) => new ResumeAction(threadOpt ?? default, spec.Kind, spec.Signal));

    public static readonly Parser<byte, VContCommand> VCont =
        Parser<byte>.Sequence("vCont"u8.ToArray())
            .Then(VContSegment.AtLeastOnce(), static (_, actions) => new VContCommand(actions.ToArray()));

    public static readonly Parser<byte, VContQueryCommand> VContQuery =
        Parser<byte>.Sequence("vCont?"u8.ToArray()).ThenReturn(new VContQueryCommand());

    /// <summary>
    /// H&lt;op&gt;&lt;thread-id&gt; (§6.6)。op は 'g'(汎用操作用)/'c'
    /// (レガシーなcontinue/step用、vCont普及以降は非推奨)のいずれか。
    /// 本ライブラリは両者を区別せず、既定ハンドラが単一の「現在スレッド」
    /// として扱う(利用者は上書き可能、§6.6)。
    /// </summary>
    public static readonly Parser<byte, SetThreadCommand> SetThread =
        Parser<byte>.Token((byte)'H')
            .Then(Parser<byte>.Token((byte)'g').Or(Parser<byte>.Token((byte)'c')))
            .Then(ThreadIdRef, static (op, thread) => new SetThreadCommand((char)op, thread));

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