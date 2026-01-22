using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LogicSimulator;

/// <summary>
/// 
/// </summary>
/// <param name="LogicID">配線する時に接続する素子を識別するID</param>
/// <param name="LogicData"></param>
public record LogicNode(string LogicID, ILogicElement LogicData);
public record LogicConnector(string LogicID, string PinName);
public record LogicConnection(LogicConnector Source, LogicConnector Target);

public record LogicNode<T>(string LogicID, T LogicData);

public record PinDefinition(string PinName, int BitSize);

public record IOConnectorDefinition(string LogicID, IReadOnlyList<PinDefinition> InputPins, IReadOnlyList<PinDefinition> OutputPins);

public interface ILogicElement;

/// <summary>
/// 素子の構成データから演算用オブジェクトを生成します。
/// </summary>
public interface ILogicExecutorFactory;

public interface ILogicExecutorFactory<T> : ILogicExecutorFactory {
    IOConnectorDefinition GetConnectorDefinition(LogicNode<T> logic);
    ILogicExecutor CreateExecutor(LogicNode<T>[] nodes, Action onInputChangedNotify);
}

public record AndLogic(int NumOfInputs) : ILogicElement;
public class NotLogic : ILogicElement;
public record OrLogic(int NumOfInputs) : ILogicElement;
public record NAndLogic(int NumOfInputs) : ILogicElement;
public record NOrLogic(int NumOfInputs) : ILogicElement;
public record XOrLogic(int NumOfInputs) : ILogicElement;

/// <summary>
/// プリセット・クリア端子付きのJK-FF論理素子
/// </summary>
public class JK_FF_PresetClear : ILogicElement;

/// <summary>
/// 入力ピンと出力ピンをまとめたり分割したりして、素子間の線の接続本数を減らします。
/// </summary>
public record Splitter(IReadOnlyList<int> InputSplits, IReadOnlyList<int> OutputSplits) : ILogicElement;

/// <summary>
/// 回路一式をコンポートとして使い回しする時に外部と接続するための入力コネクタ
/// </summary>
public record InputConnector(int DataBits) : ILogicElement;

/// <summary>
/// 回路一式をコンポートとして使い回しする時に外部と接続するための出力コネクタ
/// </summary>
public record OutputConnector(int DataBits) : ILogicElement;

/// <summary>
/// MCUといったプログラムを書き込み動作するICを名前で指定します。
/// 外部からプログラムを指定する際は、パスを指定して読み込むファイルを決定します。
/// <see cref="CustomCircuit"/>で読み込まれた対象に本クラスが含まれているとパスが自動的に追加されます。
/// </summary>
public record ProgramableIC(string IcName) : ILogicElement;

/// <summary>
/// ユーザーが作成した回路を名前で呼び出します。
/// </summary>
public record CustomCircuit(string TargetCircuitName) : ILogicElement;

public class Light : ILogicElement;

public record Button(bool ActiveLow) : ILogicElement;

public record ToggleSwitch(bool OutputIsHigh) : ILogicElement;

/// <summary>
/// 指定したビット幅でデータを出力します。
/// 下の桁が下のビットに対応します。
/// </summary>
public record ConstantValue(int DataBits, ulong Value) : ILogicElement;

/// <summary>
/// string.Formatによりビット列を数値としてデコードし表示します。
/// </summary>
public record NumberDecoder(string Format) : ILogicElement;

public interface ILogicExecutor {
    void Execute(LogicPinReader inputs, LogicPinsWriter outputs);
}

/// <summary>
/// 複数の素子の複数のピンをあわらします。
/// </summary>
public struct LogicPins {
    public bool[] Pins;

    /// <summary>
    /// ピン番号から素子を識別できる番号に変換するためのテーブル
    /// </summary>
    public int[] PinNumberToLogicNumber;

