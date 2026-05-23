using LogicSimulator.Runtime;

namespace LogicSimulator.Gates;

public record NAndLogic(int NumOfInputs) : ILogicElement;

/// <summary>
/// NAND論理素子。全入力がHighのときのみ出力がLow、それ以外はHigh。
/// 1つでもLow入力があれば他の入力の不定(X)に関わらず出力はHigh。
/// Low がなくX が混在する場合は出力がX になる。入力数は任意に指定可能。
/// </summary>
public class NAndLogicExecutorFactory : ILogicExecutorFactory<NAndLogic> {
    class NAndLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool hasUnknownSignal = false;
                bool shouldProcess = true;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var signal = inputs.ReadBit(logicNo, i);
                    switch (signal) {
                        case LogicSignal.Low:
                            outputs.WriteBit(logicNo, 0, LogicSignal.High);
                            shouldProcess = false;
                            break;
                        case LogicSignal.High:
                            break;
                        case LogicSignal.X:
                            hasUnknownSignal = true;
                            break;
                    }
                }
                if (shouldProcess) {
                    var output = hasUnknownSignal ? LogicSignal.X : LogicSignal.Low;
                    outputs.WriteBit(logicNo, 0, output);
                }
            }
        }
    }

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<NAndLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [new PinDefinition("in", node.LogicData.NumOfInputs)],
            [new PinDefinition("out")]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<NAndLogic>[] nodes, Action onInputChangedNotify) {
        return new NAndLogicExecutor();
    }
}
