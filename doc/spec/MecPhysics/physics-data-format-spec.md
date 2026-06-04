# データ形式仕様 — JSON スキーマ・glTF extras・Blender 検証

物理ライブラリが読み込むデータ形式の確定仕様。`physics-library-spec.md`(全体設計)と `physics-constraints-reference.md`(拘束 30 種)を補完する。
**Blender／glTF の編集可否に依存する箇所**は §6 のチェックリストで明示し、未確定点(辺タグ)も記録する。

---

## 0. 入力と規約

**ローダの入力**：(1) glTF(幾何＋extras)、(2) 機械設定 JSON、(3) 媒体ライブラリ JSON。機械設定と媒体は独立(同じ機械を別の媒体ライブラリで走らせられる)。

**共通規約**
- キーは **camelCase**。
- **glTF への参照**＝シーンルートからの `/` 区切り絶対パス(例 `machine/feedSection/feedRoller_geo`)。ローダがパス→ノード→**node index** に解決し、index を正典キーとする(物理↔レンダラ契約も index)。パス一意性をローダが検証。
- **JSON 内参照**＝id 文字列。
- **単位はフィールド名接尾辞(純 camelCase)**：`thicknessMm`・`grammageGsm`・`angleDeg`／`yieldAngleDeg`・`massKg`・`targetDistanceMm`・`*DegPerSec`・`*Mps`・`*MmPerSec`・`*N`。無単位の調整値(`sleepThreshold`・`bendingStiffness`・`stiffnessFactor`・`dampingRatio`・`ratio`・`friction`・`bounciness`)は接尾辞なし。ローダが内部 SI(m・rad・kg・s)へ変換。座標・幾何は glTF の m が正典。

---

## 1. 機械設定 JSON

### `global`
```jsonc
{ "schemaVersion": 1, "timestepMs": 0.3, "substepCount": 8,
  "velocityIterationCount": 1, "gravityMps2": [0,-9.81,0] }
```
`substepCount`/`velocityIterationCount` → bepu `SolveDescription`。`gravityMps2` はベクトル。`sleepThreshold` は media へ移動。

### `bodies`
```jsonc
[ { "id": "feedRoller", "members": ["machine/feed/feedRoller_geo"],
    "class": "kinematic", "massKg": 0 } ]
```
形状を持つ glTF ノードは body で**既定 static**。`bodies` は kinematic/dynamic のみ列挙。`members` は常にパス配列(複合は複数)。dynamic は massKg 必須・慣性は形状＋質量から自動。複合の本体枠は先頭メンバー。

### `materials`
```jsonc
[ { "id": "rubber", "friction": 1.1, "bounciness": 0.0 } ]
```
割り当ては glTF ノード extras の `material`(全 shape ノードに必須、**省略はエラー**)。接触 μ＝両 body の friction の**平均**(`ConfigureContactManifold`)。`bounciness` は bepu の接触バネ減衰＋`MaximumRecoveryVelocity` に写す近似。接触バネ剛性は当面 global 既定。

### `constraints`
`type` 判別配列。**全 30 種**の詳細フィールドは `physics-constraints-reference.md` 参照。`gear` は `angularAxisGearMotor` の別名。軸の既定＝各 body の +Z、アンカー既定＝節点原点、共有 `spring`/`servo`/`motor` ブロック。
```jsonc
[ { "type": "gear", "bodyA": "driveGear", "bodyB": "drivenGear", "ratio": 2.0 } ]
```

### `actuators`
```jsonc
[ { "id": "feedMotor", "body": "feedRoller", "kind": "rotary",
    "mode": "velocity", "axis": [0,0,1], "releasable": false } ]
```
- アクチュエータは bepu モーター拘束と別物：**ライブラリが毎ステップ kinematic ボディの速度を直接与える**(無限権限・厳密・パルス整数累積)。指令値は JSON でなく実行時。
- `kind`: rotary(度・度毎秒・軸まわり) / linear(mm・mm毎秒・軸方向)。
- `mode`: velocity(目標速度) / position(目標角・変位へ、到達速度はアプリのランプ次第)。最大速度設定なし(bepu に該当なし＝無制限)。
- **`releasable`(クラッチ)**：`true` の場合 body に massKg＋形状 必須。enable=true で kinematic 駆動(`SetLocalInertia` ゼロ)、enable=false で dynamic 解放(`SetLocalInertia`＝形状＋質量、ライブラリ内部の +Z 軸拘束を張りフリーホイール)。駆動時は軸拘束を外し kinematic-static 縮退を回避。

