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
public interface ILogicExecutorFactory {
    IOConnectorDefinition GetConnectorDefinition(LogicNode node);
    ILogicExecutor CreateExecutor(LogicNode[] nodes, Action onInputChangedNotify);    
}

public class LogicExecutorFactory<T>(
    ILogicExecutorFactory<T> factory
    ) : ILogicExecutorFactory {

    readonly ILogicExecutorFactory<T> factory = factory;

    public IOConnectorDefinition GetConnectorDefinition(LogicNode node) {
        return factory.GetConnectorDefinition(new LogicNode<T>(node.LogicID, (T)node.LogicData));
    }

    public ILogicExecutor CreateExecutor(LogicNode[] nodes, Action onInputChangedNotify) {
        var genericNodes = nodes.Select(node => new LogicNode<T>(node.LogicID, (T)node.LogicData)).ToArray();
        return factory.CreateExecutor(genericNodes, onInputChangedNotify);
    }
}

public interface ILogicExecutorFactory<T> {
    IOConnectorDefinition GetConnectorDefinition(LogicNode<T> node);
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

public class AndLogicExecutorFactory : ILogicExecutorFactory<AndLogic> {
    class AndExecutor : ILogicExecutor {
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

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<AndLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            Enumerable.Range(0, node.LogicData.NumOfInputs).Select(i => new PinDefinition($"in[{i}]", 1)).ToArray(),
            [new PinDefinition("out", 1)]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<AndLogic>[] nodes, Action onInputChangedNotify) {
        return new AndExecutor();
    }
}

public class OrLogicExecutorFactory : ILogicExecutorFactory<OrLogic> {
    class OrLogicExecutor : ILogicExecutor {
    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            int length = inputs.GetPinsLength(logicNo);
            bool result = false;
            for (int i = 0; i < length; i++) {
                if (inputs.ReadBit(logicNo, i)) {
                    result = true;
                    break;
                }
            }
            outputs.WriteBit(logicNo, 0, result);
        }
    }
}

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<OrLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [.. Enumerable.Range(0, node.LogicData.NumOfInputs).Select(i => new PinDefinition($"in[{i}]", 1))],
            [new PinDefinition("out", 1)]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<OrLogic>[] nodes, Action onInputChangedNotify) {
        return new OrLogicExecutor();
    }
}

public class NotLogicExecutorFactory : ILogicExecutorFactory<NotLogic> {
    class NotLogicExecutor : ILogicExecutor {
    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
            bool input = inputs.ReadBit(logicNo, 0);
            outputs.WriteBit(logicNo, 0, !input);
        }
    }
}

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<NotLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [new PinDefinition("in", 1)],
            [new PinDefinition("out", 1)]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<NotLogic>[] nodes, Action onInputChangedNotify) {
        return new NotLogicExecutor();
    }
}

public class NAndLogicExecutorFactory : ILogicExecutorFactory<NAndLogic> {
    class NAndLogicExecutor : ILogicExecutor {
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
            outputs.WriteBit(logicNo, 0, !isAllTrue);
        }
    }
}

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<NAndLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            Enumerable.Range(0, node.LogicData.NumOfInputs).Select(i => new PinDefinition($"in[{i}]", 1)).ToArray(),
            [new PinDefinition("out", 1)]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<NAndLogic>[] nodes, Action onInputChangedNotify) {
        return new NAndLogicExecutor();
    }
}

public class NOrLogicExecutorFactory : ILogicExecutorFactory<NOrLogic> {
    class NOrLogicExecutor : ILogicExecutor {
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
            outputs.WriteBit(logicNo, 0, !isAnyTrue);
        }
    }
}

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<NOrLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            Enumerable.Range(0, node.LogicData.NumOfInputs).Select(i => new PinDefinition($"in[{i}]", 1)).ToArray(),
            [new PinDefinition("out", 1)]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<NOrLogic>[] nodes, Action onInputChangedNotify) {
        return new NOrLogicExecutor();
    }
}

