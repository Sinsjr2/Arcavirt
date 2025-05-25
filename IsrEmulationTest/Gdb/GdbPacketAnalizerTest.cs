using System.Text;
using Gdb;
using Gdb.BreakPoint;
using Gdb.Thread;
using Gdb.Thread.SingleThread;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TestTools;
namespace IsrEmulationTest.Gdb;

/// <summary>
/// 何も値を持たないことを表します。
/// </summary>
public class Unit {
    public static readonly Unit Default = new();
    Unit() {}
};

public class MockMethod<TArgument, TReturn> {
    /// <summary>
    /// メソッド呼び出し時に渡された値の履歴
    /// </summary>
    public readonly List<TArgument> ArgumentHistory = [];

    public int NumOfCalled => ArgumentHistory.Count;

    public readonly TReturn InitialReturnValue;

    public TReturn ReturnValue;

    public MockMethod(TReturn returnValue) {
        InitialReturnValue = returnValue;
        ReturnValue = returnValue;
    }

    /// <summary>
    /// 関数を呼び出し事前に設定された値を返します。
    /// </summary>
    public TReturn Call(TArgument argument) {
        ArgumentHistory.Add(argument);
        return ReturnValue;
    }

    public void Clear() {
        ArgumentHistory.Clear();
        ReturnValue = InitialReturnValue;
    }
}

public class DummyGdbSingleThreadStub :
    IGDBSingleThread, ISingleThreadResume, ISingleThreadRangeStep, ISingleThreadStep,
    IBreakPoint, IHardWareBreakPoint, ISoftWareBreakPoint, IWatchPoint {

    public ISingleThreadResume? ResumeObject { get; set; } = null;

    public ISingleThreadStep? StepObject { get; set; } = null;

    public ISingleThreadRangeStep? RangeStepObject => throw new NotImplementedException();

    public int NumOfRegisters { get; set; } = 0;

    public IHardWareBreakPoint? HwBreakPointObject => this;

    public ISoftWareBreakPoint? SwBreakPointObject => this;

    public IWatchPoint? WatchPointObject => this;

    public readonly MockMethod<(ulong start, ulong end), Unit> RangeStepMethod = new(Unit.Default);

    public readonly MockMethod<(ulong startAddr, ulong length), string?> ReadMemoryMethod = new("");

    public readonly MockMethod<int, string?> ReadRegisterMethod = new("");

    public readonly MockMethod<(ulong? address, byte? signal), Unit> ResumeMethod = new(Unit.Default);

    public readonly MockMethod<(ulong? address, byte? signal) , Unit> StepMethod = new(Unit.Default);

    public readonly MockMethod<(ulong startAddr, string values), bool> WriteMemoryMethod = new(true);

    public readonly MockMethod<(int i, string value), int> WriteRegisterMethod = new (0);

    public readonly MockMethod<(ulong address, uint length), bool> AddHwBreakPointMethod = new(true);

    public readonly MockMethod<(ulong address, uint length), bool> RemoveHwBreakPointMethod = new(true);

    public readonly MockMethod<(ulong address, uint length), bool> AddSwBreakPointMethod = new(true);

    public readonly MockMethod<(ulong address, uint length), bool> RemoveSwBreakPointMethod = new(true);

    public readonly MockMethod<(ulong address, ulong length, BreakWatchKind kind), bool> AddWatchPointMethod = new(true);

    public readonly MockMethod<(ulong address, ulong length, BreakWatchKind kind), bool> RemoveWatchPointMethod = new(true);

    public void RangeStep(ulong start, ulong end) {
        RangeStepMethod.Call((start, end));
    }

    public bool ReadMemory(StringBuilder result, ulong startAddr, ulong length) {
        var ret = ReadMemoryMethod.Call((startAddr, length));
        if (ret == null) {
            return false;
        }
        result.Append(ret);
        return true;
    }

    public int ReadRegister(int i, StringBuilder result) {
        var beforeLength = result.Length;
        var ret = ReadRegisterMethod.Call(i);
        if (ret == null) {
            return 0;
        }
        result.Append(ret);
        var afterLength = result.Length;
        return afterLength - beforeLength;        
    }

    public void Resume(ulong? address, byte? signal) {
        ResumeMethod.Call((address, signal));
    }

    public void Step(ulong? address, byte? signal = null) {
        StepMethod.Call((address, signal));
    }

    public bool WriteMemory(ulong startAddr, ReadOnlySpan<char> values) {
        return WriteMemoryMethod.Call((startAddr, new string(values)));
    }

    public int WriteRegisters(int i, ReadOnlySpan<char> value) {
        return WriteRegisterMethod.Call((i, new string(value)));
    }

    public bool AddHwBreakPoint(ulong address, uint length) {
        return AddHwBreakPointMethod.Call((address, length));
    }

    public bool RemoveHwBreakPoint(ulong address, uint length) {
        return RemoveHwBreakPointMethod.Call((address, length));
    }

    public bool AddSwBreakPoint(ulong address, uint length) {
        return AddSwBreakPointMethod.Call((address, length));
    }

    public bool RemoveSwBreakPoint(ulong address, uint length) {
        return RemoveSwBreakPointMethod.Call((address, length));
    }

    public bool AddWatchPoint(ulong address, ulong length, BreakWatchKind kind) {
        return AddWatchPointMethod.Call((address, length, kind));
    }

    public bool RemoveWatchPoint(ulong address, ulong length, BreakWatchKind kind) {
        return RemoveWatchPointMethod.Call((address, length, kind));
    }
}

