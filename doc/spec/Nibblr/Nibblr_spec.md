# Nibblr パーサーコンビネーター 仕様書

> 本仕様書は、パーサー構築部と解析実行部を 2 段階に分離するという当初要望から出発し、検討の結果到達した「統合方式」を中心に、データ構造の役割・意味論・契約・不変条件・設計判断の根拠を規定する。
>
> **本仕様書の方針**: 仕様書はアルゴリズム(各 `Parse` の内部実装)を記述しない。記述するのは、契約として守るべき型シグネチャ、各構成要素の意味論、横断的な不変条件、および「なぜそう決めたか」の設計判断ログである。実装の自由を残すため、コードは原則として型シグネチャと契約の提示に限定する。

Nibblr ニブラー
由来: `nibble`(4ビット = 半バイト / 「少しずつかじる」)＋ `-r`。データを少しづつかじりながら解析するライブラリを示唆する造語。

---

## 1. 背景と目的

### 1.1 当初の要望

現行の Nibblr は、パーサーコンビネーターの構築用 API と、パースを実行する部分が単一のクラスに同居していた。当初の要望は次の 2 点であった。

1. **データ化による最適化** — パーサー構築部をデータとして表現し、コンパイラーが行うような構築時最適化を可能にする。
2. **ディスパッチ除去の余地** — 通常はポリモーフィズム(仮想関数呼び出し)で解析を行うが、その仮想呼び出しはメソッドのインライン展開を妨げ、わずかに遅い。利用者レベルで型ごとに呼び出すメソッドを直書きし、仮想呼び出しを防いでインライン展開できる拡張余地を残す。

### 1.2 検討の結論

検討の結果、当初想定した「データ部クラスと実行部クラスの物理的分離」は採らず、**両者を同一オブジェクトに統合する**方式に到達した。同一オブジェクトでありながら、

- **実行の顔**: 子パーサーの具象型を型引数として保持することで、解析実行時の仮想呼び出しを除去できる。
- **データの顔**: 非ジェネリックなビューインターフェースを併せ持つことで、最適化器がノードをデータとして走査・書き換えできる。

この統合方式により、当初対立していた目的 (1)(最適化) と目的 (2)(ディスパッチ除去) が、二者択一ではなく両立する。詳細は §10 の設計判断ログに記す。

---

## 2. 用語

| 用語 | 定義 |
|------|------|
| パーサー記述 / ノード | 「どんなパーサーか」を表す不変の `record`。木(厳密には DAG)を構成する。実行の顔とデータの顔を同一オブジェクトで併せ持つ。 |
| 評価器 / 解析実行 | ノードが自身を解析実行する振る舞い。本方式ではノードと同一(統合方式)。 |
| 実行の顔 | ノードが `IParser<TIn,TOut>` を具象型引数経由で実装する側面。仮想呼び出しを除去する。 |
| データの顔 | ノードが非ジェネリックビュー(`INode` 系)を実装する側面。最適化器が型引数なしで走査する。 |
| 最適化器 | ノードの木を入力に、観測可能な結果を変えずに別の木へ書き換える純粋変換。 |
| SG | ソースジェネレーター。文法定義を解析し、具象型を保持した解析コードをビルド時に生成する。 |
| 再帰境界 | `Deferred` ノード。循環(自己参照)を断ち切る唯一の点。 |

> **設計判断**: 当初 IR(中間表現)という語を用いたが、コンパイラー内部用語であり本ライブラリの語彙として不適切なため、「パーサー記述 / ノード」に改めた。

---

## 3. 横断的な設計原則(全体に効く前提)

1. **AOT / IL2CPP 安全** — リフレクションによる動的コード生成(`MakeGenericType`、`Reflection.Emit` 等)を行わない。統合方式のノードは静的なジェネリック `record` の木であり、この制約を自然に満たす。
2. **解析時ゼロアロケーション** — 解析(`Parse`)のホットパスは、成功パス・失敗パスの双方で 0 B/op であること。例外は §7.3 に定義する 2 点(利用者の意味作用、`Many` 系の成果物生成)のみ。
3. **構築と解析の責務分離** — アロケーションを伴う計算(`ExpectedSet` の生成・結合、デリゲート生成、`Deferred` の解決)はすべて構築時または初回解決時に行い、解析ホットパスから排除する。構築は軽量であり複数回行われ得る。
4. **開放性(利用者拡張の自由)** — 利用者は `IParser<TIn,TOut>` を実装した独自パーサーを追加でき、ライブラリの全コンビネーターと相互運用できる。ライブラリのいかなる機構も、利用者によるパーサー追加を妨げてはならない。
5. **観測可能な結果の不変性** — 最適化器は、観測可能な解析結果(成功/失敗・消費量・値・エラーの位置と期待集合)を変更してはならない。
6. **仕様と実装の分離** — 本仕様書はデータ構造の形・意味論・契約・不変条件を規定し、各パーサーの内部アルゴリズムは規定しない。

---

## 4. 中核の型シグネチャ(背骨)

> 本章のシグネチャは規範である。`Parse` の中身(アルゴリズム)は実装に委ねる。

### 4.1 結果型(すべて値型・解析時ゼロアロ)

