using System.Text;
using System.Threading.Channels;

namespace GdbStubDotnet;

public sealed class StubServer : IDisposable {
    private static readonly byte[] AckBytes = [(byte)'+'];
    private static readonly byte[] NakBytes = [(byte)'-'];
    private static readonly byte[] VCtrlCPayload = "vCtrlC"u8.ToArray();
    private const byte InterruptByte = 0x03;

    private readonly ITransport _transport;
    private readonly Dispatcher _dispatcher;
    private readonly ChannelReader<ExecOutcome> _execReader;
    private readonly Framer _framer = new();
    private readonly Action? _onInterrupt;
    private Task? _loopTask;

    internal StubServer(ITransport transport, Dispatcher dispatcher, ChannelReader<ExecOutcome> execReader, Action? onInterrupt) {
        _transport = transport;
        _dispatcher = dispatcher;
        _execReader = execReader;
        _onInterrupt = onInterrupt;
    }

    public void Start() {
        _loopTask = RunLoopAsync();
    }

    /// <summary>
    /// transport の読み取りと、実行中コマンドの stop 待ちを同時に待ち受ける
    /// (§4.6: exec 実行中でも 0x03/vCtrlC による割込を検知できる必要がある)。
    /// stop 待ちは接続共有の ExecOutcome チャネル(_execReader)を常に読み続ける
    /// 形にした(Arcavirt-o3e.10.1: 旧 resume 1回ごとの専用チャネルから移行)。
    /// resume が outstanding でない間もチャネルは単に読み待ちのまま滞留する
    /// だけなので害はない。all-stop は resume 1回につき outstanding な exec が
    /// 高々1つという前提(現行スコープ)のため execTask は単一。
    /// non-stop 対応(Arcavirt-o3e.10.3以降)ではこの前提自体が崩れるため、
    /// 複数の stop を相関する形へ作り直しが必要になる。
    /// readTask/execTask は Task.WhenAny で負けた側を次周回に持ち越す
    /// (transport/Channel いずれも同一 reader に対する二重の読み取り待ちを
    /// 作ってはならないため、勝った側だけを都度作り直す)。
    /// </summary>
    private async Task RunLoopAsync() {
        byte[] buffer = new byte[4096];
        Task<int>? readTask = null;
        Task<ExecOutcome>? execTask = null;

        while (true) {
            readTask ??= _transport.ReadAsync(buffer).AsTask();
            execTask ??= _execReader.ReadAsync().AsTask();

            await Task.WhenAny(readTask, execTask);

            if (execTask is not null && execTask.IsCompleted) {
                ExecOutcome outcome = await execTask;
                execTask = null;
                byte[] replyPayload = outcome.IsReject
                    ? HexUtil.EncodeError(outcome.RejectError)
                    : EncodeStopReply(outcome.Stop);
                await _transport.WriteAsync(Framer.Encode(replyPayload));
                continue;
            }

            int n;
            try {
                n = await readTask;
            } catch {
                return;
            }
            readTask = null;
            if (n == 0) {
                return;
            }

            List<byte[]> immediateWrites = [];
            ProcessChunk(buffer.AsSpan(0, n), immediateWrites);

            foreach (byte[] write in immediateWrites) {
                await _transport.WriteAsync(write);
            }
        }
    }

    /// <summary>
    /// 生 0x03(interrupt)をフレーミング層に渡さず抜き出し、残りのバイト列を
    /// Framer へ渡す。0x03 はパケットの一部ではないため、チャンク中のどこに
    /// 現れても即座に割込ハンドラを呼ぶ(§4.6)。
    /// </summary>
    private void ProcessChunk(ReadOnlySpan<byte> chunk, List<byte[]> immediateWrites) {
        int start = 0;
        for (int i = 0; i < chunk.Length; i++) {
            if (chunk[i] != InterruptByte) {
                continue;
            }
            if (i > start) {
                ProcessFramerChunk(chunk[start..i], immediateWrites);
            }
            _onInterrupt?.Invoke();
            start = i + 1;
        }
        if (start < chunk.Length) {
            ProcessFramerChunk(chunk[start..], immediateWrites);
        }
    }

    private void ProcessFramerChunk(ReadOnlySpan<byte> chunk, List<byte[]> immediateWrites) {
        _framer.ProcessBytes(chunk, evt => {
            switch (evt.Kind) {
                case FramerEventKind.Packet:
                    immediateWrites.Add(AckBytes);
                    if (evt.Payload!.AsSpan().SequenceEqual(VCtrlCPayload)) {
                        // vCtrlC は内部で interrupt 経路へ集約する(§4.6)。
                        // 直接の内容応答は返さない(停止時に exec 側が stop reply を返す)。
                        _onInterrupt?.Invoke();
                        break;
                    }
                    RouteResult? routed = _dispatcher.Route(evt.Payload!);
                    if (routed is null) {
                        immediateWrites.Add(Framer.Encode(ReadOnlySpan<byte>.Empty));
                    } else if (!routed.Value.ExecStarted) {
                        immediateWrites.Add(Framer.Encode(routed.Value.SyncResponse!));
                    }
                    // ExecStarted の場合は共有チャネル経由で RunLoopAsync が後刻応答する。
                    break;
                case FramerEventKind.ChecksumMismatch:
                    immediateWrites.Add(NakBytes);
                    break;
            }
        });
    }

    private static byte[] EncodeStopReply(StopEvent stop) {
        string text = "T" + stop.SignalOrExit.ToString("x2");
        return Encoding.ASCII.GetBytes(text);
    }

    public void Stop() {
        _transport.Close();
    }

    public void Dispose() {
        Stop();
    }
}