using Clock;
using Peripheral.Renesas;

namespace Renesas;

public class CLOCK {

    static readonly byte PLLCR2_PLLEN = 1 << 0;
    static readonly byte OSCOVFSR_MOOVF  = 1 << 0;
    static readonly byte OSCOVFSR_SOOVF  = 1 << 1;
    static readonly byte OSCOVFSR_PLOVF  = 1 << 2;
    static readonly byte OSCOVFSR_HCOVF  = 1 << 3;
    static readonly byte OSCOVFSR_ILCOVF = 1 << 4;

    uint subOscFreq;
    uint mainOscFreq;
    uint iwdtOscFreq;
    uint locoOscFreq;
    uint hocoOscFreq;

    public readonly MainClock clkMainOsc;
    public readonly MainClock clkSubOsc;
    public readonly MainClock clkHoco;
    public readonly MainClock clkLoco;
    public readonly SubClock clkPllOut;

    readonly ClockSelector pllInClk;
    readonly ClockSelector clkFDivIn;

    public readonly ClockSelector clkFCLK;
    public readonly ClockSelector clkICLK;
    public readonly ClockSelector clkPCKA;
    public readonly ClockSelector clkPCKB;
    public readonly ClockSelector clkPCKC;
    public readonly ClockSelector clkPCKD;
    public readonly ClockSelector clkBCLK;
    public readonly ClockSelector clkUCLK;
    public readonly MainClock clkIWDTCLK;

    public readonly RegisterValue32<uint> SCKCR;
    public readonly RegisterValue32<byte> ROMWT;
    public readonly RegisterValue32<ushort> SCKCR2;
    public readonly RegisterValue32<ushort> SCKCR3;
    public readonly RegisterValue32<ushort> PLLCR;
    public readonly RegisterValue32<byte> PLLCR2;
    public readonly RegisterValue32<byte> BCKCR;
    public readonly RegisterValue32<byte> MOSCCR;
    public readonly RegisterValue32<byte> SOSCCR;
    public readonly RegisterValue32<byte> LOCOCR;
    public readonly RegisterValue32<byte> ILOCOCR;
    public readonly RegisterValue32<byte> HOCOCR;
    public readonly RegisterValue32<byte> HOCOCR2;
    public readonly ReadOnlyRegisterValue<byte> OSCOVFSR;
    public readonly RegisterValue32<byte> OSTDCR;
    public readonly RegisterValue32<byte> OSTDSR;
    public readonly RegisterValue32<byte> MOSCWTCR;
    public readonly RegisterValue32<byte> SOSCWTCR;
    public readonly RegisterValue32<byte> MOFCR;
    public readonly RegisterValue32<byte> HOCOPCR;

