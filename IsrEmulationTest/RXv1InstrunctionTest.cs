using NUnit.Framework;
using RX;
using TestTools;
using static RX.RXv1Assembler;
using static RX.Reg;
using Pheripheral;
using System.Buffers;
using Peripheral.Renesas;
using Util;

namespace IsrEmulationTest;
public class RXv1InstrunctionTest {
    RXv1Core cpu;
    RAM32Bit memory;
    RAM32Bit rom;
    BusManager busManager;

    // 毎回インスタンスを作ると重いため
    static readonly Translate rxv1Translate = new();

    uint ramBeginAddress;
    uint ramEndAddress;
    uint romBeginAddress;
    uint romEndAddress;

    static uint GetSize(MemEx size) =>
        size switch {
        MemEx.B => 1,
        MemEx.W or MemEx.UW => 2,
        MemEx.L => 4,
        _ => throw new NotSupportedException(size.ToString())
    };


    static uint CalcDspAddr(uint[] registers, uint dsp, int reg) {
        return registers[reg] + dsp;
    }


    static uint ReadIndexAddr(uint[] registers, LengthOfDisplacement ld, uint? dsp, int reg) {
        switch (ld) {
            case LengthOfDisplacement.RefReg:
                return registers[reg];
            case LengthOfDisplacement.DSP8Reg:
            case LengthOfDisplacement.DSP16Reg:
                return CalcDspAddr(registers, dsp!.Value, reg);
        }
        throw new ArgumentException($"expected: 0 <= && ld < 3 actual: {ld}");
    }

    static void StoreDestOperand(uint[] registers, IBus32 bus, MemEx sz, Reg rd, LengthOfDisplacement ld, uint? dsp, uint value) {
        var size = sz switch {
            MemEx.B => 1,
            MemEx.W => 2,
            MemEx.L => 4,
            MemEx.UW => 2,
            _ => throw new ArgumentException($"not supported sz. actual: {sz}")
        };
        if (ld == 0) {
            bus.Write(registers[(int)rd], size, value);
            return;
        }
        if ((int)ld < 3) {
            var addr = ReadIndexAddr(registers, ld, dsp!.Value, (int)rd);
            bus.Write(addr, size, value);
            return;
        }
        registers[(int)rd] = value;
    }

    static uint LoadData(uint[] registers, IBus32 bus, LengthOfDisplacement ld, uint sz, uint? dsp, Reg rs) {
        if (ld == 0) {
            return bus.Read(registers[(int)rs], 1 << (int)sz);
        }
        if ((int)ld < 3) {
            var addr = ReadIndexAddr(registers, ld, dsp!.Value, (int)rs);
            return bus.Read(addr, 1 << (int)sz);
        }
        return BitOperation.GetLowerBits(registers[(int)rs], 1u << (int)sz);
    }

    /// <summary>
    /// ランダムなアドレスに対してデータを書き込みます。
    /// 間接参照の場合はレジスターに設定するメモリの範囲を指定する必要があります。
    /// </summary>
    void StoreRandomDest(StdRegAddressing dest, uint addrBegin, uint addrEnd, uint value) {
        var random = TestContext.CurrentContext.Random;
        switch (dest.LD) {
            case LengthOfDisplacement.RefReg:
            case LengthOfDisplacement.DSP8Reg:
            case LengthOfDisplacement.DSP16Reg: {
                // メモリにデータを書き込めるように範囲を制限する (最大4バイト)
                var addr = random.NextUInt(addrBegin, addrEnd - 4);
                cpu.Registers[(int)dest.TargetReg] = addr;
                break;
            }
        }
        StoreDestOperand(cpu.Registers, busManager, MemEx.L, dest.TargetReg, dest.LD, dest.Displacement, value);
    }

    static StdRegAddressing GetRandomStdRegAddressing(Reg reg, MemEx size, LengthOfDisplacement ld) {
        var random = TestContext.CurrentContext.Random;
        switch (ld) {
            case LengthOfDisplacement.Reg:
                return reg;
            case LengthOfDisplacement.RefReg:
                return new RegRef(reg, size);
            case LengthOfDisplacement.DSP8Reg: {
                var dsp = random.NextByte();
                return new RelRef8(dsp, reg, size);
            }
            case LengthOfDisplacement.DSP16Reg: {
                var dsp = random.NextUShort(0, 300);
                return new RelRef16(dsp, reg, size);
            }
            default:
                throw new ArgumentException($"{ld} not supported");
        }
    }

    public RXv1InstrunctionTest() {
        Setup();
    }

    [SetUp]
    public void Setup() {
        memory = new RAM32Bit("sram", 512_000);
        rom = new RAM32Bit("rom", 1024);
        busManager = new BusManager();
        ramBeginAddress = 0;
        ramEndAddress = ramBeginAddress + memory.MemorySize;
        busManager.AddRangedAddressMapping(ramBeginAddress, ramEndAddress, memory);
        romEndAddress = 0xFFFFFFFF;
        romBeginAddress = romEndAddress - rom.MemorySize;
        busManager.AddRangedAddressMapping(romBeginAddress, romEndAddress, rom);
        cpu = new RXv1Core(busManager);
        //cpu.PC = romBeginAddress;
        cpu.PC = 0;
        cpu.SP = ramEndAddress;
    }

    void RunOpcode(Instruction32 instruction) {
        var writer = new ArrayBufferWriter<byte>();
        var asmWriter = new AssemblyWriter(writer);
        rxv1Translate.CreateBinary(instruction, ref asmWriter);
        var instData = new RAM32Bit("", 10);
        instData.WriteRange(0, writer.WrittenMemory.ToArray());
        var reader = new Reader(instData, cpu.PC);
        var prevPos = reader.Position;
        var queue = new Queue<uint>();
        rxv1Translate.ParseAssembly(ref reader, queue);
        var afterPos = reader.Position;
        var opSize = afterPos - prevPos;

        var instArgs = queue.ToArray();
        cpu.ExecuteInstruction((OpCode)instArgs[0], instArgs.AsSpan(1), opSize);
    }