```csharp
// 入力中の範囲(絶対オフセット基準。§6 参照)
public readonly record struct SpanSlice(int Start, int Length);

// 期待集合(構築時に確定する不変オブジェクト。解析時は参照コピーのみ)
public sealed class ExpectedSet
{
    public IReadOnlyList<string> Labels { get; }
    public ExpectedSet(params string[] labels);
    public ExpectedSet Union(ExpectedSet other);   // ※構築時のみ呼ぶ規約
}

// エラー(第1層:ホットパス用。位置は絶対オフセット)
public readonly record struct ParseError(int Position, ExpectedSet Expected);

// 解析結果(型引数は TOut のみ。TError は廃止)
public readonly record struct ParseResult<TOut>(
    bool Success, int Consumed, TOut Value, ParseError Error)
{
    public static ParseResult<TOut> Ok(int consumed, TOut value);
    public static ParseResult<TOut> Fail(ParseError error);
}
```

### 4.2 パーサーの中核契約

```csharp
public interface IParser<TIn, TOut> where TIn : IEquatable<TIn>
{
    // baseOffset = 元入力先頭からの絶対オフセット
    // (SpanSlice / ParseError の座標系を揃えるために引き回す。値型のため ゼロアロに影響しない)
    ParseResult<TOut> Parse(ReadOnlySpan<TIn> input, int baseOffset);
}
```

### 4.3 ノード:統合方式(record + 子の具象型を型引数で保持)

各ノードは「実行の顔(`IParser<TIn,TOut>`)」と「データの顔(非ジェネリックビュー)」を同一オブジェクトで併せ持つ。

```csharp
// --- データの顔:非ジェネリックビュー(最適化器がパターンマッチで使う)---
public interface INode { }
public interface ITagNode      : INode { /* 種別固有のデータを非ジェネリックに公開 */ }
public interface IThenNode     : INode { INode First { get; } INode Second { get; } }
public interface IOrNode       : INode { INode First { get; } INode Second { get; } }
public interface IMapNode      : INode { INode Inner { get; } }
public interface IDeferredNode  : INode { INode Resolve(); }   // 再帰境界

// --- 実行の顔:型引数付き具象 record(代表例)---
public sealed record TagParser<TIn>(ReadOnlyMemory<TIn> Pattern, ExpectedSet Expected)
    : IParser<TIn, SpanSlice>, ITagNode
    where TIn : IEquatable<TIn>;

public sealed record ThenParser<TIn, T1, T2, TFirst, TSecond>(TFirst First, TSecond Second)
    : IParser<TIn, (T1, T2)>, IThenNode
    where TIn : IEquatable<TIn>
    where TFirst  : IParser<TIn, T1>     // ← 子の具象型(仮想呼び出し除去の鍵)
    where TSecond : IParser<TIn, T2>;

public sealed record OrParser<TIn, TOut, TFirst, TSecond>(
        TFirst First, TSecond Second, ExpectedSet MergedHead)   // MergedHead=構築時計算の和
    : IParser<TIn, TOut>, IOrNode
    where TIn : IEquatable<TIn>
    where TFirst  : IParser<TIn, TOut>
    where TSecond : IParser<TIn, TOut>;

public sealed record MapParser<TIn, TMid, TOut, TInner>(TInner Inner, Func<TMid, TOut> Selector)
    : IParser<TIn, TOut>, IMapNode
    where TIn : IEquatable<TIn>
    where TInner : IParser<TIn, TMid>;
```

> **規約(型引数の露出)**: 利用者はローカル変数で `var` を用い、型引数を直接書かない。フィールド/プロパティに保持する場合は `IParser<TIn,TOut>` へアップキャストして保持してよい(この経路の呼び出しは仮想呼び出しになるが許容する)。型引数はコンパイルエラー時にのみ露出する。R3(型引数の増加)はこの規約により実害を抑える(§10 参照)。

### 4.4 再帰境界

```csharp
// 参照等価の class(record にしない)。Func + 内部キャッシュで前方参照を実現。
public sealed class DeferredParser<TIn, TOut> : IParser<TIn, TOut>, IDeferredNode
    where TIn : IEquatable<TIn>
{
    public DeferredParser(Func<IParser<TIn, TOut>> factory);
    public ParseResult<TOut> Parse(ReadOnlySpan<TIn> input, int baseOffset); // 初回解決+委譲(一体)
}
```

**規則**:
1. `DeferredParser` は参照等価の `class` とし、`record` の値等価および木の走査はここを切断点とする(循環グラフ上での値等価の無限再帰を構造的に防ぐ)。
2. 解決(ファクトリ実行)は初回 `Parse` 時に一度だけ行い、結果を恒久キャッシュする。解決は冪等かつスレッドセーフであること。
3. ファクトリが `null` を返した場合、および解決中に自分自身の解決を再帰要求した場合は明確な例外とする。
4. 利用上の注意: `Deferred` はフィールド/プロパティ初期化子に 1 つだけ保持すること(アクセスのたびに新インスタンスを生む書き方を避ける)。

### 4.5 構築ファサード

