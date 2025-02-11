using System.Drawing;

namespace RX {
    public enum Reg {
        R0,
        R1,
        R2,
        R3,
        R4,
        R5,
        R6,
        R7,
        R8,
        R9,
        R10,
        R11,
        R12,
        R13,
        R14,
        R15,
    }

    public struct UInt3 {
        public readonly byte Value;

        public UInt3(byte value) {
            if (!(value <= 7)) {
                throw new ArgumentOutOfRangeException($"value <= 7 actual: {value}", nameof(value));
            }

            Value = value;
        }
    }

    public struct UInt4 {
        public readonly byte Value;

        public UInt4(byte value) {
            if (!(0 <= value && value <= 15)) {
                throw new ArgumentOutOfRangeException($"0 <= value <= 15 actual: {value}", nameof(value));
            }

            Value = value;
        }
    }

    public struct UInt5 {
        public readonly byte Value;

        public UInt5(byte value) {
            if (!(value <= 31)) {
                throw new ArgumentOutOfRangeException($"value <= 31 actual: {value}", nameof(value));
            }

            Value = value;
        }
    }

    public struct Int24 {
        public readonly int Value;

        public Int24(int value) {
            // 上位ビットが全て0(型に収まる値)もしくは23ビット以上が全て1(負の値)であることを確認する
            if (!((value & 0xFF00_0000) == 0 || (value & 0xFF80_0000) == 0xFF80_0000)) {
                throw new ArgumentOutOfRangeException($"-8388608 <= value <= 8388607 actual: {value}", nameof(value));
            }
            Value = value;
        }
    }

    /// <summary>
    /// レジスタ間接アドレッシング
    /// register reference
    /// </summary>
    public struct RegRef {
        public readonly Reg RegisterNo;
        public readonly MemEx Memex;

        public RegRef(Reg registerNo, MemEx mi) {
            RegisterNo = registerNo;
            Memex = mi;
        }
    }

    /// <summary>
    /// レジスタ相対アドレッシング
    /// relative reference
    /// </summary>
    public struct RelRef8 {
        public readonly byte Displacement;
        public readonly Reg RegisterNo;
        public readonly MemEx Memex;

        public RelRef8(byte displacement, Reg registerNo, MemEx mi) {
            Displacement = displacement;
            RegisterNo = registerNo;
            Memex = mi;
        }
    }

    /// <summary>
    /// レジスタ相対アドレッシング
    /// relative reference
    /// </summary>
    public struct RelRef16 {
        public readonly ushort Displacement;
        public readonly Reg RegisterNo;
        public readonly MemEx Memex;

        public RelRef16(ushort displacement, Reg registerNo, MemEx mi) {
            Displacement = displacement;
            RegisterNo = registerNo;
            Memex = mi;
        }
    }

    /// <summary>
    /// src が以下の、レジスタを使用したアドレッシングを扱います。
    /// - Rs
    /// - [Rx]
    /// - dsp:8[Rs]
    /// - dsp:16[Rs]
    /// </summary>
    public struct StdRegAddressing {
        /// <summary>
        /// null の場合は、「 memex == "UB" または src == Rs 」
        /// のオペコードになります。
        /// </summary>
        public readonly MemEx? Memex;

        public readonly LengthOfDisplacement LD;

        public readonly uint? Displacement;

        /// <summary>
        /// アドレッシングの対象
        /// </summary>
        public readonly Reg TargetReg;

        public StdRegAddressing(MemEx? mi, LengthOfDisplacement ld, Reg reg, uint? displacement) {
            Memex = mi;
            LD = ld;
            this.TargetReg = reg;
            Displacement = displacement;
        }

        public static implicit operator StdRegAddressing(Reg a) =>
            new(null, LengthOfDisplacement.Reg, a, null);

        public static implicit operator StdRegAddressing(RegRef a) =>
            new(a.Memex, LengthOfDisplacement.RefReg, a.RegisterNo, null);

        public static implicit operator StdRegAddressing(RelRef8 a) =>
            new(a.Memex, LengthOfDisplacement.DSP8Reg, a.RegisterNo, a.Displacement);

        public static implicit operator StdRegAddressing(RelRef16 a) =>
            new(a.Memex, LengthOfDisplacement.DSP16Reg, a.RegisterNo, a.Displacement);
    }

    public struct RegAddressing5 {
        public readonly byte Displacement;
        public readonly Reg TargetReg;

        public RegAddressing5(byte displacement, Reg targetReg) {
            if (!(Reg.R0 <= targetReg && targetReg <= Reg.R7)) {
                throw new ArgumentException($"actual: {targetReg}");
            }
            if (!(0 <= displacement && displacement <= 31)) {
                throw new ArgumentException($"actual: {displacement}");
            }
            Displacement = displacement;
            TargetReg = targetReg;
        }
    }

    public enum MemEx {
        B = 0b00,
        W = 0b01,
        L = 0b10,
        UW = 0b11,
    }

    public enum LengthOfImmediate : byte {
        SIMM8  = 0b01,
        SIMM16 = 0b10,
        SIMM24 = 0b11,
        IMM32  = 0b00
    }


    public enum LengthOfDisplacement : byte {
        Reg = 0b11,
        RefReg = 0b00,
        DSP8Reg = 0b01,
        DSP16Reg = 0b10
    }

