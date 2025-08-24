using System.Buffers;
using System.Text;
using System.Threading.Channels;
using Gdb.BreakPoint;
using Gdb.Thread.SingleThread;
using IsrEmulation.Util;
using Microsoft.Extensions.Logging;

namespace Gdb;

public enum GdbLoopRequest {
    /// <summary>
    /// GDBのリクエスト待ちを通知します。
    /// </summary>
    WaitToRequest,

    /// <summary>
    /// ブレークが発生したことを通知します。
    /// </summary>
    OnBreak,
}

public class GDBMessage {
    public static readonly char Break = '\x03';
    public static readonly string OK = "OK";
}

public enum GdbPacketInputKind {
    OnReceivedChar,
    Response,
    Exit,
}

public record struct GdbPacketInput {
    public GdbPacketInputKind Kind;
    public string Message;
    public char InputChar;

    public GdbPacketInput(GdbPacketInputKind kind, string message, char inputChar) {
        Kind = kind;
        Message = message;
        InputChar = inputChar;
    }
}

public enum GdbPacketOutputKind {
    None,
    CpuBreakRequest,
    // SendMessage,
    OnReceivedPacket,
    Exit
}

public struct GdbPacketOutput {
    public GdbPacketOutputKind Kind;
    public StringBuilder SendMessage;
    public StringBuilder ReceivedMessage;

    public GdbPacketOutput(GdbPacketOutputKind kind, StringBuilder sendMessage, StringBuilder receivedMessage) {
        Kind = kind;
        SendMessage = sendMessage;
        ReceivedMessage = receivedMessage;
    }
}

public class GdbCommunicator {

    enum GdbPacketState {
        Start,
        Idle,
        FoundPacketStart,
        EscapeChar,
        RunLengthEncoding,
        FoundCheckSum1,
        FoundCheckSum2,
        PacketSending,
        Exit
    }

    GdbPacketState gdbPacketState = GdbPacketState.Start;
    ArrayBufferWriter<char> receivedDatas = new();

    ArrayBufferWriter<char> sendingDatas = new();
    byte packetSum = 0;

    byte checkSum = 0;

    bool nonAckMode = false;

    int maxPayloadSize = 16384;

    int retrySendMax = 3;

    int retrySend = 0;

