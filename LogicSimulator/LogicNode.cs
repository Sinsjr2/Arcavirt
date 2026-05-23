namespace LogicSimulator;

/// <summary>
///
/// </summary>
/// <param name="LogicID">配線する時に接続する素子を識別するID</param>
/// <param name="LogicData"></param>
public record LogicNode(string LogicID, ILogicElement LogicData);

public record LogicNode<T>(string LogicID, T LogicData);
