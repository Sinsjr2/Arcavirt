using Pheripheral;
using Util;

namespace RX {
    public class RXv1Core : CPU.ILeveledISRNotify {
        public readonly uint[] Registers = new uint[16];

        uint isp;
        public uint ISP {
            set {
                if (PSW_u) {
                    isp = value;
                }
                else {
                    Registers[0] = value;
                }
            }
            get => PSW_u ? isp : Registers[0];
        }

        uint usp;
        public uint USP {
            set {
                if (PSW_u) {
                    Registers[0] = value;
                }
                else {
                    usp = value;
                }
            }
            get => PSW_u ? Registers[0] : usp;
        }
        public uint INTB;
        public uint EXTB;
        public uint PC;

        public uint SP {
            get => Registers[0];
            set => Registers[0] = value;
        }

        #region PSW
        public bool PSW_c;
        public bool PSW_z;
        public bool PSW_s;
        public bool PSW_o;
        public bool PSW_i;
        bool psw_u;
        public bool PSW_u {
            get => psw_u;
            set {
                if (psw_u == value) {
                    return;
                }
                psw_u = value;
                if (value) {
                    isp = Registers[0];
                    Registers[0] = usp;
                }
                else {
                    usp = Registers[0];
                    Registers[0] = isp;
                }
            }
        }
        public bool PSW_pm;
        public byte PSW_ipl;

        public uint PSW {
            get =>
                ((PSW_c ? 1u : 0u) << 0) |
                ((PSW_z ? 1u : 0u) << 1) |
                ((PSW_s ? 1u : 0u) << 2) |
                ((PSW_o ? 1u : 0u) << 3) |
                ((PSW_i ? 1u : 0u) << 16) |
                ((PSW_u ? 1u : 0u) << 17) |
                ((PSW_pm ? 1u : 0u) << 20) |
                ((uint)PSW_ipl << 24);
            set {
                PSW_c = (value & (1u << 0)) != 0u;
                PSW_z = (value & (1u << 1)) != 0u;
                PSW_s = (value & (1u << 2)) != 0u;
                PSW_o = (value & (1u << 3)) != 0u;
                PSW_i = (value & (1u << 16)) != 0u;
                PSW_u = (value & (1u << 17)) != 0u;
                PSW_pm = (value & (1u << 20)) != 0u;
                PSW_ipl = (byte)((value >> 24) & 0b1111);
            }
        }
        #endregion

        public uint BPC;
        public uint BPSW;
        public uint FINTV;

        #region FPSW
        public RXFloatRoundingMode FPSW_rm;
        public bool FPSW_cv;
        public bool FPSW_co;
        public bool FPSW_cz;
        public bool FPSW_cu;
        public bool FPSW_cx;
        public bool FPSW_ce;
        public bool FPSW_dn;
        public bool FPSW_ev;
        public bool FPSW_eo;
        public bool FPSW_ez;
        public bool FPSW_eu;
        public bool FPSW_ex;
        public bool FPSW_fv;
        public bool FPSW_fo;
        public bool FPSW_fz;
        public bool FPSW_fu;
        public bool FPSW_fx;
        public bool FPSW_fs;

        public uint FPSW {
            get {
                return
                    (((byte)FPSW_rm & 0b11u) << 0) |
                    ((FPSW_cv ? 1u : 0u) << 2) |
                    ((FPSW_co ? 1u : 0u) << 3) |
                    ((FPSW_cz ? 1u : 0u) << 4) |
                    ((FPSW_cu ? 1u : 0u) << 5) |
                    ((FPSW_cx ? 1u : 0u) << 6) |
                    ((FPSW_ce ? 1u : 0u) << 7) |
                    ((FPSW_dn ? 1u : 0u) << 8) |
                    ((FPSW_ev ? 1u : 0u) << 10) |
                    ((FPSW_eo ? 1u : 0u) << 11) |
                    ((FPSW_ez ? 1u : 0u) << 12) |
                    ((FPSW_eu ? 1u : 0u) << 13) |
                    ((FPSW_ex ? 1u : 0u) << 14) |
                    ((FPSW_fv ? 1u : 0u) << 26) |
                    ((FPSW_fo ? 1u : 0u) << 27) |
                    ((FPSW_fz ? 1u : 0u) << 28) |
                    ((FPSW_fu ? 1u : 0u) << 29) |
                    ((FPSW_fx ? 1u : 0u) << 30) |
                    ((FPSW_fs ? 1u : 0u) << 31);
            }
            set {
                FPSW_rm = (RXFloatRoundingMode)(value & 0b11u);
                FPSW_cv = (value & (1u << 2)) != 0;
                FPSW_co = (value & (1u << 3)) != 0;
                FPSW_cz = (value & (1u << 4)) != 0;
                FPSW_cu = (value & (1u << 5)) != 0;
                FPSW_cx = (value & (1u << 6)) != 0;
                FPSW_ce = (value & (1u << 7)) != 0;
                FPSW_dn = (value & (1u << 8)) != 0;
                FPSW_ev = (value & (1u << 10)) != 0;
                FPSW_eo = (value & (1u << 11)) != 0;
                FPSW_ez = (value & (1u << 12)) != 0;
                FPSW_eu = (value & (1u << 13)) != 0;
                FPSW_ex = (value & (1u << 14)) != 0;
                FPSW_fv = (value & (1u << 26)) != 0;
                FPSW_fo = (value & (1u << 27)) != 0;
                FPSW_fz = (value & (1u << 28)) != 0;
                FPSW_fu = (value & (1u << 29)) != 0;
                FPSW_fx = (value & (1u << 30)) != 0;
                FPSW_fs = (value & (1u << 31)) != 0;
            }
        }
        #endregion

        public ulong Acc;

        int pendingIrqNo;
        int pendingPriorityNo;

        public bool Waiting { get; private set; }

        readonly GPIO.OutputSignalBit isrAck = new("isrAck");

        readonly IBus32 bus;

        public RXv1Core(IBus32 bus) {
            this.bus = bus;
        }

        uint CalcDspAddr(uint dsp, int reg) {
            return Registers[reg] + dsp;
        }

        /// <summary>
        /// dsp[reg]
        /// </summary>
        uint ReadIndexAddr(int ld, uint? dsp, int reg) {
            switch (ld) {
                case (int)LengthOfDisplacement.RefReg:
                    return Registers[reg];
                case (int)LengthOfDisplacement.DSP8Reg:
                case (int)LengthOfDisplacement.DSP16Reg:
                    return CalcDspAddr(dsp!.Value, reg);
            }
            throw new ArgumentException($"expected: 0 <= && ld < 3 actual: {ld}");
        }

        struct MemoryAccessOp {
            public readonly byte Size;
            public readonly bool IsSigned;

            public MemoryAccessOp(byte size, bool isSigned) {
                Size = size;
                IsSigned = isSigned;
            }
        }

        /// <summary>
        /// mi の値から変換するためのテーブル
        /// </summary>
        static readonly ReadOnlyMemory<MemoryAccessOp> MemOps = new MemoryAccessOp[] {
            new(0, true),
            new(1, true),
            new(2, false),
            new(1, false),
            new(0, false)
        };

        /// <summary>
        /// 即値を符号拡張もしくはゼロ拡張します。
        /// </summary>
        static uint ImmediateValueExpantion(uint sz, uint value) {
            var memOps = MemOps.Span[(int)sz];
            return SignExtension(memOps.IsSigned, value, memOps.Size);
        }

        /// <summary>
        /// value が符号ありの場合、符号拡張を行います。
        /// それ以外は、ゼロ拡張として扱うので何もせずにそのまま返します。
        /// </summary>
        static uint SignExtension(bool isSigned, uint value, int size) {
            if (!isSigned) {
                return value;
            }
            switch (size) {
                case 0:
                    return (uint)(int)unchecked((sbyte)value);
                case 1:
                    return (uint)(int)unchecked((short)value);
                case 2:
                    // uint のサイズと同じであるので符号拡張出来ないので何もしない
                    return value;
            }
            throw new ArgumentException($"not supported size. actual: {size}");
        }

