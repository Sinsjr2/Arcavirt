using System;

namespace CPU; 

/// <summary>
/// 登録されたコールバックを割り込み要求があった時に実行します。
/// </summary>
public class ISRActions : IISRNotify {

    readonly Dictionary<int, Action?> actions = new();

    public void SetCallback(int irqNo, Action? action) {
        actions[irqNo] = action;
    }

    public void SetInterrupt(int irqNo, bool value) {
        if (!value) {
            return;
        }
        if (actions.TryGetValue(irqNo, out var act)) {
            act?.Invoke();
        }
    }
}