    public CLOCK(string name) {
        mainOscFreq = 12 * 1000 * 1000;
        subOscFreq = 32768;
        iwdtOscFreq = 120 * 1000;
        locoOscFreq = 240 * 1000;
        hocoOscFreq = 16 * 1000 * 1000;

        SCKCR = new RegisterValue32<uint>(0, onWrite: WriteSCKCR);
        ROMWT = new RegisterValue32<byte>(0, onWrite: WriteROMWT);
        SCKCR2 = new RegisterValue32<ushort>(0, onWrite: WriteSCKCR2, onRead: ReadSCKCR2);
        SCKCR3 = new RegisterValue32<ushort>(0, onWrite: WriteSCKCR3);
        PLLCR = new RegisterValue32<ushort>(0x1d00, onWrite: WritePLLCR);
        PLLCR2 = new RegisterValue32<byte>(0x1, onWrite: WritePLLCR2);
        BCKCR = new RegisterValue32<byte>(0, onWrite: WriteBCKCR);
        MOSCCR = new RegisterValue32<byte>(0, onWrite: WriteMOSCCR);
        SOSCCR = new RegisterValue32<byte>(0, onWrite: WriteSOSCCR);
        LOCOCR = new RegisterValue32<byte>(0, onWrite: WriteLOCOCR);
        ILOCOCR = new RegisterValue32<byte>(1, onWrite: WriteILOCOCR);
        HOCOCR = new RegisterValue32<byte>(0, onWrite: WriteHOCOCR);
        HOCOCR2 = new RegisterValue32<byte>(0, onWrite: WriteHOCOCR2);
        OSCOVFSR = new ReadOnlyRegisterValue<byte>(0);
        OSTDCR = new RegisterValue32<byte>(0, onWrite: WriteOSTDCR);
        OSTDSR = new RegisterValue32<byte>(0, onWrite: WriteOSTDSR);
        MOSCWTCR = new RegisterValue32<byte>(0x53, onWrite: WriteMOSCWTCR);
        SOSCWTCR = new RegisterValue32<byte>(0x21, onWrite: WriteSOSCWTCR);
        MOFCR = new RegisterValue32<byte>(0, onWrite: WriteMOFCR);
        HOCOPCR = new RegisterValue32<byte>(0, onWrite: WriteHOCOPCR);

        clkMainOsc = new MainClock($"{name}.extal", new Fraction(0, 1));
        clkSubOsc = new MainClock($"{name}.subosc", new Fraction(subOscFreq, 1));
        clkHoco = new MainClock($"{name}.hoco", new Fraction(0, 1));
        clkLoco = new MainClock($"{name}.loco", new Fraction(locoOscFreq, 1));

        clkPllOut = new SubClock($"{name}.pllout", clkMainOsc, new Fraction(0, 1));
        pllInClk = new ClockSelector($"{name}.pllin", new IClockFrequency[] {
                clkMainOsc,
                clkHoco
            }, 0);
        clkFDivIn = new ClockSelector($"{name}.fdivin", new IClockFrequency[] {
                clkLoco,
                clkHoco,
                clkMainOsc,
                clkSubOsc,
                clkPllOut,
            }, 3);
        var clkDiv1 = new SubClock($"{name}.div1", clkFDivIn, new Fraction(1, 1));
        var clkDiv2 = new SubClock($"{name}.div2", clkFDivIn, new Fraction(1, 2));
        var clkDiv3 = new SubClock($"{name}.div3", clkFDivIn, new Fraction(1, 3));
        var clkDiv4 = new SubClock($"{name}.div4", clkFDivIn, new Fraction(1, 4));
        var clkDiv5 = new SubClock($"{name}.div5", clkFDivIn, new Fraction(1, 5));
        var clkDiv8 = new SubClock($"{name}.div8", clkFDivIn, new Fraction(1, 8));
        var clkDiv16 = new SubClock($"{name}.div16", clkFDivIn, new Fraction(1, 16));
        var clkDiv32 = new SubClock($"{name}.div32", clkFDivIn, new Fraction(1, 32));
        var clkDiv64 = new SubClock($"{name}.div64", clkFDivIn, new Fraction(1, 64));

        var derivedClocks = new SubClock[] {
            clkDiv1,
            clkDiv2,
            clkDiv4,
            clkDiv8,
            clkDiv16,
            clkDiv32,
            clkDiv64,
        };

        clkFCLK = new ClockSelector($"{name}.fclk", derivedClocks, 0);
        clkICLK = new ClockSelector($"{name}.iclk", derivedClocks, 0);
        clkICLK.OnChangedFrequency += (_, _) => VerifyROMWT();

        clkPCKA = new ClockSelector($"{name}.pcka", derivedClocks, 0);
        clkPCKB = new ClockSelector($"{name}.pckb", derivedClocks, 0);
        clkPCKC = new ClockSelector($"{name}.pckc", derivedClocks, 0);
        clkPCKD = new ClockSelector($"{name}.pckd", derivedClocks, 0);
        clkBCLK = new ClockSelector($"{name}.bclk", derivedClocks, 0);
        clkUCLK = new ClockSelector(
            $"{name}.uclk",
            new SubClock[] {
                // 1
                clkDiv2,
                clkDiv3,
                clkDiv4,
                clkDiv5,
            },
            1);
        clkIWDTCLK = new MainClock($"{name}.iwdtclk", new Fraction(0, 1));
    }

