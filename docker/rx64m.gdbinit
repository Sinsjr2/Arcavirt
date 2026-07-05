# RX64M + E2 Lite 用 GDB 起動スクリプト（生 rx-elf-gdb での接続手順）。
# 使い方:
#   1) 別プロセスで  e2-server-gdb.sh 61234  を起動しておく
#   2) rx-elf-gdb -x /opt/renesas/rx64m.gdbinit path/to/test.elf
#
# TODO(要確認): set_target のデバイス名は実機の型番に合わせること。
#   本プロジェクトの test は RX64M（ROM 4MB / RAM 512KB）で R5F564ML 系だが、
#   パッケージ末尾は未確定。DebugComp 取得後に
#   `grep -ri R5F564 $DEBUGCOMP_DIR` で正式名を確認して置き換える。

target remote 127.0.0.1:61234
monitor is_target_connected
monitor set_target,R5F564ML
monitor is_target_connected
monitor prg_download_start_on_connect
load
monitor configuration_complete
monitor prg_download_end
monitor reset
monitor launch_complete
break main
continue
