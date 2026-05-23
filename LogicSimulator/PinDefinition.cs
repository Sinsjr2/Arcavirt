using System.Text.RegularExpressions;

namespace LogicSimulator;

public record PinDefinition {
    public string PinName { get; }
    public int BitSize { get; }
    public bool IsIndexed { get; }

    public PinDefinition(string pinName) {
        if (Regex.IsMatch(pinName, @"^.+\[\d+\]$"))
            throw new ArgumentException($"Pin name must not contain index notation: '{pinName}'");
        PinName = pinName;
        BitSize = 1;
        IsIndexed = false;
    }

    public PinDefinition(string pinName, int bitCount) {
        if (Regex.IsMatch(pinName, @"^.+\[\d+\]$"))
            throw new ArgumentException($"Pin name must not contain index notation: '{pinName}'");
        if (bitCount < 1)
            throw new ArgumentException($"bitCount must be >= 1: {bitCount}");
        PinName = pinName;
        BitSize = bitCount;
        IsIndexed = true;
    }

    public PinDefinition(string baseName, int arrayIndex, int bitCount) {
        if (Regex.IsMatch(baseName, @"^.+\[\d+\]$"))
            throw new ArgumentException($"Base name must not contain index notation: '{baseName}'");
        if (arrayIndex < 0)
            throw new ArgumentException($"arrayIndex must be >= 0: {arrayIndex}");
        if (bitCount < 1)
            throw new ArgumentException($"bitCount must be >= 1: {bitCount}");
        PinName = $"{baseName}[{arrayIndex}]";
        BitSize = bitCount;
        IsIndexed = true;
    }
}
