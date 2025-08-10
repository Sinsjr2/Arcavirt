using System.Buffers;
using System.Text;
using Gdb.BreakPoint;
using Gdb.Thread.SingleThread;
using Microsoft.Extensions.Logging;

namespace Gdb;

public class GdbPacketAnalizer {

    readonly IGdbStub targetStub;

    readonly ILogger logger;

    bool canNotifyBreak = false;
    bool breaked = false;

    public GdbPacketAnalizer(IGdbStub targetStub, ILogger logger) {
        this.targetStub = targetStub;
        this.logger = logger;
    }

    /// <summary>
    /// 指定された読み書き先からの GdbStub のコマンドに対して解析します。
    /// ループは例外が発生する以外で終了しません。
    /// </summary>
    public async ValueTask CommandAnalizeLoop(IStreamCommunication targetCommunication, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        var rxBuffer = new byte[64 * 1024];
        var responseString = new StringBuilder();
        var buf = new ArrayBufferWriter<char>();
        var decodedCharBuf = new ArrayBufferWriter<char>();
        var encodedByteBuf = new ArrayBufferWriter<byte>();
        var encoding = Encoding.UTF8;
        var workingBuf = new StringBuilder();

        // var sb = new StringBuilder("%Stop:T02thread:p1.1;core:0;#d1");
        var sb = new StringBuilder();
        GDBUtils.GDBMessage(sb, STOP_REPLY_TRAP);
        var stopReplySigint = encoding.GetBytes(sb.ToString());
        async void OnBreak() {
            logger.LogInformation("break");
            breaked = true;
            if (!canNotifyBreak) {
                return;
            }
            canNotifyBreak = false;
            if (logger.IsEnabled(LogLevel.Debug)) {
                logger.LogDebug(sb.ToString());
            }
            await targetCommunication.Write(stopReplySigint, token);
        }
        try {
            targetStub.OnBreak += OnBreak;
            while (true) {
                try {
                    var bytesRead = await targetCommunication.Read(rxBuffer, token);
                    if (bytesRead <= 0) {
                        this.RemoveConnection(/*connection*/);
                        logger.LogInformation("GDB disconnected");
                        break;
                    }
                    decodedCharBuf.Clear();
                    encoding.GetChars(rxBuffer.AsSpan(0, bytesRead), decodedCharBuf);
                    responseString.Clear();
                    decodedCharBuf.WrittenSpan.CopyTo(buf.GetSpan(decodedCharBuf.WrittenCount));
                    buf.Advance(decodedCharBuf.WrittenCount);
                    var bufLength = buf.WrittenCount;
                    var consumedLength = FeedData(buf.WrittenMemory, responseString, workingBuf);
                    GDBUtils.MoveAhead(buf, consumedLength);
                    if (0 < responseString.Length) {
                        encodedByteBuf.Clear();
                        foreach (var chunk in responseString.GetChunks()) {
                            encoding.GetBytes(chunk.Span, encodedByteBuf);
                        }
                        await targetCommunication.Write(encodedByteBuf.WrittenMemory, token);
                    }
                    if (breaked) {
                        breaked = false;
                        if (logger.IsEnabled(LogLevel.Debug)) {
                            logger.LogDebug(sb.ToString());
                        }
                        await targetCommunication.Write(stopReplySigint, token);
                    }
                } catch (Exception ex) {
                    this.RemoveConnection(/*connection*/);
                    logger.LogError("GDB error: {message}", ex);
                    break;
                }
            }
        }
        finally {
            targetStub.OnBreak -= OnBreak;
        }
    }

    int FeedData(ReadOnlyMemory<char> buf, StringBuilder response, StringBuilder workingBuf) {
        var initialBuf = buf;
        bool shouldLoop = true;
        while (shouldLoop) {
            workingBuf.Clear();
            // Ctrl+C (ASCII 3)
            if (0 < buf.Length && buf.Span[0] == 3) {
                logger.LogInformation("BREAK");
                targetStub.HandleCtrlC();
                // GDBUtils.GDBMessage(response, STOP_REPLY_SIGINT);
                buf = buf[1..];
            }
            var parseResult = GDBUtils.ParseGDBMessage(buf.Span);
            var nextBuf = buf[parseResult.NextIndex..];
            switch (parseResult.ResultKind) {
                case GDBMessageKind.InvalidChecksum:
                    logger.LogWarning($"GDB checksum error in message: {buf[parseResult.MessageArea]}");
                    response.Append('-');
                    break;
                case GDBMessageKind.HasMessage:
                    response.Append('+');
                    var cmd = buf[parseResult.ValueArea];
                    ProcessGDBMessage(response, cmd, workingBuf);
                    if (response.Length == 1) {
                        response.Clear();
                    }
                    if (logger.IsEnabled(LogLevel.Debug)) {
                        logger.LogDebug($">{buf[parseResult.MessageArea]}\n{response}");
                    }
                    break;
                case GDBMessageKind.StoreBuffer:
                    shouldLoop = false;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid GDBMessageKind : {parseResult.ResultKind}");
            }
            buf = nextBuf;
        }
        return initialBuf.Length - buf.Length;
    }