    /// <summary>
    /// 各同じ素子の入力もしくは出力のピン数を表します。
    /// ピン情報にアクセスするために配列のオフセットとそこからの長さを保持しています。
    /// </summary>
    public (int arrayOffset, int length)[] NumOfPins;

    /// <summary>
    /// ピンの配列のインデックスを返します。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPinIndex(int logicNumber, int pinNumber) {
        var num = NumOfPins[logicNumber];
        if (num.length <= pinNumber) {
            throw new IndexOutOfRangeException($"length: {num.length}, actual: {pinNumber}");
        }
        return num.arrayOffset + pinNumber;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPinsLength(int logicNumber) {
        return NumOfPins[logicNumber].length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBit(int logicNumber, int pinNumber) {
        var num = NumOfPins[logicNumber];
        if (num.length <= pinNumber) {
            throw new IndexOutOfRangeException($"length: {num.length}, actual: {pinNumber}");
        }
        var index = num.arrayOffset + pinNumber;
        return Pins[index];
    }

    /// <summary>
    /// 今回書き込んだ値と変化したかどうかを返します。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool WriteBit(int logicNumber, int pinNumber, bool value) {
        var num = NumOfPins[logicNumber];
        if (num.length <= pinNumber) {
            throw new IndexOutOfRangeException($"length: {num.length}, actual: {pinNumber}");
        }
        var index = num.arrayOffset + pinNumber;
        var currentValue = Pins[index];
        bool isChanged = currentValue != value;
        if (isChanged) {
            Pins[index] = value;
        }
        return isChanged;
    }
}

/// <summary>
/// 入力ピンが変化したもののみを抽出します。
/// </summary>
public struct LogicPinReader {
    LogicPins targetPins;
    List<int> changedPins;

    /// <summary>
    /// 素子の演算は、その素子のいずれかの入力ピンが1つでも変化した場合に、
    /// 一度だけ行うことで演算回数を減らせる
    /// </summary>
    bool[] isExecutedLogicNumbers;

    int pos;

    public LogicPinReader(LogicPins targetPins, List<int> changedPins, bool[] isExecutedLogicNumbers, int pos) {
        this.targetPins = targetPins;
        this.changedPins = changedPins;
        this.isExecutedLogicNumbers = isExecutedLogicNumbers;
        this.pos = pos;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPinsLength(int logicNumber) {
        return targetPins.GetPinsLength(logicNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBit(int logicNumber, int pinNumber) {
        return targetPins.ReadBit(logicNumber, pinNumber);
    }

    /// <summary>
    /// 入力が変化した素子の番号を次々と返します。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetNextChangedLogicNumber([NotNullWhen(true)] out int logicNumber) {
        while (true) {
            if (changedPins.Count <= pos) {
                logicNumber = 0;
                return false;
            }
            var logicNo = targetPins.PinNumberToLogicNumber[changedPins[pos]];
            pos++;
            if (!isExecutedLogicNumbers[logicNo]) {
                isExecutedLogicNumbers[logicNo] = true;
                logicNumber = logicNo;
                return true;
            }
        }
    }
}

public struct LogicPinsWriter {
    LogicPins targetPins;
    List<int> valueChangedPins;

    public LogicPinsWriter(LogicPins targetPins, List<int> valueChangedPins) {
        this.targetPins = targetPins;
        this.valueChangedPins = valueChangedPins;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPinsLength(int logicNumber) {
        return targetPins.GetPinsLength(logicNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBit(int logicNumber, int pinNumber) {
        return targetPins.ReadBit(logicNumber, pinNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBit(int logicNumber, int pinNumber, bool value) {
        if (targetPins.WriteBit(logicNumber, pinNumber, value)) {
            valueChangedPins.Add(targetPins.GetPinIndex(logicNumber, pinNumber));
        }
    }
}

public class AndExecutor : ILogicExecutor {
    readonly LogicNode<AndLogic>[] datas;

    public IReadOnlyList<IOConnectorDefinition> GetPinDefinitions() {
        return datas.Select(x =>
            new IOConnectorDefinition(
                x.LogicID,
                Enumerable.Range(0, x.LogicData.NumOfInputs).Select(i => new PinDefinition($"in[{i}]", 1)).ToArray(),
                [ new PinDefinition("out", 1) ]))
            .ToArray();
    }

    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            int length = inputs.GetPinsLength(logicNo);
            bool isAllTrue = true;
            for (int i = 0; i < length; i++) {
                if (!inputs.ReadBit(logicNo, i)) {
                    isAllTrue = false;
                    break;
                }
            }
            outputs.WriteBit(logicNo, 0, isAllTrue);
        }
    }
}

public class OrExecutor : ILogicExecutor {
    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            int length = inputs.GetPinsLength(logicNo);
            bool isAnyTrue = false;
            for (int i = 0; i < length; i++) {
                if (inputs.ReadBit(logicNo, i)) {
                    isAnyTrue = true;
                    break;
                }
            }
            outputs.WriteBit(logicNo, 0, isAnyTrue);
        }
    }
}

public class NotExecutor : ILogicExecutor {
    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            outputs.WriteBit(logicNo, 0, !inputs.ReadBit(logicNo, 0));
        }
    }
}

public class NAndExecutor : ILogicExecutor {
    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            int length = inputs.GetPinsLength(logicNo);
            bool result = false;
            for (int i = 0; i < length; i++) {
                if (!inputs.ReadBit(logicNo, i)) {
                    result = true;
                    break;
                }
            }
            outputs.WriteBit(logicNo, 0, result);
        }
    }
}

