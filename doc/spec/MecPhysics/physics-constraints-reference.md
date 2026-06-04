# 拘束(constraints)JSON 参照表 — 全 30 種

bepuphysics2 の `BepuPhysics/Constraints` にある拘束記述(description)を全数、本プロジェクトの JSON 規約に写したもの。
各 `type` は対応する bepu 拘束の公開フィールドを写し、軸・アンカー・単位の規約を被せている。フィールド名・型は bepu ソースから確認済み。

---

## 共通規約

- **判別**：`constraints` は `type` で判別する配列。
- **ボディ参照**：2 体拘束は `bodyA` / `bodyB`、単体拘束は `body`、面積は `bodies:[a,b,c]`、体積は `bodies:[a,b,c,d]`。値は `bodies` テーブルの **id**。
- **アンカー(`localOffset*Mm`)**：ボディ中心(質量中心)から取付点へのローカル オフセット [x,y,z](mm)。**既定は節点原点**(ライブラリが COM からのオフセットを算出)。
- **軸・方向・法線(`localAxis*` 等)**：単位ベクトル [x,y,z]。**既定は節点の局所 +Z**。複合ボディは先頭メンバーのノード枠。
- **基底(`localBasis*`)**：完全な姿勢枠。`eulerDeg:[x,y,z]` で与える。既定は節点の局所枠。
- **角度** = `*Deg`(度)、**距離・変位** = `*Mm`、**力** = `*N`、**線速度** = `*Mps`、**角速度** = `*DegPerSec`。ライブラリが内部 SI(m・rad・kg・s)へ変換。

### 共有サブブロック

```jsonc
"spring": { "frequencyHz": 60, "dampingRatio": 1.0 }
// frequencyHz: 単位時間あたりの無減衰振動回数。dampingRatio: 0=無減衰, 1=臨界, >1=過減衰。

"servo":  { "maximumForceN": 1000, "maximumSpeed": 1e9, "baseSpeed": 0 }
// 目標位置/姿勢/距離へ向かう。force ≤ maximumForceN、速度は spring 由来を maximumSpeed で頭打ち。

"motor":  { "maximumForceN": 1000, "softness": 0 }
// 目標速度へ向かう。softness = 1/damping(0=完全剛、大=軟)。
```

`servo` 系拘束は `spring` と `servo` の両方、`motor` 系は `motor` のみ、`limit`/固定系は `spring` のみを持つ(下表の各型に従う)。

---

## A. 完全・点結合

### `weld` — 6 自由度を完全固定(剛接合)
2 体。`bodyB` を `bodyA` に相対姿勢固定。
```jsonc
{ "type": "weld", "bodyA": "...", "bodyB": "...",
  "localOffsetMm": [0,0,0],          // A から見た B の相対位置(既定:読込時の相対位置)
  "localOrientationEulerDeg": [0,0,0],// A から見た B の相対姿勢(既定:読込時の相対姿勢)
  "spring": { "frequencyHz": 240, "dampingRatio": 1 } }
```

