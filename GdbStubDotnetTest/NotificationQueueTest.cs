using System.Threading.Channels;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class NotificationQueueTest {
    /// <summary>
    /// in-flightな通知が無い状態でEnqueueすると、その場でpushチャネルへ
    /// 書き込まれる(§4.5: 最初の%Stopは即時送出)ことを確認する。
    /// </summary>
    [Test]
    public void Enqueue_WhenNothingInFlight_PushesImmediately() {
        var channel = Channel.CreateBounded<INotification>(1);
        var queue = new NotificationQueue(channel.Writer);
        var notification = new StopNotification(new StopEvent(default, StopReason.Signal, 5, 0));

        queue.Enqueue(notification);

        bool pushed = channel.Reader.TryRead(out INotification? delivered);
        Assert.Multiple(() => {
            Assert.That(pushed, Is.True);
            Assert.That(delivered, Is.EqualTo(notification));
        });
    }

    /// <summary>
    /// 既にin-flightな通知がある状態で2件目をEnqueueすると、pushチャネルへは
    /// 書き込まれず(即時送出は1件のみ)、DrainVStopped で取り出せることを
    /// 確認する(§4.5: 2件目以降はvStoppedドレインで配送)。
    /// </summary>
    [Test]
    public void Enqueue_WhileAlreadyInFlight_QueuesForVStoppedDrain() {
        var channel = Channel.CreateBounded<INotification>(1);
        var queue = new NotificationQueue(channel.Writer);
        var first = new StopNotification(new StopEvent(default, StopReason.Signal, 5, 0));
        var second = new StopNotification(new StopEvent(default, StopReason.Signal, 9, 0));

        queue.Enqueue(first);
        queue.Enqueue(second);

        bool firstPushed = channel.Reader.TryRead(out INotification? pushedItem);
        bool secondAlsoPushed = channel.Reader.TryRead(out _);
        INotification? drained = queue.DrainVStopped();

        Assert.Multiple(() => {
            Assert.That(firstPushed, Is.True);
            Assert.That(pushedItem, Is.EqualTo(first));
            Assert.That(secondAlsoPushed, Is.False);
            Assert.That(drained, Is.EqualTo(second));
        });
    }

    /// <summary>
    /// 保留通知が無い状態で DrainVStopped を呼ぶと null が返る(呼び出し側が
    /// OK を返す合図、§4.5のドレイン完了)ことを確認する。
    /// </summary>
    [Test]
    public void DrainVStopped_WithNoPendingNotifications_ReturnsNull() {
        var channel = Channel.CreateBounded<INotification>(1);
        var queue = new NotificationQueue(channel.Writer);

        INotification? drained = queue.DrainVStopped();

        Assert.That(drained, Is.Null);
    }

    /// <summary>
    /// 3件Enqueueした場合、1件目は即時push、2・3件目はDrainVStoppedの呼び出し
    /// 順に沿ってFIFOで取り出せることを確認する(Arcavirt-o3e.10.4の
    /// lockベース実装の回帰防止)。
    /// </summary>
    [Test]
    public void Enqueue_ThreeNotifications_DeliversInFifoOrderAcrossPushAndDrain() {
        var channel = Channel.CreateBounded<INotification>(1);
        var queue = new NotificationQueue(channel.Writer);
        var first = new StopNotification(new StopEvent(default, StopReason.Signal, 1, 0));
        var second = new StopNotification(new StopEvent(default, StopReason.Signal, 2, 0));
        var third = new StopNotification(new StopEvent(default, StopReason.Signal, 3, 0));

        queue.Enqueue(first);
        queue.Enqueue(second);
        queue.Enqueue(third);

        channel.Reader.TryRead(out INotification? pushed);
        INotification? drained1 = queue.DrainVStopped();
        INotification? drained2 = queue.DrainVStopped();

        Assert.Multiple(() => {
            Assert.That(pushed, Is.EqualTo(first));
            Assert.That(drained1, Is.EqualTo(second));
            Assert.That(drained2, Is.EqualTo(third));
        });
    }
}