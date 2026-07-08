# RX64M + E2 Lite 用 GDB 起動スクリプト（生 rx-elf-gdb での接続手順）。
# 使い方:
#   1) 別プロセスで  e2-server-gdb.sh 61234  を起動しておく
#   2) rx-elf-gdb -x /opt/renesas/rx64m.gdbinit path/to/test.elf
#
# 実機検証で判明:
#   - all-stop モードの "target remote" だと接続直後の一括レジスタ読み出し
#     (g パケット)がこの e2-server-gdb では E01 で失敗する（個別レジスタ
#     読み出し p は正常）。non-stop + target-async + "target
#     extended-remote" で接続すると g パケットが発行されず回避できる。
#   - reset 後に対象を実行状態へ持っていくには monitor
#     enable_stopped_notify_on_connect / enable_execute_on_connect の
#     組が必要（これで一旦自動実行→自動割り込みで停止する）。
#   - 自動判定を伴う検査には docker/gdb_mi_inspect.py（GDB MI モード）を
#     使うこと。バッチ CLI は非同期の *stopped 通知を待つイベントループを
#     持たないため、continue 直後に他コマンドを続けると
#     "Cannot execute this command while the selected thread is running"
#     で失敗しやすい。対話利用なら都度手動で確認・再実行すればよい。

set non-stop on
set mi-async on
target extended-remote 127.0.0.1:61234
monitor is_target_connected
monitor set_target,R5F564ML
monitor is_target_connected
monitor prg_download_start_on_connect
load
monitor configuration_complete
monitor prg_download_end
monitor reset
monitor enable_stopped_notify_on_connect
monitor enable_execute_on_connect
break main
monitor ignore_continue_during_connect
continue
