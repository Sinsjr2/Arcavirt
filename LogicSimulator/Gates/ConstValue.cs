using LogicSimulator.Runtime;

namespace LogicSimulator.Gates;

public record ConstValueLogic(int BitLength, ulong Value) : ILogicElement {
    public static ConstValueLogic FromBool(bool v) {
        return new ConstValueLogic(1, v ? 1UL : 0UL);
    }

    public static ConstValueLogic FromU8(int bitLength, byte v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI8(int bitLength, sbyte v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    public static ConstValueLogic FromU16(int bitLength, ushort v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI16(int bitLength, short v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    public static ConstValueLogic FromU32(int bitLength, uint v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI32(int bitLength, int v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    public static ConstValueLogic FromU64(int bitLength, ulong v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI64(int bitLength, long v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    static void ValidateUnsigned(int bitLength, ulong v) {
        if (bitLength <= 0 || bitLength > 64) {
            throw new ArgumentException($"bitLength must be between 1 and 64, but was {bitLength}.");
        }
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        if ((v & ~mask) != 0) {
            throw new ArgumentException($"Value {v} does not fit in {bitLength} bits.");
        }
    }

    static void ValidateSigned(int bitLength, long v) {
        if (bitLength <= 0 || bitLength > 64) {
            throw new ArgumentException($"bitLength must be between 1 and 64, but was {bitLength}.");
        }
        if (bitLength < 64) {
            long min = -(1L << (bitLength - 1));
            long max = (1L << (bitLength - 1)) - 1;
            if (v < min || v > max) {
                throw new ArgumentException($"Value {v} does not fit in {bitLength} signed bits.");
            }
        }
    }
}

public class ConstValueLogicExecutorFactory : ILogicExecutorFactory<ConstValueLogic> {
    class ConstValueLogicExecutor(LogicNode<ConstValueLogic>[] nodes) : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            for (int i = 0; i < nodes.Length; i++) {
                var cv = nodes[i].LogicData;
                for (int bit = 0; bit < cv.BitLength; bit++) {
                    outputs.WriteBit(i, bit, ((cv.Value >> bit) & 1) != 0
                        ? LogicSignal.High : LogicSignal.Low);
                }
            }
        }
    }

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<ConstValueLogic> node) {
        return new IOConnectorDefinition(node.LogicID, [], [new PinDefinition("out", node.LogicData.BitLength)]);
    }

    public ILogicExecutor CreateExecutor(LogicNode<ConstValueLogic>[] nodes, Action onInputChangedNotify) {
        onInputChangedNotify();
        return new ConstValueLogicExecutor(nodes);
    }
}