public class DummyGdbStub : IGdbStub, IBreakPoint, IHardWareBreakPoint, ISoftWareBreakPoint, IWatchPoint {

    public IBreakPoint? BreakpointObject { get; set; } = null;

    public IGDBThread? ThreadObject { get; set; } = null;

    public string TargetDescriptionXML { get; set; } = "";

    public IHardWareBreakPoint? HwBreakPointObject { get; set; } = null;

    public ISoftWareBreakPoint? SwBreakPointObject { get; set; } = null;

    public IWatchPoint? WatchPointObject { get; set; } = null;
    
    public event Action? OnBreak;

    public readonly MockMethod<(ulong address, uint kind), bool> AddHwBreakPointMethod = new(true);

    public readonly MockMethod<(ulong address, ulong lengthh, BreakWatchKind kind), bool> AddWatchPointMethod = new(true);

    public readonly MockMethod<(ulong address, uint kind), bool> AddSwBreakPointMethod = new(true);

    public readonly MockMethod<Unit, Unit> HandleCtrlCMethod = new(Unit.Default);

    public readonly MockMethod<(ulong address, uint kind), bool> RemoveHwBreakPointMethod = new(true);

    public readonly MockMethod<(ulong address, ulong length, BreakWatchKind kind), bool> RemoveHwWatchPointMethod = new(true);

    public readonly MockMethod<(ulong address, uint kind), bool> RemoveSwBreakPointMethod = new(true);

    public void RunOnBreak() {
        OnBreak?.Invoke();
    }

    public bool AddHwBreakPoint(ulong address, uint kind) {
        return AddHwBreakPointMethod.Call((address, kind));
    }

    public bool AddWatchPoint(ulong address, ulong length, BreakWatchKind kind) {
        return AddWatchPointMethod.Call((address, length, kind));
    }

    public bool AddSwBreakPoint(ulong address, uint kind) {
        return AddSwBreakPointMethod.Call((address, kind));
    }

    public void HandleCtrlC() {
        HandleCtrlCMethod.Call(Unit.Default);
    }

    public bool RemoveHwBreakPoint(ulong address, uint kind) {
        return RemoveHwBreakPointMethod.Call((address, kind));
    }

    public bool RemoveWatchPoint(ulong address, ulong length, BreakWatchKind kind) {
        return RemoveHwWatchPointMethod.Call((address, length, kind));
    }

    public bool RemoveSwBreakPoint(ulong address, uint kind) {
        return RemoveSwBreakPointMethod.Call((address, kind));
    }
}

public class LogBuffer : ILogger {
    public readonly List<string> History = [];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        History.Add($"{logLevel} {eventId} {formatter(state, exception)} ");
    }
}

[CancelAfter(10000)]
public class GdbPacketAnalizerTest {

    /// <summary>
    /// 1要求 1応答のコマンドのテストを行います。
    /// </summary>
    async Task OneCommandTest(IGdbStub stub, string inputCommand, string expectedResponse, CancellationToken token) {
        var targetCommunication = new ChannelCommunication();
        var logger = new LogBuffer();
        var analyzer = new GdbPacketAnalizer(stub, logger);
        var encoding = Encoding.UTF8;
        var sb = new StringBuilder();
        GDBUtils.GDBMessage(sb, inputCommand);
        var cts = new CancellationTokenSource();
        var task = analyzer.CommandAnalizeLoop(targetCommunication, cts.Token);

        await WriteAll(targetCommunication, encoding.GetBytes(sb.ToString()), token);
        await AssertExpectedGdbResponse(targetCommunication, expectedResponse, token);

        cts.Cancel();
        await task;
    }

