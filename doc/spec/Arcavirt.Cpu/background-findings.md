# 調査知見（実装時の参照資料）

[README.md](README.md) の設計判断の根拠となった、実装に直結する調査結果をまとめる。
設計意図は README を参照し、ここでは実装者が再調査せずに済むよう一次データを記録する。

## 1. 現行 C# 実装のコンストラクタ引数（チップ固有値 vs 他オブジェクト参照）

JSON の `parameters`（スカラ値、README で削減対象とした種類）と、C# に残す「配線」の区別に使った実測データ。

| クラス | 引数 | 性質 |
| --- | --- | --- |
| `CLOCK(string name)` | なし | 外部依存なし |
| `CMT(uint base_nr, IClock clock, IClockFrequency clkIn)` | `clock`/`clkIn` | 他インスタンス参照。`base_nr` は現状**未使用**（バグ） |
| `ICU(string name, ILeveledISRNotify isrNotify, IReadOnlyList<short> irqToIpr, IReadOnlyList<IRQMD> irqcr, int numOfSWINTs)` | `isrNotify`=構築済み CPU コア、`irqToIpr`/`irqcr`=256要素チップ固有テーブル | `isrNotify` は他インスタンス参照（構築順序: バス→コア→ICU を強制）。テーブル2本は JSON の `VECTOR` に展開する対象 |
| `PORT(string name, IReadOnlyList<string> portNames)` | `portNames` | チップ固有スカラ。JSON の `CHANNEL` 一覧から導出できるため `parameters` からは削除対象 |
| `RSPI(string name, SubClock clkIn, IClock clock)` | `clkIn`/`clock` | 他インスタンス参照 |
| `TPUa(string name, IClock clock, SubClock clkIn, int numOfOutputSignals)` | 同上 | `numOfOutputSignals` は現状**未使用**（バグ） |

## 1.5 ビット単位クラス（`RegisterBit32`/`RegisterBitField32`）— 既存だが未使用

`RegisterValues.cs:93-280` に、ビット単位で read-only/read-write を混在させられる基盤クラスが**既に定義済み**。
しかし現行の RX64M 向けモジュール実装（CLOCK/CMT/PORT/ICU/USB0 等）では、
`new RegisterBit32(...)` や `new RegisterBitField32(...)` の呼び出しは**1件も無い**
（全モジュールが「レジスタ単位クラス + `onWrite` コールバックで手書き」を採用している）。

```csharp
// RegisterValues.cs:251（抜粋）
public RegisterBitField32(IEnumerable<(int bitPos, IRegisterBit32 value)> fields, uint initialValue = 0)
```

**`bitPos` は最初からコンストラクタの外部引数として設計されている**。
README「BitField」節、および「registerOverrides」節の「ビットフィールドの並び替え」の例が示す
「フィールドのビット位置を JSON から供給する」設計は、
この既存インフラをそのまま利用できる（新規に何かを発明する必要はない。
JSON ローダーが `(bitPos, value)` のタプル列を組み立てて渡すだけで済む）。
`onWrite`/`onRead`はビット位置ではなく`fields["MODE"]`のように名前で参照する形に書き直す必要がある。

## 2. onWrite/onRead の特殊挙動（C# に残す責務の具体例）

「JSON で宣言できる範囲」と「C# コードに残すべき範囲」の境界を決めた実例。

### モード依存のクリア意味論（静的マスクで表現不可能な最良の反例）

`ICU.WriteIR`（`ICU.cs:76-87`）: 同じレジスタが `IRQCR` の値により「0を書いたビットがクリア（write-0-to-clear）」にも「何もしない」にも変わる。

### 他レジスタ状態に依存した書き込み拒否

`CLOCK.WriteHOCOCR2`（`CLOCK.cs:341-343`）: `HOCOCR` が発振許可中なら例外を投げる。

### レジスタウィンドウ（セレクタで実体が切り替わる）

USB0 の `PIPESEL`/`CURPIPE` 系（7種）、RSPI の `SPDR`（`SPDCR.SPRDTD` で TX/RX 切替）。
JSON のアドレス→オブジェクト対応では原理的に表現できないカテゴリ。

### FIFO の read-pop / write-push

`USB0.cfifo_read`、`RSPI.WriteSPDR`。

### 派生値の合成読み出し

`CMT_N.ReadCMCNT` は `timer.Count` を返す（`Value` ではない）。
`IoPort.PIDR` は `Update()` がピン状態を直接書き込む。

### 書き込みが他オブジェクトの状態を変える

`CLOCK.WriteSCKCR` → クロック7本の `ClockSelector.ChangeSource`。
`CMT.WriteCMSTR` → `CMTn[0/1].IsRunning`（= `timer.Enable`）。

## 3. 予約ビット文言のタクソノミ（RX64M マニュアル全文、約1,100行を機械抽出）

> 表中の `rw-must-be-zero` 等の分類名は、当時の検討過程での呼称（現在のスキーマには`reservedPolicy` という列挙は存在しない）。
> 現在は README「BitField」の`mapped`節にある分類表のとおり
> `BitField.access`/`resetValue`/`mapped`の組み合わせに対応する（`state`という概念は消滅済み）。