        uint LoadSourceOperand(uint ld, uint mi, uint rs, ReadOnlySpan<uint> dsp) {
            if (ld == 0) {
                var memOp = MemOps.Span[(int)mi];
                return SignExtension(memOp.IsSigned, bus.Read(Registers[rs], 1 << memOp.Size), memOp.Size);
            }
            if (ld < 3) {
                var memOp = MemOps.Span[(int)mi];
                var addr = ReadIndexAddr((int)ld, dsp[0], (int)rs);
                return SignExtension(memOp.IsSigned, bus.Read(addr, 1 << memOp.Size), memOp.Size);
            }
            return Registers[rs];
        }

        uint LoadUnsignedSourceOperand(uint ld, uint sz, ReadOnlySpan<uint> dsp, uint rs) {
            if (ld == 0) {
                return bus.Read(Registers[rs], 1 << (int)sz);
            }
            if (ld < 3) {
                var addr = ReadIndexAddr((int)ld, dsp[0], (int)rs);
                return bus.Read(addr, 1 << (int)sz);
            }
            return BitOperation.GetLowerBits(Registers[rs], 1u << (int)sz);
        }

        uint LoadUnsignedSourceOperand(uint sz, uint dsp, uint rs) {
            return bus.Read(CalcDspAddr(dsp, (int)rs), 1 << (int)sz);
        }

        uint LoadSourceOperand(uint sz, uint dsp, uint rs) {
            return SignExtension(true, LoadUnsignedSourceOperand(sz, dsp, rs), (int)sz);
        }

        void StoreDestOperand(uint mi, uint rd, uint ld, ReadOnlySpan<uint> dsp, uint value) {
            if (ld == 0) {
                var memOp = MemOps.Span[(int)mi];
                bus.Write(Registers[rd], 1 << memOp.Size, value);
                return;
            }
            if (ld < 3) {
                var memOp = MemOps.Span[(int)mi];
                var addr = ReadIndexAddr((int)ld, dsp[0], (int)rd);
                bus.Write(addr, 1 << memOp.Size, value);
                return;
            }
            Registers[rd] = value;
        }

        void StoreDestOperand(uint sz, uint dsp,  uint rd, uint value) {
            var addr = CalcDspAddr(dsp, (int)rd);
            var size = sz switch {
                (uint)MemEx.B => 1,
                (uint)MemEx.W => 2,
                (uint)MemEx.L => 4,
                _ => throw new ArgumentException($"not supported sz. actual: {sz}")
            };
            bus.Write(addr, size, value);
        }

        void SetPSWFlag(uint cb, bool value) {
            switch (cb) {
                case 0b000:
                    PSW_c = value;
                    break;
                case 0b0001:
                    PSW_z = value;
                    break;
                case 0b0010:
                    PSW_s = value;
                    break;
                case 0b0011:
                    PSW_o = value;
                    break;
                case 0b1000 when !PSW_pm:
                    PSW_i = value;
                    break;
                case 0b1001 when !PSW_pm:
                    PSW_u = value;
                    break;
            }
        }

        void SetControlRegister(uint cr, uint value) {
            // TODO スーパーバイザーモードでしか書き込めないレジスタにガードを追加する
            switch (cr) {
                case (uint)ControlReg.PSW:
                    PSW = value;
                    break;
                case 0b0001:
                    // reserved
                    break;
                case (uint)ControlReg.USP:
                    USP = value;
                    break;
                case (uint)ControlReg.FPSW:
                    FPSW = value;
                    break;
                case 0b0100:
                case 0b0101:
                case 0b0110:
                case 0b0111:
                    // reserved
                    break;
                case (uint)ControlReg.BPSW:
                    BPSW = value;
                    break;
                case (uint)ControlReg.BPC:
                    BPC = value;
                    break;
                case (uint)ControlReg.ISP:
                    ISP = value;
                    break;
                case (uint)ControlReg.FINTV:
                    FINTV = value;
                    break;
                case (uint)ControlReg.INTB:
                    INTB = value;
                    break;
                default:
                    // reserved
                    break;
            }
        }

        uint GetControlRegister(uint cr) {
            switch (cr) {
                case 0b0000:
                    return PSW;
                case 0b0001:
                    return PC;
                case 0b0010:
                    return USP;
                case 0b0011:
                    return FPSW;
                case 0b0100:
                case 0b0101:
                case 0b0110:
                case 0b0111:
                    // reserved
                    return 0;
                case 0b1000:
                    return BPSW;
                case 0b1001:
                    return BPC;
                case 0b1010:
                    return ISP;
                case 0b1011:
                    return FINTV;
                case 0b1100:
                    return INTB;
                default:
                    // reserved
                    return 0;
            }
        }

        bool CheckCondition(uint cond) {
            switch (cond) {
                case 0b0000: // BEQ,BZ
                    return PSW_z;
                case 0b0001: // BNE, BNZ
                    return !PSW_z;
                case 0b0010: // BGEU, BC
                    return PSW_c;
                case 0b0011: // BLTU, BNC
                    return !PSW_c;
                case 0b0100: // BGTU
                    return PSW_c && !PSW_z;
                case 0b0101: // BLEU
                    return !(PSW_c && !PSW_z);
                case 0b0110: // BPZ
                    return !PSW_s;
                case 0b0111: // BN
                    return PSW_s;
                case 0b1000: //BGE
                    return !(PSW_s ^ PSW_o);
                case 0b1001: // BLT
                    return PSW_s ^ PSW_o;
                case 0b1010: // BGT
                    return !((PSW_s ^ PSW_o) || PSW_z);
                case 0b1011: // BLE
                    return (PSW_s ^ PSW_o) || PSW_z;
                case 0b1100: // BO
                    return PSW_o;
                case 0b1101: // BNO
                    return !PSW_o;
                case 0b1110: // BRA.B
                    return true;
                case 0b1111: // reserved
                    return false;
            }
            throw new ArgumentException($"expected: 0 <= {nameof(cond)} <= 15, expected: {cond}");
        }

        public void SetInterrupt(int irqNo, int priority) {
            Waiting = false;
            pendingIrqNo = irqNo;
            pendingPriorityNo = priority;
        }

        void RunExceptionIfNeed() {
            if (PSW_i && PSW_ipl < pendingPriorityNo) {
                RunException(INTB, pendingIrqNo, pendingPriorityNo == 0xFF);
                isrAck.SetOutputSignal(true);
                isrAck.SetOutputSignal(false);
            }
        }

        void RunException(uint baseAddr, int exceptionEntryNumber, bool isFastIntrrupt) {
            if (isFastIntrrupt) {
                BPC = PC;
                BPSW = PSW;
                PSW_u = false;
                PSW_i = false;
                PSW_pm = false;
                PC = FINTV;
            }
            else {
                SP -= (uint)4;
                bus.Write(SP, 4, PC);
                SP -= (uint)4;
                bus.Write(SP, 4, PSW);
                PSW_u = false;
                PSW_i = false;
                PSW_pm = false;
            }

            var readAddr = (baseAddr + (uint)(exceptionEntryNumber * 4));
            PC = bus.Read(readAddr, 4);
        }

        static bool IsNegativeValue(uint x) {
            return (x & (1 << 31)) != 0;
        }

        /// <summary>
        /// IEEE 754 で0であるかを判定します。
        /// </summary>
        static bool IsFZero(uint x) {
            return (x & 0x7FFF_FFFF) == 0;
        }

        void AddFlags(uint op1, uint op2, uint result) {
            uint index = (op1 >> 31) | ((op2 >> 31) << 1) | ((result >> 31) << 2);

            (bool c, bool o, bool s) flag = index switch {
                0b000 => (false, false, false),
                0b001 => (true, false, false),
                0b010 => (true, false, false),
                0b011 => (true, true, false),
                0b100 => (false, true, true),
                0b101 => (false, false, true),
                0b110 => (false, false, true),
                0b111 => (true, false, true),
                _ => (false, false, false)
            };

            PSW_z = result == 0;
            PSW_s = flag.s;
            PSW_o = flag.o;
            PSW_c = flag.c;
        }

        void SubFlags(uint op1, uint op2, uint result) {
            PSW_c = (IsNegativeValue(op1) && !IsNegativeValue(op2))
                 || (IsNegativeValue(op1) && !IsNegativeValue(result))
                 || (!IsNegativeValue(op2) && !IsNegativeValue(result));
            PSW_o = (IsNegativeValue(op1) && !IsNegativeValue(op2) && !IsNegativeValue(result))
                || (!IsNegativeValue(op1) && IsNegativeValue(op2) && IsNegativeValue(result));
            PSW_z = result == 0;
            PSW_s = IsNegativeValue(result);
        }

