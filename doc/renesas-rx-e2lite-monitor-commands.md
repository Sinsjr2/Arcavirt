# e2-server-gdb の monitor コマンドリファレンス

対象: `e2-server-gdb` Version 10.5.1.v20260505-085216（`docker/e2-server-gdb.sh` が
起動するバイナリ、`$DEBUGCOMP_DIR/e2-server-gdb`）。

用途:
1. E2 Lite 運用中のトラブル復旧時に参照するリファレンス
2. RX64M 用 gdb スタブを自作する際の参考資料
3. e2-server-gdb が無応答になった場合に、USB を物理的に切断せず復旧するための手順書

前提は `doc/renesas-rx-e2lite-debug.md` を参照（コンテナ構成・ビルド・USB パススルー）。

## 調査方法

- **静的調査**: コンテナ内で `grep -aoE "[ -~]{4,}" "$DEBUGCOMP_DIR/e2-server-gdb"`
  によりバイナリに埋め込まれた ASCII 文字列を抽出し、コマンド名・ヘルプ文字列を洗い出した
  （strings コマンドはランタイムイメージに含まれないため grep で代用）。
- **動的調査**: E2 Lite + RX64M(R5F564ML) 実機接続、`tool/build-rx.sh` でビルドした
  `RenesasRXNative/test/build-rx64m/cmake_test` を対象に、GDB MI モードで monitor
  コマンドを実行し応答を記録した。使い捨ての調査スクリプト（`gdb_mi_inspect.cs` の
  `MiSession` パターンを流用、リポジトリには含めない）で、接続直後・set_target 後・
  load 後・reset 直後・main 到達後の各状態でコマンドを発行した。

以下、各記載には **[静的]**（バイナリの埋め込み文字列からの抽出のみ、実行未確認）と
**[実機確認]**（上記の動的調査で実際に実行し応答を観測済み）を明示する。**[静的]** のみの
記載は「バイナリにこの文字列/説明が存在する」という事実の記録であり、コマンドの実際の
動作を保証するものではない。

## 実機確認済み: 日常デバッグで使用中のコマンド

`rx64m.gdbinit` / `gdb_mi_inspect.cs` が使う接続シーケンス。E2 Lite + RX64M
実機で複数回実行し、毎回同一の結果になることを確認した（`docker/run-inspection.sh`
実行結果: `OK: すべての手順が正常に完了し、エラーは検出されませんでした。`）。

実行順序と各コマンドの実機応答:

| 順序 | コマンド | 実機での応答（監視チャネルの `@"..."` 出力） |
|---|---|---|
| 1 | `monitor is_target_connected`（set_target 前） | `Connection status=connected.` |
| 2 | `monitor set_target,R5F564ML` | 応答テキストなし（`^done` のみ） |
| 3 | `monitor is_target_connected`（set_target 後） | `Connection status=connected.` |
| 4 | `monitor prg_download_start_on_connect` | 応答テキストなし |
| 5 | `load`（GDB 標準コマンド、monitor ではない） | 各セクションのダウンロード進捗（`.text` 等、計 16512 bytes、26 KB/sec） |
| 6 | `monitor configuration_complete` | 応答テキストなし |
| 7 | `monitor prg_download_end` | 応答テキストなし |
| 8 | `monitor reset` | 応答テキストなし |
| 9 | `monitor enable_stopped_notify_on_connect` | 応答テキストなし。直後に非同期 `*stopped` 通知（`signal-name="SIGINT"`, `func="sandbox_init"`）が届く |
| 10 | `monitor enable_execute_on_connect` | 応答テキストなし |
| 11 | `-break-insert -h -f main` | ハードウェアブレークポイント設定（`addr="0xffc033ac"`） |
| 12 | `monitor ignore_continue_during_connect` | 応答テキストなし |
| 13 | `-exec-continue` | 非同期 `*stopped`（`signal-name="SIGTRAP"`）が届く。ただし下記補足の通り停止先フレームは main のブレークポイントではなかった |

