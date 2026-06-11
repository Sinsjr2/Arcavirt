# Nibblr 技術仕様書 (v1)

ゼロアロケーション・パーサーコンビネーターライブラリ for .NET（マルチプラットフォーム / Unity IL2CPP 対応）

| 項目 | 値 |
|---|---|
| ライブラリ名 | **Nibblr** |
| 読み方 | ニブラー |
| 由来 | `nibble`(4ビット = 半バイト / 「少しずつかじる」)＋ `-r`。Rust の `nom`(nom nom = 食べる音)へのオマージュ。バイト指向・ゼロアロケーションを名前で示唆する造語。検索一意性を重視して一般単語を避けた |
| ベースライン TFM | **netstandard2.1**（Unity / IL2CPP を含む全環境互換） |
| 任意の追加 TFM | `net10.0`（対応環境のみの拡張機能を加える。§11） |
| 系譜 | Rust `nom` の設計を C# に移植。意味論(コンビネーター合成・既定バックトラック・`cut`・streaming/complete 二系統)を踏襲。ディスパッチは neuecc 流の値型静的ディスパッチ |
| ステータス | 設計確定。実装着手可能 |

> **重要な方針転換（マルチプラットフォーム対応）**: 当初は .NET 10 の `allows ref struct`(C# 13)を土台に据えていたが、Unity IL2CPP では `allows ref struct` が使えない（Unity の CoreCLR 移行は 2026 年末の 6.8 で .NET 8 = C# 12 から開始予定であり、IL2CPP は AOT バックエンドとして最先端言語機能を追わない）。そのため `allows ref struct` を「土台」から「net10.0 限定の任意拡張」へ降格し、**`allows ref struct` に依存しないポータブル設計をベースライン**とする。これにより単一コードベースで Unity IL2CPP を含む全環境で動作する。

---

## 1. 概要

Nibblr は、Rust の `nom` に相当する**汎用パーサーコンビネーターライブラリ**を C# で実現するものである。第一目的は「汎用かつ正しく、ホットパスがゼロアロケーションなコンビネーター基盤」を提供することであり、最初の実証対象として GDB リモートシリアルプロトコル(RSP)のコマンド解析を用いる。

パーサーは小さな関数的部品(プリミティブ)を合成して組み立てる。各部品は「入力を消費して『残りの入力』と『結果』を返す」という統一形を持ち、`Alt`(分岐)・連接・変換などのコンビネーターで合成される。`nom` 同様、別個のレクサ・トークナイザ段を持たず、単一パスで入力列を直接解析する。

---

## 2. 設計目標と非目標

### 2.1 目標

- **汎用性**: 特定フォーマットに依存しない、再利用可能なコンビネーター基盤。要素型をジェネリック化(`ReadOnlySpan<TItem>`、`TItem` は byte / char / トークン型など)。
- **マルチプラットフォーム**: netstandard2.1 ベースラインで、Mono / CoreCLR / **IL2CPP(AOT)** を含む全環境で動作する。
- **ホットパスのゼロアロケーション**: 一度構築・キャッシュしたパーサーによる解析処理はヒープ確保を一切行わない（繰り返し系で利用者が `List` を選んだ場合等の明示的オプトインを除く）。
- **ゼロコピー**: 入力の部分列は元バッファへの参照で表す。ベースラインでは `SpanSlice`(オフセット+長さ)として返し、呼び出し側が `ReadOnlySpan` に復元する（理由は §2.2）。
- **静的ディスパッチ**: コンビネーター合成は値型ジェネリック構造体で表現し、JIT/AOT のモノモーフィズ化により仮想呼び出しを排し、インライン展開を可能にする(neuecc 流)。
- **nom 忠実**: 既定バックトラック、`Error`/`Failure` 二段エラー、`cut`、streaming/complete 二系統、`Incomplete`/`Needed`、`TError` ジェネリック。

### 2.2 非目標 / ベースラインでの割り切り

- パーサー**構築時**のゼロアロケーション（構築時のヒープ確保は許容。キャッシュ前提）。
- 間接呼び出しの完全排除（再帰の `Boxed` シームは仮想呼び出しを許容）。
- **`allows ref struct` への依存**（IL2CPP 非対応のため不使用。net10.0 限定の任意拡張に降格）。
- **ref struct を `TOut`（出力型）にすること**。`allows ref struct` が無いと `TOut` を ref struct に制約できないため、ゼロコピー出力は `ReadOnlySpan` ではなく `SpanSlice`(オフセット+長さ)で表現する。
- **自作 `IInput<T>` 抽象**（ref struct をジェネリック型引数にする必要があり IL2CPP 非対応）。入力は `ReadOnlySpan<TItem>` の直接渡しに固定する。カスタム入力型は net10.0 拡張/将来へ（§11）。
- **`static abstract` インターフェースメンバ**（IL2CPP 対応が不安定）。エラー構築は値型ファクトリ(`IErrorFactory<TError>`)経由とする。
- 先頭バイトジャンプテーブル等の文法特化最適化（将来・スコープ外。§11）。
- 特殊コンビネーター(`escaped` / `permutation` / `many_till` / `is_a` / `is_not` 等)。

### 2.3 対象フレームワーク (TFM)

- **ベースライン: `netstandard2.1`**。Unity 6 系の .NET Standard 2.1 互換レベルで利用可能。`Span<T>` / `ref struct` / 値型ジェネリックを使用するが `allows ref struct` は使わない。
- 必要なら `netstandard2.0`(+ `System.Memory` パッケージ)も追加し、より古い Unity をカバー。
- 任意で `net10.0` をマルチターゲットし、`#if NET10_0_OR_GREATER` でカスタム入力型・ref struct 出力・`static abstract` エラー等の拡張を加える（§11）。
- リフレクション・動的コード生成は不使用（AOT 安全）。

---

## 3. 用語

| 用語 | 意味 |
|---|---|
| パーサー | `IParser` を実装する値。入力を消費し `ParseResult` を返す |
| コンビネーター | 1つ以上のパーサーを受け取り新しいパーサーを返す関数/構造体 |
| プリミティブ | 入力を直接読む最小のパーサー(`Tag`, `TakeWhile` 等) |
| remaining(残り入力) | パーサーが消費した後に残った入力スライス |
| SpanSlice | 入力内の (オフセット, 長さ)。ゼロコピー出力の表現 |
| 既定バックトラック | `Alt` が、先行分岐が `Error` を返したら(消費量に関わらず)次分岐を元の入力で試す挙動 |
| cut | `Error` を `Failure` に昇格させ、以降のバックトラックを禁止する操作 |
| complete / streaming | 入力が完結している前提の系統 / 部分入力を許容し不足時に `Incomplete` を返す系統 |

---

## 4. アーキテクチャ全体図

```
            ┌─────────────────────────────────────────────┐
            │  利用者コード(例: GdbCommandParser)          │
            └─────────────────────────────────────────────┘
                              │ 合成
                              ▼
   ┌──────────────────────────────────────────────────────────┐
   │  コンビネーター(値型ジェネリック構造体)                  │
   │   Map<…> / Alt<…> / Preceded<…> / Opt<…> / Fold<…> …       │
   │   ── すべて IParser<TItem,TOut,TError> を実装             │
   │      (値型 → 静的ディスパッチ・AOT 安全)                 │
   └──────────────────────────────────────────────────────────┘
        │ 実装                        │ 再帰のときだけ
        ▼                              ▼
   ┌─────────────────┐         ┌──────────────────────────┐
   │ IParser<TItem,   │◄────────│ BoxedParser(仮想シーム)  │
   │   TOut, TError>  │         │  型消去・再帰の輪を閉じる │
   └─────────────────┘         └──────────────────────────┘
        │ 消費                        IL2CPP では型肥大の
        ▼                            分断点としても機能
   ┌──────────────────────────────────────────────────────────┐
   │ 入力: ReadOnlySpan<TItem> を直接渡し（不変）              │
   │   TItem = byte（GDB）/ char / トークン型 …               │
   └──────────────────────────────────────────────────────────┘
        │ 返す
        ▼
   ┌──────────────────────────────────────────────────────────┐
   │ readonly ref struct ParseResult<TItem,TOut,TError>        │
   │   Status(Ok/Error/Failure/Incomplete)                     │
   │   + Remaining(ReadOnlySpan<TItem>) + Value(TOut, 値型)    │
   │   + Needed(Incomplete時) + TError(失敗時)                │
   └──────────────────────────────────────────────────────────┘
```

---

## 5. コア型定義

> 以下の C# シグネチャは設計意図を示すスケッチである。すべて `allows ref struct` / `static abstract` を使わず、IL2CPP(AOT) で動作することを前提とする。

### 5.1 入力の表現

ベースラインでは入力を **`ReadOnlySpan<TItem>` の直接渡し**で表す（自作 `IInput<T>` 抽象は使わない）。要素型 `TItem` はジェネリック（byte / char / トークン型）。`ReadOnlySpan<T>` は引数として渡す限り旧 C# でも許され、`TItem` が ref struct でなければ(=`allows ref struct` 不使用なので保証される)成立する。

不変・関数的に扱い、消費は「残り `ReadOnlySpan<TItem>` を返す」ことで表現する。これによりバックトラックは「同じ入力を渡し直すだけ」で成立する。

### 5.2 ゼロコピー出力 `SpanSlice`

`TOut` を ref struct にできない（§2.2）ため、入力の部分列は **(オフセット, 長さ)** で返し、呼び出し側でスライスに復元する。これにより確保もコピーもしない。

```csharp
public readonly struct SpanSlice
{
    public int Offset { get; }
    public int Length { get; }
    public SpanSlice(int offset, int length) { Offset = offset; Length = length; }
    // 元入力に当てて ReadOnlySpan を復元（ゼロコピー）
    public System.ReadOnlySpan<T> Of<T>(System.ReadOnlySpan<T> source)
        => source.Slice(Offset, Length);
}
```

### 5.3 パーサー契約 `IParser<TItem,TOut,TError>`

```csharp
public interface IParser<TItem, TOut, TError>
    where TError : IParseError
{
    ParseResult<TItem, TOut, TError> Parse(System.ReadOnlySpan<TItem> input);
}
```

- `Parse` の引数 `ReadOnlySpan<TItem>` は直接渡し（ref struct を型引数にしないため `allows ref struct` 不要）。
- 実装は値型ジェネリック構造体。`where TParser : IParser<…>` 制約越しに呼ぶと、値型のとき脱仮想化＋インライン化される（neuecc 流 / §6）。これは IL2CPP の AOT でも、全インスタンス化が静的に到達可能であれば成立する。

### 5.4 結果型 `ParseResult<TItem,TOut,TError>`

`nom` の `IResult = Result<(I,O), Err<E>>` 相当。1値返し・スタックのみ・ゼロアロケーション。`Remaining` が ref struct(span)を含むため `ParseResult` 自体は `ref struct` だが、ジェネリック型引数には使わないので問題ない。

```csharp
public enum ParseStatus : byte { Ok, Error, Failure, Incomplete }

public readonly ref struct ParseResult<TItem, TOut, TError>
    where TError : IParseError
{
    public ParseStatus Status { get; }
    public System.ReadOnlySpan<TItem> Remaining { get; }  // Ok のとき有効
    public TOut Value { get; }                             // Ok のとき有効（値型）
    public TError Error { get; }                           // Error/Failure のとき有効
    public Needed Needed { get; }                          // Incomplete のとき有効

    public bool IsOk => Status == ParseStatus.Ok;

    public static ParseResult<TItem, TOut, TError> Ok(System.ReadOnlySpan<TItem> rem, TOut val);
    public static ParseResult<TItem, TOut, TError> Err(TError e);       // 回復可能
    public static ParseResult<TItem, TOut, TError> Fail(TError e);      // 回復不能(cut済)
    public static ParseResult<TItem, TOut, TError> Incomplete(Needed n);

    // 失敗を別の TOut へ転送（値は持たないので型だけ載せ替え）
    public ParseResult<TItem, TOther, TError> CastError<TOther>();
}
```

### 5.5 `Needed`(不足量)

`nom` の `Needed`(`Size` / `Unknown`)相当。streaming で `Incomplete` を返す際の追加データ量。

```csharp
public readonly struct Needed
{
    public bool IsUnknown { get; }
    public int Size { get; }            // IsUnknown=false のとき有効
    public static Needed Unknown { get; }
    public static Needed Of(int size);
}
```

### 5.6 エラー型 `TError` と構築 `IErrorFactory<TError>`

エラー型は `TError` ジェネリック（nom 完全準拠）。ただし `static abstract`(IL2CPP 不安定)を避け、**値型のエラーファクトリ**経由で構築する。ファクトリも値型ならファクトリ呼び出しも静的ディスパッチされる。

```csharp
public interface IParseError
{
    int Offset { get; }          // 失敗位置（入力先頭からのオフセット）
    ErrorKind Kind { get; }
}

public interface IErrorFactory<TError> where TError : IParseError
{
    TError FromErrorKind(int offset, ErrorKind kind);
    TError AddContext(int offset, string context, TError inner);  // nom の ContextError 相当
}

public enum ErrorKind : ushort { Tag, TakeWhile1, Satisfy, OneOf, Alt, Eof, /* … */ }

// 既定の軽量ゼロアロエラーとそのファクトリ
public readonly struct DefaultError : IParseError { /* Offset, Kind */ }
public readonly struct DefaultErrorFactory : IErrorFactory<DefaultError> { /* … */ }
```

> エラーファクトリは、各コンビネーターに型引数 `TFactory : IErrorFactory<TError>` として持たせるか、ビルダー API(`Parse` ファサード)が既定で `DefaultErrorFactory` を注入する。型引数の増加は §10 R3 のトレードオフとして扱う。

---

## 6. ディスパッチ戦略

### 6.1 静的ディスパッチ(neuecc 流)

各コンビネーターを**値型ジェネリック構造体**で表現し、内側パーサーを型引数として保持する。`where`制約越しに `Parse` を呼ぶと、型引数が値型のとき JIT/AOT がモノモーフィズ化し、インターフェース呼び出しが**直接呼び出しに脱仮想化＋インライン化**される。これが `nom` のゼロコスト性の C# 版。

```csharp
public readonly struct Map<TParser, TItem, TIn, TOut, TError> : IParser<TItem, TOut, TError>
    where TParser : IParser<TItem, TIn, TError>
    where TError  : IParseError
{
    private readonly TParser _inner;
    private readonly System.Func<TIn, TOut> _f;   // 構築時に一度だけ確保（キャッシュ前提）
    public Map(TParser inner, System.Func<TIn, TOut> f) { _inner = inner; _f = f; }

    public ParseResult<TItem, TOut, TError> Parse(System.ReadOnlySpan<TItem> input)
    {
        var r = _inner.Parse(input);   // ← 値型なら直接呼び出し＋インライン
        return r.IsOk
            ? ParseResult<TItem, TOut, TError>.Ok(r.Remaining, _f(r.Value))
            : r.CastError<TOut>();
    }
}
```

### 6.2 再帰の `Boxed` シーム

再帰文法は型が無限サイズになるため値型のままでは表現できない。`nom` が `Box<dyn Parser>` で輪を閉じるのと同様、Nibblr は **`BoxedParser`(参照型・仮想ディスパッチ)を一点だけ**置く。ここのみ間接呼び出しを許容。

```csharp
public sealed class BoxedParser<TItem, TOut, TError> : IParser<TItem, TOut, TError>
    where TError : IParseError
{
    public IParser<TItem, TOut, TError> Core { get; set; } = default!;  // 前方参照で輪を閉じる
    public ParseResult<TItem, TOut, TError> Parse(System.ReadOnlySpan<TItem> input)
        => Core.Parse(input);   // ← 仮想呼び出し（再帰ノードのみ）
}
```

### 6.3 IL2CPP(AOT)固有の配慮

- **静的到達性**: すべてのジェネリック具体化が静的に到達可能であること（利用者がパーサーを静的合成する限り満たす）。リフレクションでパーサーを動的構築してはならない。
- **コードサイズ肥大**: `Map<Alt<…>>` の深い入れ子は具体型を量産し、IL2CPP のビルドサイズ・時間を増やす（§10 R1）。緩和策は (a) モジュール境界で `BoxedParser` を挟んで具体型の連鎖を分断、(b) 将来のソースジェネレーターで具体型を生成、(c) サイズ重視ビルド向けに `BoxedParser` 主体の「compact モード」を提供。
- **型肥大化のエルゴノミクス**: `static class Parse` のファクトリ(`Parse.Tag(...)`, `Parse.Alt(...)`)に集約し、利用者が巨大なジェネリック型名を書かずに済むようにする。

---

## 7. コンビネーター MVP(v1 範囲)

すべて complete / streaming の**二系統**を持つ(`Nibblr.Complete` / `Nibblr.Streaming`)。streaming 版は入力不足時に `Incomplete(Needed)` を返す。complete 版は不足を `Error` として扱う。

### 7.1 プリミティブ

| 名前 | 役割 | 出力 |
|---|---|---|
| `Tag(value)` | 指定列に一致 | `SpanSlice` |
| `Take(n)` | n 要素取得 | `SpanSlice` |
| `TakeWhile(pred)` / `TakeWhile1` | 述語が真の間取得 | `SpanSlice` |
| `TakeWhileMN(m,n,pred)` | m〜n 要素を述語で取得 | `SpanSlice` |
| `TakeTill(pred)` / `TakeTill1` | 述語が真になるまで取得 | `SpanSlice` |
| `TakeUntil(value)` | 指定列が現れるまで取得 | `SpanSlice` |
| `Satisfy(pred)` | 1要素が述語を満たす | `TItem` |
| `OneOf(set)` / `NoneOf(set)` | 集合に含まれる/含まれない1要素 | `TItem` |
| `AnyItem()` | 任意の1要素 | `TItem` |
| `Rest()` | 残り全部 | `SpanSlice` |
| `Eof()` | 入力終端 | （なし） |

### 7.2 分岐

| 名前 | 役割 |
|---|---|
| `Alt(p1, p2, …)` | 既定バックトラックで順に試行。`Failure` で打ち切り |

### 7.3 連接

| 名前 | 役割 |
|---|---|
| タプル連接 `(p1, p2, …)` | 順に適用し結果をタプルで返す |
| `Pair(a, b)` | 2つを適用 |
| `SeparatedPair(a, sep, b)` | a sep b、a と b を返す |
| `Preceded(prefix, p)` | prefix を読み捨て p を返す |
| `Terminated(p, suffix)` | p を返し suffix を読み捨て |
| `Delimited(open, p, close)` | open p close、p を返す |

### 7.4 変換・検査

| 名前 | 役割 |
|---|---|
| `Map(p, f)` | 結果を変換 |
| `MapRes(p, f)` | 失敗しうる変換 |
| `MapOpt(p, f)` | `null` を失敗とする変換 |
| `Value(v, p)` | p 成功時に固定値 v を返す |
| `Opt(p)` | 失敗を成功(値なし)に変える |
| `Recognize(p)` | p が消費した範囲を `SpanSlice` で返す |
| `Consumed(p)` | (消費 `SpanSlice`, 値) を返す |
| `Verify(p, pred)` | 結果が述語を満たすか検査 |
| `Peek(p)` | 入力を消費せず先読み |
| `Not(p)` | p が失敗すれば成功(消費なし) |
| `Cut(p)` | p の `Error` を `Failure` に昇格(コミット) |
| `AllConsuming(p)` | p 後に入力が空であることを要求 |

### 7.5 繰り返し（ゼロアロ核 + 確保版は別レイヤ）

**ゼロアロ核(`Nibblr` 本体)**:

| 名前 | 役割 |
|---|---|
| `Fold(p, init, acc)` / `Fold1` | 結果を畳み込む(コレクション不要) |
| `Count(p)` | 出現回数のみ数える |
| `RepeatInto(p, Span<TOut> buffer)` | 呼び出し側バッファに書き込み(確保なし) |

**確保版コレクタ(別レイヤ `Nibblr.Collections`、nom の `alloc` feature 相当・明示オプトイン)**:

| 名前 | 役割 |
|---|---|
| `ManyList(p)` / `Many1List` | `List<TOut>` に集める |
| `SeparatedList(sep, p)` | 区切り付きで `List<TOut>` に集める |

> 注: `TOut` は値型なので `List<TOut>` に格納可能。要素がスライス由来なら `SpanSlice` のまま集めるか、所有型(`byte[]` 等)へコピーする。前者はゼロコピー（背骨のみ確保）、後者はコピーを伴う点を明記する。

---

## 8. エラーモデル(nom 準拠で確定)

- **二段エラー**: `Error`(回復可能。`Alt` は次分岐を試す) / `Failure`(回復不能。`Alt` も即伝播)。
- **既定バックトラック**: `Alt` は先行分岐がどれだけ消費しても、`Error` なら元の入力で次分岐を試す。`nom` と同じく `try` 相当は不要。
- **`cut`**: `Cut(p)` で「ここまで来たら後戻り禁止」を表現(`Error` → `Failure`)。
- **エラー型ジェネリック**: 全コンビネーターは `TError : IParseError`。構築は値型 `IErrorFactory<TError>` 経由（`static abstract` を使わない）。既定は `DefaultError` / `DefaultErrorFactory`(失敗位置 + `ErrorKind`、ゼロアロ)。
- **複数候補の集約**: `Alt` 失敗時のエラー合成方針はファクトリに委譲(既定は最後/最深の `Error` を返す)。

---

## 9. 実証ケース: GDB stub コマンド解析

チェックサムを除いたコマンド本体(`$` と `#` の間)を入力とする。`TItem = byte`。可変長ペイロードは `SpanSlice`(ゼロコピー)で保持し、呼び出し側が `Of(input)` でスライスに復元する。

### 9.1 出力モデル（値型 = `TOut` に使える）

```csharp
public struct GdbCommand               // 値型（ref struct ではない）。TOut として返せる
{
    public GdbKind Kind;
    public ulong   Addr;
    public ulong   Len;
    public byte    BpType;
    public SpanSlice Payload;          // G / M: / X の生データ。input に当てて復元
}
public enum GdbKind : byte { Step, Continue, ReadRegs, WriteRegs, ReadMem, WriteMem,
                             ReadReg, InsertBp, RemoveBp, HaltReason, Query, Unknown }
```

### 9.2 コンビネーターでの組み立て（イメージ）

```csharp
// m addr,len
var readMem = Parse.Preceded(
    Parse.Tag("m"u8),
    Parse.SeparatedPair(HexU64, Parse.Tag(","u8), HexU64)
).Map(t => new GdbCommand { Kind = GdbKind.ReadMem, Addr = t.Item1, Len = t.Item2 });

// Z<type>,addr,kind
var insertBp = (Parse.Tag("Z"u8), Parse.OneOf("01234"u8),
                Parse.Tag(","u8), HexU64, Parse.Tag(","u8), HexU64)
    .Map(t => new GdbCommand { Kind = GdbKind.InsertBp,
                               BpType = (byte)(t.Item2 - (byte)'0'),
                               Addr = t.Item4, Len = t.Item6 });

var command = Parse.AllConsuming(Parse.Alt(step, cont, readMem, writeMem,
                                           insertBp, removeBp, readRegs, halt, query, unknown));

// 解析と、ペイロードのゼロコピー復元
var r = command.Parse(packet);
if (r.IsOk && r.Value.Kind == GdbKind.WriteMem)
{
    System.ReadOnlySpan<byte> data = r.Value.Payload.Of(packet);  // ゼロコピー
}
```

アドレス・長さ・kind は16進から整数へデコードしてスタック上に保持(ゼロアロ)、`G`/`M:` の16進ペイロードは消費範囲を `SpanSlice` で保持(ゼロコピー)。デコード済みバイト列が必要な場合は呼び出し側バッファへ展開する(§7.5)。

---

## 10. リスクと未決事項

| 項目 | 内容 | 対応 |
|---|---|---|
| **R1: IL2CPP コードサイズ/ビルド時間の肥大** | 深いジェネリック入れ子が具体型を量産し、AOT のバイナリサイズ・ビルド時間を押し上げる。 | (a) モジュール境界で `BoxedParser` を挟み具体型連鎖を分断、(b) 将来ソースジェネレーターで具体型生成、(c) サイズ重視向けに `BoxedParser` 主体の compact モード提供。実機 IL2CPP でサイズ計測を早期に行う。 |
| **R2: AOT ジェネリック到達性** | IL2CPP は未到達のジェネリック具体化を生成できない。 | パーサーは静的合成に限定。**リフレクション/動的生成でパーサーを組まない**ことを規約化。 |
| **R3: 型引数の増加** | `TItem` + `TOut` + `TError`(+ ファクトリ)で型引数が増え可読性が落ちる。 | `static class Parse` ファサードが既定 `DefaultErrorFactory` を注入し、利用者の型記述を最小化。`.Boxed()` で均一な `IParser` 型に落とせる。 |
| **R4: `Func` による span キャプチャ不可** | `Map` の変換 `Func` は ref struct(span)をキャプチャできない。 | 変換関数は値（デコード済み or `SpanSlice`）を受ける設計に限定。生スライスは `Recognize`/`Consumed`→`SpanSlice` 経由で取り回す。 |
| **R5: streaming/complete 二系統の保守コスト** | プリミティブが約2倍。 | 内部実装を共有し、不足時の分岐(`Error` vs `Incomplete`)のみ差し替えて重複を最小化。 |
| **R6: 不変 span 渡しの性能** | 各ステップで span(16バイト)を渡し直すコスト。 | インライン化でほぼ消える想定。問題があれば可変 `ref ByteReader` カーソルへの切替を検討（save/restore でバックトラック）。 |

---

## 11. スコープ外 / 将来拡張

- **net10.0 任意拡張レイヤ**（`#if NET10_0_OR_GREATER`）: `allows ref struct` を用いた自作 `IInput<T>` 抽象、ref struct 出力(`SpanSlice` を介さない直接 span 出力)、`static abstract` ベースのエラー構築。非 IL2CPP 環境で API を強化。
- **入力要素型の追加実装**: `CharInput`(UTF-8/UTF-16)、トークン列入力など。
- **特殊コンビネーター**: `escaped` / `escaped_transform` / `permutation` / `many_till` / `is_a` / `is_not` / ビットレベル解析。
- **streaming の充実**: `Needed` の精緻化、部分入力をまたぐステートマシン支援。
- **位置情報**: `nom_locate` 相当の行・列追跡(`LocatedInput`)。
- **先頭バイトジャンプテーブル**: 先頭1要素で `switch`(ジャンプテーブル)分岐する `Dispatch` コンビネーター。
- **ソースジェネレーター**: 既知文法を具体型の入れ子パーサーとしてコンパイル時生成し、`Boxed` シームと IL2CPP の型肥大を同時に削減。

---

## 付録 A: 確定した設計判断ログ

| # | 判断項目 | 決定 |
|---|---|---|
| Q1 | ランタイム / C# バージョン | **改訂**: ベースライン netstandard2.1（`allows ref struct` 不使用）。net10.0 は任意拡張 |
| Q2 | 入力要素型 | ジェネリック化（`ReadOnlySpan<TItem>` 直接渡し。自作 `IInput<T>` 抽象は net10 拡張/将来へ） |
| Q3 | パーサーの表現 / ディスパッチ | 値型ジェネリック構造体 + 静的ディスパッチ(neuecc 流)、再帰のみ `Boxed` シーム |
| Q4 | 入力の受け渡し | 不変 span-in / remaining-out（フォールバック: 可変 ref カーソル） |
| Q5 | 結果・エラーの器 | `readonly ref struct ParseResult<TItem,TOut,TError>`（status: Ok/Error/Failure/Incomplete）。`TOut` は値型、ゼロコピー出力は `SpanSlice` |
| Q6 | ストリーミング | streaming + complete の二系統、`Needed` 型導入 |
| Q7 | 複数結果コンビネーター | ゼロアロ核(fold/count/Span バッファ)＋確保版コレクタは別レイヤ |
| Q8 | エラー型の表現 | `TError` ジェネリック化。構築は値型 `IErrorFactory<TError>` 経由（`static abstract` 不使用） |
| Q9 | v1 コンビネーター範囲 | 汎用コア MVP（特殊系は後回し） |
| — | マルチプラットフォーム | netstandard2.1 ベースラインで Unity **IL2CPP** を含む全環境対応。`allows ref struct` は net10 限定の任意拡張に降格 |
| — | ライブラリ名 | **Nibblr**（ニブラー） |
| — | 仕様書粒度 | 詳細技術仕様書（実装着手可能） |
