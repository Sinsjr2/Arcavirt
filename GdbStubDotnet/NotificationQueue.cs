using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;

namespace GdbStubDotnet;

/// <summary>
/// non-stop の非同期通知の抽象(§10クラス図)。WriteTo は通知内容を
/// stop-reply と同形式(例: "T05...")で書き込む。'%'接頭辞や種別プレフィックス
/// ("Stop:"等)の付与は呼び出し側(StubServer の即時送出/vStoppedハンドラ)の
/// 責務であり、WriteTo 自体には含めない。
/// </summary>
internal interface INotification {
    void WriteTo(IBufferWriter<byte> writer);
}

internal readonly record struct StopNotification(StopEvent Stop) : INotification {
    public void WriteTo(IBufferWriter<byte> writer) {
        byte[] bytes = Encoding.ASCII.GetBytes("T" + Stop.SignalOrExit.ToString("x2"));
        writer.Write(bytes);
    }
}

/// <summary>
/// non-stop の非同期通知(%Stop等)を仲介する内部コンポーネント(§4.5)。
/// 「二段ack」の内容ack側を担う: 通知は最大1件だけ「in-flight」(未ackの
/// 送出済み)状態を持てる。in-flightが無ければ Enqueue が直ちに push
/// チャネルへ書き込み、StubServer が %通知として即時送出する。既に
/// in-flightなら、次の vStopped 受信(DrainVStopped)まで滞留させる。
/// 現行スコープ(Arcavirt-o3e.10.3)は単一スレッドを前提とする簡易実装であり、
/// DrainVStopped が「保留無し」と判定してからin-flightを解除するまでの
/// 極小窓での Enqueue との競合は考慮していない(複数スレッド相関は
/// Arcavirt-o3e.10.4で再検討する)。
/// </summary>
internal sealed class NotificationQueue {
    private readonly ConcurrentQueue<INotification> _pending = new();
    private readonly ChannelWriter<INotification> _pushWriter;
    private int _inFlight;

    internal NotificationQueue(ChannelWriter<INotification> pushWriter) {
        _pushWriter = pushWriter;
    }

    /// <summary>
    /// 通知をキューへ追加する。in-flightな通知が無ければ、この呼び出しで
    /// キューの先頭(=今追加したもの)を即座に取り出して push チャネルへ
    /// 書き込み、in-flight状態にする。
    /// </summary>
    internal void Enqueue(INotification notification) {
        _pending.Enqueue(notification);
        if (Interlocked.CompareExchange(ref _inFlight, 1, 0) == 0 && _pending.TryDequeue(out INotification? toPush)) {
            _pushWriter.TryWrite(toPush);
        }
    }

    /// <summary>
    /// vStopped 受信時に呼ぶ。次の保留通知があればそれを取り出して返す
    /// (in-flightを維持)。無ければ in-flight を解除して null を返す
    /// (呼び出し側が OK を書く、§4.5の内容ack完了)。
    /// </summary>
    internal INotification? DrainVStopped() {
        if (_pending.TryDequeue(out INotification? next)) {
            return next;
        }
        Volatile.Write(ref _inFlight, 0);
        return null;
    }
}