補足:
- **要注意（未解決の観測）**: 手順9（`enable_stopped_notify_on_connect` /
  `enable_execute_on_connect`）で自動停止した際の `*stopped` フレームは
  `addr="0xffc00000", func="sandbox_init"` だったが、直後に `monitor getPC` で
  読んだ PC は `0xffc003b4` で一致しなかった。さらに手順13（`-exec-continue`）後の
  `*stopped` フレームは `addr="0xffc003b4", func="PowerON_Reset_PC"`
  （`reason="signal-received"`, `signal-name="SIGTRAP"`）であり、`break main`
  で設定したブレークポイントのアドレス（`addr="0xffc033ac"`, `<main+4>`）とは
  異なっていた。つまり **この実行では continue が実際に main まで到達したことを
  確認できていない**。`run-inspection.sh` の自動判定（`gdb_inspect_judge.cs`）は
  `*stopped` の到達有無のみを見て BEGIN/END マーカーの整合性で合否判定しており、
  到達フレームのアドレスまでは検証していないため、OK 判定は「何らかの `*stopped`
  が届いたこと」の保証であって「main に到達したこと」の保証ではない。
  原因（continue を複数回発行する必要がある、`ignore_continue_during_connect` と
  自動停止状態の相互作用など）は未特定。日常デバッグでこの手順を使う際は、
  `continue` 後に `frame` や `getPC` で実際の停止位置を必ず確認すること。
- `monitor` コマンドの応答は GDB MI の `@"..."` レコード（target 出力チャネル）で届く。
  `^done`/`^error` 自体は成否のみを表し、テキストを含まない。

## 実機確認: 個別調査したコマンド

上記シーケンスに加えて、状態を変えながら以下を個別に実行した。

| コマンド | 実行タイミング | 結果 |
|---|---|---|
| `monitor help` | 接続直後 | `^done`（エラーなし）。ただし `@"..."` 等の応答テキストは一切届かず、サーバー側ログ（`e2-server-gdb.sh` の標準出力）にも出力されなかった。**コマンド名としては受理されるが、GDB monitor 経由でヘルプ本文を得る手段としては機能しない**（後述の静的抽出で得たヘルプ文字列は別経路向けの可能性がある。telnet インターフェース向けの文言である可能性は未検証） |
| `monitor printSettings` / `monitor printAttrib` / `monitor printBkPts` | 各種状態（接続直後・set_target後・ブレークポイント設定後 等） | いずれも `^done` のみで応答テキストなし。サーバー側ログにも出力なし。引数なしでは無出力と判断した（引数付きの挙動は未検証） |
| `monitor is_target_stopped`（引数なし） | reset 直後・自動停止後 | `^done` のみ、応答テキストなし |
| `monitor is_target_stopped,0`（コア番号引数付き） | 自動停止後 | `Target status=stopped.` — **引数なしとの差が実機で確認できた。埋め込みヘルプの `is_target_stopped,core` という書式（後述）が実際に必要** |
| `monitor getPC` | 自動停止後・`-exec-continue` 後（上記補足の通り main 到達は未確認） | いずれも `0xffc003b4`（16進アドレス文字列、両タイミングで同一値） |
| `monitor console_log_enable` / `monitor log_disable` | 接続後 | いずれも `^done` のみ、応答テキストなし。副作用は未観測（サーバーログの詳細度に変化があるかは未検証） |

## 重要な発見: 初回切断後は新規接続を受け付けなくなる

**e2-server-gdb は、最初の1クライアント接続が切断されると、以後の新規接続
（`-target-select`）を受け付けなくなる。** 切断が正規の GDB セッション終了
（`kill` → 通常の切断）であっても、異常な切断（開通プローブ等）であっても、
同じ症状になる。

実機で3回、独立に再現・確認した:

1. **正規の切断後に再接続できないケース**: GDB MI セッションで接続 →
   `monitor is_target_connected` に成功 → `kill` でクリーンに切断。直後に同じ
   ポートへ再度 `-target-select extended-remote` すると
   `^error,msg="could not connect: Connection timed out."` で失敗する。
   サーバーの標準出力には最初の切断時に `Disconnected from the Target Debugger.`
   が1回だけ記録され、以後は（2回目の接続試行があっても）ログに追記がない。