    static readonly string STOP_REPLY_SIGINT = "S02";
    static readonly string STOP_REPLY_TRAP = "S05";

    bool TryReadAllRegisters(StringBuilder response, ReadOnlySpan<char> cmd, StringBuilder workingBuf) {
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread:
                for (int i = 0; i < singleThread.NumOfRegisters; i++) {
                    if (singleThread.ReadRegister(i, workingBuf) == 0) {
                        return false;
                    }
                }
                GDBUtils.GDBMessage(response, workingBuf);
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
                    GDBUtils.GDBMessage(response, "OK");
                    return true;
                }
                break;
        }
        return false;
    }

    bool TryReadMemory(StringBuilder response, ReadOnlyMemory<char> cmd, StringBuilder workingBuf) {
        var gdbReader = new GdbMessageReader(cmd);
        if (!(gdbReader.TryReadIfExpChar('m') &&
            gdbReader.TryReadHexUIntegerBE(out var address) &&
            gdbReader.TryReadIfExpChar(',') &&
            gdbReader.TryReadHexUIntegerBE(out var length))) {
            return false;
        }
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread:
                if (!singleThread.ReadMemory(workingBuf, address, length)) {
                    return false;
                }
                GDBUtils.GDBMessage(response, workingBuf);
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
                GDBUtils.GDBMessage(response, "OK");
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
            GDBUtils.GDBMessage(response, "OK");
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
            GDBUtils.GDBMessage(response, "OK");
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

    void ProcessGDBMessage(StringBuilder response, ReadOnlyMemory<char> cmd, StringBuilder workingBuf) {
        // if (cmd.Span.SequenceEqual("Hg0")) {
        //     GDBUtils.GDBMessage(response, "OK");
        //     return;
        // }

        // if (cmd.Span.StartsWith("Hgp0")) {
        //     GDBUtils.GDBMessage(response, "E31");
        //     return;
        // }

        switch (cmd.Span[0]) {
            // case '!':
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
            case '?':
                GDBUtils.GDBMessage(response, STOP_REPLY_TRAP);
                return;
            case 'q':
                if (cmd.Span.StartsWith("qSupported:")) {
                    // GDBUtils.GDBMessage(response, "PacketSize=4000;qXfer:memory-map:read+;qXfer:features:read+;qXfer:threads:read+;vContSupported+;multiprocess+;QNonStop+;swbreak+;hwbreak+");
                    GDBUtils.GDBMessage(response, "PacketSize=4000;vContSupported+;qXfer:features:read+;");
                    return;
                }
                // if (cmd.Span.StartsWith("qTStatus")) {
                // }
                // if (cmd.Span.StartsWith("qAttached")) {
                //     GDBUtils.GDBMessage(response, "0");
                //     return;
                // }
                if (cmd.Span.StartsWith("qXfer:features:read:target.xml")) {
                    GDBUtils.GDBMessage(response, "l" + targetStub.TargetDescriptionXML);
                    return;
                }
                // if (cmd.Span.StartsWith("qXfer:threads:read")) {
                //     var xml = """
                //     <?xml version="1.0"?><threads><thread id="p1.1" core="0">single core</thread></threads>
                //     """;
                //     GDBUtils.GDBMessage(response, "l" + xml);
                //     return;
                // }
                if (cmd.Span.StartsWith("qRcmd")) {
                    TryRunCustomCommand(response, cmd);
                    return;
                }
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
            case 'c':
                canNotifyBreak = true;
                if (TryExecuteCommand_C(cmd)) {
                    return;
                }
                break;
            case 'g':
                if (TryReadAllRegisters(response, cmd.Span, workingBuf)) {
                    return;
                }
                break;
            case 'G':
                if (TryWriteAllRegisters(response, cmd.Span)) {
                    return;
                }
                break;
            case 'm':
                if (TryReadMemory(response, cmd, workingBuf)) {
                    return;
                }
                break;
            case 'M':
                if (TryWriteMemory(response, cmd)) {
                    return;
                } else {
                    GDBUtils.GDBMessage(response, "E00");
                    return;
                }
            // case 'Z':
            //     if (TryAddBreakPoint(response, cmd)) {
            //         return;
            //     }
            //     break;
            // case 'z':
            //     if (TryRemoveBreakPoint(response, cmd)) {
            //         return;
            //     }
            //     break;
            case 's':
                canNotifyBreak = true;
                if (TryStep(cmd)) {
                    return;
                }
                break;
            // case 'T':
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
            // case 'H':
            //     GDBUtils.GDBMessage(response, "OK");
            //     return;
        }

        GDBUtils.GDBMessage(response, "");
        return;
    }

    void RemoveConnection(/*GDBConnection connection*/) {
        //this.connections.Remove(connection);
    }
}