    public void Step(GdbPacketInput input, ref GdbPacketOutput result) {
        if (gdbPacketState == GdbPacketState.Start) {
            gdbPacketState = GdbPacketState.Idle;
        }
        if (gdbPacketState == GdbPacketState.Exit) {
            result.Kind = GdbPacketOutputKind.Exit;
            return;
        }
        if (gdbPacketState == GdbPacketState.Idle) {
            if (input.Kind == GdbPacketInputKind.Exit) {
                gdbPacketState = GdbPacketState.Exit;
                result.Kind = GdbPacketOutputKind.Exit;
                return;
            } else if (input.Kind == GdbPacketInputKind.OnReceivedChar) {
                if (input.InputChar == '$') {
                    receivedDatas.Clear();
                    packetSum = 0;
                    gdbPacketState = GdbPacketState.FoundPacketStart;
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                } else if (input.InputChar == '\x03') {
                    result.Kind = GdbPacketOutputKind.CpuBreakRequest;
                    return;
                }
            } else if (input.Kind == GdbPacketInputKind.Response) {
                result.SendMessage.Append(input.Message);
                result.Kind = GdbPacketOutputKind.None;
                if (!nonAckMode) {
                    retrySend = 0;
                    sendingDatas.Clear();
                    input.Message.CopyTo(sendingDatas.GetSpan(input.Message.Length));
                    sendingDatas.Advance(input.Message.Length);
                    gdbPacketState = GdbPacketState.PacketSending;
                }
                return;
            }
        } else if (gdbPacketState == GdbPacketState.FoundPacketStart) {
            if (input.Kind == GdbPacketInputKind.OnReceivedChar) {
                if (input.InputChar == '}') {
                    result.Kind = GdbPacketOutputKind.None;
                    unchecked {
                        packetSum += (byte)input.InputChar;
                    }
                    gdbPacketState = GdbPacketState.EscapeChar;
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                } else if (input.InputChar == '*') {
                    unchecked {
                        packetSum += (byte)input.InputChar;
                    }
                    gdbPacketState = GdbPacketState.RunLengthEncoding;
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                } else if (input.InputChar == '#') {
                    gdbPacketState = GdbPacketState.FoundCheckSum1;
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                } else if (maxPayloadSize <= receivedDatas.WrittenCount) {
                    Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()!.Name}: command buffer overrun, dropping command");
                    gdbPacketState = GdbPacketState.Idle;
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                } else {
                    unchecked {
                        packetSum += (byte)input.InputChar;
                    }
                    receivedDatas.GetSpan(1)[0] = input.InputChar;
                    receivedDatas.Advance(1);
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                }
            }
        } else if (gdbPacketState == GdbPacketState.EscapeChar) {
            if (input.InputChar == '#') {
                gdbPacketState = GdbPacketState.FoundCheckSum1;
                result.Kind = GdbPacketOutputKind.None;
                return;
            } else if (maxPayloadSize <= receivedDatas.WrittenCount) {
                Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()!.Name}: command buffer overrun, dropping command");
                gdbPacketState = GdbPacketState.Idle;
                result.Kind = GdbPacketOutputKind.None;
                return;
            } else {
                receivedDatas.GetSpan(1)[0] = (char)(input.InputChar ^ 0x20);
                receivedDatas.Advance(1);
                unchecked {
                    packetSum += (byte)input.InputChar;
                }
                gdbPacketState = GdbPacketState.FoundPacketStart;
                result.Kind = GdbPacketOutputKind.None;
                return;
            }
        } else if (gdbPacketState == GdbPacketState.RunLengthEncoding) {
            if (input.InputChar < ' ') {
                Console.WriteLine($"{nameof(Step)}: got invalid RLE count: 0x{input.InputChar:X}");
                gdbPacketState = GdbPacketState.FoundPacketStart;
                result.Kind = GdbPacketOutputKind.None;
                return;
            } else {
                int repeat = input.InputChar - ' ' + 3;
                if (maxPayloadSize <= receivedDatas.WrittenCount) {
                    Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod()!.Name}: command buffer overrun, dropping command");
                    gdbPacketState = GdbPacketState.Idle;
                } else if (receivedDatas.WrittenCount < 1) {
                    Console.WriteLine($"{nameof(Step)}: got invalid RLE sequence");
                    gdbPacketState = GdbPacketState.FoundPacketStart;
                } else {
                    var repeatChar = receivedDatas.WrittenSpan[^1];
                    receivedDatas.GetSpan(repeat)[..repeat].Fill(repeatChar);
                    receivedDatas.Advance(repeat);
                    unchecked {
                        packetSum += (byte)input.InputChar;
                    }
                    gdbPacketState = GdbPacketState.FoundPacketStart;
                }
                result.Kind = GdbPacketOutputKind.None;
                return;
            }
        } else if (gdbPacketState == GdbPacketState.FoundCheckSum1) {
            if (!GDBUtils.TryParseHexToByte(input.InputChar, out var checkSumValue)) {
                Console.WriteLine($"{nameof(Step)}:got invalid command checksum digit");
                gdbPacketState = GdbPacketState.Idle;
                result.Kind = GdbPacketOutputKind.None;
                return;
            } else {
                this.checkSum = (byte)(checkSumValue << 4);
                gdbPacketState = GdbPacketState.FoundCheckSum2;
                result.Kind = GdbPacketOutputKind.None;
                return;
            }
        } else if (gdbPacketState == GdbPacketState.FoundCheckSum2) {
            if (!GDBUtils.TryParseHexToByte(input.InputChar, out var checkSumValue)) {
                Console.WriteLine($"{nameof(Step)}:got invalid command checksum digit");
                gdbPacketState = GdbPacketState.Idle;
                result.Kind = GdbPacketOutputKind.None;
                return;
            } else {
                this.checkSum |= checkSumValue;
                if (packetSum != checkSum) {
                    Console.WriteLine("gdbserver: got command packet with incorrect checksum");
                    result.SendMessage.Append('-');
                    result.Kind = GdbPacketOutputKind.None;
                    gdbPacketState = GdbPacketState.Idle;
                    return;
                } else {
                    result.SendMessage.Append('+');
                    result.ReceivedMessage.Append(receivedDatas.WrittenSpan);
                    result.Kind = GdbPacketOutputKind.None;
                    gdbPacketState = GdbPacketState.Idle;
                    return;
                }
            }
        } else if (gdbPacketState == GdbPacketState.PacketSending) {
            if (input.Kind == GdbPacketInputKind.OnReceivedChar) {
                if (input.InputChar == '+') {
                    gdbPacketState = GdbPacketState.Idle;
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                } else if (retrySendMax <= retrySend) {
                    // 再送上限に達した
                    gdbPacketState = GdbPacketState.Idle;
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                } else {
                    retrySend++;
                    result.SendMessage.Append(sendingDatas.WrittenSpan);
                    result.Kind = GdbPacketOutputKind.None;
                    return;
                }
            }
        }
        result.Kind = GdbPacketOutputKind.None;
        return;
    }
}

