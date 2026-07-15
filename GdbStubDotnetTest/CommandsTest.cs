using GdbStubDotnet;
using NUnit.Framework;
using Pidgin;

namespace GdbStubDotnetTest;

[TestFixture]
public class CommandsTest {
    /// <summary>
    /// "m1000,4" をパースすると、Addr=0x1000, Len=4 の ReadMemoryCommand に
    /// なることを確認する。
    /// </summary>
    [Test]
    public void ReadMemory_ParsesAddrAndLenAsHex() {
        var result = Commands.ReadMemory.Parse("m1000,4"u8);

        Assert.That(result.Success, Is.True);
        Assert.Multiple(() => {
            Assert.That(result.Value.Addr, Is.EqualTo(0x1000UL));
            Assert.That(result.Value.Len, Is.EqualTo(4));
        });
    }

    /// <summary>
    /// "M1000,4:deadbeef" をパースすると、Addr=0x1000, Len=4,
    /// Data=[0xde,0xad,0xbe,0xef] の WriteMemoryCommand になることを確認する。
    /// </summary>
    [Test]
    public void WriteMemory_ParsesAddrLenAndHexData() {
        var result = Commands.WriteMemory.Parse("M1000,4:deadbeef"u8);

        Assert.That(result.Success, Is.True);
        Assert.Multiple(() => {
            Assert.That(result.Value.Addr, Is.EqualTo(0x1000UL));
            Assert.That(result.Value.Len, Is.EqualTo(4));
            Assert.That(result.Value.Data, Is.EqualTo(new byte[] { 0xde, 0xad, 0xbe, 0xef }));
        });
    }

    /// <summary>
    /// "qSupported" のみ(コロン以降なし)をパースすると、Features が空配列に
    /// なることを確認する。
    /// </summary>
    [Test]
    public void Supported_WithoutFeatures_ParsesWithEmptyFeatures() {
        var result = Commands.Supported.Parse("qSupported"u8);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.Features, Is.Empty);
    }

    /// <summary>
    /// "qSupported:multiprocess+" をパースすると、コロン以降が Features として
    /// キャプチャされることを確認する。
    /// </summary>
    [Test]
    public void Supported_WithFeatures_CapturesFeaturesAfterColon() {
        var result = Commands.Supported.Parse("qSupported:multiprocess+"u8);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.Features, Is.EqualTo("multiprocess+"u8.ToArray()));
    }

    /// <summary>
    /// "Gdeadbeef" をパースすると、Data=[0xde,0xad,0xbe,0xef] の
    /// WriteRegistersCommand になることを確認する。
    /// </summary>
    [Test]
    public void WriteRegisters_ParsesHexData() {
        var result = Commands.WriteRegisters.Parse("Gdeadbeef"u8);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.Data, Is.EqualTo(new byte[] { 0xde, 0xad, 0xbe, 0xef }));
    }

    /// <summary>
    /// "p1a" をパースすると、Number=0x1a(26) の ReadRegisterCommand に
    /// なることを確認する。
    /// </summary>
    [Test]
    public void ReadRegister_ParsesRegisterNumberAsHex() {
        var result = Commands.ReadRegister.Parse("p1a"u8);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.Number, Is.EqualTo(0x1a));
    }

    /// <summary>
    /// "P1a=deadbeef" をパースすると、Number=0x1a、Data=[0xde,0xad,0xbe,0xef]
    /// の WriteRegisterCommand になることを確認する。
    /// </summary>
    [Test]
    public void WriteRegister_ParsesNumberAndHexData() {
        var result = Commands.WriteRegister.Parse("P1a=deadbeef"u8);

        Assert.That(result.Success, Is.True);
        Assert.Multiple(() => {
            Assert.That(result.Value.Number, Is.EqualTo(0x1a));
            Assert.That(result.Value.Data, Is.EqualTo(new byte[] { 0xde, 0xad, 0xbe, 0xef }));
        });
    }

    /// <summary>
    /// "Z0,1000,4" をパースすると、Type=Soft、Addr=0x1000、Kind=4 の
    /// InsertBreakpointCommand になることを確認する。
    /// </summary>
    [Test]
    public void InsertBreakpoint_ParsesTypeAddrAndKind() {
        var result = Commands.InsertBreakpoint.Parse("Z0,1000,4"u8);

        Assert.That(result.Success, Is.True);
        Assert.Multiple(() => {
            Assert.That(result.Value.Type, Is.EqualTo(BpType.Soft));
            Assert.That(result.Value.Addr, Is.EqualTo(0x1000UL));
            Assert.That(result.Value.Kind, Is.EqualTo(4));
        });
    }

    /// <summary>
    /// "z1,2000,2" をパースすると、Type=Hard、Addr=0x2000、Kind=2 の
    /// RemoveBreakpointCommand になることを確認する。
    /// </summary>
    [Test]
    public void RemoveBreakpoint_ParsesTypeAddrAndKind() {
        var result = Commands.RemoveBreakpoint.Parse("z1,2000,2"u8);

        Assert.That(result.Success, Is.True);
        Assert.Multiple(() => {
            Assert.That(result.Value.Type, Is.EqualTo(BpType.Hard));
            Assert.That(result.Value.Addr, Is.EqualTo(0x2000UL));
            Assert.That(result.Value.Kind, Is.EqualTo(2));
        });
    }

    /// <summary>
    /// "?" をパースすると HaltReasonCommand になることを確認する。
    /// </summary>
    [Test]
    public void HaltReason_ParsesQuestionMark() {
        var result = Commands.HaltReason.Parse("?"u8);

        Assert.That(result.Success, Is.True);
    }

    /// <summary>
    /// "s"(アドレス省略)をパースすると Addr=null の StepCommand になる
    /// ことを確認する。
    /// </summary>
    [Test]
    public void Step_WithoutAddr_ParsesWithNullAddr() {
        var result = Commands.Step.Parse("s"u8);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.Addr, Is.Null);
    }

    /// <summary>
    /// "s1000" をパースすると Addr=0x1000 の StepCommand になることを確認する。
    /// </summary>
    [Test]
    public void Step_WithAddr_ParsesHexAddr() {
        var result = Commands.Step.Parse("s1000"u8);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.Addr, Is.EqualTo(0x1000UL));
    }

    /// <summary>
    /// "vCont;c" をパースすると、Continue アクション1件を持つ VContCommand に
    /// なることを確認する。
    /// </summary>
    [Test]
    public void VCont_ParsesContinueAction() {
        var result = Commands.VCont.Parse("vCont;c"u8);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.Actions, Has.Length.EqualTo(1));
        Assert.That(result.Value.Actions[0].Kind, Is.EqualTo(ActionKind.Continue));
    }

    /// <summary>
    /// "vCont?" をパースすると VContQueryCommand になることを確認する。
    /// </summary>
    [Test]
    public void VContQuery_ParsesQuestionForm() {
        var result = Commands.VContQuery.Parse("vCont?"u8);

        Assert.That(result.Success, Is.True);
    }
}