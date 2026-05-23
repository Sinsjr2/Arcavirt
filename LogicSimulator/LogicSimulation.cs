using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LogicSimulator;

public enum CircuitErrorKind {
    UnconnectedInput,
    MultipleSourceConnections,
    BitWidthMismatch,
    UnresolvableConnector,
    InvalidNodeReference,
    JunctionBitSumMismatch,
}

public record CircuitError(
    string NodeId,
    string? PinName,
    CircuitErrorKind Kind,
    string Message
);

/// <summary>
/// 
/// </summary>
/// <param name="LogicID">配線する時に接続する素子を識別するID</param>
/// <param name="LogicData"></param>
public record LogicNode(string LogicID, ILogicElement LogicData);
public record LogicConnector(string LogicID, string PinName);
public record LogicConnection(LogicConnector Source, LogicConnector Target);

public record LogicNode<T>(string LogicID, T LogicData);

/// <summary>
/// 回路の素子とその回路の接続を表します。
/// </summary>
public record Circuit(IReadOnlyList<LogicNode> LogicNodes, IReadOnlyList<LogicConnection> LogicConnections);

public record PinDefinition(string PinName, int BitSize);

public record IOConnectorDefinition(
    string LogicID,
    IReadOnlyList<PinDefinition> InputPins,
    IReadOnlyList<PinDefinition> OutputPins);

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

public record ConstValueLogic(int BitLength, ulong Value) : ILogicElement {
    public static ConstValueLogic FromBool(bool v) {
        return new ConstValueLogic(1, v ? 1UL : 0UL);
    }

