# RX マイコンを E2 Lite でデバッグする（Docker）

## 構成
```
rx-elf-gdb ──TCP(GDB remote)──> e2-server-gdb ──USB──> E2 Lite ──JTAG/FINE──> RX
```
- rx-elf-gdb と e2-server-gdb はどちらも DebugComp/RX に含まれる（コンテナ内 `$DEBUGCOMP_DIR`）。
- コンパイラ（rx-elf-gcc）はソースからビルドして `/opt/rx-elf` に入る。VS Code や e2 studio は不要。

## イメージのビルド
- `docker compose build` だけで完結する（手動ダウンロード不要）。
- builder 段で binutils 2.44 / gcc 14.2 / newlib 4.4.0 を llvm-gcc-renesas.com の
  ソース配布（GPL 準拠・認証不要）から取得しクロスビルドする。
- runtime 段で DebugComp/RX（e2-server-gdb・rx-elf-gdb 等）を e2 studio の
  p2 リポジトリ（認証不要）から取得する。
- 注意: ソースビルドのため初回ビルドは数十分かかる。

## ホスト側の準備（USB）
- Linux ホスト前提。E2 Lite を挿し、`lsusb` で Renesas（VID `045b`）が見えることを確認する。
- 非 root でアクセスできるよう udev ルールを作成する。
  `/etc/udev/rules.d/99-renesas-emulator.rules` に次を記述する。
  ```
  SUBSYSTEM=="usb", ATTRS{idVendor}=="045b", MODE="0666"
  ```
  反映は `sudo udevadm control --reload && sudo udevadm trigger`。
- docker-compose.yml が `/dev/bus/usb` のマウントと USB メジャー 189 の許可を行うので、
  コンテナからエミュレータが見える。

## デバッグの実行
1. コンテナに入る: `docker compose run --rm dev bash`
2. サーバー起動（別シェル/バックグラウンド）: `e2-server-gdb.sh 61234`
3. GDB 接続: `rx-elf-gdb -x /opt/renesas/rx64m.gdbinit /work/test.elf`
   - rx64m.gdbinit が接続・デバイス設定・書き込み・リセット・main ブレークまで行う。

## 動作確認
- ホストとコンテナ両方で `lsusb` に同じ Renesas デバイスが出れば、パススルー成功。
- `ls -l /dev/bus/usb/*/*` でノードに rw できるか（udev の MODE）を確認する。
- gdb で `monitor is_target_connected` が接続を返せば通信成立。

## 未確定事項 / TODO
- `monitor set_target` のデバイス名（RX64M の正式型番）は DebugComp 取得後に
  `grep -ri R5F564 $DEBUGCOMP_DIR` で確定する。
- DebugComp の jar 内 supportFileArchive の構造は未検証。初回ビルド後に
  `find $DEBUGCOMP_DIR -name e2-server-gdb` で実体を確認する。
- e2-server-gdb の共有ライブラリ依存は `ldd` で確認し、不足あれば
  Dockerfile runtime 段に apt 追加する。
- RX ツールチェーンのマルチリブ等 configure フラグは初回ビルドのログで検証する。

## トラブルシュート
- コンテナで lsusb に出ない → ホストで挿さっているか、compose の USB 設定、udev を確認。
- `e2-server-gdb not found` → DebugComp 取得に失敗。ビルドログと DEBUGCOMP_BASE_URL を確認。
- 接続できるが書き込めない → デバイス名（set_target）とエミュレータ結線（FINE/JTAG）を確認。
