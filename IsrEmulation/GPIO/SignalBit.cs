
namespace GPIO;

/*
IOを結合した場合は、の動作は以下のとおりになること。
- 2以上のIOを結合できること
- 結合した場合、一つでも信号がHiがあれば全ての信号はHiになること
  入力信号として使用する場合は必ず信号をLowにする必要がある
  もちろん、自身の出力信号をHiにすると自身の入力信号もHiになる
- Hiだった信号をLowに戻すもしくはリンクを解除するとすべての信号はLowになること
*/

/// <summary>
/// 名前付きの信号を表します。
/// </summary>
public interface INamedSignal {
    string SignalName { get; }
}

/// <summary>
/// 出力値を取得したり、入力場合は他の出力値で反映された結果を取得します。
/// </summary>
public interface IOutputableSignal : INamedSignal {
    bool OutputSignal { get; }

    /// <summary>
    /// 変化する前の値と変化した後の出力の値を通知します。
    /// </summary>
    event Action<bool, bool>? OnChangedSignalValue;
}

/// <summary>
/// 信号に入力値を反映させます。
/// </summary>
public interface IInputableSignal : INamedSignal {
    bool InputSignal { get; set; }
}

/// <summary>
/// 入出力可能な信号を表します。
/// 以下、IInputableSignal の動作です。
/// - 出力を true にした場合は、入力も同じくtrueになります。
/// - 出力が false の場合は入力で設定した値が優先されます。
/// </summary>
public interface IIOSignal : IOutputableSignal, IInputableSignal {}

public class InputSignalBit : IInputableSignal {
    public string SignalName { get; }

    bool inputSignalValue = false;

    public bool InputSignal {
        get => inputSignalValue;
        set {
            var prevSignalValue = inputSignalValue;
            inputSignalValue = value;
            if (prevSignalValue != value) {
                onChangedInputSignal?.Invoke(prevSignalValue, value);
            }
        }
    }

    /// <summary>
    /// 入力の信号が変化したことを通知します。
    /// 以前の値と現在の値を渡します。
    /// </summary>
    readonly Action<bool, bool>? onChangedInputSignal;

    public InputSignalBit(string signalName, Action<bool, bool>? onChangedInputSignal) {
        SignalName = signalName;
        this.onChangedInputSignal = onChangedInputSignal;
    }
}

public class OutputSignalBit : IOutputableSignal {
    public string SignalName { get; }

    bool outputSignalValue = false;
    public bool OutputSignal => outputSignalValue;

    public event Action<bool, bool>? OnChangedSignalValue;

    public OutputSignalBit(string signalName, bool outputSignalValue = false) {
        SignalName = signalName;
        this.outputSignalValue = outputSignalValue;
    }

    public void SetOutputSignal(bool value) {
        var prevOutputValue = outputSignalValue;
        outputSignalValue = value;
        if (prevOutputValue != value) {
            OnChangedSignalValue?.Invoke(prevOutputValue, value);
        }
    }
}

public class SignalBit : IIOSignal {
    public string SignalName { get; }

    bool outputSignalValue = false;
    bool inputSignalValue = false;
    public bool OutputSignal => outputSignalValue;

    public event Action<bool, bool>? OnChangedSignalValue;

    public bool InputSignal {
        get => outputSignalValue || inputSignalValue;
        set => Update(outputSignalValue, value);
    }

    /// <summary>
    /// 入力の信号が変化したことを通知します。
    /// 以前の値と現在の値を渡します。
    /// </summary>
    readonly Action<bool, bool>? onChangedInputSignal;

    public SignalBit(string signalName, Action<bool, bool>? onChangedInputSignal, bool outputSignalValue = false) {
        SignalName = signalName;
        this.outputSignalValue = outputSignalValue;
        this.onChangedInputSignal = onChangedInputSignal;
    }

    public void SetOutputSignal(bool value) {
        Update(value, inputSignalValue);
    }

