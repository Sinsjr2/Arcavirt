namespace LogicSimulator;

/// <summary>
/// 回路の素子とその回路の接続を表します。
/// </summary>
public record Circuit(IReadOnlyList<LogicNode> LogicNodes, IReadOnlyList<LogicConnection> LogicConnections);
