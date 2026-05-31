# 入力イベント伝達アーキテクチャ

本書は、OS レベルの入力（マウス・キーボード）を 3D アプリケーション（C# ロジック）へ伝達する
ための基盤設計を定める。描画用の NeoVeldrid と組み合わせて使う。

## 設計の背景と目的

- NeoVeldrid との併用前提。描画は NeoVeldrid（保守フォーク。本家 Veldrid は 2023 年 2 月以降
  凍結のため不採用。[project-foundation.md の技術選定](project-foundation.md#技術選定)）。OS
  入力の取得は NeoVeldrid の `Sdl2Window.PumpEvents()` に委譲する。
- テスト容易性・ヘッドレス対応。グローバル状態（`Input.GetKeyDown()` 的シングルトン）を排除し、
  モック入力を注入可能にする。これは NeoVeldrid 自身が提供しない要件であり、本アーキテクチャの
  存在理由そのものである。
- 状態とイベントの両立。カメラ操作に必要な連続的な状態（level / ポーリング）と、ショートカット
  等に必要な単発の遷移（edge）を、単一スナップショットから矛盾なく導出する。
- ボイラープレートの削減。入力購読（`+=` / `-=`）をゼロにする。単一スレッドの同期ループでは、
  イベント購読よりポーリングの方が記述量・安全性ともに優れるため、通知イベントは設けない。

### 依存方針

NeoVeldrid への依存は層によって意味が異なる。型・名前空間の完全修飾の規約は
[coding-conventions.md の外部型の参照規約](coding-conventions.md#外部型の参照規約) に従う。

| 対象 | 依存可否 | 理由 |
|---|---|---|
| `NeoVeldrid.Key` 等の enum | コアで直接利用可 | 純粋なマネージド型。参照してもウィンドウ生成・ネイティブロードは発生せず、ヘッドレスを妨げない |
| `NeoVeldrid.InputSnapshot`（interface） | 再利用しない | モック可能だが、保持集合・デルタ・`KeyEventType`・契約 A の不変性を欠き品質が劣る |
| `Sdl2Window` / `PumpEvents()` | アダプタ内のみ | 実ウィンドウとネイティブを要求する唯一のヘッドレス境界。コア・公開 API から隔離する |

## コアデータ構造

毎フレームの入力を 1 つの不変構造体にパッキングする。フレーム内のどこから参照しても決定論的な
状態が保証される。

```csharp
/// <summary>
/// 1 フレーム間の入力状態と単発イベントを保持する不変の構造体。
///
/// 【寿命 ＝ 契約 A】本スナップショットおよびその公開コレクション
/// （Keyboard.HeldKeys / KeyEvents）は、次回 IInputPump.PumpEvents() が呼ばれるまでのみ
/// 有効。内部バッファを使い回すため、フレームをまたいで参照・保持してはならない。
/// 過去フレームの状態が必要な場合は、必要な値だけを呼び出し側でコピーすること。
/// （TextInput は不変な string のため保持しても安全だが、規約は一律「保持禁止」とする。）
/// </summary>
public readonly struct InputSnapshot {
    // --- 連続的な状態（level / ポーリング用） ---
    public readonly MouseState    Mouse;
    public readonly KeyboardState Keyboard;

    // --- 単発イベント（edge / 順序保持） ---
    public readonly IReadOnlyList<KeyEvent> KeyEvents;   // キーボードの押下/解放エッジ

    // --- このフレームに入力された文字（IME・レイアウト変換後） ---
    public readonly string TextInput;                    // 連結済み。無入力時は string.Empty
}
```

### マウス

`MouseState` は連続状態（level）に加え、ボタンごとに level と edge を自己記述する。ボタン点数が
少ないため、リストではなくインライン展開する。

```csharp
/// <summary>マウスボタンの level（保持）と edge（このフレームの遷移）。</summary>
public readonly struct MouseButtonState {
    public readonly bool IsDown;    // level : 今押されているか
    public readonly bool Pressed;   // edge  : このフレームに押された
    public readonly bool Released;  // edge  : このフレームに離された
}

public readonly struct MouseState {
    public readonly Vector2 Position;   // 画面座標（level）
    public readonly Vector2 Delta;      // 前フレームからの移動量
    public readonly Vector2 Wheel;      // X=横スクロール / Y=縦スクロール ※後述
    public readonly MouseButtonState Left, Right, Middle;
    // 必要に応じて X1 / X2 を追加
}
```

ホイール X 軸について。NeoVeldrid の公開 `InputSnapshot` は縦の `float WheelDelta` しか提供せず、
横軸を破棄する（`Sdl2Window` が縦のホイールのみを読むため）。よって `Wheel.X` は標準では常に 0
（スタブ）とする。横軸を埋めるには生の `SDL_MOUSEWHEEL` イベント（2.0.18 以降は preciseX /
preciseY を持つ）を拾う配線が別途必要で、これは `Sdl2Window.SdlInstance`（Silk.NET.SDL）経由で
行う。アダプタのホイール代入は 1 箇所に集約し、後日 1 行で切り替え可能にしておく。

### キーボード

level（保持状態）は `KeyboardState`、edge（遷移）は `KeyEvents` リストで表現する。キー点数が
多いため、エッジは順序付きリストが適切。保持状態は「押されているキーの集合」（疎）として持つ。
これは全プラットフォームの入力取得の最小公約数（エッジ配信）に直結し、「離しているキー」を
明示保持する必要がない。

```csharp
/// <summary>キーボードの保持状態（level）。現在スナップショット単体で完結する。</summary>
public readonly struct KeyboardState {
    public bool IsDown(NeoVeldrid.Key key);                 // 内部は使い回しの HashSet
    public IReadOnlySet<NeoVeldrid.Key> HeldKeys { get; }   // 読み取り専用で公開
}

/// <summary>ボタンのエッジ（遷移）種別。遷移であって状態ではない。</summary>
public enum KeyEventType {
    Pressed,   // 押された瞬間（down エッジ）
    Released,  // 離された瞬間（up エッジ）
    Repeat,    // OS オートリピート（コマンド処理は Type == Pressed で絞ること）
}

/// <summary>物理キーの単発イベント。※ NeoVeldrid.KeyEvent とは別物。</summary>
public readonly struct KeyEvent {
    public readonly NeoVeldrid.Key Key;    // 完全修飾で直接利用（エイリアス無し）
    public readonly KeyEventType   Type;
}
```

保持集合の実装は `HashSet<NeoVeldrid.Key>` を使い回し、`IReadOnlySet<NeoVeldrid.Key>` のファサードで
公開する（毎フレームの GC を避ける）。`KeyEvents` も同様に使い回しリストとし、いずれも契約 A に
より「次の PumpEvents() まで有効・保持禁止」とする。

「4 状態」の導出は、押したとき＝`KeyEvents` の `Pressed`、離したとき＝`Released`、押している＝
`Keyboard.IsDown(key)`、離している＝`IsDown(key)` が false（既定・無保持）。専用の 4 値 enum は
設けない（エッジと level の混線を避けるため）。修飾キーも専用アクセサは設けず、
`IsDown(NeoVeldrid.Key.ShiftLeft) || IsDown(NeoVeldrid.Key.ShiftRight)` 等で判定する。

## 責務を分離したインターフェース

「入力を確定させる側（メインループ）」と「入力を利用する側（アプリケーション）」を物理的に
分離する。1 つの具象クラスが両インターフェースを実装し、各協力者には適切なインターフェース
だけを渡す。通知イベントは設けない（ポーリングのみ）。

```csharp
/// <summary>入力を確定させる側。アプリのメインループ先頭で毎フレーム 1 回だけ呼ぶ。</summary>
public interface IInputPump {
    /// <summary>OS から溜まったイベントを刈り取り、内部スナップショットを更新する。</summary>
    void PumpEvents();
}

/// <summary>入力を読む側。読み取り専用・ポーリング。</summary>
public interface IInputProvider {
    /// <summary>現在フレームの確定済み入力状態。いつでもポーリング可能。</summary>
    InputSnapshot CurrentSnapshot { get; }
}
```

## 実装（アダプタとモック）

実機用 `IInputPump` は `Sdl2Window.PumpEvents()` を呼び、NeoVeldrid の `InputSnapshot` を本設計の
`InputSnapshot` へ翻訳する。`NeoVeldrid.MouseButton` 等はこの内側でのみ使用し、Left / Right /
Middle へ振り分ける。NeoVeldrid の型を公開 API に漏らさない。

実機アダプタが負う責務は次のとおり。

- フォーカス喪失時に保持集合をクリアする。押下中に他ウィンドウへ移ると `Released` が来ず、キーが
  「押しっぱなし」で固着するため（全プラットフォーム共通の罠）。
- X11 では検出可能オートリピートを有効化し、リピート由来の偽 release/press を抑止する。これは
  生 SDL（`Sdl2Window.SdlInstance` 経由の Silk.NET.SDL）で行う。NeoVeldrid / SDL 側で処理される
  場合は不要。

カメラのドラッグ操作（オービット・パン）は、現時点では相対マウスモードを使わず、`Mouse.Delta`
（絶対モードのカーソル位置の前フレーム差分）をそのまま用いる。カーソルは常時表示で、ドラッグは
カーソルが画面内で動ける範囲に限られ、端で頭打ちになるが許容する。アンカーへのワープは行わない
（NeoVeldrid では `SetMousePosition` が素の警告ワープのため、ワープして差分を読む方式は Windows で
信頼できない）。経緯として、端で頭打ちにならない連続回転が必要になった場合に備え、相対マウス
モード（カーソルを隠して固定し、`MouseDelta` で無限の移動量を読む）へ切り替える拡張余地を残す。
その場合はカーソルが非表示・固定になることと符号が反転することが代償になる。この方針は
[render-engine.md のカメラの描画計算と操作](render-engine.md#カメラの描画計算と操作) と対応する。

モック用 `IInputPump` は `Sdl2Window` を継承せず、完全に独立した実装として台本化入力を内部
スナップショットに流し込む（base のウィンドウ経路を一切踏まないため）。これにより、カメラ操作
（回転・拡大縮小・移動）のロジックは、実入力・単体テスト・ヘッドレスのいずれでも同一コードで
動作する。

## 設計の判断理由

なぜシングルトン（グローバル参照）を使わないか。テストや Linux ヘッドレスで任意のモック入力を
外部注入するためである。グローバル状態は予測不可能性とテスト困難性を招く。NeoVeldrid 自体は
この切替を提供しないため、本層が担う。

なぜ通知イベント（`OnSnapshotUpdated` 等）を設けないか。単一スレッドの同期ループでは、イベント
ハンドラ内の `foreach(snapshot.KeyEvents)` は、ポーリング消費者が自分の Update で書く `foreach` と
同一コードである。すでにループで回る消費者にとってイベントは購読・解除（解除忘れ＝リーク）を
足すだけで何も節約しない。よってポーリングを唯一の正規経路とし、購読をゼロにすることで
「ボイラープレート削減」をより完全に達成する。なお UI とカメラの入力競合（消費）は、本ライブラリ
では ImGui を使わないため（[project-foundation.md の UI とネイティブ依存の方針](project-foundation.md#ui-とネイティブ依存の方針)）
非該当とし、一般のボタン類による入力の取り合いは生じない。

なぜ level（保持状態）を別に持つか。エッジから再構築すればよいのではないか。ほぼ全プラット
フォームはエッジ（押下/解放）を配信し、保持状態はそれを適用して再構成するものである。各消費者に
フレームをまたぐ簿記を強いると契約 A（単一スナップショットで決定論）が崩れる。よって保持集合を
本層で一元的に維持する。

なぜスナップショットを使い回し、保持を禁止するか（契約 A）。毎フレーム不変オブジェクトを確保
すると恒常的な GC 圧になる。バッファ使い回しで GC を避ける代償として、保持は不可とする。過去
状態が要る稀な消費者は、必要な値のみを自前でコピーする（プール付きコピーは返却管理＝リーク源と
なるため設けない）。

なぜ `TextInput` は `string` で、`KeyEvents` の `Key` と分けるか。`KeyEvents` は物理ボタン操作、
`TextInput` は OS / IME がレイアウト変換した後の最終的な文字で、用途が異なる。`string` は生の
UTF-16 列をそのまま保持するため、サロゲートペア・結合文字・IME 確定済みフレーズを分解せず正しく
扱える。テキスト入力は疎なため、無入力時は `string.Empty`（確保ゼロ）、入力時のみ極小の確保で、
契約 A の GC 方針と矛盾しない。

`NeoVeldrid.Key` は物理キーか仮想キーか。文字は `TextInput` で取得するため `KeyEvents` 側は物理
（スキャンコード）が望ましい。ただし `NeoVeldrid.Key` はレイアウト変換後の仮想キー寄りである
可能性があり、AZERTY / Dvorak で WASD が物理位置に対応しない恐れがある。カメラ操作の物理一貫性が
要件なら、現行の NeoVeldrid ソースで `Key` の意味論を確認すること。
