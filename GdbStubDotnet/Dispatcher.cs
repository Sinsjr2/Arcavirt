using System.Buffers;
using Pidgin;

namespace GdbStubDotnet;

interface IDispatchedCommand {
    /// <summary>
    /// 同期コマンドなら output にバイト列を書き込み false を返す。
    /// 実行コマンドなら、all-stop では何も書き込まず true を返す(結果は
    /// ExecutionCoordinator 経由で接続共有チャネルへ後刻届く)。non-stop
    /// では resume 開始の直後に即時 OK を書き込み false を返す(§4.3手順5)。
    /// </summary>
    bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator);
}

sealed class SyncDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ResponseWriter<SyncResponse>> handler) : IDispatchedCommand {
    /// <summary>
    /// TCmd が IThreadScoped を実装していれば、ハンドラ呼び出し
    /// 直前に H(SetThread)で選択済みの現在スレッドをThreadフィールドへ
    /// 差し替える(§6.6)。実装していないコマンドは素通しする。
    /// </summary>
    public bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator) {
        TCmd effective = command is IThreadScoped scoped ? (TCmd)scoped.WithThread(coordinator.CurrentThread) : command;
        var writer = new ResponseWriter<SyncResponse>(output, coordinator.DetailedErrors);
        handler(effective, writer);
        return false;
    }
}

sealed class ExecDispatchedCommand<TCmd>(TCmd command, Action<TCmd, ExecutionResponder> handler) : IDispatchedCommand {
    /// <summary>
    /// ハンドラが例外を投げた場合、resume は開始しなかったものとして扱い
    /// coordinator.AbortResume で token を無効化してから再送出する(呼び出し
    /// 元の Dispatcher.Route が E NN 応答・OnError通知を行う)。これにより
    /// 「BeginResumeで払い出したtokenが有効なまま残り、後刻ターゲット側の
    /// (無関係な)ReportStopが誤って受理される」事故を防ぐ。
    /// non-stopでの即時OKは、ハンドラが同期的にResponder.Rejectを呼んで
    /// いた場合は送らない(coordinator.IsPendingでtokenがまだ消費されて
    /// いないことを確認してから送る。Rejectは常にtokenをCAS消費するため、
    /// このチェックはReportStop(non-stopでは消費しない)の有無に影響
    /// されず、Reject発生の有無だけを正確に反映する)。Reject済みの場合は
    /// AllStop同様 true を返し、共有ExecOutcomeチャネル経由でE NN応答が
    /// 配送されるのに任せる。
    /// </summary>
    public bool Execute(IBufferWriter<byte> output, ExecutionCoordinator coordinator) {
        ExecutionResponder responder = coordinator.BeginResume();
        try {
            handler(command, responder);
        } catch {
            coordinator.AbortResume(responder.Token);
            throw;
        }
        if (coordinator.Mode == ResumeMode.NonStop && coordinator.IsPending(responder.Token)) {
            new ResponseWriter<SyncResponse>(output, coordinator.DetailedErrors).Ok();
            return false;
        }
        return true;
    }
}

readonly record struct RouteResult(byte[]? SyncResponse, bool ExecStarted);

sealed class Dispatcher {
    readonly Parser<byte, IDispatchedCommand> router;
    readonly ExecutionCoordinator coordinator;

    public Dispatcher(IReadOnlyList<Parser<byte, IDispatchedCommand>> entries, ExecutionCoordinator coordinator) {
        router = Parser.OneOf(entries);
        this.coordinator = coordinator;
    }

    /// <summary>
    /// ハンドラ例外は境界でcatchし、I/Oループを継続させる(§7.2)。
    /// 例外は OnError へのみ通知し、GDB へは安全な E NN 応答を返す
    /// (部分的に書き込まれていたかもしれない内容は buffer.Clear() で
    /// 破棄してから同じバッファへ書き直す)。
    /// </summary>
    public RouteResult? Route(ReadOnlySpan<byte> payload) {
        var result = router.Parse(payload);
        if (!result.Success) {
            return null;
        }
        var buffer = new ArrayBufferWriter<byte>();
        bool execStarted;
        try {
            execStarted = result.Value.Execute(buffer, coordinator);
        } catch (Exception ex) {
            coordinator.OnError?.Invoke(new StubFault(ex, result.Value.GetType().Name));
            // 部分的に書き込まれていたかもしれないバッファを破棄してから
            // 同じインスタンスを再利用する(Clear()はWrittenCountを0へ
            // 戻すのみで、新規ArrayBufferWriterの割当を避けられる)。
            buffer.Clear();
            new ResponseWriter<SyncResponse>(buffer, coordinator.DetailedErrors).Error(new RspError(1, null));
            return new RouteResult(buffer.WrittenSpan.ToArray(), false);
        }
        return execStarted
            ? new RouteResult(null, true)
            : new RouteResult(buffer.WrittenSpan.ToArray(), false);
    }
}