# Nibblr コーディングガイドライン

> 本ガイドラインは、Nibblr の実装コードを書く際のファイル分割・命名・コーディング方針を定める。仕様書(`Nibblr_spec_v2.md`)が「何を作るか(契約・意味論)」を規定するのに対し、本ガイドラインは「どう書くか(実装規約)」を規定する。
>
> **適用範囲**: インデント・改行・空白・括弧位置・`using` 順序・命名の大文字小文字といった、formatter(`dotnet format` 等)および標準アナライザーが自動化できる事項は本ガイドラインに含めない。それらは `.editorconfig` に委ねる。本ガイドラインは **formatter では対応できない、設計意図に基づく判断**のみを定める。

---

## 1. ファイル分割

### 1.1 基本方針:1 コンビネーター = 1 ファイルに凝集

各コンビネーター `Xxx` に関する三位一体の要素 ——「実行の顔(具象 `record`)」「データの顔(非ジェネリックビュー)」「構築の入口(ファサードメソッド)」—— を**単一ファイル `Xxx.cs` に凝集**させる。

```
Tag.cs:
    public sealed record TagParser<TIn>(...) : IParser<TIn, SpanSlice>, ITagNode { ... }
    public interface ITagNode : INode { ... }
    public static partial class Parse { public static TagParser<TIn> Tag<TIn>(...) { ... } }
```

**理由**: これら 3 要素は常に一緒に変更されるため、同一ファイルに置くと凝集度が高まり、コンビネーターの追加・削除がファイル単位になる(開放性原則と物理構造が一致する)。

### 1.2 フォルダ構成

関心事ごとにトップレベルのフォルダを分ける。フォルダ境界は仕様書の関心事の区分に対応する。

```
src/Nibblr/
├── Core/                      … 契約と結果型(全体が依存する土台)
│   ├── IParser.cs             … IParser<TIn,TOut>
│   ├── INode.cs               … INode 基底のみ(各ビューはコンビネーター側)
│   ├── ParseResult.cs         … ParseResult<TOut>
│   ├── ParseError.cs          … ParseError + ExpectedSet
│   └── SpanSlice.cs
├── Combinators/               … 1 コンビネーター = 1 ファイル
│   ├── Tag.cs / Then.cs / Or.cs / Map.cs / Many.cs / SeparatedBy.cs
│   ├── Satisfy.cs / Take.cs / TakeWhile.cs / Optional.cs / ExpectMsg.cs / Eof.cs
│   └── Deferred.cs
├── Bits/                      … ビット世界(境界アダプター)
│   ├── BitInput.cs / IBitParser.cs / BitParseResult.cs
│   └── TakeBits.cs / Bits.cs / Bytes.cs
├── Optimization/              … 最適化器
│   ├── IOptimizationPass.cs / ParserOptimizer.cs
│   └── JumpTablePass.cs
├── Diagnostics/               … エラー整形(第 2 層)
│   └── ParseErrorFormatter.cs
└── SourceGen/                 … ソースジェネレーター(Roslyn 参照の別プロジェクト)
    ├── GenerateParserAttribute.cs
    └── ...
```

