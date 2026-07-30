---
name: rx-svd-reference
description: Renesas RXマイコンのiodefine.h + データシートPDFから、CMSIS-SVD形式のレジスタ参考実装(アドレス・ビットフィールド・リセット値・アクセス権限)を生成する。新しいRXマイコン向けにSVDを作る、またはiodefine.h/データシートからレジスタのリセット値・アクセス権限を収集したいときに使う。
---

# RX マイコン SVD 参考実装スキル

Renesas RX ファミリのマイコン(RX64M/RX111 等)について、`iodefine.h`(Cヘッダ、
アドレス・ビットフィールド定義)とデータシートPDFから、CMSIS-SVD形式の
レジスタ参考実装(`.svd`)を生成するための手順。Arcavirt-c0i(RX64M/RX111の
参考実装)で確立した方式を汎用化したもの。

## 前提ツール(このリポジトリに実装済み)

- `doc/spec/RegisterSvdReference/tools/IodefineToSvd/` — iodefine.hをパースし
  CMSIS-SVD XMLを生成するC#ツール。`RegisterDatasheetMetadata`(ResetValue・
  ResetMask・Access)をstruct名ごとのプロバイダとして`Program.cs`に登録する。
- `doc/spec/RegisterSvdReference/tools/IodefineToSvdTest/` — NUnitテスト
  (パーサー単体テスト・実機データ統合テスト・XSD検証・全モジュール再生成検証)。
- `doc/spec/RegisterSvdReference/tools/datasheet_extract/extract_registers.py` —
  データシートPDFのレジスタ節を機械的に構造化抽出するPythonスクリプト
  (標準ライブラリのみ、追加パッケージ不要)。
- `doc/spec/RegisterSvdReference/CMSIS-SVD.xsd` — ARM公式XSD(v1.3.9)。

新しいマイコン向けにこのツール自体が存在しない場合(全く新規のリポジトリ等)は、
このスキルの「レジスタ層の設計判断」節を参照しつつ、同等のC#ツールを新規実装する
(IodefineToSvdの構造をそのまま移植するのが早い)。

## 全体フロー(1モジュール = 1レジスタ群、例: SCI・PORT・GPT等)

### ステップ0: 対象の特定(機械的)

1. 対象struct名をiodefine.hから特定する:
   `grep -n "^struct st_<name> {" iodefine.h`
2. インスタンスのアドレスを特定する:
   `grep -nP '#define\s+<INSTANCE_NAME>\s' iodefine.h`
   (例: `#define GPT0 (*(volatile struct st_gpt0 *)0xC2100)`)
3. データシートの該当章を特定する。テキスト抽出済みの全文
   (`pdftotext -layout datasheet.pdf out.txt`、または既存の抽出済みテキストが
   あればそれ)に対し、章タイトルを検索する:
   `grep -n "^ N\.\s.*<モジュール名>" out.txt`
   前後の章番号でその章の開始行・終了行(次章の開始行)を特定する。

### ステップ1: パーサーがそのstructを解析できるか確認する(★委任しない)

```
dotnet run --project doc/spec/RegisterSvdReference/tools/IodefineToSvd -- \
  <iodefine.hのパス> /tmp/probe.svd <deviceName> st_<name>
```

例外なく完了すればステップ2へ進む。例外が出た場合、それは新しいCの記法パターン
(新種のunion構造・ポインタ型・無名構造体等)を発見したことを意味する。

**この対応は必ず自分(統括)が行い、サブエージェントに委任しない。**
理由: 「実際にツールを適用して例外メッセージを手がかりに1つずつ対応する」という
フェイルファストな探索作業であり、判断を誤ると誤ったサイズ・オフセットのレジスタを
サイレントに生成してしまう(XSD検証は構造しか見ないため、この種の誤りを検出できない)。

過去に発見したパターンの例(`CStructBodyParser.cs`のコメント・
`CStructBodyParserXxxPatternTest.cs`群を参照):
- union内にビットフィールド構造体が丸ごとコメントアウトされている
  (`union { unsigned char BYTE; } PIBR0;`)
