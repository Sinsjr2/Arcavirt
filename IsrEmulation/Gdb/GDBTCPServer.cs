using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Gdb.Thread.SingleThread;
using Microsoft.Extensions.Logging;

namespace Gdb;

public class GDBTCPServer {
    private readonly TcpListener socketServer;

    readonly IGdbStub targetStub;

    readonly ILogger logger;

    public int Port { get; }

    Action<object?, Func<object?, ValueTask>> runOnLoop;

    public GDBTCPServer(Action<object?, Func<object?, ValueTask>> runOnLoop, ILogger logger, int port, IGdbStub targetStub)
    {
        this.runOnLoop = runOnLoop;
        this.Port = port;
        this.socketServer = new TcpListener(IPAddress.Any, port);
        this.logger = logger;
        this.targetStub = targetStub;
    }

    public async ValueTask ConnectionLoop(CancellationToken token) {
        token.ThrowIfCancellationRequested();
        socketServer.Start();

        while (true) {
            token.ThrowIfCancellationRequested();
            try {
                var listner = await socketServer.AcceptTcpClientAsync(token);
                logger.LogInformation("GDB connected");
                runOnLoop(this, obj => ((GDBTCPServer?)obj)!.CommandAnalizeLoop(listner, token));
            } catch (Exception ex) {
                logger.LogError("GDB connection error: {message}", ex);
            }
        }
    }

    async ValueTask CommandAnalizeLoop(TcpClient listner, CancellationToken token) {
        token.ThrowIfCancellationRequested();

        using var client = listner;
        var rxBuffer = new byte[64 * 1024];
        var responseString = new StringBuilder();
        using var scopedClient = client;
        var stream = scopedClient.GetStream();
        var buf = new ArrayBufferWriter<char>();
        var decodedCharBuf = new ArrayBufferWriter<char>();
        var encodedByteBuf = new ArrayBufferWriter<byte>();
        var encoding = Encoding.UTF8;
        var workingBuf = new StringBuilder();
        while (true) {
            try {
                var bytesRead = await stream.ReadAsync(rxBuffer, token);
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
                var consumedLength = FeedData(buf.WrittenSpan, responseString, workingBuf);
                GDBUtils.MoveAhead(buf, consumedLength);
                if (0 < responseString.Length) {
                    encodedByteBuf.Clear();
                    foreach (var chunk in responseString.GetChunks()) {
                        encoding.GetBytes(chunk.Span, encodedByteBuf);
                    }
                    await stream.WriteAsync(encodedByteBuf.WrittenMemory, token);
                }
            } catch (Exception ex) {
                this.RemoveConnection(/*connection*/);
                logger.LogError("GDB socket error: {message}", ex);
                break;
            }
        }
    }