| 件数 | 文言（原文） | R/W | reservedPolicy |
| --- | --- | --- | --- |
| 492 | `These bits are read as 0. The write value should be 0.` | R/W | `rw-must-be-zero` |
| 127 | `This bit is read as 0. The write value should be 0.` | R/W | `rw-must-be-zero` |
| 79 | `The read value is 0. The write value should be 0.` | R/W | `rw-must-be-zero` |
| 36 | `This bit is read as 1. The write value should be 1.` | R/W | `rw-must-be-one` |
| 34 | `These bits are 0 when read and cannot be modified.` | R | `ro-ignored` |
| 11 | `These bits are read as 0. Writing to these bits has no effect.` | R | `ro-ignored` |
| 8 | `The read value is undefined. The write value should be 0.` | R/W | `rw-must-be-zero` + `undefined-read` |

同一レジスタ内で `rw-must-be-zero` と `rw-must-be-one` が混在する実例: `SCKCR2`(0008 0024h)。
同一レジスタ内で `rw-must-be-zero` と `ro-ignored`/`undefined-read` が混在する実例: SCI の `SISR` 系。

### PORT の実装マスクの根拠（マニュアル §22, p.789-791 原文）

> `PORTm.PDR`: However, the bits that correspond to port m on the 176-pin product but do not exist on a product with fewer pins … are reserved. **The B5 bit in PORT3.PDR is reserved, because the P35 pin is input only.**
> `PORTm.PIDR`: … **A reserved bit is read as undefined, and cannot be modified.**

## 4. RX64M パッケージ差の実測値（iodefine.h 機械抽出 + マニュアル Table 22.1 突合せ）

抽出コマンド（端子名から実装マスクを算出）:
```python
pat = re.compile(r'(?<![A-Za-z0-9_])P([0-9A-J])([0-7])(?![0-9A-Za-z_])')
```

| ポート | 176pin | 145/144pin | 100pin | iodefine(和集合) |
| --- | --- | --- | --- | --- |
| PORT0 | 0xAF | 0xAF | 0xA0 | 0xAF |
| PORT5 | 0x0F | 0x7F | 0x3F | 0x7F |
| PORT9 | 0xFF | 0x0F | なし | 0xFF |
| PORTF | 0x3F | 0x20 | なし | 0x3F |
| PORTG | 0xFF | なし | なし | 0xFF |
| PORTJ | 0x28 | 0x28 | 0x08 | 0x28 |

**iodefine.h のマスクは各パッケージの OR（和集合）と一致する**（PORT3 を除く）。
→ README「違いをどう書くか（索引）」の「ビット実装/未実装が違う」（既定値は和集合、
パッケージ別に不要なビットは`registerOverrides`の`mapped: false`にする）の裏付け。

レジスタごとの実装マスクの差（ポート単位ではないことの実例、RX64M）:

| ポート | PDR | PIDR | DSCR | ODR1 |
| --- | --- | --- | --- | --- |
| PORT0 | 0xAF | 0xAF | **0x07** | **0x44** |
| PORT3 | 0xDF | **0xFF** | なし | **0x51** |
| PORTE | 0xFF | 0xFF | 0xFF | **0x5D** |

DSCR を持つポート: **m = 0, 2, 5, 9, A-E, G**（マニュアル §22.3.8 原文で確認）。

### RX64M パッケージ差（ピン数以外）

| 機能 | 176pin | 145/144pin | 100pin |
| --- | --- | --- | --- |
| External bus width | 32 bits | 16 bits | 16 bits |
| SDRAM area controller | Available | Available | 非対応 |
| ETHERC | Ch.0,1 | Ch.0 | Ch.0 |
| USBA | Available | 非対応 | 非対応 |
| S12AD unit1 | 21ch | 21ch | 14ch |

## 5. RX64M vs RX111 の機械比較（iodefine.h 全数比較）

- RX111: 48 周辺定義 / RX64M: 133。共通 44（うちベースアドレス一致 33、不一致 11）
- SYSTEM(CLOCK) レジスタ: (a)完全一致 18 / (b)同オフセットだがビット構成が違う 17 / (c)片方のみ 25
- CMT: 構造は完全同一。差は「ユニット数1→2」「チャネル数2→4」のみ
- PORT: オフセット規則は RX64M と完全同一。**RX111 には DSCR が1つも無い**
- SCI: 先頭14レジスタは完全同一オフセット。RX64M が3本追加、インスタンス数 3→9
- 割込ベクタ: 共通名 28/221。うち番号一致16、**不一致12**（例: `SCI1_RXI1` RX111=219 → RX64M=60）
- RX111 は全固定ベクタ、RX64M はグループ割込 + 選択型割込（`SLIBRn`/`SLIARn`）を追加
  → セクション7「割込み配線・スコープ境界の実装状況」の根拠

### ベースアドレスの「見かけの差」（アーティファクト）に注意

