#!/bin/bash
# RX64M 向け CMake ビルドをコンテナ外から一発で実行する。
# 使い方: tool/build-rx.sh [preset]   (preset 省略時は rx64m)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PRESET="${1:-rx64m}"

"${SCRIPT_DIR}/rx-run.sh" bash -c \
  "cd RenesasRXNative/test && cmake --preset '${PRESET}' && cmake --build --preset '${PRESET}'"