    void Update(bool newOutput, bool newInput) {
        var prevInputValue = InputSignal;
        var prevOutputValue = outputSignalValue;
        inputSignalValue = newInput;
        outputSignalValue = newOutput;
        // 出力信号も考慮して入力値が変化したかを判定する
        var currentInput = InputSignal;

        if (prevInputValue != currentInput) {
            onChangedInputSignal?.Invoke(prevInputValue, currentInput);
        }

        if (prevOutputValue != newOutput) {
            OnChangedSignalValue?.Invoke(prevOutputValue, newOutput);
        }
    }
}

/// <summary>
/// 信号の接続を行います。
/// 接続した出力が一つでも true になると入力は true になります。
/// すべての出力が false の場合 入力も false になります。
/// 接続の解除は Remove メソッドに登録した信号のインスタンスを渡すことで削除できます。
///
/// TODO 参照で比較するように明示的に記述する
/// </summary>
public class SignalLinker {
    readonly List<IOutputableSignal> outputSignals = new();
    readonly List<IInputableSignal> inputSignals = new();
    readonly Dictionary<IIOSignal, Action<bool, bool>> ioSignals = new();

    readonly Action<bool, bool> onChangedOutputSignal;

    public SignalLinker() {
        onChangedOutputSignal = (_, newOutput) => ApplySignals(null, newOutput);
    }

    public bool AddOutputSignal(IOutputableSignal signal) {
        if (outputSignals.Contains(signal)) {
            return false;
        }
        outputSignals.Add(signal);
        signal.OnChangedSignalValue += onChangedOutputSignal;
        ApplySignals(null, false);
        return true;
    }

    public bool AddInputSignal(IInputableSignal signal) {
        if (inputSignals.Contains(signal)) {
            return false;
        }
        inputSignals.Add(signal);
        // リンクしている信号の出力を反映させる
        ApplySignals(null, false);
        return true;
    }

    public bool AddIOSignal(IIOSignal signal) {
        Action<bool, bool> onChanged = (_, newOutput) => ApplySignals(signal, newOutput);
        if (!ioSignals.TryAdd(signal, onChanged)) {
            return false;
        }
        signal.OnChangedSignalValue += onChanged;
        ApplySignals(null, false);
        return true;
    }

    public bool RemoveOutputSignal(IOutputableSignal signal) {
        if (!outputSignals.Remove(signal)) {
            return false;
        }
        signal.OnChangedSignalValue -= onChangedOutputSignal;
        ApplySignals(null, false);
        return true;
    }

    public bool RemoveInputSignal(IInputableSignal signal) {
        if (!inputSignals.Remove(signal)) {
            return false;
        }
        ApplySignals(null, false);
        return true;
    }

    public bool RemoveIOSignal(IIOSignal signal) {
        if (!ioSignals.TryGetValue(signal, out var act)) {
            return false;
        }
        ioSignals.Remove(signal);
        signal.OnChangedSignalValue -= act;
        ApplySignals(null, false);
        return true;
    }

    void ApplySignals(IIOSignal? src, bool newOutput) {
        // 一つでも出力がtrueになっていた場合は、反映する入力はtrueにする。
        // 全てがfalseの場合はfalseにする
        bool applyValue = newOutput;
        if (!applyValue) {
            foreach (var output in outputSignals) {
                if (output.OutputSignal) {
                    applyValue = true;
                    break;
                }
            }
        }
        if (!applyValue) {
            foreach (var ioSignal in ioSignals.Keys) {
                if (ioSignal.OutputSignal) {
                    applyValue = true;
                    break;
                }
            }
        }
        foreach (var input in inputSignals) {
            input.InputSignal = applyValue;
        }
        foreach (var ioSignal in ioSignals.Keys) {
            // 自分自身には反映させない
            if (src != ioSignal) {
                ioSignal.InputSignal = applyValue;
            }
        }
    }
}