    [Test]
    public async Task Command_Unsupport_Test(CancellationToken token) {
        var src = "unsupport";
        var expectedResponse = "+$#00";
        var stub = new DummyGdbStub();
        await OneCommandTest(stub, src, expectedResponse, token);
    }

    [Test]
    public async Task Command_Question_Test(CancellationToken token) {
        var src = "?";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        await OneCommandTest(stub, src, expectedResponse, token);
    }

    [Test]
    public async Task Command_g_Test(CancellationToken token) {
        var src = "g";
        var expectedResponse = "+$abab#86";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        singleThreadStub.NumOfRegisters = 2;
        singleThreadStub.ReadRegisterMethod.ReturnValue = "ab";
        stub.ThreadObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
    }

    [Test]
    public async Task Command_G_Test(CancellationToken token) {
        var src = "G1234abcd";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        singleThreadStub.NumOfRegisters = 4;
        singleThreadStub.WriteRegisterMethod.ReturnValue = 2;
        stub.ThreadObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.WriteRegisterMethod.ArgumentHistory
            .Is([(0, "1234abcd"), (1, "34abcd"), (2, "abcd"), (3, "cd")]);
    }

    [Test]
    public async Task Command_m_Test(CancellationToken token) {
        var src = "m1234abCD,3000";
        var expectedResponse = "+$00#60";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        singleThreadStub.ReadMemoryMethod.ReturnValue = "00";
        stub.ThreadObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.ReadMemoryMethod.ArgumentHistory.Is([(0x1234abCD, 0x3000)]);
    }

    [Test]
    public async Task Command_M_Test(CancellationToken token) {
        var src = "M1234abCD,a:0102030405a0b0C0d0E0";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        singleThreadStub.WriteMemoryMethod.ReturnValue = true;
        stub.ThreadObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.WriteMemoryMethod.ArgumentHistory.Is([(0x1234abCD, "0102030405a0b0C0d0E0")]);
    }

    [Test]
    public async Task Command_M_wrongLength_Test(CancellationToken token) {
        var src = "M1234abCD,1:0102";
        var expectedResponse = "+$E00#a5";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.ThreadObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.WriteMemoryMethod.ArgumentHistory.Count.Is(0);
    }

    [Test]
    public async Task Command_s_Test(CancellationToken token) {
        var src = "s";
        var expectedResponse = "+$S05#b8";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.ThreadObject = singleThreadStub;
        singleThreadStub.ResumeObject = singleThreadStub;
        singleThreadStub.StepObject = singleThreadStub;
        var task = OneCommandTest(stub, src, expectedResponse, token);
        while (true) {
            token.ThrowIfCancellationRequested();
            if (singleThreadStub.StepMethod.ArgumentHistory.Any()) {
                break;
            }
            await Task.Yield();
        }
        singleThreadStub.StepMethod.ArgumentHistory.Is([(null, null)]);
        stub.RunOnBreak();
        await task;
    }

    [Test]
    public async Task Command_s_address_Test(CancellationToken token) {
        var src = "sabcd1234";
        var expectedResponse = "+$S05#b8";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        singleThreadStub.ResumeObject = singleThreadStub;
        singleThreadStub.StepObject = singleThreadStub;
        stub.ThreadObject = singleThreadStub;
        var task = OneCommandTest(stub, src, expectedResponse, token);
        while (true) {
            token.ThrowIfCancellationRequested();
            if (singleThreadStub.StepMethod.ArgumentHistory.Count != 0) {
                break;
            }
            await Task.Yield();
        }
        singleThreadStub.StepMethod.ArgumentHistory.Is([(0xABCD1234, null)]);
        stub.RunOnBreak();
        await task;
    }

    [Test]
    public async Task Command_c_Test(CancellationToken token) {
        var src = "c";
        var expectedResponse = "";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.ThreadObject = singleThreadStub;
        singleThreadStub.ResumeObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.ResumeMethod.ArgumentHistory.Is([(null, null)]);
    }