        void AndFlags(uint result) {
            PSW_s = IsNegativeValue(result);
            PSW_z = result == 0;
        }

        void OrFlags(uint result) {
            PSW_s = IsNegativeValue(result);
            PSW_z = result == 0;
        }

         void NotFlags(uint result) {
            PSW_s = IsNegativeValue(result);
            PSW_z = result == 0;
        }

        void XorFlags(uint result)
        {
            PSW_s = IsNegativeValue(result);
            PSW_z = result == 0;
        }

        uint OpABS(uint src) {
            var result = (uint)Math.Abs((int)src);
            PSW_z = result == 0;
            PSW_s = result >> 31 == 1;
            PSW_o = src == 0x80000000;
            return result;
        }

        uint OpADC(uint a, uint b) {
            var result = unchecked(a + b + (PSW_c ? 1u : 0u));
            return result;
        }

        uint OpADD(uint a, uint b) {
            var result = unchecked(a + b);
            AddFlags(a, b, result);
            return result;
        }

        uint OpAND(uint a, uint b) {
            var result = a & b;
            AndFlags(result);
            return result;
        }

        byte OpBCLR_m(uint a, uint b) {
            return (byte)(b & ~(1u << (int)(a & 7)));
        }

        uint OpBCLR_r(uint src, uint dest) {
            return dest & ~(1u << (int)(src & 31));
        }

        void OpBCnd(uint condition, uint src, uint opSize) {
            if (CheckCondition(condition)) {
                PC += src;
            }
            else {
                PC += opSize;
            }
        }

        byte OpBMCnd_m(uint cnd, uint src, uint dest) {
            if (CheckCondition(cnd)) {
                return (byte)(dest | (1u << (int)(src & 7u)));
            }
            return (byte)(dest & ~(1u << (int)(src & 7u)));
        }

        uint OpBMCnd_r(uint cnd, uint src, uint dest) {
            if (CheckCondition(cnd)) {
                return dest | (1u << (int)(src & 31u));
            }
            return dest & ~(1u << (int)(src & 31u));
        }

        byte OpBNOT_m(uint src, byte dest) {
            return (byte)(dest ^ (1u << (int)(src & 7u)));
        }

        uint OpBNOT_r(uint src, uint dest) {
            return dest ^ (1u << (int)(src & 31u));
        }

        void OpBRA(uint src) {
            PC += src;
        }

        void OpBRK() {
            var tmp0 = PSW;
            this.PSW_u = false;
            this.PSW_i = false;
            this.PSW_pm = false;
            var tmp1 = PC + 1;
            PC = bus.Read(INTB, 4);
            SP = SP - 4;
            bus.Write(SP, 4, tmp0);
            SP = SP - 4;
            bus.Write(SP, 4, tmp1);
        }

        byte OpBSET_m(uint src, byte dest) {
            return (byte)(dest | (1u << (int)(src & 7u)));
        }

        uint OpBSET_r(uint src, uint dest) {
            return dest | (1u << (int)(src & 31u));
        }

        void OpBSR(uint src, uint n) {
            SP -= 4;
            bus.Write(SP, 4, PC + n);
            PC += src;
        }

        void OpBTST_m(uint src, byte src2) {
            PSW_z = (src2 >> (int)(src & 7u) & 1u) == 0u;
            PSW_c = (src2 >> (int)(src & 7u) & 1u) != 0u;
        }

        void OpBTST_r(uint src, uint src2) {
            PSW_z = (src2 >> (int)(src & 31u) & 1u) == 0u;
            PSW_c = (src2 >> (int)(src & 31u) & 1u) != 0u;
        }

        void OpCLRPSW(uint dest) {
            SetPSWFlag(dest, false);
        }

        uint OpDIV(uint src, uint dest) {
            if (src == 0 || (dest == 0x80000000 && src == ~0u)) {
                PSW_o = true;
                return dest;
            }
            PSW_o = false;
            return (uint)((int)dest / (int)src);
        }

        uint OpDIVU(uint src, uint dest) {
            var result = src == 0 ? dest : dest / src;
            PSW_o = src == 0;
            return result;
        }

        void OpEMUL(uint src, uint destRegNo) {
            long a = (int)src;
            long b = (int)Registers[destRegNo];
            ulong result = (ulong)(b * a);

            Acc = result;
            Registers[destRegNo] = unchecked((uint)result);
            Registers[destRegNo + 1] = unchecked((uint)(result >> 32));
        }

        void OpEMULU(uint src, uint destRegNo) {
            ulong a = src;
            ulong b = Registers[destRegNo];
            ulong result = (ulong)(b * a);

            Acc = result;
            Registers[destRegNo] = unchecked((uint)result);
            Registers[destRegNo + 1] = unchecked((uint)(result >> 32));
        }

        uint OpFADD(uint src, uint dest) {
            var a = BitConverter.UInt32BitsToSingle(src);
            var b = BitConverter.UInt32BitsToSingle(dest);
            var result = a + b;

            var converted = BitConverter.SingleToUInt32Bits(result);
            PSW_z = IsFZero(converted);
            PSW_s = IsNegativeValue(converted);

            FPSW_cz = false;
            return converted;
        }

        void OpFCMP(uint src, uint src2) {
            var a = BitConverter.UInt32BitsToSingle(src);
            var b = BitConverter.UInt32BitsToSingle(src2);

            PSW_z = b == a;
            PSW_s = b < a;

            FPSW_co = false;
            FPSW_cz = false;
            FPSW_cu = false;
            FPSW_cx = false;
        }

        uint OpFDIV(uint src, uint dest) {
            var a = BitConverter.UInt32BitsToSingle(src);
            var b = BitConverter.UInt32BitsToSingle(dest);
            var result = b / a;
            var converted = BitConverter.SingleToUInt32Bits(result);

            PSW_z = IsFZero(converted);
            PSW_s = IsNegativeValue(converted);
            return converted;
        }

        uint OpFMUL(uint src, uint dest) {
            var a = BitConverter.UInt32BitsToSingle(src);
            var b = BitConverter.UInt32BitsToSingle(dest);
            var result = b * a;
            var converted = BitConverter.SingleToUInt32Bits(result);

            PSW_z = IsFZero(converted);
            PSW_s = IsNegativeValue(converted);
            return converted;
        }

        uint OpFSUB(uint src, uint dest) {
            var a = BitConverter.UInt32BitsToSingle(src);
            var b = BitConverter.UInt32BitsToSingle(dest);
            var result = b - a;

            var converted = BitConverter.SingleToUInt32Bits(result);

            PSW_z = IsFZero(converted);
            PSW_s = IsNegativeValue(converted);

            FPSW_cz = false;
            return converted;
        }

        uint OpFTOI(uint src) {
            float a = BitConverter.UInt32BitsToSingle(src);
            var result = (uint)(int)a;

            PSW_z = IsFZero(result);
            PSW_s = IsNegativeValue(result);

            FPSW_co = false;
            FPSW_cz = false;
            FPSW_cu = false;
            return result;
        }

        void OpINT(uint src) {
            var tmp0 = PSW;
            this.PSW_u = false;
            this.PSW_i = false;
            this.PSW_pm = false;
            var tmp1 = PC + 3;
            PC = bus.Read(INTB + src * 4, 4);
            SP = SP - 4;
            bus.Write(SP, 4, tmp0);
            SP = SP - 4;
            bus.Write(SP, 4, tmp1);
        }

        uint OpITOF(uint src) {
            float result = (int)src;
            var converted = BitConverter.SingleToUInt32Bits(result);

            PSW_z = IsFZero(converted);
            PSW_s = IsNegativeValue(converted);

            FPSW_cv = false;
            FPSW_co = false;
            FPSW_cz = false;
            FPSW_cu = false;
            FPSW_ce = false;
            return converted;
        }

        void OpJMP(uint src) {
            PC = src;
        }

        void OpJSR(uint src) {
            SP -= 4;
            bus.Write(SP, 4, PC + 2);
            PC = src;
        }

