using GPIO;

namespace Peripheral.Renesas;

public class IoPin {
    /// <summary>
    /// Pin (output / input)
    /// </summary>
    public readonly SignalBit P;
    /// <summary>
    /// Pull Up
    /// </summary>
    public readonly IOutputableSignal PU;
    /// <summary>
    /// Peripheral Module Output
    /// </summary>
    public readonly IInputableSignal PMO;
    /// <summary>
    /// Peripheral Module Enable
    /// </summary>
    public readonly InputSignalBit PMOE;
    /// <summary>
    /// Pheripheral Module Input
    /// </summary>
    public readonly IOutputableSignal PMI;
    /// <summary>
    /// ISEL from MPC
    /// </summary>
    public readonly InputSignalBit ISEL;
    /// <summary>
    /// ASEL from MPC
    /// </summary>
    public readonly InputSignalBit ASEL;

    public IoPin(SignalBit p,
                 IOutputableSignal pu,
                 IInputableSignal pmo,
                 InputSignalBit pmoe,
                 IOutputableSignal pmi,
                 InputSignalBit isel,
                 InputSignalBit asel) {
        P = p;
        PU = pu;
        PMO = pmo;
        PMOE = pmoe;
        PMI = pmi;
        ISEL = isel;
        ASEL = asel;
    }
}

class IoPinInternal {
    /// <summary>
    /// Pin (output / input)
    /// </summary>
    public readonly SignalBit P;
    /// <summary>
    /// Pull Up
    /// </summary>
    public readonly SignalBit PU;
    /// <summary>
    /// Peripheral Module Output
    /// </summary>
    public readonly InputSignalBit PMO;
    /// <summary>
    /// Peripheral Module Enable
    /// </summary>
    public readonly InputSignalBit PMOE;
    /// <summary>
    /// Pheripheral Module Input
    /// </summary>
    public readonly SignalBit PMI;
    /// <summary>
    /// ISEL from MPC
    /// </summary>
    public readonly InputSignalBit ISEL;
    /// <summary>
    /// ASEL from MPC
    /// </summary>
    public readonly InputSignalBit ASEL;

    public IoPinInternal(string name, string portname, int pinNr, Action<bool, bool> onChangedInputSignal) {
        this.P = new SignalBit($"{name}.P{portname}.{pinNr}", onChangedInputSignal);
        this.PU = new SignalBit($"{name}.pu{portname}.{pinNr}", onChangedInputSignal);
        this.PMO = new InputSignalBit($"{name}.pmo{portname}.{pinNr}", onChangedInputSignal);
        this.PMOE = new InputSignalBit($"{name}.pmoe{portname}.{pinNr}", onChangedInputSignal);
        this.PMI = new SignalBit($"{name}.pmi{portname}.{pinNr}", onChangedInputSignal);
        this.ISEL = new InputSignalBit($"{name}.iseli{portname}.{pinNr}", onChangedInputSignal);
        this.ASEL = new InputSignalBit($"{name}.aseli{portname}.{pinNr}", onChangedInputSignal);
    }
}

public class IoPort {
    /// <summary>
    /// Port Direction Register
    /// </summary>
    public readonly RegisterValue32<byte> PDR;
    /// <summary>
    /// Port Output Data Register
    /// </summary>
    public readonly RegisterValue32<byte> PODR;
    /// <summary>
    /// Port Input Data Register
    /// </summary>
    public readonly ReadOnlyRegisterValue<byte> PIDR;
    /// <summary>
    /// Port Mode Register (Peripheral/IO)
    /// </summary>
    public readonly RegisterValue32<byte> PMR;
    /// <summary>
    /// Open Drain Control Register 0
    /// </summary>
    public readonly RegisterValue32<byte> ODR0;
    /// <summary>
    /// Open Drain Control Register 1
    /// </summary>
    public readonly RegisterValue32<byte> ODR1;
    /// <summary>
    /// Pull Up Resistor Control Register
    /// </summary>
    public readonly RegisterValue32<byte> PCR;
    /// <summary>
    /// Drive Capacity Control Register
    /// </summary>
    public readonly RegisterValue32<byte> DSCR;
    readonly IoPinInternal[] pins;

    public readonly IReadOnlyList<IoPin> Pins;

    static (int pu, int pmi, int pin) UpdateSignal(
        int pin, int pcr, int pdr, int pmoe, int podr, int pmo, int pmr, int isel, int asel) {
        var outputEnable = Multiplexer.Select(pmr, pdr, pmoe);
        var output = (outputEnable & Multiplexer.Select(pmr, podr, pmo)) | pin;
        var pu = pcr & ~outputEnable;
        var pmi = output | asel | ~(pmr | isel);
        return (pu, pmi, output);
    }

