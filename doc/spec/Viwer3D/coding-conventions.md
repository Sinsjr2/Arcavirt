# コーディング規約

本書は、本ライブラリのコードベースに適用する規約をまとめる。各分冊のコード例もすべて本書に
従う。

## 命名規則

クラスのフィールド変数に `_`（アンダースコア）の接頭辞は使用しない。通常の変数と同様の命名
（例: `graphicsDevice`、`commandList`）で統一する。

## 標準型の利用

座標・回転・行列は `System.Numerics` の標準型（`Vector3`、`Quaternion`、`Matrix4x4`、色は
`Vector4`）を使う。独自の数学型や、イージング等のアニメーション用ヘルパーは作らない。時間に
よる変化（回転速度など）の計算はアプリケーション側で `System.Math` / `System.Numerics` を
そのまま用いて組み立てる。

トランスフォームの合成は、ファクトリメソッドと `*` 演算子で書く。`System.Numerics` は行
ベクトル規格で、左から右に書いた順に計算が適用されるため、合成順がコード上に明示される。
合成用の独自拡張メソッドは作らない。

```csharp
// 「回転させたあと、指定方向に移動する」が左から右に読める
Matrix4x4 m = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(0, 0, 1.0f);
```

## 外部型の参照規約

NeoVeldrid 由来の型（`NeoVeldrid.Key`、`NeoVeldrid.MouseButton`、`NeoVeldrid.InputSnapshot`
など）は完全修飾で直接利用し、エイリアスを作らない。公開型を定義するファイルでは
`using NeoVeldrid;` を行わない。

これは、`NeoVeldrid` 名前空間に本設計の同名型（`KeyEvent` / `MouseButton` / `InputSnapshot`）と
衝突する型が存在するためである。完全修飾を徹底することで、どちらの型を指すかが常に一意に
なり、`using` による暗黙の取り違えを防ぐ。

## 波括弧と制御構文

開き波括弧 `{` の前で改行しない。メソッド・オブジェクト初期化子・制御構文のいずれでも、
`{` は直前のコードと同じ行に置く。`else` は `} else {` の形にする。

制御構文（`if` / `for` / `while` / `foreach` など）では波括弧 `{}` を省略しない。単文であっても
必ず波括弧で囲む。

```csharp
public static CameraState Zoom(CameraState c, float wheelDelta) {
    if (c.Projection == ProjectionMode.Perspective) {
        c.Distance = Math.Clamp(c.Distance * (1 - wheelDelta * ZoomSensitivity), MinDistance, MaxDistance);
    } else {
        c.OrthographicSize = Math.Clamp(c.OrthographicSize * (1 - wheelDelta * ZoomSensitivity), MinSize, MaxSize);
    }
    return c;
}
```
