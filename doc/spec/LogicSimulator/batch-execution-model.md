# バッチ実行モデル

LogicSimulator の中核となる設計です。**論理素子を1つずつ演算するのではなく、同じ種類の素子をまとめて1つのメソッド呼び出しで一括演算します。**

> コード断片は、フローを正確に伝えるため実コードからの抜粋です。**正本はコード**であり、ここでは核心部分に絞っています。シグネチャ等の詳細は出典のソースを参照してください。

---

## 1. なぜバッチ処理するのか（設計動機）

論理回路は素子数が非常に多くなり得ます（数千〜数万個）。素子を1つずつ演算する素朴な実装は、次の理由で遅くなります。

### (a) 仮想関数呼び出しの回数を減らす

演算ロジックはインターフェース [`ILogicExecutor`](../../../LogicSimulator/ILogicExecutor.cs) を介して呼び出します。**インターフェース経由の呼び出しは仮想関数呼び出しになり、インライン展開できないため通常のメソッド呼び出しより遅くなります。**

素子を1つずつ演算すると、**素子の数だけ** 仮想関数呼び出しが発生します。そこで、**同じ種類の素子を1つの Executor がまとめて持ち、`Execute` を1回呼ぶだけで、その種類の変化した素子すべてを内部ループで処理** します。これにより仮想関数呼び出しの回数が **「素子の数」ではなく「素子の種類の数」** で済み、呼び出し回数と処理負荷が減ります。

| 方式 | 仮想関数呼び出しの回数（1ステップあたり） |
|---|---|
| 素子を1つずつ演算 | 変化した素子の数 |
| **バッチ実行（本ライブラリ）** | **変化した素子を含む「素子の種類の数」** |

### (b) キャッシュ局所性

同じ種類の素子の全ピンを1本の配列（[`LogicPins`](../../../LogicSimulator/Runtime/LogicPins.cs)）に平坦化して持つため、連続したメモリを順に走査でき、CPU キャッシュ効率が高くなります。

### (c) 変化した素子だけを処理する

毎ステップ全素子を再計算せず、**入力が変化した素子だけ** を演算します（後述の変化検知）。

---

## 2. 素子の種類ごとのグルーピング

ビルド時、素子は **型ごとにグルーピング** され、各グループに1つの Executor（と1つの `ExecutorContext`）が割り当てられます。

出典: [`LogicSimulation.cs`](../../../LogicSimulator/LogicSimulation.cs) `BuildExecutorContexts`（抜粋）

```csharp
foreach (var group in nodes
    .Where(n => n.LogicData is not InputConnector and not OutputConnector)
    .GroupBy(node => node.LogicData.GetType())) {     // ← 型ごとにまとめる

    var logicNodes = group.OrderBy(n => n.LogicID).ToArray();  // LogicID 順で安定化
    var executor   = factory.CreateExecutor(logicNodes, () => { needsInitialExecution = true; });
    var definitions = logicNodes.Select(node => factory.GetConnectorDefinition(node)).ToArray();
    // 1グループ = 1 ExecutorContext
}
```

- `GroupBy(GetType())` … `AndLogic` は `AndLogic` どうし、`NotLogic` は `NotLogic` どうしで1グループ。
- `OrderBy(LogicID)` … `GroupBy` の列挙順は非決定的なので、`LogicID` でソートして各素子の `logicNumber`（グループ内の通し番号）を安定させる。
- `InputConnector` / `OutputConnector` はそれぞれ専用に1つの `ExecutorContext`（演算しない `NoOpExecutor`）へまとめられる。

---

## 3. LogicPins ― ピンの平坦化

1つの Executor が持つ全素子・全ピンの信号は、[`LogicPins`](../../../LogicSimulator/Runtime/LogicPins.cs) という **構造体（struct）** が1本の配列で保持します。

```csharp
public struct LogicPins {
    public LogicSignal[] Pins;                       // 全素子の全ピンを平坦化した1本の配列
    public int[] PinNumberToLogicNumber;             // ピン番号 → 素子番号(logicNumber)
    public (int arrayOffset, int length)[] NumOfPins; // 素子ごとの「開始位置と長さ」
}
```

素子番号（`logicNumber`）とピン番号から、`NumOfPins[logicNumber]` のオフセットを使って `Pins` 配列の位置を求めます。素子ごとに別オブジェクトを持たないため、メモリが連続し、確保するオブジェクト数も少なくなります。

---

## 4. 変化検知 ― 変化した素子だけを演算する

`Execute` には、その Executor が持つ素子のうち **入力が変化した素子だけ** を列挙する [`LogicPinReader`](../../../LogicSimulator/Runtime/LogicPinReader.cs) が渡されます。Executor は `while` ループで変化素子を取り出し、まとめて処理します。

出典: [`Gates/And.cs`](../../../LogicSimulator/Gates/And.cs) `AndExecutor.Execute`（抜粋）