    public enum Cnd {
        EQ,
        NE,
        GEU,
        LTU,
        GTU,
        LEU,
        PZ,
        N,
        GE,
        LT,
        GT,
        LE,
        O,
        NO,
        RA_B
    }

    /// <summary>
    /// MVFC と MVTC 命令で制御レジスタを指定するために使用します。
    /// </summary>
    public enum ControlReg {
        PSW   = 0b0000,
        USP   = 0b0010,
        FPSW  = 0b0011,
        BPSW  = 0b1000,
        BPC   = 0b1001,
        ISP   = 0b1010,
        FINTV = 0b1011,
        INTB  = 0b1100
    }

    public record Instruction32(uint OpcodeKind, IReadOnlyList<uint> Operands);

    /// <summary>
    /// 以下の即値をまとめて扱いします。
    /// SIMM:8
    /// SIMM:16
    /// SIMM:24
    /// IMM:32
    /// </summary>
    public struct StdImmValue {
        public readonly LengthOfImmediate LI;
        public readonly uint Value;

        public StdImmValue(LengthOfImmediate lengthOfImmediate, uint value) {
            LI = lengthOfImmediate;
            Value = lengthOfImmediate switch {
                LengthOfImmediate.SIMM8 => (uint)(int)(sbyte)value,
                LengthOfImmediate.SIMM16 => (uint)(int)(short)value,
                LengthOfImmediate.SIMM24 => (uint)new Int24((int)value).Value,
                LengthOfImmediate.IMM32 => value,
                _ => throw new ArgumentException($"{lengthOfImmediate}")
            };
        }

        public static implicit operator StdImmValue(sbyte a) {
            int expanded = a;
            return new(LengthOfImmediate.SIMM8, unchecked((uint)expanded));
        }

        public static implicit operator StdImmValue(short a) {
            int expanded = a;
            return new(LengthOfImmediate.SIMM16, unchecked((uint)expanded));
        }

        public static implicit operator StdImmValue(Int24 a) {
            int expanded = a.Value;
            return new(LengthOfImmediate.SIMM24, unchecked((uint)expanded));
        }

        public static implicit operator StdImmValue(int a) {
            int expanded = a;
            return new(LengthOfImmediate.IMM32, unchecked((uint)expanded));
        }
    }

    public enum Addressing {
        PostInc = 2,
        PreDec = 3
    }

    /// <summary>
    /// CLRPSW と SETPSW 命令で使用します。
    /// </summary>
    public enum PSWFlag {
        C = 0b0000,
        Z = 0b0001,
        S = 0b0010,
        O = 0b0011,
        I = 0b1000,
        U = 0b1001
    }

    public class RXv1Assembler {

        static Instruction32 Create(OpCode opcode, params uint?[] operands) =>
            new((uint)opcode,
                 operands.Where(x => x.HasValue)
                 .Select(x => x!.Value)
                 .ToArray());

        public static Instruction32 ABS(Reg dest) =>
            Create(OpCode.ABS_rd, (uint)dest);

        public static Instruction32 ABS(Reg src, Reg dest) =>
            Create(OpCode.ABS_rs_rd, (uint)src, (uint)dest);