public class NOrExecutor : ILogicExecutor {
    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            int length = inputs.GetPinsLength(logicNo);
            bool result = true;
            for (int i = 0; i < length; i++) {
                if (inputs.ReadBit(logicNo, i)) {
                    result = false;
                    break;
                }
            }
            outputs.WriteBit(logicNo, 0, result);
        }
    }
}

public class XOrExecutor : ILogicExecutor {
    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            int length = inputs.GetPinsLength(logicNo);
            bool result = false;
            for (int i = 0; i < length; i++) {
                result ^= inputs.ReadBit(logicNo, i);
            }
            outputs.WriteBit(logicNo, 0, result);
        }
    }
}

public class JK_FF_PresetClearExecutor : ILogicExecutor {
    readonly LogicNode<JK_FF_PresetClear>[] datas;

    public IReadOnlyList<IOConnectorDefinition> GetPinDefinitions() {
        return datas.Select(x =>
            new IOConnectorDefinition(
                x.LogicID,
                [ new PinDefinition("preN", 1), new PinDefinition("j", 1), new PinDefinition("k", 1), new PinDefinition("clk", 1), new PinDefinition("clrN", 1)],
                [ new PinDefinition("q", 1), new PinDefinition("qN", 1)]))
            .ToArray();
    }

    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            bool preN = inputs.ReadBit(logicNo, 0);
            bool j = inputs.ReadBit(logicNo, 1);
            bool k = inputs.ReadBit(logicNo, 2);
            bool clk = inputs.ReadBit(logicNo, 3);
            bool clrN = inputs.ReadBit(logicNo, 4);

            bool prevQ = outputs.ReadBit(logicNo, 0);

            var newQ = !clrN && (
                ((j && clk) ? 0b001 : 0) |
                ((k && clk) ? 0b010 : 0) |
                (prevQ ? 0b100 : 0)) switch {
                    0b000 => false,
                    0b001 => true,
                    0b010 => false,
                    0b011 => false,
                    0b100 => true,
                    0b101 => true,
                    0b110 => true,
                    0b111 => false,
                    _ => prevQ
                };
            bool q = !preN || newQ;
            bool qN = !clrN || q;
            outputs.WriteBit(logicNo, 0, q);
            outputs.WriteBit(logicNo, 1, qN);
        }
    }
}

