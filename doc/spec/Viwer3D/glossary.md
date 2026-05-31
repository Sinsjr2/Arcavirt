# 用語集

本書は、分冊をまたいで使う用語の意味を 1 か所に固定し、表現のぶれを防ぐためのものである。
各用語には簡潔な定義を付し、その用語の正典（詳細を定める分冊）へリンクする。

## シーン記述系

- ノード … 宣言ツリーを構成する 1 要素。軽量な記述子に徹し、重い処理や確保を持たない。
  詳細は [scene-nodes.md](scene-nodes.md#宣言的シーン記述の仕組み)。
- レンダーツリー（宣言ツリー） … 1 フレームの「画面がどうあるべきか」を表す使い捨ての木。
  毎フレーム生成して描画し、差分比較は行わない。詳細は
  [scene-nodes.md](scene-nodes.md#評価と更新のタイミング)。
- ModelNode … glTF モデルの 1 配置（インスタンス）。配置トランスフォーム・パーツ上書き
  （NodeOverride）・色上書き（ColorOverride）・ID を持つ。詳細は
  [scene-nodes.md](scene-nodes.md#modelnode)。
- GroupNode … 座標変換つきの入れ物。子ノードをまとめ、配置・繰り返しの単位になる。詳細は
  [scene-nodes.md](scene-nodes.md#groupnode)。
- GridNode … 床面の基準グリッド（補助表示）。3D 世界に置かれ深度判定を受ける。詳細は
  [scene-nodes.md](scene-nodes.md#gridnode)。
- NavigationGizmoNode … 画面隅に重ねる向き表示（表示専用・クリック判定なし）。詳細は
  [scene-nodes.md](scene-nodes.md#navigationgizmonode)。
- NodeOverride … glTF 内の特定パーツのローカル変形を C# から上書きする指定。階層グループ
  記法で書き、値は `Matrix4x4`。詳細は [scene-nodes.md](scene-nodes.md#nodeoverride)。
- ColorOverride … 特定パーツの色を上書きする指定。色（`Vector4`）とブレンド（Replace /
  Multiply）の組。詳細は [scene-nodes.md](scene-nodes.md#coloroverride)。
- ID … 配置（ModelNode 等）の同一性を表す任意のラベル。他ライブラリの key に相当するが、
  本ライブラリの用語は ID で統一する。任意・一意・省略時は自動採番。詳細は
  [scene-nodes.md](scene-nodes.md#id)。
- アセットとインスタンス … アセットは読み込んだモデルの形・階層・材質と GPU データで、
  ファイルパスをキーに共有される。インスタンスはツリーに置いた各 ModelNode で、配置ごとの
  上書きを個別に持つ。詳細は [scene-nodes.md](scene-nodes.md#アセットとインスタンス)。

## 描画系

- IDrawableSource … 形式に依存しない境界インターフェース。名前つきノードの階層・メッシュ・
  材質を、上書きを受けて最終的な描画物として返す契約。描画エンジンはこれだけに依存する。
  正典は [render-engine.md](render-engine.md#宣言ツリーと描画)。
- DrawCall … GPU への 1 回の描画命令。本ライブラリはパーツ（ノード／メッシュプリミティブ）
  ごとに個別発行する。詳細は [render-engine.md](render-engine.md#内部描画と実行環境)。
- MSAA … ポリゴンの輪郭を滑らかにするマルチサンプル。既定 4x。詳細は
  [render-engine.md](render-engine.md#描画の見え方)。
- 解像（resolve） … MSAA の絵を 1 枚の通常画像へまとめる処理。画面表示にもヘッドレスの
  PNG にも回す前段。詳細は [render-engine.md](render-engine.md#画像キャプチャ機構)。
- Replace / Multiply … 色上書きのブレンドモード。Replace は単色で塗りつぶし、Multiply は
  元の質感に色を乗算する。詳細は [render-engine.md](render-engine.md#マテリアルと色)。

## カメラ系

- オービットカメラ … 注視点を中心に回るカメラ。ドラッグで視点角度、ホイールで距離、
  Shift＋ドラッグで注視点を平行移動する。詳細は
  [render-engine.md](render-engine.md#カメラの描画計算と操作)。
- CameraState … カメラの状態。注視点・距離・方位角・仰角・投影モード・平行投影時の視野幅を
  持つオービット・パラメータ表現。詳細は
  [render-engine.md](render-engine.md#カメラの描画計算と操作)。
- CameraInteraction … マウス操作から新しい CameraState を算出する純粋関数群（Orbit / Pan /
  Zoom）。詳細は [render-engine.md](render-engine.md#カメラの描画計算と操作)。
- ProjectionMode … 透視投影（Perspective）と平行投影（Orthographic）の別。詳細は
  [render-engine.md](render-engine.md#カメラの描画計算と操作)。

## 入力系

- level / edge … level は「今押されているか」という連続状態、edge は「このフレームに押された／
  離された」という単発の遷移。詳細は [input-architecture.md](input-architecture.md#コアデータ構造)。
- 契約 A … スナップショットおよびその公開コレクションは次回 PumpEvents まで有効で、フレームを
  またいで保持してはならないという規約。詳細は
  [input-architecture.md](input-architecture.md#コアデータ構造)。
- InputSnapshot … 1 フレーム間の入力状態と単発イベントを保持する不変構造体。詳細は
  [input-architecture.md](input-architecture.md#コアデータ構造)。

## 技術基盤系

- NeoVeldrid … Veldrid の保守フォーク。ネイティブバインディングを Silk.NET に置き換え、
  公開 API を維持する。MIT、net10.0。詳細は
  [project-foundation.md](project-foundation.md#技術選定)。
- Vulkan … 主軸とするグラフィックス API。macOS では MoltenVK（同梱）経由で動く。詳細は
  [project-foundation.md](project-foundation.md#技術選定)。
- Silk.NET.SDL … ウィンドウと OS 入力の取得に使う SDL バインディング。詳細は
  [project-foundation.md](project-foundation.md#技術選定) および
  [input-architecture.md](input-architecture.md)。
- SharpGLTF … glTF 読み込みに使うライブラリ。詳細は
  [project-foundation.md](project-foundation.md#技術選定)。
- SPIRV（NeoVeldrid.SPIRV） … GLSL を 1 種類だけ書き、実行時に各バックエンドへ
  クロスコンパイルする仕組み。純 C# 実装。詳細は
  [project-foundation.md](project-foundation.md#技術選定)。
- ITimeSource … 時間の進め方を抽象化したインターフェース。実時間用とステップ用の 2 実装を
  切り替える。詳細は [project-foundation.md](project-foundation.md#技術選定)。
- ヘッドレス／オフスクリーン … 画面なしでメモリ上へ描画する動作・対象。詳細は
  [project-foundation.md](project-foundation.md#アーキテクチャと依存)。
- Tier1 / Tier2 … 設定の階層。Tier1 は作成時に焼き付くもの、Tier2 は毎フレーム自由に渡すもの。
  詳細は [render-engine.md](render-engine.md#描画パラメータの設定ライフサイクル)。
