using System.Buffers;
using System.Threading.Channels;
using Pidgin;

namespace GdbStubDotnet;

public sealed class StubServerBuilder {
    readonly List<Parser<byte, IDispatchedCommand>> entries = [];
    ITransport? transport;
    Action? onInterrupt;
    Action? onDisconnect;
    Action<StubFault>? onError;
    Action<SupportedCommand, ResponseWriter<SyncResponse>>? supportedHandler;
    bool detailedErrors;

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ResponseWriter<SyncResponse>> handler) {
        entries.Add(WrapSync(parser, handler));
        return this;
    }

    /// <summary>
    /// qSupported(Commands.Supported)専用の拡張点(Arcavirt-580)。通常の
    /// Map(Commands.Supported, ...) は先勝ちOneOfで組込み既定を丸ごと
    /// 上書きするため、独自の機能一覧を返すと組込みの "QNonStop+" 広告が
    /// 失われ、実gdbがnon-stopを有効化しなくなる。MapSupported はハンドラの
    /// 出力をFW側が合成する: ハンドラが書いたテキストに既に "QNonStop+"
    /// トークンが含まれていればそのまま、含まれていなければ ";QNonStop+"
    /// を末尾へ補って応答する(qSupportedだけがFWと利用者の両方が機能を
    /// 持ち寄る「合成」対象であるという性質を反映した専用APIであり、
    /// 他のコマンドのような単純な先勝ち上書きにはしない)。
    /// </summary>
    public StubServerBuilder MapSupported(Action<SupportedCommand, ResponseWriter<SyncResponse>> handler) {
        supportedHandler = handler;
        return this;
    }

    public StubServerBuilder Map<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ExecutionResponder> handler) {
        entries.Add(Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new ExecDispatchedCommand<TCmd>(cmd, handler)));
        return this;
    }

    /// <summary>
    /// 生 0x03 または vCtrlC パケットを受信したときに呼ばれるハンドラを登録する(§4.6)。
    /// 直接の応答は発生しない。実行中の ExecutionResponder が停止検知後に
    /// stop reply(SIGINT 相当)を返す想定(利用者側の resume 実装が対応する)。
    /// </summary>
    public StubServerBuilder OnInterrupt(Action handler) {
        onInterrupt = handler;
        return this;
    }

    /// <summary>
    /// TCP切断時に呼ばれるハンドラを登録する(§4.6)。応答は発生しない。
    /// D(detach)コマンドとは別の経路。
    /// </summary>
    public StubServerBuilder OnDisconnect(Action handler) {
        onDisconnect = handler;
        return this;
    }

    /// <summary>
    /// ハンドラ内で発生した予期しない例外の診断通知先を登録する(§7.2)。
    /// GDBへは安全なE NN応答が自動送出され、例外自体はこちらへのみ通知
    /// される(ILogger依存なし)。
    /// </summary>
    public StubServerBuilder OnError(Action<StubFault> handler) {
        onError = handler;
        return this;
    }

    /// <summary>
    /// §7.3。true にすると、Detail が設定されたエラー応答は E.&lt;text&gt;
    /// (人間可読)として送出される。既定は false(E NN のみ)。
    /// </summary>
    public StubServerBuilder EnableDetailedErrors(bool on = true) {
        detailedErrors = on;
        return this;
    }

    public StubServerBuilder UseTransport(ITransport transport) {
        this.transport = transport;
        return this;
    }

    public StubServer Build() {
        if (transport is null) {
            throw new InvalidOperationException("UseTransport が呼ばれていません。");
        }
        var execChannel = Channel.CreateBounded<ExecOutcome>(1);
        var notifyChannel = Channel.CreateBounded<INotification>(1);
        var notificationQueue = new NotificationQueue(notifyChannel.Writer);
        var coordinator = new ExecutionCoordinator(execChannel.Writer, notificationQueue, detailedErrors, onError);

        // 組込み既定コマンド(QNonStop/vStopped/qSupportedへのQNonStop+広告/
        // H(SetThread))は利用者の Map 登録より後ろに追加する。Dispatcher の
        // OneOf は先勝ちでマッチするため、利用者が同じコマンドを Map で
        // 上書きすればそちらが優先される(§5.1「Map による既定登録ハンドラ
        // （上書き可）」、H は §6.6 で明示的に上書き可能とされている)。
        // QNonStop/vStopped はプロトコル機構そのもの(利用者が独自実装する
        // 対象ではない)であり、上書きされる想定はない。
        List<Parser<byte, IDispatchedCommand>> allEntries = [
            .. entries,
            BuiltInQNonStop(coordinator),
            BuiltInVStopped(notificationQueue),
            BuiltInSupported(coordinator, supportedHandler),
            BuiltInSetThread(coordinator),
        ];
        var dispatcher = new Dispatcher(allEntries, coordinator);
        return new StubServer(transport, dispatcher, execChannel.Reader, notifyChannel.Reader, coordinator, onDisconnect, onInterrupt);
    }

    /// <summary>
    /// 同期コマンドの Pidgin パーサ+ハンドラを IDispatchedCommand へ包む
    /// 共通処理。公開 Map(同期版)と組込み既定コマンド(QNonStop/vStopped/
    /// qSupported/H)の登録が同じ包み方(Try+End+SyncDispatchedCommand化)を
    /// 必要とするため、ここへ一本化している。
    /// </summary>
    static Parser<byte, IDispatchedCommand> WrapSync<TCmd>(Parser<byte, TCmd> parser, Action<TCmd, ResponseWriter<SyncResponse>> handler) {
        return Parser.Try(parser.Before(Parser<byte>.End)).Select(cmd => (IDispatchedCommand)new SyncDispatchedCommand<TCmd>(cmd, handler));
    }

    readonly record struct QNonStopCommand(bool Enable);

    static readonly Parser<byte, QNonStopCommand> qNonStopWire =
        Parser<byte>.Sequence("QNonStop:"u8.ToArray())
            .Then(
                Parser<byte>.Token((byte)'0').ThenReturn(false)
                    .Or(Parser<byte>.Token((byte)'1').ThenReturn(true)),
                static (_, enable) => new QNonStopCommand(enable));

    static Parser<byte, IDispatchedCommand> BuiltInQNonStop(ExecutionCoordinator coordinator) {
        return WrapSync(qNonStopWire, (c, res) => {
            coordinator.SetMode(c.Enable ? ResumeMode.NonStop : ResumeMode.AllStop);
            res.Ok();
        });
    }

    readonly record struct VStoppedCommand;

    static Parser<byte, IDispatchedCommand> BuiltInVStopped(NotificationQueue notificationQueue) {
        return WrapSync(Parser<byte>.Sequence("vStopped"u8.ToArray()).ThenReturn(default(VStoppedCommand)), (_, res) => {
            INotification? next = notificationQueue.DrainVStopped();
            if (next is null) {
                res.Ok();
                return;
            }
            var buffer = new ArrayBufferWriter<byte>();
            next.WriteTo(buffer);
            res.Text(buffer.WrittenSpan);
        });
    }

    /// <summary>
    /// userHandler が未登録(MapSupportedが呼ばれていない)なら "QNonStop+"
    /// のみを返す(従来どおり)。登録されていれば、いったんスクラッチ
    /// バッファへ書かせてから "QNonStop+" トークンの有無を確認し、
    /// 無ければ末尾へ補ってから応答する(Arcavirt-580)。
    /// </summary>
    static Parser<byte, IDispatchedCommand> BuiltInSupported(ExecutionCoordinator coordinator, Action<SupportedCommand, ResponseWriter<SyncResponse>>? userHandler) {
        return WrapSync(Commands.Supported, (cmd, res) => {
            if (userHandler is null) {
                res.Text("QNonStop+"u8);
                return;
            }
            var scratch = new ArrayBufferWriter<byte>();
            userHandler(cmd, new ResponseWriter<SyncResponse>(scratch, coordinator.DetailedErrors));
            ReadOnlySpan<byte> userText = scratch.WrittenSpan;
            if (userText.Length == 0) {
                res.Text("QNonStop+"u8);
                return;
            }
            if (ContainsFeatureToken(userText, "QNonStop+"u8)) {
                res.Text(userText);
                return;
            }
            // userTextが既に ';' で終わっている場合は区切りを重ねない
            // (例: "multiprocess+;" + "QNonStop+" であって
            // "multiprocess+;;QNonStop+" にはしない)。
            bool needsSeparator = userText[^1] != (byte)';';
            byte[] combined = new byte[userText.Length + (needsSeparator ? 1 : 0) + "QNonStop+"u8.Length];
            userText.CopyTo(combined);
            int tail = userText.Length;
            if (needsSeparator) {
                combined[tail] = (byte)';';
                tail += 1;
            }
            "QNonStop+"u8.CopyTo(combined.AsSpan(tail));
            res.Text(combined);
        });
    }

    /// <summary>
    /// ';' 区切りの機能一覧テキストの中に token と完全一致するトークンが
    /// あるかを判定する(部分文字列一致による誤検出を避ける)。末尾が ';'
    /// で終わる入力(例: "multiprocess+;")でも空トークンをスキップする
    /// ため誤って二重の ';' を生成しない。
    /// </summary>
    static bool ContainsFeatureToken(ReadOnlySpan<byte> text, ReadOnlySpan<byte> token) {
        int start = 0;
        for (int i = 0; i <= text.Length; i++) {
            if (i == text.Length || text[i] == (byte)';') {
                ReadOnlySpan<byte> candidate = text[start..i];
                if (candidate.Length > 0 && candidate.SequenceEqual(token)) {
                    return true;
                }
                start = i + 1;
            }
        }
        return false;
    }

    static Parser<byte, IDispatchedCommand> BuiltInSetThread(ExecutionCoordinator coordinator) {
        return WrapSync(Commands.SetThread, (c, res) => {
            coordinator.SetCurrentThread(c.Thread);
            res.Ok();
        });
    }
}