using LogicSimulator.Runtime;

namespace LogicSimulator.Gates;

public record NOrLogic(int NumOfInputs) : ILogicElement;

/// <summary>
/// NOR論理素子。全入力がLowのときのみ出力がHigh、それ以外はLow。
/// 1つでもHigh入力があれば他の入力の不定(X)に関わらず出力はLow。
/// High がなくX が混在する場合は出力がX になる。入力数は任意に指定可能。
/// </summary>
public class NOrLogicExecutorFactory : ILogicExecutorFactory<NOrLogic> {
    class NOrLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool hasUnknownSignal = false;
                bool shouldProcess = true;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var signal = inputs.ReadBit(logicNo, i);
                    switch (signal) {
                        case LogicSignal.Low:
                            break;
                        case LogicSignal.High:
                            outputs.WriteBit(logicNo, 0, LogicSignal.Low);
                            shouldProcess = false;
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

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<NOrLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [new PinDefinition("in", node.LogicData.NumOfInputs)],
            [new PinDefinition("out")]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<NOrLogic>[] nodes, Action onInputChangedNotify) {
        return new NOrLogicExecutor();
    }
}