```csharp
public static class Parse
{
    public static TagParser<TIn> Tag<TIn>(ReadOnlyMemory<TIn> pattern, string? name = null)
        where TIn : IEquatable<TIn>;

    // 拡張メソッドの戻り値が具象型引数を保持 → var 推論で仮想呼び出し除去が伝播する
    public static ThenParser<TIn,T1,T2,TF,TS> Then<TIn,T1,T2,TF,TS>(this TF first, TS second)
        where TIn : IEquatable<TIn> where TF : IParser<TIn,T1> where TS : IParser<TIn,T2>;

    public static OrParser<TIn,TOut,TF,TS> Or<TIn,TOut,TF,TS>(this TF first, TS second)
        where TIn : IEquatable<TIn> where TF : IParser<TIn,TOut> where TS : IParser<TIn,TOut>;

    public static MapParser<TIn,TMid,TOut,TInner> Map<TIn,TMid,TOut,TInner>(
            this TInner inner, Func<TMid,TOut> selector)
        where TIn : IEquatable<TIn> where TInner : IParser<TIn,TMid>;

    public static DeferredParser<TIn,TOut> Deferred<TIn,TOut>(Func<IParser<TIn,TOut>> factory)
        where TIn : IEquatable<TIn>;

    public static IParser<TIn,TOut> ExpectMsg<TIn,TOut>(this IParser<TIn,TOut> p, string name)
        where TIn : IEquatable<TIn>;
}
```

---

## 5. コンビネーター語彙(MVP)

命名は Haskell Parsec を基準とし、C# の慣習に合わせて衝突・記号系のみ調整する(§10 参照)。

### 5.1 末端

| 名称 | 役割 |
|------|------|
| `Tag` | リテラル(要素列)一致。Parsec の `string`、nom の `tag` に相当(`string` 型との混同を避け `Tag` を採用)。 |
| `Take` | 固定長の取得。 |
| `TakeWhile` / `TakeWhile1` | 述語を満たす要素の連続取得(0 個以上 / 1 個以上)。 |
| `Satisfy` | 述語を満たす 1 要素。Parsec / nom と同名。 |
| `Eof` | 入力終端。 |

### 5.2 組み合わせ子

| 名称 | 役割 |
|------|------|
| `Then` | 連接。 |
| `Or` | 選択(Parsec の `<\|>`、nom の `alt`)。 |
| `Map` | 出力変換。 |
| `Many` / `Many1` | 反復(0 回以上 / 1 回以上)。**ループ実装**とする(§9.3)。 |
| `SeparatedBy` | 区切り反復(Parsec の `sepBy`)。 |
| `Optional` | 省略可。 |
| `ExpectMsg` | 期待集合の命名(Parsec の `<?>`)。意味論は §7.2。 |
| `Deferred` | 前方参照(再帰境界)。 |

### 5.3 ビット世界

`TakeBits`、および境界アダプター `Bits` / `Bytes`。`Then` / `Or` / `Map` はビット世界にも提供する(§8)。

> **設計判断**: 各コンビネーターは「入力→出力の意味・消費量・失敗時の挙動・期待集合への寄与」を意味論として規定し、内部アルゴリズムは記述しない。`char.IsDigit` 等の標準述語を `Satisfy` / `TakeWhile` の引数として用いることを許可する(専用ノードを増やさない)。

---

## 6. 入力モデルと位置の座標系

- 入力は `ReadOnlySpan<TIn>`。`TIn` は `IEquatable<TIn>` を満たす任意の値型(汎用設計)。**MVP の検証対象は `char`(JSON)と `byte`(バイナリ / ビット解析)の 2 型**に限定する。トークン列入力は汎用設計上は可能だが MVP の検証対象外。
- `SpanSlice` の `Start`、および `ParseError.Position` は、**常に元入力の先頭からの絶対オフセット**とする。これにより成功結果とエラー位置が同一座標系に揃い、`original.Slice(Start, Length)` がそのまま正しい部分範囲を返す。
- 合成ノードは、子の失敗位置・結果位置に自身の消費量を加算して絶対化する。この座標系の統一のため、`Parse` は `baseOffset`(値型 `int`)を引き回す。

---

## 7. エラー設計

### 7.1 2 層構成

| 層 | 役割 | アロケーション |
|----|------|----------------|
| 第 1 層: `ParseError` | ホットパスを流れるデータ。`Position`(絶対オフセット) + `Expected`(構築時キャッシュ済みの `ExpectedSet` への参照)。 | なし(値コピーのみ) |
| 第 2 層: `ParseErrorFormatter` | 失敗後にのみ動く表示生成。行・桁換算、該当行抜粋、キャレット生成。 | 許容(失敗時のみ) |

```csharp
public static class ParseErrorFormatter
{
    public static string Format(ReadOnlySpan<char> input, ParseError error);       // 行桁・抜粋・キャレット
    public static string FormatBytes(ReadOnlySpan<byte> input, ParseError error);  // 16進ダンプ形式
}
```

### 7.2 意味論と規則