        public static Instruction32 ADC(StdImmValue src, Reg dest) =>
            Create(OpCode.ADC_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 ADC(StdRegAddressing src, Reg dest) {
            if (src.Memex.HasValue) {
                if (src.Memex != MemEx.L) {
                    throw new ArgumentException(nameof(src.Memex));
                }
                return Create(OpCode.ADC_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
            }
            return Create(OpCode.ADC_rr, (uint)src.TargetReg, (uint)dest);
        }

        public static Instruction32 ADD(UInt4 src, Reg dest) =>
            Create(OpCode.ADD_4irr, src.Value, (uint)dest);

        public static Instruction32 ADD(StdImmValue src, Reg dest) =>
            Create(OpCode.ADD_irrr, (uint)dest, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 ADD(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.ADD_mr, (uint)src.Memex, (uint)src.LD, (uint)src.TargetReg, (uint)dest, src.Displacement)
            : Create(OpCode.ADD_ub_rs_mr, (uint)src.LD, (uint)src.TargetReg, (uint)dest, src.Displacement);

        public static Instruction32 ADD(StdImmValue src, Reg src2, Reg dest) =>
            Create(OpCode.ADD_irrr, (uint)src2, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 ADD(Reg src, Reg src2, Reg dest) =>
            Create(OpCode.ADD_rrr, (uint)dest, (uint)src, (uint)src2);

        public static Instruction32 AND(UInt4 src, Reg dest) =>
            Create(OpCode.AND_4ir, src.Value, (uint)dest);

        public static Instruction32 AND(StdImmValue src, Reg dest) =>
            Create(OpCode.AND_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 AND(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.AND_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.AND_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 AND(Reg src, Reg src2, Reg dest) =>
            Create(OpCode.AND_rrr, (uint)dest, (uint)src, (uint)src2);

        public static Instruction32 BCLR(UInt3 src, StdRegAddressing dest) {
            if (dest.LD == LengthOfDisplacement.Reg) {
                throw new ArgumentException($"actual: {dest.LD}");
            }
            if (dest.Memex != MemEx.B) {
                throw new ArgumentException($"actual: {dest.Memex}");
            }
            return Create(OpCode.BCLR_im, (uint)dest.TargetReg, src.Value, (uint)dest.LD, dest.Displacement);
        }

        public static Instruction32 BCLR(Reg src, StdRegAddressing dest) {
            if (dest.LD == LengthOfDisplacement.Reg) {
                return Create(OpCode.BCLR_rr, (uint)dest.TargetReg, (uint)src);
            }
            if (dest.Memex != MemEx.B) {
                throw new ArgumentException($"actual: {dest.Memex}");
            }
            return Create(OpCode.BCLR_rm, (uint)dest.TargetReg, (uint)src, (uint)dest.LD, dest.Displacement);
        }

        public static Instruction32 BCLR(UInt5 src, Reg dest) =>
            Create(OpCode.BCLR_ir, src.Value, (uint)dest);

        public static Instruction32 BC_S(Cnd condition, byte src) {
            if (!(3 <= src && src <= 10)) {
                throw new ArgumentOutOfRangeException($"3 <= src && src <= 10 actual: {src}", nameof(src));
            }
            if (!(condition is Cnd.EQ or Cnd.NE)) {
                throw new ArgumentException($"actual: {condition}", nameof(condition));
            }
            return Create(OpCode.BCnd_s, (uint)condition, src % 8u);
        }

        public static Instruction32 BC_B(Cnd condition, byte src) =>
            Create(OpCode.BCnd_b, (uint)condition, src);

        public static Instruction32 BC_W(Cnd condition, ushort src) {
            if (!(condition is Cnd.EQ or Cnd.NE)) {
                throw new ArgumentException($"actual: {condition}", nameof(condition));
            }
            return Create(OpCode.BCnd_w, (uint)condition, src);
        }

        public static Instruction32 BMC(Cnd condition, byte src, StdRegAddressing dest) {
            if (condition is Cnd.RA_B) {
                throw new ArgumentException($"actual: {condition}", nameof(condition));
            }
            if (dest.Memex.HasValue) {
                if (dest.Memex != MemEx.B) {
                    throw new ArgumentException($"actual: {dest.Memex}");
                }
                if (8 <= src) {
                    throw new ArgumentOutOfRangeException($"src <= 7 actual: {src}", nameof(src));
                }
                return Create(OpCode.BMCnd_im, src, (uint)dest.TargetReg, (uint)condition, (uint)dest.LD, dest.Displacement);
            }

            if (32 <= src) {
                throw new ArgumentOutOfRangeException($"src <= 31 actual: {src}", nameof(src));
            }
            return Create(OpCode.BMCnd_ir, src, (uint)condition, (uint)dest.TargetReg);
        }

        public static Instruction32 BNOT(UInt3 src, StdRegAddressing dest) {
            if (!dest.Memex.HasValue) {
                throw new ArgumentException();
            }
            if (dest.Memex != MemEx.B) {
                throw new ArgumentException($"actual: {dest.Memex}");
            }
            return Create(OpCode.BNOT_im, src.Value, (uint)dest.TargetReg, (uint)dest.LD, dest.Displacement);
        }

        public static Instruction32 BNOT(Reg src, StdRegAddressing dest) {
            if (dest.Memex.HasValue) {
                if (dest.Memex != MemEx.B) {
                    throw new ArgumentException($"actual: {dest.Memex}");
                }
                return Create(OpCode.BNOT_rm, (uint)dest.TargetReg, (uint)src, (uint)dest.LD, dest.Displacement);
            }
            return Create(OpCode.BNOT_rr, (uint)dest.TargetReg, (uint)src);
        }

        public static Instruction32 BNOT(UInt5 src, Reg dest) =>
            Create(OpCode.BNOT_ir, src.Value, (uint)dest);

        public static Instruction32 BRA_S(byte src) {
            if (!(3 <= src && src <= 10)) {
                throw new ArgumentOutOfRangeException($"3 <= src && src <= 10 actual: {src}", nameof(src));
            }
            return Create(OpCode.BRA_s, src % 8u);
        }

        public static Instruction32 BRA_B(sbyte src) {
            int expanded = src;
            return Create(OpCode.BRA_b, unchecked((uint)expanded));
        }

        public static Instruction32 BRA_W(short src) {
            int expanded = src;
            return Create(OpCode.BRA_w, unchecked((uint)expanded));
        }

        public static Instruction32 BRA_A(Int24 src) {
            int expanded = src.Value;
            return Create(OpCode.BRA_a, unchecked((uint)expanded));
        }

        public static Instruction32 BRA_L(Reg src) =>
            Create(OpCode.BRA_l, (uint)src);

        public static readonly Instruction32 BRK = Create(OpCode.BRK);

        public static Instruction32 BSET(UInt3 src, StdRegAddressing dest) {
            if (!dest.Memex.HasValue) {
                throw new ArgumentException();
            }
            if (dest.Memex != MemEx.B) {
                throw new ArgumentException($"actual: {dest.Memex}");
            }

            return Create(OpCode.BSET_im, (uint)dest.TargetReg, src.Value, (uint)dest.LD, dest.Displacement);
        }

        public static Instruction32 BSET(Reg src, StdRegAddressing dest) {
            if (dest.Memex.HasValue) {
                if (dest.Memex != MemEx.B) {
                    throw new ArgumentException($"actual: {dest.Memex}");
                }
                return Create(OpCode.BSET_rm, (uint)dest.TargetReg, (uint)src, (uint)dest.LD, dest.Displacement);
            }
            return Create(OpCode.BSET_rr, (uint)dest.TargetReg, (uint)src);
        }

        public static Instruction32 BSET(UInt5 src, Reg dest) {
            return Create(OpCode.BSET_ir, src.Value, (uint)dest);
        }

        public static Instruction32 BSR_W(short src) {
            int expanded = src;
            return Create(OpCode.BSR_w, unchecked((uint)expanded));
        }

        public static Instruction32 BSR_A(Int24 src) {
            int expanded = src.Value;
            return Create(OpCode.BSR_a, unchecked((uint)expanded));
        }

        public static Instruction32 BSR_L(Reg src) =>
            Create(OpCode.BSR_l, (uint)src);

        public static Instruction32 BTST(UInt3 src, StdRegAddressing src2) {
            if (!src2.Memex.HasValue) {
                throw new ArgumentException();
            }
            if (src2.Memex != MemEx.B) {
                throw new ArgumentException($"actual: {src2.Memex}");
            }
            return Create(OpCode.BTST_im, (uint)src2.TargetReg, src.Value, (uint)src2.LD, src2.Displacement);
        }

        public static Instruction32 BTST(Reg src, StdRegAddressing src2) {
            if (src2.Memex.HasValue) {
                if (src2.Memex != MemEx.B) {
                    throw new ArgumentException($"actual: {src2.Memex}");
                }
                return Create(OpCode.BTST_rm, (uint)src2.TargetReg, (uint)src, (uint)src2.LD, src2.Displacement);
            }
            return Create(OpCode.BTST_rr, (uint)src2.TargetReg, (uint)src);
        }

        public static Instruction32 BTST(UInt5 src, Reg dest) =>
            Create(OpCode.BTST_ir, src.Value, (uint)dest);

        public static Instruction32 CLRPSW(PSWFlag dest) =>
            Create(OpCode.CLRPSW, (uint)dest);

        public static Instruction32 CMP(UInt4 src, Reg src2) =>
            Create(OpCode.CMP_4ir, src.Value, (uint)src2);

        public static Instruction32 CMP(byte src, Reg src2) =>
            Create(OpCode.CMP_8ir, (uint)src2, src);

        public static Instruction32 CMP(StdImmValue src, Reg src2) =>
            Create(OpCode.CMP_ir, (uint)src2, (uint)src.LI, src.Value);

        public static Instruction32 CMP(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.CMP_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.CMP_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 DIV(StdImmValue src, Reg dest) =>
            Create(OpCode.DIV_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 DIV(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.DIV_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.DIV_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 DIVU(StdImmValue src, Reg dest) =>
            Create(OpCode.DIVU_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 DIVU(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.DIVU_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.DIVU_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 EMUL(StdImmValue src, Reg dest) =>
            Create(OpCode.EMUL_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 EMUL(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.EMUL_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.EMUL_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 EMULU(StdImmValue src, Reg dest) =>
            Create(OpCode.EMULU_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 EMULU(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.EMULU_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.EMULU_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 FADD(int src, Reg dest) {
            return Create(OpCode.FADD_ir, (uint)dest, unchecked((uint)src));
        }

        public static Instruction32 FADD(StdRegAddressing src, Reg dest) {
            if (src.Memex is not null and not MemEx.L) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.FADD_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 FCMP(int src, Reg dest) {
            return Create(OpCode.FCMP_ir, (uint)dest, unchecked((uint)src));
        }

        public static Instruction32 FCMP(StdRegAddressing src, Reg dest) {
            if (src.Memex is not null and not MemEx.L) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.FCMP_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 FDIV(int src, Reg dest) {
            return Create(OpCode.FDIV_ir, (uint)dest, unchecked((uint)src));
        }

        public static Instruction32 FDIV(StdRegAddressing src, Reg dest) {
            if (src.Memex is not null and not MemEx.L) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.FDIV_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 FMUL(int src, Reg dest) {
            return Create(OpCode.FMUL_ir, (uint)dest, unchecked((uint)src));
        }

        public static Instruction32 FMUL(StdRegAddressing src, Reg dest) {
            if (src.Memex is not null and not MemEx.L) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.FMUL_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 FSUB(int src, Reg dest) {
            return Create(OpCode.FSUB_ir, (uint)dest, unchecked((uint)src));
        }

        public static Instruction32 FSUB(StdRegAddressing src, Reg dest) {
            if (src.Memex is not null and not MemEx.L) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.FSUB_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 FTOI(StdRegAddressing src, Reg dest) {
            if (src.Memex is not null and not MemEx.L) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.FTOI, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 INT(byte src) =>
            Create(OpCode.INT, src);

        public static Instruction32 ITOF(StdRegAddressing src, Reg dest) {
            return src.Memex.HasValue
            ? Create(OpCode.ITOF_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.ITOF_ub_rs, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 JMP(Reg src) =>
            Create(OpCode.JMP, (uint)src);

        public static Instruction32 JSR(Reg src) =>
            Create(OpCode.JSR, (uint)src);

        public static Instruction32 MACHI(Reg src, Reg src2) =>
            Create(OpCode.MACHI, (uint)src, (uint)src2);

        public static Instruction32 MACLO(Reg src, Reg src2) =>
            Create(OpCode.MACLO, (uint)src, (uint)src2);

        public static Instruction32 MAX(StdImmValue src, Reg dest) =>
            Create(OpCode.MAX_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 MAX(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.MAX_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.MAX_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 MIN(StdImmValue src, Reg dest) =>
            Create(OpCode.MIN_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 MIN(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.MIN_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.MIN_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        static void AssertSizeBWL(MemEx sz) {
            if (!(sz is MemEx.B or MemEx.W or MemEx.L)) {
                throw new ArgumentException($"actual: {sz}");
            }
        }

        static void AssertRegRange(Reg src, Reg begin, Reg end) {
            if (!(begin <= src && src <= end)) {
                throw new ArgumentException($"actual: {src}");
            }
        }

        public static Instruction32 MOV(MemEx sz, Reg src, RegAddressing5 dest) {
            AssertSizeBWL(sz);
            AssertRegRange(src, Reg.R0, Reg.R7);
            return Create(OpCode.MOV_rm, (uint)sz, dest.Displacement, (uint)dest.TargetReg, (uint)src);
        }

        public static Instruction32 MOV(MemEx sz, RegAddressing5 src, Reg dest) {
            AssertSizeBWL(sz);
            AssertRegRange(dest, Reg.R0, Reg.R7);
            return Create(OpCode.MOV_mr, (uint)sz, src.Displacement, (uint)src.TargetReg, (uint)dest);
        }

        public static Instruction32 MOV(UInt4 src, Reg dest) =>
            Create(OpCode.MOV_4ir, src.Value, (uint)dest);

        public static Instruction32 MOV(MemEx sz, byte src, RegAddressing5 dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_im, (uint)sz, dest.Displacement, (uint)dest.TargetReg, src);
        }

        public static Instruction32 MOV(byte src, Reg dest) =>
            Create(OpCode.MOV_u8ir, (uint)dest, src);

        public static Instruction32 MOV(StdImmValue src, Reg dest) =>
            Create(OpCode.MOV_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 MOV(MemEx sz, Reg src, Reg dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_rr, (uint)sz, (uint)src, (uint)dest);
        }

        public static Instruction32 MOV(MemEx sz, StdImmValue src, StdRegAddressing dest) {
            AssertSizeBWL(sz);
            return dest.LD switch {
                LengthOfDisplacement.RefReg   => Create(OpCode.MOV_im_p    , (uint)dest.TargetReg, (uint)sz, (uint)LengthOfDisplacement.RefReg, (uint)src.LI, src.Value),
                LengthOfDisplacement.DSP8Reg  => Create(OpCode.MOV_im_dsp8 , (uint)dest.TargetReg, (uint)sz, (uint)dest.LD, dest.Displacement, (uint)src.LI, src.Value),
                LengthOfDisplacement.DSP16Reg => Create(OpCode.MOV_im_dsp16, (uint)dest.TargetReg, (uint)sz, (uint)dest.LD, dest.Displacement, (uint)src.LI, src.Value),
                _ => throw new ArgumentException($"not support: {dest.LD}")
            };
        }

        public static Instruction32 MOV(MemEx sz, StdRegAddressing src, Reg dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_l_mr, (uint)sz, (uint)src.LD, (uint)src.TargetReg, (uint)dest, src.Displacement);
        }

        public static Instruction32 MOV_indexed(MemEx sz, Reg src, UInt4 ni, Reg dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_ar, (uint)sz, ni.Value, (uint)dest, (uint)src);
        }

        public static Instruction32 MOV(MemEx sz, Reg src, StdRegAddressing dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_r_dsp, (uint)sz, (uint)dest.LD, (uint)dest.TargetReg, (uint)src, dest.Displacement);
        }

        public static Instruction32 MOV_indexed(MemEx sz, UInt4 ni, Reg src, Reg dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_ra, (uint)sz, ni.Value, (uint)dest, (uint)src);
        }

        public static Instruction32 MOV(MemEx sz, StdRegAddressing src, StdRegAddressing dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_mm, (uint)sz, (uint)dest.LD, (uint)src.LD, (uint)src.TargetReg, (uint)dest.TargetReg, src.Displacement, dest.Displacement);
        }

        public static Instruction32 MOV(MemEx sz, Reg src, Addressing ad, Reg dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_rp, ((uint)ad) & 1, (uint)sz, (uint)dest, (uint)src);
        }

        public static Instruction32 MOV(MemEx sz, Addressing ad, Reg src, Reg dest) {
            AssertSizeBWL(sz);
            return Create(OpCode.MOV_pr, (uint)ad, (uint)sz, (uint)src, (uint)dest);
        }

        static void AssertSizeBW(MemEx sz) {
            if (!(sz is MemEx.B or MemEx.W)) {
                throw new ArgumentException($"actual: {sz}");
            }
        }

        public static Instruction32 MOVU(MemEx sz, RegAddressing5 src, Reg dest) {
            AssertSizeBW(sz);
            return Create(OpCode.MOVU_dsp5_mr, (uint)sz, src.Displacement, (uint)src.TargetReg, (uint)dest);
        }

        public static Instruction32 MOVU(MemEx sz, StdRegAddressing src, Reg dest) {
            AssertSizeBW(sz);
            return Create(OpCode.MOVU_mr, (uint)sz, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 MOVU_indexed(MemEx sz, UInt4 ni, Reg rb, Reg dest) {
            AssertSizeBW(sz);
            return Create(OpCode.MOVU_ar, (uint)sz, ni.Value, (uint)rb, (uint)dest);
        }

        public static Instruction32 MOVU(MemEx sz, Addressing ad, Reg src, Reg dest) {
            AssertSizeBW(sz);
            return Create(OpCode.MOVU_pr, (uint)ad, (uint)sz, (uint)src, (uint)dest);
        }

        public static Instruction32 MUL(UInt4 src, Reg dest) =>
            Create(OpCode.MUL_4ir, src.Value);

        public static Instruction32 MUL(StdImmValue src, Reg dest) =>
            Create(OpCode.MUL_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 MUL(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.MUL_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.MUL_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 MUL(Reg src, Reg src2, Reg dest) =>
            Create(OpCode.MUL_rrr, (uint)dest, (uint)src, (uint)src2);

        public static Instruction32 MULHI(Reg src, Reg src2) =>
            Create(OpCode.MULHI, (uint)src, (uint)src2);

        public static Instruction32 MULLO(Reg src, Reg src2) =>
            Create(OpCode.MULLO, (uint)src, (uint)src2);

        public static Instruction32 MVFACHI(Reg dest) =>
            Create(OpCode.MVFACHI, (uint)dest);

        public static Instruction32 MVFACMI(Reg dest) =>
            Create(OpCode.MVFACMI, (uint)dest);

        public static Instruction32 MVFC(ControlReg src, Reg dest) =>
            Create(OpCode.MVFC, (uint)src, (uint)dest);

        public static Instruction32 MVTACHI(Reg src) =>
            Create(OpCode.MVTACHI, (uint)src);

        public static Instruction32 MVTACLO(Reg src) =>
            Create(OpCode.MVTACLO, (uint)src);

        public static Instruction32 MVTC(StdImmValue src, ControlReg dest) =>
            Create(OpCode.MVTC_i, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 MVTC(Reg src, ControlReg dest) =>
            Create(OpCode.MVTC_r, (uint)src, (uint)dest);

        public static Instruction32 MVTIPL(UInt4 src) =>
            Create(OpCode.MVTIPL, src.Value);

        public static Instruction32 NEG(Reg dest) =>
            Create(OpCode.NEG_rd, (uint)dest);

        public static Instruction32 NEG(Reg src, Reg dest) =>
            Create(OpCode.NEG_rs_rd, (uint)src, (uint)dest);

        public static readonly Instruction32 NOP =
            Create(OpCode.NOP);

        public static Instruction32 NOT(Reg dest) =>
            Create(OpCode.NOT_rd, (uint)dest);

        public static Instruction32 NOT(Reg src, Reg dest) =>
            Create(OpCode.NOT_rs_rd, (uint)src, (uint)dest);

        public static Instruction32 OR(UInt4 src, Reg dest) =>
            Create(OpCode.OR_4ir, src.Value, (uint)dest);

        public static Instruction32 OR(StdImmValue src, Reg dest) =>
            Create(OpCode.OR_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 OR(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.OR_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.OR_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 OR(Reg src, Reg src2, Reg dest) =>
            Create(OpCode.OR_rrr, (uint)dest, (uint)src, (uint)src2);

        public static Instruction32 POP(Reg dest) =>
            Create(OpCode.POP, (uint)dest);

        public static Instruction32 POPC(ControlReg dest) =>
            Create(OpCode.POPC, (uint)dest);

        public static Instruction32 POPM(Reg dest, Reg dest2) =>
            Create(OpCode.POPM, (uint)dest, (uint)dest2);

        public static Instruction32 PUSH(MemEx size, Reg src) {
            if (size == MemEx.UW) {
                throw new ArgumentException($"actual: {size}");
            }
            return Create(OpCode.PUSH_r, (uint)size, (uint)src);
        }

        public static Instruction32 PUSH(StdRegAddressing src) {
            if (!src.Memex.HasValue) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            if (src.LD == LengthOfDisplacement.Reg) {
                throw new ArgumentException($"actual: {src.LD}");
            }
            if (src.Memex == MemEx.UW) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.PUSH_m, (uint)src.TargetReg, (uint)src.Memex, (uint)src.LD, src.Displacement);
        }

        public static Instruction32 PUSHC(ControlReg src) =>
            Create(OpCode.PUSHC, (uint)src);

        public static Instruction32 PUSHM(Reg src, Reg src2) =>
            Create(OpCode.PUSHM, (uint)src, (uint)src2);

        public static Instruction32 RACW(byte src) {
            if (!(src is 1 or 2)) {
                throw new ArgumentException($"actual: {src}");
            }
            return Create(OpCode.RACW, (uint)(src - 1));
        }

        public static Instruction32 REVL(Reg src, Reg dest) =>
            Create(OpCode.REVL, (uint)src, (uint)dest);

        public static Instruction32 REVW(Reg src, Reg dest) =>
            Create(OpCode.REVW, (uint)src, (uint)dest);

        public static Instruction32 RMPA(MemEx size) {
            if (size == MemEx.UW) {
                throw new ArgumentException($"actual: {size}");
            }
            return Create(OpCode.RMPA);
        }

        public static Instruction32 ROLC(Reg dest) =>
            Create(OpCode.ROLC, (uint)dest);

        public static Instruction32 RORC(Reg dest) =>
            Create(OpCode.RORC, (uint)dest);

        public static Instruction32 ROTL(UInt5 src, Reg dest) =>
            Create(OpCode.ROTL_ir, src.Value, (uint)dest);

        public static Instruction32 ROTL(Reg src, Reg dest) =>
            Create(OpCode.ROTL_rr, (uint)src, (uint)dest);

        public static Instruction32 ROTR(UInt5 src, Reg dest) =>
            Create(OpCode.ROTR_ir, src.Value, (uint)dest);

        public static Instruction32 ROTR(Reg src, Reg dest) =>
            Create(OpCode.ROTR_rr, (uint)src, (uint)dest);

        public static Instruction32 ROUND(StdRegAddressing src, Reg dest) {
            if (src.Memex is not null and not MemEx.L) {
                throw new ArgumentException($"actual: {src.Memex}");
            }
            return Create(OpCode.ROUND, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
        }

        public static readonly Instruction32 RTE =
            Create(OpCode.RTE);

        public static readonly Instruction32 RTFI =
            Create(OpCode.RTFI);

        public static readonly Instruction32 RTS =
            Create(OpCode.RTS);

        public static Instruction32 RTSD(byte src) =>
            Create(OpCode.RTSD_i, src);

        public static Instruction32 RTSD(byte src, Reg dest, Reg dest2) =>
            Create(OpCode.RTSD_irr, (uint)dest, (uint)dest2, src);

        public static Instruction32 SAT(Reg dest) =>
            Create(OpCode.SAT, (uint)dest);

        public static readonly Instruction32 SATR =
            Create(OpCode.SATR);

        public static Instruction32 SBB(StdRegAddressing src, Reg dest) {
            if (src.Memex.HasValue) {
                if (src.Memex != MemEx.L) {
                    throw new ArgumentException($"actual: {src.Memex}");
                }
                return Create(OpCode.SBB_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);
            }
            return Create(OpCode.SBB_rr, (uint)src.TargetReg, (uint)dest);
        }

        public static Instruction32 SCC(Cnd condition, Reg dest) {
            if (condition == Cnd.RA_B) {
                throw new ArgumentException($"actual: {condition}");
            }
            return Create(OpCode.SCCnd, (uint)MemEx.L, (uint)dest, (uint)condition, (uint)LengthOfDisplacement.Reg);
        }

        public static Instruction32 SCC(Cnd condition, StdRegAddressing dest) {
            if (dest.Memex is null or MemEx.UW) {
                throw new ArgumentException($"actual: {dest.Memex}");
            }
            if (condition == Cnd.RA_B) {
                throw new ArgumentException($"actual: {condition}");
            }
            return Create(OpCode.SCCnd, (uint)dest.Memex, (uint)dest.TargetReg, (uint)condition, (uint)dest.LD, dest.Displacement);
        }

        public static readonly Instruction32 SCMPU =
            Create(OpCode.SCMPU);

        public static Instruction32 SETPSW(PSWFlag dest) =>
            Create(OpCode.SETPSW, (uint)dest);

        public static Instruction32 SHAR(UInt5 src, Reg dest) =>
            Create(OpCode.SHAR_5irr, src.Value, (uint)dest);

        public static Instruction32 SHAR(Reg src, Reg dest) =>
            Create(OpCode.SHAR_rr, (uint)src, (uint)dest);

        public static Instruction32 SHAR(UInt5 src, Reg src2, Reg dest) =>
            Create(OpCode.SHAR_irr, src.Value, (uint)src2, (uint)dest);

        public static Instruction32 SHLL(UInt5 src, Reg dest) =>
            Create(OpCode.SHLL_5irr, src.Value, (uint)dest);

        public static Instruction32 SHLL(Reg src, Reg dest) =>
            Create(OpCode.SHLL_rr, (uint)src, (uint)dest);

        public static Instruction32 SHLL(UInt5 src, Reg src2, Reg dest) =>
            Create(OpCode.SHLL_irr, src.Value, (uint)src2, (uint)dest);

        public static Instruction32 SHLR(UInt5 src, Reg dest) =>
            Create(OpCode.SHLR_5irr, src.Value, (uint)dest);

        public static Instruction32 SHLR(Reg src, Reg dest) =>
            Create(OpCode.SHLR_rr, (uint)src, (uint)dest);

        public static Instruction32 SHLR(UInt5 src, Reg src2, Reg dest) =>
            Create(OpCode.SHLR_irr, src.Value, (uint)src2, (uint)dest);

        public static readonly Instruction32 SMOVB =
            Create(OpCode.SMOVB);

        public static readonly Instruction32 SMOVF =
            Create(OpCode.SMOVF);

        public static readonly Instruction32 SMOVU =
            Create(OpCode.SMOVU);

        public static Instruction32 SSTR(MemEx size) {
            if (size == MemEx.UW) {
                throw new ArgumentException($"actual: {size}");
            }
            return Create(OpCode.SSTR, (uint)size);
        }

        public static Instruction32 STNZ(StdImmValue src, Reg dest) =>
            Create(OpCode.STNZ, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 STZ(StdImmValue src, Reg dest) =>
            Create(OpCode.STZ, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 SUB(UInt4 src, Reg dest) =>
            Create(OpCode.SUB_ir, src.Value, (uint)dest);

        public static Instruction32 SUB(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.SUB_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.SUB_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 SUB(Reg src, Reg src2, Reg dest) =>
            Create(OpCode.SUB_rrr, (uint)dest, (uint)src, (uint)src2);

        public static Instruction32 SUNTIL(MemEx size) {
            if (size == MemEx.UW) {
                throw new ArgumentException($"actual: {size}");
            }
            return Create(OpCode.SUNTIL, (uint)size);
        }

        public static Instruction32 SWHILE(MemEx size) {
            if (size == MemEx.UW) {
                throw new ArgumentException($"actual: {size}");
            }
            return Create(OpCode.SWHILE, (uint)size);
        }

        public static Instruction32 TST(StdImmValue src, Reg src2) =>
            Create(OpCode.TST_ir, (uint)src2, (uint)src.LI, src.Value);

        public static Instruction32 TST(StdRegAddressing src, Reg src2) =>
            src.Memex.HasValue
            ? Create(OpCode.TST_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)src2, (uint)src.LD, src.Displacement)
            : Create(OpCode.TST_ub_rs_mr, (uint)src.TargetReg, (uint)src2, (uint)src.LD, src.Displacement);

        public static readonly Instruction32 WAIT =
            Create(OpCode.WAIT);

        public static Instruction32 XCHG(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.XCHG_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.XCHG_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);

        public static Instruction32 XOR(StdImmValue src, Reg dest) =>
            Create(OpCode.XOR_ir, (uint)dest, (uint)src.LI, src.Value);

        public static Instruction32 XOR(StdRegAddressing src, Reg dest) =>
            src.Memex.HasValue
            ? Create(OpCode.XOR_mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement)
            : Create(OpCode.XOR_ub_rs_mr, (uint)src.TargetReg, (uint)dest, (uint)src.LD, src.Displacement);


        // public static readonly Instruction32  =
        //     Create(OpCode.);



        /// ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        // public static Instruction32 (UInt5 src, Reg dest) =>
        //     Create(OpCode., src.Value, (uint)dest);

        // public static Instruction32 (Reg src, Reg dest) =>
        //     Create(OpCode., (uint)src, (uint)dest);

        // public static Instruction32 (UInt5 src, Reg src2, Reg dest) =>
        //     Create(OpCode., src.Value, (uint)src2, (uint)dest);
        /// ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑


        /// ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        // public static Instruction32 (Reg src) =>
        //     Create(OpCode., (uint)src);
        /// ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑




        // public static Instruction32 (Reg dest) =>
        //     Create(OpCode._rd, (uint)dest);

        // public static Instruction32 (Reg src, Reg dest) =>
        //     Create(OpCode._rs_rd, (uint)src, (uint)dest);


        /// ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        // public static Instruction32 (UInt4 src, Reg dest) =>
        //     Create(OpCode._4ir, src.Value, (uint)dest);

        // public static Instruction32 (StdImmValue src, Reg dest) =>
        //     Create(OpCode._ir, (uint)dest, src.LI, src.Value);

        // public static Instruction32 (StdRegAddressing src, Reg dest) =>
        //     src.Memex.HasValue
        //     ? Create(OpCode._mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, src.LD, src.Displacement)
        //     : Create(OpCode._ub_rs_mr, (uint)src.TargetReg, (uint)dest, src.LD, src.Displacement);

        // public static Instruction32 (Reg src, Reg src2, Reg dest) =>
        //     Create(OpCode._rrr, (uint)dest, (uint)src, (uint)src2);
        /// ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑




        // public static Instruction32 (Reg src, Reg src2) =>
        //     Create(OpCode., (uint)src, (uint)src2);

        // public static Instruction32 (Reg src, Reg dest) =>
        //     Create(OpCode., (uint)dest, (uint)src);


        /// ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        // public static Instruction32 (Reg src) =>
        //     Create(OpCode., (uint)src);
        /// ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

        // public static Instruction32 F(int src, Reg dest) {
        //     return Create(OpCode.F_ir, (uint)dest, unchecked((uint)src));
        // }

        // public static Instruction32 F(StdRegAddressing src, Reg dest) {
        //     if (src.Memex is not null and not MemEx.L) {
        //         throw new ArgumentException($"actual: {src.Memex}");
        //     }
        //     return Create(OpCode.F_mr, (uint)src.TargetReg, (uint)dest, src.LD, src.Displacement);
        // }


        // public static Instruction32 (StdImmValue src, Reg dest) =>
        //     Create(OpCode._ir, (uint)dest, src.LI, src.Value);

        // public static Instruction32 (StdRegAddressing src, Reg dest) =>
        //     src.Memex.HasValue
        //     ? Create(OpCode._mr, (uint)src.Memex, (uint)src.TargetReg, (uint)dest, src.LD, src.Displacement)
        //     : Create(OpCode._ub_rs_mr, (uint)src.TargetReg, (uint)dest, src.LD, src.Displacement);

    }
}
