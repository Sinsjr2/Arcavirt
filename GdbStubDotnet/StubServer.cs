using System.Buffers;
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
    private readonly ChannelReader<INotification> _notifyReader;
    private readonly bool _detailedErrors;
    private readonly Framer _framer = new();
    private readonly Action? _onDisconnect;
    private readonly Action? _onInterrupt;
    private Task? _loopTask;

    internal StubServer(ITransport transport, Dispatcher dispatcher, ChannelReader<ExecOutcome> execReader, ChannelReader<INotification> notifyReader, bool detailedErrors, Action? onDisconnect, Action? onInterrupt) {
        _transport = transport;
        _dispatcher = dispatcher;
        _execReader = execReader;
        _notifyReader = notifyReader;
        _detailedErrors = detailedErrors;
        _onDisconnect = onDisconnect;
        _onInterrupt = onInterrupt;
    }

    public void Start() {
        _loopTask = RunLoopAsync();
    }

    /// <summary>
    /// transport の読み取りと、実行中コマンドの stop 待ち・non-stop通知の
    /// 送出待ちを同時に待ち受ける(§4.6: exec 実行中でも 0x03/vCtrlC による
    /// 割込を検知できる必要がある)。stop 待ちは接続共有の ExecOutcome
    /// チャネル(_execReader、all-stop用、Arcavirt-o3e.10.1)、通知待ちは
    /// 接続共有の INotification プッシュチャネル(_notifyReader、non-stop用、
    /// Arcavirt-o3e.10.3)をそれぞれ常に読み続ける形にした。いずれも
    /// outstanding でない間は単に読み待ちのまま滞留するだけなので害はない。
    /// 現行スコープは単一スレッド・単一 outstanding resume が前提であり、
    /// 複数スレッドの同時 resume・相関は Arcavirt-o3e.10.4 で作り直しが必要
    /// になる。readTask/execTask/notifyTask は Task.WhenAny で負けた側を
    /// 次周回に持ち越す(同一 reader に対する二重の読み取り待ちを作っては
    /// ならないため、勝った側だけを都度作り直す)。
    /// transport切断(読み取り0バイトまたは例外)時は OnDisconnect を呼ぶ
    /// (Arcavirt-o3e.18、§4.6)。
    /// </summary>
    private async Task RunLoopAsync() {
        byte[] buffer = new byte[4096];
        Task<int>? readTask = null;
        Task<ExecOutcome>? execTask = null;
        Task<INotification>? notifyTask = null;

        while (true) {
            readTask ??= _transport.ReadAsync(buffer).AsTask();
            execTask ??= _execReader.ReadAsync().AsTask();
            notifyTask ??= _notifyReader.ReadAsync().AsTask();

            await Task.WhenAny(readTask, execTask, notifyTask);

            if (execTask is not null && execTask.IsCompleted) {
                ExecOutcome outcome = await execTask;
                execTask = null;
                byte[] replyPayload = outcome.IsReject
                    ? HexUtil.EncodeError(outcome.RejectError, _detailedErrors)
                    : EncodeStopReply(outcome.Stop);
                await _transport.WriteAsync(Framer.Encode(replyPayload));
                continue;
            }

            if (notifyTask is not null && notifyTask.IsCompleted) {
                INotification notification = await notifyTask;
                notifyTask = null;
                var notifyBuffer = new ArrayBufferWriter<byte>();
                // 現状 %Stop のみ実装(§4.5)。他の通知種別が増えたら種別ごとの
                // プレフィックスを一般化する(Arcavirt-o3e.10.3のスコープ外)。
                notifyBuffer.Write("Stop:"u8);
                notification.WriteTo(notifyBuffer);
                await _transport.WriteAsync(Framer.EncodeNotification(notifyBuffer.WrittenSpan));
                continue;
            }

            int n;
            try {
                n = await readTask;
            } catch {
                _onDisconnect?.Invoke();
                return;
            }
            readTask = null;
            if (n == 0) {
                _onDisconnect?.Invoke();
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