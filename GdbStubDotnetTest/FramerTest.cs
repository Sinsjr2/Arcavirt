using GdbStubDotnet;
using NUnit.Framework;

namespace GdbStubDotnetTest;

[TestFixture]
public class FramerTest {
    /// <summary>
    /// 1パケット分のバイト列を一括で渡すと、Packetイベントが正しいpayloadで
    /// 1回だけ発生することを確認する。
    /// </summary>
    [Test]
    public void ProcessBytes_SingleChunkOnePacket_EmitsPacketWithCorrectPayload() {
        var framer = new Framer();
        byte[] packet = Framer.Encode("g"u8);
        List<FramerEvent> events = [];

        framer.ProcessBytes(packet, events.Add);

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].Kind, Is.EqualTo(FramerEventKind.Packet));
        Assert.That(events[0].Payload, Is.EqualTo("g"u8.ToArray()));
    }

    /// <summary>
    /// 同じパケットを1バイトずつ渡しても、一括で渡した場合と同じ結果になる
    /// ことを確認する(TCP の読み取り境界とパケット境界が一致しない状況の再現)。
    /// </summary>
    [Test]
    public void ProcessBytes_ByteAtATime_EmitsSamePacketAsSingleChunk() {
        var framer = new Framer();
        byte[] packet = Framer.Encode("qC"u8);
        List<FramerEvent> events = [];

        foreach (byte b in packet) {
            framer.ProcessBytes([b], events.Add);
        }

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].Kind, Is.EqualTo(FramerEventKind.Packet));
        Assert.That(events[0].Payload, Is.EqualTo("qC"u8.ToArray()));
    }

    /// <summary>
    /// チェックサムの2桁が別々のチャンクに分割されて届いても、
    /// 正しく1つのPacketイベントとして完成することを確認する。
    /// </summary>
    [Test]
    public void ProcessBytes_SplitMidChecksum_StillCompletesPacket() {
        var framer = new Framer();
        byte[] packet = Framer.Encode("g"u8);
        int splitPoint = packet.Length - 1;
        List<FramerEvent> events = [];

        framer.ProcessBytes(packet.AsSpan(0, splitPoint), events.Add);
        framer.ProcessBytes(packet.AsSpan(splitPoint), events.Add);

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].Kind, Is.EqualTo(FramerEventKind.Packet));
        Assert.That(events[0].Payload, Is.EqualTo("g"u8.ToArray()));
    }

    /// <summary>
    /// 1チャンクに2パケット分のバイト列が含まれる場合、両方が順番に
    /// Packetイベントとして発生することを確認する。
    /// </summary>
    [Test]
    public void ProcessBytes_TwoPacketsInOneChunk_EmitsBothInOrder() {
        var framer = new Framer();
        byte[] first = Framer.Encode("g"u8);
        byte[] second = Framer.Encode("qC"u8);
        byte[] combined = [.. first, .. second];
        List<FramerEvent> events = [];

        framer.ProcessBytes(combined, events.Add);

        Assert.That(events, Has.Count.EqualTo(2));
        Assert.Multiple(() => {
            Assert.That(events[0].Payload, Is.EqualTo("g"u8.ToArray()));
            Assert.That(events[1].Payload, Is.EqualTo("qC"u8.ToArray()));
        });
    }

    /// <summary>
    /// チェックサムが不正な場合、Packetではなく ChecksumMismatch イベントが
    /// 発生することを確認する。
    /// </summary>
    [Test]
    public void ProcessBytes_WrongChecksum_EmitsChecksumMismatch() {
        var framer = new Framer();
        byte[] packet = "$g#00"u8.ToArray();
        List<FramerEvent> events = [];

        framer.ProcessBytes(packet, events.Add);

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].Kind, Is.EqualTo(FramerEventKind.ChecksumMismatch));
    }

    /// <summary>
    /// パケットの前に単独の + (ack) が混ざっていても、Ackイベントとして
    /// 消費されるだけで、後続のパケット解析がデシンクしないことを確認する。
    /// </summary>
    [Test]
    public void ProcessBytes_LeadingAckBeforePacket_DoesNotDesyncSubsequentPacket() {
        var framer = new Framer();
        byte[] packet = Framer.Encode("g"u8);
        byte[] combined = [(byte)'+', .. packet];
        List<FramerEvent> events = [];

        framer.ProcessBytes(combined, events.Add);

        Assert.That(events, Has.Count.EqualTo(2));
        Assert.Multiple(() => {
            Assert.That(events[0].Kind, Is.EqualTo(FramerEventKind.Ack));
            Assert.That(events[1].Kind, Is.EqualTo(FramerEventKind.Packet));
            Assert.That(events[1].Payload, Is.EqualTo("g"u8.ToArray()));
        });
    }

    /// <summary>
    /// Encode したバイト列を Framer でデコードすると、元の payload と
    /// 一致することを確認する(往復の整合性)。
    /// </summary>
    [Test]
    public void Encode_ThenProcessBytes_RoundTripsToOriginalPayload() {
        byte[] original = "X0,4:deadbeef"u8.ToArray();
        byte[] encoded = Framer.Encode(original);
        var framer = new Framer();
        List<FramerEvent> events = [];

        framer.ProcessBytes(encoded, events.Add);

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].Kind, Is.EqualTo(FramerEventKind.Packet));
        Assert.That(events[0].Payload, Is.EqualTo(original));
    }
}