using CPU;
using GPIO;

namespace Peripheral.Renesas;

public enum IRQMD : byte {
    LO_LVL    = 0,
    N_EDGE    = 1 << 2,
    P_EDGE    = 2 << 2,
    BOTH_EDGE = 3 << 2
}

public class ICU {

    static readonly byte   IR_IR           = 1 << 0;
    static readonly byte   DTCER_DTCE      = 1 << 0;
    static readonly byte   SWINTR_SWINT    = 1 << 0;
    static readonly ushort FIR_FIEN        = 1 << 15;
    static readonly byte   IRQCR_IRQMD_MSK = 3 << 2;

    public class Irq {
        readonly ICU icu;
        public readonly int IrqNr;
        public readonly RegisterValue32<byte> IR;
        public readonly RegisterValue32<byte> DTCER;
        public readonly RegisterValue32<byte> IRQCR;
        public readonly IInputableSignal SignalIRQ;

        public Irq(ICU icu, string name, int irqNr, byte initialIRQCR) {
            this.icu = icu;
            IR = new RegisterValue32<byte>(0, onWrite: WriteIR);
            DTCER = new RegisterValue32<byte>(0, onWrite: WriteDTCER);
            IRQCR = new RegisterValue32<byte>(initialIRQCR, onWrite: WriteIRQCR);
            IrqNr = irqNr;
            SignalIRQ = new SignalBit($"{name}.irq{irqNr}", SigIrqTraceProc);
        }

        public void UpdateInterrupt() {
            icu.TryRemoveIRQ(IrqNr);

            bool ien = 0 != ((icu.IER[IrqNr >> 3].Value >> (IrqNr & 7)) & 1);
            short iprIndex = icu.irqToIpr[IrqNr];
            if ((iprIndex < 0) || (iprIndex >= icu.IPR.Count)) {
                throw new Exception($"No interrupt priority for irq {IrqNr}");
            }
            if ((IrqNr == (icu.FIR.Value & 0xff)) && (icu.FIR.Value & FIR_FIEN) != 0
                && (IR.Value & IR_IR) != 0 && ien) {
                icu.SetIRQ(ipr: 0xFF, irqNr: IrqNr);
            }
            else {
                byte ipl = (byte)(icu.IPR[iprIndex].Value & 0xf);
                if (ien && (IR.Value & IR_IR) != 0 && ipl != 0) {
                    // 割り込みがあった場合
                    icu.SetIRQ(ipr: ipl, irqNr: IrqNr);
                }
            }
            icu.PostInterrupt();
        }

        void WriteIRQCR(RegisterValue32<byte> reg, byte value) {
            reg.Value = value;
            UpdateLevelInterrupt();
        }

        public void UpdateLevelInterrupt() {
            switch((IRQMD)(IRQCR.Value & IRQCR_IRQMD_MSK)) {
                case IRQMD.LO_LVL:
                    IR.Value = SignalIRQ.InputSignal
                        ? (byte)0
                        : IR_IR;
                    UpdateInterrupt();
                    break;
            }
        }

        void WriteIR(RegisterValue32<byte> reg, byte value) {
            switch ((IRQMD)(IRQCR.Value & IRQCR_IRQMD_MSK)) {
                case IRQMD.LO_LVL:
                    break;
                case IRQMD.N_EDGE:
                case IRQMD.P_EDGE:
                case IRQMD.BOTH_EDGE:
                    reg.Value &= value;
                    UpdateInterrupt();
                    break;
            }
        }

        void WriteDTCER(RegisterValue32<byte> reg, byte value) {
            reg.Value = (byte)(value & DTCER_DTCE);
        }

        void SigIrqTraceProc(bool _, bool value) {
            byte old = IR.Value;
            int nextIR = (IRQMD)(IRQCR.Value & IRQCR_IRQMD_MSK) switch {
                IRQMD.LO_LVL => !value
                ? IR_IR
                : 0,
                IRQMD.N_EDGE => !value
                ? IR_IR
                : old,
                IRQMD.P_EDGE => value
                ? IR_IR
                : old,
                IRQMD.BOTH_EDGE => IR_IR,
                _ => old
            };
            bool isChanged = old != nextIR;
            IR.Value = (byte)nextIR;
            if (isChanged) {
                UpdateInterrupt();
            }
        }
    }

    record struct IrqNo(int IPL, int IRQNR);

