namespace Peripheral;

public record struct MaskedBit32(
    uint Mask, int Shift) {

    public static uint operator&(uint a, MaskedBit32 b) {
        return (a >> b.Shift) & b.Mask;
    }

    public static uint operator&(MaskedBit32 a, uint b) {
        return b & a;
    }
}
