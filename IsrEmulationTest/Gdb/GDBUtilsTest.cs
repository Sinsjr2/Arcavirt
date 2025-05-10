using Gdb;
using System.Buffers;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TestTools;

namespace IsrEmulationTest.Gdb;
public partial class GDBUtilsTest {

    [Test]
    [TestCase("1ff0", 0x1FF0uL)]
    [TestCase("1FF0", 0x1FF0uL)]
    [TestCase("00", 0x00uL)]
    [TestCase("01", 0x01uL)]
    [TestCase("10", 0x10uL)]
    [TestCase("100", 0x100uL)]
    [TestCase("F", 0xFuL)]
    [TestCase("FF", 0xFFuL)]
    public void TryDecoeHexUIntegerBE_SuccessTest(string input, ulong expected) {
        GDBUtils.TryDecoeHexUIntegerBE(input, out var actual, out var actualLength).Is(true);
        actualLength.Is(input.Length);
        actual.Is(expected);
    }

    [Test]
    [TestCase("0123456789", 0, "0123456789")]
    [TestCase("0123456789", 1, "123456789")]
    [TestCase("0123456789", 2, "23456789")]
    [TestCase("0123456789", 8, "89")]
    [TestCase("0123456789", 9, "9")]
    [TestCase("0123456789", 10, "")]
    public void MoveAheadTest(string input, int moveLength, string expected) {
        var writer = new ArrayBufferWriter<char>();
        writer.Write(input);
        GDBUtils.MoveAhead(writer, moveLength);
        new string(writer.WrittenSpan).Is(expected);
    }

    [Test]
    [TestCase(GDBMessageKind.StoreBuffer, 0, 0, 1, "+")]
    [TestCase(GDBMessageKind.StoreBuffer, 0, 0, 1, "+$qSupported:")]
    [TestCase(GDBMessageKind.StoreBuffer, 0, 0, 0, "$qSupported:")]
    [TestCase(GDBMessageKind.StoreBuffer, 0, 0, 3, "+++")]
    [TestCase(GDBMessageKind.StoreBuffer, 0, 0, 3, "+++$")]
    [TestCase(GDBMessageKind.HasMessage, 1, 1, 4, "$#00")]
    [TestCase(GDBMessageKind.InvalidChecksum, 0, 0, 4, "$#0j")]
    [TestCase(GDBMessageKind.StoreBuffer, 0, 0, 1, "+$qSupported:multiprocess+;swbreak+;hwbreak+;qRelocInsn+;fork-events+;vfork-events+;exec-events+;vContSupported+;QThreadEvents+;no-resumed+;memory-tagging+#")]
    [TestCase(GDBMessageKind.StoreBuffer, 0, 0, 1, "+$qSupported:multiprocess+;swbreak+;hwbreak+;qRelocInsn+;fork-events+;vfork-events+;exec-events+;vContSupported+;QThreadEvents+;no-resumed+;memory-tagging+#e")]
    [TestCase(GDBMessageKind.InvalidChecksum, 0, 0, 158, "+$qSupported:multiprocess+;swbreak+;hwbreak+;qRelocInsn+;fork-events+;vfork-events+;exec-events+;vContSupported+;QThreadEvents+;no-resumed+;memory-tagging+#e4")]
    [TestCase(GDBMessageKind.HasMessage, 2, 155, 158, "+$qSupported:multiprocess+;swbreak+;hwbreak+;qRelocInsn+;fork-events+;vfork-events+;exec-events+;vContSupported+;QThreadEvents+;no-resumed+;memory-tagging+#ec")]
    [TestCase(GDBMessageKind.HasMessage, 2, 155, 158, "+$qSupported:multiprocess+;swbreak+;hwbreak+;qRelocInsn+;fork-events+;vfork-events+;exec-events+;vContSupported+;QThreadEvents+;no-resumed+;memory-tagging+#ec++")]
    public void ParseGDBMessageTest(GDBMessageKind expKind, int expBeginIndex, int expEndIndex, int expNextIndex, string message) {
        var result = GDBUtils.ParseGDBMessage(message);
        result.NextIndex.Is(expNextIndex);
        result.ResultKind.Is(expKind);
        result.ValueArea.Start.Is(expBeginIndex);
        result.ValueArea.End.Is(expEndIndex);
    }

    static ReadOnlyMemory<char>? TryRead(ref Regex.ValueMatchEnumerator en, ReadOnlyMemory<char> input) {
        if (!en.MoveNext())  {
            return null;
        }
        var x = en.Current;
        return input.Slice(x.Index, x.Length);
    }

    [Test]
    [TestCase("Z1,ffc02592,1", 1, 0xFFC02592u, 1)]
    public void BreakPointTest(string input, int expType, uint expAddress, int expKind) {
        var reader = new GdbMessageReader(input.AsMemory());
        reader.TryReadChar(out var x).Is(true);
        (x is 'z' or 'Z').Is(true);
        reader.TryReadHexUIntegerBE(out var type).Is(true);
        type.Is((ulong)expType);
        reader.TryReadIfExpChar(',').Is(true);
        reader.TryReadHexUIntegerBE(out var address).Is(true);
        address.Is(expAddress);
        reader.TryReadIfExpChar(',').Is(true);
        reader.TryReadHexUIntegerBE(out var kind).Is(true);
        kind.Is((ulong)expKind);
    }
}