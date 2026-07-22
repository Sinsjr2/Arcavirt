using System.Buffers;
using System.Threading.Channels;

namespace GdbStubDotnet;

/// <summary>
/// non-stop の非同期通知の抽象(§10クラス図)。WriteTo は通知内容を
/// stop-reply と同形式(例: "T05...")で書き込む。'%'接頭辞や種別プレフィックス
/// ("Stop:"等)の付与は呼び出し側(StubServer の即時送出/vStoppedハンドラ)の
/// 責務であり、WriteTo 自体には含めない。
/// </summary>
interface INotification {
    void WriteTo(IBufferWriter<byte> writer);
}

readonly record struct StopNotification(StopEvent Stop) : INotification {
    public void WriteTo(IBufferWriter<byte> writer) {
        HexUtil.WriteStopReplyText(writer, Stop.SignalOrExit, Stop.Thread);
    }
}

/// <summary>
/// non-stop の非同期通知(%Stop等)を仲介する内部コンポーネント(§4.5)。
/// 「二段ack」の内容ack側を担う: 通知は最大1件だけ「in-flight」(未ackの
/// 送出済み)状態を持てる。in-flightが無ければ Enqueue が直ちに push
/// チャネルへ書き込み、StubServer が %通知として即時送出する。既に
/// in-flightなら、次の vStopped 受信(DrainVStopped)まで滞留させる。
/// Enqueue/DrainVStopped は _gate で排他し、「保留無し」の判定から
/// in-flight解除までを1つの臨界区間にすることで、両者が競合する
/// タイミング依存のすり抜け(Arcavirt-o3e.10.3で既知の限界として
/// 保留していた極小窓)を構造的に排除する(Arcavirt-o3e.10.4で解消)。
/// </summary>
sealed class NotificationQueue {
    readonly object gate = new();
    readonly Queue<INotification> pending = new();
    readonly ChannelWriter<INotification> pushWriter;
    bool inFlight;

    internal NotificationQueue(ChannelWriter<INotification> pushWriter) {
        this.pushWriter = pushWriter;
    }

    /// <summary>
    /// 通知をキューへ追加する。in-flightな通知が無ければ、その場で
    /// in-flight状態にして push チャネルへ書き込む。既に in-flight なら
    /// キューに滞留させるだけにする。
    /// </summary>
    internal void Enqueue(INotification notification) {
        INotification? toPush = null;
        lock (gate) {
            if (inFlight) {
                pending.Enqueue(notification);
            } else {
                inFlight = true;
                toPush = notification;
            }
        }
        if (toPush is not null) {
            pushWriter.TryWrite(toPush);
        }
    }

    /// <summary>
    /// vStopped 受信時に呼ぶ。次の保留通知があればそれを取り出して返す
    /// (in-flightを維持)。無ければ in-flight を解除して null を返す
    /// (呼び出し側が OK を書く、§4.5の内容ack完了)。
    /// </summary>
    internal INotification? DrainVStopped() {
        lock (gate) {
            if (pending.TryDequeue(out INotification? next)) {
                return next;
            }
            inFlight = false;
            return null;
        }
    }
}