    void VerifyROMWT() {
        uint div = ROMWT.Value switch {
            0 => 1,
            1 => 2,
            2 => 3,
            _ => throw new Exception("Illegal Wait states for Flash in ROMWT register"),
        };
        var flashFreq = (clkICLK.Frequency / div).Value;
        if (flashFreq > (50 * 1000 * 1000)) {
            throw new Exception($"Error: ROMWT Flash Frequency to high: {flashFreq}");
        }
    }

    void UpdatePLLClock() {
        int plldiv = PLLCR.Value & 0xf;
        int pllsrcsel = (PLLCR.Value >> 4) & 1;
        int stc = (PLLCR.Value >> 8) & 0x3f;
        uint div = plldiv switch {
            0 => 1,
            1 => 2,
            2 => 3,
            _ => throw new Exception($"Illegal PLLCR value 0x{PLLCR:x}")
        };
        if ((stc > 0x3b) || (stc < 0x13)) {
            throw new Exception($"Illegal PLLCR STC value {stc} (0x{PLLCR:x})");
        }
        int mul = 20 + (stc - 0x13);
        pllInClk.ChangeSource(pllsrcsel);
        if ((PLLCR2.Value & 1) != 0) {
            OSCOVFSR.Value &= (byte)~OSCOVFSR_PLOVF;
            // If PLL is disabled then the clock is 0
            clkPllOut.MulDiv = new Fraction(0, 1);
        } else {
            {
                var freq = pllInClk.Frequency.Value;
                if (((freq / div) > 24000000) || ((freq / div) < 8000000)) {
                    Console.Error.WriteLine($"PLL: Divided Frequency out of range: {freq} / {div}");
                }
            }
            clkPllOut.MulDiv = new Fraction(1, div);
            {
                var freq = clkPllOut.Frequency.Value;
                if ((freq > 240000000) || (freq < 120000000)) {
                    Console.Error.WriteLine($"PLL: Output Frequency out of range {freq}");
                }
            }
            OSCOVFSR.Value |= OSCOVFSR_PLOVF;
        }
    }

    static void SCKCRDeriveClock(ClockSelector derivedClk, uint selection) {
        switch (selection) {
            case >= 0 and <= 6:
                derivedClk.ChangeSource((int)selection);
                break;
            default:
                Console.Error.WriteLine("Illegal CLK selection in SCKCR");
                derivedClk.ChangeSource(0);
                break;
        }
    }

    void WriteSCKCR(RegisterValue32<uint> reg, uint value) {
        reg.Value = value & 0xffcfffff;
        uint fck = (value >> 28) & 0xf;
        uint ick = (value >> 24) & 0xf;
        uint pstop = (value >> 22) & 3;
        uint bck = (value >> 16) & 0xf;
        uint pcka = (value >> 12) & 0xf;
        uint pckb = (value >> 8) & 0xf;
        uint pckc = (value >> 4) & 0xf;
        uint pckd = (value & 0xf);
        SCKCRDeriveClock(clkFCLK, fck);
        SCKCRDeriveClock(clkICLK, ick);
        SCKCRDeriveClock(clkBCLK, bck);
        SCKCRDeriveClock(clkPCKA, pcka);
        SCKCRDeriveClock(clkPCKB, pckb);
        SCKCRDeriveClock(clkPCKC, pckc);
        SCKCRDeriveClock(clkPCKD, pckd);
    }

    void WriteROMWT(RegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(value & 3);
        VerifyROMWT();
    }

    ushort ReadSCKCR2(RegisterValue32<ushort> reg) {
        return (ushort)(reg.Value | 1);
    }

    void WriteSCKCR2(RegisterValue32<ushort> reg, ushort value) {
        reg.Value = (ushort)(value & (0xf << 4));
        uint uck = ((uint)value >> 4) & 0xf;
        if ((value & 0xff0f) != 0x0001) {
            throw new Exception($"SCKCR2: Bad Reserved Bits: 0x{value:x}");
        }
        switch (uck) {
            case >= 1 and <= 4:
                clkUCLK.ChangeSource((int)uck - 1);
                break;
            default:
                throw new Exception($"Bad UCK selection {uck}");
        }
    }

