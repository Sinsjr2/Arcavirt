#!/bin/bash
# ホストの UID/GID をコンテナへ引き継いで、dev サービス上で任意コマンドを実行する。
# 使い方: tool/rx-run.sh <コマンド...>
#   例: tool/rx-run.sh bash
#       tool/rx-run.sh lsusb
#
# docker compose run を直接使うと UID は 1000 固定になるため、
# ホストの UID が 1000 以外の環境ではこのラッパーを正規ルートとする。
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

HOST_UID="$(id -u)"
HOST_GID="$(id -g)"
export HOST_UID HOST_GID

exec docker compose -f "${REPO_ROOT}/docker-compose.yml" run --rm dev "$@"