    public static ConstValueLogic FromU8(int bitLength, byte v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI8(int bitLength, sbyte v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    public static ConstValueLogic FromU16(int bitLength, ushort v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI16(int bitLength, short v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    public static ConstValueLogic FromU32(int bitLength, uint v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI32(int bitLength, int v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    public static ConstValueLogic FromU64(int bitLength, ulong v) {
        ValidateUnsigned(bitLength, v);
        return new ConstValueLogic(bitLength, v);
    }

    public static ConstValueLogic FromI64(int bitLength, long v) {
        ValidateSigned(bitLength, v);
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        return new ConstValueLogic(bitLength, (ulong)v & mask);
    }

    static void ValidateUnsigned(int bitLength, ulong v) {
        if (bitLength <= 0 || bitLength > 64) {
            throw new ArgumentException($"bitLength must be between 1 and 64, but was {bitLength}.");
        }
        ulong mask = bitLength == 64 ? ulong.MaxValue : (1UL << bitLength) - 1;
        if ((v & ~mask) != 0) {
            throw new ArgumentException($"Value {v} does not fit in {bitLength} bits.");
        }
    }

    static void ValidateSigned(int bitLength, long v) {
        if (bitLength <= 0 || bitLength > 64) {
            throw new ArgumentException($"bitLength must be between 1 and 64, but was {bitLength}.");
        }
        if (bitLength < 64) {
            long min = -(1L << (bitLength - 1));
            long max = (1L << (bitLength - 1)) - 1;
            if (v < min || v > max) {
                throw new ArgumentException($"Value {v} does not fit in {bitLength} signed bits.");
            }
        }
    }
}

/// <summary>
/// 回路一式をコンポーネントとして使い回す時に外部と接続するための入力コネクタ。
/// <para>
/// <see cref="DataBits"/> を省略した場合は、<see cref="LogicSimulation.TryBuild"/> 時に
/// 接続先ピンのビット幅から自動推論されます。
/// 明示指定した場合は接続先ピンとの整合性チェックが行われ、不一致があれば
/// <see cref="CircuitErrorKind.BitWidthMismatch"/> エラーとして報告されます。
/// </para>
/// </summary>
public record InputConnector : ILogicElement {
    /// <summary>
    /// データのビット幅。<see langword="null"/> の場合は回路ビルド時に接続先ピンから自動推論されます。
    /// </summary>
    public int? DataBits { get; }
    public InputConnector(int? dataBits = null) {
        if (dataBits.HasValue && dataBits.Value <= 0)
            throw new ArgumentException(
                $"InputConnector DataBits must be greater than 0, but was {dataBits.Value}.");
        DataBits = dataBits;
    }
}

/// <summary>
/// 回路一式をコンポーネントとして使い回す時に外部と接続するための出力コネクタ。
/// <para>
/// <see cref="DataBits"/> を省略した場合は、<see cref="LogicSimulation.TryBuild"/> 時に
/// 接続元ピンのビット幅から自動推論されます。
/// 明示指定した場合は接続元ピンとの整合性チェックが行われ、不一致があれば
/// <see cref="CircuitErrorKind.BitWidthMismatch"/> エラーとして報告されます。
/// </para>
/// </summary>
public record OutputConnector : ILogicElement {
    /// <summary>
    /// データのビット幅。<see langword="null"/> の場合は回路ビルド時に接続元ピンから自動推論されます。
    /// </summary>
    public int? DataBits { get; }
    public OutputConnector(int? dataBits = null) {
        if (dataBits.HasValue && dataBits.Value <= 0)
            throw new ArgumentException(
                $"OutputConnector DataBits must be greater than 0, but was {dataBits.Value}.");
        DataBits = dataBits;
    }
}

/// <summary>
/// ユーザーが作成した回路を名前で呼び出します。
/// </summary>
public record CustomCircuit(string TargetCircuitName) : ILogicElement;

public interface ILogicExecutor {
    void Execute(LogicPinReader inputs, LogicPinsWriter outputs);
}

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

/// <summary>
/// 複数の素子の複数のピンをあわらします。
/// </summary>
public struct LogicPins {
    /// <summary>
    /// ピンの信号
    /// </summary>
    public LogicSignal[] Pins;

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
    public LogicSignal ReadBit(int logicNumber, int pinNumber) {
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
    public bool WriteBit(int logicNumber, int pinNumber, LogicSignal value) {
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
    public LogicSignal ReadBit(int logicNumber, int pinNumber) {
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
    public LogicSignal ReadBit(int logicNumber, int pinNumber) {
        return targetPins.ReadBit(logicNumber, pinNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBit(int logicNumber, int pinNumber, LogicSignal value) {
        if (targetPins.WriteBit(logicNumber, pinNumber, value)) {
            valueChangedPins.Add(targetPins.GetPinIndex(logicNumber, pinNumber));
        }
    }
}

/// <summary>
/// AND論理素子。全入力がHighのときのみ出力がHigh、それ以外はLow。
/// 1つでもLow入力があれば他の入力の不定(X)に関わらず出力はLow。
/// Low がなくX が混在する場合は出力がX になる。入力数は任意に指定可能。
/// </summary>
public class AndLogicExecutorFactory : ILogicExecutorFactory<AndLogic> {
    class AndExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool hasUnknownSignal = false;
                bool shouldProcess = true;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var signal = inputs.ReadBit(logicNo, i);
                    switch (signal) {
                        case LogicSignal.Low:
                            // 1つでもLow があった場合は、入力が不定でも出力は常にLow
                            outputs.WriteBit(logicNo, 0, LogicSignal.Low);
                            shouldProcess = false;
                            break;
                        case LogicSignal.High:// 計算する必要は無いが、最適化(ルックアップテーブル生成)のために設けている
                            break;
                        case LogicSignal.X:
                            hasUnknownSignal = true;
                            break;
                    }
                }
                if (shouldProcess) {
                    var output = hasUnknownSignal ? LogicSignal.X : LogicSignal.High;
                    outputs.WriteBit(logicNo, 0, output);
                }
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

/// <summary>
/// OR論理素子。1つでもHigh入力があれば出力がHigh、全Low入力のときのみ出力がLow。
/// 1つでもHigh入力があれば他の入力の不定(X)に関わらず出力はHigh。
/// High がなくX が混在する場合は出力がX になる。入力数は任意に指定可能。
/// </summary>
public class OrLogicExecutorFactory : ILogicExecutorFactory<OrLogic> {
    class OrLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool hasUnknownSignal = false;
                bool shouldProcess = true;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var signal = inputs.ReadBit(logicNo, i);
                    switch (signal) {
                        case LogicSignal.Low: // 計算する必要は無いが、最適化(ルックアップテーブル生成)のために設けている
                            break;
                        case LogicSignal.High:
                            // 1つでもHigh があった場合は、入力が不定でも出力は常にHigh
                            outputs.WriteBit(logicNo, 0, LogicSignal.High);
                            shouldProcess = false;
                            break;
                        case LogicSignal.X:
                            hasUnknownSignal = true;
                            break;
                    }
                }
                if (shouldProcess) {
                    var output = hasUnknownSignal ? LogicSignal.X : LogicSignal.Low;  
                    outputs.WriteBit(logicNo, 0, output);
                }
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

/// <summary>
/// NOT論理素子。入力を反転して出力する（High → Low、Low → High）。
/// 不定入力(X)はそのままX として出力する。入力・出力は各1ビット。
/// </summary>
public class NotLogicExecutorFactory : ILogicExecutorFactory<NotLogic> {
    class NotLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                var input = inputs.ReadBit(logicNo, 0);
                var output = input switch {
                    LogicSignal.Low => LogicSignal.High,
                    LogicSignal.High => LogicSignal.Low,
                    _ => input
                };
                outputs.WriteBit(logicNo, 0, output);
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

/// <summary>
/// NAND論理素子。全入力がHighのときのみ出力がLow、それ以外はHigh。
/// 1つでもLow入力があれば他の入力の不定(X)に関わらず出力はHigh。
/// Low がなくX が混在する場合は出力がX になる。入力数は任意に指定可能。
/// </summary>
public class NAndLogicExecutorFactory : ILogicExecutorFactory<NAndLogic> {
    class NAndLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool hasUnknownSignal = false;
                bool shouldProcess = true;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var signal = inputs.ReadBit(logicNo, i);
                    switch (signal) {
                        case LogicSignal.Low:
                            // 1つでもLow があった場合は、入力が不定でも出力は常にHigh
                            outputs.WriteBit(logicNo, 0, LogicSignal.High);
                            shouldProcess = false;
                            break;
                        case LogicSignal.High:// 計算する必要は無いが、最適化(ルックアップテーブル生成)のために設けている
                            break;
                        case LogicSignal.X:
                            hasUnknownSignal = true;
                            break;
                    }
                }
                if (shouldProcess) {
                    var output = hasUnknownSignal ? LogicSignal.X : LogicSignal.Low;
                    outputs.WriteBit(logicNo, 0, output);
                }
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

/// <summary>
/// NOR論理素子。全入力がLowのときのみ出力がHigh、それ以外はLow。
/// 1つでもHigh入力があれば他の入力の不定(X)に関わらず出力はLow。
/// High がなくX が混在する場合は出力がX になる。入力数は任意に指定可能。
/// </summary>
public class NOrLogicExecutorFactory : ILogicExecutorFactory<NOrLogic> {
    class NOrLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool hasUnknownSignal = false;
                bool shouldProcess = true;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var signal = inputs.ReadBit(logicNo, i);
                    switch (signal) {
                        case LogicSignal.Low: // 計算する必要は無いが、最適化(ルックアップテーブル生成)のために設けている
                            break;
                        case LogicSignal.High:
                            // 1つでもHigh があった場合は、入力が不定でも出力は常にLow
                            outputs.WriteBit(logicNo, 0, LogicSignal.Low);
                            shouldProcess = false;
                            break;
                        case LogicSignal.X:
                            hasUnknownSignal = true;
                            break;
                    }
                }
                if (shouldProcess) {
                    var output = hasUnknownSignal ? LogicSignal.X : LogicSignal.High;  
                    outputs.WriteBit(logicNo, 0, output);
                }
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

/// <summary>
/// XOR論理素子。入力のうち奇数個がHighのとき出力がHigh、偶数個のときLow。
/// いずれかの入力が不定(X)であれば、他の入力値に関わらず出力は常にX になる。
/// 入力数は任意に指定可能。
/// </summary>
public class XOrLogicExecutorFactory : ILogicExecutorFactory<XOrLogic> {
    class XOrLogicExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
                int length = inputs.GetPinsLength(logicNo);
                bool shouldProcess = true;
                bool result = false;
                for (int i = 0; shouldProcess && i < length; i++) {
                    var input = inputs.ReadBit(logicNo, i);
                    bool boolSignal;
                    switch (input) {
                        case LogicSignal.Low:
                            boolSignal = false;
                            break;
                        case LogicSignal.High:
                            boolSignal = true;
                            break;
                        case LogicSignal.X:
                        default:
                            outputs.WriteBit(logicNo, 0, input);
                            shouldProcess = false;
                            continue;
                    }
                    result ^= boolSignal;
                }
                if (shouldProcess) {
                    outputs.WriteBit(logicNo, 0, result ? LogicSignal.High : LogicSignal.Low);
                }
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


public class ConstValueLogicExecutorFactory : ILogicExecutorFactory<ConstValueLogic> {
    class ConstValueLogicExecutor(LogicNode<ConstValueLogic>[] nodes) : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
            for (int i = 0; i < nodes.Length; i++) {
                var cv = nodes[i].LogicData;
                for (int bit = 0; bit < cv.BitLength; bit++) {
                    outputs.WriteBit(i, bit, ((cv.Value >> bit) & 1) != 0
                        ? LogicSignal.High : LogicSignal.Low);
                }
            }
        }
    }

    public IOConnectorDefinition GetConnectorDefinition(LogicNode<ConstValueLogic> node) {
        var outPins = Enumerable.Range(0, node.LogicData.BitLength)
            .Select(i => new PinDefinition($"out[{i}]", 1))
            .ToArray();
        return new IOConnectorDefinition(node.LogicID, [], outPins);
    }

    public ILogicExecutor CreateExecutor(LogicNode<ConstValueLogic>[] nodes, Action onInputChangedNotify) {
        onInputChangedNotify();
        return new ConstValueLogicExecutor(nodes);
    }
}

public class LogicSimulation {
    ExecutorContext[] executorContexts = null!;

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

    Dictionary<string, Dictionary<string, (int executorIndex, int logicNumberInExecutor, int pinIndex)>> logicIdAndPinNameToPinIndex = null!;

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

    class NoOpExecutor : ILogicExecutor {
        public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) { }
    }

    class TargetConnection {
        public int LogicTypeNumber;
        public int[] PinNumbers;

        public TargetConnection(int logicTypeNumber, int[] pinNumbers) {
            LogicTypeNumber = logicTypeNumber;
            PinNumbers = pinNumbers;
        }
    }

    LogicSimulation() { }

public static bool TryBuild(Circuit circuit, [NotNullWhen(true)] out LogicSimulation? simulation, out CircuitError[] errors, IReadOnlyDictionary<string, Circuit>? circuitLibrary = null) {
        var instance = new LogicSimulation();
        var (result, errorList) = instance.BuildCore(circuit, CreateDefaultFactories(), circuitLibrary);
        if (errorList.Length > 0) {
            simulation = null;
            errors = errorList;
            return false;
        }
        simulation = result!;
        errors = errorList;
        return true;
    }

    static Dictionary<Type, ILogicExecutorFactory> CreateDefaultFactories() {
        return new Dictionary<Type, ILogicExecutorFactory> {
            { typeof(AndLogic), new LogicExecutorFactory<AndLogic>(new AndLogicExecutorFactory()) },
            { typeof(OrLogic), new LogicExecutorFactory<OrLogic>(new OrLogicExecutorFactory()) },
            { typeof(NotLogic), new LogicExecutorFactory<NotLogic>(new NotLogicExecutorFactory()) },
            { typeof(NAndLogic), new LogicExecutorFactory<NAndLogic>(new NAndLogicExecutorFactory()) },
            { typeof(NOrLogic), new LogicExecutorFactory<NOrLogic>(new NOrLogicExecutorFactory()) },
            { typeof(XOrLogic), new LogicExecutorFactory<XOrLogic>(new XOrLogicExecutorFactory()) },
            { typeof(ConstValueLogic), new LogicExecutorFactory<ConstValueLogic>(new ConstValueLogicExecutorFactory()) },
        };
    }

    (LogicSimulation? simulation, CircuitError[] errors) BuildCore(
        Circuit circuit,
        Dictionary<Type, ILogicExecutorFactory> factories,
        IReadOnlyDictionary<string, Circuit>? circuitLibrary) {

        IReadOnlyList<LogicNode> nodes = circuit.LogicNodes;
        IReadOnlyList<LogicConnection> connections = circuit.LogicConnections;
        if (circuitLibrary != null) {
            var expanded = ExpandCustomCircuits(circuitLibrary, new Circuit(nodes, connections));
            nodes = expanded.LogicNodes;
            connections = expanded.LogicConnections;
        }

        var pinWidthMap = BuildPinWidthMap(nodes, factories);
        var (resolvedBits, resolveErrors) = ResolveConnectorBits(nodes, connections, pinWidthMap);
        var executorAndDefinitionsList = BuildExecutorContexts(nodes, factories, resolvedBits);
        var validateErrors = ValidateInputConnections(executorAndDefinitionsList, connections);

        var allErrors = new List<CircuitError>(resolveErrors.Count + validateErrors.Count);
        allErrors.AddRange(resolveErrors);
        allErrors.AddRange(validateErrors);
        if (allErrors.Count > 0) {
            return (null, allErrors.ToArray());
        }

        logicIdAndPinNameToPinIndex = BuildPinIndex(executorAndDefinitionsList);
        ResolveConnections(connections);
        return (this, []);
    }

    /// <summary>
    /// ノードリストをファクトリでグループ化し、各グループの <see cref="ExecutorContext"/> を生成します。
    /// <para>
    /// 同じ型の素子は1つの <see cref="ExecutorContext"/> を共有します（logicNumberInExecutor で識別）。
    /// GroupBy の列挙順は非決定的なため、LogicID でソートして logicNumberInExecutor を安定させます。
    /// </para>
    /// <para>
    /// CreateExecutor のコールバック（onInputChangedNotify）が呼ばれた場合、
    /// その素子は構築時点で出力値が確定しているため（例: ConstValueLogic）、
    /// <see cref="inputValueChangedExecutorIndexes"/> に登録して初期実行を予約します。
    /// </para>
    /// </summary>
    /// <returns>2パス目で logicIdAndPinNameToPinIndex を構築するために使用するリスト。</returns>
    List<(ExecutorContext ctx, IOConnectorDefinition[] defs)> BuildExecutorContexts(
        IReadOnlyList<LogicNode> nodes,
        Dictionary<Type, ILogicExecutorFactory> factories,
        IReadOnlyDictionary<string, int> resolvedBits) {
        var executorList = new List<ExecutorContext>();
        var executorAndDefinitionsList = new List<(ExecutorContext ctx, IOConnectorDefinition[] defs)>();
        var noOp = new NoOpExecutor();

        void AddConnectorGroup(LogicNode[] logicNodes, IOConnectorDefinition[] definitions) {
            var inputPinNumberToLogicNumber = new List<int>();
            var outputPinNumberToLogicNumber = new List<int>();
            var inputNumOfPins = new List<(int arrayOffset, int length)>();
            var outputNumOfPins = new List<(int arrayOffset, int length)>();

            for (int i = 0; i < logicNodes.Length; i++) {
                var def = definitions[i];

                var inputLength = def.InputPins.Sum(p => p.BitSize);
                var inputOffset = inputPinNumberToLogicNumber.Count;
                inputNumOfPins.Add((inputOffset, inputLength));
                inputPinNumberToLogicNumber.AddRange(Enumerable.Repeat(i, inputLength));

                var outputLength = def.OutputPins.Sum(p => p.BitSize);
                var outputOffset = outputPinNumberToLogicNumber.Count;
                outputNumOfPins.Add((outputOffset, outputLength));
                outputPinNumberToLogicNumber.AddRange(Enumerable.Repeat(i, outputLength));
            }

            var inputs = new LogicPins {
                NumOfPins = inputNumOfPins.ToArray(),
                Pins = [.. Enumerable.Repeat(LogicSignal.X, inputPinNumberToLogicNumber.Count)],
                PinNumberToLogicNumber = inputPinNumberToLogicNumber.ToArray()
            };
            var outputs = new LogicPins {
                NumOfPins = outputNumOfPins.ToArray(),
                Pins = [.. Enumerable.Repeat(LogicSignal.X, outputPinNumberToLogicNumber.Count)],
                PinNumberToLogicNumber = outputPinNumberToLogicNumber.ToArray()
            };

            var executorContext = new ExecutorContext(
                noOp,
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

        var inputConnectorNodes = nodes
            .Where(n => n.LogicData is InputConnector)
            .OrderBy(n => n.LogicID)
            .ToArray();
        if (inputConnectorNodes.Length > 0) {
            var inputConnectorDefs = inputConnectorNodes
                .Select(n => new IOConnectorDefinition(n.LogicID, [], [new PinDefinition("out", resolvedBits.GetValueOrDefault(n.LogicID, 1))]))
                .ToArray();
            AddConnectorGroup(inputConnectorNodes, inputConnectorDefs);
        }

        var outputConnectorNodes = nodes
            .Where(n => n.LogicData is OutputConnector)
            .OrderBy(n => n.LogicID)
            .ToArray();
        if (outputConnectorNodes.Length > 0) {
            var outputConnectorDefs = outputConnectorNodes
                .Select(n => new IOConnectorDefinition(n.LogicID, [new PinDefinition("in", resolvedBits.GetValueOrDefault(n.LogicID, 1))], []))
                .ToArray();
            AddConnectorGroup(outputConnectorNodes, outputConnectorDefs);
        }

        foreach (var group in nodes
            .Where(n => n.LogicData is not InputConnector and not OutputConnector)
            .GroupBy(node => node.LogicData.GetType())) {
            if (!factories.TryGetValue(group.Key, out var factory)) {
                continue;
            }

            // GroupBy の列挙順は非決定的なため、logicNumberInExecutor が実行ごとに変わらないよう LogicID でソートする
            var logicNodes = group.OrderBy(n => n.LogicID).ToArray();

            // CreateExecutor のコールバック（onInputChangedNotify）が呼ばれたかどうかで初期実行の要否を判定する
            // ConstValueLogic など構築時点で出力値が確定している素子はコールバックを呼ぶ
            bool needsInitialExecution = false;
            var executor = factory.CreateExecutor(logicNodes, () => { needsInitialExecution = true; });
            var definitions = logicNodes.Select(node => factory.GetConnectorDefinition(node)).ToArray();

            var inputPinNumberToLogicNumber = new List<int>();
            var outputPinNumberToLogicNumber = new List<int>();
            var inputNumOfPins = new List<(int arrayOffset, int length)>();
            var outputNumOfPins = new List<(int arrayOffset, int length)>();

            for (int i = 0; i < logicNodes.Length; i++) {
                var def = definitions[i];

                var inputLength = def.InputPins.Sum(p => p.BitSize);
                // このグループ内で累積された入力ピン数（= このノードの入力ピンが始まるオフセット）
                var inputOffset = inputPinNumberToLogicNumber.Count;
                inputNumOfPins.Add((inputOffset, inputLength));
                inputPinNumberToLogicNumber.AddRange(Enumerable.Repeat(i, inputLength));

                var outputLength = def.OutputPins.Sum(p => p.BitSize);
                // このグループ内で累積された出力ピン数（= このノードの出力ピンが始まるオフセット）
                var outputOffset = outputPinNumberToLogicNumber.Count;
                outputNumOfPins.Add((outputOffset, outputLength));
                outputPinNumberToLogicNumber.AddRange(Enumerable.Repeat(i, outputLength));
            }

            var inputs = new LogicPins {
                NumOfPins = inputNumOfPins.ToArray(),
                Pins = [.. Enumerable.Repeat(LogicSignal.X, inputPinNumberToLogicNumber.Count)],
                PinNumberToLogicNumber = inputPinNumberToLogicNumber.ToArray()
            };
            var outputs = new LogicPins {
                NumOfPins = outputNumOfPins.ToArray(),
                Pins = [.. Enumerable.Repeat(LogicSignal.X, outputPinNumberToLogicNumber.Count)],
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
            if (needsInitialExecution) {
                inputValueChangedExecutorIndexes.Add(executorList.Count - 1);
            }
        }

        // ExecutorContext の配列を確定してから BuildPinIndex で Array.IndexOf を使うため、ここで確定させる
        executorContexts = executorList.ToArray();
        return executorAndDefinitionsList;
    }

    /// <summary>
    /// 各素子の LogicID とピン名から ExecutorContext 内のピンインデックスへのマッピングを構築します。
    /// <para>
    /// <see cref="BuildExecutorContexts"/> で executorContexts が確定した後に呼び出す必要があります。
    /// Array.IndexOf で executorContexts 内の位置を特定するため、2パスに分離しています。
    /// </para>
    /// </summary>
    Dictionary<string, Dictionary<string, (int executorIndex, int logicNumberInExecutor, int pinIndex)>> BuildPinIndex(
        List<(ExecutorContext ctx, IOConnectorDefinition[] defs)> executorAndDefinitionsList) {
        var pinIndex = new Dictionary<string, Dictionary<string, (int executorIndex, int logicNumberInExecutor, int pinIndex)>>();

        foreach (var (ctx, definitions) in executorAndDefinitionsList) {
            var execIdx = Array.IndexOf(executorContexts, ctx);
            for (int logicNumInExec = 0; logicNumInExec < definitions.Length; logicNumInExec++) {
                var def = definitions[logicNumInExec];
                pinIndex[def.LogicID] = new Dictionary<string, (int executorIndex, int logicNumberInExecutor, int pinIndex)>();

                int currentInputPinOffset = 0;
                foreach (var pin in def.InputPins) {
                    pinIndex[def.LogicID][pin.PinName] = (execIdx, logicNumInExec, ctx.Inputs.GetPinIndex(logicNumInExec, currentInputPinOffset));
                    currentInputPinOffset += pin.BitSize;
                }
                int currentOutputPinOffset = 0;
                foreach (var pin in def.OutputPins) {
                    pinIndex[def.LogicID][pin.PinName] = (execIdx, logicNumInExec, ctx.Outputs.GetPinIndex(logicNumInExec, currentOutputPinOffset));
                    currentOutputPinOffset += pin.BitSize;
                }
            }
        }

        return pinIndex;
    }

    /// <summary>
    /// 接続リストを解決し、各出力ピンから接続先入力ピンへの <see cref="ExecutorContext.OutputToInputPinConnections"/> を構築します。
    /// 接続定義に不正な LogicID またはピン名が含まれる場合は例外をスローします。
    /// </summary>
    void ResolveConnections(IReadOnlyList<LogicConnection> connections) {
        foreach (var connection in connections) {
            var sourceLogicID = connection.Source.LogicID;
            var sourcePinName = connection.Source.PinName;
            var targetLogicID = connection.Target.LogicID;
            var targetPinName = connection.Target.PinName;

            if (!logicIdAndPinNameToPinIndex.TryGetValue(sourceLogicID, out var sourcePins)) {
                throw new ArgumentException($"Connection source '{sourceLogicID}' is not defined in the circuit.");
            }
            if (!sourcePins.ContainsKey(sourcePinName)) {
                throw new ArgumentException($"Connection source '{sourceLogicID}' does not have pin '{sourcePinName}'.");
            }
            if (!logicIdAndPinNameToPinIndex.TryGetValue(targetLogicID, out var targetPins)) {
                throw new ArgumentException($"Connection target '{targetLogicID}' is not defined in the circuit.");
            }
            if (!targetPins.ContainsKey(targetPinName)) {
                throw new ArgumentException($"Connection target '{targetLogicID}' does not have pin '{targetPinName}'.");
            }

            var sourcePinInfo = sourcePins[sourcePinName];
            var targetPinInfo = targetPins[targetPinName];

            executorContexts[sourcePinInfo.executorIndex].OutputToInputPinConnections[sourcePinInfo.pinIndex].Add(
                new TargetConnection(targetPinInfo.executorIndex, new int[] { targetPinInfo.pinIndex })
            );
        }
    }

    /// <summary>
    /// 通常素子（InputConnector/OutputConnector 以外）のピン幅マップを構築します。
    /// </summary>
    static Dictionary<string, Dictionary<string, int>> BuildPinWidthMap(
        IReadOnlyList<LogicNode> nodes,
        Dictionary<Type, ILogicExecutorFactory> factories) {

        var pinWidthMap = new Dictionary<string, Dictionary<string, int>>();
        foreach (var node in nodes) {
            if (node.LogicData is InputConnector or OutputConnector) {
                continue;
            }
            if (!factories.TryGetValue(node.LogicData.GetType(), out var factory)) {
                continue;
            }
            var def = factory.GetConnectorDefinition(node);
            var pinWidths = new Dictionary<string, int>();
            foreach (var pin in def.InputPins) {
                pinWidths[pin.PinName] = pin.BitSize;
            }
            foreach (var pin in def.OutputPins) {
                pinWidths[pin.PinName] = pin.BitSize;
            }
            pinWidthMap[node.LogicID] = pinWidths;
        }
        return pinWidthMap;
    }

    /// <summary>
    /// InputConnector/OutputConnector のビット幅を接続グラフから推論し、確定します。
    /// <para>
    /// DataBits が明示されている場合は接続との整合性チェックを行います。
    /// DataBits が null の場合は接続先/接続元のピン幅から推論します。
    /// </para>
    /// </summary>
    static (IReadOnlyDictionary<string, int> resolvedBits, IReadOnlyList<CircuitError> errors) ResolveConnectorBits(
        IReadOnlyList<LogicNode> nodes,
        IReadOnlyList<LogicConnection> connections,
        IReadOnlyDictionary<string, Dictionary<string, int>> pinWidthMap) {

        var connectorNodes = nodes
            .Where(n => n.LogicData is InputConnector or OutputConnector)
            .ToDictionary(n => n.LogicID);

        var resolved = new Dictionary<string, int>();

        // 明示指定済みコネクタのビット幅を確定
        foreach (var node in connectorNodes.Values) {
            if (node.LogicData is InputConnector ic && ic.DataBits.HasValue) {
                resolved[node.LogicID] = ic.DataBits.Value;
            } else if (node.LogicData is OutputConnector oc && oc.DataBits.HasValue) {
                resolved[node.LogicID] = oc.DataBits.Value;
            }
        }

        var errors = new List<CircuitError>();
        // 接続グラフを走査してコネクタのビット幅を反復解決（null は推論、明示指定は整合性チェック）
        var checkedExplicit = new HashSet<string>();
        bool madeProgress = true;
        while (madeProgress) {
            madeProgress = false;
            foreach (var node in connectorNodes.Values) {
                var id = node.LogicID;
                bool isExplicit = resolved.ContainsKey(id);
                if (isExplicit && checkedExplicit.Contains(id)) {
                    continue;
                }
                if (node.LogicData is InputConnector inputConnector) {
                    var outgoingConnections = connections
                        .Where(c => c.Source.LogicID == id)
                        .ToArray();
                    if (outgoingConnections.Length == 0) {
                        if (isExplicit) {
                            checkedExplicit.Add(id);
                        }
                        continue;
                    }
                    var widths = new List<int>();
                    bool allResolved = true;
                    foreach (var conn in outgoingConnections) {
                        var targetId = conn.Target.LogicID;
                        var targetPin = conn.Target.PinName;
                        if (connectorNodes.TryGetValue(targetId, out var targetNode)) {
                            if (targetNode.LogicData is OutputConnector && resolved.TryGetValue(targetId, out var w)) {
                                widths.Add(w);
                            } else {
                                allResolved = false;
                            }
                        } else if (pinWidthMap.TryGetValue(targetId, out var targetPinMap)
                            && targetPinMap.TryGetValue(targetPin, out var pinWidth)) {
                            widths.Add(pinWidth);
                        } else {
                            allResolved = false;
                        }
                    }
                    if (!allResolved || widths.Count == 0) {
                        continue;
                    }
                    if (widths.Distinct().Count() > 1) {
                        errors.Add(new CircuitError(id, null, CircuitErrorKind.BitWidthMismatch,
                            $"InputConnector '{id}' has conflicting bit widths: {widths[0]} and {widths.First(w => w != widths[0])}."));
                        checkedExplicit.Add(id);
                        continue;
                    }
                    var inferredWidth = widths[0];
                    if (inputConnector.DataBits.HasValue && inputConnector.DataBits.Value != inferredWidth) {
                        errors.Add(new CircuitError(id, null, CircuitErrorKind.BitWidthMismatch,
                            $"InputConnector '{id}' DataBits={inputConnector.DataBits.Value} does not match connected pin bit width {inferredWidth}."));
                        checkedExplicit.Add(id);
                        continue;
                    }
                    if (!isExplicit) {
                        resolved[id] = inferredWidth;
                        madeProgress = true;
                    } else {
                        checkedExplicit.Add(id);
                    }
                } else if (node.LogicData is OutputConnector outputConnector) {
                    var incomingConns = connections.Where(c => c.Target.LogicID == id).ToArray();
                    if (incomingConns.Length == 0) {
                        if (isExplicit) {
                            checkedExplicit.Add(id);
                        }
                        continue;
                    }
                    // 複数ソースがある場合はどれか1つを選ぶ（エラーは ValidateInputConnections で報告）
                    var incomingConn = incomingConns[0];
                    var sourceId = incomingConn.Source.LogicID;
                    var sourcePin = incomingConn.Source.PinName;
                    int? inferredWidth = null;
                    if (connectorNodes.TryGetValue(sourceId, out var sourceNode)) {
                        if (sourceNode.LogicData is InputConnector && resolved.TryGetValue(sourceId, out var w)) {
                            inferredWidth = w;
                        }
                    } else if (pinWidthMap.TryGetValue(sourceId, out var sourcePinMap)
                        && sourcePinMap.TryGetValue(sourcePin, out var pinWidth)) {
                        inferredWidth = pinWidth;
                    }
                    if (!inferredWidth.HasValue) {
                        continue;
                    }
                    if (outputConnector.DataBits.HasValue && outputConnector.DataBits.Value != inferredWidth.Value) {
                        errors.Add(new CircuitError(id, null, CircuitErrorKind.BitWidthMismatch,
                            $"OutputConnector '{id}' DataBits={outputConnector.DataBits.Value} does not match connected pin bit width {inferredWidth.Value}."));
                        checkedExplicit.Add(id);
                        continue;
                    }
                    if (!isExplicit) {
                        resolved[id] = inferredWidth.Value;
                        madeProgress = true;
                    } else {
                        checkedExplicit.Add(id);
                    }
                }
            }
        }

        // 解決できなかったコネクタをエラーとして収集
        foreach (var node in connectorNodes.Values) {
            if (resolved.ContainsKey(node.LogicID)) {
                continue;
            }
            var id = node.LogicID;
            if (node.LogicData is InputConnector) {
                bool hasOutgoing = connections.Any(c => c.Source.LogicID == id);
                if (!hasOutgoing) {
                    errors.Add(new CircuitError(id, null, CircuitErrorKind.UnresolvableConnector,
                        $"InputConnector '{id}' has no connection. Cannot infer bit width."));
                } else {
                    errors.Add(new CircuitError(id, null, CircuitErrorKind.UnresolvableConnector,
                        $"InputConnector '{id}' cannot resolve bit width: only connected to unresolved connectors."));
                }
            } else if (node.LogicData is OutputConnector) {
                bool hasIncoming = connections.Any(c => c.Target.LogicID == id);
                if (!hasIncoming) {
                    errors.Add(new CircuitError(id, null, CircuitErrorKind.UnresolvableConnector,
                        $"OutputConnector '{id}' has no connection. Cannot infer bit width."));
                } else {
                    errors.Add(new CircuitError(id, null, CircuitErrorKind.UnresolvableConnector,
                        $"OutputConnector '{id}' cannot resolve bit width: only connected to unresolved connectors."));
                }
            }
        }

        return (resolved, errors);
    }

    /// <summary>
    /// 全素子の入力ピン未接続チェックおよび複数ソース接続チェックを行います。
    /// </summary>
    static IReadOnlyList<CircuitError> ValidateInputConnections(
        IReadOnlyList<(ExecutorContext ctx, IOConnectorDefinition[] defs)> executorAndDefinitionsList,
        IReadOnlyList<LogicConnection> connections) {

        var errors = new List<CircuitError>();

        var inputPinSourceCount = new Dictionary<(string nodeId, string pinName), int>();
        foreach (var conn in connections) {
            var key = (conn.Target.LogicID, conn.Target.PinName);
            inputPinSourceCount.TryGetValue(key, out var cnt);
            inputPinSourceCount[key] = cnt + 1;
        }

        foreach (var (_, defs) in executorAndDefinitionsList) {
            foreach (var def in defs) {
                foreach (var pin in def.InputPins) {
                    var key = (def.LogicID, pin.PinName);
                    inputPinSourceCount.TryGetValue(key, out var cnt);
                    if (cnt == 0) {
                        errors.Add(new CircuitError(def.LogicID, pin.PinName,
                            CircuitErrorKind.UnconnectedInput,
                            $"Input pin '{pin.PinName}' of '{def.LogicID}' has no source connection."));
                    } else if (cnt > 1) {
                        errors.Add(new CircuitError(def.LogicID, pin.PinName,
                            CircuitErrorKind.MultipleSourceConnections,
                            $"Input pin '{pin.PinName}' of '{def.LogicID}' has {cnt} source connections, but only 1 is allowed."));
                    }
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// CustomCircuit を含む回路をフラット化・コネクタ透過・ノード整理して展開します。
    /// </summary>
    Circuit ExpandCustomCircuits(
        IReadOnlyDictionary<string, Circuit> circuitLibrary,
        Circuit originalCircuit) {
        var (expandedNodes, expandedConnections) = FlattenCircuit(circuitLibrary, originalCircuit);
        var resultConnections = SolveConnectors(expandedNodes, expandedConnections);
        var resultNodes = CleanupNodes(expandedNodes);
        return new Circuit(resultNodes, resultConnections);
    }

    /// <summary>
    /// 全ノード・接続を prefix 付きでフラット化します。CustomCircuit を再帰的に展開します。
    /// isTop フラグはトップレベル回路（level == 0）のノード・接続であることを示します。
    /// </summary>
    (Dictionary<string, (bool isTop, LogicNode node)> nodes,
     List<(bool isTop, LogicConnection connection)> connections)
    FlattenCircuit(IReadOnlyDictionary<string, Circuit> circuitLibrary, Circuit originalCircuit) {
        var expandedNodes = new Dictionary<string, (bool isTop, LogicNode node)>();
        var expandedConnections = new List<(bool isTop, LogicConnection connection)>();

        void Flatten(string prefix, Circuit circuit, int level) {
            // 接続する名前も展開する回路の名前をつけてユニークにする
            foreach (var connection in circuit.LogicConnections) {
                var source = connection.Source;
                var target = connection.Target;
                expandedConnections.Add((level == 0, new LogicConnection(
                    source with { LogicID = prefix + source.LogicID },
                    target with { LogicID = prefix + target.LogicID })));
            }
            foreach (var node in circuit.LogicNodes) {
                var newLogicID = prefix + node.LogicID;
                var newNode = node with { LogicID = newLogicID };
                expandedNodes[newLogicID] = (level == 0, newNode);
                if (node.LogicData is CustomCircuit customCircuit) {
                    // CustomCircuitノードの場合、内部回路を展開
                    var targetCircuitName = customCircuit.TargetCircuitName;
                    if (!circuitLibrary.TryGetValue(targetCircuitName, out var circuitDef)) {
                        throw new ArgumentException($"Circuit '{targetCircuitName}' not found in library");
                    }
                    // ネストを示すプリフィックス
                    var idPrefix = $"{prefix}{node.LogicID}.";
                    Flatten(idPrefix, circuitDef, level + 1);
                }
            }
        }

        Flatten("", originalCircuit, 0);
        return (expandedNodes, expandedConnections);
    }

    /// <summary>
    /// コネクタ（InputConnector/OutputConnector）を透過して、実際の接続（通常素子 ↔ トップレベルコネクタ）に変換します。
    /// 接続のソースは1つしか接続されないことを前提としています（このメソッドが呼ばれるよりも先にエラー検知で弾いていること）。
    /// </summary>
    List<LogicConnection> SolveConnectors(
        Dictionary<string, (bool isTop, LogicNode node)> expandedNodes,
        List<(bool isTop, LogicConnection connection)> expandedConnections) {
        // 計算量を減らすために辞書にして接続先を高速で検索できるようにする
        var groupedSourceConnections = expandedConnections
            .GroupBy(x =>
                expandedNodes[x.connection.Source.LogicID].node.LogicData is OutputConnector or InputConnector
                ? x.connection.Source.LogicID
                : $"{x.connection.Source.LogicID}.{x.connection.Source.PinName}")
            .ToDictionary(x => x.Key, x => x.ToArray());

        var alreadyConnectedSourceConnectorNames = new HashSet<string>();
        var resultTargetConnectors = new List<LogicConnector>();
        var resultConnections = new List<LogicConnection>();

        // トップレベルの InputConnector と OutputConnector は残す
        foreach (var connection in expandedConnections) {
            if (!expandedNodes.TryGetValue(connection.connection.Source.LogicID, out var sourceNode)) {
                throw new ArgumentException($"logic ID not found: '{connection.connection.Source.LogicID}'");
            }
            if (sourceNode.node.LogicData is CustomCircuit ||
                (!connection.isTop && sourceNode.node.LogicData is InputConnector or OutputConnector)) {
                continue;
            }
            resultTargetConnectors.Clear();
            alreadyConnectedSourceConnectorNames.Clear();
            FindTargetConnections(expandedNodes, groupedSourceConnections, alreadyConnectedSourceConnectorNames, connection.connection.Target, resultTargetConnectors);
            foreach (var targetConnector in resultTargetConnectors) {
                resultConnections.Add(new LogicConnection(connection.connection.Source, targetConnector));
            }
        }
        return resultConnections;
    }

    /// <summary>
    /// 指定された接続ターゲットから、コネクタを再帰的に辿って実際の接続先をリストに追加します。
    /// </summary>
    void FindTargetConnections(
        Dictionary<string, (bool isTop, LogicNode node)> expandedNodes,
        Dictionary<string, (bool isTop, LogicConnection connection)[]> groupedSourceConnections,
        HashSet<string> skipSourceConnectorNames,
        LogicConnector outputConnector,
        List<LogicConnector> resultConnections) {
        if (!expandedNodes.TryGetValue(outputConnector.LogicID, out var sourceNode)) {
            throw new ArgumentException($"logic ID not found: '{outputConnector}'");
        }
        if (sourceNode.node.LogicData is not OutputConnector and not InputConnector and not CustomCircuit) {
            resultConnections.Add(outputConnector);
            return;
        }
        // すでに処理済みのノードはスキップ
        // CustomCircuit は "CC:" プレフィックスと PinName を加えてスキップキーとする。
        // こうすることで同じ CustomCircuit の異なるピン（J と K など）を独立して処理でき、
        // かつ内部の InputConnector のスキップキー（"LogicID" 形式）と競合しない。
        var skipKey = sourceNode.node.LogicData is CustomCircuit
            ? $"CC:{outputConnector.LogicID}:{outputConnector.PinName}"
            : outputConnector.LogicID;
        if (!skipSourceConnectorNames.Add(skipKey)) {
            return;
        }
        if (!expandedNodes.TryGetValue(outputConnector.LogicID, out var targetNode)) {
            throw new ArgumentException($"logic ID not found: '{outputConnector}'");
        }
        // 一番外側の回路の場合は、出力用のコネクタを残す
        if (targetNode.isTop) {
            if (targetNode.node.LogicData is OutputConnector) {
                resultConnections.Add(outputConnector);
                return;
            }
        }
        if (targetNode.node.LogicData is OutputConnector or InputConnector) {
            if (groupedSourceConnections.TryGetValue(outputConnector.LogicID, out var targetConnections)) {
                foreach (var targetConnection in targetConnections) {
                    FindTargetConnections(
                        expandedNodes,
                        groupedSourceConnections,
                        skipSourceConnectorNames,
                        targetConnection.connection.Target,
                        resultConnections);
                }
            }
        } else if (targetNode.node.LogicData is CustomCircuit) {
            FindTargetConnections(
                expandedNodes,
                groupedSourceConnections,
                skipSourceConnectorNames,
                new LogicConnector($"{outputConnector.LogicID}.{outputConnector.PinName}", ""),
                resultConnections);
        } else {
            resultConnections.Add(outputConnector);
        }
    }

    /// <summary>
    /// CustomCircuit ノードと、トップレベル以外の InputConnector/OutputConnector ノードを削除します。
    /// </summary>
    LogicNode[] CleanupNodes(Dictionary<string, (bool isTop, LogicNode node)> expandedNodes) {
        return expandedNodes
            .Where(x =>
                x.Value.node.LogicData is not CustomCircuit &&
                (x.Value.isTop || x.Value.node.LogicData is not InputConnector and not OutputConnector))
            .Select(x => x.Value.node)
            .ToArray();
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
                    if (0 < ctx.ValueChangedInputPins.Count || ctx.Inputs.Pins.Length == 0) {
                        ctx.IsExecutedLogicNumbers.AsSpan().Clear();
                        ctx.Executor.Execute(
                            new LogicPinReader(ctx.Inputs, ctx.ValueChangedInputPins, ctx.IsExecutedLogicNumbers, 0),
                            new LogicPinsWriter(ctx.Outputs, ctx.ValueChangedOutputPins));
                        ctx.ValueChangedInputPins.Clear();
                        // Executor の出力が変化した場合、その出力の伝播処理を実行する必要がある
                        if (!ctx.ShouldCopy && 0 < ctx.ValueChangedOutputPins.Count) {
                            ctx.ShouldCopy = true;
                            outputValueChangedExecutorIndexes.Add(changedLogicNo);
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
    public void SetInput(string inputConnectorLogicID, int pinNumber, LogicSignal value) {
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
    public LogicSignal GetOutput(string outputConnectorLogicID, int pinNumber) {
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