    void WriteSCKCR3(RegisterValue32<ushort> reg, ushort value) {
        reg.Value = (ushort)(value & 0x0700);
        uint cksel = ((uint)value >> 8) & 7;
        if ((value & 0xF8FF) != 0) {
            throw new Exception($"SCKCR3: Write illegal value 0x{value:x}");
        }
        switch (cksel) {
            case >= 0 and <= 4:
                clkFDivIn.ChangeSource((int)cksel);
                break;
            default:
                throw new Exception($"SCKCR3: illegal cksel {cksel}\n");
        }
    }

    void WritePLLCR(RegisterValue32<ushort> reg, ushort value) {
        reg.Value = (ushort)(value & 0x3f13);
        UpdatePLLClock();
    }

    void WritePLLCR2(RegisterValue32<byte> reg, byte value) {
        byte diffEn = (byte)((value & 1) ^ (PLLCR2.Value & 1));
        reg.Value = (byte)(value & 1);
        if (diffEn != 0) {
            UpdatePLLClock();
        }
    }

    void WriteBCKCR(RegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(value & 1);
        Console.Error.WriteLine("External bus clock pin not implemented");
    }

    void WriteMOSCCR(RegisterValue32<byte> reg, byte value) {
        if ((value & 1) != 0) {
            OSCOVFSR.Value &= (byte)~OSCOVFSR_MOOVF;
            clkMainOsc.Frequency = new Fraction(0, 1);
        } else {
            clkMainOsc.Frequency = new Fraction(1, mainOscFreq);
            OSCOVFSR.Value |= OSCOVFSR_MOOVF;
        }
        reg.Value = (byte)(value & 1);
    }

    void WriteSOSCCR(RegisterValue32<byte> reg, byte value) {
        // Clock also runs when RTCEN is set, this is missing here
        if ((value & 1) != 0) {
            OSCOVFSR.Value &= (byte)~OSCOVFSR_SOOVF;
            clkSubOsc.Frequency = new Fraction(0, 1);
        } else {
            OSCOVFSR.Value |= OSCOVFSR_SOOVF;
            clkSubOsc.Frequency = new Fraction(1, subOscFreq);
        }
        reg.Value = (byte)(value & 1);
    }

    void WriteLOCOCR(RegisterValue32<byte> reg, byte value) {
        if ((value & 1) != 0) {
            clkLoco.Frequency = new Fraction(0, 1);
        } else {
            clkLoco.Frequency = new Fraction(1, locoOscFreq);
        }
        reg.Value = (byte)(value & 1);
    }

    void WriteILOCOCR(RegisterValue32<byte> reg, byte value) {
        if ((value & 1) != 0) {
            OSCOVFSR.Value &= (byte)~OSCOVFSR_ILCOVF;
            clkIWDTCLK.Frequency = new Fraction(0, 1);
        } else {
            OSCOVFSR.Value |= OSCOVFSR_ILCOVF;
            clkIWDTCLK.Frequency = new Fraction(1, iwdtOscFreq);
        }
        reg.Value = (byte)(value & 1);
    }

    void WriteHOCOCR(RegisterValue32<byte> reg, byte value) {
        if ((value & 1) != 0) {
            OSCOVFSR.Value |= OSCOVFSR_HCOVF;
            clkHoco.Frequency = new Fraction(0, 1);
        } else {
            clkHoco.Frequency = new Fraction(1, hocoOscFreq);
        }
        reg.Value = (byte)(value & 1);
    }

    void WriteHOCOCR2(RegisterValue32<byte> reg, byte value) {
        byte hcfrq = (byte)(value & 3);
        if ((HOCOCR.Value & 1) == 0) {
            throw new Exception("Prohibited writing to HOCOCR2 while Oscillator is enabled");
        }
        var freq = hcfrq switch {
            0=> 16 * 1000 * 1000,
            1 => 18 * 1000 * 1000,
            2 => 20 * 1000 * 1000,
            _ => throw new Exception("Illegal HOCO Frequence selection"),
        };
        hocoOscFreq = (uint)freq;
        reg.Value = (byte)(value & 3);
    }