    class IrqNoComparer : IComparer<IrqNo> {
        public static readonly IrqNoComparer Instance = new();
        public int Compare(IrqNo x, IrqNo y) {
            return x.IPL != y.IPL ? y.IPL.CompareTo(x.IPL)
                :  x.IRQNR - y.IRQNR;
        }
    }

    public readonly IReadOnlyList<Irq> IRQ;
    readonly SortedSet<IrqNo> activeIRQs = new(IrqNoComparer.Instance);

    /// <summary>
    /// activeIRQs に登録されている値を割り込み番号から逆引きするために使用します。
    /// </summary>
    readonly Dictionary<int, IrqNo> irqNrToCurrentActiveIrq = new();

    readonly ILeveledISRNotify isrNotify;

    public readonly InputSignalBit SignalIrqAck;
    public IReadOnlyList<IOutputableSignal> SignalSWINTs => signalSWINTs;
    readonly OutputSignalBit[] signalSWINTs;
    public readonly IReadOnlyList<RegisterValue32<byte>> IER;
    public readonly IReadOnlyList<RegisterValue32<byte>> IPR;
    readonly short[] irqToIpr;
    /// <summary>
    /// 割り込み要因プライオリティレジスタの番号からベクター番号に変換するためのテーブル
    /// </summary>
    readonly IReadOnlyDictionary<short, ReadOnlyMemory<short>> iprToIrqNrs;
    public readonly IReadOnlyList<RegisterValue32<byte>> DMRSR;
    public readonly RegisterValue32<ushort> IRQFLTE0;
    public readonly RegisterValue32<ushort> IRQFLTE1;
    public readonly RegisterValue32<ushort> IRQFLTC0;
    public readonly RegisterValue32<ushort> IRQFLTC1;
    public readonly ReadOnlyRegisterValue<byte> NMISR;
    public readonly RegisterValue32<byte> NMIER;
    public readonly RegisterValue32<byte> NMICLR;
    public readonly RegisterValue32<byte> NMICR;
    public readonly RegisterValue32<byte> NMIFLTE;
    public readonly RegisterValue32<byte> NMIFLTC;
    public readonly RegisterValue32<ushort> FIR;
    public readonly IReadOnlyList<RegisterValue32<byte>> SWINTRs;

    void SetIRQ(int ipr, int irqNr) {
        var irqNo = new IrqNo(IPL: ipr, irqNr);
        irqNrToCurrentActiveIrq[irqNr] = irqNo;
        activeIRQs.Add(irqNo);
    }

    bool TryRemoveIRQ(int irqNr) {
        if (irqNrToCurrentActiveIrq.TryGetValue(irqNr, out var currentIRQ)) {
            activeIRQs.Remove(currentIRQ);
            return true;
        }
        return false;
    }

    void PostInterrupt() {
        var irq = activeIRQs.FirstOrDefault(static _ => true, new (0, 0));
        isrNotify.SetInterrupt(irq.IRQNR, irq.IPL);
    }

    void WriteIER(int no, RegisterValue32<byte> reg, byte value) {
        int diff = reg.Value ^ value;
        reg.Value = value;
        for (int i = 0; i < 8; i++) {
            if ((diff & (1 << i)) != 0) {
                Irq irq = IRQ[8 * no + i];
                irq.UpdateInterrupt();
            }
        }
    }

    void WriteSWINTR(int no, RegisterValue32<byte> reg, byte value) {
        if ((value & SWINTR_SWINT) != 0) {
            signalSWINTs[no].SetOutputSignal(true);
            signalSWINTs[no].SetOutputSignal(false);
        }
    }

    void WriteFIR(RegisterValue32<ushort> reg, ushort value) {
        Console.Error.WriteLine("*** FIR ena %u fir %u", (value & FIR_FIEN) != 0, value & 0xff);
        var oldIrqNo = reg.Value;
        reg.Value = (ushort)(value & 0x80ff);
        if ((reg.Value & FIR_FIEN) != 0) {
            IRQ[oldIrqNo].UpdateInterrupt();
        }
        if ((value & FIR_FIEN) != 0) {
            IRQ[value & 0xff].UpdateInterrupt();
        }
    }

    void WriteIPR(int no, RegisterValue32<byte> reg, byte value) {
        reg.Value = value;
        foreach (var irqNr in iprToIrqNrs[(short)no].Span) {
            if ((irqNr < 0) || (irqNr > 255)) {
                throw new Exception("Bug: Bad irqList");
            }
            Irq irq = IRQ[irqNr];
            Console.Error.WriteLine("IPR write IRQ %u idx %08x", irq.IrqNr, no);
            irq.UpdateLevelInterrupt();
        }
    }


