using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using LogicSimulator.Gates;
using LogicSimulator.Runtime;

namespace LogicSimulator;

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

/// <summary>
/// 回路定義からシミュレーションを構築します。
/// 構築に成功した場合は <see langword="true"/> を返し <paramref name="simulation"/> に値を設定します。
/// 失敗した場合は <see langword="false"/> を返し <paramref name="errors"/> にエラーの一覧を設定します。
/// </summary>
/// <param name="circuit">構築する回路の定義。</param>
/// <param name="simulation">構築に成功した場合のシミュレーションインスタンス。失敗時は <see langword="null"/>。</param>
/// <param name="errors">検出されたエラーの配列。成功時は空配列。</param>
/// <param name="circuitLibrary">CustomCircuit の展開に使用する回路ライブラリ。</param>
/// <remarks>
/// <para>このメソッドが返す <see cref="CircuitError"/> の <see cref="CircuitErrorKind"/> は以下の通りです。</para>
/// <list type="table">
///   <listheader>
///     <term>ErrorKind</term>
///     <description>発生条件</description>
///   </listheader>
///   <item>
///     <term><see cref="CircuitErrorKind.DuplicateNodeId"/></term>
///     <description>同じ LogicID を持つノードが複数登録されている。</description>
///   </item>
///   <item>
///     <term><see cref="CircuitErrorKind.InvalidNodeReference"/></term>
///     <description>接続定義に存在しない LogicID・ピン名が含まれているか、回路ライブラリに未登録の CustomCircuit 名が指定されている。</description>
///   </item>
///   <item>
///     <term><see cref="CircuitErrorKind.UnregisteredLogicElement"/></term>
///     <description>回路に含まれる素子の型が、登録済みのファクトリに存在しない。</description>
///   </item>
///   <item>
///     <term><see cref="CircuitErrorKind.BitWidthMismatch"/></term>
///     <description>接続されているピン同士のビット幅が一致しない。</description>
///   </item>
///   <item>
///     <term><see cref="CircuitErrorKind.UnresolvableConnector"/></term>
///     <description>InputConnector/OutputConnector のビット幅を接続から推論できない。</description>
///   </item>
///   <item>
///     <term><see cref="CircuitErrorKind.UnconnectedInput"/></term>
///     <description>入力ピンが未接続のままになっている。</description>
///   </item>
///   <item>
///     <term><see cref="CircuitErrorKind.MultipleSourceConnections"/></term>
///     <description>1つの入力ピンに複数の出力ピンが接続されている。</description>
///   </item>
/// </list>
/// </remarks>
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

        var dupErrors = ValidateDuplicateNodeIds(circuit);

        IReadOnlyList<LogicNode> nodes = circuit.LogicNodes;
        IReadOnlyList<LogicConnection> connections = circuit.LogicConnections;
        IReadOnlyList<CircuitError> expandErrors = [];
        if (circuitLibrary != null) {
            var (expandedCircuit, expandErrs) = ExpandCustomCircuits(circuitLibrary, new Circuit(nodes, connections));
            expandErrors = expandErrs;
            if (expandErrors.Count == 0) {
                nodes = expandedCircuit.LogicNodes;
                connections = expandedCircuit.LogicConnections;
            }
        }

        if (dupErrors.Count + expandErrors.Count > 0) {
            var earlyErrors = new List<CircuitError>(dupErrors.Count + expandErrors.Count);
            earlyErrors.AddRange(dupErrors);
            earlyErrors.AddRange(expandErrors);
            return (null, earlyErrors.ToArray());
        }

        var pinWidthMap = BuildPinWidthMap(nodes, factories);
        var (resolvedBits, resolveErrors) = ResolveConnectorBits(nodes, connections, pinWidthMap);
        var (expandedConnections, busErrors) = ExpandBusConnections(connections, pinWidthMap, resolvedBits);
        connections = expandedConnections;
        var (executorAndDefinitionsList, unregsErrors) = BuildExecutorContexts(nodes, factories, resolvedBits);
        var validateErrors = ValidateInputConnections(executorAndDefinitionsList, connections, pinWidthMap);

        var allErrors = new List<CircuitError>(resolveErrors.Count + busErrors.Count + unregsErrors.Count + validateErrors.Count);
        allErrors.AddRange(resolveErrors);
        allErrors.AddRange(busErrors);
        allErrors.AddRange(unregsErrors);
        allErrors.AddRange(validateErrors);
        if (allErrors.Count > 0) {
            return (null, allErrors.ToArray());
        }

        logicIdAndPinNameToPinIndex = BuildPinIndex(executorAndDefinitionsList);
        var connErrors = ResolveConnections(connections);
        if (connErrors.Count > 0) {
            return (null, connErrors.ToArray());
        }
        return (this, []);
    }

    static IReadOnlyList<CircuitError> ValidateDuplicateNodeIds(Circuit circuit) {
        var seen = new HashSet<string>();
        var errors = new List<CircuitError>();
        foreach (var node in circuit.LogicNodes) {
            if (!seen.Add(node.LogicID)) {
                errors.Add(new CircuitError(node.LogicID, null, CircuitErrorKind.DuplicateNodeId,
                    $"Node '{node.LogicID}' is defined more than once in the circuit."));
            }
        }
        return errors;
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
    /// <returns>2パス目で logicIdAndPinNameToPinIndex を構築するために使用するリストと、検出されたエラー。</returns>
    (List<(ExecutorContext ctx, IOConnectorDefinition[] defs)> results, IReadOnlyList<CircuitError> errors) BuildExecutorContexts(
        IReadOnlyList<LogicNode> nodes,
        Dictionary<Type, ILogicExecutorFactory> factories,
        IReadOnlyDictionary<string, int> resolvedBits) {
        var executorList = new List<ExecutorContext>();
        var executorAndDefinitionsList = new List<(ExecutorContext ctx, IOConnectorDefinition[] defs)>();
        var buildErrors = new List<CircuitError>();
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
                foreach (var node in group.OrderBy(n => n.LogicID)) {
                    buildErrors.Add(new CircuitError(node.LogicID, null, CircuitErrorKind.UnregisteredLogicElement,
                        $"No factory registered for '{group.Key.Name}'."));
                }
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
        return (executorAndDefinitionsList, buildErrors);
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
                    if (pin.IsIndexed) {
                        for (int k = 0; k < pin.BitSize; k++) {
                            pinIndex[def.LogicID][$"{pin.PinName}[{k}]"] = (execIdx, logicNumInExec, ctx.Inputs.GetPinIndex(logicNumInExec, currentInputPinOffset + k));
                        }
                    }
                    currentInputPinOffset += pin.BitSize;
                }
                int currentOutputPinOffset = 0;
                foreach (var pin in def.OutputPins) {
                    pinIndex[def.LogicID][pin.PinName] = (execIdx, logicNumInExec, ctx.Outputs.GetPinIndex(logicNumInExec, currentOutputPinOffset));
                    if (pin.IsIndexed) {
                        for (int k = 0; k < pin.BitSize; k++) {
                            pinIndex[def.LogicID][$"{pin.PinName}[{k}]"] = (execIdx, logicNumInExec, ctx.Outputs.GetPinIndex(logicNumInExec, currentOutputPinOffset + k));
                        }
                    }
                    currentOutputPinOffset += pin.BitSize;
                }
            }
        }

        return pinIndex;
    }

    /// <summary>
    /// 接続リストを解決し、各出力ピンから接続先入力ピンへの <see cref="ExecutorContext.OutputToInputPinConnections"/> を構築します。
    /// 接続定義に不正な LogicID またはピン名が含まれる場合は <see cref="CircuitErrorKind.InvalidNodeReference"/> エラーを返します。
    /// </summary>
    IReadOnlyList<CircuitError> ResolveConnections(IReadOnlyList<LogicConnection> connections) {
        var errors = new List<CircuitError>();
        foreach (var connection in connections) {
            var sourceLogicID = connection.Source.LogicID;
            var sourcePinName = connection.Source.PinName;
            var targetLogicID = connection.Target.LogicID;
            var targetPinName = connection.Target.PinName;

            if (!logicIdAndPinNameToPinIndex.TryGetValue(sourceLogicID, out var sourcePins)) {
                errors.Add(new CircuitError(sourceLogicID, null, CircuitErrorKind.InvalidNodeReference,
                    $"Connection source '{sourceLogicID}' is not defined in the circuit."));
                continue;
            }
            if (!sourcePins.ContainsKey(sourcePinName)) {
                errors.Add(new CircuitError(sourceLogicID, sourcePinName, CircuitErrorKind.InvalidNodeReference,
                    $"Connection source '{sourceLogicID}' does not have pin '{sourcePinName}'."));
                continue;
            }
            if (!logicIdAndPinNameToPinIndex.TryGetValue(targetLogicID, out var targetPins)) {
                errors.Add(new CircuitError(targetLogicID, null, CircuitErrorKind.InvalidNodeReference,
                    $"Connection target '{targetLogicID}' is not defined in the circuit."));
                continue;
            }
            if (!targetPins.ContainsKey(targetPinName)) {
                errors.Add(new CircuitError(targetLogicID, targetPinName, CircuitErrorKind.InvalidNodeReference,
                    $"Connection target '{targetLogicID}' does not have pin '{targetPinName}'."));
                continue;
            }

            var sourcePinInfo = sourcePins[sourcePinName];
            var targetPinInfo = targetPins[targetPinName];

            executorContexts[sourcePinInfo.executorIndex].OutputToInputPinConnections[sourcePinInfo.pinIndex].Add(
                new TargetConnection(targetPinInfo.executorIndex, new int[] { targetPinInfo.pinIndex })
            );
        }
        return errors;
    }

    /// <summary>
    /// 通常素子（InputConnector/OutputConnector 以外）のピン幅マップを構築します。
    /// </summary>
    static Dictionary<string, Dictionary<string, PinDefinition>> BuildPinWidthMap(
        IReadOnlyList<LogicNode> nodes,
        Dictionary<Type, ILogicExecutorFactory> factories) {

        var pinWidthMap = new Dictionary<string, Dictionary<string, PinDefinition>>();
        foreach (var node in nodes) {
            if (node.LogicData is InputConnector or OutputConnector) {
                continue;
            }
            if (!factories.TryGetValue(node.LogicData.GetType(), out var factory)) {
                continue;
            }
            var def = factory.GetConnectorDefinition(node);
            var pinDefs = new Dictionary<string, PinDefinition>();
            foreach (var pin in def.InputPins) {
                pinDefs[pin.PinName] = pin;
            }
            foreach (var pin in def.OutputPins) {
                pinDefs[pin.PinName] = pin;
            }
            pinWidthMap[node.LogicID] = pinDefs;
        }
        return pinWidthMap;
    }

    /// <summary>
    /// ピン名からピン幅を取得します。
    /// <c>"name[k]"</c> 形式の場合はベース名 <c>"name"</c> でルックアップし、IsIndexed=true なら 1 を返します。
    /// </summary>
    static int? GetPinWidthFromMap(Dictionary<string, PinDefinition> pinMap, string pinName) {
        if (pinMap.TryGetValue(pinName, out var def)) {
            return def.BitSize;
        }
        var m = Regex.Match(pinName, @"^(.+)\[(\d+)\]$");
        if (!m.Success) {
            return null;
        }
        var baseName = m.Groups[1].Value;
        var k = int.Parse(m.Groups[2].Value);
        if (pinMap.TryGetValue(baseName, out var baseDef) && baseDef.IsIndexed && k < baseDef.BitSize) {
            return 1;
        }
        return null;
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
        IReadOnlyDictionary<string, Dictionary<string, PinDefinition>> pinWidthMap) {

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
                        } else if (pinWidthMap.TryGetValue(targetId, out var targetPinMap)) {
                            int? targetWidth = GetPinWidthFromMap(targetPinMap, targetPin);
                            if (targetWidth.HasValue) {
                                widths.Add(targetWidth.Value);
                            } else {
                                allResolved = false;
                            }
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
                    } else if (pinWidthMap.TryGetValue(sourceId, out var sourcePinMap)) {
                        inferredWidth = GetPinWidthFromMap(sourcePinMap, sourcePin);
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
    /// ピンのビット幅を取得します（バスピン・個別ビット・コネクタを含む）。
    /// </summary>
    static int? GetPinWidth(
        LogicConnector connector,
        IReadOnlyDictionary<string, Dictionary<string, PinDefinition>> pinWidthMap,
        IReadOnlyDictionary<string, int> resolvedBits) {

        if (resolvedBits.TryGetValue(connector.LogicID, out var bits)) {
            var m = Regex.Match(connector.PinName, @"^(.+)\[(\d+)\]$");
            if (!m.Success) {
                return bits;
            }
            var k = int.Parse(m.Groups[2].Value);
            return k < bits ? 1 : null;
        }
        if (pinWidthMap.TryGetValue(connector.LogicID, out var pinMap)) {
            return GetPinWidthFromMap(pinMap, connector.PinName);
        }
        return null;
    }

    /// <summary>
    /// ピン名にビットインデックスを付加します。
    /// <c>"out"</c> → <c>"out[0]"</c>、<c>"in[2]"</c> → <c>"in[2][0]"</c>
    /// </summary>
    static string ExpandBitPinName(string pinName, int bit) {
        return $"{pinName}[{bit}]";
    }

    /// <summary>
    /// ピンへのアクセスが InvalidPinAccess かを判別します。
    /// スカラーピンへのインデックスアクセス、またはバスピンの範囲外インデックスアクセスを検出します。
    /// </summary>
    static CircuitErrorKind? CheckPinAccessError(
        string logicId, string pinName,
        IReadOnlyDictionary<string, Dictionary<string, PinDefinition>> pinWidthMap) {

        var match = Regex.Match(pinName, @"^(.+)\[(\d+)\]$");
        if (!match.Success) {
            return null;
        }
        var baseName = match.Groups[1].Value;
        var k = int.Parse(match.Groups[2].Value);
        if (!pinWidthMap.TryGetValue(logicId, out var pinMap)) {
            return null;
        }
        if (!pinMap.TryGetValue(baseName, out var pinDef)) {
            return null;
        }
        if (!pinDef.IsIndexed) {
            return CircuitErrorKind.InvalidPinAccess;
        }
        if (k >= pinDef.BitSize) {
            return CircuitErrorKind.InvalidPinAccess;
        }
        return null;
    }

    /// <summary>
    /// バス接続（N ビット同士）を個別ビット接続に展開します。
    /// ビット幅不一致・InvalidPinAccess・InvalidNodeReference はエラーとして返します。
    /// </summary>
    static (IReadOnlyList<LogicConnection> connections, IReadOnlyList<CircuitError> errors)
    ExpandBusConnections(
        IReadOnlyList<LogicConnection> connections,
        IReadOnlyDictionary<string, Dictionary<string, PinDefinition>> pinWidthMap,
        IReadOnlyDictionary<string, int> resolvedBits) {

        var expandedConnections = new List<LogicConnection>();
        var errors = new List<CircuitError>();

        foreach (var conn in connections) {
            var srcAccessError = pinWidthMap.ContainsKey(conn.Source.LogicID)
                ? CheckPinAccessError(conn.Source.LogicID, conn.Source.PinName, pinWidthMap)
                : null;
            var tgtAccessError = pinWidthMap.ContainsKey(conn.Target.LogicID)
                ? CheckPinAccessError(conn.Target.LogicID, conn.Target.PinName, pinWidthMap)
                : null;
            if (srcAccessError != null) {
                errors.Add(new CircuitError(conn.Source.LogicID, conn.Source.PinName, srcAccessError.Value,
                    $"Connection source '{conn.Source.LogicID}'.'{conn.Source.PinName}' is an invalid pin access."));
                continue;
            }
            if (tgtAccessError != null) {
                errors.Add(new CircuitError(conn.Target.LogicID, conn.Target.PinName, tgtAccessError.Value,
                    $"Connection target '{conn.Target.LogicID}'.'{conn.Target.PinName}' is an invalid pin access."));
                continue;
            }

            var srcWidth = GetPinWidth(conn.Source, pinWidthMap, resolvedBits);
            var tgtWidth = GetPinWidth(conn.Target, pinWidthMap, resolvedBits);

            if (srcWidth is null) {
                bool srcKnown = pinWidthMap.ContainsKey(conn.Source.LogicID) || resolvedBits.ContainsKey(conn.Source.LogicID);
                if (srcKnown) {
                    errors.Add(new CircuitError(conn.Source.LogicID, conn.Source.PinName, CircuitErrorKind.InvalidNodeReference,
                        $"Connection source '{conn.Source.LogicID}'.'{conn.Source.PinName}' could not be resolved."));
                    continue;
                }
                expandedConnections.Add(conn);
                continue;
            }
            if (tgtWidth is null) {
                bool tgtKnown = pinWidthMap.ContainsKey(conn.Target.LogicID) || resolvedBits.ContainsKey(conn.Target.LogicID);
                if (tgtKnown) {
                    errors.Add(new CircuitError(conn.Target.LogicID, conn.Target.PinName, CircuitErrorKind.InvalidNodeReference,
                        $"Connection target '{conn.Target.LogicID}'.'{conn.Target.PinName}' could not be resolved."));
                    continue;
                }
                expandedConnections.Add(conn);
                continue;
            }

            if (srcWidth == 1 && tgtWidth == 1) {
                expandedConnections.Add(conn);
                continue;
            }

            if (srcWidth != tgtWidth) {
                errors.Add(new CircuitError(conn.Source.LogicID, conn.Source.PinName, CircuitErrorKind.BitWidthMismatch,
                    $"Bit width mismatch: '{conn.Source.LogicID}'.'{conn.Source.PinName}' ({srcWidth} bits) " +
                    $"-> '{conn.Target.LogicID}'.'{conn.Target.PinName}' ({tgtWidth} bits)."));
                continue;
            }

            for (int bit = 0; bit < srcWidth; bit++) {
                expandedConnections.Add(new LogicConnection(
                    conn.Source with { PinName = ExpandBitPinName(conn.Source.PinName, bit) },
                    conn.Target with { PinName = ExpandBitPinName(conn.Target.PinName, bit) }
                ));
            }
        }

        return (expandedConnections, errors);
    }

    /// <summary>
    /// 全素子の入力ピン未接続チェックおよび複数ソース接続チェックを行います。
    /// </summary>
    static IReadOnlyList<CircuitError> ValidateInputConnections(
        IReadOnlyList<(ExecutorContext ctx, IOConnectorDefinition[] defs)> executorAndDefinitionsList,
        IReadOnlyList<LogicConnection> connections,
        IReadOnlyDictionary<string, Dictionary<string, PinDefinition>> pinWidthMap) {

        var errors = new List<CircuitError>();

        var allPinDefs = new Dictionary<string, Dictionary<string, PinDefinition>>();
        foreach (var (_, defs) in executorAndDefinitionsList) {
            foreach (var def in defs) {
                var pinMap = new Dictionary<string, PinDefinition>();
                foreach (var pin in def.InputPins.Concat(def.OutputPins))
                    pinMap[pin.PinName] = pin;
                allPinDefs[def.LogicID] = pinMap;
            }
        }

        var sourceCounts = new Dictionary<(string nodeId, string pinName), int>();
        foreach (var conn in connections) {
            var targetId = conn.Target.LogicID;
            var targetPin = conn.Target.PinName;
            var normalizedPin = targetPin;
            var m = Regex.Match(targetPin, @"^(.+)\[(\d+)\]$");
            if (m.Success) {
                var baseName = m.Groups[1].Value;
                var k = int.Parse(m.Groups[2].Value);
                if (k == 0
                    && allPinDefs.TryGetValue(targetId, out var pm)
                    && pm.TryGetValue(baseName, out var pd)
                    && pd.IsIndexed && pd.BitSize == 1) {
                    normalizedPin = baseName;
                }
            }
            var key = (targetId, normalizedPin);
            sourceCounts.TryGetValue(key, out var cnt);
            sourceCounts[key] = cnt + 1;
        }

        foreach (var (_, defs) in executorAndDefinitionsList) {
            foreach (var def in defs) {
                foreach (var pin in def.InputPins) {
                    var logicId = def.LogicID;
                    if (!pin.IsIndexed) {
                        sourceCounts.TryGetValue((logicId, pin.PinName), out var cnt);
                        if (cnt == 0) {
                            errors.Add(new CircuitError(logicId, pin.PinName,
                                CircuitErrorKind.UnconnectedInput,
                                $"Input pin '{pin.PinName}' of '{logicId}' has no source connection."));
                        } else if (cnt > 1) {
                            errors.Add(new CircuitError(logicId, pin.PinName,
                                CircuitErrorKind.MultipleSourceConnections,
                                $"Input pin '{pin.PinName}' of '{logicId}' has {cnt} source connections, but only 1 is allowed."));
                        }
                    } else {
                        sourceCounts.TryGetValue((logicId, pin.PinName), out var busCount);
                        int[] bitCounts;
                        if (pin.BitSize == 1) {
                            bitCounts = [0];
                        } else {
                            bitCounts = new int[pin.BitSize];
                            for (int k = 0; k < pin.BitSize; k++) {
                                sourceCounts.TryGetValue((logicId, $"{pin.PinName}[{k}]"), out bitCounts[k]);
                            }
                        }
                        bool hasBitConnections = Array.Exists(bitCounts, c => c > 0);
                        if (busCount > 0 && hasBitConnections) {
                            errors.Add(new CircuitError(logicId, pin.PinName,
                                CircuitErrorKind.MultipleSourceConnections,
                                $"Input pin '{pin.PinName}' of '{logicId}' has mixed bus and bit-level connections."));
                            continue;
                        }
                        if (busCount > 1) {
                            errors.Add(new CircuitError(logicId, pin.PinName,
                                CircuitErrorKind.MultipleSourceConnections,
                                $"Input pin '{pin.PinName}' of '{logicId}' has {busCount} source connections, but only 1 is allowed."));
                        } else if (busCount == 0) {
                            for (int k = 0; k < pin.BitSize; k++) {
                                var bitPinName = $"{pin.PinName}[{k}]";
                                if (bitCounts[k] == 0) {
                                    errors.Add(new CircuitError(logicId, bitPinName,
                                        CircuitErrorKind.UnconnectedInput,
                                        $"Input pin '{bitPinName}' of '{logicId}' has no source connection."));
                                } else if (bitCounts[k] > 1) {
                                    errors.Add(new CircuitError(logicId, bitPinName,
                                        CircuitErrorKind.MultipleSourceConnections,
                                        $"Input pin '{bitPinName}' of '{logicId}' has {bitCounts[k]} source connections, but only 1 is allowed."));
                                }
                            }
                        }
                    }
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// CustomCircuit を含む回路をフラット化・コネクタ透過・ノード整理して展開します。
    /// </summary>
    (Circuit circuit, IReadOnlyList<CircuitError> errors) ExpandCustomCircuits(
        IReadOnlyDictionary<string, Circuit> circuitLibrary,
        Circuit originalCircuit) {
        var (expandedNodes, expandedConnections, flattenErrors) = FlattenCircuit(circuitLibrary, originalCircuit);
        if (flattenErrors.Count > 0) {
            return (originalCircuit, flattenErrors);
        }
        var (resultConnections, solveErrors) = SolveConnectors(expandedNodes, expandedConnections);
        if (solveErrors.Count > 0) {
            return (originalCircuit, solveErrors);
        }
        var resultNodes = CleanupNodes(expandedNodes);
        return (new Circuit(resultNodes, resultConnections), []);
    }

    /// <summary>
    /// 全ノード・接続を prefix 付きでフラット化します。CustomCircuit を再帰的に展開します。
    /// isTop フラグはトップレベル回路（level == 0）のノード・接続であることを示します。
    /// </summary>
    (Dictionary<string, (bool isTop, LogicNode node)> nodes,
     List<(bool isTop, LogicConnection connection)> connections,
     IReadOnlyList<CircuitError> errors)
    FlattenCircuit(IReadOnlyDictionary<string, Circuit> circuitLibrary, Circuit originalCircuit) {
        var expandedNodes = new Dictionary<string, (bool isTop, LogicNode node)>();
        var expandedConnections = new List<(bool isTop, LogicConnection connection)>();
        var flattenErrors = new List<CircuitError>();

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
                        flattenErrors.Add(new CircuitError(newLogicID, null, CircuitErrorKind.InvalidNodeReference,
                            $"Circuit '{targetCircuitName}' not found in library."));
                        continue;
                    }
                    // ネストを示すプリフィックス
                    var idPrefix = $"{prefix}{node.LogicID}.";
                    Flatten(idPrefix, circuitDef, level + 1);
                }
            }
        }

        Flatten("", originalCircuit, 0);
        return (expandedNodes, expandedConnections, flattenErrors);
    }

    /// <summary>
    /// コネクタ（InputConnector/OutputConnector）を透過して、実際の接続（通常素子 ↔ トップレベルコネクタ）に変換します。
    /// 接続のソースは1つしか接続されないことを前提としています（このメソッドが呼ばれるよりも先にエラー検知で弾いていること）。
    /// </summary>
    (List<LogicConnection> result, IReadOnlyList<CircuitError> errors) SolveConnectors(
        Dictionary<string, (bool isTop, LogicNode node)> expandedNodes,
        List<(bool isTop, LogicConnection connection)> expandedConnections) {
        var solveErrors = new List<CircuitError>();

        // トップレベル接続のソース LogicID が存在するか事前検証
        foreach (var conn in expandedConnections) {
            if (conn.isTop && !expandedNodes.ContainsKey(conn.connection.Source.LogicID)) {
                solveErrors.Add(new CircuitError(conn.connection.Source.LogicID, null,
                    CircuitErrorKind.InvalidNodeReference,
                    $"Connection source '{conn.connection.Source.LogicID}' is not defined in the circuit."));
            }
        }
        if (solveErrors.Count > 0) {
            return ([], solveErrors);
        }

        // 計算量を減らすために辞書にして接続先を高速で検索できるようにする
        // 無効なソースを除外して KeyNotFoundException を防止
        var groupedSourceConnections = expandedConnections
            .Where(x => expandedNodes.ContainsKey(x.connection.Source.LogicID))
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
                continue;
            }
            if (sourceNode.node.LogicData is CustomCircuit ||
                (!connection.isTop && sourceNode.node.LogicData is InputConnector or OutputConnector)) {
                continue;
            }
            resultTargetConnectors.Clear();
            alreadyConnectedSourceConnectorNames.Clear();
            FindTargetConnections(expandedNodes, groupedSourceConnections, alreadyConnectedSourceConnectorNames, connection.connection.Target, resultTargetConnectors, solveErrors);
            foreach (var targetConnector in resultTargetConnectors) {
                resultConnections.Add(new LogicConnection(connection.connection.Source, targetConnector));
            }
        }
        return (resultConnections, solveErrors);
    }

    /// <summary>
    /// 指定された接続ターゲットから、コネクタを再帰的に辿って実際の接続先をリストに追加します。
    /// </summary>
    void FindTargetConnections(
        Dictionary<string, (bool isTop, LogicNode node)> expandedNodes,
        Dictionary<string, (bool isTop, LogicConnection connection)[]> groupedSourceConnections,
        HashSet<string> skipSourceConnectorNames,
        LogicConnector outputConnector,
        List<LogicConnector> resultConnections,
        List<CircuitError> errors) {
        if (!expandedNodes.TryGetValue(outputConnector.LogicID, out var sourceNode)) {
            errors.Add(new CircuitError(outputConnector.LogicID, null,
                CircuitErrorKind.InvalidNodeReference,
                $"Connection target '{outputConnector.LogicID}' is not defined in the circuit."));
            return;
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
            return;
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
                        resultConnections,
                        errors);
                }
            }
        } else if (targetNode.node.LogicData is CustomCircuit) {
            FindTargetConnections(
                expandedNodes,
                groupedSourceConnections,
                skipSourceConnectorNames,
                new LogicConnector($"{outputConnector.LogicID}.{outputConnector.PinName}", ""),
                resultConnections,
                errors);
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