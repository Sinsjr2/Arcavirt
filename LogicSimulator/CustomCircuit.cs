namespace LogicSimulator;

/// <summary>
/// ユーザーが作成した回路を名前で呼び出します。
/// </summary>
public record CustomCircuit(string TargetCircuitName) : ILogicElement;