    void AckInterrupt(bool prev, bool value) {
        if (!(!prev && value)) {
            return;
        }
        if (activeIRQs.Count <= 0) {
            throw new InvalidOperationException("non posted interrupt");
        }
        var irqNo = activeIRQs.First();
        var irq = IRQ[irqNo.IRQNR];
        switch ((IRQMD)(irq.IRQCR.Value & IRQCR_IRQMD_MSK)) {
            case IRQMD.N_EDGE:
            case IRQMD.P_EDGE:
            case IRQMD.BOTH_EDGE:
                irq.IR.Value = 0;
                irq.UpdateInterrupt();
                break;
        }
    }

    public ICU(string name, ILeveledISRNotify iSRNotify,
               IReadOnlyList<short> irqToIpr,
               IReadOnlyList<IRQMD> irqcr,
               int numOfSWINTs) {
        isrNotify = iSRNotify;
        this.irqToIpr = irqToIpr.ToArray();

        iprToIrqNrs = irqToIpr
            .Select((irqNr, ipr) => (irqNr, ipr: (short)ipr))
            .GroupBy(t => t.ipr)
            .ToDictionary(grouped => grouped.Key,
                          grouped => new ReadOnlyMemory<short>(grouped.Select(t => t.irqNr).ToArray()));
        IRQ = irqcr.Select((x, i) => new Irq(this, name, i, (byte)x))
            .ToArray();
        SignalIrqAck = new InputSignalBit($"{name}.irqAck", AckInterrupt);
        signalSWINTs = Enumerable.Range(0, numOfSWINTs)
            .Select(i => new OutputSignalBit($"{name}.swint{i + 1}"))
            .ToArray();

        IER = Enumerable.Range(0, 32)
            .Select(i => new RegisterValue32<byte>(0, onWrite: (reg, value) => WriteIER(i, reg, value)))
            .ToArray();

        IPR = Enumerable.Range(0, 256)
            .Select(i => new RegisterValue32<byte>(0, onWrite: (reg, value) => WriteIPR(i, reg, value)))
            .ToArray();

        DMRSR = Enumerable.Range(0, 8)
            .Select(_ => new RegisterValue32<byte>(0))
            .ToArray();

        IRQFLTE0 = new RegisterValue32<ushort>(0);
        IRQFLTE1 = new RegisterValue32<ushort>(0);
        IRQFLTC0 = new RegisterValue32<ushort>(0);
        IRQFLTC1 = new RegisterValue32<ushort>(0);
        NMISR = new ReadOnlyRegisterValue<byte>(0);
        NMIER = new RegisterValue32<byte>(0);
        NMICLR = new RegisterValue32<byte>(0);
        NMICR = new RegisterValue32<byte>(0);
        NMIFLTE = new RegisterValue32<byte>(0);
        NMIFLTC = new RegisterValue32<byte>(0);
        FIR = new RegisterValue32<ushort>(0, onWrite: WriteFIR);
        SWINTRs = Enumerable.Range(0, numOfSWINTs)
            .Select(i => new RegisterValue32<byte>(0, onWrite: (reg, val) => WriteSWINTR(i, reg, val)))
            .ToArray();
    }
}

public static class ICUOffset {
    public static readonly uint IR        = 0x000;
    public static readonly uint DTCER     = 0x100;
    public static readonly uint IER       = 0x200;
    public static readonly uint SWINTR    = 0x2E0;
    public static readonly uint FIR       = 0x2F0;
    public static readonly uint IPR       = 0x300;
    public static readonly uint DMRSR     = 0x400;
    public static readonly uint IRQCR     = 0x500;
    public static readonly uint IRQFLTE0  = 0x520;
    public static readonly uint IRQFLTE1  = 0x521;
    public static readonly uint IRQFLTC0  = 0x528;
    public static readonly uint IRQFLTC1  = 0x52A;
    public static readonly uint NMISR     = 0x580;
    public static readonly uint NMIER     = 0x581;
    public static readonly uint NMICLR    = 0x582;
    public static readonly uint NMICR     = 0x583;
    public static readonly uint NMIFLTE   = 0x590;
    public static readonly uint NMIFLTC   = 0x594;
}

