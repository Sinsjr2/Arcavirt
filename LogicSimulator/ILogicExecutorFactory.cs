namespace LogicSimulator;

/// <summary>
/// 素子の構成データから演算用オブジェクトを生成します。
/// </summary>
public interface ILogicExecutorFactory {
    IOConnectorDefinition GetConnectorDefinition(LogicNode node);
    ILogicExecutor CreateExecutor(LogicNode[] nodes, Action onInputChangedNotify);
    IReadOnlyList<CircuitError> Validate(LogicNode node) => [];
}

public interface ILogicExecutorFactory<T> {
    IOConnectorDefinition GetConnectorDefinition(LogicNode<T> node);
    ILogicExecutor CreateExecutor(LogicNode<T>[] nodes, Action onInputChangedNotify);
    IReadOnlyList<CircuitError> Validate(LogicNode<T> node) => [];
}

public class LogicExecutorFactory<T>(
    ILogicExecutorFactory<T> factory
    ) : ILogicExecutorFactory {

    readonly ILogicExecutorFactory<T> factory = factory;

    public IOConnectorDefinition GetConnectorDefinition(LogicNode node) {
        return factory.GetConnectorDefinition(new LogicNode<T>(node.LogicID, (T)node.LogicData));
    }

    public ILogicExecutor CreateExecutor(LogicNode[] nodes, Action onInputChangedNotify) {
        var genericNodes = nodes.Select(node => new LogicNode<T>(node.LogicID, (T)node.LogicData)).ToArray();
        return factory.CreateExecutor(genericNodes, onInputChangedNotify);
    }

    public IReadOnlyList<CircuitError> Validate(LogicNode node) {
        return factory.Validate(new LogicNode<T>(node.LogicID, (T)node.LogicData));
    }
}