2. **開通プローブ（素の TCP 接続）による再現**: `doc/renesas-rx-e2lite-debug.md`
   記載の通り、bash の `/dev/tcp` でポートへ接続するだけで同様に切断扱いになる。
   実機で確認したコマンド:
   ```bash
   timeout 3 bash -c 'exec 3<>/dev/tcp/127.0.0.1/61234'
   ```
   実行後、サーバーログに `Disconnected from the Target Debugger.` が記録され、
   以後の `-target-select` は `^error,msg="could not connect: Connection refused."`
   （または環境により `Connection timed out.`）で失敗する。
3. どちらのケースも、**e2-server-gdb プロセス自体を再起動するだけで復旧し、
   USB の物理的な抜き差しは不要**なことを確認した（下記手順）。

**未特定の点**: 上記の「接続を受け付けなくなる」機構そのものは特定できていない。
`ps aux` で確認した際にはプロセス自体が既に終了しており（＝リスンソケットが
存在しないためのリジェクト/タイムアウト）、これがサーバーが「1接続限りで
自ら終了する」設計なのか、「切断イベントを受けて accept ループを止めるだけで
プロセスは生存し続ける」設計なのかは切り分けられていない（切断直後・生存確認の
タイミングで `pkill -f` が自分自身のコマンドライン文字列にマッチして意図せず
自プロセスを巻き込んでしまい、切り分け用の観測ができなかった）。運用上の対処
（プロセス再起動で復旧する）は機構によらず同じなので、復旧手順としては
以下で十分だが、原因の特定には追加調査が必要。

この制約により、日常運用でも「一度切断した gdb セッションへ同じサーバープロセスで
再接続する」運用はできない。デバッグをやり直す場合は、毎回 e2-server-gdb を
起動し直す必要がある。

## 復旧手順（USB 切断なし）

サーバーが無応答（`-target-select` が失敗する）になった場合の具体的なコマンド列。
E2 Lite の USB 抜き差しは行わない。

```bash
# 1. 残っている e2-server-gdb プロセスを強制終了する。
#    フルパスで絞り込むこと（"e2-server-gdb" だけだと、このコマンド自身の
#    コマンドライン文字列にもマッチして pkill が自分自身を巻き込むことがある。
#    実機検証で実際に踏んだ）。
pkill -9 -f "DebugComp/RX/e2-server-gdb"

# 2. 再起動する。
e2-server-gdb.sh 61234 > /tmp/server.log 2>&1 &

# 3. 起動完了を待つ。ポートへの疎通確認(nc 等)は使わないこと
#    （doc/renesas-rx-e2lite-debug.md に記載の通り、それ自体が新たな
#    開通プローブとなり同じ問題を誘発する）。
#    サーバーログに "GDB: <ポート番号>" 行が出るまで待つ。
until grep -q "GDB: " /tmp/server.log 2>/dev/null; do sleep 1; done

# 4. gdb から再接続する。
rx-elf-gdb -x /opt/renesas/rx64m.gdbinit /work/test.elf
```

手順1〜4は実機で3回（正規切断からの復旧2回、開通プローブからの復旧1回）実行し、
いずれも復旧を確認した。プロセス自体は `sleep` 等の待機なしに数秒で再起動でき、
E2 Lite のファームウェア再接続（`Finished target connection` ログ）も毎回成功した。

## 静的抽出: サーバーに埋め込まれた全 monitor コマンド一覧【静的】

バイナリ内に、GDB Eclipse プラグイン向けと思われるカテゴリ別のヘルプテキストが
埋め込まれていた（`grep -aoE "[ -~]{4,}"` の抽出結果、該当箇所はバイナリ中の
連続した文字列領域）。**この一覧は文字列としてバイナリに存在することのみを
確認したものであり、`monitor help` 経由では取得できず（上記実機確認の通り）、
各コマンドの実際の動作は個別に実機確認したもの以外は未検証。**

