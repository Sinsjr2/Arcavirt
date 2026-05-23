using LogicSimulator.Runtime;

namespace LogicSimulator;

public interface ILogicExecutor {
    void Execute(LogicPinReader inputs, LogicPinsWriter outputs);
}
