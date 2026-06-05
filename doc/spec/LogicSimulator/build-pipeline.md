# ビルドパイプライン

[`LogicSimulation.TryBuild`](../../../LogicSimulator/LogicSimulation.cs) は、回路定義（`Circuit`）を検証しながら、実行可能な `LogicSimulation` に変換します。処理は **多段のフェーズ** に分かれており、**順序には意味があります**。保守時にフェーズを追加・並べ替えるときの判断材料として、各フェーズの責務と依存関係を記します。

> 各エラー種別（`CircuitErrorKind`）の発生条件は [`TryBuild`](../../../LogicSimulator/LogicSimulation.cs) と [`CircuitError.cs`](../../../LogicSimulator/CircuitError.cs) の XML コメントを参照してください（ここでは重複させません）。

---

## フェーズ一覧と順序

```
[1] 重複ノードID検証 ............ ValidateDuplicateNodeIds
        ↓ (circuitLibrary が指定された場合のみ、階層回路の展開ブロック)
[2] CustomCircuit 参照検証 ...... ValidateCircuitLibraryReferences
[3] フラット化 .................. FlattenCircuit       （prefix を付けて再帰展開）
[4] トップレベルソース検証 ...... ValidateTopLevelSourceIds
[5] コネクタ透過解決 ............ SolveConnectors      （Input/OutputConnector を畳む）
[6] 不要ノード削除 .............. CleanupNodes
        ↓
[7] 素子の検証 .................. ValidateElements     （各 Factory の Validate）
[8] ピン幅マップ構築 ............ BuildPinWidthMap
[9] コネクタのビット幅解決 ...... ResolveConnectorBits （接続からビット幅を推論／整合性検査）
[10] バス接続の検証 ............. ValidateBusConnections
[11] ExecutorContext 構築 ....... BuildExecutorContexts（型ごとに Executor 生成）
[12] バス接続の展開 ............. ExpandBusConnections （N ビット接続を1ビットずつに）
[13] 入力接続の検証 ............. ValidateInputConnections（未接続・複数ソース）
[14] ピンインデックス構築 ....... BuildPinIndex
[15] 接続の解決 ................. ResolveConnections   （出力→入力の接続表を構築）
```

各フェーズの後で **エラーが1件でもあれば `false` を返して打ち切り** ます（早期 return）。後続フェーズは前段が成功していることを前提にできるため、不正な状態のまま処理が進みません。

出典: [`LogicSimulation.cs`](../../../LogicSimulator/LogicSimulation.cs) `TryBuild`（早期 return の骨格・抜粋）

```csharp
errors = [.. ValidateDuplicateNodeIds(circuit)];
if (errors.Length > 0) { return false; }

if (circuitLibrary != null) {
    // [2]-[6] 階層回路の展開（各段でエラーがあれば return false）
    circuit = new Circuit(CleanupNodes(flattenedNodes), solvedConns);
}

errors = [.. ValidateElements(circuit.LogicNodes, factories)];
if (errors.Length > 0) { return false; }

var pinWidthMap = BuildPinWidthMap(...);
var (resolvedBits, resolveErrors) = ResolveConnectorBits(...);
var busErrors = ValidateBusConnections(...);
var (..., executorContextsArray, ...) = BuildExecutorContexts(...);
var expandedConnections = busErrors.Count > 0 ? circuit.LogicConnections : ExpandBusConnections(...);
// …検証エラーをまとめて確認し、問題なければ…
var logicIdAndPinNameToPinIndex = BuildPinIndex(...);
errors = [.. ResolveConnections(expandedConnections, logicIdAndPinNameToPinIndex, executorContextsArray)];
if (errors.Length > 0) { return false; }

simulation = new LogicSimulation(executorContextsArray, logicIdAndPinNameToPinIndex, initialInputChangedIndexes);
return true;
```

---

## なぜこの順なのか（依存関係）

フェーズの順序は、**後段が前段の成果物を必要とする** という依存で決まっています。

### 階層展開（[2]〜[6]）が最初

`CustomCircuit` を含む階層回路は、まず1枚の平坦な回路に展開してからでないと、個々の素子・配線を一律に検証できません。

- **[3] フラット化** で、内側の回路のノード・接続に `親.子` の prefix を付けて1階層に並べる。
- **[5] コネクタ透過解決** で、回路の境界に置かれた `InputConnector` / `OutputConnector` を辿り、「実際の素子どうしの接続」へ畳む。これによりトップレベル以外のコネクタは中継点として消える。
- **[6] 不要ノード削除** で `CustomCircuit` 本体とトップレベル以外のコネクタを取り除く。
- 以降のフェーズは、階層を意識しない平坦な回路として扱える。

### ビット幅の解決（[9]）が、バス展開・接続検証より前

[`InputConnector`](../../../LogicSimulator/InputConnector.cs) / [`OutputConnector`](../../../LogicSimulator/OutputConnector.cs) は `DataBits` を省略でき、その場合は **接続先のピン幅から推論** します。バス（多ビット）接続を1ビットずつに展開（[12]）するにも、未接続・複数ソースを検証（[13]）するにも、各ピンの確定したビット幅が必要です。そのため幅の解決を先に行います。

### バス展開（[12]）は、整合性検証（[10]）の後

[12] `ExpandBusConnections` は、N ビット接続を `[0]`…`[N-1]` の個別ビット接続へ機械的に展開します。このとき左右のビット幅が一致していることが前提です。**[10] `ValidateBusConnections` でビット幅不一致を検出しておく** ことで、展開時に範囲外アクセスを踏みません（コード上も `busErrors` があれば展開をスキップします）。

### ピンインデックス構築（[14]）と接続解決（[15]）が最後

- **[11] `BuildExecutorContexts`** で Executor と各素子のピン定義（`logicNumber`・ピン配置）が確定する。
- **[14] `BuildPinIndex`** は、その確定した配置をもとに「`LogicID` ＋ ピン名 → ピンの位置」の索引を作る。よって [11] の後でなければ作れない。
- **[15] `ResolveConnections`** は、**展開済みの接続（[12]）** と **ピンインデックス（[14]）** の両方を使って、出力ピンから接続先入力ピンへの伝播表（`ExecutorContext.OutputToInputPinConnections`）を構築する。両方の成果物に依存するため最後に置く。

この伝播表が、実行時の [Step()](batch-execution-model.md#5-step--収束までの伝播ループ) における「出力 → 入力のコピー」に使われます。
