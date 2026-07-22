using Clock;
using GPIO;

namespace Peripheral.Renesas;

public class CMT_N {
    static readonly ushort CMSTR_STR1   = 1 << 1;
    static readonly ushort CMCR_CKS_MSK = 3;
    static readonly ushort CMCR_CKS_SHIFT = 0;
    static readonly ushort CMCR_CMIE  = 1 << 6;
    static readonly ushort CMSTR_STR0 = 1 << 0;

    public bool IsRunning {
        get => timer.Enable;
        set => timer.Enable = value;
    }

    public readonly RegisterValue32<ushort> CMCR;
    public readonly RegisterValue32<ushort> CMCNT;
    public readonly RegisterValue32<ushort> CMCOR;

    readonly SubClock clkCntr;

    readonly CountTimer timer;

    /// <summary>
    /// コンペアマッチが発生したことを通知します。
    /// </summary>
    public IOutputableSignal IRQ => irq;
    readonly OutputSignalBit irq = new ("irq");

    public CMT_N(IClock clock, IClockFrequency clkIn) {
        clkCntr = new SubClock("clkCntr", clkIn, new Fraction(1, 8));
        CMCR = new(0, onWrite: WriteCMCR);
        CMCNT = new(0, onRead: ReadCMCNT, onWrite: WriteCMCNT);
        CMCOR = new(0xFFFF, onRead: ReadCMCOR, onWrite: WriteCMCOR);
        timer = new(clkCntr, clock,
                    new TickTimerTriggerAction[] {
                        new(new(TimerTriggerKind.None, ushort.MaxValue), OnElapsed)
                    },
                    TimerUpperLimitTriggerKind.ResetZero, ushort.MaxValue, null);
    }

    void WriteCMCR(RegisterValue32<ushort> reg, ushort value) {
        int cks = value & 3;
        reg.Value = value;
        uint div = 8u << (2 * cks);
        clkCntr.MulDiv = new Fraction(1, div);
    }

    ushort ReadCMCNT(RegisterValue32<ushort> reg) =>
        (ushort)timer.Count;

    void WriteCMCNT(RegisterValue32<ushort> reg, ushort value) =>
        timer.Count = value;

    ushort ReadCMCOR(RegisterValue32<ushort> reg) =>
        (ushort)timer.GetTrigger(0).TriggerCount;

    void WriteCMCOR(RegisterValue32<ushort> reg, ushort value) =>
        timer.SetTrigger(0, timer.GetTrigger(0) with { TriggerCount = value });

    void OnElapsed() {
        if ((CMCR.Value & CMCR_CMIE) != 0) {
            irq.SetOutputSignal(true);
            irq.SetOutputSignal(false);
        }
    }
}

public class CMT {
    public readonly RegisterValue32<ushort> CMSTR;
    public readonly IReadOnlyList<CMT_N> CMTn;

    public CMT(uint base_nr, IClock clock, IClockFrequency clkIn) {
        var cmt = new CMT_N[2];
        for (int i = 0; i < cmt.Length; i++) {
            cmt[i] = new CMT_N(clock, clkIn);
        }
        CMTn = cmt;
        CMSTR = new(0, onWrite: WriteCMSTR);
    }

    void WriteCMSTR(RegisterValue32<ushort> reg, ushort value) {
        CMSTR.Value = (ushort)(value & 3);
        CMTn[0].IsRunning = (value & 1) != 0;
        CMTn[1].IsRunning = (value & 2) != 0;
    }
}

public class CMTMapping {

    public IReadOnlyCollection<Register32MappingInfo> CreateMappingRX64M(CMT cmt) {
        var mappingInfos = new Register32MappingInfo[] {
            new(0, cmt.CMSTR),
        }
            .Concat(
                cmt.CMTn.Select(cmt => new Register32MappingInfo[] {
                        new(0, cmt.CMCR),
                        new(2, cmt.CMCNT),
                        new(4, cmt.CMCOR)
                    })
                .SelectMany((x, i) => x.Select(x2 => x2 with { Offset = 2 + (uint)i * 6 })))
            .ToArray();
        return mappingInfos;
    }
}