**規約**:
- 非ジェネリックビュー(`ITagNode` 等)は、対応するコンビネーターファイルに**同梱**する(ビューは特定コンビネーターと一蓮托生で変更されるため)。`Core/INode.cs` には基底 `INode` のみを置く。
- 依存方向は一方向に保つ(`Combinators → Core`、`Optimization → Core + Combinators`、`Bits → Core` 等)。フォルダ構成で循環依存を物理的に防ぐ。
- SG は Roslyn を参照する**別プロジェクト**とする(アナライザー/ジェネレーターをメインアセンブリと分離する C# の標準に従う)。

---

## 2. 命名規約

### 2.1 コンビネーターの 4 要素の命名

各コンビネーター `Xxx` について、次の規約で機械的に名前を導出する。

| 要素 | 規約 | 例(Tag) | 例(Then) |
|------|------|----------|-----------|
| 具象ノード型 | `{名}Parser` | `TagParser<TIn>` | `ThenParser<TIn,T1,T2,TFirst,TSecond>` |
| 非ジェネリックビュー | `I{名}Node` | `ITagNode` | `IThenNode` |
| ファサードメソッド | `{名}`(Parsec 基準名) | `Parse.Tag(...)` | `.Then(...)` |
| SG 生成型(モード 2) | 利用者が戻り値に書いた型名。推奨デフォルトは `{文法名}Parser` | — | `ValueParser` 等 |

### 2.2 コンビネーター名の原則(Parsec 基準 + C# 化)

1. コンビネーター名は **Haskell Parsec を基準**とし、C# の予約語・BCL と衝突する場合のみ改名する(`string` → `Tag`)。
2. **記号演算子は使わず英単語**にする(`<|>` → `Or`、`<?>` → `ExpectMsg`)。
3. **PascalCase** を用いる(`sepBy` → `SeparatedBy`、`many1` → `Many1`)。

### 2.3 型引数の命名と順序

**命名**:
- 入力要素型: `TIn`
- 出力型: `TOut`、中間出力: `TMid`、複数出力: `T1` / `T2`
- 子の具象型: `TFirst` / `TSecond` / `TInner`(番号ではなく**子の役割を表す名前**にする。型シグネチャの可読性を上げ、R3 を緩和する)

**順序**: `TIn`(先頭) → 出力型群(`T1`,`T2`,`TOut`,`TMid`) → 子の具象型群(`TFirst`,`TSecond`,`TInner`)(末尾)。

> 本質的な型(入力・出力)を前に、実装詳細である子の具象型を後ろに置く。全ノード型でこの並びを固定することで、型引数の順序が予測可能になり、エラーメッセージの可読性が一貫する。

例:
```csharp
ThenParser<TIn, T1, T2, TFirst, TSecond>     // TIn → 出力(T1,T2) → 具象(TFirst,TSecond)
MapParser<TIn, TMid, TOut, TInner>           // TIn → 出力(TMid,TOut) → 具象(TInner)
```

---

## 3. コーディング方針(formatter 非対応分)

formatter および標準アナライザーが矯正できない、設計意図に基づく判断のみを定める。

### 3.1 ゼロアロケーション厳守(設計の核心)

- 解析ホットパス(`Parse` の中)では、`new`(参照型生成)・LINQ・クロージャ生成・ボクシングを**行わない**。
- アロケーションを伴う処理(`ExpectedSet` の生成・結合、デリゲート生成、コレクション準備)は、**構築時**(コンストラクタ・`static` 初期化)に寄せる。
- 例外は仕様書 §7.3 の 2 点(利用者の意味作用、`Many` 系の成果物生成)のみ。

### 3.2 具象型の保持

- 仮想呼び出しを避けたい箇所では、合成の戻り値・変数を具象型で保つ(`var` または具象型引数)。宣言型を `IParser` にして具象型を捨てない。
- `IParser` へのアップキャストは「**意図的に仮想呼び出しを許容する**」箇所(フィールド保持、`Deferred` 境界、SG が逆算不能な箇所等)に限り、その意図をコメントで明示する。

### 3.3 `[MethodImpl(MethodImplOptions.AggressiveInlining)]` の付与

- 各ノードの `Parse`、および小さな合成ヘルパーには付与する(JIT のインライン展開を促す)。
- 付けない箇所の指針: 本体が大きいメソッド、再帰境界(`Deferred`)など、インライン展開が無益または不能な箇所には付けない。

### 3.4 `record` / `class` / `struct` の使い分け(設計上の意味を持つ)

| 用途 | 種別 | 理由 |
|------|------|------|
| ノード(`TagParser` 等) | 参照型 `sealed record` | データ性(値等価・パターンマッチ)と参照共有(DAG)を両立 |
| 再帰境界(`Deferred`) | 参照等価の `sealed class`(`record` にしない) | 値等価の無限再帰を構造的に回避(循環の切断点) |
| 結果型(`ParseResult`/`ParseError`/`SpanSlice`/`ExpectedSet` 以外の値) | `readonly record struct` | 値型・ゼロアロ |
| 期待集合(`ExpectedSet`) | 参照型 `sealed class` | 構築時に確定し解析時は参照共有(キャッシュ) |

### 3.5 `null` と例外の方針

- 解析の失敗は**例外で表現しない**。必ず `ParseResult.Fail(...)` を返す(例外をホットパスに持ち込まない)。
- 例外は「**構築時の誤用**」に限定する(`Deferred` 未解決のまま `Parse`、ファクトリが `null` を返す、`Set` の二重呼び出し等)。

### 3.6 非ジェネリックビューの実装

- 各ノードはデータの顔(`INode` 系)を**明示的インターフェース実装**(`INode IThenNode.First => First;` 等)で提供し、実行の顔(具象プロパティ)と名前・型を衝突させない。
- ビューは最適化器のためのものであり、解析ホットパスからは呼ばない。

### 3.7 XML ドキュメントの方針

- public API には XML doc を必須とする。
- 特にコンビネーターには、仕様書の**意味論**(入力→出力・消費量・失敗時の挙動・期待集合への寄与)を記述し、仕様書とコードを対応させる。

---

## 4. 適用とレビュー

- 本ガイドラインの §3 は、コードレビューで人手確認する対象(formatter が検出できないため)。特に §3.1(ゼロアロ)は、BenchmarkDotNet の MemoryDiagnoser による受け入れ基準(仕様書 §7.3)と合わせて検証する。
- §1・§2 は新規コンビネーター追加時のチェックリストとして用いる。
