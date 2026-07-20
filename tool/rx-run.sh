#!/bin/bash
# ホストの UID/GID をコンテナへ引き継いで、dev サービス(常駐)上で任意コマンドを実行する。
# 使い方: tool/rx-run.sh <コマンド...>
#   例: tool/rx-run.sh bash
#       tool/rx-run.sh lsusb
#
# dev サービスは claude code を対話的に常駐利用するため docker compose run --rm
# (使い捨て)から常駐方式へ変更された。本スクリプトはコンテナが未起動なら起動した
# うえで docker compose exec を使う(起動済みなら up -d は何もしない)。
#
# docker compose run を直接使うと UID は 1000 固定になるため、
# ホストの UID が 1000 以外の環境ではこのラッパーを正規ルートとする。
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE=(docker compose -f "${REPO_ROOT}/docker-compose.yml")

HOST_UID="$(id -u)"
HOST_GID="$(id -g)"
export HOST_UID HOST_GID

"${COMPOSE[@]}" up -d dev

# 標準入力が端末でない場合(パイプ経由・非対話実行等)は擬似端末を確保しない。
if [ -t 0 ]; then
  exec "${COMPOSE[@]}" exec dev "$@"
else
  exec "${COMPOSE[@]}" exec -T dev "$@"
fi
