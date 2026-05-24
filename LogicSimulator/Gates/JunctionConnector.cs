using LogicSimulator.Runtime;

namespace LogicSimulator.Gates;

/// <summary>
/// InputBits.Sum() == OutputBits.Sum() が不変条件。
/// MSBファーストでビットを内部バッファに詰め、同順で出力に展開する。
/// </summary>
public record JunctionConnector(
    IReadOnlyList<int> InputBits,
    IReadOnlyList<int> OutputBits
) : ILogicElement;

public class JunctionConnectorExecutorFactory : ILogicExecutorFactory<JunctionConnector> {

    sealed class JunctionConnectorExecutor(int[][] inToOutPinMappings) : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                var mapping = inToOutPinMappings[logicNo];
                for (int inPin = 0; inPin < mapping.Length; inPin++) {
                    outputs.WriteBit(logicNo, mapping[inPin], inputs.ReadBit(logicNo, inPin));
                }
            }
        }
    }

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<JunctionConnector> node) {
        var junc = node.LogicData;
        var inputPins  = junc.InputBits.Select((bits, i)  => new PinDefinition("in",  i, bits)).ToArray();
        var outputPins = junc.OutputBits.Select((bits, i) => new PinDefinition("out", i, bits)).ToArray();
        return new IOConnectorDefinition(node.LogicID, inputPins, outputPins);
    }

    public IReadOnlyList<CircuitError> Validate(LogicNode<JunctionConnector> node) {
        var junc = node.LogicData;
        if (junc.InputBits.Sum() != junc.OutputBits.Sum()) {
            return [new CircuitError(node.LogicID, null, CircuitErrorKind.JunctionBitSumMismatch,
                $"JunctionConnector '{node.LogicID}': InputBits.Sum()={junc.InputBits.Sum()} != OutputBits.Sum()={junc.OutputBits.Sum()}")];
        }
        return [];
    }

    public ILogicExecutor CreateExecutor(LogicNode<JunctionConnector>[] nodes, Action onInputChangedNotify) {
        var mappings = nodes.Select(n => ComputeMapping(n.LogicData)).ToArray();
        return new JunctionConnectorExecutor(mappings);
    }

    static int[] ComputeMapping(JunctionConnector junc) {
        var totalBits = junc.InputBits.Sum();
        var bufPosToOutPin = new int[totalBits];
        int outOffset = 0;
        for (int i = 0; i < junc.OutputBits.Count; i++) {
            for (int j = 0; j < junc.OutputBits[i]; j++) {
                int bufPos = outOffset + junc.OutputBits[i] - 1 - j;
                bufPosToOutPin[bufPos] = outOffset + j;
            }
            outOffset += junc.OutputBits[i];
        }
        var mapping = new int[totalBits];
        int inOffset = 0;
        for (int i = 0; i < junc.InputBits.Count; i++) {
            for (int j = 0; j < junc.InputBits[i]; j++) {
                int bufPos = inOffset + junc.InputBits[i] - 1 - j;
                mapping[inOffset + j] = bufPosToOutPin[bufPos];
            }
            inOffset += junc.InputBits[i];
        }
        return mapping;
    }
}