```csharp
public void Execute(LogicPinReader inputs, LogicPinsWriter outputs) {
    // 入力が変化した素子を次々に取り出し、1回の Execute で複数素子をまとめて処理する
    while (inputs.TryGetNextChangedLogicNumber(out var logicNo)) {
        int length = inputs.GetPinsLength(logicNo);
        // …この素子の入力ピンを読み、AND 演算して…
        outputs.WriteBit(logicNo, 0, output);
    }
}
```

`TryGetNextChangedLogicNumber` は、変化した入力ピンを `PinNumberToLogicNumber` で素子番号に変換しつつ、`isExecutedLogicNumbers` で **同じ素子を二重に処理しない** ようにします（1素子の複数入力が同時に変化しても演算は1回）。

出典: [`LogicPinReader.cs`](../../../LogicSimulator/Runtime/LogicPinReader.cs)（抜粋）

```csharp
public bool TryGetNextChangedLogicNumber(out int logicNumber) {
    while (true) {
        if (changedPins.Count <= pos) { logicNumber = 0; return false; }
        var logicNo = targetPins.PinNumberToLogicNumber[changedPins[pos]];
        pos++;
        if (!isExecutedLogicNumbers[logicNo]) {  // まだ処理していない素子だけ返す
            isExecutedLogicNumbers[logicNo] = true;
            logicNumber = logicNo;
            return true;
        }
    }
}
```

出力側 [`LogicPinsWriter.WriteBit`](../../../LogicSimulator/Runtime/LogicPinsWriter.cs) は、**値が実際に変化したときだけ** 変化リストに記録します（差分のみ伝播）。

---

## 5. Step() ― 収束までの伝播ループ

[`LogicSimulation.Step`](../../../LogicSimulator/LogicSimulation.cs) は、信号が変化しなくなる（収束する）まで、次の2つを交互に繰り返します。

1. **出力 → 入力の伝播**: 変化した出力ピンの値を、接続先の入力ピンへコピーする。変化した入力ピンを持つ Executor を「実行が必要」として登録する。
2. **入力変化素子の実行**: 入力が変化した Executor の `Execute` を呼ぶ。出力が変化したら、その Executor を「伝播が必要」として登録する。

出典: [`LogicSimulation.cs`](../../../LogicSimulator/LogicSimulation.cs) `Step`（構造の抜粋）

```csharp
int maxIteration = 10000;
for (int loopCount = 0;
     0 < outputValueChangedExecutorIndexes.Count || 0 < inputValueChangedExecutorIndexes.Count;
     loopCount++) {
    if (maxIteration <= loopCount) {
        throw new InvalidOperationException("circuit oscillation"); // 発振の検出
    }

    // (1) 出力の変化を、接続先の入力へコピーして伝播
    // (2) 入力が変化した Executor だけ Execute を呼ぶ
    ctx.IsExecutedLogicNumbers.AsSpan().Clear();
    ctx.Executor.Execute(
        new LogicPinReader(ctx.Inputs,  ctx.ValueChangedInputPins, ctx.IsExecutedLogicNumbers, 0),
        new LogicPinsWriter(ctx.Outputs, ctx.ValueChangedOutputPins));
}
```

- 入力にも出力にも変化が無くなったらループ終了（＝回路が安定した）。
- フリップフロップのフィードバックなどで信号が振動して収束しない場合、`maxIteration`（10000）回で `InvalidOperationException("circuit oscillation")` を投げて無限ループを防ぐ。

---

## 6. ゼロアロケーション方針

`Step` / `Execute` は1ステップで何度も呼ばれる **ホットパス** です。ここでは **毎回のヒープ確保（アロケーション）をゼロ** にして、GC（ガベージコレクション）の負荷を避けています。

| 手段 | 内容 |
|---|---|
| **struct 化** | [`LogicPins`](../../../LogicSimulator/Runtime/LogicPins.cs) / [`LogicPinReader`](../../../LogicSimulator/Runtime/LogicPinReader.cs) / [`LogicPinsWriter`](../../../LogicSimulator/Runtime/LogicPinsWriter.cs) はすべて `struct`。`Step` 内で `new LogicPinReader(...)` してもヒープに確保されない。 |
| **AggressiveInlining** | これらの構造体のメソッドには `[MethodImpl(MethodImplOptions.AggressiveInlining)]` を付け、呼び出しオーバーヘッドを抑える。 |
| **コレクションの使い回し** | 変化ピンを記録する `ValueChangedInputPins` / `ValueChangedOutputPins`（`List<int>`）や `IsExecutedLogicNumbers`（`bool[]`）は、毎ステップ確保し直さず、`Clear()` / `AsSpan().Clear()` で内容だけ消して再利用する。 |

> 関連: 状態を持つ Executor を実装する場合も、この方針に沿って **接続の確立・解除は構造体で表現** し、JIT のインライン展開と GC 回避を狙う設計になっています（[extending.md](extending.md) 参照）。
