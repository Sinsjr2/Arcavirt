using System.Text;
using CPU;
using Gdb;
using Gdb.BreakPoint;
using Gdb.Thread;
using Gdb.Thread.SingleThread;
using RX;

namespace GdbStub;

public class RXv1Stub : IGdbStub,
    IGDBSingleThread, IBreakPoint, IHardWareBreakPoint, ISoftWareBreakPoint, ISingleThreadResume, IGdbCustomCommand, ISingleThreadStep {

    readonly InstructionLoop<RXv1Core> instructionLoop;

    RXv1Core rxCore => instructionLoop.Target;

    public IBreakPoint? BreakpointObject => this;

    public IGDBThread? ThreadObject => this;

    public string TargetDescriptionXML => """
<?xml version="1.0"?>
<!DOCTYPE feature SYSTEM "gdb-target.dtd">
<feature name="org.gnu.gdb.rx.core">
  <flags id="psw_flags" size="4">
    <field name="C" start="0" end="0"/>
    <field name="Z" start="1" end="1"/>
    <field name="S" start="2" end="2"/>
    <field name="O" start="3" end="3"/>
    <field name="I" start="16" end="16"/>
    <field name="U" start="17" end="17"/>
    <field name="PM" start="20" end="20"/>
    <field name="IPL" start="24" end="27"/>
  </flags>
  <flags id="fpsw_flags" size="4">
    <field name="RM" start="0" end="1"/>
    <field name="CV" start="2" end="2"/>
    <field name="CO" start="3" end="3"/>
    <field name="CZ" start="4" end="4"/>
    <field name="CU" start="5" end="5"/>
    <field name="CX" start="6" end="6"/>
    <field name="CE" start="7" end="7"/>
    <field name="DN" start="8" end="8"/>
    <field name="EV" start="10" end="10"/>
    <field name="EO" start="11" end="11"/>
    <field name="EZ" start="12" end="12"/>
    <field name="EU" start="13" end="13"/>
    <field name="EX" start="14" end="14"/>
    <field name="FV" start="26" end="26"/>
    <field name="FO" start="27" end="27"/>
    <field name="FZ" start="28" end="28"/>
    <field name="FU" start="29" end="29"/>
    <field name="FX" start="30" end="30"/>
    <field name="FS" start="31" end="31"/>
  </flags>
  <reg name="r0" bitsize="32" type="data_ptr"/>
  <reg name="r1" bitsize="32" type="uint32"/>
  <reg name="r2" bitsize="32" type="uint32"/>
  <reg name="r3" bitsize="32" type="uint32"/>
  <reg name="r4" bitsize="32" type="uint32"/>
  <reg name="r5" bitsize="32" type="uint32"/>
  <reg name="r6" bitsize="32" type="uint32"/>
  <reg name="r7" bitsize="32" type="uint32"/>
  <reg name="r8" bitsize="32" type="uint32"/>
  <reg name="r9" bitsize="32" type="uint32"/>
  <reg name="r10" bitsize="32" type="uint32"/>
  <reg name="r11" bitsize="32" type="uint32"/>
  <reg name="r12" bitsize="32" type="uint32"/>
  <reg name="r13" bitsize="32" type="uint32"/>
  <reg name="r14" bitsize="32" type="uint32"/>
  <reg name="r15" bitsize="32" type="uint32"/>
  <reg name="usp" bitsize="32" type="data_ptr"/>
  <reg name="isp" bitsize="32" type="data_ptr"/>
  <reg name="psw" bitsize="32" type="psw_flags"/>
  <reg name="pc" bitsize="32" type="code_ptr"/>
  <reg name="intb" bitsize="32" type="data_ptr"/>
  <reg name="bpsw" bitsize="32" type="psw_flags"/>
  <reg name="bpc" bitsize="32" type="code_ptr"/>
  <reg name="fintv" bitsize="32" type="code_ptr"/>
  <reg name="fpsw" bitsize="32" type="fpsw_flags"/>
  <reg name="acc" bitsize="64" type="uint64"/>
</feature>
""";

    public ISingleThreadResume? ResumeObject => this;

    public int NumOfRegisters => 26;

    public IHardWareBreakPoint? HwBreakPointObject => this;

    public ISoftWareBreakPoint? SwBreakPointObject => this;

    public IWatchPoint? WatchPointObject => null;

    public ISingleThreadStep? StepObject => this;

    public ISingleThreadRangeStep? RangeStepObject => null;

    public IGdbCustomCommand? GdbCustomCommandObject => this;

    public event Action? OnBreak;

    public RXv1Stub(InstructionLoop<RXv1Core> instructionLoop) {
        this.instructionLoop = instructionLoop;
        instructionLoop.Target.OnBreak += () => OnBreak?.Invoke();
    }

    public int WriteRegisters(int i, ReadOnlySpan<char> value) {
        switch (i) {
            case < 0:
                return 0;
            case <= 15 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.Registers[i] = x;
                return len;
            case 16 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.USP = x;
                return len;
            case 17 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.ISP = x;
                return len;
            case 18 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.PSW = x;
                return len;
            case 19 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.PC = x;
                return len;
            case 20 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.INTB = x;
                return len;
            case 21 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.BPSW = x;
                return len;
            case 22 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.BPC = x;
                return len;
            case 23 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.FINTV = x;
                return len;
            case 24 when GDBUtils.TryDecodeHexUInt32LE(value, out var x, out var len):
                rxCore.FPSW = x;
                return len;
            case 25 when GDBUtils.TryDecodeHexUInt64LE(value, out var x, out var len):
                rxCore.Acc = x;
                return len;
            default:
                return 0;
        }
    }

    int IGDBSingleThread.ReadRegister(int i, StringBuilder result) {
        switch (i) {
            case < 0: return 0;
            case <= 15: return GDBUtils.EncodeHexUInt32LE(result, rxCore.Registers[i]);
            case 16: return GDBUtils.EncodeHexUInt32LE(result, rxCore.USP);
            case 17: return GDBUtils.EncodeHexUInt32LE(result, rxCore.ISP);
            case 18: return GDBUtils.EncodeHexUInt32LE(result, rxCore.PSW);
            case 19: return GDBUtils.EncodeHexUInt32LE(result, rxCore.PC);
            case 20: return GDBUtils.EncodeHexUInt32LE(result, rxCore.INTB);
            case 21: return GDBUtils.EncodeHexUInt32LE(result, rxCore.BPSW);
            case 22: return GDBUtils.EncodeHexUInt32LE(result, rxCore.BPC);
            case 23: return GDBUtils.EncodeHexUInt32LE(result, rxCore.FINTV);
            case 24: return GDBUtils.EncodeHexUInt32LE(result, rxCore.FPSW);
            case 25: return GDBUtils.EncodeHexUInt64LE(result, rxCore.Acc);
            default: return 0;
        }
    }

    public bool ReadMemory(StringBuilder result, ulong startAddr, ulong length) {
        for (uint i = 0; i < (uint)length; i++) {
            GDBUtils.EncodeHexByte(result, (byte)rxCore.Bus.Read((uint)startAddr + i, 1));
        }
        return true;
    }

    public bool WriteMemory(ulong startAddr, ReadOnlySpan<char> values) {
        if ((values.Length % 2) != 0) {
            return false;
        }
        var byteLength = values.Length / 2;
        for (uint i = 0; i < byteLength; i++) {
            if (!GDBUtils.TryDecodeHexU8(values[((int)i * 2)..], out _, out _)) {
                return false;
            }
        }

        for (uint i = 0; i < byteLength; i++) {
            GDBUtils.TryDecodeHexU8(values[((int)i * 2)..], out var x, out _);
            rxCore.Bus.Write((uint)startAddr + i, 1, x);
        }
        return true;
    }

    public void HandleCtrlC() {
        instructionLoop.Stop();
        rxCore.Stop();
    }

    bool AddBreakPoint(ulong address) {
        if (uint.MaxValue < address) {
            return false;
        }
        rxCore.PcBreakpoints.Add((uint)address);
        return true;
    }

    bool RemoveBreakPoint(ulong address) {
        if (uint.MaxValue < address) {
            return false;
        }
        rxCore.PcBreakpoints.Remove((uint)address);
        return true;
    }

    public bool AddHwBreakPoint(ulong address, uint length) {
        return AddBreakPoint(address);
    }

    public bool RemoveHwBreakPoint(ulong address, uint length) {
        return RemoveBreakPoint(address);
    }

    public bool AddSwBreakPoint(ulong address, uint length) {
        return AddBreakPoint(address);
    }

    public bool RemoveSwBreakPoint(ulong address, uint length) {
        return RemoveBreakPoint(address);
    }

    public void Resume(ulong? address, byte? signal) {
        if (address.HasValue) {
            rxCore.PC = unchecked((uint)address);
        }
        instructionLoop.Target.Start();
        instructionLoop.Start();
        instructionLoop.Target.NextStep(true);
    }

    public void Step(ulong? address, byte? signal = null) {
        if (address.HasValue) {
            rxCore.PC = (uint)address.Value & 0xFFFF_FFFF;
        }
        instructionLoop.Target.NextStep(true);
        OnBreak?.Invoke();
    }


    public void RunCustomCommand(StringBuilder result, string command) {
        switch (command) {
            case "reset":
                rxCore.Reset();
                break;
            case "user_reset":
                rxCore.Reset();
                break;
        }
        result.Append("OK");
    }
}