### 制御 API 対応(id 経由、専用名前空間)
- 読み取り＝`ISensor<T>`、書き込み＝`IOutputTransducer<T>`(メソッド `T Read()` / `void Write(T)`、対応するものだけ実装、論理・エッジはアプリ側)。
- アクチュエータ：指令＝`IOutputTransducer<float>`、現在角／変位＝`ISensor<float>`、releasable なら enable＝`IOutputTransducer<bool>`。
- 値は**ラッチ保持**、**人間単位**(rotary=度・度毎秒、linear=mm・mm毎秒)、**1 フレーム複数 Write は last-write-wins・次ステップ反映**、enable=false で速度 0(非 releasable は enable なし)。

---

## 2. 媒体ライブラリ JSON

### `media`
```jsonc
[ { "id": "cartonA", "controlMesh": "blanks/cartonA_control",
    "grammageGsm": 300, "thicknessMm": 0.4,
    "bendingStiffness": 0.5, "bendingDampingRatio": 1,
    "yieldAngleDeg": 45, "divisionLevel": 2, "sleepThreshold": 0.01 } ]
```
`controlMesh`(複雑ブランク)か `sizeMm:[w,h]`(平面シート)の一方。`bendingStiffness` は物理定数でなく**校正前提の調整値**(→bepu バネ周波数、分割レベルで自動スケール=解像度非依存)。`yieldAngleDeg` は公称(非スコア)。

### `scoreClasses`(共有トップレベル)
```jsonc
[ { "id": "crease", "stiffnessFactor": 0.1, "yieldAngleDeg": 5 } ]
```
`stiffnessFactor`＝媒体公称 `bendingStiffness` への乗数(相対)、`yieldAngleDeg`＝絶対。対称(山/谷なし、折り方向は機械のプラウから創発)。制御メッシュの辺タグ名と id が一致。タグ無し辺は媒体の公称値。

---

## 3. glTF extras 語彙

物理データは extras の単一キー **`physics`** 配下に置き、`role` で判別(表示・Blender カスタムプロパティとの衝突回避)。

```jsonc
"extras": { "physics": { "role": "...", /* role 固有 */ } }
```

| role | フィールド | 原点/軸・備考 |
|---|---|---|
| `collider` | `shape`, 寸法, `material` | 形状寸法。`material` 必須 |
| `sensor` | `id`, `rangeMm` | 原点=節点・方向 +Z。`ISensor<bool>`＋`ISensor<float>` |
| `sink` | `id`, `shape`, 寸法 | ゾーン体積(実体でない)。完全進入で連結塊ごと除去 |
| `spawn` | `id` | 位置・向き=節点 transform |
| `glueNozzle` | `id`, `rangeMm`, `radiusMm` | 原点=節点・方向 +Z。`IOutputTransducer<bool>` |
| `controlMesh` | `id` | メッシュ幾何＋辺タグが topology を定義(media から参照) |
| `scoreLines` | `controlMesh`, `class` | Loose Edges 由来の LINES。端点一致で controlMesh の辺へ対応付け |

### 形状語彙(collider / sink）
| shape | 寸法フィールド | bepu | 軸 |
|---|---|---|---|
| `box` | `widthMm`(X)・`heightMm`(Y)・`lengthMm`(Z) | `Box(w,h,l)` | 節点 X/Y/Z |
| `sphere` | `radiusMm` | `Sphere(r)` | — |
| `cylinder` | `radiusMm`・`lengthMm` | `Cylinder(r,len)` | len=節点 +Z |
| `capsule` | `radiusMm`・`lengthMm`(bepu セグメント長) | `Capsule(r,len)` | 節点 +Z |

