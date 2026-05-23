using LogicSimulator.Runtime;

namespace LogicSimulator.Gates;

public record AndLogic(int NumOfInputs) : ILogicElement;

/// <summary>
/// AND論理素子。全入力がHighのときのみ出力がHigh、それ以外はLow。
/// 1つでもLow入力があれば他の入力の不定(X)に関わらず出力はLow。
/// Low がなくX が混在する場合は出力がX になる。入力数は任意に指定可能。
/// </summary>
public class AndLogicExecutorFactory : ILogicExecutorFactory<AndLogic> {
    class AndExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool hasUnknownSignal = false;
                bool shouldProcess = true;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var signal = inputs.ReadBit(logicNo, i);
                    switch (signal) {
                        case LogicSignal.Low:
                            outputs.WriteBit(logicNo, 0, LogicSignal.Low);
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
                    var output = hasUnknownSignal ? LogicSignal.X : LogicSignal.High;
                    outputs.WriteBit(logicNo, 0, output);
                }
            }
        }
    }

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<AndLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [new PinDefinition("in", node.LogicData.NumOfInputs)],
            [new PinDefinition("out")]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<AndLogic>[] nodes, Action onInputChangedNotify) {
        return new AndExecutor();
    }
}