public enum GdbStubStateMachineInputKind {
    OnCpuBreak,
    OnReceivedCpuBreakRequest,
    OnReceivedStubRequest,
    OnCompleteStubRequest,
}

public record struct GdbStubStateMachineInput {
    public GdbStubStateMachineInputKind Kind;
    public string Message;

    public GdbStubStateMachineInput(GdbStubStateMachineInputKind kind, string message) {
        Kind = kind;
        Message = message;
    }
}

public enum GdbStubStateMachineOutputKind {
    None,
    StopCpu,
    RunCpu,
    /// <summary>
    /// 命令を1ステップ実行する
    /// </summary>
    StepInstruction,
    OnReceivedStubRequest,
}

public struct GdbStubStateMachineOutput {
    public GdbStubStateMachineOutputKind Kind;
    public StringBuilder SendMessage;

    public GdbStubStateMachineOutput(GdbStubStateMachineOutputKind kind, StringBuilder sendMessage) {
        Kind = kind;
        SendMessage = sendMessage;
    }
}

public class GdbStubStateMachine {
    enum State {
        Idle,
        CpuRunning,
        RequestProcessing
    }

    State gdbStubState = State.Idle;

    public void Step(GdbStubStateMachineInput input, ref GdbStubStateMachineOutput result) {
        if (gdbStubState == State.Idle) {
            if (input.Kind == GdbStubStateMachineInputKind.OnReceivedStubRequest) {
                var reader = new GdbMessageReader(input.Message.AsMemory());
                if (!reader.TryReadChar(out var c)) {
                    result.Kind = GdbStubStateMachineOutputKind.None;
                    return;
                }
                switch (c) {
                    case 'c':
                        gdbStubState = State.CpuRunning;
                        result.Kind = GdbStubStateMachineOutputKind.RunCpu;
                        return;
                    case 's':
                        gdbStubState = State.CpuRunning;
                        result.Kind = GdbStubStateMachineOutputKind.StepInstruction;
                        return;
                    default:
                        gdbStubState = State.RequestProcessing;
                        result.Kind = GdbStubStateMachineOutputKind.OnReceivedStubRequest;
                        break;
                }
            }
        } else if (gdbStubState == State.CpuRunning) {
            if (input.Kind is GdbStubStateMachineInputKind.OnCpuBreak) {
                // c コマンドで実行しているので停止シグナルで応答する
                gdbStubState = State.Idle;
                result.Kind = GdbStubStateMachineOutputKind.None;
                GDBUtils.GDBMessage(result.SendMessage, "S05");
                return;
            } else if (input.Kind == GdbStubStateMachineInputKind.OnReceivedCpuBreakRequest) {
                result.Kind = GdbStubStateMachineOutputKind.StopCpu;
                return;
            }
        } else if (gdbStubState == State.RequestProcessing) {
            if (input.Kind is GdbStubStateMachineInputKind.OnCompleteStubRequest) {
                gdbStubState = State.Idle;
                result.Kind = GdbStubStateMachineOutputKind.None;
                return;
            }
        }
    }
}

