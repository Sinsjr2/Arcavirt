using LogicSimulator.Runtime;

namespace LogicSimulator.Gates;

public class NotLogic : ILogicElement;

/// <summary>
/// NOT論理素子。入力を反転して出力する（High → Low、Low → High）。
/// 不定入力(X)はそのままX として出力する。入力・出力は各1ビット。
/// </summary>
public class NotLogicExecutorFactory : ILogicExecutorFactory<NotLogic> {
    class NotLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                var input = inputs.ReadBit(logicNo, 0);
                var output = input switch {
                    LogicSignal.Low => LogicSignal.High,
                    LogicSignal.High => LogicSignal.Low,
                    _ => input
                };
                outputs.WriteBit(logicNo, 0, output);
            }
        }
    }

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<NotLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [new PinDefinition("in")],
            [new PinDefinition("out")]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<NotLogic>[] nodes, Action onInputChangedNotify) {
        return new NotLogicExecutor();
    }
}
