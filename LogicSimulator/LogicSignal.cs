namespace LogicSimulator;

/// <summary>
/// 論理回路で使用する信号を表します。
/// </summary>
public enum LogicSignal : byte {
    /// <summary>
    /// 不定
    /// </summary>
    X,
    /// <summary>
    /// ロー信号
    /// </summary>
    Low,
    /// <summary>
    /// ハイ信号
    /// </summary>
    High,
}

public static class LogicSignalExtensions {
    public static LogicSignal ToSignal(this bool value) =>
        value ? LogicSignal.High : LogicSignal.Low;
}