- union内配列(`IR[256]`)・素の配列(`DPSBKR[32]`)
- バイト単位のサブメンバーを持つunion(`TDRHL`のTDRH/TDRL)
- 無名union(複数のプレーンメンバーが同一アドレスを共有、例: SCIFAのBRR/MDDR)
  → `CStructMember.AliasNames` → SVDの`alternateRegister`要素で表現
- ポインタ型メンバー(`void *DMSAR`)は`BaseTypeByteSize`が未対応のため例外になる
  (意図的なフェイルファスト設計。対応する場合は型サイズ表に追加する)

新パターンへの対応は `CStructBodyParser.cs`(パース)・`CStructModel.cs`(モデル)・
`RegisterLayoutBuilder.cs`(オフセット計算)・`SvdDocumentBuilder.cs`(XML出力)の
いずれかを拡張する。対応後は**必ず既存の全コミット済み.svdファイルを再生成し、
差分がゼロであることを確認する**(後述の`RegenerateAllSvdTest`が自動化している)。

### ステップ2: データシートからレジスタ節を機械的に抽出する

```
pdftotext -layout -f <開始ページ> -l <終了ページ> datasheet.pdf chapter.txt
python3 doc/spec/RegisterSvdReference/tools/datasheet_extract/extract_registers.py chapter.txt
```

出力は1レジスタ1行のJSON。各レジスタについて:
- `instance_groups`: インスタンスグループ(同一節内でリセット値が異なる例外
  ケース、例: BSCのCS0CR vs CS1-7CRを区別して保持)ごとの
  `reset_value_candidate` / `reset_mask_candidate` / `access_candidate`
- `undefined_bits`: Value after reset行の"x"表記、またはビット説明表の
  "undefined"記載から検出したビット

**この出力は「たたき台」であり、そのまま鵜呑みにしないこと。** 特に以下は
スクリプトのスコープ外(本文の自由記述を読む必要がある):
- レジスタ全体が「動作モード・外部ピン設定に依存する」等の理由で単一の
  固定リセット値を持たない場合(例: SYSCR0、BSCのCS0CR.BSIZE) →
  該当ビットをResetMaskから除外するか、レジスタ全体をnullにするか判断が必要
- `access_candidate`が`null`の場合(Bit/Symbol表を持たないプレーンな
  データレジスタ、例: FTDR/FRDR) → 本文の"read-only register"/
  "write-only register"のような明記を探して判断する
- 複数のインスタンスグループがあるレジスタ群(GPT0-3、PPG0/PPG1等)で、
  本文が本当に「全インスタンス共通」と明記しているか(データ上は同じでも
  本文に反例が無いか)

### ステップ3: RegisterDatasheetMetadataファイルを作成する

命名規約: `Rx<Chip><Module>DatasheetMetadata.cs`
(例: `Rx64mGptDatasheetMetadata.cs`、RX111なら`Rx111<Module>DatasheetMetadata.cs`)

```csharp
namespace IodefineToSvd;

/// <summary>
/// <データシート名・版> の <章番号>章(<章タイトル>, p.<開始>-<終了>)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。<検証した代表
/// レジスタ>についてマニュアル本文と個別に突き合わせて確認済み(<日付>)。
/// <特殊ケース(null表現・部分マスク・インスタンス間の例外)があれば理由を記載>
/// </summary>
public static class Rx<Chip><Module>DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["<レジスタ名>"] = new RegisterDatasheetMetadata(<ResetValue or null>, <ResetMask>, "<read-write|read-only|write-only>"),
        // ...
    };
}
```

設計規約(`RegisterDatasheetMetadata(ulong? ResetValue, ulong ResetMask, string Access)`):
1. **Accessの集約**: SVDの`access`はレジスタ全体で1つ。ビットごとにR/R(/W)/W混在
   なら"read-write"(1ビットでも書込み可能なら read-write)。全ビットRのみなら
   read-only、全ビットWのみ(かつ明記あり)ならwrite-only。
2. **null表現**: 単一の固定リセット値が無い(起動要因・外部ピン・OFS設定等に
   依存)場合、`ResetValue=null`。ResetMaskは意味を持たないため`0`を設定。
3. **部分マスク表現**: レジスタの一部ビットのみUndefinedの場合、定義済み
   ビットの値を`ResetValue`に設定し、`ResetMask`でUndefinedビットを除外する。