        void OpMACHI(uint src, uint src2) {
            short tmp1 = (short)(src >> 16);
            short tmp2 = (short)(src2 >> 16);
            long tmp3 = (int)tmp1 * (int)tmp2;
            Acc += (ulong)(tmp3 << 16);
        }

        void OpMACLO(uint src, uint src2) {
            short tmp1 = (short)(src);
            short tmp2 = (short)(src2);
            long tmp3 = (int)tmp1 * (int)tmp2;
            Acc += (ulong)(tmp3 << 16);
        }

        uint OpMAX(uint src, uint dest) {
            int a = (int)src;
            int b = (int)dest;
            return (uint)Math.Max(a, b);
        }

        uint OpMIN(uint src, uint dest) {
            int a = (int)src;
            int b = (int)dest;
            return (uint)Math.Min(a, b);
        }

        uint OpMUL(uint src, uint src2) {
            uint result = src * src2;
            return result;
        }

        void OpMULHI(uint src, uint src2) {
            short tmp1 = (short)(src >> 16);
            short tmp2 = (short)(src2 >> 16);
            long tmp3 = (int)tmp1 * (int)tmp2;
            Acc = (ulong)(tmp3 << 16);
        }

        void OpMULLO(uint src, uint src2) {
            short tmp1 = (short)(src);
            short tmp2 = (short)(src2);
            long tmp3 = (int)tmp1 * (int)tmp2;
            Acc = (ulong)(tmp3 << 16);
        }

        uint OpMVFACHI() {
            return (uint)(Acc >> 32);
        }

        uint OpMVFACMI() {
            return (uint)(Acc >> 16);
        }

        uint OpMVFC(uint src) {
            return GetControlRegister(src);
        }

        void OpMVTACHI(uint src) {
            Acc = (Acc & 0x00000000FFFFFFFF) | ((ulong)src << 32);
        }

        void OpMVTACLO(uint src) {
            Acc = (Acc & 0xFFFFFFFF00000000) | src;
        }

        void OpMVTC(uint src, uint dest) {
            SetControlRegister(dest, src);
        }

        void OpMVTIPL(uint src) {
            if (PSW_pm) {
                // TODO ユーザーモードのため特権命令例外を発生させる
            }
            PSW_ipl = (byte)src;
        }

        uint OpNEG(uint src) {
            int result = -(int)src;
            SubFlags(0, src, (uint)result);
            return (uint)result;
        }

        uint OpNOT(uint src) {
            var result = ~src;
            NotFlags(result);
            return result;
        }

        uint OpOR(uint src, uint src2) {
            var result = src2 | src;
            OrFlags(result);
            return result;
        }

        uint OpPOP() {
            var tmp = bus.Read(SP, 4);
            SP += 4;
            return tmp;
        }

        void OpPOPC(uint dest) {
            var tmp = bus.Read(SP, 4);
            SP += 4;
            SetControlRegister(dest, tmp);
        }

        void OpPOPM(uint dest, uint dest2) {
            // TODO dest に 0が入った場合を考慮する
            for (int i = (int)dest;i <= dest2; i++) {
                uint tmp = bus.Read(SP, 4);
                SP += 4;
                Registers[i] = tmp;
            }
        }

        void OpPUSH(uint size, uint src) {
            var length = 1 << (int)size;
            SP -= 4;
            bus.Write(SP, length, src);
        }

        void OpPUSHC(uint src) {
            uint tmp = GetControlRegister(src);
            SP -= 4;
            bus.Write(SP, 4, tmp);
        }

        void OpPUSHM(uint src, uint src2) {
            // TODO src に 0が入った場合を考慮する
            for (int i = (int)src2; i >= src; i--) {
                uint tmp = Registers[i];
                SP -= 4;
                bus.Write(SP, 4, tmp);
            }
        }

        void OpRACW(uint src) {
            long tmp = (long)Acc << (int)(src + 1);
            tmp += 0x0000000080000000;
            if (tmp > (long)0x00007FFF00000000) {
                Acc = 0x00007FFF00000000;
            }
            else if (tmp < unchecked((long)0xFFFF800000000000)) {
                Acc = 0xFFFF800000000000;
            }
            else {
                Acc = (ulong)tmp & 0xFFFFFFFF00000000;
            }
        }

        uint OpREVL(uint src) {
            return
                (src << 24) |
                ((src & (0xFF << 8)) << 8) |
                ((src & (0xFFu << 16)) >> 8) |
                (src >> 24);
        }

        uint OpREVW(uint src) {
            return
                ((src & (0xFF << 16)) << 8) |
                ((src & (0xFFu << 24)) >> 8) |
                ((src & 0xFF) << 8) |
                ((src & (0xFF << 8)) >> 8);
        }

        void OpRMPA(uint size) {
            if (Registers[3] == 0) {
                return;
            }
            var n = 1 << (int)size;
            ulong resultL =
                Registers[5] << 32 |
                Registers[4];
            uint resultH = Registers[6];
            PSW_o = false;

            while (Registers[3] != 0) {
                long tmp0 = bus.Read(Registers[1], n);
                long tmp1 = bus.Read(Registers[2], n);
                long tmp3 = tmp0 * tmp1;
                ulong prev = resultL;
                resultL += (ulong)tmp3;
                // carry / bollow
                if (tmp3 < 0) {
                    if (prev > resultL) {
                        resultH--;
                    }
                } else {
                    if (prev < resultL) {
                        resultH++;
                    }
                }

                Registers[1] += (uint)n;
                Registers[2] += (uint)n;
                Registers[3]--;
            }
            PSW_s = (resultH >> 31) != 0;
            PSW_o = resultH != 0 && resultH != unchecked((uint)-1);
            Registers[6] = resultH;
            Registers[5] = (uint)(resultL >> 32);
            Registers[4] = (uint)(resultL & 0xffffffff);
        }

        uint OpROLC(uint dest) {
            var result = dest << 1;
            if (!PSW_c) {
                result &= 0xFFFFFFFE;
            }
            else {
                result |= 0x00000001;
            }
            PSW_c = (dest >> 31) != 0;
            PSW_z = result == 0;
            PSW_s = IsNegativeValue(result);
            return result;
        }

        uint OpRORC(uint dest) {
            var result = dest >> 1;
            if (!PSW_c) {
                result &= 0x7FFFFFFF;
            }
            else {
                result |= 0x80000000;
            }
            PSW_c = (dest & 1) != 0;
            PSW_z = result == 0;
            PSW_s = IsNegativeValue(result);
            return result;
        }

        uint OpROTL(uint src, uint dest) {
            int tmp0 = (int)(src & 31);
            uint tmp1 = dest << tmp0;
            var result = (dest >> (32 - tmp0)) | tmp1;
            PSW_c = (result & 1) != 0;
            PSW_z = result == 0;
            PSW_s = IsNegativeValue(result);
            return result;
        }

        uint OpROTR(uint src, uint dest) {
            uint tmp0 = src & 31;
            uint tmp1 = dest >> (int)tmp0;
            var result = ((dest << (int)(32 - tmp0)) | tmp1);
            PSW_c = (result & 0x8000_0000) != 0;
            PSW_z = result == 0;
            PSW_s = IsNegativeValue(result);
            return result;
        }

        uint OpROUND(uint src) {
            float a = BitConverter.UInt32BitsToSingle(src);
            int round = (int)a;
            var converted = (uint)round;

            PSW_z = converted == 0;
            PSW_s = IsNegativeValue(converted);

            FPSW_co = false;
            FPSW_cz = false;
            FPSW_cu = false;
            return converted;
        }

        void OpRTE() {
            if (PSW_pm) {
                RunException(EXTB, (byte)RXCoreExceptions.PrivilegedInstructionException, false);
                return;
            }
            PC = bus.Read(SP, 4);
            SP += 4;
            var tmp = bus.Read(SP, 4);
            SP += 4;
            PSW = tmp;
            if (PSW_pm) {
                PSW_u = true;
            }
        }

        void OpRTFI() {
            if (PSW_pm) {
                RunException(EXTB, (byte)RXCoreExceptions.PrivilegedInstructionException, false);
                return;
            }
            PSW = BPSW;
            if (PSW_pm) {
                PSW_u = true;
            }
            PC = BPC;
        }

        void OpRTS() {
            PC = bus.Read(SP, 4);
            SP += 4;
        }

        void OpRTSD(uint src)
        {
            SP += src;
            PC = bus.Read(SP, 4);
            SP += 4;
        }

