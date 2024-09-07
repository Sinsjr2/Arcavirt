namespace CPU;

/// <summary>
/// 優先度付きの割り込みを表します。
/// </summary>
public interface ILeveledISRNotify {

    /// <summary>
    /// irqNo は実行する割り込みのベクター番号を指定します。
    /// 割り込みがなしの場合は irqNo と priority が 0 になります。
    /// priority は大きいほど優先度が高くなります。
    /// </summary>
    public void SetInterrupt(int irqNo, int priority);
}
