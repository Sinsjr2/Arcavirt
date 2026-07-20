# ブランチ命名ガイドライン

## 目的
claude codeが自動的に作成する作業用ブランチを、他のブランチと一目で区別し、
由来(手動指示 or beadsのissueから起票)を追跡できるようにする。

## 命名規則
`claude/<作業内容の短い説明>[_<beads issue番号>]`

- prefix: 常に `claude/`
- 作業内容の短い説明: 英語小文字・ハイフン区切り(kebab-case)
- beadsのissueから作成した場合: 末尾に `_<issue番号>` を付与

## 例
- `claude/add-docker-claude-support_Arcavirt-42`(beads issueから作成)
- `claude/fix-typo-in-readme`(issueなし、単発の指示から作成)