public class GdbPacketAnalizerCStubLike {

    static readonly string STOP_REPLY_SIGINT = "S02";
    static readonly string STOP_REPLY_TRAP = "S05";

    readonly StreamWriter streamWriter;
    readonly StreamReader streamReader;

    readonly GdbStubStateMachine gdbStubStateMachine = new();
    readonly GdbCommunicator gdbCommunicator = new();
    readonly AsyncEventFlag writeStreamLock = ScopedLock.Create(true);

    readonly StringBuilder sendMessageBuilder_GdbPacket = new();
    readonly StringBuilder receivedMessageBuilder_GdbPacket = new();

    readonly StringBuilder sendGdbPacketBuilder = new();


    readonly IGdbStub targetStub;

    readonly ILogger logger;

    readonly int rxBufferSize = 1024;

    readonly Action<object?, Func<object?, ValueTask>> runOn;

    readonly Channel<GdbLoopRequest> loopRequestChannel =
        Channel.CreateUnbounded<GdbLoopRequest>(new UnboundedChannelOptions() { AllowSynchronousContinuations = true });

    public GdbPacketAnalizerCStubLike(
        IGdbStub targetStub, ILogger logger,
        Action<object?, Func<object?, ValueTask>> runOn,
        Stream writeStream, Stream readStream) {

        this.targetStub = targetStub;
        this.logger = logger;
        this.runOn = runOn;
        this.streamWriter = new StreamWriter(writeStream, Encoding.ASCII);
        this.streamReader = new StreamReader(readStream, Encoding.ASCII);
    }