public class ConstantValueExecutor : ILogicExecutor {
    /// <summary>
    /// インデックスは素子の番号と一致します。
    /// 設定する定数
    /// </summary>
    readonly LogicNode<ConstantValue>[] datas;

    public IReadOnlyList<IOConnectorDefinition> GetPinDefinitions() {
        return datas.Select(x =>
            new IOConnectorDefinition(
                x.LogicID,
                [],
                [ new PinDefinition("out", 1)]))
            .ToArray();
    }

    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        for (int i = 0; i < datas.Length; i++) {
            var data = datas[i];
            for (int pinNo = 0; pinNo < data.LogicData.DataBits; pinNo++) {
                outputs.WriteBit(i, pinNo, ((data.LogicData.Value >> pinNo) & 1) != 0);
            }
        }
    }
}

public class LogicSimulation {
    ExecutorContext[] executorContexts;

    /// <summary>
    /// 入力が変化したので実行する必要があるexecutorの配列のインデックス
    /// 処理負荷軽減のために使用します。
    /// </summary>
    readonly List<int> inputValueChangedExecutorIndexes = [];

    /// <summary>
    /// 出力が変化したので入力にコピーする必要があるexecutorの配列のインデックス
    /// 処理負荷軽減のために使用します。
    /// </summary>
    readonly List<int> outputValueChangedExecutorIndexes = [];

    class ExecutorContext {
        public ILogicExecutor Executor;
        public LogicPins Inputs;
        public LogicPins Outputs;

        /// <summary>
        /// 入力ピンが変化し、通知する必要がない素子を識別するために使用します。
        /// 配列の要素数は素子の数と一致します。
        /// </summary>
        public bool[] IsExecutedLogicNumbers;

        /// <summary>
        /// この素子の入力ピンの内、値が変化したピン配列のインデックス
        /// </summary>
        public List<int> ValueChangedInputPins;

        /// <summary>
        /// この素子に対しての出力が変化したピンの配列のインデックス
        /// </summary>
        public List<int> ValueChangedOutputPins;

        /// <summary>
        /// この素子のピン配列のインデックスに一致します。
        /// 出力ピンから対象の素子の入力ピンへの接続を表します。
        /// ピンが接続されていないインデックスはnullになります。
        /// </summary>
        public TargetConnection?[] OutputToInputPinConnections;

        /// <summary>
        /// 出力が変化したので入力にコピーする必要があることを表します。
        /// </summary>
        public bool ShouldCopy;

        /// <summary>
        /// 入力が変化したので実行する必要があることを表します。
        /// 処理負荷軽減のために使用します。
        /// </summary>
        public bool ShouldExecute;

        public ExecutorContext(
            ILogicExecutor executor,
            LogicPins inputs,
            LogicPins outputs,
            bool[] isExecutedLogicNumbers,
            List<int> valueChangedInputPins,
            List<int> valueChangedOutputPins,
            TargetConnection?[] outputToInputPinConnections,
            bool shouldCopy,
            bool shouldExecute) {

            Executor = executor;
            Inputs = inputs;
            Outputs = outputs;
            IsExecutedLogicNumbers = isExecutedLogicNumbers;
            ValueChangedInputPins = valueChangedInputPins;
            ValueChangedOutputPins = valueChangedOutputPins;
            OutputToInputPinConnections = outputToInputPinConnections;
            ShouldCopy = shouldCopy;
            ShouldExecute = shouldExecute;
        }
    }

    class TargetConnection {
        public int LogicTypeNumber;
        public int[] PinNumbers;

        public TargetConnection(int logicTypeNumber, int[] pinNumbers) {
            LogicTypeNumber = logicTypeNumber;
            PinNumbers = pinNumbers;
        }
    }

    LogicSimulation(IReadOnlyList<LogicNode> nodes, IReadOnlyList<LogicConnection> connections) {
        var nameToNodeIndex = nodes.Select((node, i) => (node, i)).ToDictionary(t => t.node.LogicID, t => (t.i, t.node));
        
    }