### `ballSocket` — 並進 3 自由度を拘束(回転自由)
2 体。2 アンカー点を一致。
```jsonc
{ "type": "ballSocket", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `hinge` — 蝶番(並進 3＋角 2 を拘束、1 軸回転のみ)
2 体。`ballSocket` ＋ ヒンジ軸合わせ。
```jsonc
{ "type": "hinge", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "localHingeAxisA": [0,0,1], "localHingeAxisB": [0,0,1],  // 既定 +Z
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `swivelHinge` — 首振り蝶番(2 軸回転を許可)
2 体。A の swivel 軸と B の hinge 軸。
```jsonc
{ "type": "swivelHinge", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "localSwivelAxisA": [0,0,1], "localHingeAxisB": [0,0,1],
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

---

## B. 距離

### `centerDistanceConstraint` — 重心間距離を固定
```jsonc
{ "type": "centerDistanceConstraint", "bodyA": "...", "bodyB": "...",
  "targetDistanceMm": 100, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `centerDistanceLimit` — 重心間距離を [min,max] に制限
```jsonc
{ "type": "centerDistanceLimit", "bodyA": "...", "bodyB": "...",
  "minDistanceMm": 50, "maxDistanceMm": 150,
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `distanceServo` — アンカー間距離を目標へ(バネ駆動)
※ **バネ付きアイドラの予圧バネはこれ**。
```jsonc
{ "type": "distanceServo", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "targetDistanceMm": 5,
  "servo": { "maximumForceN": 50 }, "spring": { "frequencyHz": 30, "dampingRatio": 1 } }
```

### `distanceLimit` — アンカー間距離を [min,max] に制限
```jsonc
{ "type": "distanceLimit", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "minDistanceMm": 0, "maxDistanceMm": 20,
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

---

## C. 軸方向の並進

### `linearAxisServo` — 軸に沿った変位を目標へ
軸＝`localAxisA`(bepu の LocalPlaneNormal)。
```jsonc
{ "type": "linearAxisServo", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "localAxisA": [0,0,1], "targetOffsetMm": 10,
  "servo": { "maximumForceN": 100 }, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `linearAxisMotor` — 軸に沿った速度を駆動
```jsonc
{ "type": "linearAxisMotor", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "localAxis": [0,0,1], "targetVelocityMps": 0.5,
  "motor": { "maximumForceN": 100, "softness": 0 } }
```

### `linearAxisLimit` — 軸に沿った変位を [min,max] に制限
```jsonc
{ "type": "linearAxisLimit", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "localAxis": [0,0,1], "minOffsetMm": 0, "maxOffsetMm": 50,
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `pointOnLineServo` — 点を直線上に拘束(2 並進自由度を拘束)
```jsonc
{ "type": "pointOnLineServo", "bodyA": "...", "bodyB": "...",
  "localOffsetAMm": [0,0,0], "localOffsetBMm": [0,0,0],
  "localDirection": [0,0,1],
  "servo": { "maximumForceN": 100 }, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

---

## D. 角度・ヒンジ角(並進は拘束しない)

### `angularHinge` — 1 軸回転だけ残す(角 2 自由度を拘束)
```jsonc
{ "type": "angularHinge", "bodyA": "...", "bodyB": "...",
  "localHingeAxisA": [0,0,1], "localHingeAxisB": [0,0,1],
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `angularSwivelHinge` — 2 軸回転を許可(角 1 自由度を拘束)
```jsonc
{ "type": "angularSwivelHinge", "bodyA": "...", "bodyB": "...",
  "localSwivelAxisA": [0,0,1], "localHingeAxisB": [0,0,1],
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `angularServo` — 相対姿勢を目標へ駆動
```jsonc
{ "type": "angularServo", "bodyA": "...", "bodyB": "...",
  "targetRelativeRotationEulerDeg": [0,0,0],
  "servo": { "maximumForceN": 100 }, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `angularMotor` — 相対角速度を目標へ駆動(全軸)
```jsonc
{ "type": "angularMotor", "bodyA": "...", "bodyB": "...",
  "targetAngularVelocityLocalADegPerSec": [0,0,90],
  "motor": { "maximumForceN": 100, "softness": 0 } }
```

### `angularAxisMotor` — 1 軸まわりの角速度を駆動
```jsonc
{ "type": "angularAxisMotor", "bodyA": "...", "bodyB": "...",
  "localAxisA": [0,0,1], "targetAngularVelocityDegPerSec": 360,
  "motor": { "maximumForceN": 100, "softness": 0 } }
```

### `angularAxisGearMotor` — ギア(軸角速度を比で結合)
※ 既に確定済み。`ratio` = VelocityScale。
```jsonc
{ "type": "gear", "bodyA": "...", "bodyB": "...",
  "localAxisA": [0,0,1], "ratio": 2.0,
  "motor": { "maximumForceN": 1000, "softness": 0 } }
```
> 別名 `gear` を `angularAxisGearMotor` の別表記として受理(既存の確定に合わせる)。

---

## E. ねじり・首振り(球関節の自由度制御)

### `twistServo` — ねじり角を目標へ
基底 A/B でねじり軸を定義。
```jsonc
{ "type": "twistServo", "bodyA": "...", "bodyB": "...",
  "localBasisAEulerDeg": [0,0,0], "localBasisBEulerDeg": [0,0,0],
  "targetAngleDeg": 0,
  "servo": { "maximumForceN": 100 }, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `twistMotor` — ねじり速度を駆動
```jsonc
{ "type": "twistMotor", "bodyA": "...", "bodyB": "...",
  "localAxisA": [0,0,1], "localAxisB": [0,0,1], "targetAngularVelocityDegPerSec": 90,
  "motor": { "maximumForceN": 100, "softness": 0 } }
```

### `twistLimit` — ねじり角を [min,max] に制限
```jsonc
{ "type": "twistLimit", "bodyA": "...", "bodyB": "...",
  "localBasisAEulerDeg": [0,0,0], "localBasisBEulerDeg": [0,0,0],
  "minAngleDeg": -45, "maxAngleDeg": 45,
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `swingLimit` — 軸の首振りを円錐角で制限
`maxSwingAngleDeg` をライブラリが MinimumDot=cos(角) に変換。
```jsonc
{ "type": "swingLimit", "bodyA": "...", "bodyB": "...",
  "axisLocalA": [0,0,1], "axisLocalB": [0,0,1], "maxSwingAngleDeg": 30,
  "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

---

## F. 単一ボディ → 世界

### `oneBodyLinearServo` — ボディ上の点を世界座標の目標位置へ
```jsonc
{ "type": "oneBodyLinearServo", "body": "...",
  "localOffsetMm": [0,0,0], "targetWorldMm": [100,200,0],
  "servo": { "maximumForceN": 100 }, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `oneBodyLinearMotor` — ボディの並進速度を駆動
```jsonc
{ "type": "oneBodyLinearMotor", "body": "...",
  "localOffsetMm": [0,0,0], "targetVelocityMps": [0,0,0.5],
  "motor": { "maximumForceN": 100, "softness": 0 } }
```

### `oneBodyAngularServo` — ボディを世界姿勢の目標へ
```jsonc
{ "type": "oneBodyAngularServo", "body": "...",
  "targetWorldOrientationEulerDeg": [0,0,0],
  "servo": { "maximumForceN": 100 }, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```

### `oneBodyAngularMotor` — ボディの角速度を駆動
```jsonc
{ "type": "oneBodyAngularMotor", "body": "...",
  "targetAngularVelocityDegPerSec": [0,0,90],
  "motor": { "maximumForceN": 100, "softness": 0 } }
```

---

## G. ソフトボディ(多体) — 当ドメインでは任意

### `areaConstraint` — 3 体が成す三角形の面積を保つ(布)
```jsonc
{ "type": "areaConstraint", "bodies": ["a","b","c"],
  "targetAreaMm2": 100, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```
> bepu は内部で「スケール済み面積」を保持。ライブラリが mm² から変換。

### `volumeConstraint` — 4 体が成す四面体の体積を保つ(軟体)
```jsonc
{ "type": "volumeConstraint", "bodies": ["a","b","c","d"],
  "targetVolumeMm3": 1000, "spring": { "frequencyHz": 60, "dampingRatio": 1 } }
```
> 同上、内部は「スケール済み体積」。

---

## 命名規則の要約

| 接尾 | 意味 | 付帯 |
|---|---|---|
| `...Constraint` / `weld` / `hinge` 等 | 等式で固定 | `spring` |
| `...Servo` | 目標位置/姿勢/距離へ駆動 | `spring` ＋ `servo` |
| `...Motor` | 目標速度へ駆動 | `motor` |
| `...Limit` | 範囲内で自由(不等式) | `spring` |

## ローダ検証(拘束)

- `bodyA`/`bodyB`/`body`/`bodies` の id が `bodies` に存在するか。
- 体数の一致(2 体/単体/3 体/4 体)。
- 軸ベクトルが非ゼロ(正規化はローダが実施)。
- `spring`/`servo`/`motor` の必須有無が型に合うか。
- `min ≤ max`(各 limit)。
- いずれも例外でなく診断(error/warning)に集約。