        void OpRTSD(uint src, uint dest, uint dest2) {
            SP += src - (dest2 - dest + 1) * 4;
            for (int i = (int)dest; i <= (int)dest2; i++) {
                var tmp = bus.Read(SP, 4);
                SP += 4;
                Registers[i] = tmp;
            }
            PC = bus.Read(SP, 4);
            SP += 4;
        }

        uint OpSAT(uint dest) {
            if (PSW_o && PSW_s) {
                return 0x7FFFFFFF;
            }
            else if (PSW_o && !PSW_s) {
                return 0x80000000;
            }
            return dest;
        }

        void OpSATR() {
            if (PSW_o && !PSW_s) {
                Registers[6] = 0x00000000;
                Registers[5] = 0x7fffffff;
                Registers[4] = 0xffffffff;
            }
            else if (PSW_o && PSW_s) {
                Registers[6] = 0xffffffff;
                Registers[5] = 0x80000000;
                Registers[4] = 0x00000000;
            }
        }

        uint OpSBB(uint src, uint dest) {
            uint result = dest - src - (PSW_c ? 0u : 1u);
            SubFlags(dest, src, result);
            return result;
        }

        uint OpSCCnd(uint cnd) {
            if (CheckCondition(cnd)) {
                return 1;
            }
            return 0;
        }

        bool OpSCMPU() {
            if (Registers[3] != 0) {
                byte tmp0 = (byte)bus.Read(Registers[1]++, 1);
                byte tmp1 = (byte)bus.Read(Registers[2]++, 1);
                Registers[3]--;
                PSW_c = tmp0 - tmp1 >= 0;
                PSW_z = tmp0 == tmp1;
                if (tmp0 != tmp1 || tmp0 == 0) {
                    return true;
                }
                return Registers[3] == 0;
            }
            return true;
        }

        void OpSETPSW(uint dest) {
            SetPSWFlag(dest, true);
        }

        uint OpSHAR(uint src, uint src2) {
            int shift = (int)(src & 31);
            var result = (int)src2 >> shift;
            PSW_c = shift != 0 && (src2 & (1u << (shift - 1))) != 0;
            PSW_z = result == 0;
            PSW_s = IsNegativeValue((uint)result);
            PSW_o = false;
            return (uint)result;
        }

        uint OpSHLL(uint src, uint src2) {
            int shift = (int)(src & 31);
            uint result = src2 << shift;
            PSW_c = shift != 0 && (src2 & (1 << (32 - shift))) != 0;
            PSW_z = result == 0;
            PSW_s = IsNegativeValue(result);
            // 算術右シフトする
            PSW_o = shift != 0 && ((int)result >> 31) != ((int)src2 >> (32 - shift));
            return result;
        }

        uint OpSHLR(uint src, uint src2) {
            int shift = (int)(src & 31);
            var result = src2 >> shift;
            PSW_c = shift != 0 && (src2 & (1u << (shift - 1))) != 0;
            PSW_z = result == 0;
            PSW_s = IsNegativeValue(result);
            return result;
        }

        bool OpSMOVB() {
            if (Registers[3] != 0) {
                uint tmp = bus.Read(Registers[2], 1);
                bus.Write(Registers[1], 1, tmp);
                Registers[1]--;
                Registers[2]--;
                Registers[3]--;
                return Registers[3] == 0;
            }
            return true;
        }

        bool OpSMOVF() {
            if (Registers[3] != 0) {
                uint tmp = bus.Read(Registers[2], 1);
                bus.Write(Registers[1], 1, tmp);
                Registers[1]++;
                Registers[2]++;
                Registers[3]--;
                return Registers[3] == 0;
            }
            return true;
        }

        bool OpSMOVU() {
            if (Registers[3] != 0) {
                uint tmp = bus.Read(Registers[2], 1);
                bus.Write(Registers[1], 1, tmp);
                Registers[1]++;
                Registers[2]++;
                Registers[3]--;
                if (tmp == 0) {
                    return true;
                }
                return Registers[3] == 0;
            }
            return true;
        }

        bool OpSSTR(uint size) {
            if (Registers[3] != 0) {
                uint byteLength = 1u << (int)size;
                bus.Write(Registers[1], (int)byteLength, Registers[2]);
                Registers[1] += byteLength;
                Registers[3]--;
                return Registers[3] == 0;
            }
            return true;
        }

        uint OpSTNZ(uint src, uint dest) {
            return !PSW_z ? src : dest;
        }

        uint OpSTZ(uint src, uint dest) {
            return PSW_z ? src : dest;
        }

        uint OpSUB(uint src, uint src2) {
            var result = src2 - src;
            SubFlags(src2, src, result);
            return result;
        }

        bool OpSUNTIL(uint size) {
            if (Registers[3] != 0) {
                var byteLength = 1u << (int)size;
                var tmp = bus.Read(Registers[1], (int)byteLength);
                Registers[1] += byteLength;
                Registers[3]--;
                PSW_c = tmp >=  Registers[2];
                PSW_z = tmp == Registers[2];
                if (tmp == Registers[2]) {
                    return true;
                }
                return Registers[3] == 0;
            }
            return true;
        }

        bool OpSWHILE(uint size) {
            if (Registers[3] != 0) {
                var byteLength = 1u << (int)size;
                var tmp = bus.Read(Registers[1], (int)byteLength);
                Registers[1] += byteLength;
                Registers[3]--;
                PSW_c = tmp >= Registers[2];
                PSW_z = tmp == Registers[2];
                if (tmp != Registers[2]) {
                    return true;
                }
                return Registers[3] == 0;
            }
            return true;
        }

        void OpTST(uint src, uint src2) {
            var result = src2 & src;
            AndFlags(result);
        }

        void OpWAIT() {
            Waiting = true;
        }

        (uint src, uint dest) OpXCHG(uint src, uint dest) {
            return (dest, src);
        }

        uint OpXOR(uint src, uint dest) {
            var result = dest ^ src;
            XorFlags(result);
            return result;
        }