    void WriteOSTDCR(IRegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(value & 0x81);
    }

    void WriteOSTDSR(RegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(reg.Value & value);
    }

    void WriteMOSCWTCR(RegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(value & 0xff);
    }

    void WriteSOSCWTCR(RegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(value & 0xff);
    }

    void WriteMOFCR(RegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(value & 0x71);
        Console.Error.WriteLine("Main Osc driving strenth");
    }

    void WriteHOCOPCR(RegisterValue32<byte> reg, byte value) {
        reg.Value = (byte)(value & 1);
        if ((value & 1) != 0) {
            Console.Error.WriteLine("Turning off HOCO power supply not implemented");
        }
    }
}

public static class CLOCKOffset {
    public static readonly uint SCKCR    = 0x0020;
    public static readonly uint ROMWT    = 0x101C;
    public static readonly uint SCKCR2   = 0x0024;
    public static readonly uint SCKCR3   = 0x0026;
    public static readonly uint PLLCR    = 0x0028;
    public static readonly uint PLLCR2   = 0x002A;
    public static readonly uint BCKCR    = 0x0030;
    public static readonly uint MOSCCR   = 0x0032;
    public static readonly uint SOSCCR   = 0x0033;
    public static readonly uint LOCOCR   = 0x0034;
    public static readonly uint ILOCOCR  = 0x0035;
    public static readonly uint HOCOCR   = 0x0036;
    public static readonly uint HOCOCR2  = 0x0037;
    public static readonly uint OSCOVFSR = 0x003C;
    public static readonly uint OSTDCR   = 0x0040;
    public static readonly uint OSTDSR   = 0x0041;
    public static readonly uint MOSCWTCR = 0x00A2;
    public static readonly uint SOSCWTCR = 0x00A3;
    public static readonly uint MOFCR    = 0xC293;
    public static readonly uint HOCOPCR  = 0xC294;
}

public class CLOCKMapping {
    public IReadOnlyList<Register32MappingInfo> Mapping { get; }
    public CLOCK Instance { get; }

    public CLOCKMapping(IReadOnlyList<Register32MappingInfo> mapping, CLOCK instance) {
        Mapping = mapping;
        Instance = instance;
    }
}
public class CLOCKRX64MMapping {
    public CLOCKMapping Mapping { get; }

    public CLOCKRX64MMapping() {
        var obj = new CLOCK("CLOCK");
        var mapping = new Register32MappingInfo[] {
            new(CLOCKOffset.SCKCR, obj.SCKCR),
            new(CLOCKOffset.ROMWT, obj.ROMWT),
            new(CLOCKOffset.SCKCR2, obj.SCKCR2),
            new(CLOCKOffset.SCKCR3, obj.SCKCR3),
            new(CLOCKOffset.PLLCR, obj.PLLCR),
            new(CLOCKOffset.PLLCR2, obj.PLLCR2),
            new(CLOCKOffset.BCKCR, obj.BCKCR),
            new(CLOCKOffset.MOSCCR, obj.MOSCCR),
            new(CLOCKOffset.SOSCCR, obj.SOSCCR),
            new(CLOCKOffset.LOCOCR, obj.LOCOCR),
            new(CLOCKOffset.ILOCOCR, obj.ILOCOCR),
            new(CLOCKOffset.HOCOCR, obj.HOCOCR),
            new(CLOCKOffset.HOCOCR2, obj.HOCOCR2),
            new(CLOCKOffset.OSCOVFSR, obj.OSCOVFSR),
            new(CLOCKOffset.OSTDCR, obj.OSTDCR),
            new(CLOCKOffset.OSTDSR, obj.OSTDSR),
            new(CLOCKOffset.MOSCWTCR, obj.MOSCWTCR),
            new(CLOCKOffset.SOSCWTCR, obj.SOSCWTCR),
            new(CLOCKOffset.MOFCR, obj.MOFCR),
            new(CLOCKOffset.HOCOPCR, obj.HOCOPCR),
        };
        Mapping = new(mapping, obj);
    }
}
