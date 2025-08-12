using NUnit.Framework;
using RX;
using TestTools;
using static RX.RXv1Assembler;
using Pheripheral;
using System.Buffers;
using Peripheral.Renesas;
using Util;
using System.Runtime.Intrinsics.Arm;
using ScottPlot;

namespace IsrEmulationTest;
public class RXv1InstrunctionTest {
    RXv1Core cpu;
    RAM32Bit memory;
    RAM32Bit rom;
    BusManager busManager;

    // 毎回インスタンスを作ると重いため
    readonly Translate rxv1Translate = Translate.Instance;

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

    static StdRegAddressing GetRandomStdRegAddressing(Reg reg, MemEx? size, LengthOfDisplacement ldBegin, LengthOfDisplacement ldEnd) {
        var random = TestContext.CurrentContext.Random;
        var ld = size == MemEx.L
            ? (LengthOfDisplacement)random.Next((int)ldBegin, (int)ldEnd + 1)
            : (LengthOfDisplacement)random.Next(
                Math.Min((int)ldBegin, (int)LengthOfDisplacement.Reg),
                Math.Min((int)ldEnd + 1, (int)LengthOfDisplacement.Reg));
        switch (ld) {
            case LengthOfDisplacement.Reg:
                return reg;
            case LengthOfDisplacement.RefReg:
                return new RegRef(reg, size);
            case LengthOfDisplacement.DSP8Reg: {
                var dsp = random.NextByte(0, 10);
                return new RelRef8(dsp, reg, size);
            }
            case LengthOfDisplacement.DSP16Reg: {
                var dsp = random.NextUShort(0, 10);
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
        cpu.PC = romBeginAddress;
        //cpu.PC = 0;
        cpu.SP = ramEndAddress;
    }

    void RunOpcode(Instruction32 instruction) {
        var writer = new ArrayBufferWriter<byte>();
        var asmWriter = new AssemblyWriter(writer);
        rxv1Translate.CreateBinary(instruction, ref asmWriter);
        for (uint i = 0; i < writer.WrittenCount; i++) {
            busManager.Write(cpu.PC + i, 1, writer.WrittenSpan[(int)i]);
        }
        var reader = new Reader(busManager, cpu.PC);
        var prevPos = reader.Position;
        var list = new List<uint>();
        rxv1Translate.ParseAssembly(ref reader, list);
        var afterPos = reader.Position;
        var opSize = afterPos - prevPos;

        var instArgs = list.ToArray();
        cpu.ExecuteInstruction((OpCode)instArgs[0], instArgs.AsSpan(1), opSize);
    }

    [Test]
    [TestCase(0xFFFF_FFFFu, 0x1u, false, false, false)]
    [TestCase(0x8000_0000u, 0x8000_0000, false, true, true)]
    [TestCase(0x0u, 0x0u, true, false, false)]
    [TestCase(1000u, 1000u, false, false, false)]
    [TestCase(unchecked((uint)-1000), 1000u, false, false, false)]
    public void ABS_rd_Test(uint a, uint result, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        cpu.Registers[rd] = a;
        RunOpcode(ABS((Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z = expZ;
        cpu.PSW_s = expS;
        cpu.PSW_o = expO;
    }

    [Test]
    [TestCase(0xFFFF_FFFFu, 0x1u, false, false, false)]
    [TestCase(0x8000_0000u, 0x8000_0000, false, true, true)]
    [TestCase(550u, 550u, false, false, false)]
    [TestCase(  0u,   0u, true, false, false)]
    [TestCase(unchecked((uint)-440), 440u, false, true, false)]
    public void ABS_rs_rd_Test(uint a, uint result, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 16);
        var rd = random.Next(1, 16);
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        cpu.Registers[rs] = a;
        RunOpcode(ABS((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z = expZ;
        cpu.PSW_s = expS;
        cpu.PSW_o = expO;
    }

    [Test]
    [TestCase(false, LengthOfImmediate.SIMM8, 0x80u/*-128*/, 128u, 0x0u, true, true, false, false)]
    [TestCase(false, LengthOfImmediate.SIMM8, 127u, 129u, 256u, false, false, false, false)]
    [TestCase(false, LengthOfImmediate.SIMM16, 32767u, 32769u, 65536u, false, false, false, false)]
    [TestCase(false, LengthOfImmediate.SIMM16, 0x8000u, 0x8000u, 0u, true, true, false, false)]
    [TestCase(false, LengthOfImmediate.SIMM24, 0x80_0000u, 0x80_0000u, 0x0u, true, true, false, false)]
    [TestCase(false, LengthOfImmediate.SIMM24, 0x7F_FFFFu, 0x10_0001u, 0x90_0000u, false, false, false, false)]
    [TestCase(false, LengthOfImmediate.IMM32, 10u, 5u, 15u, false, false, false, false)]
    [TestCase( true, LengthOfImmediate.IMM32, 10u, 5u, 16u, false, false, false, false)]
    [TestCase(false, LengthOfImmediate.IMM32, 0u, 0u, 0u, false, true, false, false)]
    [TestCase(true, LengthOfImmediate.IMM32, 0u, 0u, 1u, false, false, false, false)]
    [TestCase(false, LengthOfImmediate.IMM32, 0xFFFF_FFFFu, 1u, 0u, true, true, false, false)]
    [TestCase(false, LengthOfImmediate.IMM32, 1u, 0xFFFF_FFFFu, 0u, true, true, false, false)]
    [TestCase(false, LengthOfImmediate.IMM32, 0x7FFF_FFFFu, 1u, 0x8000_0000u, false, false, true, true)]
    [TestCase(false, LengthOfImmediate.IMM32, 1u, 0x7FFF_FFFFu, 0x8000_0000u, false, false, true, true)]
    [TestCase(false, LengthOfImmediate.IMM32, 0x8000_0000u, 0xFFFF_FFFFu, 0x7FFF_FFFFu, true, false, false, true)]
    [TestCase(false, LengthOfImmediate.IMM32, 0x8000_0000u, 0x8000_0000u, 0u, true, true, false, true)]
    public void ADC_ir_Test(bool psw_c, LengthOfImmediate li, uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {

        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_c = psw_c;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(ADC(new StdImmValue(li, a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(false, 10u, 5u, 15u, false, false, false, false)]
    [TestCase(true, 10u, 5u, 16u, false, false, false, false)]
    [TestCase(false, 0u, 0u, 0u, false, true, false, false)]
    [TestCase(true, 0u, 0u, 1u, false, false, false, false)]
    [TestCase(false, 0xFFFF_FFFFu, 1u, 0u, true, true, false, false)]
    [TestCase(false, 1u, 0xFFFF_FFFFu, 0u, true, true, false, false)]
    [TestCase(false, 0x7FFF_FFFFu, 1u, 0x8000_0000u, false, false, true, true)]
    [TestCase(false, 1u, 0x7FFF_FFFFu, 0x8000_0000u, false, false, true, true)]
    [TestCase(false, 0x8000_0000u, 0xFFFF_FFFFu, 0x7FFF_FFFFu, true, false, false, true)]
    [TestCase(false, 0x8000_0000u, 0x8000_0000u, 0u, true, true, false, true)]
    public void ADC_mr__ADC_rr_Test(bool psw_c, uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {

        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.PSW_c = psw_c;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        cpu.Registers[rd] = b;
        RunOpcode(ADC(dsp, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(0, 0xFFu, 0xFFu, false, false, false, false)]
    [TestCase(1, 0xFFFF_FFFFu, 0u, true, true, false, false)]
    [TestCase(13, 1u, 14u, false, false, false, false)]
    [TestCase(15, 0xFFFF_FFF0u, 0xFFFF_FFFFu, false, false, true, false)]
    [TestCase(15, 0x7FFF_FFFFu, 0x8000_000eu, false, false, true, true)]
    public void ADD_4irr_Test(
        byte a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(ADD(new UInt4(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 110, 10, 120, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM8, -110, 10, -100, false, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM16, 300, 20, 320, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM16, -300, 20, -280, false, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM24, 300_000, 50, 300_050, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 300_000, 50, 300_050, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 4_590_125, 100, 4_590_225, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, -300_000, 50, -299_950, false, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM24, 70000, 100, 70100, false, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 100, 10, 110, false, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 50, 300, 350, false, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0x1, unchecked((int)0xFFFF_FFFF), 0x0, true, true, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0xF, unchecked((int)0x7FFF_FFFF), unchecked((int)0x8000_000E), false, false, true, true)]
    [TestCase(LengthOfImmediate.IMM32, unchecked((int)0x8000_0000), unchecked((int)0x8000_0000), 0x0, true, true, false, true)]
    public void ADD_irrr_Test(
        LengthOfImmediate li, int a, int b, int result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = unchecked((uint)b);
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(ADD(new StdImmValue(li, unchecked((uint)a)), (Reg)rd));
        cpu.Registers[rd].Is(unchecked((uint)result));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(null, 0x7Fu, 0x1u, 0x80u, false, false, false, false)]
    [TestCase(null, 0x80u, 0x1u, 0x81u, false, false, false, false)]
    [TestCase(MemEx.B, 0x7Fu, 0x1u, 0x80u, false, false, false, false)]
    [TestCase(MemEx.B, 0x80u, 0x1u, 0xFFFF_FF81u, false, false, true, false)]
    [TestCase(MemEx.W, 0x7F00u, 0x1u, 0x7F01u, false, false, false, false)]
    [TestCase(MemEx.W, 0x8000u, 0x1u, 0xFFFF_8001u, false, false, true, false)]
    [TestCase(MemEx.UW, 0x7Fu, 0x1u, 0x80u, false, false, false, false)]
    [TestCase(MemEx.UW, 0x80u, 0x1u, 0x81u, false, false, false, false)]
    [TestCase(MemEx.UW, 0x7F01u, 0x1u, 0x7F02u, false, false, false, false)]
    [TestCase(MemEx.UW, 0x8001u, 0x1u, 0x8002u, false, false, false, false)]
    [TestCase(MemEx.L, 100u, 10u, 110u, false, false, false, false)]
    [TestCase(MemEx.L, 50u, 300u, 350u, false, false, false, false)]
    [TestCase(MemEx.L, 0x1u, 0xFFFF_FFFFu, 0x0u, true, true, false, false)]
    [TestCase(MemEx.L, 0xFu, 0x7FFF_FFFFu, 0x8000_000Eu, false, false, true, true)]
    [TestCase(MemEx.L, 0x8000_0000u, 0x8000_0000u, 0x0u, true, true, false, true)]
    public void ADD_mr__ADD_ub_rs_mr_Test(
        MemEx? sz, uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(ADD(dsp, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 0x7Fu, 0x1u, 0x80u, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM8, 0x80u, 0x1u, 0xFFFF_FF81u, false, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7F00u, 0x1u, 0x7F01u, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x8000u, 0x1u, 0xFFFF_8001u, false, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x7F_0123u, 0x1u, 0x7F_0124u, false, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x8F_0123u, 0x1u, 0xFF8F_0124, false, false, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 100u, 10u, 110u, false, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 50u, 300u, 350u, false, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0x1u, 0xFFFF_FFFFu, 0x0u, true, true, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0xFu, 0x7FFF_FFFFu, 0x8000_000Eu, false, false, true, true)]
    [TestCase(LengthOfImmediate.IMM32, 0x8000_0000u, 0x8000_0000u, 0x0u, true, true, false, true)]
    public void ADD_irrr_Test(
        LengthOfImmediate li, uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs2 = random.Next(1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs2] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(ADD(new StdImmValue(li, a), (Reg)rs2, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(100u, 10u, 110u, false, false, false, false)]
    [TestCase(50u, 300u, 350u, false, false, false, false)]
    [TestCase(0x1u, 0xFFFF_FFFFu, 0x0u, true, true, false, false)]
    [TestCase(0xFu, 0x7FFF_FFFFu, 0x8000_000Eu, false, false, true, true)]
    [TestCase(0x8000_0000u, 0x8000_0000u, 0x0u, true, true, false, true)]
    public void ADD_rrr_Test(
        uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rs2] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(ADD((Reg)rs, (Reg)rs2, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(0, 1u, 0u, true, false)]
    [TestCase(1, 1u, 1u, false, false)]
    [TestCase(1, 0u, 0u, true, false)]
    [TestCase(0, 0u, 0u, true, false)]
    [TestCase(0xF, 0xFFFF_FFFFu, 0xFu, false, false)]
    public void AND_4ir_Test(byte a, uint b, uint result, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(AND(new UInt4(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 0x71u, 0xFFFF_FFFFu, 0x71u, false, false)]
    [TestCase(LengthOfImmediate.SIMM8, 0x81u, 0xFFFF_FFFFu, 0xFFFF_FF81u, false, true)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7F00u, 0xFFFF_FFFFu, 0x7F00u, false, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x80F0u, 0xFFFF_FFFFu, 0xFFFF_80F0, false, true)]
    [TestCase(LengthOfImmediate.SIMM24, 0x7F_1234u, 0xFFFF_FFFFu, 0x7F_1234u, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x80_0000u, 0xFFFF_FFFFu, 0xFF80_0000u, false, true)]
    [TestCase(LengthOfImmediate.SIMM24, 0x1_FFFFu, 0xFFFF_FFFFu, 0x1_FFFFu, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0u, 1u, 0u, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 1u, 1u, 1u, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 1u, 0u, 0u, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 0u, 0u, 0u, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 0xFFFF_FFFFu, 0x0u, 0x0u, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, true)]
    [TestCase(LengthOfImmediate.IMM32, 0x8123_4567u, 0x8123_4567u, 0x8123_4567u, false, true)]
    public void AND_ir(LengthOfImmediate li, uint a, uint b, uint result, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(AND(new StdImmValue(li, a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(null, 0x71u, 0xFFFF_FFFFu, 0x71u, false, false)]
    [TestCase(null, 0x81u, 0xFFFF_FFFFu, 0x81u, false, false)]
    [TestCase(MemEx.B, 0x71u, 0xFFFF_FFFFu, 0x71u, false, false)]
    [TestCase(MemEx.B, 0x81u, 0xFFFF_FFFFu, 0xFFFF_FF81u, false, true)]
    [TestCase(MemEx.W, 0x7F00u, 0xFFFF_FFFFu, 0x7F00u, false, false)]
    [TestCase(MemEx.W, 0x80F0u, 0xFFFF_FFFFu, 0xFFFF_80F0, false, true)]
    [TestCase(MemEx.UW, 0x7F00u, 0xFFFF_FFFFu, 0x7F00u, false, false)]
    [TestCase(MemEx.UW, 0x80F0u, 0xFFFF_FFFFu, 0x80F0u, false, false)]
    [TestCase(MemEx.L, 0u, 1u, 0u, true, false)]
    [TestCase(MemEx.L, 1u, 1u, 1u, false, false)]
    [TestCase(MemEx.L, 1u, 0u, 0u, true, false)]
    [TestCase(MemEx.L, 0u, 0u, 0u, true, false)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0x0u, 0x0u, true, false)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, true)]
    [TestCase(MemEx.L, 0x8123_4567u, 0x8123_4567u, 0x8123_4567u, false, true)]
    public void AND_mr__AND_ub_rs_mr_Test(MemEx? sz, uint a, uint b, uint result, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(AND(dsp, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [TestCase(0u, 1u, 0u, true, false)]
    [TestCase(1u, 1u, 1u, false, false)]
    [TestCase(1u, 0u, 0u, true, false)]
    [TestCase(0u, 0u, 0u, true, false)]
    [TestCase(0xFFFF_FFFFu, 0x0u, 0x0u, true, false)]
    [TestCase(0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, true)]
    [TestCase(0x8123_4567u, 0x8123_4567u, 0x8123_4567u, false, true)]
    public void AND_rrr(uint a, uint b, uint result, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rs2] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(AND((Reg)rs, (Reg)rs2, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [TestCase(0, 0x0u, 0u)]
    [TestCase(0, 0xFFFFu, 0xFFFEu)]
    [TestCase(0, 0xFFu, 0xFEu)]
    [TestCase(0, 0xFFFF_FFFFu, 0xFFFF_FFFEu)]
    [TestCase(1, 0xFFFF_FFFFu, 0xFFFF_FFFDu)]
    [TestCase(7, 0xFFFF_FFFFu, 0xFFFF_FF7Fu)]
    [TestCase(7, 0xFFFFu, 0xFF7Fu)]
    public void BCLR_im_Test(Byte a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 200, b);
        RunOpcode(BCLR(new UInt3(a), dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(result);
    }

    [Test]
    [TestCase(0, 0xFFFF_FFFF, 0xFFFF_FFFEu)]
    [TestCase(1, 0xFFFF_FFFF, 0xFFFF_FFFDu)]
    [TestCase(31, 0xFFFF_FFFF, 0x7FFF_FFFFu)]
    [TestCase(31, 0x0u, 0u)]
    public void BCLR_ir_Test(byte a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        RunOpcode(BCLR(new UInt5(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(0u, 0x0u, 0u)]
    [TestCase(0u, 0xFFFFu, 0xFFFEu)]
    [TestCase(0u, 0xFFu, 0xFEu)]
    [TestCase(0u, 0xFFFF_FFFFu, 0xFFFF_FFFEu)]
    [TestCase(8u, 0xFFFF_FFFFu, 0xFFFF_FFFEu)]

    [TestCase(1u, 0xFFFF_FFFFu, 0xFFFF_FFFDu)]
    [TestCase(9u, 0xFFFF_FFFFu, 0xFFFF_FFFDu)]

    [TestCase(7u, 0xFFFF_FFFFu, 0xFFFF_FF7Fu)]
    [TestCase(7u, 0xFFFFu, 0xFF7Fu)]
    [TestCase(15u, 0xFFFF_FFFFu, 0xFFFF_FF7Fu)]
    public void BCLR_rm_Test(uint a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        var dsp = GetRandomStdRegAddressing((Reg)rd, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 200, b);
        RunOpcode(BCLR((Reg)rs, dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(result);
    }

    [Test]
    [TestCase(0u, 0x0u, 0u)]
    [TestCase(0u, 0xFFFF_FFFFu, 0xFFFF_FFFEu)]
    [TestCase(0u, 0xFFu, 0xFEu)]
    [TestCase(0u, 0xFFFFu, 0xFFFEu)]
    [TestCase(32u, 0x0u, 0u)]
    [TestCase(32u, 0xFFFF_FFFFu, 0xFFFF_FFFEu)]
    [TestCase(0x8000_0000u, 0x0u, 0u)]
    [TestCase(0x8000_0000u, 0xFFFF_FFFFu, 0xFFFF_FFFEu)]

    [TestCase( 1u, 0xFFFF_FFFFu, 0xFFFF_FFFDu)]
    [TestCase(33u, 0xFFFF_FFFFu, 0xFFFF_FFFDu)]

    [TestCase(31u, 0xFFFF_FFFFu, 0x7FFF_FFFFu)]
    public void BCLR_rr_Test(uint a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        RunOpcode(BCLR((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
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
    public void BCnd_s_Test(
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
    [TestCase(false, false, false, false, 4, Cnd.GEU, false)]
    [TestCase(true, false, false, false, 4, Cnd.GEU, true)]
    [TestCase(true, false, false, false, 100, Cnd.GEU, true)]
    [TestCase(false, false, false, false, 100, Cnd.GEU, false)]
    [TestCase(false, false, false, false, 20, Cnd.EQ, false)]
    [TestCase(false, true, false, false, 20, Cnd.EQ, true)]
    [TestCase(false, false, false, false, 50, Cnd.GTU, false)]
    [TestCase(false, true, false, false, 50, Cnd.GTU, false)]
    [TestCase(true, true, false, false, 50, Cnd.GTU, false)]
    [TestCase(true, false, false, false, 50, Cnd.GTU, true)]
    [TestCase(false, false, false, false, 70, Cnd.PZ, true)]
    [TestCase(false, false, true, false, 70, Cnd.PZ, false)]
    [TestCase(false, false, false, false, 90, Cnd.GE, true)]
    [TestCase(false, false, true, false, 90, Cnd.GE, false)]
    [TestCase(false, false, false, true, 90, Cnd.GE, false)]
    [TestCase(false, false, true, true, 90, Cnd.GE, true)]
    [TestCase(false, false, false, false, 110, Cnd.GT, true)]
    [TestCase(false, true, false, false, 110, Cnd.GT, false)]
    [TestCase(false, false, true, false, 110, Cnd.GT, false)]
    [TestCase(false, false, false, true, 110, Cnd.GT, false)]
    [TestCase(false, true, true, false, 110, Cnd.GT, false)]
    [TestCase(false, false, true, true, 110, Cnd.GT, true)]
    [TestCase(false, true, false, true, 110, Cnd.GT, false)]
    [TestCase(false, true, true, true, 110, Cnd.GT, false)]
    [TestCase(false, false, false, false, 127, Cnd.O, false)]
    [TestCase(false, false, false, true, 127, Cnd.O, true)]

    [TestCase(false, false, false, false, 5, Cnd.LTU, true)]
    [TestCase(true, false, false, false, 5, Cnd.LTU, false)]
    [TestCase(true, false, false, false, 15, Cnd.LTU, false)]
    [TestCase(false, false, false, false, 15, Cnd.LTU, true)]
    [TestCase(false, false, false, false, 30, Cnd.NE, true)]
    [TestCase(false, true, false, false, 30, Cnd.NE, false)]
    [TestCase(false, false, false, false, 40, Cnd.LEU, true)]
    [TestCase(false, true, false, false, 40, Cnd.LEU, true)]
    [TestCase(true, true, false, false, 40, Cnd.LEU, true)]
    [TestCase(true, false, false, false, 40, Cnd.LEU, false)]
    [TestCase(false, false, false, false, 60, Cnd.N, false)]
    [TestCase(false, false, true, false, 60, Cnd.N, true)]
    [TestCase(false, false, false, false, 99, Cnd.LE, false)]
    [TestCase(false, false, true, false, 99, Cnd.LE, true)]
    [TestCase(false, false, false, true, 99, Cnd.LE, true)]
    [TestCase(false, false, true, true, 99, Cnd.LE, false)]
    [TestCase(false, true, false, false, 99, Cnd.LE, true)]
    [TestCase(false, true, true, false, 99, Cnd.LE, true)]
    [TestCase(false, true, false, true, 99, Cnd.LE, true)]
    [TestCase(false, true, true, true, 99, Cnd.LE, true)]
    [TestCase(false, false, false, false, 111, Cnd.LT, false)]
    [TestCase(false, false, true, false, 111, Cnd.LT, true)]
    [TestCase(false, false, false, true, 111, Cnd.LT, true)]
    [TestCase(false, false, true, true, 111, Cnd.LT, false)]
    [TestCase(false, false, false, false, -1, Cnd.NO, true)]
    [TestCase(false, false, false, true, -1, Cnd.NO, false)]
    [TestCase(false, false, false, false, -128, Cnd.NO, true)]
    [TestCase(false, false, false, true, -128, Cnd.NO, false)]
    public void BCnd_b_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        int src, Cnd condition,
        bool shouldJump
    ) {
        var beforePC = cpu.PC;
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        RunOpcode(BC_B(condition, (sbyte)src));

        if (shouldJump) {
            cpu.PC.Is((uint)(beforePC + (sbyte)src));
        } else {
            cpu.PC.Is(beforePC + 2);
        }
    }

    [TestCase(false, false, false, false, 2999, Cnd.EQ, false)]
    [TestCase(false, true, false, false, 2999, Cnd.EQ, true)]
    [TestCase(false, false, false, false, 32767, Cnd.NE, true)]
    [TestCase(false, true, false, false, 32767, Cnd.NE, false)]
    [TestCase(false, false, false, false, -32768, Cnd.NE, true)]
    [TestCase(false,  true, false, false, -32768, Cnd. NE, false)]
    public void BCnd_w_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        int src, Cnd condition,
        bool shouldJump
    ) {
        var beforePC = cpu.PC;
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        RunOpcode(BC_W(condition, (short)src));

        if (shouldJump) {
            cpu.PC.Is((uint)(beforePC + (short)src));
        } else {
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
        var reg = random.Next(1, 16);
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
        var reg = random.Next(1, 16);
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
    [TestCase(0u, 7, 1u << 7)]
    [TestCase(~0u, 7, ~(1u << 7))]
    [TestCase(0u, 1, 1u << 1)]
    [TestCase(~0u, 1, ~(1u << 1))]
    [TestCase(0u, 0, 1u)]
    [TestCase(~0u, 0, ~1u)]
    public void BNOT_im_Test(uint a, byte b, uint expcted) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 200, a);
        RunOpcode(BNOT(new UInt3(b), dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expcted);
    }

    [Test]
    [TestCase(0u, 31, 0u)]
    [TestCase(~0u, 31, ~0u)]
    [TestCase(0u, 15, 0u)]
    [TestCase(~0u, 14, ~0u)]
    [TestCase(0u, 8, 0u)]
    [TestCase(~0u, 8, ~0u)]
    [TestCase(0u, 7, 1u << 7)]
    [TestCase(~0u, 7, ~(1u << 7))]
    [TestCase(0u, 1, 1u << 1)]
    [TestCase(~0u, 1, ~(1u << 1))]
    [TestCase(0u, 0, 1u)]
    [TestCase(~0u, 0, ~1u)]
    public void BNOT_rr_Test(uint a, byte b, uint expcted) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        cpu.Registers[rs] = b;
        StoreRandomDest(dsp, 0, 200, a);
        RunOpcode(BNOT((Reg)rs, dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expcted);
    }

    [Test]
    [TestCase(0u, 15, 1u << 15)]
    [TestCase(~0u, 14, ~(1u << 14))]
    public void BNOT_ir_Test(uint a, byte b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = a;
        RunOpcode(BNOT(new UInt5(b), (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(21u, 0u, 1u << 21)]
    [TestCase(0u, 1u << 21, 1u << 21 | 1u)]
    [TestCase(1u, 0u, 1u << 1)]
    [TestCase(0xE0u, 1u, 0u)]
    public void BNOT_rr_Test(uint a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        RunOpcode(BNOT((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
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

    [Test]
    public void BRK_Test() {
        var random = TestContext.CurrentContext.Random;
        var intb = random.NextUInt(ramBeginAddress, ramBeginAddress + 1024);
        var interruptTable = random.NextUInt(ramBeginAddress + 3_000, ramBeginAddress + 10_000);
        memory.Write(intb, 4, interruptTable);
        cpu.INTB = intb;
        var prevPC = cpu.PC;
        var prevSP = cpu.SP;
        var afterPC = prevPC + 1;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        var prevPSW = cpu.PSW;
        RunOpcode(BRK);
        var nextPC = interruptTable;
        cpu.PC.Is(nextPC);
        cpu.PSW_u.Is(false);
        cpu.PSW_i.Is(false);
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        busManager.Read(cpu.SP, 4).Is(afterPC);
        busManager.Read(cpu.SP + 4, 4).Is(prevPSW);
        RunOpcode(RTE);
        cpu.PC.Is(afterPC);
        cpu.PSW.Is(prevPSW);
        cpu.SP.Is(prevSP);
    }

    [TestCase(0, 2u, 3u)]
    [TestCase(0, 0u, 1u)]
    [TestCase(7, 1u << 7,1u << 7)]
    [TestCase(0, 0xFFFF_FF02u, 0xFFFF_FF03u)]
    public void BSET_im_Test(byte a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 200, b);
        RunOpcode(BSET(new UInt3(a), dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(result);
    }

    [Test]
    [TestCase(25, 0u, 1u << 25)]
    [TestCase(8, 1u << 8,1u << 8)]
    public void BSET_ir_Test(byte a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        RunOpcode(BSET(new UInt5(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(0u, 0u, 0x01u)]
    [TestCase(0u, 1u, 0x01u)]
    [TestCase(1u, 0u, 0x02u)]
    [TestCase(7u, 0u, 1u << 7)]
    [TestCase(8u, 0u, 1u)]
    [TestCase(31u, 0u, 1u << 7)]
    [TestCase(8u, 0x1234_5678u, 0x1234_5678u | 1u)]
    [TestCase(31u, 0x1234_5678u, 0x1234_5678u | (1u << 7))]
    [TestCase(64u, 0x1234_5678u, 0x1234_5678u | 1u)]
    public void BSET_rm_Test(uint a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        cpu.Registers[rs] = a;
        StoreRandomDest(dsp, 0, 200, b);
        RunOpcode(BSET((Reg)rs, dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(result);
    }

    [Test]
    [TestCase(0u, 0u, 0x01u)]
    [TestCase(1u, 0u, 0x02u)]
    [TestCase(31u, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    [TestCase(31u, 0x7FFF_FFFFu, 0xFFFF_FFFFu)]
    public void BSET_rr_Test(uint a, uint b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        RunOpcode(BSET((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
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
    [TestCase(0, 0xFFu, true, false)]
    [TestCase(0, 0xFEu, false, true)]
    [TestCase(1, 0xFFu, true, false)]
    [TestCase(1, 0xFFFF_FFFFu, true, false)]
    [TestCase(1, 0x00u, false, true)]
    [TestCase(1, 0xFFFF_FFFDu, false, true)]
    [TestCase(7, 0u, false, true)]
    [TestCase(7, 1u << 7, true, false)]
    public void BTST_im_Test(byte a, uint b, bool expC, bool expZ) {
        var random = TestContext.CurrentContext.Random;
        var rs2 = random.Next(1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs2, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 200, b);
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        RunOpcode(BTST(new UInt3(a), dsp));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
    }

    [Test]
    [TestCase(0u, 0xFFu, true, false)]
    [TestCase(0u, 0xFEu, false, true)]
    [TestCase(8u, 0xFFu, true, false)]
    [TestCase(8u, 0xFEu, false, true)]
    [TestCase(16u, 0xFFu, true, false)]
    [TestCase(16u, 0xFEu, false, true)]
    [TestCase(24u, 0xFFu, true, false)]
    [TestCase(24u, 0xFEu, false, true)]
    [TestCase(32u, 0xFFu, true, false)]
    [TestCase(32u, 0xFEu, false, true)]
    [TestCase(0x80u, 0xFFu, true, false)]
    [TestCase(0x80u, 0xFEu, false, true)]
    [TestCase(0x8000u, 0xFFu, true, false)]
    [TestCase(0x8000u, 0xFEu, false, true)]
    [TestCase(0x0080_0000u, 0xFFu, true, false)]
    [TestCase(0x0080_0000u, 0xFEu, false, true)]
    [TestCase(0x8000_0000u, 0xFFu, true, false)]
    [TestCase(0x8000_0000u, 0xFEu, false, true)]

    [TestCase(1u, 0xFFu, true, false)]
    [TestCase(1u, 0xFFFF_FFFFu, true, false)]
    [TestCase(1u, 0x00u, false, true)]
    [TestCase(1u, 0xFFFF_FFFDu, false, true)]
    [TestCase(9u, 0xFFu, true, false)]
    [TestCase(9u, 0xFFFF_FFFFu, true, false)]
    [TestCase(9u, 0x00u, false, true)]
    [TestCase(9u, 0xFFFF_FFFDu, false, true)]
    [TestCase(17u, 0xFFu, true, false)]
    [TestCase(17u, 0xFFFF_FFFFu, true, false)]
    [TestCase(17u, 0x00u, false, true)]
    [TestCase(17u, 0xFFFF_FFFDu, false, true)]
    [TestCase(25u, 0xFFu, true, false)]
    [TestCase(25u, 0xFFFF_FFFFu, true, false)]
    [TestCase(25u, 0x00u, false, true)]
    [TestCase(25u, 0xFFFF_FFFDu, false, true)]
    [TestCase(33u, 0xFFu, true, false)]
    [TestCase(33u, 0xFFFF_FFFFu, true, false)]
    [TestCase(33u, 0x00u, false, true)]
    [TestCase(33u, 0xFFFF_FFFDu, false, true)]

    [TestCase(7u, 0u, false, true)]
    [TestCase(7u, 1u << 7, true, false)]
    [TestCase(15u, 0u, false, true)]
    [TestCase(15u, 1u << 7, true, false)]
    [TestCase(23u, 0u, false, true)]
    [TestCase(23u, 1u << 7, true, false)]
    [TestCase(31u, 0u, false, true)]
    [TestCase(31u, 1u << 7, true, false)]
    public void BTST_rm_Test(uint a, uint b, bool expC, bool expZ) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs2, MemEx.B, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        cpu.Registers[rs] = a;
        StoreRandomDest(dsp, 0, 200, b);
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        RunOpcode(BTST((Reg)rs, dsp));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
    }

    [Test]
    [TestCase(0, 0u, false, true)]
    [TestCase(0, 1u, true, false)]
    [TestCase(1, 0xFFu, true, false)]
    [TestCase(25, 0u, false, true)]
    [TestCase(25, 1u << 25, true, false)]
    [TestCase(31, 0u, false, true)]
    [TestCase(31, 1u << 31, true, false)]
    public void BTST_ir_Test(byte a, uint b, bool expC, bool expZ) {
        var random = TestContext.CurrentContext.Random;
        var rs2 = random.Next(1, 16);
        cpu.Registers[rs2] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        RunOpcode(BTST(new UInt5(a), (Reg)rs2));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
    }

    [Test]
    [TestCase(0u, 0u, false, true)]
    [TestCase(0u, 1u, true, false)]
    [TestCase(32u, 0u, false, true)]
    [TestCase(32u, 1u, true, false)]
    [TestCase(0x80u, 0u, false, true)]
    [TestCase(0x80u, 1u, true, false)]
    [TestCase(0x8000u, 0u, false, true)]
    [TestCase(0x8000u, 1u, true, false)]
    [TestCase(0x0080_0000u, 0u, false, true)]
    [TestCase(0x0080_0000u, 1u, true, false)]
    [TestCase(0x8000_0000u, 0u, false, true)]
    [TestCase(0x8000_0000u, 1u, true, false)]

    [TestCase(1u, 0xFFu, true, false)]
    [TestCase(33u, 0xFFu, true, false)]

    [TestCase(25u, 0u, false, true)]
    [TestCase(25u, 1u << 25, true, false)]
    [TestCase(57u, 0u, false, true)]
    [TestCase(57u, 1u << 25, true, false)]

    [TestCase(31u, 0u, false, true)]
    [TestCase(31u, 1u << 31, true, false)]
    [TestCase(63u, 0u, false, true)]
    [TestCase(63u, 1u << 31, true, false)]

    public void BTST_rr_Test(uint a, uint b, bool expC, bool expZ) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rs2] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        RunOpcode(BTST((Reg)rs, (Reg)rs2));
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
    [TestCase(0, 0u, true,  true, false, false)]
    [TestCase(1, 2u, true, false, false, false)]
    [TestCase(1, 5u, true, false, false, false)]
    [TestCase(5, 1u, false, false, true, false)]
    [TestCase(15, 14u, false, false, true, false)]
    public void CMP_4ir_Test(byte a, uint b,
                            bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs2 = random.Next(1, 16);
        cpu.Registers[rs2] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(CMP(new UInt4(a), (Reg)rs2));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(0, 0u, true,  true, false, false)]
    [TestCase(1, 5u, true, false, false, false)]
    [TestCase(5, 1u, false, false, true, false)]
    [TestCase(0x70, ~0u, true, false, true, false)]
    [TestCase(0x80, ~0u, true, false, true, false)]
    public void CMP_8ir_Test(byte a, uint b,
                            bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs2 = random.Next(1, 16);
        cpu.Registers[rs2] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(CMP(a, (Reg)rs2));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 0x70u, ~0u, true, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM8, 0x80u, ~0u, true, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7000u, ~0u, true, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x8000u, ~0u, true, false, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x70_0000u, ~0u, true, false, true, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x80_0000u, ~0u, true, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0x7000_0000u, ~0u, true, false, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 0x8000_0000u, ~0u, true, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0u, 0u, true,  true, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 1u, 5u, true, false, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 5u, 1u, false, false, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 0xFFFF_FC18u, 0x7FFF_FFFFu, false, false, true, true)]
    [TestCase(LengthOfImmediate.IMM32, 0x7FFF_FFFFu, 0xFFFF_FC18u, true, false, false, true)]
    public void CMP_ir_Test(LengthOfImmediate li, uint a, uint b,
                            bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs2 = random.Next(1, 16);
        cpu.Registers[rs2] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(CMP(new StdImmValue(li, a), (Reg)rs2));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(null, 0x70u,~0u, true, false, true, false)]
    [TestCase(null, 0x80u, ~0u, true, false, true, false)]
    [TestCase(MemEx.B, 0x70u, ~0u, true, false, true, false)]
    [TestCase(MemEx.B, 0x80u, ~0u, true, false, false, false)]
    [TestCase(MemEx.W, 0x7000u, ~0u, true, false, true, false)]
    [TestCase(MemEx.W, 0x8000u, ~0u, true, false, false, false)]
    [TestCase(MemEx.UW, 0x7000u, ~0u, true, false, true, false)]
    [TestCase(MemEx.UW, 0x8000u, ~0u, true, false, true, false)]
    [TestCase(MemEx.L, 0x7000_0000u, ~0u, true, false, true, false)]
    [TestCase(MemEx.L, 0x8000_0000u, ~0u, true, false, false, false)]
    [TestCase(MemEx.L, 0u, 0u, true,  true, false, false)]
    [TestCase(MemEx.L, 1u, 5u, true, false, false, false)]
    [TestCase(MemEx.L, 5u, 1u, false, false, true, false)]
    [TestCase(MemEx.L, 0xFFFF_FC18u, 0x7FFF_FFFFu, false, false, true, true)]
    [TestCase(MemEx.L, 0x7FFF_FFFFu, 0xFFFF_FC18u, true, false, false, true)]
    public void CMP_mr__CMP_ub_rs_mr_Test(MemEx? sz, uint a, uint b,
                         bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(CMP(dsp, (Reg)rd));
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, -126, 1260000, -10000, false)]
    [TestCase(LengthOfImmediate.SIMM8, 126, 1260000, 10000, false)]
    [TestCase(LengthOfImmediate.SIMM16, -4000, 4000, -1, false)]
    [TestCase(LengthOfImmediate.SIMM16, 300, 900, 3, false)]
    [TestCase(LengthOfImmediate.SIMM24, 300_000, 600_000, 2, false)]
    [TestCase(LengthOfImmediate.SIMM24, -300_000, 600_000, -2, false)]
    [TestCase(LengthOfImmediate.IMM32, 10, 400, 40, false)]
    [TestCase(LengthOfImmediate.IMM32, 5, 0, 0, false)]
    [TestCase(LengthOfImmediate.IMM32, 0,  5, null,  true)]
    [TestCase(LengthOfImmediate.IMM32, -1, -2147483648, null, true)]
    [TestCase(LengthOfImmediate.IMM32, 99, -2000, -20, false)]
    public void DIV_ir_Test(LengthOfImmediate li, int a, int b, int? result, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = unchecked((uint)b);
        cpu.PSW_o = random.NextBool();
        RunOpcode(DIV(new StdImmValue(li, unchecked((uint)a)), (Reg)rd));
        if (result.HasValue) {
            cpu.Registers[rd].Is(unchecked((uint) result.Value));
        }
        else {
            // 計算不能な場合は前の値を維持していることを確認する
            cpu.Registers[rd].Is(unchecked((uint)b));
        }
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(null, 100, 60000, 600, false)]
    [TestCase(null, 200, 60000, 300, false)]
    [TestCase(MemEx.B, -126, 1260000, -10000, false)]
    [TestCase(MemEx.B, 126, 1260000, 10000, false)]
    [TestCase(MemEx.W, -4000, 4000, -1, false)]
    [TestCase(MemEx.W, 300, 900, 3, false)]
    [TestCase(MemEx.UW, 40000, 40000, 1, false)]
    [TestCase(MemEx.UW, 300, 900, 3, false)]
    [TestCase(MemEx.L, 10, 400, 40, false)]
    [TestCase(MemEx.L, 5, 0, 0, false)]
    [TestCase(MemEx.L, 0,  5, null,  true)]
    [TestCase(MemEx.L, -1, -2147483648, null, true)]
    [TestCase(MemEx.L, 99, -2000, -20, false)]
    public void DIV_mr__DIV_ub_rs_mr_Test(MemEx? sz, int a, int b, int? result, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, unchecked((uint)a));
        cpu.Registers[rd] = unchecked((uint)b);
        cpu.PSW_o = random.NextBool();
        RunOpcode(DIV(dsp, (Reg)rd));
        if (result.HasValue) {
            cpu.Registers[rd].Is(unchecked((uint) result.Value));
        }
        else {
            // 計算不能な場合は前の値を維持していることを確認する
            cpu.Registers[rd].Is(unchecked((uint)b));
        }
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 0x70u, 0xFFFF_FFFFu, 0x249_2492u, false)]
    [TestCase(LengthOfImmediate.SIMM8, 0x80u, 0xFFFF_FFFFu, 1u, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7000u, 0xFFFF_FFFFu, 0x2_4924u, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x8000u, 0xFFFF_FFFFu, 1u, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x70_0000u, 0xFFFF_FFFF, 0x249u, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x80_0000u, 0xFFFF_FFFF, 1u, false)]
    [TestCase(LengthOfImmediate.IMM32, 10u, 400u, 40u, false)]
    [TestCase(LengthOfImmediate.IMM32, 0u, 5u, null, true)]
    // -2000 -> 0xFFFFF830 -> 4294965296
    [TestCase(LengthOfImmediate.IMM32, 236u, unchecked((uint)-2000), 18199005u, false)]
    public void DIVU_ir_Test(LengthOfImmediate li, uint a, uint b, uint? result, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        RunOpcode(DIVU(new StdImmValue(li, a), (Reg)rd));
        if (result.HasValue) {
            cpu.Registers[rd].Is(result.Value);
        }
        else {
            // 計算不能な場合は前の値を維持していることを確認する
            cpu.Registers[rd].Is(b);
        }
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(null, 0x70u, 0xFFFF_FFFFu, 0x249_2492u, false)]
    [TestCase(null, 0x80u, 0xFFFF_FFFFu, 0x1FFFFFFu, false)]
    [TestCase(MemEx.B, 0x70u, 0xFFFF_FFFFu, 0x249_2492u, false)]
    [TestCase(MemEx.B, 0x80u, 0xFFFF_FFFFu, 1u, false)]
    [TestCase(MemEx.W, 0x7000u, 0xFFFF_FFFFu, 0x2_4924u, false)]
    [TestCase(MemEx.W, 0x8000u, 0xFFFF_FFFFu, 1u, false)]
    [TestCase(MemEx.UW, 20000u, 80000u, 4u, false)]
    [TestCase(MemEx.UW, 40000u, 80000u, 2u, false)]
    [TestCase(MemEx.L, 10u, 400u, 40u, false)]
    [TestCase(MemEx.L, 0u, 5u, null, true)]
    // -2000 -> 0xFFFFF830 -> 4294965296
    [TestCase(MemEx.L, 236u, unchecked((uint)-2000), 18199005u, false)]
    public void DIVU_mr__DIVU_ub_rs_mr_Test(MemEx? sz, uint a, uint b, uint? result, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        RunOpcode(DIVU(dsp, (Reg)rd));
        if (result.HasValue) {
            cpu.Registers[rd].Is(result.Value);
        }
        else {
            // 計算不能な場合は前の値を維持していることを確認する
            cpu.Registers[rd].Is(b);
        }
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 100, 1000, 100L * 1000L)]
    [TestCase(LengthOfImmediate.SIMM8, -100, 1000, -100L * 1000L)]
    [TestCase(LengthOfImmediate.SIMM16, 20000, 2000, 20000L * 2000L)]
    [TestCase(LengthOfImmediate.SIMM16, -1000, 2000, -1000L * 2000L)]
    [TestCase(LengthOfImmediate.SIMM24, 300000, 2000, 300000L * 2000L)]
    [TestCase(LengthOfImmediate.SIMM24, -300000, 2000, -300000L * 2000L)]
    [TestCase(LengthOfImmediate.IMM32, 1000000, 99999, 1000000L * 99999L)]
    [TestCase(LengthOfImmediate.IMM32, -100000, 99999, -100000L * 99999L)]
    public void EMUL_ir_Test(LengthOfImmediate li, int a, int b, long result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 14);
        cpu.Registers[rd] = unchecked((uint)b);
        RunOpcode(EMUL(new StdImmValue(li, (uint)a), (Reg)rd));
        cpu.Registers[rd + 1].Is((uint)((ulong)result >> 32));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(null, 100, 1000, 100L * 1000L)]
    [TestCase(null, 200, 1000, 200L * 1000L)]
    [TestCase(MemEx.B, 100, 1000, 100L * 1000L)]
    [TestCase(MemEx.B, -100, 1000, -100L * 1000L)]
    [TestCase(MemEx.W, 20000, 2000, 20000L * 2000L)]
    [TestCase(MemEx.W, -1000, 2000, -1000L * 2000L)]
    [TestCase(MemEx.UW, 20000, 2000, 20000L * 2000L)]
    [TestCase(MemEx.UW, 40000, 2000, 40000L * 2000L)]
    [TestCase(MemEx.L, 1000000, 99999, 1000000L * 99999L)]
    [TestCase(MemEx.L, -100000, 99999, -100000L * 99999L)]
    public void EMUL_mr__EMUL_ub_rs_mr_Test(MemEx? sz, int a, int b, long result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 13);
        var rd = random.Next(rs + 1, 14);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, unchecked((uint)a));
        cpu.Registers[rd] = unchecked((uint)b);
        RunOpcode(EMUL(dsp, (Reg)rd));
        cpu.Registers[rd + 1].Is((uint)((ulong)result >> 32));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 0x70u, 1000u, 0x70Lu * 1000Lu)]
    [TestCase(LengthOfImmediate.SIMM8, 0x80u, 1000u, 0xFFFF_FF80Lu * 1000Lu)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7012u, 1000u, 0x7012Lu * 1000Lu)]
    [TestCase(LengthOfImmediate.SIMM16, 0x8012u, 1000u, 0xFFFF_8012Lu * 1000Lu)]
    [TestCase(LengthOfImmediate.SIMM24, 0x70_1234u, 1000u, 0x70_1234Lu * 1000Lu)]
    [TestCase(LengthOfImmediate.SIMM24, 0x80_1234u, 1000u, 0xFF80_1234Lu * 1000Lu)]
    [TestCase(LengthOfImmediate.IMM32, 1000000u, 99999u, 1000000Lu * 99999Lu)]
    public void EMULU_ir_Test(LengthOfImmediate li, uint a, uint b, ulong result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 14);
        cpu.Registers[rd] = b;
        RunOpcode(EMULU(new StdImmValue(li, a), (Reg)rd));
        cpu.Registers[rd + 1].Is((uint)(result >> 32));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(null, 0x70u, 1000u, 0x70Lu * 1000Lu)]
    [TestCase(null, 200u, 1000u, 200Lu * 1000Lu)]
    [TestCase(MemEx.B, 0x70u, 1000u, 0x70Lu * 1000Lu)]
    [TestCase(MemEx.B, 0x80u, 1000u, 0xFFFF_FF80Lu * 1000Lu)]
    [TestCase(MemEx.W, 0x7012u, 1000u, 0x7012Lu * 1000Lu)]
    [TestCase(MemEx.W, 0x8012u, 1000u, 0xFFFF_8012Lu * 1000Lu)]
    [TestCase(MemEx.UW, 0x7012u, 1000u, 0x7012Lu * 1000Lu)]
    [TestCase(MemEx.UW, 0x8012u, 1000u, 0x8012Lu * 1000Lu)]
    [TestCase(MemEx.L, 1000000u, 99999u, 1000000Lu * 99999Lu)]
    public void EMULU_mr__EMULU_ub_rs_mr_Test(MemEx? sz, uint a, uint b, ulong result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 13);
        var rd = random.Next(rs + 1, 14);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        RunOpcode(EMULU(dsp, (Reg)rd));
        cpu.Registers[rd + 1].Is((uint)(result >> 32));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [TestCase(3.55f, 0.5f, 4.05f)]
    public void FADD_ir_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FADD(a, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase(3.55f, 0.5f, 4.05f)]
    public void FADD_mr_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, BitConverter.SingleToUInt32Bits(a));
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FADD(dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase(5.5f, 5.3f, false, true, false)]
    [TestCase(5.3f, 5.5f, false, false, false)]
    [TestCase(5.3f, 5.3f, false, false, true)]
    public void FCMP_ir_Test(float a, float b,
                          bool expO, bool expS, bool expZ) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        cpu.PSW_o = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_z = random.NextBool();
        RunOpcode(FCMP(a, (Reg)rd));
        //cpu.PSW_o.Is(expO); // TODO 修正する
        cpu.PSW_s.Is(expS);
        cpu.PSW_z.Is(expZ);
    }

    [Test]
    [TestCase(5.5f, 5.3f, false, true, false)]
    [TestCase(5.3f, 5.5f, false, false, false)]
    [TestCase(5.3f, 5.3f, false, false, true)]
    public void FCMP_mr_Test(float a, float b,
                          bool expO, bool expS, bool expZ) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, BitConverter.SingleToUInt32Bits(a));
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        cpu.PSW_o = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_z = random.NextBool();
        RunOpcode(FCMP(dsp, (Reg)rd));
        //cpu.PSW_o.Is(expO); // TODO 修正する
        cpu.PSW_s.Is(expS);
        cpu.PSW_z.Is(expZ);
    }

    [Test]
    [TestCase( 2.0f, 6.0f,3.0f)]
    [TestCase( 2.0f, 1.0f,0.5f)]
    [TestCase( -2.0f, 1.0f, -0.5f)]
    [TestCase(2.0f, -1.0f, -0.5f)]
    [TestCase(-2.0f, -1.0f, 0.5f)]
    public void FDIV_ir_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FDIV(a, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase( 2.0f, 6.0f,3.0f)]
    [TestCase( 2.0f, 1.0f,0.5f)]
    [TestCase( -2.0f, 1.0f, -0.5f)]
    [TestCase(2.0f, -1.0f, -0.5f)]
    [TestCase(-2.0f, -1.0f, 0.5f)]
    public void FDIV_mr_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, BitConverter.SingleToUInt32Bits(a));
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FDIV(dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase( 100.0f,  15.34f,  1534.0f)]
    [TestCase(-100.0f,  15.34f, -1534.0f)]
    [TestCase( 100.0f, -15.34f, -1534.0f)]
    [TestCase(-100.0f, -15.34f,  1534.0f)]
    public void FMUL_ir_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FMUL(a, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase( 100.0f,  15.34f,  1534.0f)]
    [TestCase(-100.0f,  15.34f, -1534.0f)]
    [TestCase( 100.0f, -15.34f, -1534.0f)]
    [TestCase(-100.0f, -15.34f,  1534.0f)]
    public void FMUL_mr_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, BitConverter.SingleToUInt32Bits(a));
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FMUL(dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase(0.5f,  5.5f, 5.0f)]
    [TestCase(-0.5f, 5.5f, 6.0f)]
    [TestCase(0.5f, -5.5f, -6.0f)]
    public void FSUB_ir_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FSUB(a, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase(0.5f,  5.5f, 5.0f)]
    [TestCase(-0.5f, 5.5f, 6.0f)]
    [TestCase(0.5f, -5.5f, -6.0f)]
    public void FSUB_mr_Test(float a, float b, float result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, BitConverter.SingleToUInt32Bits(a));
        cpu.Registers[rd] = BitConverter.SingleToUInt32Bits(b);
        RunOpcode(FSUB(dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
    }

    [Test]
    [TestCase(10.6f, 10)]
    [TestCase(10.4f, 10)]
    [TestCase(9.99f, 9)]
    [TestCase(-9.99f, -9)]
    public void FTOI_Test(float a, int result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, BitConverter.SingleToUInt32Bits(a));
        RunOpcode(FTOI(dsp, (Reg)rd));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [Test]
    public void INT_Test([Random(0, 255, 10)]byte src) {
        var random = TestContext.CurrentContext.Random;
        var intb = random.NextUInt(ramBeginAddress, ramBeginAddress + 1024);
        var interruptTable = Enumerable.Range(0, 256)
            .Select(_ => random.NextUInt(ramBeginAddress + 3_000, ramBeginAddress + 10_000))
            .ToArray();
        for (uint i = 0; i < 256; i++) {
            memory.Write(intb + i * 4, 4, interruptTable[i]);
        }
        cpu.INTB = intb;
        var prevPC = cpu.PC;
        var prevSP = cpu.SP;
        var afterPC = prevPC + 3;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        var prevPSW = cpu.PSW;
        RunOpcode(INT(src));
        var nextPC = interruptTable[src];
        cpu.PC.Is(nextPC);
        cpu.PSW_u.Is(false);
        cpu.PSW_i.Is(false);
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        busManager.Read(cpu.SP, 4).Is(afterPC);
        busManager.Read(cpu.SP + 4, 4).Is(prevPSW);
        RunOpcode(RTE);
        cpu.PC.Is(afterPC);
        cpu.PSW.Is(prevPSW);
        cpu.SP.Is(prevSP);
    }

    [Test]
    [TestCase(MemEx.B, 50, 50.0f)]
    [TestCase(MemEx.B, -50, -50.0f)]
    [TestCase(MemEx.W, -20000, -20000.0f)]
    [TestCase(MemEx.W, 20000, 20000.0f)]
    [TestCase(MemEx.UW, 40000, 40000.0f)]
    [TestCase(MemEx.UW, 20000, 20000.0f)]
    [TestCase(MemEx.L, 70000, 70000.0f)]
    [TestCase(MemEx.L, 5, 5.0f)]
    [TestCase(MemEx.L, -5, -5.0f)]
    public void ITOF_Test(MemEx sz, int a, float result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, unchecked((uint)a));
        RunOpcode(ITOF(dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitConverter.SingleToUInt32Bits(result));
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
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        cpu.Acc = unchecked((ulong)acc);
        cpu.Registers[rs] = (uint)a << 16;
        cpu.Registers[rs2] = (uint)b << 16;
        RunOpcode(MACHI((Reg)rs, (Reg)rs2));
        cpu.Acc.Is(unchecked((ulong)result));
    }

    [Test]
    [TestCase(10, 20, 100, (2000 << 16) + 10)]
    [TestCase(-100000, -2000, 500, ((-1000000L) << 16) -100000)]
    public void MACLO_Test(long acc, short a, short b, long result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        cpu.Acc = unchecked((ulong)acc);
        cpu.Registers[rs] = (uint)a;
        cpu.Registers[rs2] = (uint)b;
        RunOpcode(MACLO((Reg)rs, (Reg)rs2));
        cpu.Acc.Is(unchecked((ulong)result));
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 120, 50, 120)]
    [TestCase(LengthOfImmediate.SIMM8, -120, 50, 50)]
    [TestCase(LengthOfImmediate.SIMM16, 300, 260, 300)]
    [TestCase(LengthOfImmediate.SIMM16, -300, 260, 260)]
    [TestCase(LengthOfImmediate.SIMM16, -300, -3000, -300)]
    [TestCase(LengthOfImmediate.SIMM24, 999999, 999998, 999999)]
    [TestCase(LengthOfImmediate.SIMM24, 999999, 1000000, 1000000)]
    [TestCase(LengthOfImmediate.SIMM24, -999999, -999998, -999998)]
    [TestCase(LengthOfImmediate.SIMM24, -999999, -1000000, -999999)]
    [TestCase(LengthOfImmediate.SIMM24, -999999, 1000000, 1000000)]
    [TestCase(LengthOfImmediate.IMM32, 1000000, -5, 1000000)]
    [TestCase(LengthOfImmediate.IMM32, -150000, -100, -100)]
    public void MAX_ir_Test(LengthOfImmediate li, int a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = unchecked((uint)b);
        RunOpcode(MAX(new StdImmValue(li, unchecked((uint)a)), (Reg)rd));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(null, 120, 50, 120)]
    [TestCase(null, 200, 50, 200)]
    [TestCase(null, 200, 300, 300)]
    [TestCase(null, 200, -300, 200)]
    [TestCase(MemEx.B, 120, 50, 120)]
    [TestCase(MemEx.B, -120, 50, 50)]
    [TestCase(MemEx.W, 300, 260, 300)]
    [TestCase(MemEx.W, -300, 260, 260)]
    [TestCase(MemEx.W, -300, -3000, -300)]
    [TestCase(MemEx.UW, 40000, 41000, 41000)]
    [TestCase(MemEx.UW, 42000, 41000, 42000)]
    [TestCase(MemEx.L, 1000000, -5, 1000000)]
    [TestCase(MemEx.L, -150000, -100, -100)]
    public void MAX_mr__MAX_ub_rs_mr_Test(MemEx? sz, int a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, unchecked((uint)a));
        cpu.Registers[rd] = unchecked((uint)b);
        RunOpcode(MAX(dsp, (Reg)rd));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [TestCase(LengthOfImmediate.SIMM8, -120, 120, -120)]
    [TestCase(LengthOfImmediate.SIMM8, -120, -2000, -2000)]
    [TestCase(LengthOfImmediate.SIMM8, 80, 120, 80)]
    [TestCase(LengthOfImmediate.SIMM16, -20000, 20000, -20000)]
    [TestCase(LengthOfImmediate.SIMM16, 20000, 20001, 20000)]
    [TestCase(LengthOfImmediate.SIMM24, 800_000, 799_999, 799_999)]
    [TestCase(LengthOfImmediate.SIMM24, 800_000, 800_000, 800_000)]
    [TestCase(LengthOfImmediate.SIMM24, 800_000, 800_001, 800_000)]
    [TestCase(LengthOfImmediate.SIMM24, 800_001, 800_000, 800_000)]
    [TestCase(LengthOfImmediate.SIMM24, 800_001, 800_002, 800_001)]
    [TestCase(LengthOfImmediate.SIMM24, -800_000, -799_999, -800_000)]
    [TestCase(LengthOfImmediate.SIMM24, -800_000, -800_000, -800_000)]
    [TestCase(LengthOfImmediate.SIMM24, -800_000, -800_001, -800_001)]
    [TestCase(LengthOfImmediate.IMM32, 1000000, -5, -5)]
    [TestCase(LengthOfImmediate.IMM32, -150000, -100, -150000)]
    [TestCase(LengthOfImmediate.IMM32, 200, 3, 3)]
    public void MIN_ir_Test(LengthOfImmediate li, int a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = unchecked((uint)b);
        RunOpcode(MIN(new StdImmValue(li, unchecked((uint)a)), (Reg)rd));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(null, 80, 120, 80)]
    [TestCase(null, 200, 120, 120)]
    [TestCase(null, 200, 300, 200)]
    [TestCase(null, 200, -300, -300)]
    [TestCase(MemEx.B, -120, 120, -120)]
    [TestCase(MemEx.B, -120, -2000, -2000)]
    [TestCase(MemEx.B, 80, 120, 80)]
    [TestCase(MemEx.W, -20000, 20000, -20000)]
    [TestCase(MemEx.W, 20000, 20001, 20000)]
    [TestCase(MemEx.UW, 40000, 40001, 40000)]
    [TestCase(MemEx.UW, 40000, 39999, 39999)]
    [TestCase(MemEx.UW, 0, 40000, 0)]
    [TestCase(MemEx.L, 1000000, -5, -5)]
    [TestCase(MemEx.L, -150000, -100, -150000)]
    [TestCase(MemEx.L, 200, 3, 3)]
    public void MIN_mr__MIN_ub_rs_mr_Test(MemEx? sz, int a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, unchecked((uint)a));
        cpu.Registers[rd] = unchecked((uint)b);
        RunOpcode(MIN(dsp, (Reg)rd));
        cpu.Registers[rd].Is(unchecked((uint)result));
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
        var rs = random.Next(1, 7);
        var rd = random.Next(rs + 1, 8);
        var dspOffset = random.Next(0, 32);
        var dsp = new RegAddressing5((byte)dspOffset, (Reg)rd);
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
        var rs = random.Next(1, 8);
        var rd = random.Next(1, 8);
        var dspOffset = random.Next(0, 32);
        var dsp = new RegAddressing5((byte)dspOffset, (Reg)rs);
        StoreRandomDest(new RelRef8(dsp.Displacement, dsp.TargetReg, MemEx.L), 0, 300, value);
        RunOpcode(MOV(sz, dsp, (Reg)rd));
        cpu.Registers[rd].Is(expected);
    }

    [Test]
    public void MOV_4ir_reg_Test(
        [Random((byte)0, (byte)16, 5)]byte imm,
        [Random((byte)1, (byte)16, 5)]byte reg) {
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
        var rd = random.Next(1, 8);
        var dspOffset = random.Next(0, 32);
        var initial = random.NextUInt();
        var dsp = new RegAddressing5((byte)dspOffset, (Reg)rd);
        // mov命令は指定したサイズで上書き出来ていることを確認するため、乱数を設定する
        StoreRandomDest(new RelRef8(dsp.Displacement, dsp.TargetReg, MemEx.L), 0, 300, initial);
        RunOpcode(MOV(sz, src, dsp));
        LoadData(cpu.Registers, busManager, LengthOfDisplacement.DSP8Reg, (uint)sz, dsp.Displacement, dsp.TargetReg)
        .Is(expected);
    }

    [Test]
    public void MOV_u8ir_Test([Random(byte.MinValue, byte.MaxValue, 5)]byte src) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
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
        var rd = random.Next(1, 16);
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
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
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
        var rd = random.Next(1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, size, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 300, initial);
        RunOpcode(MOV(size, imm, dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expected);
    }

    [Test]
    [TestCase(MemEx.B, 0x71u, 0x71u)]
    [TestCase(MemEx.B, 0x82u, 0xFFFF_FF82u)]
    [TestCase(MemEx.B, 0x1234_5678u, 0x78u)]
    [TestCase(MemEx.W, 0x75FFu, 0x75FFu)]
    [TestCase(MemEx.W, 0x8123u, 0xFFFF_8123u)]
    [TestCase(MemEx.W, 0x1234_5678u, 0x5678u)]
    [TestCase(MemEx.L, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(MemEx.L, 0x0u, 0x0u)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0xFFFF_FFFFu)]
    public void MOV_l_mr_Test(MemEx size, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, size, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 300, src);
        RunOpcode(MOV(size, dsp, (Reg)rd));
        cpu.Registers[rd].Is(expected);
        // src が変化していないことを確認する
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(src);
    }

    [Test]
    [TestCase(MemEx.B, 0x1234_5678u, 0x78u)]
    [TestCase(MemEx.B, 0x70u, 0x70u)]
    [TestCase(MemEx.B, 0x80u, 0xFFFF_FF80u)]
    [TestCase(MemEx.W, 0x1234_5678u, 0x5678u)]
    [TestCase(MemEx.W, 0x7012u, 0x7012u)]
    [TestCase(MemEx.W, 0x8012u, 0xFFFF_8012u)]
    [TestCase(MemEx.L, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(MemEx.L, 0x0u, 0x0u)]
    [TestCase(MemEx.L, 0xFFFF_FFFF, 0xFFFF_FFFF)]
    public void MOV_ar_Test(MemEx sz, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var ri = random.Next(1, 14);
        var rb = random.Next(ri + 1, 15);
        var rd = random.Next(rb + 1, 16);

        var baseAddr = random.NextUInt(0, 150);
        var indexValue = random.NextUInt(0, 10);
        var addr = baseAddr + (indexValue << (int)sz);
        cpu.Registers[rb] = baseAddr;
        cpu.Registers[ri] = indexValue;
        busManager.Write(addr, 4, src);
        RunOpcode(MOV_indexed(sz, (Reg)rb, (Reg)ri, (Reg)rd));
        cpu.Registers[rd].Is(expected);
        // 変化していないことを確認する
        busManager.Read(addr, 4).Is(src);
        cpu.Registers[ri].Is(indexValue);
    }

    [Test]
    [TestCase(MemEx.B, 0x0u, 0x1234_5678u, 0x78u)]
    [TestCase(MemEx.B, 0x0u, 0x80u, 0x80u)]
    [TestCase(MemEx.B, 0x0u, 0xFFu, 0xFFu)]
    [TestCase(MemEx.B, 0xFFFF_FFFFu, 0x0u, 0xFFFF_FF00u)]
    [TestCase(MemEx.B, 0x0u, 0xFFFF_FFFFu, 0xFFu)]
    [TestCase(MemEx.B, 0x5678_9ABCu, 0x1234_5678u, 0x5678_9A78u)]
    [TestCase(MemEx.W, 0x0u, 0x1234_5678u, 0x5678u)]
    [TestCase(MemEx.W, 0x0u, 0xFFFFu, 0xFFFFu)]
    [TestCase(MemEx.W, 0x5678_9ABCu, 0x1234_5678u, 0x5678_5678u)]
    [TestCase(MemEx.L, 0x0u, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0x0u, 0u)]
    [TestCase(MemEx.L, 0x0u, 0xFFFF_FFFFu, 0x0FFFF_FFFF)]
    public void MOV_r_dsp_Test(MemEx sz, uint initial, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        cpu.Registers[rs] = src;
        StoreRandomDest(dsp, 0, 300, initial);
        RunOpcode(MOV(sz, (Reg)rs, dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expected);
    }

    [Test]
    [TestCase(MemEx.B, 0x00u, 0x71u, 0x71u)]
    [TestCase(MemEx.B, 0x00u, 0xFFu, 0xFFu)]
    [TestCase(MemEx.B, 0x00u, 0x1234_5678u, 0x78u)]
    [TestCase(MemEx.B, 0xFFFF_FFFFu, 0x00u, 0xFFFF_FF00u)]
    [TestCase(MemEx.B, 0xFFFF_FFFFu, 0x23u, 0xFFFF_FF23u)]
    [TestCase(MemEx.W, 0x00u, 0x1234_5678u, 0x5678u)]
    [TestCase(MemEx.W, 0xFFFF_FFFFu, 0x1234_5678u, 0xFFFF_5678u)]
    [TestCase(MemEx.W, 0xFFFF_FFFFu, 0x00u, 0xFFFF_0000u)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0x00u, 0x00u)]
    [TestCase(MemEx.L, 0x00u, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(MemEx.L, 0x1234_5678u, 0xABCD_EF01u, 0xABCD_EF01u)]
    public void MOV_ra_Test(MemEx sz, uint initial, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 14);
        var ri = random.Next(rs + 1, 15);
        var rb = random.Next(ri + 1, 16);

        var baseAddr = random.NextUInt(0, 150);
        var indexValue = random.NextUInt(0, 10);
        var addr = baseAddr + (indexValue << (int)sz);
        cpu.Registers[rb] = baseAddr;
        cpu.Registers[ri] = indexValue;
        busManager.Write(addr, 4, initial);
        cpu.Registers[rs] = src;
        RunOpcode(MOV_indexedDest(sz, (Reg)rs, (Reg)ri, (Reg)rb));
        busManager.Read(addr, 4).Is(expected);
    }

    [Test]
    [TestCase(MemEx.B, 0x00u, 0xFEu, 0xFEu)]
    [TestCase(MemEx.B, 0x00u, 0x1234u, 0x34u)]
    [TestCase(MemEx.B, 0xFFFF_FFFF, 0x00u, 0xFFFF_FF00u)]
    [TestCase(MemEx.W, 0x00u, 0xFEDCu, 0xFEDCu)]
    [TestCase(MemEx.W, 0x00u, 0x1234_5678u, 0x5678u)]
    [TestCase(MemEx.W, 0xFFFF_FFFFu, 0xFEDCu, 0xFFFF_FEDCu)]
    [TestCase(MemEx.L, 0x00u, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0x1234_5678u, 0x1234_5678u)]
    [TestCase(MemEx.L, 0x1248_2356u, 0x00u, 0x00u)]
    public void MOV_mm_Test(MemEx sz, uint initial, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var dspS = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);

        var rd = random.Next(rs + 1, 16);
        var dspD = GetRandomStdRegAddressing((Reg)rd, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);

        StoreRandomDest(dspD, 0, 100, initial);
        StoreRandomDest(dspS, 160, 200, src);
        RunOpcode(MOV(sz, dspS, dspD));
        LoadData(cpu.Registers, busManager, dspD.LD, (int)MemEx.L, dspD.Displacement, dspD.TargetReg)
            .Is(expected);        
    }

    [Test]
    [TestCase(Addressing.PostInc, MemEx.B, 0x0u, 0xFFu, 1, 0xFFu)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x0u, 0xFFFF_FFFFu, 1, 0xFFu)]
    [TestCase(Addressing.PostInc, MemEx.B, 0xFFu, 0x0u, 1, 0x0u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0xFFu, 0xFFFF_FF00u, 1, 0x0u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0xFFFF_FFFFu, 0x0u, 1, 0xFFFF_FF00u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x0u, 0x12u, 1, 0x12u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x0u, 0xFFFFu, 2, 0xFFFFu)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x0u, 0xFFFF_0000u, 2, 0x00u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x0u, 0x1234u, 2, 0x1234u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0xFFFF_FFFFu, 0x0u, 2, 0xFFFF_0000u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0xFFFF_FFFFu, 0x1234u, 2, 0xFFFF_1234u)]
    [TestCase(Addressing.PostInc, MemEx.L, 0x0u, 0x1234_5678u, 4, 0x1234_5678u)]
    [TestCase(Addressing.PostInc, MemEx.L, 0xFFFF_FFFFu, 0x0u, 4, 0x0u)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x0u, 0xFFu, -1, 0xFFu)]
    [TestCase(Addressing.PreDec, MemEx.B, 0xFFFF_FFFFu, 0x0u, -1, 0xFFFF_FF00u)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x0u, 0xABu, -1, 0xABu)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x0u, 0x1234_5678u, -1, 0x78u)]
    [TestCase(Addressing.PreDec, MemEx.W, 0x0u, 0xFFFFu, -2, 0xFFFFu)]
    [TestCase(Addressing.PreDec, MemEx.W, 0xFFFF_FFFFu, 0x0u, -2, 0xFFFF_0000u)]
    [TestCase(Addressing.PreDec, MemEx.W, 0x0u, 0x1234_5678u, -2, 0x5678u)]
    [TestCase(Addressing.PreDec, MemEx.L, 0x0u, 0x1234_5678u, -4, 0x1234_5678u)]
    [TestCase(Addressing.PreDec, MemEx.L, 0xFFFF_FFFFu, 0x0u, -4, 0x0u)]
    [TestCase(Addressing.PreDec, MemEx.L, 0x0u, 0xFFFF_FFFFu, -4, 0xFFFF_FFFFu)]
    public void MOV_rp_Test(Addressing ad, MemEx sz, uint initial, uint src, int afterAddrOffset, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var addr = random.Next(20, 260);

        busManager.Write((uint)addr, 4, initial);
        cpu.Registers[rs] = src;
        cpu.Registers[rd] = (uint)addr;

        RunOpcode(MOV(sz, (Reg)rs, ad, (Reg)rd));
        cpu.Registers[rd].Is((uint)(addr + afterAddrOffset));
        busManager.Read((uint)(addr + (ad == Addressing.PreDec ? afterAddrOffset : 0)), 4).Is(expected);
    }

    [Test]
    [TestCase(Addressing.PostInc, MemEx.B, 0x1u, 1, 0x1u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x7Fu, 1, 0x7Fu)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x1234_5678u, 1, 0x78u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x80u, 1, 0xFFFF_FF80u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0xFFu, 1, 0xFFFF_FFFFu)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x1234_5678u, 2, 0x5678u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x70FFu, 2, 0x70FFu)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x8000u, 2, 0xFFFF_8000u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0xFFFF_0000u, 2, 0x0u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0xFFFFu, 2, 0xFFFF_FFFFu)]
    [TestCase(Addressing.PostInc, MemEx.L, 0x1234_5678u, 4, 0x1234_5678u)]
    [TestCase(Addressing.PostInc, MemEx.L, 0xFFFF_FFFFu, 4, 0xFFFF_FFFFu)]
    [TestCase(Addressing.PostInc, MemEx.L, 0x0u, 4, 0x0u)]
    [TestCase(Addressing.PostInc, MemEx.L, 0x7FFF_FFFFu, 4, 0x7FFF_FFFFu)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x1u, -1, 0x1u)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x7Fu, -1, 0x7Fu)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x80u, -1, 0xFFFF_FF80u)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x1234_5680u, -1, 0xFFFF_FF80u)]
    [TestCase(Addressing.PreDec, MemEx.B, 0x0u, -1, 0x0u)]
    [TestCase(Addressing.PreDec, MemEx.W, 0x0u, -2, 0x0u)]
    [TestCase(Addressing.PreDec, MemEx.W, 0x7FFFu, -2, 0x7FFFu)]
    [TestCase(Addressing.PreDec, MemEx.W, 0x8000u, -2, 0xFFFF_8000u)]
    [TestCase(Addressing.PreDec, MemEx.W, 0x1234_8000u, -2, 0xFFFF_8000u)]
    [TestCase(Addressing.PreDec, MemEx.W, 0xFFFF_6787u, -2, 0x6787u)]
    [TestCase(Addressing.PreDec, MemEx.L, 0xFFFF_FFFFu, -4, 0xFFFF_FFFFu)]
    [TestCase(Addressing.PreDec, MemEx.L, 0x1234_5678u, -4, 0x1234_5678u)]
    [TestCase(Addressing.PreDec, MemEx.L, 0x7234_5678u, -4, 0x7234_5678u)]
    [TestCase(Addressing.PreDec, MemEx.L, 0x0u, -4, 0x0u)]
    public void MOV_pr_Test(Addressing ad, MemEx sz, uint src, int afterAddrOffset, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var addr = random.Next(20, 260);

        busManager.Write((uint)(addr + (ad == Addressing.PreDec ? afterAddrOffset : 0)), 4, src);
        cpu.Registers[rs] = (uint)addr;
        cpu.Registers[rd] = 0;

        RunOpcode(MOV(sz, ad, (Reg)rs, (Reg)rd));
        cpu.Registers[rs].Is((uint)(addr + afterAddrOffset));
        cpu.Registers[rd].Is(expected);
    }

    [Test]
    public void MOVU_dsp5_reg_Test(
        [Values(MemEx.B, MemEx.W)] MemEx size,
        [Random((uint)ushort.MinValue, (uint)ushort.MaxValue, 3)] uint value,
        [Random(0u, 260_000u, 2)] uint addr,
        [Random((byte)0, (byte)32, 3)]byte dsp_offset,
        [Random(1, 8, 3)]int dsp_reg,
        [Random(1, 8, 2)]int rd) {
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
        [Random(1, 8, 2)]int rd) {
        var random = TestContext.CurrentContext.Random;
        var reg = random.Next(1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)reg, size, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, value);
        RunOpcode(MOVU(size, dsp, (Reg)rd));
        cpu.Registers[rd].Is(BitOperation.GetLowerBits(value, GetSize(size)));
    }

    [TestCase(MemEx.B, 0x0u, 0x0u)]
    [TestCase(MemEx.B, 0x1u, 0x1u)]
    [TestCase(MemEx.B, 0x7Fu, 0x7Fu)]
    [TestCase(MemEx.B, 0x80u, 0x80u)]
    [TestCase(MemEx.B, 0xFFFF_FF80u, 0x80u)]
    [TestCase(MemEx.B, 0x1234_5678u, 0x78u)]
    [TestCase(MemEx.B, 0xFFu, 0xFFu)]
    [TestCase(MemEx.B, 0xFFFF_FF00u, 0x0u)]
    [TestCase(MemEx.W, 0x0u, 0x0u)]
    [TestCase(MemEx.W, 0x1u, 0x1u)]
    [TestCase(MemEx.W, 0x7Fu, 0x7Fu)]
    [TestCase(MemEx.W, 0x7FFFu, 0x7FFFu)]
    [TestCase(MemEx.W, 0x8000u, 0x8000u)]
    [TestCase(MemEx.W, 0xFFFF_8000u, 0x8000u)]
    [TestCase(MemEx.W, 0x1234_5678u, 0x5678u)]
    [TestCase(MemEx.W, 0xFFFF_FFFFu, 0xFFFFu)]
    public void MOVU_ar_Test(MemEx sz, uint src, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var ri = random.Next(1, 14);
        var rb = random.Next(ri + 1, 15);
        var rd = random.Next(rb + 1, 16);

        var baseAddr = random.NextUInt(0, 150);
        var indexValue = random.NextUInt(0, 10);
        var addr = baseAddr + (indexValue << (int)sz);
        cpu.Registers[rb] = baseAddr;
        cpu.Registers[ri] = indexValue;
        busManager.Write(addr, 4, src);
        RunOpcode(MOVU_indexed(sz, (Reg)ri, (Reg)rb, (Reg)rd));
        cpu.Registers[rd].Is(expected);
        // 変化していないことを確認する
        busManager.Read(addr, 4).Is(src);
        cpu.Registers[ri].Is(indexValue);
    }

    [Test]
    [TestCase(Addressing.PostInc, MemEx.B, 0x0u, 1, 0x0u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x7Fu, 1, 0x7Fu)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x80u, 1, 0x80u)]
    [TestCase(Addressing.PostInc, MemEx.B, 0xFFFF_FFFFu, 1, 0xFFu)]
    [TestCase(Addressing.PostInc, MemEx.B, 0x1234_5678u, 1, 0x78u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x0u, 2, 0x0u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x1234_5678u, 2, 0x5678u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x7FFFu, 2, 0x7FFFu)]
    [TestCase(Addressing.PostInc, MemEx.W, 0x8000u, 2, 0x8000u)]
    [TestCase(Addressing.PostInc, MemEx.W, 0xFFFF_FFFFu, 2, 0xFFFFu)]
    public void MOVU_pr_Test(Addressing ad, MemEx sz, uint src, int afterAddrOffset, uint expected) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var addr = random.Next(20, 260);

        busManager.Write((uint)(addr + (ad == Addressing.PreDec ? afterAddrOffset : 0)), 4, src);
        cpu.Registers[rs] = (uint)addr;
        cpu.Registers[rd] = 0;

        RunOpcode(MOVU(sz, ad, (Reg)rs, (Reg)rd));
        cpu.Registers[rs].Is((uint)(addr + afterAddrOffset));
        cpu.Registers[rd].Is(expected);
    }

    [Test]
    [TestCase(0, unchecked((int)0xFFFF_FFFF), 0)]
    [TestCase(15, 0, 0)]
    [TestCase(0, 1, 0)]
    [TestCase(1, 1, 1)]
    [TestCase(2, 250, 500)]
    [TestCase(15, 1, 15)]
    public void MUL_4ir_Test(byte a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = (uint)b;
        RunOpcode(MUL(new UInt4(a), (Reg)rd));
        cpu.Registers[rd].Is((uint)result);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, -120, 10, -1200)]
    [TestCase(LengthOfImmediate.SIMM8, 120, 10, 1200)]
    [TestCase(LengthOfImmediate.SIMM8, -120, -10, 1200)]
    [TestCase(LengthOfImmediate.SIMM16, 2000, 100, 200000)]
    [TestCase(LengthOfImmediate.SIMM16, -2000, 100, -200000)]
    [TestCase(LengthOfImmediate.SIMM24, 990000, 13, 990000 * 13)]
    [TestCase(LengthOfImmediate.SIMM24, -990000, 14, -990000 * 14)]
    [TestCase(LengthOfImmediate.IMM32, -8, 7, -56)]
    [TestCase(LengthOfImmediate.IMM32, 22, 1000, 22000)]
    public void MUL_ir_Test(LengthOfImmediate li, int a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = (uint)b;
        RunOpcode(MUL(new StdImmValue(li, unchecked((uint)a)), (Reg)rd));
        cpu.Registers[rd].Is((uint)result);
    }

    [Test]
    [TestCase(null, 120, 10, 1200)]
    [TestCase(null, 200, 10, 2000)]
    [TestCase(null, 200, -10, -2000)]
    [TestCase(MemEx.B, -120, 10, -1200)]
    [TestCase(MemEx.B, 120, 10, 1200)]
    [TestCase(MemEx.B, -120, -10, 1200)]
    [TestCase(MemEx.W, 2000, 100, 200000)]
    [TestCase(MemEx.W, -2000, 100, -200000)]
    [TestCase(MemEx.UW, 40000, 10, 400000)]
    [TestCase(MemEx.UW, 40000, -10, -400000)]
    [TestCase(MemEx.L, -8, 7, -56)]
    [TestCase(MemEx.L, 22, 1000, 22000)]
    public void MUL_mr__MUL_ub_rs_mr_Test(MemEx? sz, int a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, (uint)a);
        cpu.Registers[rd] = (uint)b;
        RunOpcode(MUL(dsp, (Reg)rd));
        cpu.Registers[rd].Is((uint)result);
    }

    [TestCase(-8, 7, -56)]
    [TestCase(-8, -7, 56)]
    [TestCase(22, 1000, 22000)]
    [TestCase(22, 1000, 22000)]
    public void MUL_rrr_Test(int a, int b, int result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs] = unchecked((uint)a);
        cpu.Registers[rs2] = unchecked((uint)b);
        RunOpcode(MUL((Reg)rs, (Reg)rs2, (Reg)rd));
        cpu.Registers[rd].Is(unchecked((uint)result));
    }

    [Test]
    [TestCase(9000, 8000, 72000000)]
    [TestCase(-9000, 8000, -72000000)]
    [TestCase(-9000, -9000, 81000000)]
    public void MULHI_Test(short a, short b, long result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        cpu.Registers[rs] = unchecked((uint)a << 16);
        cpu.Registers[rs2] = unchecked((uint)b << 16);
        RunOpcode(MULHI((Reg)rs, (Reg)rs2));
        cpu.Acc.Is(unchecked((ulong)result << 16));
    }

    [Test]
    [TestCase( 9000,  8000,  72000000)]
    [TestCase(-9000,  8000, -72000000)]
    [TestCase(-9000, -9000,  81000000)]
    public void MULLO_Test(short a, short b, long result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        cpu.Registers[rs] = unchecked((uint)a);
        cpu.Registers[rs2] = unchecked((uint)b);
        RunOpcode(MULLO((Reg)rs, (Reg)rs2));
        cpu.Acc.Is(unchecked((ulong)result << 16));
    }

    [Test]
    [TestCase(0xFFFFFFFF_00000000u, 0xFFFFFFFFu)]
    [TestCase(0x00000000_FFFFFFFFu, 0x00000000u)]
    [TestCase(0x12345678_FFFFFFFFu, 0x12345678u)]
    public void MVFACHI_Test(ulong acc, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Acc = acc;
        RunOpcode(MVFACHI((Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(0x0000_FFFFFFFF_0000u, 0xFFFFFFFFu)]
    [TestCase(0xFFFF_00000000_FFFFu, 0x00000000u)]
    [TestCase(0xFFFF_12345678_0000u, 0x12345678u)]
    public void MVFACMI_Test(ulong acc, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Acc = acc;
        RunOpcode(MVFACMI((Reg)rd));
        cpu.Registers[rd].Is(result);
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
            case ControlReg.USP  : cpu.ConvertedUSP   = input; break;
            case ControlReg.FPSW : cpu.FPSW  = input; break;
            case ControlReg.BPSW : cpu.BPSW  = input; break;
            case ControlReg.BPC  : cpu.BPC   = input; break;
            case ControlReg.ISP  : cpu.ConvertedISP   = input; break;
            case ControlReg.FINTV: cpu.FINTV = input; break;
            case ControlReg.INTB : cpu.INTB  = input; break;
            default: Assert.Fail(); break;
        }
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        RunOpcode(MVFC(a, (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(0x12345678u, 0x12345678_00000000u)]
    [TestCase(0xFFFFFFFFu, 0xFFFFFFFF_00000000u)]
    [TestCase(0u, 0u)]
    public void MVTACHI_Test(uint a, ulong result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 16);
        cpu.Registers[rs] = a;
        RunOpcode(MVTACHI((Reg)rs));
        cpu.Acc.Is(result);
    }

    [Test]
    [TestCase(0x12345678u, 0x00000000_12345678u)]
    [TestCase(0xFFFFFFFFu, 0x00000000_FFFFFFFFu)]
    [TestCase(0u, 0u)]
    public void MVTACLO_Test(uint a, ulong result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 16);
        cpu.Registers[rs] = a;
        RunOpcode(MVTACLO((Reg)rs));
        cpu.Acc.Is(result);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 1u, ControlReg.PSW  , 1u)]
    [TestCase(LengthOfImmediate.SIMM8, 2u, ControlReg.USP  , 2u)]
    [TestCase(LengthOfImmediate.SIMM8, 3u, ControlReg.FPSW , 3u)]
    [TestCase(LengthOfImmediate.SIMM8, 4u, ControlReg.BPSW , 4u)]
    [TestCase(LengthOfImmediate.SIMM8, 5u, ControlReg.BPC  , 5u)]
    [TestCase(LengthOfImmediate.SIMM8, 6u, ControlReg.ISP  , 6u)]
    [TestCase(LengthOfImmediate.SIMM8, 7u, ControlReg.FINTV, 7u)]
    [TestCase(LengthOfImmediate.SIMM8, 8u, ControlReg.INTB , 8u)]
    [TestCase(LengthOfImmediate.SIMM16, 1u, ControlReg.PSW  , 1u)]
    [TestCase(LengthOfImmediate.SIMM16, 2u, ControlReg.USP  , 2u)]
    [TestCase(LengthOfImmediate.SIMM16, 3u, ControlReg.FPSW , 3u)]
    [TestCase(LengthOfImmediate.SIMM16, 4u, ControlReg.BPSW , 4u)]
    [TestCase(LengthOfImmediate.SIMM16, 5u, ControlReg.BPC  , 5u)]
    [TestCase(LengthOfImmediate.SIMM16, 6u, ControlReg.ISP  , 6u)]
    [TestCase(LengthOfImmediate.SIMM16, 7u, ControlReg.FINTV, 7u)]
    [TestCase(LengthOfImmediate.SIMM16, 8u, ControlReg.INTB , 8u)]
    [TestCase(LengthOfImmediate.SIMM24, 1u, ControlReg.PSW  , 1u)]
    [TestCase(LengthOfImmediate.SIMM24, 2u, ControlReg.USP  , 2u)]
    [TestCase(LengthOfImmediate.SIMM24, 3u, ControlReg.FPSW , 3u)]
    [TestCase(LengthOfImmediate.SIMM24, 4u, ControlReg.BPSW , 4u)]
    [TestCase(LengthOfImmediate.SIMM24, 5u, ControlReg.BPC  , 5u)]
    [TestCase(LengthOfImmediate.SIMM24, 6u, ControlReg.ISP  , 6u)]
    [TestCase(LengthOfImmediate.SIMM24, 7u, ControlReg.FINTV, 7u)]
    [TestCase(LengthOfImmediate.SIMM24, 8u, ControlReg.INTB , 8u)]
    [TestCase(LengthOfImmediate.IMM32, 1u, ControlReg.PSW  , 1u)]
    [TestCase(LengthOfImmediate.IMM32, 2u, ControlReg.USP  , 2u)]
    [TestCase(LengthOfImmediate.IMM32, 3u, ControlReg.FPSW , 3u)]
    [TestCase(LengthOfImmediate.IMM32, 4u, ControlReg.BPSW , 4u)]
    [TestCase(LengthOfImmediate.IMM32, 5u, ControlReg.BPC  , 5u)]
    [TestCase(LengthOfImmediate.IMM32, 6u, ControlReg.ISP  , 6u)]
    [TestCase(LengthOfImmediate.IMM32, 7u, ControlReg.FINTV, 7u)]
    [TestCase(LengthOfImmediate.IMM32, 8u, ControlReg.INTB , 8u)]
    public void MVTC_i_Test(LengthOfImmediate li, uint a, ControlReg b, uint result) {
        RunOpcode(MVTC(new StdImmValue(li, a), b));
        var regValue = b switch
        {
            ControlReg.PSW => cpu.PSW,
            ControlReg.USP => cpu.ConvertedUSP,
            ControlReg.FPSW => cpu.FPSW,
            ControlReg.BPSW => cpu.BPSW,
            ControlReg.BPC => cpu.BPC,
            ControlReg.ISP => cpu.ConvertedISP,
            ControlReg.FINTV => cpu.FINTV,
            ControlReg.INTB => cpu.INTB,
            _ => throw new ArgumentException(b.ToString())
        };
        regValue.Is(result);
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
    public void MVTC_r_Test(uint a, ControlReg b, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 16);
        cpu.Registers[rs] = a;
        RunOpcode(MVTC((Reg)rs, b));
        var regValue = b switch
        {
            ControlReg.PSW => cpu.PSW,
            ControlReg.USP => cpu.ConvertedUSP,
            ControlReg.FPSW => cpu.FPSW,
            ControlReg.BPSW => cpu.BPSW,
            ControlReg.BPC => cpu.BPC,
            ControlReg.ISP => cpu.ConvertedISP,
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
    [TestCase(5u, unchecked((uint)-5), false, false, true,false)]
    [TestCase(unchecked((uint)-5), 5u, false, false, false, false)]
    [TestCase(90000000u, unchecked((uint)-90000000), false, false, true, false)]
    [TestCase(0u, 0u, true, true, false, false)]
    [TestCase(0x80000000u, unchecked((uint)-0x80000000u), false, false, true, true)]
    public void NEG_rd_Test(uint a, uint result,
                         bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = a;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(NEG((Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(5u, unchecked((uint)-5), false, false, true,false)]
    [TestCase(unchecked((uint)-5), 5u, false, false, false, false)]
    [TestCase(90000000u, unchecked((uint)-90000000), false, false, true, false)]
    [TestCase(0u, 0u, true, true, false, false)]
    [TestCase(0x80000000u, unchecked((uint)-0x80000000u), false, false, true, true)]
    public void NEG_rs_rd_Test(uint a, uint result,
                         bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs] = a;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(NEG((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
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
    public void NOT_rd_Test(uint a, uint result,
                         bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = a;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(NOT((Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(          0u, 0xFFFF_FFFFu, false,  true)]
    [TestCase(0xFFFF_FFFFu,           0u,  true, false)]
    [TestCase(0x7000_0000u, 0x8FFF_FFFFu, false,  true)]
    public void NOT_rs_rd_Test(uint a, uint result,
                         bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs] = a;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(NOT((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(0x0, 0xFFFFu, 0xFFFFu, false, false)]
    [TestCase(0x0, 0x0u, 0x0u, true, false)]
    [TestCase(0x1, 0x0u, 0x1u, false, false)]
    [TestCase(0xF, 0x0u, 0xFu, false, false)]
    [TestCase(0xA, 0x1234_5670u, 0x1234_567Au, false, false)]
    [TestCase(0x0, 0x8000_0000u, 0x8000_0000u, false, true)]
    public void OR_4ir_Test(byte a, uint b, uint result,
                        bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(OR(new UInt4(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(LengthOfImmediate.SIMM8, 0x0u, 0xFFu, 0xFFu, false, false)]
    [TestCase(LengthOfImmediate.SIMM8, 0xFFu, 0x0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(LengthOfImmediate.SIMM8, 0x7Fu, 0x0u, 0x7Fu, false, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x0u, 0xFFFFu, 0xFFFFu, false, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0xFFFFu, 0x0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7FFFu, 0x0u, 0x7FFFu, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x0u, 0xFF_FFFFu, 0xFF_FFFFu, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0xFF_FFFFu, 0x0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(LengthOfImmediate.SIMM24, 0x7F_FFFFu, 0x0u, 0x7F_FFFFu, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0xFFFF_FFFFu, 0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(LengthOfImmediate.IMM32, 0x7F00_00FFu, 0x00FF_70FFu, 0x7FFF_70FFu, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0u, 0u, 0u, true, false)]
    public void OR_ir_Test(LengthOfImmediate li, uint a, uint b, uint result,
                           bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(OR(new StdImmValue(li, a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(null, 0xFFu, 0x8000_0000, 0x8000_00FFu, false, true)]
    [TestCase(null, 0x0u, 0x71u, 0x71u, false, false)]
    [TestCase(null, 0x70u, 0x0u, 0x70u, false, false)]
    [TestCase(null, 0x80u, 0x0u, 0x80u, false, false)]
    [TestCase(MemEx.B, 0x0u, 0xFFu, 0xFFu, false, false)]
    [TestCase(MemEx.B, 0xFFu, 0x0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(MemEx.B, 0x7Fu, 0x0u, 0x7Fu, false, false)]
    [TestCase(MemEx.W, 0x0u, 0xFFFFu, 0xFFFFu, false, false)]
    [TestCase(MemEx.W, 0xFFFFu, 0x0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(MemEx.W, 0x7FFFu, 0x0u, 0x7FFFu, false, false)]
    [TestCase(MemEx.UW, 0xFFFFu, 0x0u, 0xFFFFu, false, false)]
    [TestCase(MemEx.UW, 0x1234u, 0x0u, 0x1234u, false, false)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(MemEx.L, 0x7F00_00FFu, 0x00FF_70FFu, 0x7FFF_70FFu, false, false)]
    [TestCase(MemEx.L, 0u, 0u, 0u, true, false)]
    public void OR_mr__OR_ub_rs_mr_Test(MemEx? sz, uint a, uint b, uint result,
                        bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(OR(dsp, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [TestCase(0xFFFF_FFFFu, 0u, 0xFFFF_FFFFu, false, true)]
    [TestCase(0x7F00_00FFu, 0x00FF_70FFu, 0x7FFF_70FFu, false, false)]
    [TestCase(0u, 0u, 0u, true, false)]
    public void OR_rrr_Test(uint a, uint b, uint result,
                            bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rs2] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(OR((Reg)rs, (Reg)rs2, (Reg)rd));
        cpu.Registers[rd].Is(result);
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
            ControlReg.USP => cpu.ConvertedUSP,
            ControlReg.FPSW => cpu.FPSW,
            ControlReg.BPSW => cpu.BPSW,
            ControlReg.BPC => cpu.BPC,
            ControlReg.ISP => cpu.ConvertedISP,
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
    public void PUSH_m_Test(
        [Random(0, 2, 2)]byte size,
        [Random(1, 15, 5)]byte reg,
        [Random(5)] uint value) {

        var random = TestContext.CurrentContext.Random;
        value = size switch {
            0 => value & 0xFF,
            1 => value & 0XFFFF,
            _ => value
        };
        var dsp = GetRandomStdRegAddressing((Reg)reg, (MemEx)size, LengthOfDisplacement.RefReg, LengthOfDisplacement.DSP16Reg);
        StoreRandomDest(dsp, 0, 300, value);
        var prevSP = cpu.SP;
        RunOpcode(PUSH(dsp));
        cpu.SP.Is(prevSP - 4);
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
                cpu.ConvertedUSP = value;
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
                cpu.ConvertedISP = value;
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
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        RunOpcode(REVL((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(0x1234_5678u, 0x3412_7856u)]
    [TestCase(0x9876_5432u, 0x7698_3254u)]
    public void REVW_Test(uint a, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        RunOpcode(REVW((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    public static object[][] RMPA_TestData() {
        return new object[][] {
            new object[] { MemEx.B, 0u, 0u, 0u, 1u, new[] { 1u }, new[] { 0xFFu }, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, true, false },
            new object[] { MemEx.B, 0u, 0u, 0u, 1u, new[] { 0xFFu }, new[] { 1u }, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, true, false },
            new object[] { MemEx.B, 0u, 0u, 0u, 1u, new[] { 0xFFu }, new[] { 0xFF }, 0u, 0u, 0x1u, false, false },

            new object[] { MemEx.W, 0u, 0u, 0u, 1u, new[] { 1u }, new[] { 0xFFFFu }, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, true, false },
            new object[] { MemEx.W, 0u, 0u, 0u, 1u, new[] { 0xFFFFu }, new[] { 1u }, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, true, false },
            new object[] { MemEx.W, 0u, 0u, 0u, 1u, new[] { 0xFFFFu }, new[] { 0xFFFF }, 0u, 0u, 0x1u, false, false },

            new object[] { MemEx.L, 0u, 0u, 0u, 1u, new[] { 0xFFFF_FFFFu }, new[] { 1u }, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, true, false },
            new object[] { MemEx.L, 0u, 0u, 0u, 1u, new[] { 1u }, new[] { 0xFFFF_FFFFu }, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, true, false },

            new object[] { MemEx.L, 0u, 0x7FFF_FFFFu, 0xFFFF_FFFDu, 1u, new[] { 1u }, new[] { 1u }, 0u, 0x7FFF_FFFFu, 0xFFFF_FFFEu, false, false },
            new object[] { MemEx.L, 0u, 0x7FFF_FFFFu, 0xFFFF_FFFEu, 1u, new[] { 1u }, new[] { 1u }, 0u, 0x7FFF_FFFFu, 0xFFFF_FFFFu, false, false },
            new object[] { MemEx.L, 0u, 0x7FFF_FFFFu, 0xFFFF_FFFFu, 1u, new[] { 1u }, new[] { 1u }, 0u, 0x8000_0000u, 0x0000_0000u, false, true },

            new object[] { MemEx.L, 0u, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 1u, new[] { 0x7FFF_FFFFu }, new[] { 0x7FFF_FFFFu }, 1u, 0x3FFF_FFFFu, 0u, false, true },
            new object[] { MemEx.L, 0x3FFFu, 0xFFFF_FFFFu, 0xFFFF_FFFFu, 1u, new[] { 0x7FFF_FFFFu }, new[] { 0x7FFF_FFFFu }, 0x4000u, 0x3FFF_FFFFu, 0u, false, true },
            new object[] { MemEx.L, 0x4000u, 0u, 0xFu, 1u, new[] { 0xFFFF_FFFFu }, new[] { 0xFu }, 0x4000u, 0u, 0u, false, true },
            new object[] { MemEx.L, 0x8000u, 0u, 0x0u, 1u, new[] { 0x7FFF_FFFFu }, new[] { 0u }, 0xFFFF_8000u, 0u, 0u, true, true },

            new object[] { MemEx.L, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0001u, 1u, new[] { 0xFFFF_FFFFu }, new[] { 1u }, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0000u, true, false },
            new object[] { MemEx.L, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0002u, 1u, new[] { 0xFFFF_FFFFu }, new[] { 2u }, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0000u, true, false },
            new object[] { MemEx.L, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0001u, 1u, new[] { 1u }, new[] { 0xFFFF_FFFFu }, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0000u, true, false },
            new object[] { MemEx.L, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0002u, 1u, new[] { 2u }, new[] { 0xFFFF_FFFFu }, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0000u, true, false },

            new object[] { MemEx.L, 0xFFFF_FFFFu, 0x8000_0000u, 0x0000_0000u, 1u, new[] { 0xFFFF_FFFFu }, new[] { 1u }, 0xFFFF_FFFFu, 0x7FFF_FFFFu, 0xFFFF_FFFFu, true, true },
        };
    }

    [Test]
    [TestCaseSource(nameof(RMPA_TestData))]
    public void RMPA_Test(
        MemEx sz,
        uint initialR6, uint initialR5, uint initialR4, uint count,
        uint[] a, uint[] b,
        uint expR6, uint expR5, uint expR4, bool expS, bool expO) {

        void SetValues(MemEx size, uint beginAddr, uint[] values) {
            var byteSize = 1 << (int)size;
            for (int i = 0; i < values.Length; i++) {
                busManager.Write(beginAddr + (uint)(i * byteSize), byteSize, values[i]);
            }
        }

        var random = TestContext.CurrentContext.Random;
        cpu.Registers[6] = initialR6;
        cpu.Registers[5] = initialR5;
        cpu.Registers[4] = initialR4;
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        SetValues(sz, ramBeginAddress, a);
        SetValues(sz, ramBeginAddress + 64, b);

        cpu.Registers[1] = ramBeginAddress;
        cpu.Registers[2] = ramBeginAddress + 64;
        cpu.Registers[3] = count;

        var prevPC = cpu.PC;
        for (uint i = 1; i < count; i++) {
            RunOpcode(RMPA(sz));
            // PCが進まないことを確認する
            cpu.PC.Is(prevPC);
        }
        // 終了後プログラムカウンターが進んでいることを確認する
        RunOpcode(RMPA(sz));
        cpu.PC.Is(prevPC + 2);

        cpu.Registers[6].Is(expR6);
        cpu.Registers[5].Is(expR5);
        cpu.Registers[4].Is(expR4);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(false, 0x8000_0001u, 0x0000_0002u,  true, false, false)]
    [TestCase( true, 0x8000_0001u, 0x0000_0003u,  true, false, false)]
    [TestCase( true, 0xF000_0000u, 0xE000_0001u,  true, false,  true)]
    [TestCase( true, 0x7000_0000u, 0xE000_0001u, false, false,  true)]
    [TestCase(false, 0x8000_0000u, 0x0000_0000u,  true,  true, false)]
    public void RPLC_Test(bool psw_c, uint a, uint result, bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.PSW_c = psw_c;
        cpu.Registers[rd] = a;
        RunOpcode(ROLC((Reg)rd));
        cpu.Registers[rd].Is(result);
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
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.PSW_c = psw_c;
        cpu.Registers[rd] = a;
        RunOpcode(RORC((Reg)rd));
        cpu.Registers[rd].Is(result);
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
    public void ROTL_ir_Test(byte a, uint b, uint result,
                          bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        RunOpcode(ROTL(new UInt5(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
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
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        RunOpcode(ROTL((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( 8, 0x0100_00FFu, 0xFF01_0000u,  true, false,  true)]
    [TestCase(16, 0x0100_00FFu, 0x00FF_0100u, false, false, false)]
    [TestCase( 1, 0x0000_000Fu, 0x8000_0007u,  true, false,  true)]
    [TestCase( 0, 0x1234_5678u, 0x1234_5678u, false, false, false)]
    [TestCase(31, 0x0000_0000u, 0x0000_0000u, false,  true, false)]
    public void ROTR_ir_Test(byte a, uint b, uint result,
                             bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        RunOpcode(ROTR(new UInt5(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( 8u, 0x0100_00FFu, 0xFF01_0000u,  true, false,  true)]
    [TestCase(16u, 0x0100_00FFu, 0x00FF_0100u, false, false, false)]
    [TestCase( 1u, 0x0000_000Fu, 0x8000_0007u,  true, false,  true)]
    [TestCase( 0u, 0x1234_5678u, 0x1234_5678u, false, false, false)]
    [TestCase(0xFFFF_FFE0u, 0x1234_5678u, 0x1234_5678u, false, false, false)]
    [TestCase(31u, 0x0000_0000u, 0x0000_0000u, false,  true, false)]
    public void ROTR_rr_Test(uint a, uint b, uint result,
                          bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        RunOpcode(ROTR((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    // [Test]
    // [TestCase(RXFloatRoundingMode.)]
    public void ROUND_Test(RXFloatRoundingMode rm, float a, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, BitConverter.SingleToUInt32Bits(a));
        cpu.FPSW_rm = rm;
        RunOpcode(ROUND(dsp, (Reg)rd));
        cpu.Registers[rd].Is(result);
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
    public void RTSD_i_Test([Random(5)]byte src, [Random(5)] uint pc) {
        var value = src * 4u;
        cpu.SP -= 4;
        busManager.Write(cpu.SP, 4, pc);
        cpu.SP -= value;
        RunOpcode(RTSD((ushort)value));
        cpu.PC.Is(pc);
    }

    [Test]
    public void RTSD_irr_Test(
        [Random(5)] uint pc,
        [Random(60, 255, 5)]byte src,
        [Random(1, 15, 5)] byte regBegin,
        [Random(1, 14, 5)] byte length
        ) {
        var value = src * 4u;
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

        cpu.SP -= value;
        RunOpcode(RTSD((ushort)value, (Reg)regBegin, (Reg)regEnd));
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
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.PSW_o = psw_o;
        cpu.PSW_s = psw_s;
        cpu.Registers[rd] = a;
        RunOpcode(SAT((Reg)rd));
        cpu.Registers[rd].Is(result);
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
    public void SBB_mr__SBB_rr_Test(
        bool psw_c, uint a, uint b, uint result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, MemEx.L, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.PSW_c = psw_c;
        cpu.Registers[rd] = b;
        RunOpcode(SBB(dsp, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase( true, false, false, false, Cnd.GEU, 1)]
    [TestCase(false,  true,  true,  true, Cnd.GEU, 0)]
    [TestCase(false,  true, false, false, Cnd.EQ , 1)]
    [TestCase( true, false,  true,  true, Cnd.EQ , 0)]
    [TestCase( true, false, false, false, Cnd.GTU, 1)]
    [TestCase( true, false,  true,  true, Cnd.GTU, 1)]
    [TestCase( true,  true,  true,  true, Cnd.GTU, 0)]
    [TestCase(false, false,  true,  true, Cnd.GTU, 0)]
    [TestCase(false,  true,  true,  true, Cnd.GTU, 0)]
    [TestCase(false, false,  true, false, Cnd.PZ , 0)]
    [TestCase( true,  true, false,  true, Cnd.PZ , 1)]
    [TestCase(false, false,  true,  true, Cnd.GE , 1)]
    [TestCase(false, false, false, false, Cnd.GE , 1)]
    [TestCase(false, false,  true, false, Cnd.GE , 0)]
    [TestCase(false, false, false,  true, Cnd.GE , 0)]
    [TestCase( true,  true, false,  true, Cnd.GE , 0)]
    [TestCase(false, false,  true,  true, Cnd.GT , 1)]
    [TestCase(false, false, false, false, Cnd.GT , 1)]
    [TestCase(false, false,  true, false, Cnd.GT , 0)]
    [TestCase(false, false, false,  true, Cnd.GT , 0)]
    [TestCase(false,  true,  true, false, Cnd.GT , 0)]
    [TestCase(false,  true, false,  true, Cnd.GT , 0)]
    [TestCase( true,  true, false,  true, Cnd.GT , 0)]
    [TestCase(false, false, false,  true, Cnd.O  , 1)]
    [TestCase( true,  true,  true, false, Cnd.O  , 0)]
    [TestCase(false,  true,  true,  true, Cnd.LTU, 1)]
    [TestCase( true, false, false, false, Cnd.LTU, 0)]
    [TestCase( true, false,  true,  true, Cnd.NE , 1)]
    [TestCase(false,  true, false, false, Cnd.NE , 0)]
    [TestCase(false,  true,  true,  true, Cnd.LEU, 1)]
    [TestCase(false, false,  true,  true, Cnd.LEU, 1)]
    [TestCase(false,  true, false, false, Cnd.LEU, 1)]
    [TestCase( true, false, false, false, Cnd.LEU, 0)]
    [TestCase( true, false, false, false, Cnd.LEU, 0)]
    [TestCase( true, false, true,   true, Cnd.LEU, 0)]
    [TestCase(false,  true,  true,  true, Cnd.LEU, 1)]
    [TestCase(false, false, false, false, Cnd.LEU, 1)]
    [TestCase(false, false,  true, false, Cnd.LE , 1)]
    [TestCase(false, false, false,  true, Cnd.LE , 1)]
    [TestCase(false,  true,  true, false, Cnd.LE , 1)]
    [TestCase(false,  true, false,  true, Cnd.LE , 1)]
    [TestCase(false,  true,  true,  true, Cnd.LE , 1)]
    [TestCase( true,  true, false, false, Cnd.LE , 1)]
    [TestCase(false, false,  true,  true, Cnd.LE , 0)]
    [TestCase(false, false, false, false, Cnd.LE , 0)]
    [TestCase( true, false,  true,  true, Cnd.LE , 0)]
    [TestCase( true, false, false, false, Cnd.LE , 0)]
    [TestCase(false, false,  true, false, Cnd.LT , 1)]
    [TestCase(false, false, false,  true, Cnd.LT , 1)]
    [TestCase( true,  true,  true, false, Cnd.LT , 1)]
    [TestCase(false, false,  true,  true, Cnd.LT , 0)]
    [TestCase(false, false, false, false, Cnd.LT , 0)]
    [TestCase( true,  true, false, false, Cnd.LT , 0)]
    [TestCase(false, false, false,  true, Cnd.NO , 0)]
    [TestCase( true,  true,  true,  true, Cnd.NO , 0)]
    [TestCase(false, false, false, false, Cnd.NO , 1)]
    [TestCase( true,  true,  true, false, Cnd.NO , 1)]
    public void SCCnd_mem_Test(
        bool psw_c, bool psw_z, bool psw_s, bool psw_o,
        Cnd condition, byte result) {

        var random = TestContext.CurrentContext.Random;
        var initial = random.NextUInt();
        MemEx sz = (MemEx)random.Next(0, 3);
        var rd = random.Next(1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rd, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        var expected = sz switch {
            MemEx.B => (initial & 0xFFFF_FF00) | result,
            MemEx.W => (initial & 0xFFFF_0000) | result,
            _ => result
        };
        
        cpu.PSW_c = psw_c;
        cpu.PSW_z = psw_z;
        cpu.PSW_s = psw_s;
        cpu.PSW_o = psw_o;
        StoreRandomDest(dsp, 0, 300, initial);
        RunOpcode(SCC(condition, dsp));
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expected);
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
    public void SHAR_5irr_Test(byte a, uint b, uint result,
                             bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SHAR(new UInt5(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
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
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SHAR((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(false);
    }

    [Test]
    [TestCase(8, 0x8000_0000u, 0xFF80_0000u, false, false,  true)]
    [TestCase(8, 0x8000_0080u, 0xFF80_0000u,  true, false,  true)]
    [TestCase(1, 0x4000_0000u, 0x2000_0000u, false, false, false)]
    [TestCase(0, 0x0000_0001u, 0x0000_0001u, false, false, false)]
    [TestCase(1, 0x0000_0001u, 0x0000_0000u,  true,  true, false)]
    [TestCase( 0x0, 0x7FFF_FFFFu, 0x7FFF_FFFFu, false, false,  false)]
    [TestCase( 0x0, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, false,   true)]
    public void SHAR_irr_Test(byte a, uint b, uint result,
                          bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SHAR(new UInt5(a), (Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
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
    public void SHLL_5irr_Test(byte a, uint b, uint result,
                               bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SHLL(new UInt5(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
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
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SHLL((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
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
    public void SHLL_irr_Test(byte a, uint b, uint result,
                              bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SHLL(new UInt5(a), (Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(0, 0x8FFF_FFFFu, 0x8FFF_FFFFu,  false,  false, true)]
    [TestCase(16, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(16, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(31, 0x8FFF_FFFFu, 0x0000_0001u,  false,  false, false)]
    public void SHLR_5irr_Test(byte a, uint b, uint result,
                               bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(SHLR(new UInt5(a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(0u, 0x8FFF_FFFFu, 0x8FFF_FFFFu, false, false, true)]
    [TestCase(  16u, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(  16u, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(  31u, 0x8FFF_FFFFu, 0x0000_0001u,  false,  false, false)]
    [TestCase(0xE0u, 0xFFFF_FFFFu, 0xFFFF_FFFFu,  false,  false,  true)]
    public void SHLR_rr_Test(uint a, uint b, uint result,
                             bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = a;
        cpu.Registers[rd] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(SHLR((Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(16, 0xFFFF_FFFFu, 0x0000_FFFFu,  true, false, false)]
    [TestCase( 0, 0xFFFF_FFFFu, 0xFFFF_FFFFu, false, false,  true)]
    [TestCase(16, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(16, 0x0000_FFFFu, 0x0000_0000u,   true,   true, false)]
    [TestCase(31, 0x8FFF_FFFFu, 0x0000_0001u,  false,  false, false)]
    [TestCase(24, 0x1234_5678u, 0x0000_0012u, false, false, false)]
    public void SHLR_irr_Test(byte a, uint b, uint result,
                              bool expC, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        cpu.Registers[rs] = b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(SHLR(new UInt5(a), (Reg)rs, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase( true, 6u, unchecked((int)0xFF1122EE), 6u)]
    [TestCase(false, 6u, unchecked((int)0xFF1122EE), 0xFF1122EEu)]
    public void STNZ_Test(bool psw_z, uint initial, int a, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = initial;
        cpu.PSW_z = psw_z;
        RunOpcode(STNZ(a, (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(false, 6u, unchecked((int)0xFF1122EEu), 6u)]
    [TestCase( true, 6u, unchecked((int)0xFF1122EEu), 0xFF1122EEu)]
    public void STZ_Test(bool psw_z, uint initial, int a, uint result) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = initial;
        cpu.PSW_z = psw_z;
        RunOpcode(STZ(a, (Reg)rd));
        cpu.Registers[rd].Is(result);
    }

    [Test]
    [TestCase(0, 60, 60, true, false, false, false)]
    [TestCase(1, 60, 59, true, false, false, false)]
    [TestCase(15, 60, 45, true, false, false, false)]
    public void SUB_ir_Test(
        byte a, int b, int result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = (uint)b;
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SUB(new UInt4(a), (Reg)rd));
        cpu.Registers[rd].Is((uint)result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(null, 200, 500, 300, true, false, false, false)]
    [TestCase(null, 100, 500, 400, true, false, false, false)]
    [TestCase(MemEx.B, 50, 60, 10, true, false, false, false)]
    [TestCase(MemEx.B, -10, 60, 70, false, false, false, false)]
    [TestCase(MemEx.W, 300, 400, 100, true, false, false, false)]
    [TestCase(MemEx.W, -300, 10, 310, false, false, false, false)]
    [TestCase(MemEx.UW, 40000, 42000, 2000, true, false, false, false)]
    [TestCase(MemEx.UW, 300, -20, -320, true, false, true, false)]
    [TestCase(MemEx.L, 100, 1_000_905, 1_000_805, true, false, false, false)]
    [TestCase(MemEx.L, 2000, 2000, 0,  true, true, false, false)]
    [TestCase(MemEx.L, -1, 0x7FFF_FFFF, unchecked((int)0x8000_0000), false, false, true, true)]
    [TestCase(MemEx.L, 100, -1, -101, true, false,  true, false)]
    [TestCase(MemEx.L, -1, ~0, 0, true, true, false, false)]
    public void SUB_mr__SUB_ub_rs_mr_Test(
        MemEx? sz, int a, int b, int result,
        bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, unchecked((uint)a));
        cpu.Registers[rd] = unchecked((uint)b);
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SUB(dsp, (Reg)rd));
        cpu.Registers[rd].Is((uint)result);
        cpu.PSW_c.Is(expC);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
        cpu.PSW_o.Is(expO);
    }

    [Test]
    [TestCase(100, 1_000_905, 1_000_805, true, false, false, false)]
    [TestCase(2000, 2000, 0,  true, true, false, false)]
    [TestCase(-1, 0x7FFF_FFFF, unchecked((int)0x8000_0000), false, false, true, true)]
    [TestCase(100, -1, -101, true, false,  true, false)]
    [TestCase(-1, ~0, 0, true, true, false, false)]
    public void SUB_rrr_Test(int a, int b, int result,
                             bool expC, bool expZ, bool expS, bool expO) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rs2 = random.Next(rs + 1, 16);
        var rd = random.Next(1, 16);
        cpu.Registers[rs] = unchecked((uint)a);
        cpu.Registers[rs2] = unchecked((uint)b);
        cpu.PSW_c = random.NextBool();
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        cpu.PSW_o = random.NextBool();
        RunOpcode(SUB((Reg)rs, (Reg)rs2, (Reg)rd));
        cpu.Registers[rd].Is((uint)result);
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
    [TestCase(LengthOfImmediate.SIMM8, 1u, 1u, false, false)]
    [TestCase(LengthOfImmediate.SIMM8, 0x80u, 0x8000_0000u, false, true)]
    [TestCase(LengthOfImmediate.SIMM8, 0x70u, 0x8000_0000u, true, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x8000u, 0x8000_0000u, false, true)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7000u, 0x8000_0000u, true, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x80_0000u, 0x8000_0000u, false, true)]
    [TestCase(LengthOfImmediate.SIMM24, 0x70_0000u, 0x8000_0000u, true, false)]
    [TestCase(LengthOfImmediate.IMM32,           0u, 0xFFFF_FFFFu,  true, false)]
    [TestCase(LengthOfImmediate.IMM32,           4u,         100u, false, false)]
    [TestCase(LengthOfImmediate.IMM32, 0x8000_0000u, 0x8000_0000u, false,  true)]
    public void TST_ir_Test(LengthOfImmediate li, uint a, uint b, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(TST(new StdImmValue(li, a), (Reg)rd));
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(null, 1u, 1u, false, false)]
    [TestCase(null, 0x80u, 0x8000_0000u, true, false)]
    [TestCase(null, 0x70u, 0x8000_0000u, true, false)]
    [TestCase(MemEx.B, 1u, 1u, false, false)]
    [TestCase(MemEx.B, 0x80u, 0x8000_0000u, false, true)]
    [TestCase(MemEx.B, 0x70u, 0x8000_0000u, true, false)]
    [TestCase(MemEx.W, 0x8000u, 0x8000_0000u, false, true)]
    [TestCase(MemEx.W, 0x7000u, 0x8000_0000u, true, false)]
    [TestCase(MemEx.UW, 0x8000u, 0x8000_0000u, true, false)]
    [TestCase(MemEx.UW, 0x7000u, 0x8000_0000u, true, false)]
    [TestCase(MemEx.L,           0u, 0xFFFF_FFFFu,  true, false)]
    [TestCase(MemEx.L,           4u,         100u, false, false)]
    [TestCase(MemEx.L, 0x8000_0000u, 0x8000_0000u, false,  true)]
    public void TST_mr__TST_ub_rs_mr_Test(MemEx? sz, uint a, uint b, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(TST(dsp, (Reg)rd));
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    public void Wait_Test() {}

    [Test]
    [TestCase(null, 0xFFFF_FFFFu, 0x1234_5600u, 0xFFu, 0xFFFF_FF00u)]
    [TestCase(null, 0x70u, 0x1234_5678u, 0x70u, 0x78u)]
    [TestCase(null, 0x80u, 0x1234_5678u, 0x80u, 0x78u)]
    [TestCase(MemEx.B, 0x7Fu, 0x1234_5678u, 0x7Fu, 0x78u)]
    [TestCase(MemEx.B, 0x7Fu, 0xFFu, 0x7Fu, 0xFFu)]
    [TestCase(MemEx.B, 0x70u, 0x0u, 0x70u, 0x0u)]
    [TestCase(MemEx.B, 0x80u, 0xFFu, 0xFFFF_FF80u, 0xFFu)]
    [TestCase(MemEx.W, 0x8012u, 0x3456_789Au, 0xFFFF_8012u, 0x789Au)]
    [TestCase(MemEx.W, 0x7012u, 0x3456_789Au, 0x7012u, 0x789Au)]
    [TestCase(MemEx.UW, 0xFFEEu, 0x1234_5678u, 0xFFEEu, 0x5678u)]
    [TestCase(MemEx.UW, 0xCCDDEEFFu, 0u, 0xEEFFu, 0xCCDD_0000u)]
    [TestCase(MemEx.L, 0xCCDDEEFFu, 0x11223344u, 0xCCDDEEFFu, 0x11223344u)]
    [TestCase(MemEx.L, 0xFFEEDDCCu, 0x87654321u, 0xFFEEDDCCu, 0x87654321u)]
    public void XCHG_mr__XCHG_ub_rs_mr_Test(MemEx? sz, uint a, uint b, uint exppectedReg, uint expectedMem) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        RunOpcode(XCHG(dsp, (Reg)rd));
        cpu.Registers[rd].Is(exppectedReg);
        LoadData(cpu.Registers, busManager, dsp.LD, (int)MemEx.L, dsp.Displacement, dsp.TargetReg)
            .Is(expectedMem);
    }

    [TestCase(LengthOfImmediate.SIMM8, 0x80u, 0x80u, 0xFFFF_FF00u, false, true)]
    [TestCase(LengthOfImmediate.SIMM8, 0x7Fu, 0x80u, 0xFFu, false, false)]
    [TestCase(LengthOfImmediate.SIMM8, 0x7Fu, 0x7Fu, 0u, true, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x8000u, 0x8000u, 0xFFFF_0000u, false, true)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7FFFu, 0x8000u, 0xFFFFu, false, false)]
    [TestCase(LengthOfImmediate.SIMM16, 0x7FFFu, 0x7FFFu, 0u, true, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x80_0000u, 0x80_0000u, 0xFF00_0000u, false, true)]
    [TestCase(LengthOfImmediate.SIMM24, 0x7F_FFFFu, 0x80_0000u, 0xFF_FFFFu, false, false)]
    [TestCase(LengthOfImmediate.SIMM24, 0x7F_FFFFu, 0x7F_FFFFu, 0u, true, false)]
    [TestCase(LengthOfImmediate.IMM32, 0b1010_0000u, 0b0111_0011u, 0b1101_0011u, false,  false)]
    [TestCase(LengthOfImmediate.IMM32, 0b1010_0000u << 24, 0b0111_0011u << 24, 0b1101_0011u << 24, false,  true)]
    [TestCase(LengthOfImmediate.IMM32, 0xFFFF_FFFFu, 0xFFFF_FFFFu,           0u,  true, false)]
    [TestCase(LengthOfImmediate.IMM32,           0u,           0u,           0u,  true, false)]
    public void XOR_ir_Test(LengthOfImmediate li, uint a, uint b, uint result, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rd = random.Next(1, 16);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(XOR(new StdImmValue(li, a), (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }

    [Test]
    [TestCase(null, 0xFFFF_FFFFu, 0xFFFF_FF00u, 0xFFFF_FFFFu, false, true)]
    [TestCase(null, 0x80u, 0x80u, 0x0u, true, false)]
    [TestCase(null, 0x7Fu, 0x80u, 0xFFu, false, false)]
    [TestCase(MemEx.B, 0x80u, 0x80u, 0xFFFF_FF00u, false, true)]
    [TestCase(MemEx.B, 0x7Fu, 0x80u, 0xFFu, false, false)]
    [TestCase(MemEx.B, 0x7Fu, 0x7Fu, 0u, true, false)]
    [TestCase(MemEx.W, 0x8000u, 0x8000u, 0xFFFF_0000u, false, true)]
    [TestCase(MemEx.W, 0x7FFFu, 0x8000u, 0xFFFFu, false, false)]
    [TestCase(MemEx.W, 0x7FFFu, 0x7FFFu, 0u, true, false)]
    [TestCase(MemEx.UW, 0x8000u, 0x8000u, 0u, true, false)]
    [TestCase(MemEx.UW, 0x7FFFu, 0x8000u, 0xFFFFu, false, false)]
    [TestCase(MemEx.UW, 0x7FFFu, 0x7FFFu, 0u, true, false)]
    [TestCase(MemEx.L, 0b1010_0000u, 0b0111_0011u, 0b1101_0011u, false,  false)]
    [TestCase(MemEx.L, 0b1010_0000u << 24, 0b0111_0011u << 24, 0b1101_0011u << 24, false,  true)]
    [TestCase(MemEx.L, 0xFFFF_FFFFu, 0xFFFF_FFFFu,           0u,  true, false)]
    [TestCase(MemEx.L,           0u,           0u,           0u,  true, false)]
    public void XOR_mr__XOR_ub_rs_mr_Test(MemEx? sz, uint a, uint b, uint result, bool expZ, bool expS) {
        var random = TestContext.CurrentContext.Random;
        var rs = random.Next(1, 15);
        var rd = random.Next(rs + 1, 16);
        var dsp = GetRandomStdRegAddressing((Reg)rs, sz, LengthOfDisplacement.RefReg, LengthOfDisplacement.Reg);
        StoreRandomDest(dsp, 0, 300, a);
        cpu.Registers[rd] = b;
        cpu.PSW_z = random.NextBool();
        cpu.PSW_s = random.NextBool();
        RunOpcode(XOR(dsp, (Reg)rd));
        cpu.Registers[rd].Is(result);
        cpu.PSW_z.Is(expZ);
        cpu.PSW_s.Is(expS);
    }
}
