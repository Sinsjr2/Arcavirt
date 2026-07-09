#!/usr/bin/env python3
"""RX64M + E2 Lite 用「検査」GDB スクリプト（MI モード版）。

日常デバッグ用の rx64m.gdbinit とは別。docker/gdb_inspect_judge.py で
判定する INSPECT:BEGIN/END マーカーを標準出力へ流す点は
旧 rx64m-inspect.gdbinit と同じインターフェースを踏襲している。

実機検証で判明: plain な "target remote" + all-stop の逐次バッチ
コマンド列（旧 rx64m-inspect.gdbinit 方式）では、この e2-server-gdb は
接続直後の一括レジスタ読み出し（g パケット）に E01 を返して失敗する
（個別レジスタ読み出し p は正常）。non-stop モードにすると g パケットは
回避できるが、今度は reset 後の自動実行→自動停止や continue の完了を
GDB バッチ CLI が非同期の *stopped 通知を待たずに次のコマンドへ進んで
しまい "Cannot execute this command while the selected thread is
running" で失敗する。GDB の CLI バッチモードは *stopped のような
out-of-band 通知を処理するイベントループを持たないため。

実際に動作する e2 studio の接続ログ（GDB/MI 形式）を解析した結果、
e2 studio は GDB MI モードで動作しており、-exec-continue 等の非同期
実行コマンドの後に *stopped レコードが届くまで明示的に待っている
ことを確認した。本スクリプトはこれを Python から同様に再現する。

使い方:
  1) 別プロセスで e2-server-gdb.sh <port> を起動しておく（既定 61234）
  2) python3 gdb_mi_inspect.py path/to/test.elf [ポート番号(既定 61234)]

出力は docker/gdb_inspect_judge.py で自動判定する。
"""

import re
import subprocess
import sys
import time

GDB = "rx-elf-gdb"
DEVICE_NAME = "R5F564ML"
DEFAULT_TIMEOUT_SEC = 20


class MiSession:
    """GDB MI セッションを1コマンドずつ送り、応答/非同期レコードを待つラッパー。"""

    def __init__(self, elf_path: str):
        self.proc = subprocess.Popen(
            [GDB, "--interpreter=mi2", "-q", "-nx", elf_path],
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            bufsize=1,
        )

    def send(self, command: str) -> None:
        self.proc.stdin.write(command + "\n")
        self.proc.stdin.flush()

    def read_until(self, pattern: str, timeout: float = DEFAULT_TIMEOUT_SEC) -> str:
        """pattern(正規表現)にマッチする行が来るまで MI 出力を読み続ける。

        ^error/*stopped(異常系シグナル含む)にもマッチしうる呼び出し側の
        パターンで、成功/失敗どちらも呼び出し側が判定できるようにする。
        """
        deadline = time.time() + timeout
        while time.time() < deadline:
            line = self.proc.stdout.readline()
            if not line:
                time.sleep(0.05)
                continue
            line = line.rstrip("\n")
            if re.search(pattern, line):
                return line
        raise TimeoutError(f"timeout waiting for pattern: {pattern}")

    def monitor(self, cmd: str, timeout: float = DEFAULT_TIMEOUT_SEC) -> str:
        """monitor コマンドを console 経由で発行し、^done/^error を待つ。"""
        escaped = cmd.replace('"', '\\"')
        self.send(f'-interpreter-exec console "monitor {escaped}"')
        return self.read_until(r"\^done|\^error", timeout=timeout)

    def close(self) -> None:
        try:
            self.send("-gdb-exit")
            time.sleep(0.3)
        except Exception:
            pass
        try:
            self.proc.terminate()
        except Exception:
            pass


def step(name: str):
    print(f"INSPECT:BEGIN:{name}")

    class _Ctx:
        def __enter__(self):
            return self

        def __exit__(self, exc_type, exc, tb):
            if exc_type is None:
                print(f"INSPECT:END:{name}")
            return False

    return _Ctx()


def run(elf_path: str, port: int) -> int:
    mi = MiSession(elf_path)
    try:
        mi.read_until(r"\(gdb\)", timeout=5)

        mi.send("-gdb-set pagination off")
        mi.read_until(r"\^done")
        mi.send("-gdb-set non-stop on")
        mi.read_until(r"\^done")
        mi.send("-gdb-set mi-async on")
        mi.read_until(r"\^done")

        with step("connect"):
            mi.send(f"-target-select extended-remote 127.0.0.1:{port}")
            mi.read_until(r"\^connected")
            reply = mi.monitor("is_target_connected")
            print(reply)

        with step("set_target"):
            mi.monitor(f"set_target,{DEVICE_NAME}")
            reply = mi.monitor("is_target_connected")
            print(reply)

        with step("load"):
            mi.monitor("prg_download_start_on_connect")
            mi.send('-interpreter-exec console "load"')
            mi.read_until(r"\^done|\^error", timeout=30)
            mi.monitor("configuration_complete")
            mi.monitor("prg_download_end")
            mi.monitor("reset")

        with step("reset_and_halt"):
            # reset 後、この2つの monitor コマンドで対象を一旦自動実行させ、
            # 直後に自動割り込みで停止させる（e2 studio と同じ手順）。
            mi.monitor("enable_stopped_notify_on_connect")
            mi.monitor("enable_execute_on_connect")
            mi.read_until(r"\*stopped")

        with step("break_main"):
            mi.send("-break-insert -h -f main")
            mi.read_until(r"\^done|\^error")

        with step("continue_to_main"):
            mi.monitor("ignore_continue_during_connect")
            mi.send("-exec-continue --thread 1")
            mi.read_until(r"\^running|\^error")
            mi.read_until(r"\*stopped")

        with step("read_registers"):
            mi.send("-data-list-register-values x")
            reply = mi.read_until(r"\^done|\^error")
            print(reply)

        with step("read_memory"):
            mi.send('-data-evaluate-expression "$sp"')
            sp_reply = mi.read_until(r"\^done|\^error")
            print(sp_reply)
            m = re.search(r'value="(0x[0-9a-fA-F]+)"', sp_reply)
            if m:
                sp_addr = m.group(1)
                mi.send(f'-data-read-memory-bytes {sp_addr} 8')
                mem_reply = mi.read_until(r"\^done|\^error")
                print(mem_reply)

        print("INSPECT:ALL_DONE")

        mi.monitor("do_nothing", timeout=3)
        mi.send('-interpreter-exec console "kill"')
        time.sleep(0.3)
        mi.send("y")
        mi.read_until(r"\^done|\^error", timeout=5)
        return 0
    except Exception as e:
        print(f"INSPECT:ERROR: {e}")
        return 1
    finally:
        mi.close()


def main() -> int:
    if len(sys.argv) < 2:
        print("Usage: gdb_mi_inspect.py <elfファイルパス> [ポート番号(既定 61234)]", file=sys.stderr)
        return 2
    elf_path = sys.argv[1]
    port = int(sys.argv[2]) if len(sys.argv) > 2 else 61234
    return run(elf_path, port)


if __name__ == "__main__":
    sys.exit(main())