`MPC` は RX111=0x8C11F、RX64M=0x8C100 で一見差があるが、共通レジスタ `PWPR` は両者とも0x8C11F。
RX64M に `PFCSE` 等が前置されているだけ。
→ README「違いをどう書くか（索引）」の「ベースアドレスだけ違う」
（`baseAddress`はアンカーレジスタの絶対アドレスで定義する）の根拠。
`PFCSE`はアンカー`PWPR`より`0x1F`手前に位置するため、`RegisterDefinition.offset`は負値`"-0x1F"`になる
（README「RegisterDefinition」節の負オフセット例の根拠）。

### レジスタ幅（sizeBits）自体がデバイス間で異なる実例

RX64M/RX66N 比較 APN（R01AN4820JJ0100 表2.34, p.47）より:

| レジスタ | RX64M(GPTA) | RX66N(GPTW) |
| --- | --- | --- |
| `GTIOR` | 16 ビット | **32 ビット** |
| `GTCR` | 16 ビット | **32 ビット** |
| `GTPR` | 16 ビット | **32 ビット** |

→ `implementedMask`（ビット単位マスク）では表現できない差分（マスクの対象自体が変わるため）。
`registerOverrides` を `RegisterDefinition` の全フィールド（`sizeBits` を含む）に対象を
広げる根拠（README「`registerOverrides`」節の対象パスごとの有効プロパティ表を参照）。

## 6. 実機のバスエラー機構の詳細（README「`registerLog`」の設計判断の根拠一次資料）

一次資料: RX64M ユーザーズマニュアル R01UH0377EJ0120/JJ0120 Rev.1.20、RXv2 命令セットアーキテクチャ編 R01US0071JJ0100。

- アクセス例外の発生源は **MPU のみ**（RXv2 ISA §5.1.3）。MPU はユーザモード限定かつ `MPEN=1` 限定（RX64M HW §17.1.1）
- バスエラーは例外ではなく**マスカブル割込み BUSERR（ベクタ16）**（§16.7.2）
- `BEREN`（不正アドレス検出許可）はリセット値 `0`（既定無効）（§16.3.20）
- 主要 SFR が住む「内部周辺バス1」（0008 0000h-0008 7FFFh）は**表16.21 でバスエラー対象外**
- BERSR1/2 は**一度記録したらクリアされるまで次を記録しない**（§16.7.3）。
  README の`registerLog`は既定で何も出力せず、有効にした場合はフィールド単位で
  違反の都度記録する設計であり、実機のこのラッチ機構（1回記録したら次を待つ）とは
  動作原理が異なる点に注意。

## 7. 割込み配線・スコープ境界の実装状況（RX適用における未了点）

README「責務とスコープ境界」の「担わないこと（別issue）」参照。

### 割込みモデルの違い（グループ割込み・選択型割込）

RX111 は全固定ベクタ、RX64M はグループ割込み（`SLIBRn`）+ 選択型割込み（`SLIARn`）を追加（共通ベクタ名 28/221 中、番号一致16・不一致12。
詳細はセクション5参照）。
配線自体は`pinConnections`で表現できるが、グループ内の要因判定ロジックは現行`ICU.cs`に未実装。

### 未完成のレジスタオブジェクト

USB0/TPUa/RSPI は、対応するレジスタオブジェクトを1つも生成していない（コンストラクタ引数の性質はセクション1の表を参照）。
JSONマッピング適用はC#側の実装完成が前提。

### バスエラー機構（BSC）の実装状況

`BEREN`/`BERSR1`/`BERSR2`/`BERCLR`によるBUSERR割込み（ベクタ16、詳細はセクション6）は、
正規の周辺モジュール実装として別途必要（`rx64m-bsc-pilot.svd`に定義済みだが未接続）。

### 重複登録バグの実測

| 事象 | 箇所 |
| --- | --- |
| PORT: 153 レジスタオブジェクト → 実アドレス 9 個（19 ポートが同一アドレスに衝突） | `PORT.cs:280-293` |
| CMT: 7 レジスタオブジェクト → 実アドレス 3 個（同種の衝突バグ） | `CMT.cs:103` |
| `BusManager.AddMapping` は重複登録を検出せず後勝ちで黙って上書きする | `RegisterValues.cs:335-337` |

→ README「PeripheralDefinition」の`baseAddress`節（「同一デバイス内で複数エントリの
`baseAddress`が一致することはロード時エラーとする」）の根拠。

### 割込み検出方式（edge/level）の固定化

`ICU.WriteIR`・内部割込み判定ロジック（`ICU.cs`）を調査した結果、外部端子割込み（IRQ/NMI）は`IRQCRn.IRQMD`レジスタで実行時に検出方式を切り替え可能だが、
内部割込み（`CMI0`等）は常にエッジ型に固定されており、外部端子用と内部割込み用でコード上の分岐は存在しない。
→ README「`pinConnections`」のJSONスキーマに検出方式（エッジ/レベル）のフィールドを一切持たせていない判断の根拠。
内部割込みの検出方式はコントローラー実装のC#クラスに固定で書く。
外部端子の検出方式は検出モード用`BitField`の`resetValue`で表現できる。