1. **`TError` 廃止** — エラー型のジェネリック化(nom の `E`)は行わず、固定の `ParseError` に統一する。エラーファクトリ機構も持たない。
2. **期待集合の構築時キャッシュ** — `ExpectedSet` の生成・結合(`Union`)は構築時のみ許され、解析時は事前計算済みオブジェクトへの参照コピーのみ。これが失敗パスのゼロアロを保証する根拠。
3. **`ExpectMsg(name)` の意味論** — 「期待される対象の名前」を与える。失敗時の出力は `expected <name>` 形式に組み込まれる。`Or` で合流した場合は `expected <A> or <B>` のように連結される。完成文を添えるものではない。
4. **`Or` の失敗採用規則(farthest-failure)** — 全分岐が失敗した場合、より深く(大きい `Position` まで)進んだ失敗を採用する。
5. **同位置失敗の近似** — `Or` の複数分岐が同一位置で失敗した場合、構築時に計算済みの「先頭期待集合の和(`MergedHead`)」を返す。これは厳密な集合の**近似**である(分岐深部での同位置失敗とはずれ得る)。厳密な集合を実行時に構築するとアロケーションが発生するため、ゼロアロ要件との整合点として近似を採用する。

### 7.3 ゼロアロケーションの受け入れ基準

- 代表文法(MVP の JSON)について、**成功パス・失敗パスの双方**で、BenchmarkDotNet の MemoryDiagnoser 計測が 0 B/op であること。
- 例外 1: 利用者の意味作用(`Map` のラムダが結果オブジェクトを生成する等)。これは利用者が注文した成果物のコストでありライブラリの保証範囲外。
- 例外 2: コレクションを返す反復系(`Many` が `List<T>` を返す等)。要素数が実行時に決まる以上、成果物そのものがアロケーションになる。緩和策として `Fold`(アキュムレーターへ畳み込み)・個数のみ数える/読み飛ばす系を段階的語彙として提供する。

---

## 8. ビット解析(境界アダプター方式)

nom の `bits` / `bytes` と同型の「二世界 + 境界アダプター」方式を MVP に含める。

```csharp
public readonly ref struct BitInput
{
    public ReadOnlySpan<byte> Bytes { get; }
    public int BitOffset { get; }     // 絶対ビット位置
    public BitInput(ReadOnlySpan<byte> bytes, int bitOffset);
}

public readonly record struct BitParseResult<TOut>(
    bool Success, int ConsumedBits, TOut Value, ParseError Error);

public interface IBitParser<TOut>
{
    BitParseResult<TOut> Parse(BitInput input);
}

public static partial class Parse
{
    public static IBitParser<uint> TakeBits(int count, string? name = null);
    public static IParser<byte,TOut> Bits<TOut>(IBitParser<TOut> inner);   // nom: bits(バイト→ビット世界)
    public static IBitParser<TOut> Bytes<TOut>(IParser<byte,TOut> inner);  // nom: bytes(ビット→バイト世界)
}
```

**規則**:
1. ビット読み取りは **MSB ファースト**(バイト内の最上位ビットから)。LSB ファーストは将来拡張。
2. `Bytes` でバイト世界へ復帰する際、中途半端なビット位置は**次のバイト境界へ切り上げ**る(端数ビットは破棄)。
3. ビット世界内の位置・消費量は**ビット単位**。`Bits` 境界を出るときにバイト単位へ換算する。
4. ビット世界へ入れるのは `byte` 入力のパーサーのみ(型制約でコンパイル時に保証)。

---

## 9. 最適化

### 9.1 起動インターフェースと位置づけ

最適化は明示 API(木→木の純粋関数)として起動する。最適化は `IParser` の世界(データの顔)で行い、**最適化後は仮想呼び出しになってよい**(原則 5「観測可能な結果の不変性」のみ守る)。

```csharp
public interface IOptimizationPass
{
    IParser<TIn,TOut> Rewrite<TIn,TOut>(IParser<TIn,TOut> node) where TIn : IEquatable<TIn>;
}

public static class ParserOptimizer
{
    // 不変条件:観測可能な結果を変えない。Deferred は境界として訪問済み管理し循環を停止する。
    public static IParser<TIn,TOut> Optimize<TIn,TOut>(
        IParser<TIn,TOut> root, params IOptimizationPass[] passes) where TIn : IEquatable<TIn>;
}
```

### 9.2 メカニズムと走査規約

- 各最適化パスは純粋関数(元の木を破壊せず新しい木を返す)。
- エンジンはボトムアップ(子を先に最適化してから親に当てる)で適用し、書き換えが新たな書き換え機会を生む場合に対応する(反復の停止条件は実装で規定)。
- 最適化器は**データの顔(非ジェネリックビュー `INode` 系)**でノード種別を判定・走査する。型引数付き具象型でパターンマッチしようとすると型引数の取り出しが C# では困難になるため、非ジェネリックビューを用いる。書き換え結果は `IParser<TIn,TOut>` の境界で 1 回キャストして返す。
- `Deferred` は境界として扱い、訪問済み管理で循環を停止する。

### 9.3 MVP に含める最適化の範囲

効果分析の結果、**「先頭文字で枝を確定 / 枝刈りする」テーマ**のみがオーダーを変える価値ある最適化であると判断した。定数倍の削減にとどまる最適化(Map 融合、Tag 結合など)は枠組みのみ用意し、MVP では実装しない。

| 項目 | 位置づけ |
|------|----------|
| **first-set による枝確定 / 枝刈り(代表:Or の先頭文字ジャンプテーブル化)** | MVP に 1 本含める。`value = object \| array \| string \| ...` のような分岐を、先頭文字での O(1) ディスパッチに変換し、平均試行回数を削減する。 |
| `Many` / `Many1` のループ実装 | 最適化パスではなく**初期設計に吸収**する(再帰実装による呼び出し段数の増加を最初から回避)。 |
| 定数倍系パス(Map 融合・Tag 結合・Then 平坦化等) | 枠組みのみ。MVP では非実装。 |

