namespace CPU; 

public interface IISRNotify {
    /// <summary>
    /// 割り込み信号のオン、オフを切り替えます。
    /// </summary>
    void SetInterrupt(int irqNo, bool value);
}
