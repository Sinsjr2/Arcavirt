using LogicSimulator.Runtime;

namespace LogicSimulator.Gates;

public record XOrLogic(int NumOfInputs) : ILogicElement;

/// <summary>
/// XOR論理素子。入力のうち奇数個がHighのとき出力がHigh、偶数個のときLow。
/// いずれかの入力が不定(X)であれば、他の入力値に関わらず出力は常にX になる。
/// 入力数は任意に指定可能。
/// </summary>
public class XOrLogicExecutorFactory : ILogicExecutorFactory<XOrLogic> {
    class XOrLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool shouldProcess = true;
                bool result = false;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var input = inputs.ReadBit(logicNo, i);
                    bool boolSignal;
                    switch (input) {
                        case LogicSignal.Low:
                            boolSignal = false;
                            break;
                        case LogicSignal.High:
                            boolSignal = true;
                            break;
                        case LogicSignal.X:
                        default:
                            outputs.WriteBit(logicNo, 0, input);
                            shouldProcess = false;
                            continue;
                    }
                    result ^= boolSignal;
                }
                if (shouldProcess) {
                    outputs.WriteBit(logicNo, 0, result ? LogicSignal.High : LogicSignal.Low);
                }
            }
        }
    }

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<XOrLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [new PinDefinition("in", node.LogicData.NumOfInputs)],
            [new PinDefinition("out")]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<XOrLogic>[] nodes, Action onInputChangedNotify) {
        return new XOrLogicExecutor();
    }
}
