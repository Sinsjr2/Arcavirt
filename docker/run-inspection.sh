#!/bin/bash
# RX64M + E2 Lite 用の検査オーケストレーションスクリプト。
# gdb_mi_inspect.py（GDB MI モード版）を実行し、その出力を
# gdb_inspect_judge.py に渡して自動合否判定させ、判定器の exit code を
# そのまま返す。
#
# 使い方: run-inspection.sh <elfファイルパス> [タイムアウト秒(既定120)]
#
# 前提: e2-server-gdb は事前に別プロセスで起動済みであること。
#   本スクリプトは e2-server-gdb のライフサイクル管理（起動・停止）は
#   一切行わない。
#
# 実機検証で判明: 単純な逐次バッチコマンド列（rx64m-inspect.gdbinit を
#   rx-elf-gdb -batch -x で流す旧方式）は、この e2-server-gdb の
#   non-stop モードにおける非同期 *stopped 通知を GDB バッチ CLI が
#   待たずに次のコマンドへ進んでしまい失敗する。GDB MI モードで
#   Python から明示的に *stopped を待つ gdb_mi_inspect.py に置き換えた。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MI_INSPECT="${SCRIPT_DIR}/gdb_mi_inspect.py"
JUDGE="${SCRIPT_DIR}/gdb_inspect_judge.py"

ELF="${1:-}"
TIMEOUT_SEC="${2:-120}"

if [ -z "${ELF}" ]; then
  echo "Usage: run-inspection.sh <elfファイルパス> [タイムアウト秒(既定120)]" >&2
  exit 1
fi

LOGFILE="$(mktemp)"
trap 'rm -f "${LOGFILE}"' EXIT

# gdb_mi_inspect.py がハングした場合でも timeout が強制終了させる。その場合
# INSPECT:ALL_DONE を出力しないため、判定器側で自動的に NG と判定される。
# 非ゼロで終了しても set -e でスクリプトを止めず、最終的な合否判定は
# judge スクリプトに委ねるため || true で握りつぶす。
timeout "${TIMEOUT_SEC}" python3 "${MI_INSPECT}" "${ELF}" 2>&1 | tee "${LOGFILE}" || true

python3 "${JUDGE}" "${LOGFILE}"
exit $?