### Commands - general, used by Eclipse for control
```
$@set_current_core, get_current_cores, get_disabled_cores, get_freerunning_cores,
console_log_enable, log_disable, clear_protect_memory, notify_resumed,
get_synchronised_cores, get_core_go_synced_in_target_DLL,
get_core_step_synced_in_target_DLL, enable_synced_core_stopped_notify,
disable_synced_core_stopped_notify, do_nothing, enable_stopped_notify_on_connect,
ignore_continue_during_connect, enable_execute_on_connect, setCoreAttribute,
setClusterAttribute, addCore, setExecutableFile, printAttrib, printSettings,
printBkPts, printCoreStatus, setArgument, loadArgumentFile, skip_continue,
skip_step, can_go_all, can_stop_all, can_refresh_all, is_target_connected,
is_target_stopped, user_reset, getPC, configuration_complete, launch_complete,
all_launches_complete, prg_download_start_on_connect, prg_download_start,
prg_download_end, set_alternate_address, set_io_access_width,
get_target_max_address, get_no_hw_bkpts_available, get_no_sw_bkpts_available,
query_breakpoint_availability, force_rtos_off, force_rtos_on,
debug_packets_top_level, read_debug, write_debug, get_not_attached_cores,
get_interface_port, start_interface, list_interfaces, list_defaults, test,
start_resume, finish_resume, change_supply_voltage, set_simio_pipe,
adm_message, add_cl_bkpt, remove_cl_bkpt, remove_all_cl_bkpt,
get_cl_bkpt_count, get_cl_bkpt_index, set_target, get_debugger_name,
swvstart, swvstop, swvconfig, swvsupported, query_secure_mem_regions_mismatch,
hookStartFunc, hookStopFunc
```

埋め込みヘルプに個別説明があったもの:
- `protect_memory` — "prevents writing to region of memory and fills with
  specified data"。例: `monitor protect_memory,C0,20,FF`（すべて16進数）
- `setCoreAttribute` — "sets core attribute"。例:
  `monitor setCoreAttribute,MTD_SENS,Code,0xff008800`

### Commands - info
```
get_target_max_address, get_no_hw_bkpts_available, printAttrib, printSettings,
printBkPts, is_target_connected, is_target_stopped,core, help
```
- `printAttrib` — "prints out all attributes"
- `is_target_stopped,core` — 引数として `core` を取る旨がヘルプ文字列に明記されて
  いた。**この点は実機確認と一致した**（引数なしでは無応答、`is_target_stopped,0`
  で `Target status=stopped.` を得た）。

### Commands - general, development only
```
console_log_enable, log_disable
```
- `console_log_enable` — "enables logging to console"
- `log_disable` — "disables logging"

### Multicore commands - used by Eclipse for control, debug and test
```
addCore, set_current_core, get_current_cores, get_disabled_cores,
get_freerunning_cores, notify_resumed, resume,
get_core_go_synced_in_target_DLL, get_core_step_synced_in_target_DLL,
enable_synced_core_stopped_notify, disable_synced_core_stopped_notify,
do_nothing, enable_stopped_notify_on_connect, notifyCoreAttribChangesOn,
notifyCoreAttribChangesOff, start_resume, stop_resume, get_device_cores_type
```
- `addCore` — 例: `monitor addcore,61240,PE3,enabled,258`
- `do_nothing` — 近傍のコメント文字列に "Force UI update" / "do nothing" とあり、
  副作用なしの生存確認・UI更新トリガ用途と推測される（**未検証**）

RX64M（シングルコア、`-uCore= "SINGLE_CORE|enabled|1|main"`）では Multicore
向けコマンド群（`addCore` 等）は使用する場面がない可能性が高いが、これも未検証。

## 関連ドキュメント

- `doc/renesas-rx-e2lite-debug.md` — コンテナ構成・ビルド・USB パススルー・
  日常デバッグ手順。本ドキュメントの「初回切断後は新規接続を受け付けなくなる」という発見は、
  同ドキュメントに記載の「開通プローブで無応答になる」現象のより一般的な原因である
  （開通プローブに限らず、あらゆる切断が同じ症状を引き起こす）。