public class ICUMapping {
    public IReadOnlyCollection<Register32MappingInfo> Mapping { get; }
    public ICU Instance { get; }

    public ICUMapping(IReadOnlyCollection<Register32MappingInfo> mapping, ICU instance) {
        Mapping = mapping;
        Instance = instance;
    }
}

public class ICURX65NMapping {
    static readonly IReadOnlyList<IRQMD> irqcr = new IRQMD[] {
        /*   0  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*   4  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*   8  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  12  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  16  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  20  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE,
        /*  24  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  28  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  32  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  36  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  40  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  44  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  48  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  52  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  56  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  60  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  64  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  68  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  72  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  76  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  80  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  84  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  88  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  92  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.N_EDGE,
        /*  96  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 100  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 104  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /* 108  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /* 112  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 116  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /* 120  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 124  */ IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 128  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 132  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 136  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 140  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 144  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 148  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 152  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 156  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 160  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 164  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 168  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 172  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 176  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 180  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 184  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 188  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 192  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 196  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 200  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 204  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 208  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 212  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 216  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 220  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 224  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 228  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 232  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 236  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 240  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 244  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 248  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 252  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
    };

    static readonly IReadOnlyList<short> irqToIpr = new short[] {
        /*   0 */  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        /*  16 */   0, -1,  0, -1, -1,  1, -1,  2, -1, -1,  3,  3,  4,  5,  6,  7,
        /*  32 */  -1, -1, 34, 35, -1, -1, 38, 39, 40, 41, 42, 43, 44, 45, -1, -1,
        /*  48 */  -1, -1, 50, 51, 52, 53, 54, 55, -1, -1, 58, 59, 60, 61, 62, 63,
        /*  64 */  64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
        /*  80 */  80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, -1, 92, 93, -1, 95,
        /*  96 */  96,  97,  98,  99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111,
        /* 112 */ 112, 113, 114, 115, 116, 117,  -1,  -1, 120, 121, 122, 123, 124, 125, 126, 127,
        /* 128 */ 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143,
        /* 144 */ 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 157, 159,
        /* 160 */ 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175,
        /* 176 */ 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191,
        /* 192 */ 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207,
        /* 208 */ 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223,
        /* 224 */ 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
        /* 240 */ 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255
    };

    public ICUMapping Mapping { get; }

    public ICURX65NMapping(ILeveledISRNotify isrNotify) {
        var obj = new ICU("icu", isrNotify, irqToIpr, irqcr, 2);
        var mapping = obj.IRQ
            .SelectMany((irq, i)=> new Register32MappingInfo[] {
                    new(ICUOffset.IR + (uint)i, irq.IR),
                    new(ICUOffset.DTCER + (uint)i, irq.DTCER)
                })
            .Concat(
                obj.IER.Select((ier, i) => new Register32MappingInfo(ICUOffset.IER + (uint)i, ier))
            )
            .Concat(obj.SWINTRs.Select(((swint, i) => new Register32MappingInfo(ICUOffset.SWINTR + (uint)i, swint))))
            .Prepend(new(ICUOffset.FIR, obj.FIR))
            .Concat(obj.IPR.Select((ipr, i) => new Register32MappingInfo(ICUOffset.IPR + (uint)i, ipr)))
            .Concat(obj.DMRSR.Select((dmrsr, i) => new Register32MappingInfo(ICUOffset.DMRSR + (uint)i * 4, dmrsr)))
            .Concat(Enumerable.Range(0, 16).Select(i => new Register32MappingInfo(ICUOffset.IRQCR + (uint)i, obj.IRQ[i + 64].IRQCR)))
            .Concat(new Register32MappingInfo[] {
                    new(ICUOffset.IRQFLTE0, obj.IRQFLTE0),
                    new(ICUOffset.IRQFLTE1, obj.IRQFLTE1),
                    new(ICUOffset.IRQFLTC0, obj.IRQFLTC0),
                    new(ICUOffset.IRQFLTC1, obj.IRQFLTC1),
                    new(ICUOffset.NMIFLTE, obj.NMIFLTE),
                    new(ICUOffset.NMIFLTC, obj.NMIFLTC),
                    new(ICUOffset.NMISR, obj.NMISR),
                    new(ICUOffset.NMIER, obj.NMIER),
                    new(ICUOffset.NMICLR, obj.NMICLR),
                    new(ICUOffset.NMICR, obj.NMICR)
                })
            .ToArray();
        Mapping = new(mapping, obj);
    }
}

