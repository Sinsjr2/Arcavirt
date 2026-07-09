#!/bin/bash
# E2 Lite 用 GDB サーバー（e2-server-gdb）を RX64M(R5F564ML) 向けの
# 実機検証済みパラメータで起動する。
# パラメータ列はユーザーの実機環境の e2 studio が実際に RX64M への接続に
# 成功した起動コマンドから採取したもの（-uInputClock/-uPTimerClock は
# ボードの外部クロック 24MHz 系列、-uUseFine=0 で JTAG 接続、
# -uhookWorkRamAddr は RX64M の RAM 配置に合わせた値）。
# フラグは「-uKey=」と値が別トークンである点に注意。
# 使い方: e2-server-gdb.sh [ポート番号]   （既定ポート 61234）
set -euo pipefail

: "${DEBUGCOMP_DIR:=/opt/renesas/DebugComp/RX}"
PORT="${1:-61234}"

E2SERVER="$(find "${DEBUGCOMP_DIR}" -type f -name e2-server-gdb 2>/dev/null | head -1)"
if [ -z "${E2SERVER}" ]; then
  echo "e2-server-gdb not found under ${DEBUGCOMP_DIR}" >&2
  exit 1
fi

# e2-server-gdb は cwd を DebugComp フォルダにして起動する必要がある
cd "$(dirname "${E2SERVER}")"

exec "${E2SERVER}" \
  -g E2LITE \
  -t R5F564ML \
  -p "${PORT}" \
  -uConnectionTimeout= 30 \
  -uClockSrcHoco= 0 \
  -uInputClock= 24 \
  -uPTimerClock= 120000000 \
  -uAllowClockSourceInternal= 1 \
  -uUseFine= 0 \
  -uJTagClockFreq= 6.00 \
  -w 1 \
  -z 0 \
  -uRegisterSetting= 0 \
  -uModePin= 0 \
  -uChangeStartupBank= 0 \
  -uStartupBank= 0 \
  -uDebugMode= 0 \
  -uExecuteProgram= 0 \
  -uIdCode= FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF \
  -uresetOnReload= 1 \
  -n 0 \
  -uWorkRamAddress= 1000 \
  -uverifyOnWritingMemory= 0 \
  -uProgReWriteIRom= 0 \
  -uProgReWriteDFlash= 0 \
  -uhookWorkRamAddr= 0x7fdd0 \
  -uhookWorkRamSize= 0x230 \
  -uOSRestriction= 0 \
  -l \
  -uCore= "SINGLE_CORE|enabled|1|main" \
  -uSyncMode= async \
  -uFirstGDB= main \
  --english \
  --gdbVersion= 16.2