### ステップ4: Program.cs・テスト群を更新する

1. `Program.cs`の`datasheetProviders`辞書に`["<chip>-<module>"] = Rx<Chip><Module>DatasheetMetadata.Registers,`を追加。
2. `Rx<Chip><Module>IntegrationTest.cs`を追加(実物のiodefine.hからパースして
   インスタンス生成・レジスタ数を確認する2テスト、既存ファイルを1つ参照して
   同じ構造で作成)。
3. `SvdXsdValidatorTest.cs`に、対象モジュールの`.svd`がXSD検証を通ることを
   確認するテストを追加。
4. `RegenerateAllSvdTest.cs`の`modules`配列・`datasheetProviders`辞書の両方に
   新モジュールを追加(既存モジュールのsvdスナップショットが陳腐化していない
   ことを毎回自動検証するテスト。新モジュール追加時に必ずここへの追加を忘れない)。

### ステップ5: 生成・検証・コミット(★委任しない)

```
dotnet build doc/spec/RegisterSvdReference/tools/IodefineToSvd
dotnet run --project doc/spec/RegisterSvdReference/tools/IodefineToSvd -- \
  <iodefine.hのパス> doc/spec/RegisterSvdReference/<chip>-<module>-pilot.svd \
  <DeviceName> --datasheet=<chip>-<module> st_<name>...
dotnet test doc/spec/RegisterSvdReference/tools/IodefineToSvdTest
dotnet format whitespace doc/spec/RegisterSvdReference/tools/IodefineToSvd/IodefineToSvd.csproj --verify-no-changes
dotnet format whitespace doc/spec/RegisterSvdReference/tools/IodefineToSvdTest/IodefineToSvdTest.csproj --verify-no-changes
```

ビルド・テスト・formatはサンドボックス制約(NuGetキャッシュ書き込み等)により
`dangerouslyDisableSandbox: true`が必要な環境がある。フォーマット違反があれば
`dotnet format whitespace`(--verify-no-changesを外す)で機械的に修正し、
再度テストが通ることを確認してからコミットする。

コミット前に、代表レジスタ(特にnull表現・部分マスクを使った箇所)を
データシート原本と直接突き合わせて検算すること(サブエージェントの判断が
正しいことの最終確認)。

## 分業方針(トークン消費とサブエージェント委任)

| 作業 | 担当 | 理由 |
|---|---|---|
| ステップ0-1(パーサー適用・新パターン対応) | 統括(自分) | 探索的判断、誤ると誤ったSVDをサイレントに生成する |
| ステップ2-4(データシート抽出〜ファイル作成) | サブエージェントに一括委任可 | 完全に仕様化できる機械的作業。ステップ2の抽出スクリプトの出力・命名規約・既存実装例1つを渡せば、データシート読解も含めて任せてよい(転記だけを委任すると二重コストになる) |
| ステップ5(ビルド・テスト・format・検算・コミット) | 統括(自分) | サンドボックス制約・品質ゲート・最終検証の観点 |

サブエージェントへ委任する際は、対象struct名・インスタンスアドレス・データシートの
章番号とページ範囲・上記の設計規約・既存実装例1つのファイルパスを渡し、
「パーサー本体は変更しない」「想定外の事態は自己判断で進めず報告する」ことを
明記する。

## 他のRXマイコンへの汎用化

- iodefine.hのパスとデータシートPDFが変わるだけで、上記フローはそのまま使える。
- チップ固有のクセ(RX64Mの無名エイリアスunion、RX111固有の周辺モジュール構成差
  等)は都度発見・対応する。既存のCStructBodyParserXxxPatternTestが対応している
  パターンは再利用できるが、新チップで初めて出会うパターンもあり得る。
- 複数チップで同一の周辺モジュール(例: CMT)を扱う場合、レジスタ集合・ビット
  構成が完全に同一なインスタンス配列にのみ`dim`/`derivedFrom`を使う。チップが
  違えば別デバイスなのでSVDのderivedFromはファイル内限定(XSD確認済み)、
  クロスチップの再利用はXML外部実体参照(DTD)で行う(詳細はArcavirt-c0i.6の
  実証結果を参照)。