> **ジャンプテーブル化のアルゴリズム概要(非規範・説明用)**: 解析フェーズで各ノードの first-set(成功し得る先頭要素集合)を非ジェネリックビュー経由で算出し、`Or` チェーンを平坦化する。全分岐の first-set が互いに素かつ具体的であれば、先頭要素→分岐 のディスパッチ表を持つノードへ書き換える。互いに素でなければ逐次試行のまま残す。

---

## 10. ソースジェネレーター(SG)

### 10.1 目的と方針

SG の目的は、最速のパーサーを得るために**ノード境界の仮想呼び出しを、具象型経由の通常メソッド呼び出しに変える**ことである。完全な 1 枚岩へのインライン展開は狙わず、メソッドのインライン化は JIT(`AggressiveInlining` 指定)に委ねる。

- SG は具象型を明示的に組み立てる解析コードを生成する。SG を使わない場合も、`var` による型推論で同じ具象型が得られる(§4.3 の規約)。
- 各ノードの `Parse` には `[MethodImpl(MethodImplOptions.AggressiveInlining)]` を付与することをライブラリ規約とする。
- 仮想呼び出しは「除去」ではなく「具象型保持により最初から発生させない」と定義する。
- 利用者ノードも具象型で保持される限り同じ扱いとなり、**利用者の追加実装は不要**(展開テンプレート等を書かせない)。

### 10.2 起動方法と切り替え

