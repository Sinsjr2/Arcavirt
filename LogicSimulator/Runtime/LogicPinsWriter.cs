using System.Runtime.CompilerServices;

namespace LogicSimulator.Runtime;

public struct LogicPinsWriter {
    LogicPins targetPins;
    List<int> valueChangedPins;

    public LogicPinsWriter(LogicPins targetPins, List<int> valueChangedPins) {
        this.targetPins = targetPins;
        this.valueChangedPins = valueChangedPins;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPinsLength(int logicNumber) {
        return targetPins.GetPinsLength(logicNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogicSignal ReadBit(int logicNumber, int pinNumber) {
        return targetPins.ReadBit(logicNumber, pinNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBit(int logicNumber, int pinNumber, LogicSignal value) {
        if (targetPins.WriteBit(logicNumber, pinNumber, value)) {
            valueChangedPins.Add(targetPins.GetPinIndex(logicNumber, pinNumber));
        }
    }
}