    public IoPort(string name, string pinName) {
        this.PDR = new RegisterValue32<byte>(0, onWrite: WritePDR);
        this.PODR = new RegisterValue32<byte>(0, onWrite: WritePODR);
        this.PIDR = new ReadOnlyRegisterValue<byte>(0);
        this.PMR = new RegisterValue32<byte>(0, onWrite: WritePMR);
        this.ODR0 = new RegisterValue32<byte>(0);
        this.ODR1 = new RegisterValue32<byte>(0);
        this.PCR = new RegisterValue32<byte>(0, onWrite: WritePCR);
        this.DSCR = new RegisterValue32<byte>(0);
        this.pins = Enumerable.Range(0, 8)
            .Select(j => new IoPinInternal(name, pinName, j, (_, _) => Update()))
            .ToArray();
        this.Pins = this.pins
            .Select(pin => new IoPin(pin.P,
                                     pin.PU,
                                     pin.PMO,
                                     pin.PMOE,
                                     pin.PMI,
                                     pin.ISEL,
                                     pin.ASEL))
            .ToArray();

        Update();
    }

    void Update() {
        int pinValue = 0;
        int pmoe = 0;
        int pmo = 0;
        int isel = 0;
        int asel = 0;
        for (int i = 0; i < pins.Length; i++) {
            var ioPin = pins[i];
            var shiftValue = 1 << i;
            pinValue |= ioPin.P.InputSignal ? shiftValue : 0;
            pmoe |= ioPin.PMOE.InputSignal ? shiftValue : 0;
            pmo |= ioPin.PMO.InputSignal ? shiftValue : 0;
            isel |= ioPin.ISEL.InputSignal ? shiftValue : 0;
            asel |= ioPin.ASEL.InputSignal ? shiftValue : 0;
        }
        var result = UpdateSignal(
            pin: pinValue,
            pcr: PCR.Value,
            pdr: PDR.Value,
            pmoe: pmoe,
            podr: PODR.Value,
            pmo: pmo,
            pmr: PMR.Value,
            isel: isel,
            asel: asel);

        PIDR.Value = (byte)result.pin;
        for (int i = 0; i < pins.Length; i++) {
            var ioPin = pins[i];
            var shiftValue = 1 << i;
            ioPin.P.SetOutputSignal((result.pin & shiftValue) != 0);
            ioPin.PU.SetOutputSignal((result.pu & shiftValue) != 0);
            ioPin.PMI.SetOutputSignal((result.pmi & shiftValue) != 0);
        }
    }

    void WritePDR(RegisterValue32<byte> reg, byte value) {
        bool isChanged = reg.Value != value;
        reg.Value = value;
        if (!isChanged) {
            return;
        }
        Update();
    }

    void WritePODR(RegisterValue32<byte> reg, byte value) {
        bool isChanged = reg.Value != value;
        reg.Value = value;
        if (!isChanged) {
            return;
        }
        Update();
    }

    void WritePMR(RegisterValue32<byte> reg, byte value) {
        bool isChanged = reg.Value != value;
        reg.Value = (byte)value;
        if (!isChanged) {
            return;
        }
        Update();
    }

    void WritePCR(RegisterValue32<byte> reg, byte value) {
        bool isChanged = reg.Value != value;
        reg.Value = value;
        if (!isChanged) {
            return;
        }
        Update();
    }
}

public class PORT {
    public readonly RegisterValue32<byte> PSRA = new(0);
    public readonly IReadOnlyList<IoPort> Ports;

    public PORT(string name, IReadOnlyList<string> portNames) {
        Ports = portNames.Select(name => new IoPort(name, name))
            .ToArray();
    }
}

public static class PORTOffset {
    public static readonly uint PDR  = 0x00;
    public static readonly uint PODR = 0x20;
    public static readonly uint PIDR = 0x40;
    public static readonly uint PMR  = 0x60;
    public static readonly uint ODR0 = 0x80;
    public static readonly uint ODR1 = 0x81;
    public static readonly uint PCR  = 0xC0;
    public static readonly uint DSCR = 0xE0;
    public static readonly uint PSRA = 0x121;

}

public class PORTMapping {
    public PORTMapping(IReadOnlyList<Register32MappingInfo> mapping, PORT instance) {
        Mapping = mapping;
        Instance = instance;
    }

    public IReadOnlyList<Register32MappingInfo> Mapping { get; }
    public PORT Instance { get; }
}

public class PORTRX64MMapping {
    public PORTMapping Mapping { get; }

    public PORTRX64MMapping() {
        var obj = new PORT(
            "PORT",
            new [] {
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "A", "B", "C", "D", "E", "F", "G", "x", "J"
            });
        var mapping =
            obj.Ports.SelectMany(
                port => new Register32MappingInfo[] {
                    new(PORTOffset.PDR, port.PDR),
                    new(PORTOffset.PODR, port.PODR),
                    new(PORTOffset.PIDR, port.PIDR),
                    new(PORTOffset.PMR, port.PMR),
                    new(PORTOffset.ODR0, port.ODR0),
                    new(PORTOffset.ODR1, port.ODR1),
                    new(PORTOffset.PCR, port.PCR),
                    new(PORTOffset.DSCR, port.DSCR),
                })
            .Prepend(new(PORTOffset.PSRA, obj.PSRA))
            .ToArray();
        Mapping = new PORTMapping(mapping, obj);
    }
}