    bool TryReadAllRegisters(StringBuilder response, ReadOnlySpan<char> cmd) {
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread:
                for (int i = 0; i < singleThread.NumOfRegisters; i++) {
                    if (singleThread.ReadRegister(i, response) == 0) {
                        return false;
                    }
                }
                return true;
        }
        return false;
    }

    bool TryWriteAllRegisters(StringBuilder response, ReadOnlySpan<char> cmd) {
        int pos = 0;
        cmd = cmd[1..];
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread:
                for (int i = 0; i < singleThread.NumOfRegisters; i++) {
                    var writeLength = singleThread.WriteRegisters(i, cmd.Slice(pos));
                    if (writeLength == 0) {
                        return false;
                    }
                    pos += writeLength;
                }
                // 最後まで書き込めたことを確認します。
                if (cmd.Length == pos) {
                    response.Append("OK");
                    return true;
                }
                break;
        }
        return false;
    }

    bool TryReadMemory(StringBuilder response, ReadOnlyMemory<char> cmd) {
        var gdbReader = new GdbMessageReader(cmd);
        if (!(gdbReader.TryReadIfExpChar('m') &&
            gdbReader.TryReadHexUIntegerBE(out var address) &&
            gdbReader.TryReadIfExpChar(',') &&
            gdbReader.TryReadHexUIntegerBE(out var length))) {
            return false;
        }
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread:
                if (!singleThread.ReadMemory(response, address, length)) {
                    return false;
                }
                return true;
        }
        return false;
    }

    bool TryWriteMemory(StringBuilder response, ReadOnlyMemory<char> cmd) {
        var gdbReader = new GdbMessageReader(cmd);
        if (!(gdbReader.TryReadIfExpChar('M') &&
            gdbReader.TryReadHexUIntegerBE(out var address) &&
            gdbReader.TryReadIfExpChar(',') &&
            gdbReader.TryReadHexUIntegerBE(out var length) &&
            gdbReader.TryReadIfExpChar(':') &&
            length * 2 <= int.MaxValue &&
            gdbReader.TryReadExpectedLength((int)(length * 2), out var data) &&
            gdbReader.RemainingLength == 0)) {
            return false;
        }
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread:
                if (!singleThread.WriteMemory(address, data.Span)) {
                    return false;
                }
                response.Append("OK");
                return true;
        }
        return false;
    }

    bool TryStep(ReadOnlyMemory<char> cmd) {
        var gdbReader = new GdbMessageReader(cmd);
        if (!gdbReader.TryReadIfExpChar('s')) {
            return false;
        }
        ulong? address = gdbReader.TryReadHexUIntegerBE(out var x) ? x : null;
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread: {
                    var stepObject = singleThread.ResumeObject?.StepObject;
                    if (stepObject is not null) {
                        stepObject.Step(address, null);
                        return true;
                    }
                    break;
                }
        }
        return false;
    }

    bool TryResume(ulong? address, byte? signal) {
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread: {
                    var resumeObject = singleThread.ResumeObject;
                    if (resumeObject is not null) {
                        resumeObject.Resume(address, signal);
                        return true;
                    }
                    return false;
                }
        }
        return true;
    }

    bool TryExecuteCommand_C(ReadOnlyMemory<char> cmd) {
        var gdbReader = new GdbMessageReader(cmd);
        if (!gdbReader.TryReadIfExpChar('c')) {
            return false;
        }
        ulong? address = gdbReader.TryReadHexUIntegerBE(out var x) ? x : null;
        return TryResume(address, null);
    }

    bool TryAddBreakPoint(StringBuilder response, ReadOnlyMemory<char> cmd) {
        var gdbReader = new GdbMessageReader(cmd);
        if (!gdbReader.TryReadIfExpChar('Z')) {
            return false;
        }
        if (!gdbReader.TryReadHexUIntegerBE(out var type) ||
            !gdbReader.TryReadIfExpChar(',') ||
            !gdbReader.TryReadHexUIntegerBE(out var address) ||
            !gdbReader.TryReadIfExpChar(',') ||
            !gdbReader.TryReadHexUIntegerBE(out var kind)) {
            return false;
        }
        bool isSuccess = false;
        switch (type) {
            case 0: {
                    var swBreakPoint = targetStub.BreakpointObject?.SwBreakPointObject;
                    if (swBreakPoint != null) {
                        swBreakPoint.AddSwBreakPoint(address, (uint)kind);
                        isSuccess = true;
                    }
                    break;
                }
            case 1: {
                    var hwBreakPoint = targetStub.BreakpointObject?.HwBreakPointObject;
                    if (hwBreakPoint != null) {
                        hwBreakPoint.AddHwBreakPoint(address, (uint)kind);
                        isSuccess = true;
                    }
                    break;
                }
            case 2: {
                    var watchPoint = targetStub.BreakpointObject?.WatchPointObject;
                    if (watchPoint != null) {
                        watchPoint.AddWatchPoint(address, kind, BreakWatchKind.Write);
                        isSuccess = true;
                    }
                    break;
                }
            case 3: {
                    var watchPoint = targetStub.BreakpointObject?.WatchPointObject;
                    if (watchPoint != null) {
                        watchPoint.AddWatchPoint(address, kind, BreakWatchKind.Read);
                        isSuccess = true;
                    }
                    break;
                }
            case 4: {
                    var watchPoint = targetStub.BreakpointObject?.WatchPointObject;
                    if (watchPoint != null) {
                        watchPoint.AddWatchPoint(address, kind, BreakWatchKind.ReadWrite);
                        isSuccess = true;
                    }
                    break;
                }
        }
        if (isSuccess) {
            response.Append("OK");
        }
        return isSuccess;
    }

    bool TryRemoveBreakPoint(StringBuilder response, ReadOnlyMemory<char> cmd) {
        var gdbReader = new GdbMessageReader(cmd);
        if (!gdbReader.TryReadIfExpChar('z')) {
            return false;
        }
        if (!gdbReader.TryReadHexUIntegerBE(out var type) ||
            !gdbReader.TryReadIfExpChar(',') ||
            !gdbReader.TryReadHexUIntegerBE(out var address) ||
            !gdbReader.TryReadIfExpChar(',') ||
            !gdbReader.TryReadHexUIntegerBE(out var kind)) {
            return false;
        }
        bool isSuccess = false;
        switch (type) {
            case 0: {
                    var swBreakPoint = targetStub.BreakpointObject?.SwBreakPointObject;
                    if (swBreakPoint != null) {
                        swBreakPoint.RemoveSwBreakPoint(address, (uint)kind);
                        isSuccess = true;
                    }
                    break;
                }
            case 1: {
                    var hwBreakPoint = targetStub.BreakpointObject?.HwBreakPointObject;
                    if (hwBreakPoint != null) {
                        hwBreakPoint.RemoveHwBreakPoint(address, (uint)kind);
                        isSuccess = true;
                    }
                    break;
                }
            case 2: {
                    var watchPoint = targetStub.BreakpointObject?.WatchPointObject;
                    if (watchPoint != null) {
                        watchPoint.RemoveWatchPoint(address, kind, BreakWatchKind.Write);
                        isSuccess = true;
                    }
                    break;
                }
            case 3: {
                    var watchPoint = targetStub.BreakpointObject?.WatchPointObject;
                    if (watchPoint != null) {
                        watchPoint.RemoveWatchPoint(address, kind, BreakWatchKind.Read);
                        isSuccess = true;
                    }
                    break;
                }
            case 4: {
                    var watchPoint = targetStub.BreakpointObject?.WatchPointObject;
                    if (watchPoint != null) {
                        watchPoint.RemoveWatchPoint(address, kind, BreakWatchKind.ReadWrite);
                        isSuccess = true;
                    }
                    break;
                }
        }
        if (isSuccess) {
            response.Append("OK");
        }
        return isSuccess;
    }

    void TryRunCustomCommand(StringBuilder response, ReadOnlyMemory<char> cmd) {
        var reader = new GdbMessageReader(cmd);
        if (!reader.TryReadIfExpString("qRcmd,") ||
            !reader.TryReadExpectedLength(reader.RemainingLength, out var customCmd)) {
            return;
        }
        var encoding = Encoding.UTF8;
        var decodedCmd = encoding.GetString(Convert.FromHexString(customCmd.Span));
        if (logger.IsEnabled(LogLevel.Debug)) {
            logger.LogDebug($"qRcmd {decodedCmd}");
        }
        targetStub.GdbCustomCommandObject?.RunCustomCommand(response, decodedCmd);
    }

    async ValueTask StepGdbPacket(GdbPacketInput input, CancellationToken token) {
        sendMessageBuilder_GdbPacket.Clear();
        receivedMessageBuilder_GdbPacket.Clear();
        var gdbPacketOutput = new GdbPacketOutput(GdbPacketOutputKind.None, sendMessageBuilder_GdbPacket, receivedMessageBuilder_GdbPacket);
        gdbCommunicator.Step(input, ref gdbPacketOutput);
        if (gdbPacketOutput.Kind == GdbPacketOutputKind.CpuBreakRequest) {
            await StepGdbStubStateMachine(new GdbStubStateMachineOutput(GdbStubStateMachineOutputKind.StopCpu, sendMessageBuilder_GdbPacket), string.Empty, token);
        }
        if (0 < gdbPacketOutput.SendMessage.Length) {
            using (await ScopedLock.Lock(writeStreamLock, token)) {
                await streamWriter.WriteAsync(gdbPacketOutput.SendMessage, token);
                await streamWriter.FlushAsync(token);
            }
        }
        if (0 < gdbPacketOutput.ReceivedMessage.Length) {
            await StepGdbStubStateMachine(new GdbStubStateMachineInput(GdbStubStateMachineInputKind.OnReceivedStubRequest, gdbPacketOutput.ReceivedMessage.ToString()), token);
        }
    }

    async ValueTask StepGdbStubStateMachine(GdbStubStateMachineOutput data, string message, CancellationToken token) {
        if (data.Kind == GdbStubStateMachineOutputKind.StopCpu) {
            targetStub.HandleCtrlC();
        } else if (data.Kind == GdbStubStateMachineOutputKind.RunCpu) {
            TryResume(null, null);
        } else if (data.Kind == GdbStubStateMachineOutputKind.StepInstruction) {
            TryStep(message.AsMemory());
        } else if (data.Kind == GdbStubStateMachineOutputKind.OnReceivedStubRequest) {
            // 非同期であるので、重複して実行される可能性があるので、インスタンスを分ける
            var response = new StringBuilder();
            await ExecuteStubCommand(message, response, token);
            sendGdbPacketBuilder.Clear();
            var output = new GdbStubStateMachineOutput(GdbStubStateMachineOutputKind.None, sendGdbPacketBuilder);
            gdbStubStateMachine.Step(new GdbStubStateMachineInput(GdbStubStateMachineInputKind.OnCompleteStubRequest, string.Empty), ref output);
            var encoded = new StringBuilder();
            GDBUtils.GDBMessage(encoded, response);
            await StepGdbPacket(new GdbPacketInput(GdbPacketInputKind.Response, encoded.ToString(), default), token);
            await StepGdbStubStateMachine(output, string.Empty, token);
            sendGdbPacketBuilder.Clear();
        }
        if (0 < data.SendMessage.Length) {
            await StepGdbPacket(new GdbPacketInput(GdbPacketInputKind.Response, data.SendMessage.ToString(), default), token);
            data.SendMessage.Clear();
        }
    }

    async ValueTask StepGdbStubStateMachine(GdbStubStateMachineInput input, CancellationToken token) {
        sendGdbPacketBuilder.Clear();
        var output = new GdbStubStateMachineOutput(GdbStubStateMachineOutputKind.None, sendGdbPacketBuilder);
        gdbStubStateMachine.Step(input, ref output);
        await StepGdbStubStateMachine(output, input.Message, token);
    }

    async ValueTask ExecuteStubCommand(string cmd, StringBuilder response, CancellationToken token) {
        if (logger.IsEnabled(LogLevel.Debug)) {
            logger.LogDebug($"packet rx {cmd}");
        }
        switch (cmd[0]) {
            // case '!':
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
            case '?':
                response.Append(STOP_REPLY_TRAP);
                break;
            case 'q' when cmd.StartsWith("qSupported:"):
                // GDBUtils.GDBMessage(response, "PacketSize=4000;qXfer:memory-map:read+;qXfer:features:read+;qXfer:threads:read+;vContSupported+;multiprocess+;QNonStop+;swbreak+;hwbreak+");
                response.Append("PacketSize=4000;vContSupported+;qXfer:features:read+;");
                break;
            // if (cmd.Span.StartsWith("qTStatus")) {
            // }
            // if (cmd.Span.StartsWith("qAttached")) {
            //     GDBUtils.GDBMessage(response, "0");
            //     return;
            // }
            case 'q' when (cmd.StartsWith("qXfer:features:read:target.xml")):
                response.Append("l" + targetStub.TargetDescriptionXML);
                break;
            // if (cmd.Span.StartsWith("qXfer:threads:read")) {
            //     var xml = """
            //     <?xml version="1.0"?><threads><thread id="p1.1" core="0">single core</thread></threads>
            //     """;
            //     GDBUtils.GDBMessage(response, "l" + xml);
            //     return;
            // }
            case 'q' when (cmd.StartsWith("qRcmd")):
                TryRunCustomCommand(response, cmd.AsMemory());
                break;
            // case 'Q':
            //     if (cmd.Span.StartsWith("QNonStop")) {
            //         GDBUtils.GDBMessage(response, "OK");
            //         return;
            //     }
            //     break;
            // case 'v':
            //     if (cmd.Span.SequenceEqual("vCont?")) {
            //         GDBUtils.GDBMessage(response, "vCont;c;C;s;S;t");
            //         return;
            //     }
            //     if (cmd.Span.StartsWith("vCont;c")) {
            //         canNotifyBreak = true;
            //         TryResume(null, null);
            //         GDBUtils.GDBMessage(response, "OK");
            //         return;
            //     }
            //     if (cmd.Span.StartsWith("vCont;t")) {
            //         targetStub.HandleCtrlC();
            //         GDBUtils.GDBMessage(response, "OK");
            //         return;
            //     }
            // if (cmd.Span.StartsWith("vStopped")) {
            //     //GDBUtils.GDBMessage(response, "T02thread:p1.1;core:0;");
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
            // }
            // if (cmd.Span.StartsWith("vKill")) {
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
            // }
            // break;
            // case 'c':
            //     if (TryExecuteCommand_C(cmd.AsMemory())) {
            //         break;
            //     }
            //     break;
            case 'g':
                if (TryReadAllRegisters(response, cmd)) {
                    break;
                }
                break;
            case 'G':
                if (TryWriteAllRegisters(response, cmd)) {
                    break;
                }
                break;
            case 'm':
                if (TryReadMemory(response, cmd.AsMemory())) {
                    break;
                }
                break;
            case 'M':
                if (TryWriteMemory(response, cmd.AsMemory())) {
                    break;
                } else {
                    response.Append("E00");
                    break;
                }
            case 'Z':
                if (TryAddBreakPoint(response, cmd.AsMemory())) {
                    return;
                }
                break;
            case 'z':
                if (TryRemoveBreakPoint(response, cmd.AsMemory())) {
                    return;
                }
                break;
            // case 'T':
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
            // case 'H':
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
            default:
                break;
        }
    }

    async ValueTask ReadLoop(CancellationToken token) {
        var reader = this.streamReader;
        var rxBuffer = new char[rxBufferSize];
        var receivedMessageBuilder = new StringBuilder();
        var sendMessageBuilder = new StringBuilder();
        while (true) {
            token.ThrowIfCancellationRequested();
            var readLength = await reader.ReadAsync(rxBuffer, token);
            if (readLength <= 0) {
                break;
            }
            // TODO 消すこと
            //Console.WriteLine($"<< {new string(rxBuffer.AsSpan(0, readLength))}");
            for (int i = 0; i < readLength; i++) {
                sendMessageBuilder.Clear();
                receivedMessageBuilder.Clear();
                await StepGdbPacket(new GdbPacketInput(GdbPacketInputKind.OnReceivedChar, string.Empty, rxBuffer[i]), token);
            }
        }
    }

    public async ValueTask MessageLoop(CancellationToken token) {
        void OnBreak() {
            logger.LogDebug("break");
            runOn(this, self => ((GdbPacketAnalizerCStubLike?)self)!.StepGdbStubStateMachine(new GdbStubStateMachineInput(GdbStubStateMachineInputKind.OnCpuBreak, string.Empty), CancellationToken.None));
        };
        try {
            targetStub.OnBreak += OnBreak;
            await ReadLoop(token);
        } finally {
            targetStub.OnBreak -= OnBreak;
        }
    }
}