    /// <summary>
    /// 全ての入力ピンにおいて変化したことをマークします。
    /// </summary>
    public void MarkAllInputPinChanged() {
        inputValueChangedExecutorIndexes.Clear();
        outputValueChangedExecutorIndexes.Clear();
        inputValueChangedExecutorIndexes.AddRange(Enumerable.Range(0, executorContexts.Length));
        outputValueChangedExecutorIndexes.AddRange(Enumerable.Range(0, executorContexts.Length));
        foreach (var ctx in executorContexts) {
            ctx.ValueChangedInputPins.Clear();
            ctx.ValueChangedInputPins.AddRange(Enumerable.Range(0, ctx.Inputs.Pins.Length));
        }
    }

    public void Step() {
        // 入力と出力が変化しなくなるまで繰り返す。
        for (int loopCount = 0; 0 < outputValueChangedExecutorIndexes.Count; loopCount++) {
            inputValueChangedExecutorIndexes.Clear();
            // 変化があった出力ピンの値を入力ピンに適用する
            foreach (var changedLogicNo in outputValueChangedExecutorIndexes) {
                var ctx = executorContexts[changedLogicNo];
                ctx.ShouldCopy = false;
                foreach (var changedPinNo in ctx.ValueChangedOutputPins) {
                    var outputToInputConnection = ctx.OutputToInputPinConnections[changedPinNo];
                    if (outputToInputConnection == null) {
                        continue;
                    }
                    var writeTargetLogic = executorContexts[outputToInputConnection.LogicTypeNumber];
                    var currentOutputValue = ctx.Outputs.Pins[changedPinNo];
                    foreach (var inputPinNo in outputToInputConnection.PinNumbers) {
                        var currentInputValue = writeTargetLogic.Inputs.Pins[inputPinNo];
                        if (currentInputValue != currentOutputValue) {
                            writeTargetLogic.Inputs.Pins[inputPinNo] = currentInputValue;
                            writeTargetLogic.ValueChangedInputPins.Add(inputPinNo);
                            // 入力が変化した素子で処理を実行することを通知する
                            if (!writeTargetLogic.ShouldExecute) {
                                writeTargetLogic.ShouldExecute = true;
                                inputValueChangedExecutorIndexes.Add(outputToInputConnection.LogicTypeNumber);
                            }
                        }
                    }
                }
                ctx.ValueChangedOutputPins.Clear();
            }
            outputValueChangedExecutorIndexes.Clear();
            // 入力が変化したことを検知し、出力を更新する
            foreach (var changedLogicNo in inputValueChangedExecutorIndexes) {
                var ctx = executorContexts[changedLogicNo];
                ctx.ShouldExecute = false;
                // 処理負荷軽減のため入力が変化していない場合は処理しない
                if (0 < ctx.ValueChangedInputPins.Count) {
                    ctx.IsExecutedLogicNumbers.AsSpan().Clear();
                    ctx.Executor.Execute(
                        new LogicPinReader(ctx.Inputs, ctx.ValueChangedInputPins, ctx.IsExecutedLogicNumbers, 0),
                        new LogicPinsWriter(ctx.Outputs, ctx.ValueChangedOutputPins));
                    ctx.ValueChangedInputPins.Clear();
                    foreach (var changedOutputPinNo in ctx.ValueChangedOutputPins) {
                        var connection = ctx.OutputToInputPinConnections[changedOutputPinNo];
                        if (connection == null) {
                            continue;
                        }
                        var logicTypeNo = connection.LogicTypeNumber;
                        var writeTarget = executorContexts[logicTypeNo];
                        if (!writeTarget.ShouldCopy) {
                            writeTarget.ShouldCopy = true;
                            outputValueChangedExecutorIndexes.Add(logicTypeNo);
                        }
                    }
                }
            }
        }
    }
}