    int FeedData(ReadOnlySpan<char> buf, StringBuilder response, StringBuilder workingBuf) {
        var initialBuf = buf;
        bool shouldLoop = true;
        while (shouldLoop) {
            workingBuf.Clear();
            // Ctrl+C (ASCII 3)
            if (0 < buf.Length && buf[0] == 3) { 
                logger.LogInformation("BREAK");
                GDBUtils.GDBMessage(response, STOP_REPLY_SIGINT);
                buf = buf[1..];
            }
            var parseResult = GDBUtils.ParseGDBMessage(buf);
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

    void OnBreakpoint() {
        try {
            //onResponse(GDBUtils.GDBMessage(Constants.STOP_REPLY_TRAP));
        } catch (Exception) {
            RemoveConnection(/*this*/);
        }
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

    bool TryReadMemory(StringBuilder response, ReadOnlySpan<char> cmd, StringBuilder workingBuf) {
        var commaPos = cmd.IndexOf(',');
        if (commaPos <= 0 || !(commaPos + 1 < cmd.Length)) {
            return false;
        }
        var addressArea = cmd[1..commaPos];
        var lengthArea = cmd[(commaPos + 1)..];
        if (!GDBUtils.TryDecoeHexUIntegerBE(addressArea, out var address, out var addressAreaLength) ||
            addressAreaLength != addressArea.Length ||
            !GDBUtils.TryDecoeHexUIntegerBE(lengthArea, out var length, out var lengthAreaLength) ||
            lengthAreaLength != lengthArea.Length) {
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

    bool TryWriteMemory(StringBuilder response, ReadOnlySpan<char> cmd) {
        var commaPos = cmd.IndexOf(',');
        if (commaPos <= 0) {
            return false;
        }
        var afterComma = cmd[commaPos..];
        var colonPos = afterComma.IndexOf(':');
        if (colonPos <= 0 || !(colonPos + 1 < afterComma.Length)) {
            return false;
        }
        var addressArea = cmd[1..commaPos];
        var lengthArea = afterComma[1..colonPos];
        if (!GDBUtils.TryDecoeHexUIntegerBE(addressArea, out var address, out var addressAreaLength) ||
            addressAreaLength != addressArea.Length ||
            !GDBUtils.TryDecoeHexUIntegerBE(lengthArea, out var length, out var lengthAreaLength) ||
            lengthAreaLength != lengthArea.Length) {
            return false;
        }
        ulong dataLength = length * 2;
        var dataArea = afterComma[(colonPos + 1) ..];
        if (!(dataLength <= (ulong)dataArea.Length)) {
            return false;
        }
        switch (targetStub.ThreadObject) {
            case IGDBSingleThread singleThread:
                if (!singleThread.WriteMemory(address, dataArea[..(int)dataLength])) {
                    return false;
                }
                GDBUtils.GDBMessage(response, "OK");
                return true;
        }
        return false;
    }

    bool TryAddBreakPoint(StringBuilder response, ReadOnlySpan<char> cmd) {
        return false;
    }

    void ProcessGDBMessage(StringBuilder response, ReadOnlySpan<char> cmd, StringBuilder workingBuf) {
        //var rp2040 = this.Target.RP2040;
        //var core = rp2040.Core;

        if (cmd.SequenceEqual("Hg0")) {
            GDBUtils.GDBMessage(response, "OK");
            return;
        }

        if (cmd.StartsWith("Hgp0")) {
            GDBUtils.GDBMessage(response, "E31");
            return;
        }

        switch (cmd[0]) {
            case '!':
                GDBUtils.GDBMessage(response, "OK");
                return;
            case '?':
                GDBUtils.GDBMessage(response, "OK");
                return;
            case 'q':
                if (cmd.StartsWith("qSupported:")) {
                    GDBUtils.GDBMessage(response, "PacketSize=4000;qXfer:memory-map:read+;qXfer:features:read+;qXfer:threads:read+;vContSupported+;multiprocess+;QNonStop+;swbreak+;hwbreak+");
                    return;
                }
                if (cmd.StartsWith("qTStatus")) {
                }
                if (cmd.StartsWith("qAttached")) {
                    GDBUtils.GDBMessage(response, "0");
                    return;
                }
                if (cmd.StartsWith("qXfer:features:read:target.xml")) {
                    GDBUtils.GDBMessage(response, "l" + targetStub.TargetDescriptionXML);
                    return;
                }
                if (cmd.StartsWith("qXfer:threads:read")) {
                    var xml = """
                    <?xml version="1.0"?><threads><thread id="p1.1" core="0">single core</thread></threads>
                    """;
                    GDBUtils.GDBMessage(response, "l" + xml);
                    return;
                }
                if (cmd.StartsWith("qRcmd")) {
                    GDBUtils.GDBMessage(response, "OK");
                    return;
                }
                break;
            case 'Q':
                if (cmd.StartsWith("QNonStop")) {
                    GDBUtils.GDBMessage(response, "OK");
                    return;
                }
                break;
            case 'v':
                if (cmd.SequenceEqual("vCont?")) {
                    GDBUtils.GDBMessage(response, "vCont;c;C;s;S;t");
                    return;
                }
                if (cmd.StartsWith("vCont;c")) {
                    // if (!this.Target.Executing) {
                    //     this.Target.Execute();
                    // }
                    return;
                }
                if (cmd.StartsWith("vCont;t")) {
                    GDBUtils.GDBMessage(response, "OK");
                    return;
                }
                if (cmd.StartsWith("vStopped")) {
                    GDBUtils.GDBMessage(response, "OK");
                    return;
                }
                break;
            case 'c':
                // if (!this.Target.Executing) {
                //     this.Target.Execute();
                // }
                GDBUtils.GDBMessage(response, "OK");
                return;
            case 'g':
                if (TryReadAllRegisters(response, cmd, workingBuf)) {
                    return;
                }
                break;
            case 'G':
                if (TryWriteAllRegisters(response, cmd)) {
                    return;
                }
                break;
            case 'p':
                // Handle 'p' case here
            case 'P':
                // Handle 'P' case here
                break;
            case 'm':
                if (TryReadMemory(response, cmd, workingBuf)) {
                    return;
                }
                break;
            case 'M':
                if (TryWriteMemory(response, cmd)) {
                    return;
                }
                break;
            case 'Z':
                //targetStub.BreakpointObject?.
                break;
            case 'z':
                break;
        }

        GDBUtils.GDBMessage(response, "");
        return;
    }

    void AddConnection(/*GDBConnection connection*/) {
        //var rp2040 = this.Target.RP2040;
        //this.connections.Add(connection);
        // rp2040.OnBreak = () => {
        //     this.Target.Stop();
        //     rp2040.Core.PC -= rp2040.Core.BreakRewind;
        //     foreach (var conn in this.connections) {
        //         conn.OnBreakpoint();
        //     }
        // };
    }

    void RemoveConnection(/*GDBConnection connection*/) {
        //this.connections.Remove(connection);
    }
}