    [Test]
    public async Task Command_Z0_Test(CancellationToken token) {
        var src = "Z0,12ABCDFF,1";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.AddSwBreakPointMethod.ArgumentHistory.Is([(0x12ABCDFFu, 1)]);
    }

    [Test]
    public async Task Command_z0_Test(CancellationToken token) {
        var src = "z0,FFA98765,4";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.RemoveSwBreakPointMethod.ArgumentHistory.Is([(0xFFA98765u, 4)]);
    }

    [Test]
    public async Task Command_Z1_Test(CancellationToken token) {
        var src = "Z1,3456789A,2";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.AddHwBreakPointMethod.ArgumentHistory.Is([(0x3456789Au, 2)]);
    }

    [Test]
    public async Task Command_z1_Test(CancellationToken token) {
        var src = "z1,456789AB,1";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.RemoveHwBreakPointMethod.ArgumentHistory.Is([(0x456789ABu, 1)]);
    }

    [Test]
    public async Task Command_Z2_Test(CancellationToken token) {
        var src = "Z2,56789ABC,8";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.AddWatchPointMethod.ArgumentHistory.Is([(0x56789ABCu, 8, BreakWatchKind.Write)]);
    }

    [Test]
    public async Task Command_z2_Test(CancellationToken token) {
        var src = "z2,789ABCDE,4";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.RemoveWatchPointMethod.ArgumentHistory.Is([(0x789ABCDEu, 4, BreakWatchKind.Write)]);
    }

    [Test]
    public async Task Command_Z3_Test(CancellationToken token) {
        var src = "Z3,9ABCDEF0,10";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.AddWatchPointMethod.ArgumentHistory.Is([(0x9ABCDEF0u, 16, BreakWatchKind.Read)]);
    }

    [Test]
    public async Task Command_z3_Test(CancellationToken token) {
        var src = "z3,BCDEF012,20";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.RemoveWatchPointMethod.ArgumentHistory.Is([(0xBCDEF012u, 32, BreakWatchKind.Read)]);
    }

    [Test]
    public async Task Command_Z4_Test(CancellationToken token) {
        var src = "Z4,DEF0,1";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.AddWatchPointMethod.ArgumentHistory.Is([(0xDEF0u, 1, BreakWatchKind.ReadWrite)]);
    }

    [Test]
    public async Task Command_z4_Test(CancellationToken token) {
        var src = "z4,DEF0,2";
        var expectedResponse = "+$OK#9a";
        var stub = new DummyGdbStub();
        var singleThreadStub = new DummyGdbSingleThreadStub();
        stub.BreakpointObject = singleThreadStub;
        stub.SwBreakPointObject = singleThreadStub;
        await OneCommandTest(stub, src, expectedResponse, token);
        singleThreadStub.RemoveWatchPointMethod.ArgumentHistory.Is([(0xDEF0u, 2, BreakWatchKind.ReadWrite)]);
    }

    static async ValueTask WriteAll(ChannelCommunication target, ReadOnlyMemory<byte> src, CancellationToken token) {
        for (int i = 0; i < src.Length;) {
            var buf = await target.WaitReadBuffer(token);
            var remaining = src[i..];
            var copyTarget = remaining[..Math.Min(buf.Length, remaining.Length)];
            copyTarget.CopyTo(buf);
            target.CompleteReadNotify(copyTarget.Length);
            i += copyTarget.Length;
        }
    }

    /// <summary>
    /// 期待値が受信できていることを確認します。
    /// </summary>
    public async ValueTask AssertExpectedGdbResponse(ChannelCommunication target, string expected, CancellationToken token) {
        var encode = Encoding.UTF8;
        var received = new StringBuilder();
        bool isCalled = false;
        if (expected.Length == 0) {
            // 何も到達していないことを確認するために少し待機した後に読み出す
            await Task.Delay(10, token);
            target.TryGetWriteBuffer(out _).Is(false);
        }
        for (; received.Length < expected.Length;) {
            isCalled = true;
            var buf = await target.WaitWriteBuffer(token);
            received.Append(encode.GetChars(buf.ToArray()));
            target.CompleteWriteNotify();
            received.ToString().Is(new string(expected.AsSpan(0, received.Length)));
        }
        // TODO バッファーを開放する前に終了しないようにするため応急処置
        if (isCalled) {
            await Task.Delay(10, token);
        }
    }
}