public class ICURX64MMapping {
    static IReadOnlyList<IRQMD> irqcr = new IRQMD[] {
        /*   0  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*   4  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*   8  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  12  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  16  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  20  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE,
        /*  24  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  28  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  32  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  36  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  40  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  44  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  48  */ IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  52  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  56  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  60  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  64  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  68  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  72  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  76  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  80  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  84  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /*  88  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /*  92  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.N_EDGE,
        /*  96  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 100  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 104  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /* 108  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /* 112  */ IRQMD.LO_LVL, IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 116  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.LO_LVL, IRQMD.LO_LVL,
        /* 120  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 124  */ IRQMD.LO_LVL, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 128  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 132  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 136  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 140  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 144  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 148  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 152  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 156  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 160  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 164  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 168  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 172  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 176  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 180  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 184  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 188  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 192  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 196  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 200  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 204  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 208  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 212  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 216  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 220  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 224  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 228  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 232  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 236  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 240  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 244  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 248  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
        /* 252  */ IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE, IRQMD.N_EDGE,
    };

    static IReadOnlyList<short> irqToIpr = new short[] {
        /*   0 */  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        /*  16 */   0,  0,  0, -1, -1,  1, -1,  2, -1, -1,  3,  3,  4,  5,  6,  7,
        /*  32 */  32, 33, 34, 35, -1, -1, 38, 39, -1, -1, 42, 43, 44, 45, 46, 47,
        /*  48 */  48, -1, 50, 51, 52, 53, 54, 55, -1, -1, 58, 59, 60, 61, 62, 63,
        /*  64 */  64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
        /*  80 */  80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, -1, 92, 93, 94, 95,
        /*  96 */  96,  97,  98,  99, 100, 101, 102, 103, 104, 105, 106, 107,  -1,  -1, 110, 111,
        /* 112 */ 112, 113, 114, 115, 116, 117,  -1,  -1, 120, 121, 122, 123, 124, 125, 126, 127,
        /* 128 */ 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143,
        /* 144 */ 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
        /* 160 */ 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175,
        /* 176 */ 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191,
        /* 192 */ 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207,
        /* 208 */ 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223,
        /* 224 */ 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
        /* 240 */ 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255
    };

    public ICUMapping Mapping { get; }

    public ICURX64MMapping(ILeveledISRNotify isrNotify) {
        var obj = new ICU("icu", isrNotify, irqToIpr, irqcr, 2);
        var mapping = obj.IRQ
            .SelectMany((irq, i)=> new Register32MappingInfo[] {
                    new(ICUOffset.IR + (uint)i, irq.IR),
                    new(ICUOffset.DTCER + (uint)i, irq.DTCER)
                })
            .Concat(
                obj.IER.Select((ier, i) => new Register32MappingInfo(ICUOffset.IER + (uint)i, ier))
            )
            .Concat(obj.SWINTRs.Select(((swint, i) => new Register32MappingInfo(ICUOffset.SWINTR + (uint)i, swint))))
            .Prepend(new(ICUOffset.FIR, obj.FIR))
            .Concat(obj.IPR.Select((ipr, i) => new Register32MappingInfo(ICUOffset.IPR + (uint)i, ipr)))
            .Concat(obj.DMRSR.Select((dmrsr, i) => new Register32MappingInfo(ICUOffset.DMRSR + (uint)i * 4, dmrsr)))
            .Concat(Enumerable.Range(0, 16).Select(i => new Register32MappingInfo(ICUOffset.IRQCR + (uint)i, obj.IRQ[i + 64].IRQCR)))
            .Concat(new Register32MappingInfo[] {
                    new(ICUOffset.IRQFLTE0, obj.IRQFLTE0),
                    new(ICUOffset.IRQFLTE1, obj.IRQFLTE1),
                    new(ICUOffset.IRQFLTC0, obj.IRQFLTC0),
                    new(ICUOffset.IRQFLTC1, obj.IRQFLTC1),
                    new(ICUOffset.NMIFLTE, obj.NMIFLTE),
                    new(ICUOffset.NMIFLTC, obj.NMIFLTC),
                    new(ICUOffset.NMISR, obj.NMISR),
                    new(ICUOffset.NMIER, obj.NMIER),
                    new(ICUOffset.NMICLR, obj.NMICLR),
                    new(ICUOffset.NMICR, obj.NMICR)
                })
            .ToArray();
        Mapping = new(mapping, obj);
    }
}
