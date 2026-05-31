# シーン記述用ノード

本書は、3D シーンを「あるべき状態」として宣言的に記述する仕組みと、その記述に使う各ノードの
仕様、そしてカメラをシーンへ組み込む方法を定める。状態は 100% アプリケーション側が保持し、
ライブラリは渡された宣言をその通り描く（単方向データフロー）。

宣言ツリーを実際の描画命令へ橋渡しする部分は性質が異なるため、
[render-engine.md の宣言ツリーと描画](render-engine.md#宣言ツリーと描画) で扱う。

## 宣言的シーン記述の仕組み

### 背景と他の宣言的 3D 系の調査

NeoVeldrid 単体で 3D を表示するには、ウィンドウ・スワップチェイン・パイプライン・バッファ・
シェーダ・描画ループを手作業で組む必要があり、シーンの構造を表現したり融通を利かせたりする
負担が大きい。シーングラフの生成・更新・破棄を手で管理すると、追加・削除のたびの取り回しや
GPU 資源の解放漏れ（メモリリーク）を招きやすい。

そこで、Web の React のように「シーンはこうあるべき」と書けば生成・更新・解放をライブラリが
引き受ける、宣言的なシーン記述層を設ける。本来は既存ライブラリを使いたいが、C# にこの用途の
ものが見当たらないため自作する。設計にあたり代表的な宣言的 3D 系を調査した。

- react-three-fiber（JS / three.js）: 状態変化時にツリーを前回と差分比較し、既存オブジェクトを
  書き換える「保持＋差分」型。同一性は React の key で管理する。
- USD / OpenUSD（言語非依存の仕様）: 合成アークで部品を組み上げ、非破壊のスパースな上書きで
  個性を保ち、ネイティブ・インスタンシングで同一資産の大量コピーを共有する。ただしインスタンス
  化した対象は配下の個別上書きが無視される制約を持つ。
- Qt Quick 3D（QML / C++）: 宣言言語でシーンを書き、記述（フロントエンド）と描画状態
  （バックエンド）を分離し、RHI が各 OS のネイティブ API へ翻訳する。
- elm-3d-scene（Elm）: 純粋関数・不変。エンティティとカメラ等を渡して毎回シーンを値として
  組み立てる「即時再構築」型。

本ライブラリは、即時再構築型（elm-3d-scene に最も近い）を採用する。

### 記述スタイル

C# 標準のオブジェクト初期化子・コレクション初期化子のネストを用いる。コードのインデントが
そのままシーンの階層を表す。新しい独自言語やマークアップは導入しない。アプリはループを所有し、
毎フレーム自分の状態から新しいレンダーツリーを組み立て、`Render` へ渡す。

```csharp
// アプリのメインループ（毎フレーム）
input.PumpEvents();
UpdateState(state, time);   // 状態更新は 100% アプリ側

var tree = new RenderTree(state.Camera) {
    new ModelNode("machine.gltf") {
        ID = "Machine_Left",
        Transform = Matrix4x4.CreateTranslation(-0.5f, 0, 0),
        NodeOverrides = new NodeOverrideCollection {
            new NodeOverrideGroup("Chassis/Rollers") {
                { "Roller_A", Matrix4x4.CreateRotationY(state.AngleA) }
            }
        }
    },
    new ModelNode("machine.gltf") {          // 同じファイル・別 ID = 別配置
        ID = "Machine_Right",
        Transform = Matrix4x4.CreateTranslation(0.5f, 0, 0)
    },
    new GridNode { Spacing = 1.0f },           // 床グリッド（任意の補助）
    new NavigationGizmoNode {                  // 向き表示（表示専用）
        HorizontalAlignment = Align.Right,
        VerticalAlignment = Align.Top
    }
};

renderEngine.Render(tree);   // 背景クリア色・時間などの毎フレーム入力も合わせて渡す
```

毎フレームの明示的な提出（レンダーツリーを組み立てて `Render` へ渡す）を採る。アプリが自分の
状態からシーン値を明示的に組み立てて提出するだけで、登録コールバックの間接や可変コンテキストは
介在しない。これは設定の毎フレーム入力（[render-engine.md の描画パラメータの設定ライフサイクル](render-engine.md#描画パラメータの設定ライフサイクル)）と、
アプリがメインループを所有する前提（[input-architecture.md](input-architecture.md)）に整合する。

経緯として、ビルダーを一度登録して可変コンテキストへノードを追加する形（`SetContent` に
`ctx.Add` していく形）も検討したが、ループ所有と透明性を優先して毎フレームの明示的提出に
一本化した。

### 評価と更新のタイミング

毎フレーム、無条件にレンダーツリーを再構築する（即時再構築）。差分比較（reconciliation /
diffing）は行わない。シミュレータでは状態（ローラーの角度等）が毎フレーム変わるため、毎
フレーム作り直すのが素直で、状態の不整合が起きない。差分をしない理由の詳細は
[render-engine.md の宣言ツリーと描画](render-engine.md#宣言ツリーと描画) を参照。

毎フレームの作り直しを安価に保つために、ノードは軽量な記述子に徹し（重い処理・確保を
持たない）、重い資産はレジストリでキャッシュして毎フレーム読み直さず複数配置で共有する。
時間による変化（一定速度の回転など）の計算はアプリ側で行い、確定値を毎フレーム流し込む。
ライブラリ側は「現在の角度」などの状態を持たず、送られてきた数値を実直に描画する。

```csharp
// アプリ側の更新ループ（ITimeSource を利用）。状態は全てアプリの state に集約される
state.RollerAngle += 60.0f * time.DeltaTime;   // 毎秒 60 度

// 宣言ツリー側は計算済みの確定値をただ流すだけ
{ "Roller_A", Matrix4x4.CreateRotationY(state.RollerAngle) }
```

これにより全パーツの状態が C# の `state` に集約され、3D 画面を起動しなくても「時間が進んだ
ときに正しく動くか」を通常の単体テストで検証できる。ライブラリ側には特別なアニメーション機能
（イージング関数など）は持たせない（[coding-conventions.md の標準型の利用](coding-conventions.md#標準型の利用)）。

### ID

ID は配置（ModelNode 等）の同一性を表す任意のラベルである。差分比較をしないため、ツリーに
2 つの ModelNode を書けばそれがそのまま 2 つの別配置になり、同じファイルでも別ノードなら別
配置として描かれる。ID は複数配置の前提条件ではない。

ID が推奨されるのは、同一ファイルの複数配置のうち特定の 1 つを C# 側のロジックから名指ししたい
とき、またはエラーメッセージやデバッグ出力を分かりやすくしたいときである。規則は次のとおり。

- ID を付ける場合はツリー内で一意とする。重複は即座に明確な例外とする（あいまいさを黙って
  通さない）。
- ID を省略した場合は、ライブラリが構造上の位置から内部識別子を自動採番する（診断用）。

なお他ライブラリでは同種の概念を key と呼ぶが、本ライブラリの用語は ID で統一する。

### 中核ノードの概観

中核ノードは次の 4 種に絞る。各詳細は後述する。

| ノード | 役割 |
|---|---|
| [GroupNode](#groupnode) | 座標変換つきの入れ物。子ノードをまとめ、配置・繰り返しの単位になる |
| [ModelNode](#modelnode) | glTF モデルの 1 配置。配置トランスフォーム・パーツ上書き・色上書き・ID を持つ |
| [GridNode](#gridnode) | 床面の基準グリッド（補助表示）。3D 世界に置かれ深度判定を受ける |
| [NavigationGizmoNode](#navigationgizmonode) | 画面隅に重ねる向き表示（表示専用・クリック判定なし） |

### 拡張の方針

新種ノード（後述のインスタンス化ノード、ライン、ラベル等）は、形式に依存しない契約
`IDrawableSource`（[render-engine.md の宣言ツリーと描画](render-engine.md#宣言ツリーと描画)）を
実装する形で追加でき、描画エンジン本体・中核ノードを変更せずに拡張できる。大きな固定カタログは
作らない（最小・拡張可能）。

同一物の大量配置（1000 個以上）は、1000 個の別ノードとして書くと毎フレームのノード確保（GC 圧）と
個別描画命令の本数が CPU の頭打ちを招く。これは専用の「インスタンス化ノード」を 1 つ置き、配置
ごとの変形・色の配列を持たせ、GPU インスタンシングで 1〜数回の描画命令にまとめる。宣言ツリー上は
1 ノードしか増えず宣言性は保たれる。配列は呼び出し側が保持・使い回すものを参照渡しし、毎
フレームの新規確保を避ける。このインスタンス化ノードは今は実装せず、枠だけ予約する。

将来「インスタンスごとに持続する状態」（配置ごとの GPU 資源をフレームをまたいで再利用する
最適化や、差分・補間の導入）を入れる場合に限り、ID を「必須・一意」へ格上げできる余地を残す
（それまでは任意・自動採番）。

## 各ノードの仕様

### GroupNode

座標変換つきの入れ物。`Transform` を持ち、`Children` に子ノードをまとめる。配置・繰り返しの
単位になる。

```csharp
new GroupNode {
    Transform = Matrix4x4.CreateTranslation(0, 0, 0),
    Children = {
        new ModelNode("machine.gltf") { ID = "Machine_Left" }
    }
}
```

### ModelNode

glTF モデルの 1 配置（インスタンス）。配置トランスフォーム（`Transform`）、パーツ上書き
（`NodeOverrides`）、色上書き（`ColorOverrides`）、`ID` を持つ。各パーツの最終位置は「配置 ×
親 × 子 × 上書き」の積み上げで決まる。

### アセットとインスタンス

アセット（形・階層・材質、GPU の頂点データ）はファイルパスをキーにキャッシュされ、同じ
ファイルを N 回置いても読み込み・GPU 転送は 1 回、N 配置で GPU バッファを共有する（2 台
置いても VRAM は 1 台分）。キャッシュの寿命はパス単位で、そのパスを使う配置が 1 つでもある
限り生存する（キャッシュの自動解放は
[render-engine.md のアセットのキャッシュとメモリ管理](render-engine.md#アセットのキャッシュとメモリ管理)）。

インスタンス（ツリーに置いた各 ModelNode）は、自分の配置トランスフォーム・NodeOverrides・
ColorOverrides・任意の ID を個別に持つ。各配置の変形は毎フレーム個別に計算するため、重い
データの共有と配置ごとの無制限な上書きを両立する。USD のネイティブ・インスタンシングが共有の
ために配置ごとの個別上書きを禁じるのに対し、本ライブラリは即時再構築モデルゆえに両立できる。
これは即時再構築モデルの明確な利点である。

### NodeOverride

glTF 内の特定パーツのローカル変形を C# から上書きする指定。階層グループ記法で書き、共通の親
パスをグループでくくり、その中に対象パーツとトランスフォームを書く。値は `System.Numerics` の
`Matrix4x4`。

```csharp
NodeOverrides = new NodeOverrideCollection {
    new NodeOverrideGroup("Chassis/Rollers") {
        { "Roller_A", Matrix4x4.CreateRotationY(state.AngleA) },
        { "Roller_B", Matrix4x4.CreateRotationY(state.AngleB) }
    }
}
```

記法と意味論は次のとおり。

- グループは任意の深さでネストできる（グループの中にさらにグループを書ける）。コードの
  インデントがそのまま 3D モデルの階層構造を表す。多段パスを 1 グループにまとめる書き方
  （`"Chassis/Rollers"`）も併用できる。
- 各エントリの実効フルパスは、外側のグループの接頭辞をすべて連結し、最後にそのエントリ自身の
  パスを連ねたものとする。直接のフルパスエントリとグループは同じコレクション内に混在してよい。
- 後述の存在チェック・後勝ちは、この連結後の実効フルパスに対して適用する。同一の実効フルパスに
  複数回の指定があれば、後に書かれたものが有効（後勝ち）となる。

上書きは次の規則で適用する。

- 基準はパーツ自身のローカル座標系（glTF 内で定義されたピボット点）とする。機械のパーツは
  傾いた親に取り付けられていることが多く、ワールド座標基準にすると C# 側の回転計算が極めて
  複雑になる。ローカル基準なら、親がどう傾いていても「そのパーツ自身の中心軸でまっすぐ回す」
  という直感的な制御ができる。
- 3D モデルの初期状態（デフォルトのポーズ）をベースとし、そこからの相対的な変化量（オフセット）
  として適用する。初期状態を「上書き」する仕様にすると、C# 側で Blender 上の初期配置座標を
  ハードコードして管理せねばならず、デザイン変更に弱い。相対変化量とすることで、Blender 上で
  初期位置を微調整しても C# 側のロジックを修正せずに正しく動作する。
- 親子関係の連動を原則とする。親ノードを移動させれば、その配下の子ノードも 3D 空間上を連動して
  移動する（現実の機械と同じ挙動）。
- 上書きは内部処理でワールド行列を評価する「前」に、対象ノードのローカル変形として差し込む。
  glTF の階層をたどってワールド行列を求める処理は SharpGLTF.Runtime に任せ、その評価の前段で
  対象ノードのローカル変形を上書きする。これにより上記のローカル基準・相対・連動と素直に
  整合する。なお実装着手前に、SharpGLTF.Runtime の SceneInstance に対し、glTF 内のアニメーション
  とは無関係に任意ノードのローカル変形を毎フレーム与えられるかを実コードで確認する。できない
  場合は、一段低いレベルで自前に行列を積む案へ切り替える。
- トランスフォームの合成は、ファクトリメソッドと `*` 演算子で書く（合成順をコード上に明示する。
  [coding-conventions.md の標準型の利用](coding-conventions.md#標準型の利用)）。

存在チェック（タイポやモデル構造変更で「静かに動かなくなる」のを防ぐ仕組み）は次のとおり。

- アセットの最初のロード・キャッシュ時に、`NodeOverrides`（および後述の `ColorOverrides`）に
  指定された全エントリの実効フルパスが glTF モデル内に実在するかを全走査して検証する。
- 存在しないパスがあれば、即座に `ArgumentException` をスローしてアプリケーションを止める。
  メッセージには、そのモデルに含まれる有効なノード（パーツ）名の一覧を含め、開発者が 1 秒で
  タイポやモデル不整合に気づけるようにする。

経緯として、初期には親パスをグループでくくる本方式に加え、独自型 `NodeTransform` を値とする案や、
すべてをフラットなフルパスで書く案も検討した。`NodeTransform` は `System.Numerics` 標準型に
揃える方針から採らず、フラット方式は単純だが、可読性と親パスの繰り返し削減、および存在チェック
による安全性の担保を理由に、階層グループ方式を採った。

C# 側で可動範囲のバリデーション（例: 初期位置から ±10 mm の間だけで動かす）を組めるよう、
モデルの初期トランスフォームを参照する読み取り専用 API を提供する。これにより初期座標を二重
管理（ハードコード）せずに済む。

```csharp
ModelTransformInfo initialInfo = renderEngine.Assets.GetInitialTransform("machine.gltf", "Chassis/Rollers/Roller_A");
Vector3 initialPosition = initialInfo.Translation;   // Blender 上の初期位置
```

モデル内の有効なパス一覧を取得するデバッグ API も、エンジンのインスタンス経由で提供する
（シングルトンは使わない。[project-foundation.md のシングルトン不使用](project-foundation.md#シングルトン不使用)）。

```csharp
string[] validPaths = renderEngine.Assets.GetAvailableNodes("machine.gltf");
```

### ColorOverride

特定パーツの色を上書きする指定。NodeOverrides と並列の `ColorOverrides` コレクションに書き、
NodeOverride と同じ階層グループ記法を使う。各エントリはパーツのパスを、色（`Vector4` の RGBA）と
ブレンドモード（`ColorBlend { Replace, Multiply }`）の組へ対応させる。

```csharp
new ModelNode("machine.gltf") {
    NodeOverrides = new NodeOverrideCollection {
        new NodeOverrideGroup("Chassis/Rollers") {
            { "Roller_A", Matrix4x4.CreateRotationY(state.AngleA) }
        }
    },
    ColorOverrides = new ColorOverrideCollection {
        new ColorOverrideGroup("Chassis/Rollers") {
            { "Roller_A", new ColorOverride(new Vector4(1, 0, 0, 1), ColorBlend.Multiply) }   // 異常パーツを赤に
        }
    }
}
```

存在チェック・後勝ち・実効フルパスの意味論は、NodeOverride とまったく同じものを適用する。
色とトランスフォームは独立に指定できる（片方だけ上書きしてよい）。ブレンドの意味と GPU での
扱い（Replace / Multiply の動的分岐、定数バッファでの送信）は
[render-engine.md のマテリアルと色](render-engine.md#マテリアルと色) に定める。

### GridNode

床面の基準グリッド。機械の大きさ・位置・向きの感覚をつかむための補助表示で、ツリーに置いた
ときだけ出る（既定では出さない＝何も足さなければ床なし）。グリッドは 3D の世界（床面 Y=0 の
XZ 平面）に置かれ、深度判定を受ける（手前のパーツがグリッドを正しく隠す）。既定は 1 マス =
1 メートル、無彩色で、必要に応じて間隔（`Spacing`）と広さを指定できる。線のギザギザは MSAA で
滑らかになる（[render-engine.md の描画の見え方](render-engine.md#描画の見え方)）。

### NavigationGizmoNode

画面隅に重ねる向き表示（XYZ の方向を示す小さな軸または立方体）。表示専用でクリック判定を
持たない（ヒットテスト不要）。配置（右下・左上など）は `HorizontalAlignment` /
`VerticalAlignment` で指定する。ツリーに置けば出て、外せば消え、ヘッドレスの PNG にも置けば
写る。モデルを描画するノードとカメラ操作のための表示を独立した並列ノードとして分離することで、
3D モデルデータと視点表示が独立し、配置や表示・非表示をツリーの記述だけで柔軟に切り替えられる。

## カメラのシーンへの組み込み

カメラはシーンの一部として、レンダーツリーに CameraState を渡す形で組み込む。CameraState の型
（オービット・パラメータ表現）の正典はここに置く。

```csharp
var tree = new RenderTree(state.Camera) {   // state.Camera は CameraState
    new ModelNode("machine.gltf") { /* ... */ }
};
```

```csharp
public struct CameraState {
    public Vector3 Target;             // 注視点
    public float Distance;             // 注視点からの距離
    public float Azimuth;              // 方位角（水平回り）
    public float Elevation;            // 仰角（上下。±一定角でクランプ）
    public ProjectionMode Projection;  // Perspective / Orthographic
    public float OrthographicSize;     // 平行投影時の視野幅（透視時は未使用）
}
```

上方向は座標規約の世界 Y 軸で固定し、状態には持たない。視野角は透視投影で 45 度固定、ニア／
ファーは自動のため、いずれも状態に持たない（[index.md の座標系と単位](index.md#座標系と単位)）。

ModelNode はカメラ操作の状態とは完全に切り離され、C# からの命令どおりにパーツを描く表示器に
徹する。カメラを実際にビュー行列へ落とす描画計算と、マウス操作から CameraState を算出する
操作系（CameraInteraction）は、計算が描画と一体のため
[render-engine.md のカメラの描画計算と操作](render-engine.md#カメラの描画計算と操作) に置く。
本書ではカメラを宣言的に渡すことだけを定める。