public class XOrLogicExecutorFactory : ILogicExecutorFactory<XOrLogic> {
    class XOrLogicExecutor : ILogicExecutor {
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

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<XOrLogic> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            Enumerable.Range(0, node.LogicData.NumOfInputs).Select(i => new PinDefinition($"in[{i}]", 1)).ToArray(),
            [new PinDefinition("out", 1)]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<XOrLogic>[] nodes, Action onInputChangedNotify) {
        return new XOrLogicExecutor();
    }
}

public class InputConnectorExecutor : ILogicExecutor {
    readonly LogicNode<InputConnector>[] datas;
    readonly Action onInputChangedNotify;

    public InputConnectorExecutor(LogicNode<InputConnector>[] datas, Action onInputChangedNotify) {
        this.datas = datas;
        this.onInputChangedNotify = onInputChangedNotify;
    }

    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        // 入力コネクタは入力がないため、何も処理しない
    }

    /// <summary>
    /// 入力コネクタの出力ピンに値を設定します。
    /// </summary>
    public void SetInputValue(LogicNode<InputConnector>[] nodes, LogicPins outputs, int logicNumberInExecutor, int pinNumber, bool value) {
        var logicNode = nodes[logicNumberInExecutor];
        var globalPinIndex = outputs.GetPinIndex(logicNumberInExecutor, pinNumber);
        
        if (outputs.WriteBit(logicNumberInExecutor, pinNumber, value)) {
            // 値が変化した場合、変更を通知
            onInputChangedNotify?.Invoke();
        }
    }
}

public class InputConnectorExecutorFactory : ILogicExecutorFactory<InputConnector> {
    public IOConnectorDefinition GetConnectorDefinition(LogicNode<InputConnector> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [],
            [ new PinDefinition("out", node.LogicData.DataBits) ]
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<InputConnector>[] nodes, Action onInputChangedNotify) {
        return new InputConnectorExecutor(nodes, onInputChangedNotify);
    }
}

public class OutputConnectorExecutor : ILogicExecutor {
    readonly LogicNode<OutputConnector>[] datas;
    readonly Action onInputChangedNotify;

    public OutputConnectorExecutor(LogicNode<OutputConnector>[] datas, Action onInputChangedNotify) {
        this.datas = datas;
        this.onInputChangedNotify = onInputChangedNotify;
    }

    public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
        // 出力コネクタは自身の出力を持たないため、何も処理しない。入力が外部への出力となる。
    }

    public IReadOnlyList<IOConnectorDefinition> GetPinDefinitions() {
        return datas.Select(x =>
            new IOConnectorDefinition(
                x.LogicID,
                [ new PinDefinition("in", x.LogicData.DataBits) ],
                [] // 出力ピンなし
            ))
            .ToArray();
    }
}

public class OutputConnectorExecutorFactory : ILogicExecutorFactory<OutputConnector> {
    public IOConnectorDefinition GetConnectorDefinition(LogicNode<OutputConnector> node) {
        return new IOConnectorDefinition(
            node.LogicID,
            [ new PinDefinition("in", node.LogicData.DataBits) ],
            []
        );
    }