**重要**：bepu の Cylinder/Capsule は軸が**局所 Y**。ライブラリが内部で **Y→Z 回転(局所 X 周り 90°)** を挿入し、軸を節点 +Z に合わせる(Blender の Z 長円柱がそのまま使える)。box/sphere は補正不要。

### スコア線ワークフロー(Blender だけ・ツール不要)
1. 粗い制御メッシュ(面)＝`role:"controlMesh"` ノード。
2. クラスごとに、スコアにする辺を選択→複製→分離して別オブジェクト化し、カスタムプロパティに `physics={role:"scoreLines", controlMesh:"<id>", class:"<scoreClass id>"}` を付ける。
3. glTF 書き出しで **Loose Edges を ON**(面に属さない辺を、最初のマテリアルスロットのマテリアルで LINES として出力)。クラス判定は extras で行うのでマテリアルスロット依存は使わない。
4. ライブラリが scoreLines の線分を controlMesh の辺と端点座標一致で対応付け→細分時にタグ継承→当該ヒンジが当該クラスの弱化。

---

## 4. 軸・座標規約(再掲)

- Y-up 右手系、1 単位 = 1 m(glTF/`index.md` 継承)。
- **節点の局所 +Z が主軸**：回転軸・ヒンジ・ギア軸・センサー光線・糊ノズル方向・cylinder/capsule 対称軸。原点＝ピボット／光線始点。

---

## 5. ローダ検証(例外を投げず診断を集約)

`LoadResult { bool Success; IReadOnlyList<Diagnostic> Diagnostics; World? World }`。全チェックを一括で回し、error が一つでも Success=false・World なし・全 error 列挙。I/O・parse 失敗も診断に含める。主な検証：
- パス参照の解決と一意性、id 重複。
- body 種別整合(アクチュエータ対象=kinematic、ギア従動=dynamic、dynamic=massKg、releasable=massKg＋形状)。
- 必要箇所に形状、`material` 必須、参照スコア／材料／媒体クラスの定義済み。
- 拘束の体数一致・軸非ゼロ・型ごとの spring/servo/motor 必須・`min≤max`。
- 値域(質量・厚み等は正)。

---

## 6. Blender / glTF 検証チェックリスト（要確認・変更余地あり）

データを Blender→glTF で実際に著述できるか、計画時に次を検証すること。**ここは Blender の能力に依存し、未確定**。

1. **カスタムプロパティ → `node.extras`**：オブジェクトのカスタムプロパティが glTF の `node.extras` に出るか。とくに **`physics` のような入れ子オブジェクト**(IDProperty グループ)が JSON 入れ子として出力されるか要確認。出ない場合はフラットなキー命名(例 `physicsRole`・`physicsShape`)へ変更する可能性。
2. **ノード名と階層**：名前が保持されパスが安定するか。無名ノードは最適化で消える。名前の一意性は保証されないので index を正典に。
3. **メッシュ書き出し**：モディファイア適用後のメッシュが出るか(controlMesh の topology に影響)。
4. **局所軸**：オブジェクトの局所 +Z が意図どおりか。円柱が Z 長か。
5. **折り／スコアの辺タグ(確定：Loose Edges 方式)**：glTF に一級の「辺」は無いため、スコア辺を **loose edges(面に属さない辺)** として持たせ、Blender 標準の glTF 書き出しオプション **「Loose Edges」**で **LINES プリミティブ**として出力する。クラスごとに別オブジェクト化し `physics={role:"scoreLines", controlMesh:"<id>", class:"<id>"}` を付ける。ライブラリが LINES の線分を controlMesh の辺へ端点一致で対応付け、細分時にタグ継承。**ツール不要・JSON 手書き不要**。要確認なのは Blender 版での Loose Edges 出力と extras の同時付与のみ。

---

## 7. 確定状況

- 確定：global / bodies / materials / constraints(30 種) / actuators(クラッチ込) / media / scoreClasses / glTF extras の全 role(scoreLines 含む) / 形状語彙 / 軸規約 / スコア辺タグ(Loose Edges 方式) / ローダ検証。
- 注記：extras の `physics` 入れ子が Blender で出せない場合はフラット命名へ移行(§6-1)。
