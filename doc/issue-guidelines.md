# issue ガイドライン(beads)

## 目的
issue は着手可能な作業の単位。「何をすれば終わりか」が書かれていない issue は着手できず、放置される。着手可能かどうかを機械的に判別できる状態を保つ。

## Definition of Ready（着手可能の条件）
issue は以下の 5 項目が揃って初めて着手可能とみなす:

1. **背景** — なぜこの作業が必要か
2. **作業内容** — 具体的な対象（ファイル・クラス・プロジェクト名）まで書いたチェックリスト
3. **受け入れ条件** — 検証可能な形（コマンドが成功する、〜ができる）。beads の acceptance 欄に記述
4. **スコープ外** — やらないことと、それを扱う別 issue への参照
5. **依存** — 先行 issue。`bd dep add` で実際に依存関係も設定

description のテンプレート:

```
## 背景
なぜこの作業が必要か

## 作業内容
- [ ] 対象: src/Foo.cs の Bar クラスを…

## スコープ外
- …は別 issue(Arcavirt-xxx)

## 依存
- blocked-by: Arcavirt-xxx
```

受け入れ条件は description ではなく beads の acceptance 欄（`--acceptance`）に記述する。

## deep-dive ラベル
- Definition of Ready を満たさない issue には `deep-dive` ラベルを付ける。「情報が不完全で、着手前に深掘りが必要」の印。
- 詳細化が完了したら `deep-dive` を外す。`deep-dive` が付いたままの issue には着手しない。
- 一覧確認の例: `bd list --label deep-dive`。
- `TODO` ラベルは従来の用途のまま残す（`deep-dive` とは意味が異なる）。

## 設計判断が必要な issue は spike を切る
- クラス構成や形式の選定など設計判断なしに実装できない issue は、設計 spike を子 issue として切り出し、親を spike に `blocked-by` で依存させる。
- spike の完了条件は次の 2 点: ① `doc/spec/` 配下に設計書（選択肢の比較・採用案・テスト方針）を作成し承認を得る、② 採用案に基づく実装子 issue を beads に登録し依存関係を設定。
- 親 issue の暫定の受け入れ条件は spike 完了時に検証可能な形へ更新する。
- 例: Arcavirt-3jf（RXv2 対応）に対する Arcavirt-3jf.1（構造設計と issue 分割）。

## このガイドライン自体について
commit-guidelines.md と同じ方針（運用と規約がずれたら更新、1 回限りの例外では更新しない）。
