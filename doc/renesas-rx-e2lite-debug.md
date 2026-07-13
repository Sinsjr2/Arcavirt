# RX マイコンを E2 Lite でデバッグする（Docker）

## 構成
```
rx-elf-gdb ──TCP(GDB remote)──> e2-server-gdb ──USB──> E2 Lite ──JTAG/FINE──> RX
```
- e2-server-gdb は DebugComp/RX に含まれる（コンテナ内 `$DEBUGCOMP_DIR`）。
- rx-elf-gdb・rx-elf-gcc 等のコンパイラ一式はソースからビルドして `/opt/rx-elf` に入る
  （DebugComp にも Renesas ビルド済み rx-elf-gdb が含まれるが、動作に libpython3.10 が
  別途必要になるため未使用。自前ビルド版で全機能を確認済み）。VS Code や e2 studio は不要。

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
   - non-stop + `target extended-remote` で接続する（理由は下記「実機検証で判明した
     注意点」を参照）。

## 動作確認
- ホストとコンテナ両方で `lsusb` に同じ Renesas デバイスが出れば、パススルー成功。
- `ls -l /dev/bus/usb/*/*` でノードに rw できるか（udev の MODE）を確認する。
- gdb で `monitor is_target_connected` が接続を返せば通信成立。

## 自動合否判定（検査スクリプト）
- `gdb_mi_inspect.py` は日常デバッグ用の `rx64m.gdbinit` とは別の、検査専用スクリプト
  （GDB MI モードで動作。理由は下記「実機検証で判明した注意点」を参照）。
  接続 → set_target → load → reset_and_halt → break main → continue_to_main →
  レジスタ/メモリ読み出し、を1手順ずつ `INSPECT:BEGIN:<name>` / `INSPECT:END:<name>`
  マーカーで囲んで標準出力に流す。
- `gdb_inspect_judge.py` はそのログを読み、マーカー対の整合性・`INSPECT:ALL_DONE`
  センチネルの有無・既知のエラー文字列（`monitor` の失敗など）だけで合否判定する
  汎用（マイコン非依存）判定器。
- 実行手順:
  1. サーバー起動（別シェル/バックグラウンド）: `e2-server-gdb.sh 61234`
  2. `run-inspection.sh /work/test.elf [タイムアウト秒(既定120)]` を実行する。
     内部で `gdb_mi_inspect.py` を `timeout` 付きで実行し、出力を
     `gdb_inspect_judge.py` に渡して exit code 0（合格）/非0（不合格）を返す。
     ターゲットがハングして応答しない場合も `timeout` で打ち切られ、
     `ALL_DONE` 未検出により自動的に不合格となる。

## 実機検証で判明した注意点
- **`set_target` のデバイス名は `R5F564ML`**（`grep -ri R5F564 $DEBUGCOMP_DIR` で確認済み）。
- **`e2-server-gdb.sh` のクロック/接続方式**: RX64M 実機は外部クロック 24MHz・JTAG 接続
  （`-uInputClock=24 -uPTimerClock=120000000 -uUseFine=0 -uJTagClockFreq=6.00`、
  `-t R5F564ML` で起動時にターゲットを指定）。実際に動作する e2 studio の起動コマンドを
  参考に確定した値。
- **`g` パケット（一括レジスタ読み出し）が E01 で失敗する既知の問題**: この
  e2-server-gdb（v10.5.1）は個別レジスタ読み出し（`p` パケット）は正常に処理するが、
  接続直後の一括読み出しには失敗する。`set non-stop on` + `set mi-async on` +
  `target extended-remote`（`target remote` ではない）で接続すると `g` パケットが
  発行されず回避できる。
- **non-stop モードでの `continue`/`step` はバッチ CLI と相性が悪い**: reset 後に
  `monitor enable_stopped_notify_on_connect` / `enable_execute_on_connect` で対象を
  一旦自動実行→自動割り込みで停止させ、ブレークポイント設定後は
  `monitor ignore_continue_during_connect` を挟んでから `continue` する。ただし
  `continue` は非同期通知（`*stopped`）が届くまで GDB バッチ CLI 側では待たないため、
  自動判定スクリプトは GDB MI モードで明示的に `*stopped` を待つ実装
  （`gdb_mi_inspect.py`）にしている。対話利用（`rx64m.gdbinit`）では人間が都度
  状態を確認しながら操作すればよいため、この問題は起きにくい。
- **DebugComp のジャー展開時の落とし穴**: `rx.debug` / `rx.debug.gdb.linux.x86_64` /
  `rx.debug.jlink.linux.x86_64` / `rx.debug.ffw.linux.x86_64` の4パッケージは
  すべて同名 `debug_support.tar.xz` を持つため、単純に同一ディレクトリへ展開すると
  後勝ちで上書きされる（`libCommuni.so` 等が消える原因になった）。Dockerfile では
  jar ごとに個別展開してからマージするようにして回避している。
- **リッスンポートへの素の TCP 接続（開通プローブ）で GDB を受け付けなくなる**:
  e2-server-gdb の待ち受けポートに `bash` の `/dev/tcp` 等で接続確認を行うと、
  サーバーがそれをクライアントの接続・切断（`Disconnected from the Target
  Debugger.`）として扱い、以後 rx-elf-gdb の `-target-select` が応答しなくなる。
  起動完了はポートではなく、サーバーログに `GDB: <ポート番号>` 行が出たことで
  判定する（非 root 化の実機検証時に判明。root でも同様に再現する）。

## トラブルシュート
- コンテナで lsusb に出ない → ホストで挿さっているか、compose の USB 設定、udev を確認。
- `e2-server-gdb not found` → DebugComp 取得に失敗。ビルドログと DEBUGCOMP_BASE_URL を確認。
- 接続できるが書き込めない → デバイス名（set_target）とエミュレータ結線（FINE/JTAG）を確認。
- `-target-select` が応答しない（開通プローブ後に限らず、正規の切断後も再現する） →
  `doc/renesas-rx-e2lite-monitor-commands.md` の「重要な発見: 初回切断後は新規接続を受け付けなくなる」
  「復旧手順（USB 切断なし）」を参照。e2-server-gdb プロセスの再起動のみで復旧できる。

## 関連ドキュメント
- `doc/renesas-rx-e2lite-monitor-commands.md` — monitor コマンド一覧・動作・
  復旧手順のリファレンス。
