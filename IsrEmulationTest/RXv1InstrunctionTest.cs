using NUnit.Framework;
using RX;
using TestTools;
using static RX.RXv1Assembler;
using static RX.Reg;
using Pheripheral;
using System.Buffers;

namespace IsrEmulationTest;
public class RXv1InstrunctionTest {
    RXv1Core cpu;
    RAM32Bit memory;
    readonly Translate rxv1Translate;

    public RXv1InstrunctionTest() {
        memory = new RAM32Bit("sram", 256);
        cpu = new RXv1Core(memory);
        rxv1Translate = new();
    }

    [SetUp]
    public void Setup() {
        memory = new RAM32Bit("sram", 256);
        cpu = new RXv1Core(memory);
    }

    void RunOpcode(params Instruction32[] instructions) {
        var writer = new ArrayBufferWriter<byte>();
        var asmWriter = new AssemblyWriter(writer);
        foreach (var inst in instructions) {
            rxv1Translate.CreateBinary(inst, ref asmWriter);
        }
        foreach(var inst in instructions) {
            cpu.ExecuteInstruction((OpCode)inst.OpcodeKind, inst.Operands.ToArray());
        }
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

    // [Test]
    public void BCnd_Test() {
        throw new NotImplementedException();
        // TODO 実装する
        // RunOpcode(BC_S(Cnd.EQ, ));
        // cpu.Registers[1].Is(0xFFFFFFDu);
    }

    // [Test]
    public void BMCnd_Test() {
        throw new NotImplementedException();
        // TODO 実装する
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

    // [Test]
    public void BRA_Test() {
        throw new NotImplementedException();
        // TODO 実装する
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

    // [Test]
    // public void BSR_Test() {
    //     throw new NotImplementedException();
    //     // TODO 実装する
    // }

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
    [TestCase(5u, 1u, true, false, false, false)]
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

    //[Test]
    public void JMP_Test() {
        throw new NotImplementedException();
    }

    //[Test]
    public void JSR_Test() {
        throw new NotImplementedException();
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
        RunOpcode(NOP);
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

    // [Test]
    // public void POP_Test() {
    //     throw new NotImplementedException();
    // }

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
    [TestCase( true,            0u,        0x8000_0000u,         0x8000_0000u,  false, false,  true, false)]
    [TestCase(false,            0u,        0x8000_0000u,         0x7FFF_FFFFu,  true, false, false,  true)]
    [TestCase( true,            1u,        0x8000_0000u,         0x7FFF_FFFFu,  true, false, false,  true)]
    [TestCase( true,          100u,          1_000_905u,           1_000_805u,  true, false, false, false)]
    [TestCase( true,         2000u,               2000u,                   0u,  true,  true, false, false)]
    [TestCase(false,         2000u,               2000u,  unchecked((uint)-1), false,  false, true, false)]
    [TestCase( true,  0x7FFF_FFFFu, unchecked((uint)-1),         0x8000_0000u, false,  false, true, false)]
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
    //[TestCase(                100u,          1_000_905u,             1_000_805u,  true, false, false, false)]
    //[TestCase(               2000u,               2000u,                     0u,  true,  true, false, false)]
    //[TestCase( unchecked((uint)-1),         0x7FFF_FFFFu,           0x8000_0000u,  true, false,  true,  true)]
    [TestCase(                100u, unchecked((uint)-1),   unchecked((uint)-101),  true, false,  true, false)]
    //[TestCase( unchecked((uint)-1),         0xFFFF_FFFFu,                     0u, false,  true, false,  true)]
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
}
