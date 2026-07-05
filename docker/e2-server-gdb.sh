#!/bin/bash
# E2 Lite 用 GDB サーバー（e2-server-gdb）を RX 向けの既定パラメータで起動する。
# パラメータ列は Renesas 純正デバッグアダプタ（renesas-gdb-adapter）の
# rx-E2LITE 設定を再現したもの。フラグは「-uKey=」と値が別トークンである点に注意。
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
  -p "${PORT}" \
  -uConnectionTimeout= 30 \
  -uClockSrcHoco= 0 \
  -uInputClock= 16 \
  -uPTimerClock= 16000000 \
  -uAllowClockSourceInternal= 1 \
  -uUseFine= 1 \
  -uFineBaudRate= 1.50 \
  -w 1 \
  -z 0 \
  -uHotPlug= 0 \
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
  -uhookWorkRamAddr= 0x25d0 \
  -uhookWorkRamSize= 0x230 \
  -uOSRestriction= 0 \
  -l \
  -uCore= "SINGLE_CORE|enabled|1|main" \
  -uSyncMode= async \
  -uFirstGDB= main \
  --english \
  --gdbVersion= 7.2
