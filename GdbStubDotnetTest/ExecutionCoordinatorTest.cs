using System.Threading.Channels;
using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class ExecutionCoordinatorTest {
    /// <summary>
    /// 1つ目の resume(BeginResume)からの responder が ReportStop を呼ぶ前に
    /// 2つ目の resume が始まった場合、1つ目の(既に古い) responder が遅れて
    /// ReportStop を呼んでも無視され、2つ目の resume の報告だけが共有チャネルに
    /// 届くことを確認する(§4.4のスコープ規律: resume外の停止報告は無視)。
    /// StubServer/ソケットを介さず ExecutionCoordinator を直接操作することで、
    /// チャネル読み取り側の非同期継続タイミングに依存しない決定論的な検証にする
    /// (TryWrite/TryRead はいずれも同期・即時であり、読み取りタスクが存在しない
    /// ため競合状態が原理的に発生しない)。
    /// </summary>
    [Test]
    public void OnReportStop_StaleResponderAfterNewResumeBegan_IsIgnoredAndOnlyLatestResultIsDelivered() {
        var channel = Channel.CreateBounded<ExecOutcome>(1);
        var coordinator = new ExecutionCoordinator(channel.Writer, CreateUnusedNotificationQueue());

        ExecutionResponder first = coordinator.BeginResume();
        ExecutionResponder second = coordinator.BeginResume();

        first.ReportStop(new StopEvent(default, StopReason.Signal, 9, 0));
        second.ReportStop(new StopEvent(default, StopReason.Signal, 5, 0));

        bool delivered = channel.Reader.TryRead(out ExecOutcome outcome);
        bool extra = channel.Reader.TryRead(out _);

        Assert.Multiple(() => {
            Assert.That(delivered, Is.True);
            Assert.That(outcome.Stop.SignalOrExit, Is.EqualTo(5));
            Assert.That(extra, Is.False);
        });
    }

    /// <summary>
    /// Reject 経路にも同じ token 判定が適用され、古い resume からの Reject は
    /// 無視されることを確認する(ReportStop 経路との対称性の回帰防止)。
    /// </summary>
    [Test]
    public void OnReject_StaleResponderAfterNewResumeBegan_IsIgnored() {
        var channel = Channel.CreateBounded<ExecOutcome>(1);
        var coordinator = new ExecutionCoordinator(channel.Writer, CreateUnusedNotificationQueue());

        ExecutionResponder first = coordinator.BeginResume();
        ExecutionResponder second = coordinator.BeginResume();

        first.Reject(new RspError(1, null));
        second.ReportStop(new StopEvent(default, StopReason.Signal, 5, 0));

        bool delivered = channel.Reader.TryRead(out ExecOutcome outcome);

        Assert.Multiple(() => {
            Assert.That(delivered, Is.True);
            Assert.That(outcome.IsReject, Is.False);
            Assert.That(outcome.Stop.SignalOrExit, Is.EqualTo(5));
        });
    }

    /// <summary>
    /// Mode を NonStop に設定した状態で OnReportStop を呼ぶと、all-stop用の
    /// 共有 ExecOutcome チャネルには何も書き込まれず、NotificationQueue の
    /// push チャネル側に %Stop 相当の通知が届くことを確認する
    /// (§4.3手順5のモード分岐、Arcavirt-o3e.10.3)。
    /// </summary>
    [Test]
    public void OnReportStop_InNonStopMode_RoutesToNotificationQueueInsteadOfExecChannel() {
        var execChannel = Channel.CreateBounded<ExecOutcome>(1);
        var notifyChannel = Channel.CreateBounded<INotification>(1);
        var notificationQueue = new NotificationQueue(notifyChannel.Writer);
        var coordinator = new ExecutionCoordinator(execChannel.Writer, notificationQueue);
        coordinator.SetMode(ResumeMode.NonStop);

        ExecutionResponder responder = coordinator.BeginResume();
        responder.ReportStop(new StopEvent(default, StopReason.Signal, 5, 0));

        bool execChannelHasItem = execChannel.Reader.TryRead(out _);
        bool notifyChannelHasItem = notifyChannel.Reader.TryRead(out INotification? notification);

        Assert.Multiple(() => {
            Assert.That(execChannelHasItem, Is.False);
            Assert.That(notifyChannelHasItem, Is.True);
            Assert.That(notification, Is.EqualTo(new StopNotification(new StopEvent(default, StopReason.Signal, 5, 0))));
        });
    }

    private static NotificationQueue CreateUnusedNotificationQueue() {
        return new NotificationQueue(Channel.CreateBounded<INotification>(1).Writer);
    }
}