属性付与により起動する(C# の `[GeneratedRegex]` に類似)。利用者の呼び出しコードは、SG あり/なしで不変であることを要件とする。

- 文法定義本体(コンビネーターの組み合わせ)は SG あり/なしで共通。差分は宣言形と属性のみ。
- 何万回ループの高速用途では、具象型をループ外で取得し `var` で受けてループ内で使い回すことで、入口の仮想呼び出しも発生しない。

### 10.3 SG が完全な具象化に失敗し、境界(仮想/間接呼び出し)を作るケース

以下のケースでは、SG が具象化しきれず境界が残る。これらは「SG が扱える文法の制約」「許容する間接呼び出し」として明示する。

| ケース | 境界が残る理由 | 対処方針 |
|--------|----------------|----------|
| 再帰(自己参照) | 具象型が無限に深くなり、型として存在しない | `Deferred` 境界で 1 クラス(生成メソッド)を作り、静的メソッド呼び出しで結ぶ |
| 動的合成(実行時に木の形が変わる) | コンパイル時に形が不明 | SG 対象外。実行時に木を組み仮想呼び出しで走る(二経路) |
| 配列合成(可変長で束ねる) | 配列要素が型消去される | 二項合成へ正規化すれば回避可。配列 API は非インライン経路 |
| Map セレクタ / 述語デリゲート | ラムダ本体は展開不能 | 間接呼び出しを許容(型変換・判定の本質的コスト)。`char.IsDigit` 等も許可 |
| DAG 共有(同一部品の使い回し) | 完全展開するとコード複製・爆発 | 共有点を 1 メソッドに切り出して呼ぶ(再帰境界と同様) |
| ジェネリックな文法定義 | 型引数の具体化が呼び出し箇所依存 | 呼び出し箇所から具体化型を収集して生成 |
| パラメータ化構築 | 引数で木の形が変わる | `[GenerateParser]` 対象は引数で形を変えない制約 |
| ローカル関数 / ラムダ内定義 | 構文解析の難度 | 文法定義は式本体またはトップレベルで書く制約 |
| 外部アセンブリのパーサー | 構文ツリーが不可視(IL のみ) | 境界として扱い仮想呼び出し 1 段を許容 |

> **本質的な限界**: 再帰境界の 1 段の間接呼び出しは、型システムと計算の原理的限界により、いかなる手段でも回避できない(具象型が無限で存在しないため)。これは struct 単相化を採っても同じだけ残る。一方、非再帰部分は具象型保持により仮想呼び出しを除去できる。「SG は万能ではないが、制約を明示し境界を許容すれば、実用文法のホットパスの大半の仮想呼び出しを除去できる」が結論である。

> `AggressiveInlining` 委譲方針(完全インラインを狙わない)を採ったことで、DAG 共有・パラメータ化・ローカル関数・述語デリゲートの諸ケースは「呼び出しの形を変えるだけ」で扱え、問題の多くが解消または大幅緩和される。

---

## 11. 設計判断ログ

| # | 判断 | 理由 | 却下した代替案 |
|---|------|------|----------------|
| 1 | データ部と実行部を**統合**(分離しない) | 子の具象型を型引数で保持すれば実行の顔で仮想呼び出しを除去でき、非ジェネリックビューでデータの顔も持てるため、最適化とディスパッチ除去が両立する | 物理的な 2 クラス分離(ファクトリ辞書方式・ダブルディスパッチ方式は型の取り回しが C# で煩雑) |
| 2 | ノードは**参照型 record** | コンパイラ的最適化(ジャンプテーブル化)はノードをデータとしてパターンマッチ書き換えすることに依存し、これは参照型 record が自然 | struct 単相化(最適化器の走査が型システムと衝突し、再帰で詰まり、データ性が型に埋もれる) |
| 3 | 子の具象型を**型引数で保持** | 実行時の仮想呼び出しを除去するための唯一の静的手段。`var` 推論で利用者には露出しない | interface 型フィールド保持(常に仮想呼び出し) |
| 4 | 最適化器は**非ジェネリックビュー**で走査 | 種別でのパターンマッチが可能になる。ジェネリックビューでは型引数を取り出せずマッチが書けない | ジェネリックビュー `INode<TIn,TOut>`(種別判定が不能) |
| 5 | `TError` を**廃止**し固定 `ParseError` | C# では `TError` の生成手段が乏しくエラーファクトリの引き回しが全域に及ぶ。型引数が減り R3 が緩和。失敗パスのゼロアロは固定 struct で達成可能 | nom 流の `TError` ジェネリック維持 |
| 6 | エラーは**2 層 + 期待集合の構築時キャッシュ** | C# 圏の標準(Pidgin 流の expected 集合)の診断品質を、ホットパスのゼロアロを壊さずに得る | nom 流(位置 + 種別コードのみ。診断が貧弱)/ Pidgin 流の実行時マージ(失敗パスでアロケーション) |
| 7 | `ErrorKind` 列挙を**持たない** | 閉じた列挙は利用者パーサーの追加を妨げる(開放性違反)。期待集合の文字列ラベルで識別する | 閉じた enum / 拡張可能識別子オブジェクト |
| 8 | 前方参照は `Deferred`(**Func + 内部キャッシュ**) | `Set` の呼び忘れ・順序管理が不要で宣言的に書ける。最適化パスとの両立のため走査は構築完了後 | `Set` 方式の空箱 / 純粋な Func 遅延(初回 Parse まで木が未完成で最適化と相性が悪い) |
| 9 | ビット解析は**境界アダプター方式** | nom で実証済み。record の木・最適化・命名の設計をそのまま適用でき、ビットを使わない利用者にコストを課さない | 入力モデル全体のビット対応一般化(全利用者にオフセット引き回しコスト) |
| 10 | 命名は **Parsec 基準 + C# 化** | パーサーコンビネーターの語彙の事実上の基準点が Parsec。C# 利用者の自然さと既知性を両立 | nom 完全準拠 / 完全独自 |
| 11 | `SpanSlice` は**絶対オフセット** | 成功結果とエラー位置の座標系が揃い、結果スライスをそのまま元入力に適用できる | 相対オフセット(組み立てのたびに補正連鎖) |
| 12 | 最適化は **first-set テーマ 1 本**を MVP に | オーダーを変える効果があるのはこのテーマのみ。定数倍系は手書きでも得られ実装の正当性が薄い | 複数の定数倍パスを MVP に含める |
| 13 | SG は **`AggressiveInlining` 委譲**(完全インラインを狙わない) | 仮想→通常呼び出しの変換に徹すれば実装難度が下がり、DAG・パラメータ化・述語等の問題が解消/緩和する | 文法全体の 1 枚岩展開(利用者ノードの展開規則が必要で開放性を破る) |

---

## 12. 非目標(Non-Goal)

| 項目 | 扱い |
|------|------|
| 型ごとの直書き特殊化(利用者が手作業でインライン展開用クラスを書く) | 見送り。SG による具象型保持で機械的に代替する(動機 (2) は SG 経由で実現) |
| エラーの文脈チェーン表示(nom の context チェーン相当) | 将来枠。MVP は `ExpectMsg` による期待集合の命名まで |
| トークン列入力(`IParser<MyToken,...>`)の検証 | 汎用設計上は可能だが MVP の検証対象外 |
| 自動最適化(初回 Parse 時に暗黙適用) | 明示 API のみ。自動はフェーズ 2 |
| SG 本体の実装 | フェーズ 2。**SG の詳細仕様は §14 に定義済み**(実装可能な水準)。MVP は [変換] 段のインターフェースのみ定義(既定実装は恒等)。AOT 必須か否か・式木コンパイル併用の最終判断もフェーズ 2 |
| LSB ファーストのビット読み | MVP は MSB ファーストのみ |

---

## 13. 検収範囲(MVP)

- **文法**: JSON(再帰・`Or` 分岐・反復・区切りを含む)を完全に解析できること。
- **バイナリ**: IPv4 ヘッダ程度のビット解析(`TakeBits` / `Bits` / `Bytes`)ができること。
- **性能**: §7.3 のゼロアロケーション受け入れ基準を満たすこと(成功・失敗パス双方)。
- **最適化**: first-set ジャンプテーブル化が、観測可能な結果を変えずに適用でき、`Or` 分岐の多い文法で測定可能な高速化を示すこと。
- **入力型**: `char` と `byte` の 2 型で上記を検証する。

---

## 14. ソースジェネレーター(SG)詳細仕様

> 本章は SG を実装可能なレベルまで詳細化する。SG あり/なしで利用者の文法定義(設計図)は共通であり、SG はノードの実装ロジックを二重に持たない(既存のノード型を具象型で組み立てるコードを生成するだけ)。

### 14.1 SG の責務(一文での定義)

SG の責務は、**設計図(コンビネーター式)が組み立てる木を、可能な限り具象型を逆算して具象型で実体化したコードを生成し、ノード境界の仮想呼び出しを除去すること**である。完全な 1 枚岩へのインライン展開は狙わず、メソッドのインライン化は JIT(`AggressiveInlining` 指定)に委ねる。具象型を逆算できない箇所は `IParser` 経由の呼び出し(仮想呼び出し 1 段)に退避する。これにより生成は常に機能的に正しく、誤生成は起こらない。

### 14.2 入力認識(方式 I:構文木解析)

SG は設計図メソッドの**本体(`=>` 式本体、または `{ }` ブロック本体)を Roslyn 構文木として解析**し、コンビネーター呼び出しの連鎖を辿って木の形を復元する。属性 DSL や型レベル表現は採らない。これにより、SG あり/なしで設計図のコードが完全に共通になる。

### 14.3 属性

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class GenerateParserAttribute : Attribute
{
    public string? TypeName { get; init; }                 // 具象型モード時の生成型名(省略時は規約導出)
    public ParserKind Kind  { get; init; } = ParserKind.Class;   // 既定 class(参照型)
}
public enum ParserKind { Class, Struct }
```

### 14.4 生成物:戻り値型による 2 モード

設計図(入口メソッド)の**戻り値型の書き方**で、生成物と性能特性が決まる。

#### モード 1:`IParser<TIn,TOut>` で受ける(柔軟・入口 1 段は仮想呼び出し)

```csharp
// --- 設計図 ---
public partial class JsonParser
{
    [GenerateParser]
    public static partial IParser<char, JsonValue> Value();
    static IParser<char, JsonValue> ValueImpl() => Str().Or(Number()).Or(Arr());
}

// --- SG 生成(イメージ・非規範)---
public partial class JsonParser
{
    private static readonly IParser<char, JsonValue> _value =
        /* 具象型 OrParser<...> で組み立て。木の内部は仮想呼び出しゼロ */;
    public static partial IParser<char, JsonValue> Value() => _value;
}

// 利用側:入口の Parse だけ仮想呼び出し 1 段、内部はゼロ
var r = JsonParser.Value().Parse(input, 0);
```

#### モード 2:具象型名で受ける(高速・入口もゼロ)

戻り値に書いた**型名がそのまま生成される参照型クラスの名前**になる。

```csharp
// --- 設計図 ---
public partial class JsonParser
{
    [GenerateParser]
    public static partial ValueParser Value();             // ★戻り値に型名 ValueParser を記載
    static IParser<char, JsonValue> ValueImpl() => Str().Or(Number()).Or(Arr());
}

// --- SG 生成(イメージ・非規範)---
public sealed partial class ValueParser : IParser<char, JsonValue>   // 参照型 class
{
    private readonly OrParser<char, JsonValue, /*...具象型...*/> _root = /* 組み立て */;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ParseResult<JsonValue> Parse(ReadOnlySpan<char> input, int off) => _root.Parse(input, off);
}

// 利用側:入口も具象型 → 全段仮想呼び出しゼロ。何万回ループはこのモードを使う
var p = JsonParser.Value();
for (int i = 0; i < 100_000; i++) p.Parse(data[i], 0);
```

`ValueParser` は `IParser<char,JsonValue>` を実装した参照型なので、SG なし経路(`IParser` を返す `ValueImpl`)と型の世界が連続する(原則「同じ参照型を使う」を満たす)。

### 14.5 具象型検索(Concrete Type Resolution)

SG は「境界」と諦める前に、可能な限り具象型を逆算する。

- 変数の**宣言型**ではなく**初期化式の型**を見る(`IParser<...> x = Str();` でも `Str()` の具象型を採用)。
- 呼び出し先メソッド(同一コンパイル単位内)の**本体を再帰的に解析**し、`IParser` 戻り値でも具象型を逆算する。
- 逆算に成功した範囲は具象型で組み立て、仮想呼び出しを除去する。
- 逆算が**原理的に不能**な箇所(§14.9)は、その部分式のみ `IParser` として受けて呼び出す(仮想呼び出し 1 段)。木全体は壊れない。

### 14.6 再帰の扱い

モード 2 で再帰文法を生成する場合、**再帰の相手を具象型名で参照する**ことで無限型を回避する。`ValueParser`・`ArrParser` のように利用者が名付けた参照型クラス同士が、フィールドとして互いを(参照型として)保持し合うことで循環が閉じる。これにより再帰境界の呼び出しは**仮想呼び出しではなく具象型クラスへの呼び出し**になる(初期化順は実装で調停する)。`Deferred` の役割は SG なし実行時の循環切断に限定される。

```csharp
// --- 設計図(相互再帰)---
[GenerateParser] static partial ValueParser Value();
static IParser<char,JsonValue> ValueImpl() => Str().Or(Number()).Or(Arr());
[GenerateParser] static partial ArrParser Arr();
static IParser<char,JsonValue> ArrImpl()
    => Parse.Tag("[").Then(Value().SeparatedBy(Parse.Tag(","))).Then(Parse.Tag("]")).Map(static x => /*...*/);

// --- SG 生成(イメージ)---
public sealed partial class ValueParser : IParser<char, JsonValue> { /* 内部で ArrParser を参照 */ }
public sealed partial class ArrParser   : IParser<char, JsonValue> { /* 内部で ValueParser を参照 */ }
```

### 14.7 Map セレクタの扱い

- セレクタは **static ラムダ(`static x => ...`)またはメソッドグループ**(`int.Parse` 等)に限定する。これにより SG はセレクタの構文木を生成コードへ安全に移送できる(キャプチャ対象の解決が不要)。
- **キャプチャありラムダ**(外部変数を捕捉)は SG 対象外とし、その箇所は `IParser` 退避(または当該設計図全体を実行時経路へ)+診断とする。
- セレクタの呼び出し自体はデリゲート間接呼び出しが残る(型変換の本質的コストとして許容)。

### 14.8 反復系・DAG 共有・汎用部品の扱い

| 対象 | 設計図の書き方 | SG の生成 |
|------|----------------|-----------|
| 反復系(`Many`/`SeparatedBy`) | 既定は `List`/配列。ゼロアロは `Fold`/`Count`/`Skip` | 要素パーサーを具象型保持し、ループ内で直接呼ぶ。反復のループ自体はノード実装。成果物のアロケーションは §7.3 例外 2 |
| DAG 共有(ローカル変数で部品を共有) | ブロック本体で `var ws = ...;` を複数箇所参照 | 共有部品を 1 つの `static readonly` フィールドとして 1 回だけ実体化し、各使用箇所が同じ参照を使う(コード複製・状態分裂を防ぐ)。ブロック本体は**単純代入のみ**解析対象 |
| 汎用部品(パラメータ化ヘルパー) | 戻り値型を具象型で書けば追跡、`IParser` で書けばその箇所は境界 | 戻り値の静的型で具象化可否を判定(§14.5 の具象型検索を適用) |

### 14.9 具象型を逆算できない原理的限界(仮想呼び出しが残る箇所)

以下は具象型検索を尽くしても逆算できず、`IParser` 退避(仮想呼び出し 1 段)となる。機能は保たれ、最適化のみ効かない。

| パターン | 逆算できない理由 |
|----------|------------------|
| 再帰の境界 | 具象型が無限に深く、型として存在しない(モード 2 では具象型名参照で静的呼び出しに格下げ可能。モード 1 では入口が `IParser`) |
| 条件分岐・実行時依存の構築 | 引数や実行時値で木の形が変わり、具象型が一意に定まらない |
| ループ・LINQ による動的構築 | 木の構造が実行時のコレクション等に依存し、コンパイル時に不定 |
| 外部アセンブリのパーサー | ソース構文木が見えず(IL のみ)、本体解析による逆算が不能 |
| キャプチャありラムダのセレクタ | ラムダ本体を生成コードへ安全に移送できない |

> **本質的な限界**: 「仮想呼び出しを消す」ことと「あらゆる文法を扱う」ことは両立しない。仮想呼び出しの除去には具象型のコンパイル時確定(構文解析と具象型逆算)が必要であり、それが原理的に不能な箇所は必ず存在する。SG はそうした箇所を**誤生成せず `IParser` 退避する**ことで、機能の正しさを常に保ちつつ、逆算可能な範囲(実用文法のホットパスの大半)で仮想呼び出しを除去する。

### 14.10 SG なしとの一貫性

- 設計図(コンビネーター式)は SG あり/なしで共通。SG なしのときは設計図メソッドが返す素の木をそのまま実行する(全ノード境界が仮想呼び出し)。
- 生成物は既存のノード型(`OrParser` 等、SG なしと同一の参照型)を組み立てたものであり、SG はノードの解析ロジックを二重に持たない。
- 利用者の呼び出しコードは、モード 1 では SG あり/なしで不変。モード 2 では戻り値型に具象型名を書く差分のみ。

### 14.11 設計判断ログ(SG)

| # | 判断 | 理由 | 却下した代替案 |
|---|------|------|----------------|
| S1 | 入力認識は方式 I(本体の構文木解析) | 設計図を通常のコンビネーター式のまま書け、SG あり/なしで共通化できる | DSL 文字列(表現力と学習コスト)/ 型レベル表現(C# の表現力不足) |
| S2 | 戻り値型で 2 モード(`IParser`/具象型名) | 利用者が柔軟性と速度を戻り値型の書き方だけで選べる。具象型名はその名前で生成 | 常に新具象型を別生成(「同じ型を使う/別コードを作らない」原則に反する) |
| S3 | 生成物は既存ノード型の組み立て(専用解析コードを作らない) | ノード実装の二重持ちを避け、SG なしと型の世界を連続させる | パーサー専用の最適化コードを別途生成(二重保守) |
| S4 | 再帰は具象型名参照で閉じる | 参照型クラス同士の相互参照で無限型を回避し、再帰境界を静的呼び出しに格下げ | 型引数での再帰表現(無限型で表現不能) |
| S5 | Map セレクタは static ラムダ/メソッドグループ限定 | 構文木を生成コードへ安全に移送できる(キャプチャ解決が不要) | キャプチャありの閉包再構築(複雑・脆弱) |
| S6 | 具象型検索を最大化し、不能な箇所のみ `IParser` 退避 | 機能の正しさを常に保ちつつ、逆算可能な範囲で仮想呼び出しを最大限除去。誤生成が起きない | ホワイトリスト判定で対象外を弾く(扱える文法が狭まる)/ 設計図を実行して結果保持(仮想呼び出しを全く消せず目的未達) |
