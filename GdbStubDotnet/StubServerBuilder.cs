using System.Buffers;
using System.Threading.Channels;
using Pidgin;

namespace GdbStubDotnet;

public sealed class StubServerBuilder {
    private readonly List<Parser<byte, IDispatchedCommand>> _entries = [];
    private ITransport? _transport;
    private Action? _onInterrupt;
    private Action? _onDisconnect;
    private Action<StubFault>? _onError;
    private bool _detailedErrors;

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ResponseWriter<SyncResponse>> handler) {
        _entries.Add(Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<TCmd>(cmd, handler)));
        return this;
    }

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ExecutionResponder> handler) {
        _entries.Add(Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new ExecDispatchedCommand<TCmd>(cmd, handler)));
        return this;
    }

    /// <summary>
    /// 生 0x03 または vCtrlC パケットを受信したときに呼ばれるハンドラを登録する(§4.6)。
    /// 直接の応答は発生しない。実行中の ExecutionResponder が停止検知後に
    /// stop reply(SIGINT 相当)を返す想定(利用者側の resume 実装が対応する)。
    /// </summary>
    public StubServerBuilder OnInterrupt(Action handler) {
        _onInterrupt = handler;
        return this;
    }

    /// <summary>
    /// TCP切断時に呼ばれるハンドラを登録する(§4.6)。応答は発生しない。
    /// D(detach)コマンドとは別の経路。
    /// </summary>
    public StubServerBuilder OnDisconnect(Action handler) {
        _onDisconnect = handler;
        return this;
    }

    /// <summary>
    /// ハンドラ内で発生した予期しない例外の診断通知先を登録する(§7.2)。
    /// GDBへは安全なE NN応答が自動送出され、例外自体はこちらへのみ通知
    /// される(ILogger依存なし)。
    /// </summary>
    public StubServerBuilder OnError(Action<StubFault> handler) {
        _onError = handler;
        return this;
    }

    /// <summary>
    /// §7.3。true にすると、Detail が設定されたエラー応答は E.&lt;text&gt;
    /// (人間可読)として送出される。既定は false(E NN のみ)。
    /// </summary>
    public StubServerBuilder EnableDetailedErrors(bool on = true) {
        _detailedErrors = on;
        return this;
    }

    public StubServerBuilder UseTransport(ITransport transport) {
        _transport = transport;
        return this;
    }

    public StubServer Build() {
        if (_transport is null) {
            throw new InvalidOperationException("UseTransport が呼ばれていません。");
        }
        var execChannel = Channel.CreateBounded<ExecOutcome>(1);
        var notifyChannel = Channel.CreateBounded<INotification>(1);
        var notificationQueue = new NotificationQueue(notifyChannel.Writer);
        var coordinator = new ExecutionCoordinator(execChannel.Writer, notificationQueue, _detailedErrors, _onError);

        // 組込み既定コマンド(QNonStop/vStopped/qSupportedへのQNonStop+広告/
        // H(SetThread))は利用者の Map 登録より後ろに追加する。Dispatcher の
        // OneOf は先勝ちでマッチするため、利用者が同じコマンドを Map で
        // 上書きすればそちらが優先される(§5.1「Map による既定登録ハンドラ
        // （上書き可）」、H は §6.6 で明示的に上書き可能とされている)。
        // QNonStop/vStopped はプロトコル機構そのもの(利用者が独自実装する
        // 対象ではない)であり、上書きされる想定はない。
        List<Parser<byte, IDispatchedCommand>> allEntries = [
            .. _entries,
            BuiltInQNonStop(coordinator),
            BuiltInVStopped(notificationQueue),
            BuiltInSupported(),
            BuiltInSetThread(coordinator),
        ];
        var dispatcher = new Dispatcher(allEntries, coordinator);
        return new StubServer(_transport, dispatcher, execChannel.Reader, notifyChannel.Reader, _detailedErrors, _onDisconnect, _onInterrupt);
    }

    private readonly record struct QNonStopCommand(bool Enable);

    private static readonly Parser<byte, QNonStopCommand> QNonStopWire =
        Parser<byte>.Sequence("QNonStop:"u8.ToArray())
            .Then(
                Parser<byte>.Token((byte)'0').ThenReturn(false)
                    .Or(Parser<byte>.Token((byte)'1').ThenReturn(true)),
                static (_, enable) => new QNonStopCommand(enable));

    private static Parser<byte, IDispatchedCommand> BuiltInQNonStop(ExecutionCoordinator coordinator) {
        return Parser.Try(QNonStopWire.Before(Parser<byte>.End))
            .Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<QNonStopCommand>(cmd, (c, res) => {
                coordinator.SetMode(c.Enable ? ResumeMode.NonStop : ResumeMode.AllStop);
                res.Ok();
            }));
    }

    private readonly record struct VStoppedCommand;

    private static Parser<byte, IDispatchedCommand> BuiltInVStopped(NotificationQueue notificationQueue) {
        return Parser.Try(Parser<byte>.Sequence("vStopped"u8.ToArray()).Before(Parser<byte>.End))
            .ThenReturn(default(VStoppedCommand))
            .Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<VStoppedCommand>(cmd, (_, res) => {
                INotification? next = notificationQueue.DrainVStopped();
                if (next is null) {
                    res.Ok();
                    return;
                }
                var buffer = new ArrayBufferWriter<byte>();
                next.WriteTo(buffer);
                res.Text(buffer.WrittenSpan);
            }));
    }

    private static Parser<byte, IDispatchedCommand> BuiltInSupported() {
        return Parser.Try(Commands.Supported.Before(Parser<byte>.End))
            .Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<SupportedCommand>(cmd, static (_, res) => res.Text("QNonStop+"u8)));
    }

    private static Parser<byte, IDispatchedCommand> BuiltInSetThread(ExecutionCoordinator coordinator) {
        return Parser.Try(Commands.SetThread.Before(Parser<byte>.End))
            .Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<SetThreadCommand>(cmd, (c, res) => {
                coordinator.SetCurrentThread(c.Thread);
                res.Ok();
            }));
    }
}