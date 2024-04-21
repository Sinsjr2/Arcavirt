using Pheripheral;

public class PWMTimerDriver {

    readonly Pheripheral.IPheripheral timer;
    readonly uint timerRegOffset;

    /// <summary>
    /// タイマーの動作周波数を返します。
    /// </summary>
    public readonly float TimerHz;

    // /// <summary>
    // /// 1カウントアップ当たりの
    // /// </summary>
    // public readonly float CountUpTime;

    public PWMTimerDriver(IPheripheral timer, uint timerRegOffset, float timerHz) {
        this.timer = timer;
        this.timerRegOffset = timerRegOffset;
        TimerHz = timerHz;
    }

    /// <summary>
    /// タイマーをスタートして波形の出力を開始します。
    /// 割り込みも有効にします。
    /// </summary>
    public void Start() {
        var ctrlReg = timerRegOffset + (uint)PWMRegister.Control;
        var ctrl = timer.ReadUint32(ctrlReg);
        timer.WriteUint32(ctrlReg,
                          ctrl | (uint)(PWMControlRegister.Enable |
                                        PWMControlRegister.InterruptEnable));
    }

    /// <summary>
    /// タイマーを停止して波形の出力を停止します。
    /// 割り込みも停止します。
    /// </summary>
    public void Stop() {
        var ctrlReg = timerRegOffset + (uint)PWMRegister.Control;
        var ctrl = timer.ReadUint32(ctrlReg);
        timer.WriteUint32(ctrlReg,
                          (uint)(ctrl & ~(uint)(PWMControlRegister.Enable |
                                                PWMControlRegister.InterruptEnable)));
    }

    /// <summary>
    /// 周期とデューティを設定します。
    /// 周期とパルスの幅はタイマーのカウント数で指定します。
    /// </summary>
    public void Set(int cycle, int pulseWidth) {
        timer.WriteUint32(timerRegOffset + (uint)PWMRegister.Top, (ushort)(cycle + pulseWidth));
        timer.WriteUint32(timerRegOffset + (uint)PWMRegister.CompareMatch, (ushort)(cycle - pulseWidth));
    }
}