    public ILogicExecutor CreateExecutor(LogicNode<OutputConnector>[] nodes, Action onInputChangedNotify) {
        return new OutputConnectorExecutor(nodes, onInputChangedNotify);
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

    readonly Dictionary<string, Dictionary<string, (int executorIndex, int logicNumberInExecutor, int pinIndex)>> logicIdAndPinNameToPinIndex;

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
        /// ピンが接続されていないインデックスは空のリストになります。
        /// 複数の入力ピンへの接続をサポートしています。
        /// </summary>
        public List<TargetConnection>[] OutputToInputPinConnections;

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
            List<TargetConnection>[] outputToInputPinConnections,
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

    public LogicSimulation(IReadOnlyList<LogicNode> nodes, IReadOnlyList<LogicConnection> connections) 
        : this(nodes, connections, new Dictionary<Type, ILogicExecutorFactory> {
            { typeof(AndLogic), new LogicExecutorFactory<AndLogic>(new AndLogicExecutorFactory()) },
            { typeof(OrLogic), new LogicExecutorFactory<OrLogic>(new OrLogicExecutorFactory()) },
            { typeof(NotLogic), new LogicExecutorFactory<NotLogic>(new NotLogicExecutorFactory()) },
            { typeof(NAndLogic), new LogicExecutorFactory<NAndLogic>(new NAndLogicExecutorFactory()) },
            { typeof(NOrLogic), new LogicExecutorFactory<NOrLogic>(new NOrLogicExecutorFactory()) },
            { typeof(XOrLogic), new LogicExecutorFactory<XOrLogic>(new XOrLogicExecutorFactory()) },
            { typeof(InputConnector), new LogicExecutorFactory<InputConnector>(new InputConnectorExecutorFactory()) },
            { typeof(OutputConnector), new LogicExecutorFactory<OutputConnector>(new OutputConnectorExecutorFactory()) },
        }) {
    }

    public LogicSimulation(IReadOnlyList<LogicNode> nodes, IReadOnlyList<LogicConnection> connections, Dictionary<Type, ILogicExecutorFactory> factories) {
        var executorList = new List<ExecutorContext>();
        var groupedNodes = nodes.GroupBy(node => node.LogicData.GetType());

        // ピン名から相対ピンインデックスを取得するヘルパーを構築
        var executorAndDefinitionsList = new List<(ExecutorContext ctx, IOConnectorDefinition[] defs)>();

        // 各グループに対してExecutorContextを生成
        foreach (var group in groupedNodes) {
            if (factories.TryGetValue(group.Key, out var factory)) {
                var logicNodes = group.OrderBy(n => n.LogicID).ToArray();

                var executor = factory.CreateExecutor(logicNodes, () => { });
                var definitions = logicNodes.Select(node => factory.GetConnectorDefinition(node)).ToArray();

                var inputPinNumberToLogicNumber = new List<int>();
                var outputPinNumberToLogicNumber = new List<int>();

                var inputNumOfPins = new List<(int arrayOffset, int length)>();
                var outputNumOfPins = new List<(int arrayOffset, int length)>();

                for (int i = 0; i < logicNodes.Length; i++) {
                    var node = logicNodes[i];
                    var def = definitions[i];

                    // 入力ピンのオフセットと長さ
                    var inputLength = def.InputPins.Sum(p => p.BitSize);
                    var inputOffset = inputPinNumberToLogicNumber.Count;
                    inputNumOfPins.Add((inputOffset, inputLength));
                    inputPinNumberToLogicNumber.AddRange(Enumerable.Repeat(i, inputLength));

                    // 出力ピンのオフセットと長さ
                    var outputLength = def.OutputPins.Sum(p => p.BitSize);
                    var outputOffset = outputPinNumberToLogicNumber.Count;
                    outputNumOfPins.Add((outputOffset, outputLength));
                    outputPinNumberToLogicNumber.AddRange(Enumerable.Repeat(i, outputLength));
                }
                var inputs = new LogicPins {
                    NumOfPins = inputNumOfPins.ToArray(),
                    Pins = new bool[inputPinNumberToLogicNumber.Count],
                    PinNumberToLogicNumber = inputPinNumberToLogicNumber.ToArray()
                };

                var outputs = new LogicPins {
                    NumOfPins = outputNumOfPins.ToArray(),
                    Pins = new bool[outputPinNumberToLogicNumber.Count],
                    PinNumberToLogicNumber = outputPinNumberToLogicNumber.ToArray()
                };

                var executorContext = new ExecutorContext(
                    executor,
                    inputs,
                    outputs,
                    new bool[logicNodes.Length],
                    new List<int>(),
                    new List<int>(),
                    Enumerable.Range(0, outputPinNumberToLogicNumber.Count).Select(_ => new List<TargetConnection>()).ToArray(),
                    false,
                    false
                );
                executorList.Add(executorContext);
                executorAndDefinitionsList.Add((executorContext, definitions));
            }
        }

        executorContexts = executorList.ToArray();

        // LogicIDとPinNameからExecutorContext内のピンの全体インデックスへのマッピング
        logicIdAndPinNameToPinIndex = new Dictionary<string, Dictionary<string, (int executorIndex, int logicNumberInExecutor, int pinIndex)>>();

        foreach (var (ctx, definitions) in executorAndDefinitionsList) {
            for (int logicNumInExec = 0; logicNumInExec < definitions.Length; logicNumInExec++) {
                var def = definitions[logicNumInExec];
                var execIdx = Array.IndexOf(executorContexts, ctx);

                logicIdAndPinNameToPinIndex[def.LogicID] = new Dictionary<string, (int executorIndex, int logicNumberInExecutor, int pinIndex)>();

                int currentInputPinOffset = 0;
                foreach (var pin in def.InputPins) {
                    logicIdAndPinNameToPinIndex[def.LogicID][pin.PinName] = (execIdx, logicNumInExec, ctx.Inputs.GetPinIndex(logicNumInExec, currentInputPinOffset));
                    currentInputPinOffset += pin.BitSize;
                }
                int currentOutputPinOffset = 0;
                foreach (var pin in def.OutputPins) {
                    var outputPinIndex = ctx.Outputs.GetPinIndex(logicNumInExec, currentOutputPinOffset);
                    logicIdAndPinNameToPinIndex[def.LogicID][pin.PinName] = (execIdx, logicNumInExec, outputPinIndex);
                    currentOutputPinOffset += pin.BitSize;
                }
            }
        }

        foreach (var connection in connections) {
            var sourceLogicID = connection.Source.LogicID;
            var sourcePinName = connection.Source.PinName;
            var targetLogicID = connection.Target.LogicID;
            var targetPinName = connection.Target.PinName;

            if (!logicIdAndPinNameToPinIndex.TryGetValue(sourceLogicID, out var sourcePinIndex) || !sourcePinIndex.ContainsKey(sourcePinName)) {
                throw new ArgumentException($"Source pin {sourceLogicID}.{sourcePinName} not found.");
            }
            if (!logicIdAndPinNameToPinIndex.TryGetValue(targetLogicID, out var targetPinIndex) || !targetPinIndex.ContainsKey(targetPinName)) {
                throw new ArgumentException($"Target pin {targetLogicID}.{targetPinName} not found.");
            }

            var sourcePinInfo = logicIdAndPinNameToPinIndex[sourceLogicID][sourcePinName];
            var targetPinInfo = logicIdAndPinNameToPinIndex[targetLogicID][targetPinName];

            // SourceのExecutorContextのOutputToInputPinConnectionsにTargetの情報を追加
            var sourceExecutorContext = executorContexts[sourcePinInfo.executorIndex];
            var sourceGlobalPinIndex = sourcePinInfo.pinIndex; // これはOutputs.Pinsのグローバルインデックス

            // 複数接続に対応：既存の接続リストにTargetConnectionを追加
            sourceExecutorContext.OutputToInputPinConnections[sourceGlobalPinIndex].Add(
                new TargetConnection(targetPinInfo.executorIndex, new int[] { targetPinInfo.pinIndex })
            );
        }
        
        MarkAllInputPinChanged();
        Step();
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
        // 無限ループ対策
        int maxIteration = 10000;
        for (int loopCount = 0; 0 < outputValueChangedExecutorIndexes.Count || 0 < inputValueChangedExecutorIndexes.Count; loopCount++) {
            if (maxIteration <= loopCount) {
                // TODO 例外ではなく、戻り値でどの論理素子の接続で振動しているのかを返すようにする。
                // もしくは、単純に振動したことだけを返す。
                throw new InvalidOperationException("circuit oscillation");
            }

            // 出力の変化がある場合、入力に伝播
            if (0 < outputValueChangedExecutorIndexes.Count) {
                // 変化があった出力ピンの値を入力ピンに適用する
                foreach (var changedLogicNo in outputValueChangedExecutorIndexes) {
                    var ctx = executorContexts[changedLogicNo];
                    ctx.ShouldCopy = false;
                    foreach (var changedPinNo in ctx.ValueChangedOutputPins) {
                        var outputToInputConnections = ctx.OutputToInputPinConnections[changedPinNo];
                        if (outputToInputConnections.Count == 0) {
                            continue;
                        }
                        var currentOutputValue = ctx.Outputs.Pins[changedPinNo];

                        // 複数接続をサポート：各接続先に値を伝播
                        foreach (var outputToInputConnection in outputToInputConnections) {
                            var writeTargetLogic = executorContexts[outputToInputConnection.LogicTypeNumber];
                            foreach (var inputPinNo in outputToInputConnection.PinNumbers) {
                                var currentInputValue = writeTargetLogic.Inputs.Pins[inputPinNo];
                                if (currentInputValue != currentOutputValue) {
                                    writeTargetLogic.Inputs.Pins[inputPinNo] = currentOutputValue;
                                    writeTargetLogic.ValueChangedInputPins.Add(inputPinNo);
                                    // 入力が変化した素子で処理を実行することを通知する
                                    if (!writeTargetLogic.ShouldExecute) {
                                        writeTargetLogic.ShouldExecute = true;
                                        inputValueChangedExecutorIndexes.Add(outputToInputConnection.LogicTypeNumber);
                                    }
                                }
                            }
                        }
                    }
                    ctx.ValueChangedOutputPins.Clear();
                }
                outputValueChangedExecutorIndexes.Clear();
            }
            // 入力が変化したことを検知し、出力を更新する
            if (0 < inputValueChangedExecutorIndexes.Count) {
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
                        // Executor の出力が変化した場合、その出力の伝播処理を実行する必要がある
                        if (0 < ctx.ValueChangedOutputPins.Count && !ctx.ShouldCopy) {
                            ctx.ShouldCopy = true;
                            outputValueChangedExecutorIndexes.Add(changedLogicNo);
                        }
                        foreach (var changedOutputPinNo in ctx.ValueChangedOutputPins) {
                            // 複数接続先にコピーするように設定する
                            foreach (var connection in ctx.OutputToInputPinConnections[changedOutputPinNo]) {
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
                inputValueChangedExecutorIndexes.Clear();
            }
        }
    }

    /// <summary>
    /// InputConnector経由で入力値を設定します。
    /// </summary>
    public void SetInput(string inputConnectorLogicID, int pinNumber, bool value) {
        if (!logicIdAndPinNameToPinIndex.TryGetValue(inputConnectorLogicID, out var pinMap) || !pinMap.TryGetValue("out", out var inputPinInfo)) {
            throw new ArgumentException($"Input connector {inputConnectorLogicID} not found or does not have 'out' pin.");
        }

        var targetExecutorIndex = inputPinInfo.executorIndex;
        var targetLogicNumberInExecutor = inputPinInfo.logicNumberInExecutor;
        var ctx = executorContexts[targetExecutorIndex];
        var globalPinIndex = ctx.Outputs.GetPinIndex(targetLogicNumberInExecutor, pinNumber);

        // InputConnectorの出力ピンに値を書き込む
        if (ctx.Outputs.WriteBit(targetLogicNumberInExecutor, pinNumber, value)) {
            // 変更があった場合は、伝播処理を行う必要がある
            if (!outputValueChangedExecutorIndexes.Contains(targetExecutorIndex)) {
                outputValueChangedExecutorIndexes.Add(targetExecutorIndex);
            }
            // ValueChangedOutputPinsも更新する
            ctx.ValueChangedOutputPins.Add(globalPinIndex);
        }
    }

    /// <summary>
    /// OutputConnector経由で出力値を取得します。
    /// </summary>
    public bool GetOutput(string outputConnectorLogicID, int pinNumber) {
        // logicIdAndPinNameToPinIndex を使用して OutputConnector の入力ピンを特定
        // OutputConnectorは通常"in"ピンを持つ
        if (!logicIdAndPinNameToPinIndex.TryGetValue(outputConnectorLogicID, out var pinMap) || !pinMap.TryGetValue("in", out var outputPinInfo)) {
            throw new ArgumentException($"Output connector {outputConnectorLogicID} not found or does not have 'in' pin.");
        }

        var targetExecutorIndex = outputPinInfo.executorIndex;
        var targetLogicNumberInExecutor = outputPinInfo.logicNumberInExecutor;
        var ctx = executorContexts[targetExecutorIndex];

        // OutputConnectorの入力ピンの値を読み取る
        return ctx.Inputs.ReadBit(targetLogicNumberInExecutor, pinNumber);
    }
}