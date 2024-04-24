using Pheripheral;

public class GPOutput1BitDriver {
    readonly IPheripheral gpio;
    readonly uint addrOffset;

    readonly uint mask;
    readonly int bitPos;

    public GPOutput1BitDriver(IPheripheral gpio, uint addrOffset, int bitPos) {
        if (!(0 <= bitPos && bitPos < 8)) {
            throw new ArgumentException($"bitPos: {bitPos}");
        }
        this.bitPos = bitPos;
        this.mask = (uint)1 << bitPos;
        this.gpio = gpio;
        this.addrOffset = addrOffset;
    }

    public void ChangeToOutput() {
        var dirReg = addrOffset + (uint)GPIORegister.PortDir;
        var dir = gpio.ReadUint32(dirReg);
        gpio.WriteUint32(dirReg, (uint)(dir & ~mask) | ((uint)GPIODir.Output << bitPos));

    }

    public void Set(bool value) {
        var outputReg = addrOffset + (uint)GPIORegister.PortOutput;
        var output = gpio.ReadUint32(outputReg);
        uint applied = (value ? mask : 0) | (output & ~mask);
        gpio.WriteUint32(outputReg, applied);
    }
}

public class GPInput1BitDriver {
    readonly IPheripheral gpio;
    readonly uint addrOffset;

    readonly uint mask;
    readonly int bitPos;

    public GPInput1BitDriver(IPheripheral gpio, uint addrOffset, int bitPos) {
        if (!(0 <= bitPos && bitPos < 8)) {
            throw new ArgumentException($"bitPos: {bitPos}");
        }
        this.bitPos = bitPos;
        this.mask = (uint)1 << bitPos;
        this.gpio = gpio;
        this.addrOffset = addrOffset;
    }

    public void ChangeToInput() {
        var dirReg = addrOffset + (uint)GPIORegister.PortDir;
        var dir = gpio.ReadUint32(dirReg);
        gpio.WriteUint32(dirReg, (uint)(dir & ~mask) | ((uint)GPIODir.Input << bitPos));

    }

    public bool Get() {
        var inputReg = addrOffset + (uint)GPIORegister.PortInput;
        var input = gpio.ReadUint32(inputReg);
        return (input & mask) != 0;
    }

}