        public void ExecuteInstruction(OpCode opCode, ReadOnlySpan<uint> operand, uint opSize) {
            var shouldIncrementPC = true;
            switch (opCode) {
                case OpCode.ABS_rd: {
                    ref var rd = ref Registers[operand[0]];
                    rd = OpABS(rd);
                }
                    break;
                case OpCode.ABS_rs_rd: {
                    Registers[operand[1]] = OpABS(Registers[operand[0]]);
                    break;
                }
                case OpCode.ADC_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpADC(operand[2], dest);
                    break;
                }
                case OpCode.ADC_rr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpADC(Registers[operand[0]], dest);
                    break;
                }
                case OpCode.ADC_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    ref var dest = ref Registers[operand[2]];
                    dest = OpADC(src, dest);
                    break;
                }
                case OpCode.ADD_4irr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpADD(operand[0], dest);
                    break;
                }
                case OpCode.ADD_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    ref var dest = ref Registers[operand[1]];
                    dest = OpADD(src, dest);
                    break;
                }
                case OpCode.ADD_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    ref var dest = ref Registers[operand[2]];
                    dest = OpADD(src, dest);
                    break;
                }
                case OpCode.ADD_irrr: {
                    Registers[operand[1]] = OpADD(operand[3], Registers[operand[0]]);
                    break;
                }
                case OpCode.ADD_rrr: {
                    Registers[operand[0]] = OpADD(Registers[operand[1]], Registers[operand[2]]);
                    break;
                }
                case OpCode.AND_4ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpAND(operand[0], dest);
                    break;
                }
                case OpCode.AND_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpAND(operand[2], dest);
                    break;
                }
                case OpCode.AND_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    ref var dest = ref Registers[operand[1]];
                    dest = OpAND(src, dest);
                    break;
                }
                case OpCode.AND_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    ref var dest = ref Registers[operand[2]];
                    dest = OpAND(src, dest);
                    break;
                }
                case OpCode.AND_rrr: {
                    Registers[operand[0]] = OpAND(Registers[operand[1]], Registers[operand[2]]);
                    break;
                }
                case OpCode.BCLR_im:
                    StoreDestOperand((uint)MemEx.B, operand[0], operand[2], operand.Slice(3),
                                     OpBCLR_m(operand[1], (byte)LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3))));
                    break;
                case OpCode.BCLR_rm:
                    StoreDestOperand((uint)MemEx.B, operand[0], operand[2], operand.Slice(3),
                                     OpBCLR_m(Registers[operand[1]], (byte)LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3))));
                    break;
                case OpCode.BCLR_ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpBCLR_r(operand[0], dest);
                    break;
                }
                case OpCode.BCLR_rr: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpBCLR_r(Registers[operand[1]], dest);
                    break;
                }
                case OpCode.BCnd_s: {
                    var src = operand[1] < 3 ? operand[1] + 8 : operand[1];
                    OpBCnd(operand[0], src, opSize);
                    shouldIncrementPC = false;
                    break;
                }
                case OpCode.BCnd_b:
                case OpCode.BCnd_w:
                    OpBCnd(operand[0], operand[1], opSize);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BRA_s: {
                    var src = operand[0] < 3 ? operand[0] + 8 : operand[0];
                    OpBRA(src);
                    shouldIncrementPC = false;
                    break;
                }
                case OpCode.BRA_b:
                    OpBRA(operand[0]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BRA_w:
                    OpBRA(operand[0]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BRA_a:
                    OpBRA(operand[0]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BRA_l:
                    OpBRA(Registers[operand[0]]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BMCnd_im:
                    StoreDestOperand((uint)MemEx.B, operand[1], operand[3], operand.Slice(4), OpBMCnd_m(operand[2], operand[0], LoadSourceOperand(operand[3], 4, operand[1], operand.Slice(4))));
                    break;
                case OpCode.BMCnd_ir: {
                    ref var dest = ref Registers[operand[2]];
                    dest = OpBMCnd_r(operand[1], operand[0], dest);
                    break;
                }
                case OpCode.BNOT_im:
                    StoreDestOperand((uint)MemEx.B, operand[1], operand[2], operand.Slice(3),
                                     OpBNOT_m(operand[0], (byte)LoadSourceOperand(operand[2], 4, operand[1], operand.Slice(3))));
                    break;
                case OpCode.BNOT_rm: {
                    StoreDestOperand(4, operand[0], operand[2], operand.Slice(3),
                                     OpBNOT_r(Registers[operand[1]], (byte)LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3))));
                    break;
                }
                case OpCode.BNOT_ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpBNOT_r(operand[0], dest);
                    break;
                }
                case OpCode.BNOT_rr: {
                    ref var dest = ref Registers[operand[0]];
                    var src = Registers[operand[1]];
                    dest = OpBNOT_r(src, dest);
                    break;
                }
                case OpCode.BRK:
                    OpBRK();
                    break;
                case OpCode.BSET_im:
                    StoreDestOperand(0, operand[0], operand[2], operand.Slice(3),
                                     OpBSET_m(operand[1], (byte)LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3))));
                    break;
                case OpCode.BSET_rm:
                    StoreDestOperand(0, operand[0], operand[2], operand.Slice(3),
                                     OpBSET_m(Registers[operand[1]], (byte)LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3))));

                    break;
                case OpCode.BSET_ir: {
                    ref var dest = ref Registers[operand[1]];
                    var src = operand[0];
                    dest = OpBSET_r(src, dest);
                    break;
                }
                case OpCode.BSET_rr: {
                    ref var dest = ref Registers[operand[0]];
                    var src = Registers[operand[1]];
                    dest = OpBSET_r(src, dest);
                    break;
                }
                case OpCode.BSR_w:
                    OpBSR(operand[0], 3);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BSR_a:
                    OpBSR(operand[0], 4);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BSR_l:
                    OpBSR(Registers[operand[0]], 2);
                    shouldIncrementPC = false;
                    break;
                case OpCode.BTST_im:
                    OpBTST_m(operand[1], (byte)LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3)));
                    break;
                case OpCode.BTST_rm:
                    OpBTST_m(Registers[operand[1]], (byte)LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3)));
                    break;
                case OpCode.BTST_ir:
                    OpBTST_r(operand[0], Registers[operand[1]]);
                    break;
                case OpCode.BTST_rr:
                    OpBTST_r(Registers[operand[1]], Registers[operand[0]]);
                    break;
                case OpCode.CLRPSW:
                    OpCLRPSW(operand[0]);
                    break;
                case OpCode.CMP_4ir:
                    OpSUB(operand[0], Registers[operand[1]]);
                    break;
                case OpCode.CMP_8ir:
                    OpSUB(operand[1], Registers[operand[0]]);
                    break;
                case OpCode.CMP_ir:
                    OpSUB(operand[2], Registers[operand[0]]);
                    break;
                case OpCode.CMP_ub_rs_mr: {
                    var dest = Registers[operand[1]];
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    OpSUB(src, dest);
                    break;
                }
                case OpCode.CMP_mr: {
                    var dest = Registers[operand[2]];
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    OpSUB(src, dest);
                    break;
                }
                case OpCode.DIV_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpDIV(operand[2], dest);
                    break;
                }
                case OpCode.DIV_ub_rs_mr: {
                    ref var dest = ref Registers[operand[1]];
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    dest = OpDIV(src, dest);
                    break;
                }
                case OpCode.DIV_mr: {
                    ref var dest = ref Registers[operand[2]];
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    dest = OpDIV(src, dest);
                    break;
                }
                case OpCode.DIVU_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpDIVU(operand[2], dest);
                    break;
                }
                case OpCode.DIVU_ub_rs_mr: {
                    ref var dest = ref Registers[operand[1]];
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    dest = OpDIVU(src, dest);
                    break;
                }
                case OpCode.DIVU_mr: {
                    ref var dest = ref Registers[operand[2]];
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    dest = OpDIVU(src, dest);
                    break;
                }
                case OpCode.EMUL_ir:
                    OpEMUL(operand[2], operand[0]);
                    break;
                case OpCode.EMUL_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    OpEMUL(src, operand[1]);
                    break;
                }
                case OpCode.EMUL_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    OpEMUL(src, operand[2]);
                    break;
                }
                case OpCode.EMULU_ir:
                    OpEMULU(operand[2], operand[0]);
                    break;
                case OpCode.EMULU_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    OpEMULU(src, operand[1]);
                    break;
                }
                case OpCode.EMULU_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    OpEMULU(src, operand[2]);
                    break;
                }
                case OpCode.FADD_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpFADD(operand[1], dest);
                    break;
                }
                case OpCode.FADD_mr: {
                    var src = LoadUnsignedSourceOperand(operand[2], 2, operand.Slice(3), operand[0]);
                    ref var dest = ref Registers[operand[1]];
                    dest = OpFADD(src, dest);
                    break;
                }
                case OpCode.FCMP_ir:
                    OpFCMP(operand[1], Registers[operand[0]]);
                    break;
                case OpCode.FCMP_mr: {
                    var src = LoadUnsignedSourceOperand(operand[2], 2, operand.Slice(3), operand[0]);
                    OpFCMP(src, Registers[operand[1]]);
                    break;
                }
                case OpCode.FDIV_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpFDIV(operand[1], dest);
                    break;
                }
                case OpCode.FDIV_mr: {
                    var src = LoadUnsignedSourceOperand(operand[2], 2, operand.Slice(3), operand[0]);
                    ref var dest = ref Registers[operand[1]];
                    dest = OpFDIV(src, dest);
                    break;
                }
                case OpCode.FMUL_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpFMUL(operand[1], dest);
                    break;
                }
                case OpCode.FMUL_mr: {
                    var src = LoadUnsignedSourceOperand(operand[2], 2, operand.Slice(3), operand[0]);
                    ref var dest = ref Registers[operand[1]];
                    dest = OpFMUL(src, dest);
                    break;
                }
                case OpCode.FSUB_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpFSUB(operand[1], dest);
                    break;
                }
                case OpCode.FSUB_mr: {
                    var src = LoadUnsignedSourceOperand(operand[2], 2, operand.Slice(3), operand[0]);
                    ref var dest = ref Registers[operand[1]];
                    dest = OpFSUB(src, dest);
                    break;
                }
                case OpCode.FTOI: {
                    var src = LoadUnsignedSourceOperand(operand[2], 2, operand.Slice(3), operand[0]);
                    Registers[operand[1]] = OpFTOI(src);
                    break;
                }
                case OpCode.INT:
                    OpINT(operand[0]);
                    break;
                case OpCode.ITOF_ub_rs: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    Registers[operand[1]] = OpITOF(src);
                    break;
                }
                case OpCode.ITOF_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    Registers[operand[2]] = OpITOF(src);
                    break;
                }
                case OpCode.JMP:
                    OpJMP(Registers[operand[0]]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.JSR:
                    OpJSR(Registers[operand[0]]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.MACHI:
                    OpMACHI(Registers[operand[0]], Registers[operand[1]]);
                    break;
                case OpCode.MACLO:
                    OpMACLO(Registers[operand[0]], Registers[operand[1]]);
                    break;
                case OpCode.MAX_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpMAX(operand[2], dest);
                    break;
                }
                case OpCode.MAX_ub_rs_mr: {
                    ref var dest = ref Registers[operand[1]];
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    dest = OpMAX(src, dest);
                    break;
                }
                case OpCode.MAX_mr: {
                    ref var dest = ref Registers[operand[2]];
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    dest = OpMAX(src, dest);
                    break;
                }
                case OpCode.MIN_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpMIN(operand[2], dest);
                    break;
                }
                case OpCode.MIN_ub_rs_mr: {
                    ref var dest = ref Registers[operand[1]];
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    dest = OpMIN(src, dest);
                    break;
                }
                case OpCode.MIN_mr: {
                    ref var dest = ref Registers[operand[2]];
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    dest = OpMIN(src, dest);
                    break;
                }
                case OpCode.MOV_rm:
                    StoreDestOperand(operand[0], operand[1], operand[2], Registers[operand[3]]);
                    break;
                case OpCode.MOV_mr:
                    Registers[operand[3]] = LoadSourceOperand(operand[0], operand[1], operand[2]);
                    break;
                case OpCode.MOV_4ir:
                    Registers[operand[1]] = operand[0];
                    break;
                case OpCode.MOV_im:
                    StoreDestOperand(operand[0], operand[1], operand[2], operand[3]);
                    break;
                case OpCode.MOV_u8ir:
                    Registers[operand[0]] = operand[1];
                    break;
                case OpCode.MOV_ir:
                    Registers[operand[0]] = operand[2];
                    break;
                case OpCode.MOV_rr:
                    Registers[operand[2]] = SignExtension(true, BitOperation.GetLowerBits(Registers[operand[1]], 1u << (int)operand[0]), (int)operand[0]);
                    break;
                case OpCode.MOV_im_p:
                    StoreDestOperand(operand[1], operand[0], operand[2], default, operand[4]);
                    break;
                case OpCode.MOV_im_dsp8:
                case OpCode.MOV_im_dsp16:
                    StoreDestOperand(operand[1], operand[0], operand[2], operand.Slice(3), operand[5]);
                    break;
                case OpCode.MOV_l_mr:
                    Registers[operand[2]] = SignExtension(
                        true,
                        LoadUnsignedSourceOperand(operand[3], operand[0], operand.Slice(4), operand[1]),
                        (int)operand[0]);
                    break;
                case OpCode.MOV_ar: {
                    if (operand[0] == 3) {
                        // 非対応のサイズ
                        return;
                    }
                    var sz = (int)operand[0];
                    var ri = Registers[operand[1]];
                    var rb = Registers[operand[2]];
                    var addr = rb + (ri << sz);
                    Registers[operand[3]] = SignExtension(true, bus.Read(addr, 1 << sz), sz);
                    break;
                }
                case OpCode.MOV_r_dsp:
                    StoreDestOperand(operand[0], operand[1], operand[3], operand.Slice(4), Registers[operand[2]]);
                    break;
                case OpCode.MOV_ra: {
                    if (operand[0] == 3) {
                        // 非対応のサイズ
                        return;
                    }
                    var sz = (int)operand[0];
                    var ri = Registers[operand[1]];
                    var rb = Registers[operand[2]];
                    var addr = rb + ( ri << sz);
                    bus.Write(addr, 1 << sz, Registers[operand[3]]);
                    break;
                }
                case OpCode.MOV_mm:
                    StoreDestOperand(operand[0], operand[2], operand[5], operand.Slice(6, 1),
                                    LoadUnsignedSourceOperand(operand[3], operand[0], operand.Slice(4, 1), operand[1]));
                    break;
                case OpCode.MOV_rp: {
                    if (operand[1] == 3) {
                        // 非対応のサイズ
                        break;
                    }
                    ref var dest = ref Registers[operand[2]];
                    var sz = 1u << (int)operand[1];
                    var ad = operand[0];
                    var addr = dest;
                    if (ad == 0) {
                        // post increment
                        dest = addr + sz;
                    }
                    else if (ad == 1) {
                        // pre decrement
                        addr -= sz;
                        dest = addr;
                    }
                    bus.Write(addr, (int)sz, Registers[operand[3]]);
                    break;
                }
                case OpCode.MOV_pr: {
                    if (operand[1] == 3) {
                        // 非対応のサイズ
                        break;
                    }
                    ref var src = ref Registers[operand[2]];
                    var sz = 1u << (int)operand[1];
                    var ad = operand[0];
                    var addr = src;
                    // post increment
                    if (ad == 2) {
                        src = addr + sz;
                    }
                    else if (ad == 3) {
                        // pre decrement
                        addr -= sz;
                        src = addr;
                    }
                    Registers[operand[3]] = SignExtension(true, bus.Read(addr, (int)sz), (int)operand[1]);
                    break;
                }
                case OpCode.MOVU_dsp5_mr:
                    Registers[operand[3]] = LoadUnsignedSourceOperand(operand[0], operand[1], operand[2]);
                    break;
                case OpCode.MOVU_mr:
                    Registers[operand[2]] = LoadUnsignedSourceOperand(operand[3], operand[0], operand.Slice(4), operand[1]);
                    break;
                case OpCode.MOVU_ar: {
                    var sz = (int)operand[0];
                    var ri = Registers[operand[1]];
                    var rb = Registers[operand[2]];
                    var addr = rb + (ri << sz);
                    Registers[operand[3]] = bus.Read(addr, 1 << sz);
                    break;
                }
                case OpCode.MOVU_pr: {
                    ref var src = ref Registers[operand[2]];
                    var sz = 1u << (int)operand[1];
                    var ad = operand[0];
                    var addr = src;
                    // post increment
                    if (ad == 2) {
                        src = addr + sz;
                    }
                    else if (ad == 3) {
                        // pre decrement
                        addr -= sz;
                        src = addr;
                    }
                    Registers[operand[3]] = bus.Read(addr, (int)sz);
                    break;
                }
                case OpCode.MUL_4ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpMUL(operand[0], dest);
                    break;
                }
                case OpCode.MUL_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpMUL(operand[2], dest);
                    break;
                }
                case OpCode.MUL_ub_rs_mr: {
                    ref var dest = ref Registers[operand[1]];
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    dest = OpMUL(src, dest);
                    break;
                }
                case OpCode.MUL_mr: {
                    ref var dest = ref Registers[operand[2]];
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    dest = OpMUL(src, dest);
                    break;
                }
                case OpCode.MUL_rrr:
                    Registers[operand[0]] = OpMUL(Registers[operand[1]], Registers[operand[2]]);
                    break;
                case OpCode.MULHI:
                    OpMULHI(Registers[operand[0]], Registers[operand[1]]);
                    break;
                case OpCode.MULLO:
                    OpMULLO(Registers[operand[0]], Registers[operand[1]]);
                    break;
                case OpCode.MVFACHI:
                    Registers[operand[0]] = OpMVFACHI();
                    break;
                case OpCode.MVFACMI:
                    Registers[operand[0]] = OpMVFACMI();
                    break;
                case OpCode.MVFC:
                    Registers[operand[1]] = OpMVFC(operand[0]);
                    break;
                case OpCode.MVTACHI:
                    OpMVTACHI(Registers[operand[0]]);
                    break;
                case OpCode.MVTACLO:
                    OpMVTACLO(Registers[operand[0]]);
                    break;
                case OpCode.MVTC_i:
                    OpMVTC(operand[2], operand[0]);
                    break;
                case OpCode.MVTC_r:
                    OpMVTC(Registers[operand[0]], operand[1]);
                    break;
                case OpCode.MVTIPL:
                    OpMVTIPL(operand[0]);
                    break;
                case OpCode.NEG_rd: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpNEG(dest);
                    break;
                }
                case OpCode.NEG_rs_rd:
                    Registers[operand[1]] = OpNEG(Registers[operand[0]]);
                    break;
                case OpCode.NOP:
                    // do nothing.
                    break;
                case OpCode.NOT_rd: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpNOT(dest);
                    break;
                }
                case OpCode.NOT_rs_rd:
                    Registers[operand[1]] = OpNOT(Registers[operand[0]]);
                    break;
                case OpCode.OR_4ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpOR(operand[0], dest);
                    break;
                }
                case OpCode.OR_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpOR(operand[2], dest);
                    break;
                }
                case OpCode.OR_ub_rs_mr: {
                    ref var dest = ref Registers[operand[1]];
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    dest = OpOR(src, dest);
                    break;
                }
                case OpCode.OR_mr: {
                    ref var dest = ref Registers[operand[2]];
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    dest = OpOR(src, dest);
                    break;
                }
                case OpCode.OR_rrr:
                    Registers[operand[0]] = OpOR(Registers[operand[1]], Registers[operand[2]]);
                    break;
                case OpCode.POP:
                    Registers[operand[0]] = OpPOP();
                    break;
                case OpCode.POPC:
                    OpPOPC(operand[0]);
                    break;
                case OpCode.POPM:
                    OpPOPM(operand[0], operand[1]);
                    break;
                case OpCode.PUSH_r:
                    OpPUSH(operand[0], Registers[operand[1]]);
                    break;
                case OpCode.PUSH_m: {
                    var src = LoadSourceOperand(operand[2], operand[1], operand[0], operand.Slice(3));
                    OpPUSH(operand[1], src);
                    break;
                }
                case OpCode.PUSHC:
                    OpPUSHC(operand[0]);
                    break;
                case OpCode.PUSHM:
                    OpPUSHM(operand[0], operand[1]);
                    break;
                case OpCode.RACW:
                    OpRACW(operand[0]);
                    break;
                case OpCode.REVL:
                    Registers[operand[1]] = OpREVL(Registers[operand[0]]);
                    break;
                case OpCode.REVW:
                    Registers[operand[1]] = OpREVW(Registers[operand[0]]);
                    break;
                case OpCode.SMOVF:
                    shouldIncrementPC = OpSMOVF();
                    break;
                case OpCode.RMPA:
                    OpRMPA(operand[0]);
                    break;
                case OpCode.ROLC: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpROLC(dest);
                    break;
                }
                case OpCode.RORC: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpRORC(dest);
                    break;
                }
                case OpCode.ROTL_ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpROTL(operand[0], dest);
                    break;
                }
                case OpCode.ROTL_rr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpROTL(Registers[operand[0]], dest);
                    break;
                }
                case OpCode.ROTR_ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpROTR(operand[0], dest);
                    break;
                }
                case OpCode.ROTR_rr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpROTR(Registers[operand[0]], dest);
                    break;
                }
                case OpCode.ROUND: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    ref var dest = ref Registers[operand[1]];
                    dest = OpROUND(src);
                    break;
                }
                case OpCode.RTE:
                    OpRTE();
                    shouldIncrementPC = false;
                    break;
                case OpCode.RTFI:
                    OpRTFI();
                    shouldIncrementPC = false;
                    break;
                case OpCode.RTS:
                    OpRTS();
                    shouldIncrementPC = false;
                    break;
                case OpCode.RTSD_i:
                    OpRTSD(operand[0]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.RTSD_irr:
                    OpRTSD(operand[2], operand[0], operand[1]);
                    shouldIncrementPC = false;
                    break;
                case OpCode.SAT: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpSAT(dest);
                    break;
                }
                case OpCode.SATR:
                    OpSATR();
                    break;
                case OpCode.SBB_rr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSBB(Registers[operand[0]], dest);
                    break;
                }
                case OpCode.SBB_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    ref var dest = ref Registers[operand[2]];
                    dest = OpSBB(src, dest);
                    break;
                }
                case OpCode.SCCnd:
                    StoreDestOperand(operand[0], operand[1], operand[3], operand.Slice(4), OpSCCnd(operand[2]));
                    break;
                case OpCode.SETPSW:
                    OpSETPSW(operand[0]);
                    break;
                case OpCode.SHAR_5irr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSHAR(operand[0], dest);
                    break;
                }
                case OpCode.SHAR_irr: {
                    Registers[operand[2]] = OpSHAR(operand[0], Registers[operand[1]]);
                    break;
                }
                case OpCode.SHAR_rr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSHAR(Registers[operand[0]], dest);
                    break;
                }
                case OpCode.SHLL_5irr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSHLL(operand[0], dest);
                    break;
                }
                case OpCode.SHLL_irr: {
                    Registers[operand[2]] = OpSHLL(operand[0], Registers[operand[1]]);
                    break;
                }
                case OpCode.SHLL_rr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSHLL(Registers[operand[0]], dest);
                    break;
                }
                case OpCode.SHLR_5irr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSHLR(operand[0], dest);
                    break;
                }
                case OpCode.SHLR_irr: {
                    Registers[operand[2]] = OpSHLR(operand[0], Registers[operand[1]]);
                    break;
                }
                case OpCode.SHLR_rr: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSHLR(Registers[operand[0]], dest);
                    break;
                }
                case OpCode.SMOVB:
                    shouldIncrementPC = OpSMOVB();
                    break;
                case OpCode.SSTR:
                    shouldIncrementPC = OpSSTR(operand[0]);
                    break;
                case OpCode.STNZ: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpSTNZ(operand[2], dest);
                    break;
                }
                case OpCode.STZ: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpSTZ(operand[2], dest);
                    break;
                }
                case OpCode.SUB_ir: {
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSUB(operand[0], dest);
                    break;
                }
                case OpCode.SUB_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    ref var dest = ref Registers[operand[1]];
                    dest = OpSUB(src, dest);
                    break;
                }
                case OpCode.SUB_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    ref var dest = ref Registers[operand[2]];
                    dest = OpSUB(src, dest);
                    break;
                }
                case OpCode.SUB_rrr:
                    Registers[operand[0]] = OpSUB(Registers[operand[1]], Registers[operand[2]]);
                    break;
                case OpCode.SCMPU:
                    shouldIncrementPC = OpSCMPU();
                    break;
                case OpCode.SUNTIL:
                    shouldIncrementPC = OpSUNTIL(operand[0]);
                    break;
                case OpCode.SMOVU:
                    shouldIncrementPC = OpSMOVU();
                    break;
                case OpCode.SWHILE:
                    shouldIncrementPC = OpSWHILE(operand[0]);
                    break;
                case OpCode.TST_ir:
                    OpTST(operand[2], Registers[operand[0]]);
                    break;
                case OpCode.TST_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    OpTST(src, Registers[operand[1]]);
                    break;
                }
                case OpCode.TST_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    OpTST(src, Registers[operand[2]]);
                    break;
                }
                case OpCode.WAIT:
                    OpWAIT();
                    break;
                case OpCode.XCHG_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    ref var dest = ref Registers[operand[1]];
                    var tmp = dest;
                    dest = src;
                    StoreDestOperand(4, operand[0], operand[2], operand.Slice(3), tmp);
                    break;
                }
                case OpCode.XCHG_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    ref var dest = ref Registers[operand[2]];
                    var tmp = dest;
                    dest = src;
                    StoreDestOperand(operand[0], operand[1], operand[3], operand.Slice(4), tmp);
                    break;
                }
                case OpCode.XOR_ir: {
                    ref var dest = ref Registers[operand[0]];
                    dest = OpXOR(operand[2], dest);
                    break;
                }
                case OpCode.XOR_ub_rs_mr: {
                    var src = LoadSourceOperand(operand[2], 4, operand[0], operand.Slice(3));
                    ref var dest = ref Registers[operand[1]];
                    dest = OpXOR(src, dest);
                    break;
                }
                case OpCode.XOR_mr: {
                    var src = LoadSourceOperand(operand[3], operand[0], operand[1], operand.Slice(4));
                    ref var dest = ref Registers[operand[2]];
                    dest = OpXOR(src, dest);
                    break;
                }
                default:
                    break;
            }
            if (shouldIncrementPC) {
                PC += opSize;
            }
        }
    }
}