    [Test]
    [TestCase(1000u, 1000u)]
    [TestCase(   0u, 0u)]
    [TestCase(unchecked((uint)-1000), 1000u)]
    public void ABS_r_Test(uint a, uint result) {
        cpu.Registers[1] = a;
        RunOpcode(ABS(R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase(550u, 550u)]
    [TestCase(  0u,   0u)]
    [TestCase(unchecked((uint)-440), 440u)]
    public void ABS_rr_Test(uint a, uint result) {
        cpu.Registers[1] = a;
        RunOpcode(ABS(R1, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    public void ADD_() {
        this.memory.Write(0x001, 1, 100);
        cpu.Registers[1] = 1;
        cpu.Registers[2] = 10;
        RunOpcode(ADD(new RelRef8(0, R1, MemEx.B), R2));
        cpu.Registers[2].Is(110u);
    }

    [Test]
    [TestCase(false, LengthOfImmediate.IMM32, 10u, 5u, 15u)]
    [TestCase( true, LengthOfImmediate.IMM32, 10u, 5u, 16u)]
    public void ADC_imm_Test(bool psw_c, LengthOfImmediate li, uint a, uint b, uint result) {
        cpu.Registers[2] = b;
        cpu.PSW_c = psw_c;
        RunOpcode(ADC(new StdImmValue(li, a), R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(false, 10u, 5u, 15u)]
    [TestCase( true, 10u, 5u, 16u)]
    public void ADC_rr_Test(bool psw_c, uint a, uint b, uint result) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        cpu.PSW_c = psw_c;
        RunOpcode(ADC(R1, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(13, 1u, 14u)]
    public void ADD_4ir_Test(byte a, uint b, uint result) {
        cpu.Registers[1] = b;
        RunOpcode(ADD(new UInt4(a), R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8 ,   110u,  10u,   120u)]
    [TestCase(LengthOfImmediate.SIMM24, 70000u, 100u, 70100u)]
    public void ADD_imm_Test(LengthOfImmediate li, uint a, uint b, uint result) {
        cpu.Registers[2] = b;
        RunOpcode(ADD(new StdImmValue(li, a), R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(0u, 1u, 0u, true, false)]
    [TestCase(1u, 1u, 1u, false, false)]
    public void AND_Test(uint a, uint b, uint result, bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(AND(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( 0, 0xFFFF_FFFF, 0xFFFF_FFFEu)]
    [TestCase( 1, 0xFFFF_FFFF, 0xFFFF_FFFDu)]
    [TestCase(31, 0xFFFF_FFFF, 0x7FFF_FFFFu)]
    [TestCase(31,        0x0u,           0u)]
    public void BCLR_imm_Test(byte a, uint b, uint result) {
        cpu.Registers[1] = b;
        RunOpcode(BCLR(new UInt5(a), R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase( 0u,         0x0u,           0u)]
    [TestCase( 0u, 0xFFFF_FFFFu, 0xFFFF_FFFEu)]
    [TestCase( 1u, 0xFFFF_FFFFu, 0xFFFF_FFFDu)]
    [TestCase(31u, 0xFFFF_FFFFu, 0x7FFF_FFFFu)]
    [TestCase(33u, 0xFFFF_FFFFu, 0xFFFF_FFFDu)]
    public void BCLR_rr_Test(uint a, uint b, uint result) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(BCLR(R1, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(false, false, false, false,  3, Cnd.EQ, false)]
    [TestCase(false, false, false, false, 10, Cnd.EQ, false)]
    [TestCase(false,  true, false, false,  3, Cnd.EQ, true)]
    [TestCase(false,  true, false, false,  7, Cnd.EQ, true)]
    [TestCase(false,  true, false, false, 10, Cnd.EQ, true)]
    [TestCase(false, false, false, false,  8, Cnd.NE, true)]
    [TestCase(false,  true, false, false,  8, Cnd.NE, false)]
    [TestCase(false,  true, false, false,  4, Cnd.NE, false)]
    [TestCase(false, false, false, false,  4, Cnd.NE,  true)]
    public void BCndS_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        int src, Cnd condition,
        bool shouldJump
    ) {
        var beforePC = cpu.PC;
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        RunOpcode(BC_S(condition, (byte)src));

        if (shouldJump) {
            cpu.PC.Is(beforePC + (byte)src);
        }
        else {
            cpu.PC.Is(beforePC + 1);
        }
    }

    [Test]
    [TestCase(false, false, false, false,   4, Cnd.GEU, false)]
    [TestCase( true, false, false, false,   4, Cnd.GEU,  true)]
    [TestCase( true, false, false, false, 100, Cnd.GEU,  true)]
    [TestCase(false, false, false, false, 100, Cnd.GEU, false)]
    [TestCase(false, false, false, false,  20, Cnd. EQ, false)]
    [TestCase(false,  true, false, false,  20, Cnd. EQ,  true)]
    [TestCase(false, false, false, false,  50, Cnd.GTU, false)]
    [TestCase(false,  true, false, false,  50, Cnd.GTU, false)]
    [TestCase( true,  true, false, false,  50, Cnd.GTU, false)]
    [TestCase( true, false, false, false,  50, Cnd.GTU,  true)]
    [TestCase(false, false, false, false,  70, Cnd. PZ,  true)]
    [TestCase(false, false,  true, false,  70, Cnd. PZ, false)]
    [TestCase(false, false, false, false,  90, Cnd. GE,  true)]
    [TestCase(false, false,  true, false,  90, Cnd. GE, false)]
    [TestCase(false, false, false,  true,  90, Cnd. GE, false)]
    [TestCase(false, false,  true,  true,  90, Cnd. GE,  true)]
    [TestCase(false, false, false, false, 110, Cnd. GT,  true)]
    [TestCase(false,  true, false, false, 110, Cnd. GT, false)]
    [TestCase(false, false,  true, false, 110, Cnd. GT, false)]
    [TestCase(false, false, false,  true, 110, Cnd. GT, false)]
    [TestCase(false,  true,  true, false, 110, Cnd. GT, false)]
    [TestCase(false, false,  true,  true, 110, Cnd. GT,  true)]
    [TestCase(false,  true, false,  true, 110, Cnd. GT, false)]
    [TestCase(false,  true,  true,  true, 110, Cnd. GT, false)]
    [TestCase(false, false, false, false, 130, Cnd. O,  false)]
    [TestCase(false, false, false,  true, 130, Cnd. O,   true)]

    [TestCase(false, false, false, false,   5, Cnd.LTU,  true)]
    [TestCase( true, false, false, false,   5, Cnd.LTU, false)]
    [TestCase( true, false, false, false,  15, Cnd.LTU, false)]
    [TestCase(false, false, false, false,  15, Cnd.LTU,  true)]
    [TestCase(false, false, false, false,  30, Cnd. NE,  true)]
    [TestCase(false,  true, false, false,  30, Cnd. NE, false)]
    [TestCase(false, false, false, false,  40, Cnd.LEU,  true)]
    [TestCase(false,  true, false, false,  40, Cnd.LEU,  true)]
    [TestCase( true,  true, false, false,  40, Cnd.LEU,  true)]
    [TestCase( true, false, false, false,  40, Cnd.LEU, false)]
    [TestCase(false, false, false, false,  60, Cnd.  N, false)]
    [TestCase(false, false,  true, false,  60, Cnd.  N,  true)]
    [TestCase(false, false, false, false,  99, Cnd. LE, false)]
    [TestCase(false, false,  true, false,  99, Cnd. LE,  true)]
    [TestCase(false, false, false,  true,  99, Cnd. LE,  true)]
    [TestCase(false, false,  true,  true,  99, Cnd. LE, false)]
    [TestCase(false,  true, false, false,  99, Cnd. LE,  true)]
    [TestCase(false,  true,  true, false,  99, Cnd. LE,  true)]
    [TestCase(false,  true, false,  true,  99, Cnd. LE,  true)]
    [TestCase(false,  true,  true,  true,  99, Cnd. LE,  true)]
    [TestCase(false, false, false, false, 111, Cnd. LT, false)]
    [TestCase(false, false,  true, false, 111, Cnd. LT,  true)]
    [TestCase(false, false, false,  true, 111, Cnd. LT,  true)]
    [TestCase(false, false,  true,  true, 111, Cnd. LT, false)]
    [TestCase(false, false, false, false, 255, Cnd. NO,  true)]
    [TestCase(false, false, false,  true, 255, Cnd. NO, false)]

    public void BCndB_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        int src, Cnd condition,
        bool shouldJump
    ) {
        var beforePC = cpu.PC;
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        RunOpcode(BC_B(condition, (byte)src));

        if (shouldJump) {
            cpu.PC.Is(beforePC + (byte)src);
        }
        else {
            cpu.PC.Is(beforePC + 2);
        }
    }

    [TestCase(false, false, false, false,  2999, Cnd. EQ, false)]
    [TestCase(false,  true, false, false,  2999, Cnd. EQ,  true)]
    [TestCase(false, false, false, false, 66666, Cnd. NE,  true)]
    [TestCase(false,  true, false, false, 66666, Cnd. NE, false)]
    public void BCndW_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        int src, Cnd condition,
        bool shouldJump
    ) {
        var beforePC = cpu.PC;
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        RunOpcode(BC_W(condition, (ushort)src));

        if (shouldJump) {
            cpu.PC.Is(beforePC + (ushort)src);
        }
        else {
            cpu.PC.Is(beforePC + 3);
        }
    }

    static object[][] GetBMCndTestData() {
        return new object[][] {
            new object[] { false, false, false, false, Cnd.GEU, false },
            new object[] {  true, false, false, false, Cnd.GEU,  true },
            new object[] {  true, false, false, false, Cnd.GEU,  true },
            new object[] { false, false, false, false, Cnd.GEU, false },
            new object[] { false, false, false, false, Cnd. EQ, false },
            new object[] { false,  true, false, false, Cnd. EQ,  true },
            new object[] { false, false, false, false, Cnd.GTU, false },
            new object[] { false,  true, false, false, Cnd.GTU, false },
            new object[] {  true,  true, false, false, Cnd.GTU, false },
            new object[] {  true, false, false, false, Cnd.GTU,  true },
            new object[] { false, false, false, false, Cnd. PZ,  true },
            new object[] { false, false,  true, false, Cnd. PZ, false },
            new object[] { false, false, false, false, Cnd. GE,  true },
            new object[] { false, false,  true, false, Cnd. GE, false },
            new object[] { false, false, false,  true, Cnd. GE, false },
            new object[] { false, false,  true,  true, Cnd. GE,  true },
            new object[] { false, false, false, false, Cnd. GT,  true },
            new object[] { false,  true, false, false, Cnd. GT, false },
            new object[] { false, false,  true, false, Cnd. GT, false },
            new object[] { false, false, false,  true, Cnd. GT, false },
            new object[] { false,  true,  true, false, Cnd. GT, false },
            new object[] { false, false,  true,  true, Cnd. GT,  true },
            new object[] { false,  true, false,  true, Cnd. GT, false },
            new object[] { false,  true,  true,  true, Cnd. GT, false },
            new object[] { false, false, false, false, Cnd. O,  false },
            new object[] { false, false, false,  true, Cnd. O,   true },

            new object[] { false, false, false, false, Cnd.LTU,  true },
            new object[] {  true, false, false, false, Cnd.LTU, false },
            new object[] {  true, false, false, false, Cnd.LTU, false },
            new object[] { false, false, false, false, Cnd.LTU,  true },
            new object[] { false, false, false, false, Cnd. NE,  true },
            new object[] { false,  true, false, false, Cnd. NE, false },
            new object[] { false, false, false, false, Cnd.LEU,  true },
            new object[] { false,  true, false, false, Cnd.LEU,  true },
            new object[] {  true,  true, false, false, Cnd.LEU,  true },
            new object[] {  true, false, false, false, Cnd.LEU, false },
            new object[] { false, false, false, false, Cnd.  N, false },
            new object[] { false, false,  true, false, Cnd.  N,  true },
            new object[] { false, false, false, false, Cnd. LE, false },
            new object[] { false, false,  true, false, Cnd. LE,  true },
            new object[] { false, false, false,  true, Cnd. LE,  true },
            new object[] { false, false,  true,  true, Cnd. LE, false },
            new object[] { false,  true, false, false, Cnd. LE,  true },
            new object[] { false,  true,  true, false, Cnd. LE,  true },
            new object[] { false,  true, false,  true, Cnd. LE,  true },
            new object[] { false,  true,  true,  true, Cnd. LE,  true },
            new object[] { false, false, false, false, Cnd. LT, false },
            new object[] { false, false,  true, false, Cnd. LT,  true },
            new object[] { false, false, false,  true, Cnd. LT,  true },
            new object[] { false, false,  true,  true, Cnd. LT, false },
            new object[] { false, false, false, false, Cnd. NO,  true },
            new object[] { false, false, false,  true, Cnd. NO, false },
        };
    }


    [Test]
    [TestCaseSource(nameof(GetBMCndTestData))]
    public void BMCnd_reg_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        Cnd condition, bool sets) {
        var random = TestContext.CurrentContext.Random;
        var value = random.NextUInt();
        byte src = (byte)random.Next(0, 31);
        var reg = random.Next(1, 15);
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        cpu.Registers[reg] = value;
        RunOpcode(BMC(condition, src, (Reg)reg));
        if (sets) {
            cpu.Registers[reg].Is(value | (1u << src));
        }
        else {
            cpu.Registers[reg].Is(value & ~(1u << src));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetBMCndTestData))]
    public void BMCnd_mem_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        Cnd condition, bool sets) {
        var random = TestContext.CurrentContext.Random;
        var value = random.NextByte();
        byte src = (byte)random.Next(0, 8);
        var reg = random.Next(1, 15);
        var addr = 0x10u;
        memory.Write(addr, 1, value);
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        cpu.Registers[reg] = addr;
        RunOpcode(BMC(condition, src, new RegRef((Reg)reg, MemEx.B)));
        if (sets) {
            memory.Read(addr, 1).Is(value | (1u << src));
        }
        else {
            memory.Read(addr, 1).Is(value & ~(1u << src));
        }
    }

    [Test]
    [TestCase(0u, 15, 1u << 15)]
    [TestCase(~0u, 14, ~(1u << 14))]
    public void BNOT_imm5_Test(uint a, byte b, uint result) {
        cpu.Registers[1] = a;
        RunOpcode(BNOT(new UInt5(b), R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase(21u, 0u, 1u << 21)]
    [TestCase(0u, 1u << 21, 1u << 21 | 1u)]
    [TestCase(1u, 0u, 1u << 1)]
    [TestCase(0xE0u, 1u, 0u)]
    public void BNOT_rr_Test(uint a, uint b, uint result) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(BNOT(R1, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    public void BRA_S_Test([Range(3, 10)] byte src) {
        var prevPC = cpu.PC;
        RunOpcode(BRA_S(src));
        cpu.PC.Is(prevPC + src);
    }

    [Test]
    public void BRA_B_Test([Random(-128, 127, 4)] sbyte src) {
        var prevPC = cpu.PC;
        RunOpcode(BRA_B(src));
        cpu.PC.Is((uint)(prevPC + src));
    }

    [Test]
    public void BRA_W_Test([Random(-32768, 32767, 4)] short src) {
        var prevPC = cpu.PC;
        RunOpcode(BRA_W(src));
        cpu.PC.Is((uint)(prevPC + src));
    }

    [Test]
    public void BRA_A_Test([Random(-8388608, 8388607, 4)] int src) {
        var prevPC = cpu.PC;
        RunOpcode(BRA_A(new Int24(src)));
        cpu.PC.Is((uint)(prevPC + src));
    }

    [Test]
    public void BRA_L_Test([Random(-2147483648, 2147483647, 50)] int src, [Random(1, 15, 3)] byte reg) {
        var prevPC = cpu.PC;
        cpu.Registers[reg] = (uint)src;
        RunOpcode(BRA_L((Reg)reg));
        cpu.PC.Is((uint)(prevPC + src));
    }

    // [Test]
    public void BRK_Test() {
        throw new NotImplementedException();
        // TODO 実装する
    }

    [Test]
    [TestCase(     0u,  25, 1u << 25)]
    [TestCase(1u << 8,   8, 1u <<  8)]
    public void BSET_imm5_Test(uint a, byte b, uint result) {
        cpu.Registers[1] = a;
        RunOpcode(BSET(new UInt5(b), R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase(0u, 0u, 0x01u)]
    [TestCase(1u, 0u, 0x02u)]
    [TestCase(31u, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase(31u, 0x7FFF_FFFFu, 0xFFFF_FFFFu)]
    public void BSET_rr_Test(uint a, uint b, uint result) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(BSET(R1, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    public void BSR_W_Test([Random(10)]short src) {
        var prevPC = cpu.PC;
        RunOpcode(BSR_W(src));
        memory.Read(cpu.SP, 4).Is(prevPC + 3);
        cpu.PC.Is((uint)(prevPC + src));
    }

    [Test]
    public void BSR_A_Test([Random(-8388608, 8388607, 10)]int src) {
        var prevPC = cpu.PC;
        RunOpcode(BSR_A(new Int24(src)));
        memory.Read(cpu.SP, 4).Is(prevPC + 4);
        cpu.PC.Is((uint)(prevPC + src));
    }

    [Test]
    public void BSR_L_Test([Random(10)]int src, [Random(1, 15, 5)] byte reg) {
        var prevPC = cpu.PC;
        cpu.Registers[reg] = (uint)src;
        RunOpcode(BSR_L((Reg)reg));
        memory.Read(cpu.SP, 4).Is(prevPC + 2);
        cpu.PC.Is((uint)(prevPC + src));
    }

    [Test]
    [TestCase(1 ,    0xFFu,  true, false)]
    [TestCase(25,       0u, false,  true)]
    [TestCase(25, 1u << 25,  true, false)]
    public void BTST_imm5_Test(byte a, uint b, bool expC, bool expZ) {
        cpu.Registers[1] = b;
        RunOpcode(BTST(new UInt5(a), R1));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
    }

    [Test]
    [TestCase(PSWFlag.O, false, true , true , true , true, true)]
    [TestCase(PSWFlag.S, true , false, true , true , true, true)]
    [TestCase(PSWFlag.Z, true , true , false, true , true, true)]
    [TestCase(PSWFlag.C, true , true , true , false, true, true)]
    // U と I に対してユーザーモードとスーパーバイザーの両方のテストを追加する
    // [TestCase(PSWFlag., true, true, true, true, true, true)]
    // [TestCase(PSWFlag., true, true, true, true, true, true)]
    public void CLRPSW_Test(
        PSWFlag targetBit,
        bool expO, bool expS, bool expZ, bool expC, bool expU, bool expI) {

        cpu.PSW_o = true;
        cpu.PSW_s = true;
        cpu.PSW_z = true;
        cpu.PSW_c = true;
        cpu.PSW_u = true;
        cpu.PSW_i = true;

        RunOpcode(CLRPSW(targetBit));

        cpu.PSW_o.Is(expO);
        cpu.PSW_s.Is(expS);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_c.Is(expC);
        cpu.PSW_u.Is(expU);
        cpu.PSW_i.Is(expI);
    }

    [Test]
    [TestCase(0u, 0u, true,  true, false, false)]
    [TestCase(1u, 5u, true, false, false, false)]
    public void CMP_Test(uint a, uint b,
                         bool expC, bool expZ, bool expS, bool expO) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(CMP(R1, R2));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(10u, 400u,  40u, false)]
    [TestCase(  5u,  0u,   0u, false)]
    [TestCase(  0u,  5u, null,  true)]
    [TestCase(99u, unchecked((uint)-2000), unchecked((uint)-20u), false)]
    public void DIV_Test(uint a, uint b, uint? result, bool expO) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(DIV(R1, R2));
        if (result.HasValue) {
            cpu.Registers[2].Is(result.Value);
        }
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(10u, 400u, 40u, false)]
    [TestCase(0u, 5u, null, true)]
    // -2000 -> 0xFFFFF830 -> 4294965296
    [TestCase(236u, unchecked((uint)-2000), 18199005u, false)]
    public void DIVU_Test(uint a, uint b, uint? result, bool expO) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(DIVU(R1, R2));
        if (result.HasValue) {
            cpu.Registers[2].Is(result.Value);
        }
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(1000000u, 99999u, 1000000L * 99999L)]
    [TestCase(unchecked((uint)-100000), 99999u, -100000L * 99999L)]
    public void EMUL_adr_Test(uint a, uint b, long result) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(EMUL(R1, R2));
        cpu.Registers[3].Is((uint)((ulong)result >> 32));
        cpu.Registers[2].Is(unchecked((uint)result));
    }

    [Test]
    public void EMULU_Test() {
        cpu.Registers[1] = 1000000;
        cpu.Registers[2] = 99999;
        RunOpcode(EMULU(R1, R2));
        long result = 1000000L * 99999L;
        cpu.Registers[3].Is((uint)(result >> 32));
        cpu.Registers[2].Is(unchecked((uint)result));
    }

    [Test]
    public void FADD_Test() {
        cpu.Registers[1] = BitConverter.SingleToUInt32Bits(3.55f);
        cpu.Registers[2] = BitConverter.SingleToUInt32Bits(0.5f);
        RunOpcode(FADD(R1, R2));
        cpu.Registers[2].Is(BitConverter.SingleToUInt32Bits(4.05f));
    }

    [Test]
    [TestCase(5.5f, 5.3f, false, true, false)]
    [TestCase(5.3f, 5.5f, false, false, false)]
    [TestCase(5.3f, 5.3f, false, false, true)]
    public void FCMP_Test(float a, float b,
                          bool expO, bool expS, bool expZ) {
        cpu.Registers[1] = BitConverter.SingleToUInt32Bits(a);
        cpu.Registers[2] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FCMP(R1, R2));
        cpu.PSW_o.Is(expO);
        cpu.PSW_s.Is(expS);
        cpu.PSW_z.Is(expZ);
    }

    [Test]
    [TestCase( 2.0f, 6.0f,3.0f)]
    [TestCase( 2.0f, 1.0f,0.5f)]
    [TestCase( -2.0f, 1.0f, -0.5f)]
    [TestCase(2.0f, -1.0f, -0.5f)]
    [TestCase(-2.0f, -1.0f, 0.5f)]
    public void FDIV_Test(float a, float b, float result) {
        cpu.Registers[1] = BitConverter.SingleToUInt32Bits(a);
        cpu.Registers[2] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FDIV(R1, R2));
        cpu.Registers[2].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase( 100.0f,  15.34f,  1534.0f)]
    [TestCase(-100.0f,  15.34f, -1534.0f)]
    [TestCase( 100.0f, -15.34f, -1534.0f)]
    [TestCase(-100.0f, -15.34f,  1534.0f)]
    public void FMUL_Test(float a, float b, float result) {
        cpu.Registers[1] = BitConverter.SingleToUInt32Bits(a);
        cpu.Registers[2] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FMUL(R1, R2));
        cpu.Registers[2].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase(0.5f,  5.5f, 5.0f)]
    [TestCase(-0.5f, 5.5f, 6.0f)]
    [TestCase(0.5f, -5.5f, -6.0f)]
    public void FSUB_Test(float a, float b, float result) {
        cpu.Registers[1] = BitConverter.SingleToUInt32Bits(a);
        cpu.Registers[2] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FSUB(R1, R2));
        cpu.Registers[2].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase(10.6f, 10)]
    [TestCase(10.4f, 10)]
    [TestCase(9.99f, 9)]
    [TestCase(-9.99f, -9)]
    public void FTOI_Test(float a, int result) {
        cpu.Registers[1] = BitConverter.SingleToUInt32Bits(a);
        RunOpcode(FTOI(R1, R2));
        cpu.Registers[2].Is(unchecked((uint)result));
    }

    //[Test]
    public void INT_Test() {
        throw new NotImplementedException();
    }

    [Test]
    [TestCase(5, 5.0f)]
    [TestCase(-5, -5.0f)]
    public void ITOF_Test(int a, float result) {
        cpu.Registers[1] = unchecked((uint)a);
        RunOpcode(ITOF(R1, R2));
        cpu.Registers[2].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    public void JMP_Test([Range(1, 15)] byte reg, [Random(10)] uint address) {
        cpu.Registers[reg] = address;
        RunOpcode(JMP((Reg)reg));
        cpu.PC.Is(address);
    }

    [Test]
    public void JSR_Test([Range(1, 15)] byte reg, [Random(10)] uint address) {
        uint prevPC = cpu.PC;
        cpu.Registers[reg] = address;
        RunOpcode(JSR((Reg)reg));
        cpu.PC.Is(address);
        memory.Read(cpu.SP, 4).Is(prevPC + 2);
    }

    [Test]
    [TestCase(10, 20, 100, (2000 << 16) + 10)]
    [TestCase(-100000, -2000, 500, (-1000000L << 16) - 100000)]
    public void MACHI_Test(long acc, short a, short b, long result) {
        cpu.Acc = unchecked((ulong)acc);
        cpu.Registers[1] = (uint)a << 16;
        cpu.Registers[2] = (uint)b << 16;
        RunOpcode(MACHI(R1, R2));
        cpu.Acc.Is(unchecked((ulong)result));
    }

    [Test]
    [TestCase(10, 20, 100, (2000 << 16) + 10)]
    [TestCase(-100000, -2000, 500, ((-1000000L) << 16) -100000)]
    public void MACLO_Test(long acc, short a, short b, long result) {
        cpu.Acc = unchecked((ulong)acc);
        cpu.Registers[1] = (uint)a;
        cpu.Registers[2] = (uint)b;
        RunOpcode(MACLO(R1, R2));
        cpu.Acc.Is(unchecked((ulong)result));
    }

    [Test]
    [TestCase(1000000, -5, 1000000)]
    [TestCase(-150000, -100, -100)]
    public void MAX_Test(int a, int b, int result) {
        cpu.Registers[1] = unchecked((uint)a);
        cpu.Registers[2] = unchecked((uint)b);
        RunOpcode(MAX(R1, R2));
        cpu.Registers[2].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(1000000, -5, -5)]
    [TestCase(-150000, -100, -150000)]
    [TestCase(200, 3, 3)]
    public void MIN_Test(int a, int b, int result) {
        cpu.Registers[1] = unchecked((uint)a);
        cpu.Registers[2] = unchecked((uint)b);
        RunOpcode(MIN(R1, R2));
        cpu.Registers[2].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(0xFFu, 0xFFu, MemEx.L)]
    [TestCase(0xAABBCCDDu, 0xAABBCCDDu, MemEx.L)]
    [TestCase(0x11223344u, 0x3344u, MemEx.W)]
    [TestCase(0x11223344u, 0x44u, MemEx.B)]
    public void MOV_rm_Test(
        uint value,
        uint expected,
        MemEx sz) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.NextByte(1, 6);
        var rd = random.NextByte((byte)(rs + 1), 7);
        var dspOffset = random.NextByte(0, 31);
        var dsp = new RegAddressing5(dspOffset, (Reg)rd);
        cpu.Registers[rs] = value;
        RunOpcode(MOV(sz, (Reg)rs, dsp));
        LoadData(cpu.Registers, busManager, LengthOfDisplacement.DSP8Reg, (uint)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expected);
    }

    [Test]
    [TestCase(0x11u, 0x11u, MemEx.L)]
    [TestCase(0xAABBCCDDu, 0xAABBCCDDu, MemEx.L)]
    [TestCase(0xFFFFu, 0xFFFFFFFFu, MemEx.W)]
    [TestCase(0x12347FFFu, 0x7FFFu, MemEx.W)]
    [TestCase(0xFFu, 0xFFFFFFFFu, MemEx.B)]
    [TestCase(0xFFFFFF7Fu, 0x7Fu, MemEx.B)]
    [TestCase(0x34u, 0x34u, MemEx.B)]
    public void MOV_mr_Test(
        uint value,
        uint expected,
        MemEx sz) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.NextByte(1, 7);
        var rd = random.NextByte(1, 7);
        var dspOffset = random.NextByte(0, 31);
        var dsp = new RegAddressing5(dspOffset, (Reg)rs);
        StoreRandomDest(new RelRef8(dsp.Displacement, dsp.TargetReg, MemEx.L), 0, 300, value);
        RunOpcode(MOV(sz, dsp, (Reg)rd));
        cpu.Registers[rd].Is(expected);
    }

    [Test]
    public void MOV_imm4_reg_Test(
        [Random((byte)0, (byte)15, 5)]byte imm,
        [Random((byte)1, (byte)15, 5)]byte reg) {
        RunOpcode(MOV(new UInt4(imm), (Reg)reg));
        cpu.Registers[reg].Is(imm);
    }

    [Test]
    [TestCase(MemEx.B, (byte)0xFFu, 0xFFu)]
    [TestCase(MemEx.B, (byte)0x7Fu, 0x7Fu)]
    [TestCase(MemEx.W, (byte)0xFFu, 0xFFu)]
    [TestCase(MemEx.L, (byte)0xF3u, 0xF3u)]
    public void MOV_im_Test(MemEx sz, byte src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.NextByte(1, 7);
        var dspOffset = random.NextByte(0, 31);
        var initial = random.NextUInt();
        var dsp = new RegAddressing5(dspOffset, (Reg)rd);
        // mov命令は指定したサイズで上書き出来ていることを確認するため、乱数を設定する
        StoreDestOperand(cpu.Registers, busManager, MemEx.L, dsp.TargetReg, LengthOfDisplacement.DSP8Reg, dsp.Displacement, initial);
        RunOpcode(MOV(sz, src, dsp));
        LoadData(cpu.Registers, busManager, LengthOfDisplacement.DSP8Reg, (uint)sz, dsp.Displacement, dsp.TargetReg)
        .Is(expected);
    }

    [Test]
    public void MOV_u8ir_Test([Random(byte.MinValue, byte.MaxValue, 5)]byte src) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.NextByte(1, 15);
        // 上書きされることを確認するために適当な値で埋める
        cpu.Registers[rd] = random.NextUInt();
        RunOpcode(MOV(src, (Reg)rd));
        cpu.Registers[rd].Is(src);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 0xFFu, 0xFFFF_FFFFu)]
    [TestCase(LengthOfImmediate.SIMM8, 0x7Fu, 0x7Fu)]
    [TestCase(LengthOfImmediate.SIMM16, 0xF567u, 0xFFFF_F567u)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7123u, 0x7123u)]
    [TestCase(LengthOfImmediate.SIMM24, 0x80_1234u, 0xFF80_1234u)]
    [TestCase(LengthOfImmediate.SIMM24, 0x71_2345u, 0x71_2345u)]
    [TestCase(LengthOfImmediate.IMM32, 0xFF71_2345u, 0xFF71_2345u)]
    [TestCase(LengthOfImmediate.IMM32, 0x7065_4321u, 0x7065_4321u)]
    public void MOV_ir_Test(LengthOfImmediate li, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.NextByte(1, 15);
        // 上書きされることを確認するために適当な値で埋める
        cpu.Registers[rd] = random.NextUInt();
        var imm = new StdImmValue(li, src);
        RunOpcode(MOV(imm, (Reg)rd));
        cpu.Registers[rd].Is(expected);
    }

    [Test]
    [TestCase(MemEx.B, 0xFFFF_FF00u, 0x0u)]
    [TestCase(MemEx.B, 0xFFu, 0xFFFF_FFFFu)]
    [TestCase(MemEx.B, 0x7Fu, 0x7Fu)]
    [TestCase(MemEx.B, 0x80u, 0xFFFF_FF80u)]
    [TestCase(MemEx.B, 0x1234_5678u, 0x78u)]
    [TestCase(MemEx.W, 0xFFFFu, 0xFFFF_FFFFu)]
    [TestCase(MemEx.W, 0x7FFFu, 0x7FFFu)]
    [TestCase(MemEx.W, 0x8000u, 0xFFFF_8000u)]
    [TestCase(MemEx.W, 0xFFFF_0000u, 0x0u)]
    [TestCase(MemEx.W, 0x1234_5678u, 0x5678u)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase(MemEx.L, 0x0u, 0x0u)]
    [TestCase(MemEx.L, 0x1234_5678u, 0x1234_5678u)]
    public void MOV_rr_Test(MemEx size, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.NextByte(1, 14);
        var rd = random.NextByte((byte)(rs + 1), 15);
        cpu.Registers[rs] = src;
        // 上書きされることを確認するために適当な値で埋める
        cpu.Registers[rd] = random.NextUInt();
        RunOpcode(MOV(size, (Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(expected);
    }

    [Test]
    [TestCase(MemEx.B, 0x00u, LengthOfImmediate.SIMM8, 0xFFu, 0xFFu)]
    [TestCase(MemEx.B, 0x00u, LengthOfImmediate.SIMM8, 0x80u, 0x80u)]
    [TestCase(MemEx.B, 0x1234_5678u, LengthOfImmediate.SIMM8, 0xFFu, 0x1234_56FFu)]
    [TestCase(MemEx.W, 0x00u, LengthOfImmediate.SIMM8, 0xF1u, 0xFFF1u)]
    [TestCase(MemEx.W, 0x00u, LengthOfImmediate.SIMM8, 0x71u, 0x71u)]
    [TestCase(MemEx.W, 0xFFFF_FFFFu, LengthOfImmediate.SIMM8, 0x0u, 0xFFFF_0000u)]
    [TestCase(MemEx.W, 0x00u, LengthOfImmediate.SIMM16, 0xF1u, 0xF1u)]
    [TestCase(MemEx.W, 0x00u, LengthOfImmediate.SIMM16, 0xF123u, 0xF123u)]
    [TestCase(MemEx.W, 0xFFFF_FFFFu, LengthOfImmediate.SIMM16, 0x0u, 0xFFFF_0000u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.SIMM8, 0x80u, 0xFFFF_FF80u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.SIMM8, 0x70u, 0x70u)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, LengthOfImmediate.SIMM8, 0x00u, 0x00u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.SIMM16, 0x7123u, 0x7123u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.SIMM16, 0x8ABCu, 0xFFFF_8ABCu)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, LengthOfImmediate.SIMM16, 0x0u, 0x0u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.SIMM24, 0x75_1234u, 0x75_1234u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.SIMM24, 0xF5_1234u, 0xFFF5_1234u)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, LengthOfImmediate.SIMM24, 0x0u, 0x0u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.IMM32, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(MemEx.L, 0x00u, LengthOfImmediate.IMM32, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, LengthOfImmediate.IMM32, 0x0u, 0x0u)]
    public void MOV_im_Test(MemEx size, uint initial, LengthOfImmediate li, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var imm = new StdImmValue(li, src);
        var rd = random.NextByte(1, 15);
        var ld = (LengthOfDisplacement)random.Next(0, 2);
        var dsp = GetRandomStdRegAddressing((Reg)rd, size, ld);
        StoreDestOperand(cpu.Registers, busManager, MemEx.L, dsp.TargetReg, dsp.LD, dsp.Displacement, initial);
        RunOpcode(MOV(size, imm, dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expected);
    }

    [Test]
    public void MOVU_dsp5_reg_Test(
        [Values(MemEx.B, MemEx.W)] MemEx size,
        [Random((uint)ushort.MinValue, (uint)ushort.MaxValue, 3)] uint value,
        [Random(0u, 260_000u, 2)] uint addr,
        [Random((byte)0, (byte)31, 3)]byte dsp_offset,
        [Random(1, 7, 3)]int dsp_reg,
        [Random(1, 7, 2)]int rd) {
        var dsp = new RegAddressing5(dsp_offset, (Reg)dsp_reg);
        cpu.Registers[(int)dsp.TargetReg] = addr;
        StoreDestOperand(cpu.Registers, busManager, MemEx.L, dsp.TargetReg, LengthOfDisplacement.DSP8Reg, dsp.Displacement, value);
        RunOpcode(MOVU(size, dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitOperation.GetLowerBits(value, GetSize(size)));
    }

    [Test]
    public void MOVU_stdAddr_reg_Test(
        [Values(MemEx.B, MemEx.W)] MemEx size,
        [Random((uint)ushort.MinValue, (uint)ushort.MaxValue, 3)] uint value,
        [Range(0, 3)]int ld,
        [Random(1, 7, 2)]int rd) {
        var random = TestContext.CurrentContext.Random;
        var reg = random.Next(1, 15);
        var dsp = GetRandomStdRegAddressing((Reg)reg, size, (LengthOfDisplacement)ld);
        StoreRandomDest(dsp, 0, 300, value);
        RunOpcode(MOVU(size, dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitOperation.GetLowerBits(value, GetSize(size)));
    }

    [Test]
    [TestCase(unchecked((uint)-8), 7u, unchecked((uint)-56))]
    [TestCase(22u, 1000u, 22000u)]
    public void MUL_Test(uint a, uint b, uint result) {
        cpu.Registers[1] = unchecked((uint)a);
        cpu.Registers[2] = unchecked((uint)b);
        RunOpcode(MUL(R1, R2));
        cpu.Registers[2].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(9000, 8000, 72000000)]
    [TestCase(-9000, 8000, -72000000)]
    [TestCase(-9000, -9000, 81000000)]
    public void MULHI_Test(short a, short b, long result) {
        cpu.Registers[1] = unchecked((uint)a << 16);
        cpu.Registers[2] = unchecked((uint)b << 16);
        RunOpcode(MULHI(R1, R2));
        cpu.Acc.Is(unchecked((ulong)result << 16));
    }

    [Test]
    [TestCase( 9000,  8000,  72000000)]
    [TestCase(-9000,  8000, -72000000)]
    [TestCase(-9000, -9000,  81000000)]
    public void MULLO_Test(short a, short b, long result) {
        cpu.Registers[1] = unchecked((uint)a);
        cpu.Registers[2] = unchecked((uint)b);
        RunOpcode(MULLO(R1, R2));
        cpu.Acc.Is(unchecked((ulong)result << 16));
    }

    [Test]
    [TestCase(0xFFFFFFFF_00000000u, 0xFFFFFFFFu)]
    [TestCase(0x00000000_FFFFFFFFu, 0x00000000u)]
    [TestCase(0x12345678_FFFFFFFFu, 0x12345678u)]
    public void MVFACHI_Test(ulong acc, uint result) {
        cpu.Acc = acc;
        RunOpcode(MVFACHI(R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(0x0000_FFFFFFFF_0000u, 0xFFFFFFFFu)]
    [TestCase(0xFFFF_00000000_FFFFu, 0x00000000u)]
    [TestCase(0xFFFF_12345678_0000u, 0x12345678u)]
    public void MVFACMI_Test(ulong acc, uint result) {
        cpu.Acc = acc;
        RunOpcode(MVFACMI(R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(1u, ControlReg.PSW  , 1u)]
    [TestCase(2u, ControlReg.USP  , 2u)]
    [TestCase(3u, ControlReg.FPSW , 3u)]
    [TestCase(4u, ControlReg.BPSW , 4u)]
    [TestCase(5u, ControlReg.ISP  , 5u)]
    [TestCase(6u, ControlReg.FINTV, 6u)]
    [TestCase(7u, ControlReg.INTB , 7u)]
    public void MVFC_Test(uint input, ControlReg a, uint result) {
        switch (a)
        {
            case ControlReg.PSW  : cpu.PSW   = input; break;
            case ControlReg.USP  : cpu.USP   = input; break;
            case ControlReg.FPSW : cpu.FPSW  = input; break;
            case ControlReg.BPSW : cpu.BPSW  = input; break;
            case ControlReg.BPC  : cpu.BPC   = input; break;
            case ControlReg.ISP  : cpu.ISP   = input; break;
            case ControlReg.FINTV: cpu.FINTV = input; break;
            case ControlReg.INTB : cpu.INTB  = input; break;
            default: Assert.Fail(); break;
        }

        RunOpcode(MVFC(a, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(0x12345678u, 0x12345678_00000000u)]
    [TestCase(0xFFFFFFFFu, 0xFFFFFFFF_00000000u)]
    [TestCase(0u, 0u)]
    public void MVTACHI_Test(uint a, ulong result) {
        cpu.Registers[1] = a;
        RunOpcode(MVTACHI(R1));
        cpu.Acc.Is(result);
    }

    [Test]
    [TestCase(0x12345678u, 0x00000000_12345678u)]
    [TestCase(0xFFFFFFFFu, 0x00000000_FFFFFFFFu)]
    [TestCase(0u, 0u)]
    public void MVTACLO_Test(uint a, ulong result) {
        cpu.Registers[1] = a;
        RunOpcode(MVTACLO(R1));
        cpu.Acc.Is(result);
    }

    [Test]
    [TestCase(1u, ControlReg.PSW  , 1u)]
    [TestCase(2u, ControlReg.USP  , 2u)]
    [TestCase(3u, ControlReg.FPSW , 3u)]
    [TestCase(4u, ControlReg.BPSW , 4u)]
    [TestCase(5u, ControlReg.BPC  , 5u)]
    [TestCase(6u, ControlReg.ISP  , 6u)]
    [TestCase(7u, ControlReg.FINTV, 7u)]
    [TestCase(8u, ControlReg.INTB , 8u)]
    public void MVTC_Test(uint a, ControlReg b, uint result) {
        cpu.Registers[1] = a;
        RunOpcode(MVTC(R1, b));
        var regValue = b switch
        {
            ControlReg.PSW => cpu.PSW,
            ControlReg.USP => cpu.USP,
            ControlReg.FPSW => cpu.FPSW,
            ControlReg.BPSW => cpu.BPSW,
            ControlReg.BPC => cpu.BPC,
            ControlReg.ISP => cpu.ISP,
            ControlReg.FINTV => cpu.FINTV,
            ControlReg.INTB => cpu.INTB,
            _ => throw new ArgumentException(b.ToString())
        };
        regValue.Is(result);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(14)]
    [TestCase(15)]
    public void MVTIPL_Test(byte a) {
        // TODO ユーザーモードの時に特権命令例外が発生することを確認する
        cpu.PSW_pm = true;
        RunOpcode(MVTIPL(new UInt4(a)));
        cpu.PSW_ipl.Is(a);
    }

    [Test]
    [TestCase(                 5u,             unchecked((uint)-5), false, false,  true, false)]
    [TestCase(unchecked((uint)-5),                              5u, false, false, false, false)]
    [TestCase(          90000000u,      unchecked((uint)-90000000), false, false,  true, false)]
    [TestCase(                 0u,                              0u,  true,  true, false, false)]
    [TestCase(        0x80000000u,   unchecked((uint)-0x80000000u), false, false,  true,  true)]
    public void NEG_Test(uint a, uint result,
                         bool expC, bool expZ, bool expS, bool expO) {
        cpu.Registers[1] = a;
        RunOpcode(NEG(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);

    }

    [Test]
    public void NOP_Test() {
        var prevPC = cpu.PC;
        RunOpcode(NOP);
        cpu.PC.Is(prevPC + 1);
    }

    [Test]
    [TestCase(          0u, 0xFFFF_FFFFu, false,  true)]
    [TestCase(0xFFFF_FFFFu,           0u,  true, false)]
    [TestCase(0x7000_0000u, 0x8FFF_FFFFu, false,  true)]
    public void NOT_Test(uint a, uint result,
                         bool expZ, bool expS) {
        cpu.Registers[1] = a;
        RunOpcode(NOT(R1));
        cpu.Registers[1].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(0xFFFF_FFFFu,           0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(0x7F00_00FFu, 0x00FF_70FFu, 0x7FFF_70FFu, false, false)]
    [TestCase(          0u,           0u,           0u,  true, false)]
    public void OR_Test(uint a, uint b, uint result,
                        bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(OR(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    public void POP_Test([Range(1, 15)]byte src, [Random(10)]uint value) {
        var prevSP = cpu.SP;
        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, value);
        RunOpcode(POP((Reg)src));
        cpu.Registers[src].Is(value);
        cpu.SP.Is(prevSP);
    }

    [Test]
    [TestCase(1u, ControlReg.PSW  , 1u)]
    [TestCase(2u, ControlReg.USP  , 2u)]
    [TestCase(3u, ControlReg.FPSW , 3u)]
    [TestCase(4u, ControlReg.BPSW , 4u)]
    [TestCase(5u, ControlReg.BPC  , 5u)]
    [TestCase(6u, ControlReg.ISP  , 6u)]
    [TestCase(7u, ControlReg.FINTV, 7u)]
    [TestCase(8u, ControlReg.INTB , 8u)]
    public void POPC_Test(uint value, ControlReg src, uint result) {
        switch (src)
        {
            case ControlReg.ISP:
                cpu.PSW_u = false;
                break;
            case ControlReg.USP:
                cpu.PSW_u = true;
                break;
        }
        cpu.SP = ramEndAddress;
        var prevSP = cpu.SP;
        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, value);
        RunOpcode(POPC(src));
        var regValue = src switch
        {
            ControlReg.PSW => cpu.PSW,
            ControlReg.USP => cpu.USP,
            ControlReg.FPSW => cpu.FPSW,
            ControlReg.BPSW => cpu.BPSW,
            ControlReg.BPC => cpu.BPC,
            ControlReg.ISP => cpu.ISP,
            ControlReg.FINTV => cpu.FINTV,
            ControlReg.INTB => cpu.INTB,
            _ => throw new ArgumentException(src.ToString())
        };
        regValue.Is(result);
        switch (src) {
            case ControlReg.ISP:
            case ControlReg.USP:
                cpu.SP.Is(value);
                break;
            default:
                cpu.SP.Is(prevSP);
                break;
        }
    }

    [Test]
    public void POPM_Test([Random(1, 15, 5)] byte regBegin, [Random(1, 14, 5)] byte length) {
        var regEnd = Math.Min(15, regBegin + length);
        var randomValues = new uint[16];
        for (int i = regBegin; i <= regEnd; i++) {
            randomValues[i] = TestContext.CurrentContext.Random.NextUInt();
        }
        for (int i = regEnd; regBegin <= i; i--) {
            cpu.SP -= 4;
            busManager.Write(cpu.SP, 4, randomValues[i]);
        }
        RunOpcode(POPM((Reg)regBegin, (Reg)regEnd));
        cpu.Registers.Skip(1).Is(randomValues.Skip(1));
    }

    [Test]
    public void PUSH_r_Test([Random(0, 2, 2)]byte size, [Random(1, 15, 5)]byte reg, [Random(5)] uint value) {
        value = size switch {
            0 => value & 0xFF,
            1 => value & 0XFFFF,
            _ => value
        };
        cpu.Registers[reg] = value;
        var prevSP = cpu.SP;
        RunOpcode(PUSH((MemEx)size, (Reg)reg));
        cpu.SP.Is((uint)(prevSP - 4));
        busManager.Read(cpu.SP, 1 << size).Is(value);
    }

    [Test]
    [TestCase(1u, ControlReg.PSW  , 1u)]
    [TestCase(100u, ControlReg.USP  , 100u)]
    [TestCase(3u, ControlReg.FPSW , 3u)]
    [TestCase(4u, ControlReg.BPSW , 4u)]
    [TestCase(5u, ControlReg.BPC  , 5u)]
    [TestCase(6u, ControlReg.ISP  , 6u)]
    [TestCase(7u, ControlReg.FINTV, 7u)]
    [TestCase(8u, ControlReg.INTB , 8u)]
    public void PUSHC_Test(uint value, ControlReg src, uint result) {
        switch (src)
        {
            case ControlReg.ISP:
                cpu.PSW_u = false;
                break;
            case ControlReg.USP:
                cpu.PSW_u = true;
                break;
        }
        switch (src) {
            case ControlReg.PSW:
                cpu.PSW = value;
                break;
            case ControlReg.USP:
                cpu.USP = value;
                break;
            case ControlReg.FPSW:
                cpu.FPSW = value;
                break;
            case ControlReg.BPSW:
                cpu.BPSW  = value;
                break;
            case ControlReg.BPC:
                cpu.BPC = value;
                break;
            case ControlReg.ISP:
                cpu.ISP = value;
                break;
            case ControlReg.FINTV:
                cpu.FINTV = value;
                break;
            case ControlReg.INTB:
                cpu.INTB = value;
                break;
            default:
                throw new ArgumentException(src.ToString());
        };
        var prevSP = cpu.SP;
        RunOpcode(PUSHC(src));
        cpu.SP.Is(prevSP - 4);
        busManager.Read(cpu.SP, 4).Is(result);
    }

    [Test]
    public void PUSHM_Test([Random(1, 15, 5)] byte regBegin, [Random(1, 14, 5)] byte length) {
        var regEnd = Math.Min(15, regBegin + length);
        var range = (regEnd - regBegin) + 1;
        cpu.Registers.AsSpan(1).Clear();
        var randomValues = new uint[16];
        for (int i = regBegin; i <= regEnd; i++) {
            var randomValue = TestContext.CurrentContext.Random.NextUInt();
            randomValues[i] = randomValue;
            cpu.Registers[i] = randomValue;
        }
        var prevSP = cpu.SP;
        RunOpcode(PUSHM((Reg)regBegin, (Reg)regEnd));
        cpu.SP.Is((uint)(prevSP - 4 * range));
        var sp = cpu.SP;
        for (int i = regBegin; i <= regEnd; i++, sp += 4) {
            busManager.Read(sp, 4).Is(randomValues[i]);
        }
    }

    [Test]
    [TestCase(0x0000_3FFF_4000_0000u, 1, 0x0000_7FFF_0000_0000u)]
    [TestCase(0xFFFF_C000_3FFF_FFFFu, 1, 0xFFFF_8000_0000_0000u)]
    public void RACW_Test(ulong acc, byte a, ulong result) {
        cpu.Acc = acc;
        RunOpcode(RACW(a));
        cpu.Acc.Is(result);
    }

    [Test]
    [TestCase(0x1234_5678u, 0x7856_3412u)]
    [TestCase(0x9876_5432u, 0x3254_7698u)]
    public void REVL_Test(uint a, uint result) {
        cpu.Registers[1] = a;
        RunOpcode(REVL(R1, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(0x1234_5678u, 0x3412_7856u)]
    [TestCase(0x9876_5432u, 0x7698_3254u)]
    public void REVW_Test(uint a, uint result) {
        cpu.Registers[1] = a;
        RunOpcode(REVW(R1, R2));
        cpu.Registers[2].Is(result);
    }

    [Test]
    [TestCase(false, 0x8000_0001u, 0x0000_0002u,  true, false, false)]
    [TestCase( true, 0x8000_0001u, 0x0000_0003u,  true, false, false)]
    [TestCase( true, 0xF000_0000u, 0xE000_0001u,  true, false,  true)]
    [TestCase( true, 0x7000_0000u, 0xE000_0001u, false, false,  true)]
    [TestCase(false, 0x8000_0000u, 0x0000_0000u,  true,  true, false)]
    public void RPLC_Test(bool psw_c, uint a, uint result, bool expC, bool expZ, bool expS) {
        cpu.PSW_c = psw_c;
        cpu.Registers[1] = a;
        RunOpcode(ROLC(R1));
        cpu.Registers[1].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(false, 0x0000_0001u, 0x0000_0000u,  true,  true, false)]
    [TestCase( true, 0x0000_0001u, 0x8000_0000u,  true, false,  true)]
    [TestCase( true, 0x0000_0000u, 0x8000_0000u, false, false,  true)]
    [TestCase(false, 0xF000_000Eu, 0x7800_0007u, false, false, false)]
    public void RORC_Test(bool psw_c, uint a, uint result, bool expC, bool expZ, bool expS) {
        cpu.PSW_c = psw_c;
        cpu.Registers[1] = a;
        RunOpcode(RORC(R1));
        cpu.Registers[1].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( 8, 0xFF00_0001u, 0x0000_01FFu,  true, false, false)]
    [TestCase(16, 0xFF00_0001u, 0x0001_FF00u, false, false, false)]
    [TestCase( 1, 0xF000_0000u, 0xE000_0001u,  true, false,  true)]
    [TestCase( 0, 0x1234_5678u, 0x1234_5678u, false, false, false)]
    [TestCase(31, 0x0000_0000u, 0x0000_0000u, false,  true, false)]
    public void ROTL_imm_Test(byte a, uint b, uint result,
                          bool expC, bool expZ, bool expS) {
        cpu.Registers[1] = b;
        RunOpcode(ROTL(new UInt5(a), R1));
        cpu.Registers[1].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( 8u, 0xFF00_0001u, 0x0000_01FFu,  true, false, false)]
    [TestCase(16u, 0xFF00_0001u, 0x0001_FF00u, false, false, false)]
    [TestCase( 1u, 0xF000_0000u, 0xE000_0001u,  true, false,  true)]
    [TestCase( 0u, 0x1234_5678u, 0x1234_5678u, false, false, false)]
    [TestCase(31u, 0x0000_0000u, 0x0000_0000u, false,  true, false)]
    [TestCase(0xE0u, 0x7FFF_FFFEu, 0x7FFF_FFFEu, false,  false, false)]
    [TestCase(0xE0u, 0xFFFF_FFFEu, 0xFFFF_FFFEu, false,  false, true)]
    public void ROTL_rr_Test(uint a, uint b, uint result,
                          bool expC, bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(ROTL(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( 8u, 0x0100_00FFu, 0xFF01_0000u,  true, false,  true)]
    [TestCase(16u, 0x0100_00FFu, 0x00FF_0100u, false, false, false)]
    [TestCase( 1u, 0x0000_000Fu, 0x8000_0007u,  true, false,  true)]
    [TestCase( 0u, 0x1234_5678u, 0x1234_5678u, false, false, false)]
    [TestCase(31u, 0x0000_0000u, 0x0000_0000u, false,  true, false)]
    public void ROTR_Test(uint a, uint b, uint result,
                          bool expC, bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(ROTR(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    // [Test]
    // [TestCase(RXFloatRoundingMode.)]
    public void ROUND_Test(RXFloatRoundingMode rm, float a, uint result) {
        cpu.Registers[1] = BitConverter.SingleToUInt32Bits(a);
        cpu.FPSW_rm = rm;
        RunOpcode(ROUND(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.FPSW_co.Is(false);
        cpu.FPSW_cz.Is(false);
        cpu.FPSW_cu.Is(false);
    }

    [Test]
    public void RTE_Test([Random(5)]uint pc, [Values(1u, 2u)] uint psw) {
        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, psw);
        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, pc);
        RunOpcode(RTE);
        cpu.PC.Is(pc);
        cpu.PSW.Is(psw);
    }

    [Test]
    public void RTFI_Test([Random(5)]uint pc, [Values(1u, 2u)] uint psw) {
        cpu.BPSW = psw;
        cpu.BPC = pc;
        RunOpcode(RTFI);
        cpu.PC.Is(pc);
        cpu.PSW.Is(psw);
    }

    [Test]
    public void RTS_Test([Random(5)]uint pc) {
        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, pc);
        RunOpcode(RTS);
        cpu.PC.Is(pc);
    }

    [Test]
    public void RTSD_imm_Test([Random(5)]byte src, [Random(5)] uint pc) {
        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, pc);
        cpu.SP -= src;
        RunOpcode(RTSD(src));
        cpu.PC.Is(pc);
    }

    [Test]
    public void RTSD_range_Test(
        [Random(5)] uint pc,
        [Random(60, 255, 5)]byte src,
        [Random(1, 15, 5)] byte regBegin,
        [Random(1, 14, 5)] byte length
        ) {
        var regEnd = Math.Min(15, regBegin + length);

        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, pc);

        cpu.Registers.AsSpan(1).Clear();
        var randomValues = new uint[16];
        for (int i = regBegin; i <= regEnd; i++) {
            var randomValue = TestContext.CurrentContext.Random.NextUInt();
            randomValues[i] = randomValue;
        }
        var sp = cpu.SP;
        for (int i = regEnd; regBegin <= i; i--) {
            sp -= 4;
            busManager.Write(sp, 4, randomValues[i]);
        }

        cpu.SP -= src;
        RunOpcode(RTSD(src, (Reg)regBegin, (Reg)regEnd));
        cpu.PC.Is(pc);
        cpu.Registers.AsSpan(1).ToArray().Is(randomValues.AsSpan(1).ToArray());
    }

    [Test]
    [TestCase(false, false, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase(false,  true, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase( true, false, 0xFFFF_FFFFu, 0x8000_0000u)]
    [TestCase( true,  true, 0xFFFF_FFFFu, 0x7FFF_FFFFu)]
    [TestCase(false, false, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(false,  true, 0x0000_0000u, 0x0000_0000u)]
    public void SAT_Test(bool psw_o, bool psw_s, uint a, uint result) {
        cpu.PSW_o = psw_o;
        cpu.PSW_s = psw_s;
        cpu.Registers[1] = a;
        RunOpcode(SAT(R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase(false, false, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu,
                            0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase(false,  true, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu,
                            0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase( true, false, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu,
                            0x0000_0000u, 0x7FFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase( true,  true, 0x0000_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu,
                            0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0000u)]
    [TestCase(false, false, 0x1234_5678u, 0x9ABC_DEF0u, 0x1234_5678u,
                            0x1234_5678u, 0x9ABC_DEF0u, 0x1234_5678u)]
    [TestCase(false,  true, 0x0000_0000u, 0x0000_0000u, 0x0000_0000u,
                            0x0000_0000u, 0x0000_0000u, 0x0000_0000u)]
    public void SATR_Test(bool psw_o, bool psw_s,
                          uint r6, uint r5, uint r4,
                          uint resultR6, uint resultR5, uint resultR4) {
        cpu.PSW_o = psw_o;
        cpu.PSW_s = psw_s;
        cpu.Registers[6] = r6;
        cpu.Registers[5] = r5;
        cpu.Registers[4] = r4;
        RunOpcode(SATR);
        cpu.Registers[6].Is(resultR6);
        cpu.Registers[5].Is(resultR5);
        cpu.Registers[4].Is(resultR4);
    }

    [Test]
    [TestCase(false,          100u,          1_000_905u,           1_000_804u,  true, false, false, false)]
    [TestCase( true,            0u,        0x8000_0000u,         0x8000_0000u,  true, false,  true, false)]
    [TestCase(false,            0u,        0x8000_0000u,         0x7FFF_FFFFu,  true, false, false,  true)]
    [TestCase( true,            1u,        0x8000_0000u,         0x7FFF_FFFFu,  true, false, false,  true)]
    [TestCase( true,          100u,          1_000_905u,           1_000_805u,  true, false, false, false)]
    [TestCase( true,         2000u,               2000u,                   0u,  true,  true, false, false)]
    [TestCase(false,         2000u,               2000u,  unchecked((uint)-1), false,  false, true, false)]
    [TestCase( true,  0x7FFF_FFFFu, unchecked((uint)-1),         0x8000_0000u, true,  false, true, false)]
    public void SBB_Test(
        bool psw_c, uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {
        cpu.PSW_c = psw_c;
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(SBB(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase( true, false, false, false, Cnd.GEU, 1u)]
    [TestCase(false,  true,  true,  true, Cnd.GEU, 0u)]
    [TestCase(false,  true, false, false, Cnd.EQ , 1u)]
    [TestCase( true, false,  true,  true, Cnd.EQ , 0u)]
    [TestCase( true, false, false, false, Cnd.GTU, 1u)]
    [TestCase( true, false,  true,  true, Cnd.GTU, 1u)]
    [TestCase( true,  true,  true,  true, Cnd.GTU, 0u)]
    [TestCase(false, false,  true,  true, Cnd.GTU, 0u)]
    [TestCase(false,  true,  true,  true, Cnd.GTU, 0u)]
    [TestCase(false, false,  true, false, Cnd.PZ , 0u)]
    [TestCase( true,  true, false,  true, Cnd.PZ , 1u)]
    [TestCase(false, false,  true,  true, Cnd.GE , 1u)]
    [TestCase(false, false, false, false, Cnd.GE , 1u)]
    [TestCase(false, false,  true, false, Cnd.GE , 0u)]
    [TestCase(false, false, false,  true, Cnd.GE , 0u)]
    [TestCase( true,  true, false,  true, Cnd.GE , 0u)]
    [TestCase(false, false,  true,  true, Cnd.GT , 1u)]
    [TestCase(false, false, false, false, Cnd.GT , 1u)]
    [TestCase(false, false,  true, false, Cnd.GT , 0u)]
    [TestCase(false, false, false,  true, Cnd.GT , 0u)]
    [TestCase(false,  true,  true, false, Cnd.GT , 0u)]
    [TestCase(false,  true, false,  true, Cnd.GT , 0u)]
    [TestCase( true,  true, false,  true, Cnd.GT , 0u)]
    [TestCase(false, false, false,  true, Cnd.O  , 1u)]
    [TestCase( true,  true,  true, false, Cnd.O  , 0u)]
    [TestCase(false,  true,  true,  true, Cnd.LTU, 1u)]
    [TestCase( true, false, false, false, Cnd.LTU, 0u)]
    [TestCase( true, false,  true,  true, Cnd.NE , 1u)]
    [TestCase(false,  true, false, false, Cnd.NE , 0u)]
    [TestCase(false,  true,  true,  true, Cnd.LEU, 1u)]
    [TestCase(false, false,  true,  true, Cnd.LEU, 1u)]
    [TestCase(false,  true, false, false, Cnd.LEU, 1u)]
    [TestCase( true, false, false, false, Cnd.LEU, 0u)]
    [TestCase( true, false, false, false, Cnd.LEU, 0u)]
    [TestCase( true, false, true,   true, Cnd.LEU, 0u)]
    [TestCase(false,  true,  true,  true, Cnd.LEU, 1u)]
    [TestCase(false, false, false, false, Cnd.LEU, 1u)]
    [TestCase(false, false,  true, false, Cnd.LE , 1u)]
    [TestCase(false, false, false,  true, Cnd.LE , 1u)]
    [TestCase(false,  true,  true, false, Cnd.LE , 1u)]
    [TestCase(false,  true, false,  true, Cnd.LE , 1u)]
    [TestCase(false,  true,  true,  true, Cnd.LE , 1u)]
    [TestCase( true,  true, false, false, Cnd.LE , 1u)]
    [TestCase(false, false,  true,  true, Cnd.LE , 0u)]
    [TestCase(false, false, false, false, Cnd.LE , 0u)]
    [TestCase( true, false,  true,  true, Cnd.LE , 0u)]
    [TestCase( true, false, false, false, Cnd.LE , 0u)]
    [TestCase(false, false,  true, false, Cnd.LT , 1u)]
    [TestCase(false, false, false,  true, Cnd.LT , 1u)]
    [TestCase( true,  true,  true, false, Cnd.LT , 1u)]
    [TestCase(false, false,  true,  true, Cnd.LT , 0u)]
    [TestCase(false, false, false, false, Cnd.LT , 0u)]
    [TestCase( true,  true, false, false, Cnd.LT , 0u)]
    [TestCase(false, false, false,  true, Cnd.NO , 0u)]
    [TestCase( true,  true,  true,  true, Cnd.NO , 0u)]
    [TestCase(false, false, false, false, Cnd.NO , 1u)]
    [TestCase( true,  true,  true, false, Cnd.NO , 1u)]
    public void SCCnd_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        Cnd condition, uint result) {
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        cpu.Registers[1] = 0xFF;
        RunOpcode(SCC(condition, R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    public void SCMPU_Equal_Test([Random(1, 20, 5)]int length) {
        var random = TestContext.CurrentContext.Random;
        var datas = Enumerable.Range(0, (int)length)
            .Select(_ => random.NextByte(1, 0xFF))
            .ToArray();
        var pos1 = 0u;
        var pos2 = 50u;
        memory.WriteRange(pos1, datas);
        memory.WriteRange(pos2, datas);
        var ramPos1 = ramBeginAddress + pos1;
        var ramPos2 = ramBeginAddress + pos2;
        cpu.Registers[1] = ramPos1;
        cpu.Registers[2] = ramPos2;
        cpu.Registers[3] = (uint)length;
        // フラグが変化することを確認するために逆の値を代入
        cpu.PSW_c = false;
        cpu.PSW_z = false;
        var prevPC = cpu.PC;
        for (int i = 0; i < length - 1; i++) {
            RunOpcode(SCMPU);
            // 最後まで検索するまでプログラムカウンタはインクリメントされないことを確認する
            cpu.PC.Is(prevPC);
            // 比較している位置がインクリメントされていることを確認する
            cpu.Registers[1].Is(ramPos1 + (uint)i + 1);
            cpu.Registers[2].Is(ramPos2 + (uint)i + 1);

            cpu.PSW_c.Is(true);
            cpu.PSW_z.Is(true);
        }
        RunOpcode(SCMPU);
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is((uint)(ramPos1 + length));
        cpu.Registers[2].Is((uint)(ramPos2 + length));
        cpu.Registers[3].Is(0u);
        cpu.PSW_c.Is(true);
        cpu.PSW_z.Is(true);
    }

    [Test]
    public void SMOVB_Test([Random(0, 20, 5)]int length) {
        var random = TestContext.CurrentContext.Random;
        var datas = Enumerable.Range(0, (int)length)
            .Select(_ => random.NextByte(0, 0xFF))
            .ToArray();
        var pos1 = 50u;
        var pos2 = 1u;
        memory.WriteRange(pos2, datas.ToArray());
        var ramPos1Begin = ramBeginAddress + pos1;
        var ramPos2Begin = ramBeginAddress + pos2;
        var ramPos1End = ramPos1Begin + datas.Length;
        var ramPos2End = ramPos2Begin + datas.Length;
        cpu.Registers[1] = (uint)ramPos1End - 1;
        cpu.Registers[2] = (uint)ramPos2End - 1;
        cpu.Registers[3] = (uint)length;
        var prevPC = cpu.PC;
        for (int i = 0; i < length - 1; i++) {
            RunOpcode(SMOVB);
            // 最後まで転送するまでプログラムカウンタはインクリメントされないことを確認する
            cpu.PC.Is(prevPC);
            // 位置がデクリメントされていることを確認する
            cpu.Registers[1].Is((uint)(ramPos1End - (i + 2)));
            cpu.Registers[2].Is((uint)(ramPos2End - (i + 2)));
        }
        RunOpcode(SMOVB);
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is(ramPos1Begin - 1);
        cpu.Registers[2].Is(ramPos2Begin - 1);
        cpu.Registers[3].Is(0u);
        Enumerable.Range((int)ramPos1Begin, length)
            .Select(addr => (byte)memory.Read((uint)addr, 1))
            .ToArray()
            .Is(datas);
    }

    [Test]
    public void SMOVF_Test([Random(0, 20, 5)]int length) {
        var random = TestContext.CurrentContext.Random;
        var datas = Enumerable.Range(0, (int)length)
            .Select(_ => random.NextByte(0, 0xFF))
            .ToArray();
        var pos1 = 50u;
        var pos2 = 1u;
        memory.WriteRange(pos2, datas);
        var ramPos1Begin = ramBeginAddress + pos1;
        var ramPos2Begin = ramBeginAddress + pos2;
        var ramPos1End = (uint)(ramPos1Begin + datas.Length);
        var ramPos2End = (uint)(ramPos2Begin + datas.Length);
        cpu.Registers[1] = (uint)ramPos1Begin;
        cpu.Registers[2] = (uint)ramPos2Begin;
        cpu.Registers[3] = (uint)length;
        var prevPC = cpu.PC;
        for (int i = 0; i < length - 1; i++) {
            RunOpcode(SMOVF);
            // 最後まで転送するまでプログラムカウンタはインクリメントされないことを確認する
            cpu.PC.Is(prevPC);
            // 位置がインクリメントされていることを確認する
            cpu.Registers[1].Is((uint)(ramPos1Begin + i + 1));
            cpu.Registers[2].Is((uint)(ramPos2Begin + i + 1));
        }
        RunOpcode(SMOVF);
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is(ramPos1End);
        cpu.Registers[2].Is(ramPos2End);
        cpu.Registers[3].Is(0u);
        Enumerable.Range((int)ramPos1Begin, length)
            .Select(addr => (byte)memory.Read((uint)addr, 1))
            .ToArray()
            .Is(datas);
    }

    [Test]
    public void SMOVU_Test([Random(1, 20, 5)]int length) {
        var random = TestContext.CurrentContext.Random;
        var datas = Enumerable.Range(0, (int)length)
            .Select(_ => random.NextByte(1, 0xFF))
            .ToArray();
        var pos1 = 0u;
        var pos2 = 50u;
        memory.WriteRange(pos2, datas);
        var ramPos1 = ramBeginAddress + pos1;
        var ramPos2 = ramBeginAddress + pos2;
        cpu.Registers[1] = ramPos1;
        cpu.Registers[2] = ramPos2;
        cpu.Registers[3] = (uint)length;
        var prevPC = cpu.PC;
        for (int i = 0; i < length - 1; i++) {
            RunOpcode(SMOVU);
            // 最後まで検索転送するまでプログラムカウンタはインクリメントされないことを確認する
            cpu.PC.Is(prevPC);
            // 転送している位置がインクリメントされていることを確認する
            cpu.Registers[1].Is(ramPos1 + (uint)i + 1);
            cpu.Registers[2].Is(ramPos2 + (uint)i + 1);
        }
        RunOpcode(SMOVU);
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is(ramPos1 + (uint)length);
        cpu.Registers[2].Is(ramPos2 + (uint)length);
        cpu.Registers[3].Is(0u);
    }

    [Test]
    public void SMOVU_Contains0_Test([Random(1, 20, 5)]int length) {
        var random = TestContext.CurrentContext.Random;
        var datas = Enumerable.Range(0, (int)length - 1)
            .Select(_ => random.NextByte(0, 0xFF))
            .TakeWhile(x => x != 0)
            .Append((byte)0)
            .ToArray();
        var zeroPos = Array.IndexOf<byte>(datas, 0);
        var pos1 = 0u;
        var pos2 = 50u;
        memory.WriteRange(pos2, datas);
        var ramPos1 = ramBeginAddress + pos1;
        var ramPos2 = ramBeginAddress + pos2;
        cpu.Registers[1] = ramPos1;
        cpu.Registers[2] = ramPos2;
        cpu.Registers[3] = (uint)length;
        var prevPC = cpu.PC;
        for (int i = 0; i < zeroPos; i++) {
            RunOpcode(SMOVU);
            // 最後まで転送するまでプログラムカウンタはインクリメントされないことを確認する
            cpu.PC.Is(prevPC);
            // 転送している位置がインクリメントされていることを確認する
            cpu.Registers[1].Is(ramPos1 + (uint)i + 1);
            cpu.Registers[2].Is(ramPos2 + (uint)i + 1);
        }
        RunOpcode(SMOVU);
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is((uint)(ramPos1 + zeroPos + 1));
        cpu.Registers[2].Is((uint)(ramPos2 + zeroPos + 1));
        cpu.Registers[3].Is((uint)(length - zeroPos - 1));
    }

    [Test]
    public void SSTR_Test(
        [Values(MemEx.B, MemEx.W, MemEx.L)]MemEx size,
        [Random(1u, 20u, 5)]uint length,
        [Random(uint.MinValue, uint.MaxValue, 5)]uint value) {
        var random = TestContext.CurrentContext.Random;
        var byteLength = size switch {
            MemEx.B => 1,
            MemEx.W => 2,
            MemEx.L => 4,
            _ => throw new NotSupportedException(size.ToString())
        };
        var expectedValue = size switch {
            MemEx.B => (byte)value,
            MemEx.W => (ushort)value,
            MemEx.L => value,
            _ => throw new NotSupportedException(size.ToString())
        };
        var datas = Enumerable.Range(0, (int)length * byteLength)
            .Select(_ => random.NextByte(0, 0xFF))
            .ToArray();
        var pos1 = 0u;
        memory.WriteRange(pos1, datas);
        var ramPos1 = ramBeginAddress + pos1;
        cpu.Registers[1] = ramPos1;
        cpu.Registers[2] = value;
        cpu.Registers[3] = length;
        var prevPC = cpu.PC;
        for (int i = 0; i < length - 1; i++) {
            RunOpcode(SSTR(size));
            // 最後まで転送するまでプログラムカウンタはインクリメントされないことを確認する
            cpu.PC.Is(prevPC);
            // 転送している位置がインクリメントされていることを確認する
            cpu.Registers[1].Is(ramPos1 + (uint)((i + 1) * byteLength));
            // 変化していないことを確認する
            cpu.Registers[2].Is(value);
        }
        RunOpcode(SSTR(size));
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is(ramPos1 + (uint)(length * byteLength));
        cpu.Registers[2].Is(value);
        cpu.Registers[3].Is(0u);
        Enumerable.Range(0, (int)length)
            .Select(i => memory.Read((uint)(ramPos1 + byteLength * i), byteLength))
            .ToArray()
            .Is(Enumerable.Repeat(expectedValue, (int)length));
    }

    [Test]
    [TestCase(PSWFlag.C)]
    [TestCase(PSWFlag.Z)]
    [TestCase(PSWFlag.S)]
    [TestCase(PSWFlag.O)]
    [TestCase(PSWFlag.I)]
    [TestCase(PSWFlag.U)]
    public void SETPSW_Test(PSWFlag flag) {
        // スーバーバイザモードで動作していることが前提
        cpu.PSW = 0;
        cpu.PSW.Is(0u);
        RunOpcode(SETPSW(flag));
        var result = flag switch {
            PSWFlag.C => cpu.PSW_c,
            PSWFlag.Z => cpu.PSW_z,
            PSWFlag.S => cpu.PSW_s,
            PSWFlag.O => cpu.PSW_o,
            PSWFlag.I => cpu.PSW_i,
            PSWFlag.U => cpu.PSW_u,
            _ => throw new ArgumentException(flag.ToString())
        };
        result.Is(true);
        cpu.PSW.IsNot(0u);
    }

    [Test]
    [TestCase(8, 0x8000_0000u, 0xFF80_0000u, false, false,  true)]
    [TestCase(8, 0x8000_0080u, 0xFF80_0000u,  true, false,  true)]
    [TestCase(1, 0x4000_0000u, 0x2000_0000u, false, false, false)]
    [TestCase(0, 0x0000_0001u, 0x0000_0001u, false, false, false)]
    [TestCase(1, 0x0000_0001u, 0x0000_0000u,  true,  true, false)]
    [TestCase( 0x0, 0x7FFF_FFFFu, 0x7FFF_FFFFu, false, false,  false)]
    [TestCase( 0x0, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, false,   true)]
    public void SHAR_imm_Test(byte a, uint b, uint result,
                          bool expC, bool expZ, bool expS) {
        cpu.Registers[1] = b;
        RunOpcode(SHAR(new UInt5(a), R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(false);
    }

    [TestCase(8u, 0x8000_0000u, 0xFF80_0000u, false, false,  true)]
    [TestCase(8u, 0x8000_0080u, 0xFF80_0000u,  true, false,  true)]
    [TestCase(1u, 0x4000_0000u, 0x2000_0000u, false, false, false)]
    [TestCase(0u, 0x0000_0001u, 0x0000_0001u, false, false, false)]
    [TestCase(1u, 0x0000_0001u, 0x0000_0000u,  true,  true, false)]
    [TestCase( 0x0u, 0x7FFF_FFFFu, 0x7FFF_FFFFu, false, false,  false)]
    [TestCase( 0x0u, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, false,   true)]
    [TestCase(0xE0u, 0xFFFF_0000u, 0xFFFF_0000u, false, false,  true)]
    public void SHAR_rr_Test(uint a, uint b, uint result,
                             bool expC, bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(SHAR(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(false);
    }


    [Test]
    [TestCase( 4, 0x1234_5678u, 0x2345_6780u,  true, false, false,  true)]
    [TestCase( 4, 0xF923_4567u, 0x9234_5670u,  true, false,  true, false)]
    [TestCase( 1, 0x4000_0000u, 0x8000_0000u, false, false,  true,  true)]
    [TestCase( 1, 0xC000_0000u, 0x8000_0000u,  true, false,  true, false)]
    [TestCase( 1, 0x8000_0000u, 0x0000_0000u,  true,  true, false,  true)]
    [TestCase( 0, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, false,  true, false)]
    [TestCase( 0, 0x7FFF_FFFFu, 0x7FFF_FFFFu, false, false, false, false)]
    [TestCase( 0, 0x0000_0000u, 0x0000_0000u, false,  true, false, false)]
    [TestCase(31, 0x0000_0000u, 0x0000_0000u, false,  true, false, false)]
    public void SHLL_imm_Test(byte a, uint b, uint result,
                              bool expC, bool expZ, bool expS, bool expO) {
        cpu.Registers[1] = b;
        RunOpcode(SHLL(new UInt5(a), R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(0xE0u, 0x0000_FFFFu, 0x0000_FFFFu, false, false, false, false)]
    [TestCase(   4u, 0x1234_5678u, 0x2345_6780u,  true, false, false,  true)]
    [TestCase(   4u, 0xF923_4567u, 0x9234_5670u,  true, false,  true, false)]
    [TestCase(   1u, 0x4000_0000u, 0x8000_0000u, false, false,  true,  true)]
    [TestCase(   1u, 0xC000_0000u, 0x8000_0000u,  true, false,  true, false)]
    [TestCase(   1u, 0x8000_0000u, 0x0000_0000u,  true,  true, false,  true)]
    [TestCase(   0u, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, false,  true, false)]
    [TestCase(   0u, 0x7FFF_FFFFu, 0x7FFF_FFFFu, false, false, false, false)]
    [TestCase(   0u, 0x0000_0000u, 0x0000_0000u, false,  true, false, false)]
    [TestCase(  31u, 0x0000_0000u, 0x0000_0000u, false,  true, false, false)]
    public void SHLL_rr_Test(uint a, uint b, uint result,
                             bool expC, bool expZ, bool expS, bool expO) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(SHLL(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }


    [Test]
    [TestCase(16, 0xFFFF_FFFFu, 0x0000_FFFFu,  true, false, false)]
    [TestCase( 0, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, false,  true)]
    [TestCase(16, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(16, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(31, 0x8FFF_FFFFu, 0x0000_0001u,  false,  false, false)]
    [TestCase(24, 0x1234_5678u, 0x0000_0012u, false, false, false)]
    public void SHLR_imm_Test(byte a, uint b, uint result,
                              bool expC, bool expZ, bool expS) {
        cpu.Registers[1] = b;
        RunOpcode(SHLR(new UInt5(a), R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [TestCase(  16u, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(  16u, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(  31u, 0x8FFF_FFFFu, 0x0000_0001u,  false,  false, false)]
    [TestCase(0xE0u, 0xFFFF_FFFFu, 0xFFFF_FFFFu,  false,  false,  true)]
    public void SHLR_rr_Test(uint a, uint b, uint result,
                             bool expC, bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(SHLR(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( true, 6u, unchecked((int)0xFF1122EE), 6u)]
    [TestCase(false, 6u, unchecked((int)0xFF1122EE), 0xFF1122EEu)]
    public void STNZ_Test(bool psw_z, uint initial, int a, uint result) {
        cpu.Registers[1] = initial;
        cpu.PSW_z = psw_z;
        RunOpcode(STNZ(a, R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase(false, 6u, unchecked((int)0xFF1122EEu), 6u)]
    [TestCase( true, 6u, unchecked((int)0xFF1122EEu), 0xFF1122EEu)]
    public void STZ_Test(bool psw_z, uint initial, int a, uint result) {
        cpu.Registers[1] = initial;
        cpu.PSW_z = psw_z;
        RunOpcode(STZ(a, R1));
        cpu.Registers[1].Is(result);
    }

    [Test]
    [TestCase(                100u,          1_000_905u,             1_000_805u,  true, false, false, false)]
    [TestCase(               2000u,               2000u,                     0u,  true,  true, false, false)]
    [TestCase( unchecked((uint)-1),         0x7FFF_FFFFu,           0x8000_0000u,  false, false,  true,  true)]
    [TestCase(                100u, unchecked((uint)-1),   unchecked((uint)-101),  true, false,  true, false)]
    [TestCase( unchecked((uint)-1),         0xFFFF_FFFFu,                     0u, true,  true, false,  false)]
    public void SUB_Test(
        uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(SUB(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    public void SUNTIL_Test(
        [Values(MemEx.B, MemEx.W, MemEx.L)]MemEx size,
        [Random(uint.MinValue, uint.MaxValue, 5)] uint value,
        [Random(1u, 20u, 5)] uint length) {
        var random = TestContext.CurrentContext.Random;
        var byteLength = size switch {
            MemEx.B => 1,
            MemEx.W => 2,
            MemEx.L => 4,
            _ => throw new NotSupportedException(size.ToString())
        };
        var expectedValue = size switch {
            MemEx.B => (byte)value,
            MemEx.W => (ushort)value,
            MemEx.L => value,
            _ => throw new NotSupportedException(size.ToString())
        };
        var maxValue = (uint)((1L << (8 * byteLength)) - 1);
        var datas = random.NextBool()
            ? Enumerable.Range(0, (int)(length - 1))
            .Select(_ => random.NextUInt(0, maxValue))
            .Append(expectedValue)
            .ToArray()
            : Enumerable.Range(0, (int)length)
            .Select(_ => random.NextUInt(0, maxValue))
            .ToArray();
        var pos1 = 0u;
        for (int i = 0; i < datas.Length; i++) {
            memory.Write((uint)(pos1 + (byteLength * i)), byteLength, datas[i]);
        }
        var ramPos1 = ramBeginAddress + pos1;
        cpu.Registers[1] = ramPos1;
        cpu.Registers[2] = expectedValue;
        cpu.Registers[3] = length;
        var prevPC = cpu.PC;
        int count;
        for (count = 0; count < length - 1 && datas[count] != expectedValue; count++) {
            RunOpcode(SUNTIL(size));
            // 異なる値が見つかるもしくは最後まで探索するまでプログラムカウンタはインクリメントされないことを確認する
            cpu.PC.Is(prevPC);
            // 比較している位置がインクリメントされていることを確認する
            cpu.Registers[1].Is(ramPos1 + (uint)((count + 1) * byteLength));
            // 変化していないことを確認する
            cpu.Registers[2].Is(expectedValue);
            cpu.PSW_z.Is(false);
            // TODO Cフラグがどの様に変化するのが正しいのかを確認する
            // cpu.PSW_c.Is();
        }
        RunOpcode(SUNTIL(size));
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is(ramPos1 + (uint)((count + 1) * byteLength));
        cpu.Registers[2].Is(expectedValue);
        cpu.Registers[3].Is((uint)(length - count - 1));
        cpu.PSW_z.Is(datas[count] == expectedValue);
    }

    [Test]
    public void SWHILE_Test(
        [Values(MemEx.B, MemEx.W, MemEx.L)]MemEx size,
        [Random(uint.MinValue, uint.MaxValue, 5)] uint value,
        [Random(1u, 20u, 5)] uint length) {
        var random = TestContext.CurrentContext.Random;
        var byteLength = size switch {
            MemEx.B => 1,
            MemEx.W => 2,
            MemEx.L => 4,
            _ => throw new NotSupportedException(size.ToString())
        };
        var expectedValue = size switch {
            MemEx.B => (byte)value,
            MemEx.W => (ushort)value,
            MemEx.L => value,
            _ => throw new NotSupportedException(size.ToString())
        };
        var maxValue = (uint)((1L << (8 * byteLength)) - 1);
        var datas = random.NextBool()
            // 異なる値を含む場合
            ? Enumerable.Repeat(expectedValue, (int)(length - 1))
            .Append(~expectedValue)
            .ToArray()
            // 同じ値だけの場合
            : Enumerable.Repeat(expectedValue, (int)length)
            .ToArray();
        var pos1 = 0u;
        for (int i = 0; i < datas.Length; i++) {
            memory.Write((uint)(pos1 + (byteLength * i)), byteLength, datas[i]);
        }
        var ramPos1 = ramBeginAddress + pos1;
        cpu.Registers[1] = ramPos1;
        cpu.Registers[2] = expectedValue;
        cpu.Registers[3] = length;
        var prevPC = cpu.PC;
        int count;
        for (count = 0; count < length - 1 && datas[count] == expectedValue; count++) {
            RunOpcode(SWHILE(size));
            cpu.PC.Is(prevPC);
            // 比較している位置がインクリメントされていることを確認する
            cpu.Registers[1].Is(ramPos1 + (uint)((count + 1) * byteLength));
            // 変化していないことを確認する
            cpu.Registers[2].Is(expectedValue);
            cpu.PSW_z.Is(true);
            // TODO Cフラグがどの様に変化するのが正しいのかを確認する
            // cpu.PSW_c.Is();
        }
        RunOpcode(SWHILE(size));
        cpu.PC.Is(prevPC + 2);
        cpu.Registers[1].Is(ramPos1 + (uint)((count + 1) * byteLength));
        cpu.Registers[2].Is(expectedValue);
        cpu.Registers[3].Is((uint)(length - count - 1));
        cpu.PSW_z.Is(datas[count] == expectedValue);
    }


    [Test]
    [TestCase(          0u, 0xFFFF_FFFFu,  true, false)]
    [TestCase(          4u,         100u, false, false)]
    [TestCase(0x8000_0000u, 0x8000_0000u, false,  true)]
    public void TST_Test(uint a, uint b, bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(TST(R1, R2));
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(0b1010_0000u, 0b0111_0011u, 0b1101_0011u, false,  false)]
    [TestCase(0b1010_0000u << 24, 0b0111_0011u << 24, 0b1101_0011u << 24, false,  true)]
    [TestCase(0xFFFF_FFFFu, 0xFFFF_FFFFu,           0u,  true, false)]
    [TestCase(          0u,           0u,           0u,  true, false)]
    public void XOR_Test(uint a, uint b, uint result, bool expZ, bool expS) {
        cpu.Registers[1] = a;
        cpu.Registers[2] = b;
        RunOpcode(XOR(R1, R2));
        cpu.Registers[2].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

       [Test]
    public void TSTRS_Test()
    {
        /*
        //addr指定
        uint addrA = Convert.ToUInt32(random_generate.NextInt64(1,10));
        uint addrB;
        do
        {
            addrB = Convert.ToUInt32(random_generate.NextInt64(1,10));
        }while(addrA == addrB);
        */
        //ランダムネーム
        var address = 0x3u;
        var a = 0xCCDDEEFFu;
        var b = 0x11223344u;
        memory.Write(address, 4, a);
        cpu.Registers[1] = address;
        cpu.Registers[2] = b;
        //Reg reg = (Reg)Enum.ToObject(typeof(Reg), 2);

        RunOpcode(XCHG(new RegRef(R1, MemEx.L), R2));
        //cpu.Registers[2].Is(a&b);
        memory.Read(address, 4).Is(b);


    }
}
