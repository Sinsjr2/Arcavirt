#!/usr/bin/env -S dotnet run
// RX64M + E2 Lite 用「検査」GDB スクリプト（MI モード版）。
//
// 日常デバッグ用の rx64m.gdbinit とは別。docker/gdb_inspect_judge.cs で
// 判定する INSPECT:BEGIN/END マーカーを標準出力へ流す点は
// 旧 rx64m-inspect.gdbinit と同じインターフェースを踏襲している。
//
// 実機検証で判明: plain な "target remote" + all-stop の逐次バッチ
// コマンド列（旧 rx64m-inspect.gdbinit 方式）では、この e2-server-gdb は
// 接続直後の一括レジスタ読み出し（g パケット）に E01 を返して失敗する
// （個別レジスタ読み出し p は正常）。non-stop モードにすると g パケットは
// 回避できるが、今度は reset 後の自動実行→自動停止や continue の完了を
// GDB バッチ CLI が非同期の *stopped 通知を待たずに次のコマンドへ進んで
// しまい "Cannot execute this command while the selected thread is
// running" で失敗する。GDB の CLI バッチモードは *stopped のような
// out-of-band 通知を処理するイベントループを持たないため。
//
// 実際に動作する e2 studio の接続ログ（GDB/MI 形式）を解析した結果、
// e2 studio は GDB MI モードで動作しており、-exec-continue 等の非同期
// 実行コマンドの後に *stopped レコードが届くまで明示的に待っている
// ことを確認した。本スクリプトはこれを C# から同様に再現する。
//
// 使い方:
//   1) 別プロセスで e2-server-gdb.sh <port> を起動しておく（既定 61234）
//   2) ./gdb_mi_inspect.cs path/to/test.elf [ポート番号(既定 61234)]
//
// 出力は docker/gdb_inspect_judge.cs で自動判定する。

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

const string DeviceName = "R5F564ML";

void Step(string name, Action action) {
    Console.WriteLine($"INSPECT:BEGIN:{name}");
    action();
    Console.WriteLine($"INSPECT:END:{name}");
}

int Run(string elfPath, int port) {
    var mi = new MiSession(elfPath);
    try {
        mi.ReadUntil(@"\(gdb\)", 5);

        mi.Send("-gdb-set pagination off");
        mi.ReadUntil(@"\^done");
        mi.Send("-gdb-set non-stop on");
        mi.ReadUntil(@"\^done");
        mi.Send("-gdb-set mi-async on");
        mi.ReadUntil(@"\^done");

        Step("connect", () => {
            mi.Send($"-target-select extended-remote 127.0.0.1:{port}");
            mi.ReadUntil(@"\^connected");
            var reply = mi.Monitor("is_target_connected");
            Console.WriteLine(reply);
        });

        Step("set_target", () => {
            mi.Monitor($"set_target,{DeviceName}");
            var reply = mi.Monitor("is_target_connected");
            Console.WriteLine(reply);
        });

        Step("load", () => {
            mi.Monitor("prg_download_start_on_connect");
            mi.Send("-interpreter-exec console \"load\"");
            mi.ReadUntil(@"\^done|\^error", 30);
            mi.Monitor("configuration_complete");
            mi.Monitor("prg_download_end");
            mi.Monitor("reset");
        });

        Step("reset_and_halt", () => {
            // reset 後、この2つの monitor コマンドで対象を一旦自動実行させ、
            // 直後に自動割り込みで停止させる（e2 studio と同じ手順）。
            mi.Monitor("enable_stopped_notify_on_connect");
            mi.Monitor("enable_execute_on_connect");
            mi.ReadUntil(@"\*stopped");
        });

        Step("break_main", () => {
            mi.Send("-break-insert -h -f main");
            mi.ReadUntil(@"\^done|\^error");
        });

        Step("continue_to_main", () => {
            mi.Monitor("ignore_continue_during_connect");
            mi.Send("-exec-continue --thread 1");
            mi.ReadUntil(@"\^running|\^error");
            mi.ReadUntil(@"\*stopped");
        });

        Step("read_registers", () => {
            mi.Send("-data-list-register-values x");
            var reply = mi.ReadUntil(@"\^done|\^error");
            Console.WriteLine(reply);
        });

        Step("read_memory", () => {
            mi.Send("-data-evaluate-expression \"$sp\"");
            var spReply = mi.ReadUntil(@"\^done|\^error");
            Console.WriteLine(spReply);
            var m = Regex.Match(spReply, "value=\"(0x[0-9a-fA-F]+)\"");
            if (m.Success) {
                var spAddr = m.Groups[1].Value;
                mi.Send($"-data-read-memory-bytes {spAddr} 8");
                var memReply = mi.ReadUntil(@"\^done|\^error");
                Console.WriteLine(memReply);
            }
        });

        Console.WriteLine("INSPECT:ALL_DONE");

        mi.Monitor("do_nothing", 3);
        mi.Send("-interpreter-exec console \"kill\"");
        Thread.Sleep(300);
        mi.Send("y");
        mi.ReadUntil(@"\^done|\^error", 5);
        return 0;
    } catch (Exception e) {
        Console.WriteLine($"INSPECT:ERROR: {e.Message}");
        return 1;
    } finally {
        mi.Close();
    }
}

if (args.Length < 1) {
    Console.Error.WriteLine("Usage: gdb_mi_inspect.cs <elfファイルパス> [ポート番号(既定 61234)]");
    return 2;
}

string elfPath = args[0];
int port = args.Length > 1 ? int.Parse(args[1]) : 61234;
return Run(elfPath, port);

// GDB MI セッションを1コマンドずつ送り、応答/非同期レコードを待つラッパー。
class MiSession {
    private const string Gdb = "rx-elf-gdb";
    private const double DefaultTimeoutSec = 20;

    private readonly Process _proc;
    private readonly BlockingCollection<string> _lines = new();

    public MiSession(string elfPath) {
        _proc = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = Gdb,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };
        _proc.StartInfo.ArgumentList.Add("--interpreter=mi2");
        _proc.StartInfo.ArgumentList.Add("-q");
        _proc.StartInfo.ArgumentList.Add("-nx");
        _proc.StartInfo.ArgumentList.Add(elfPath);

        _proc.OutputDataReceived += (_, e) => { if (e.Data is not null) _lines.Add(e.Data); };
        _proc.ErrorDataReceived += (_, e) => { if (e.Data is not null) _lines.Add(e.Data); };
        _proc.Start();
        _proc.BeginOutputReadLine();
        _proc.BeginErrorReadLine();
    }

    public void Send(string command) {
        _proc.StandardInput.Write(command + "\n");
        _proc.StandardInput.Flush();
    }

    // pattern(正規表現)にマッチする行が来るまで MI 出力を読み続ける。
    //
    // ^error/*stopped(異常系シグナル含む)にもマッチしうる呼び出し側の
    // パターンで、成功/失敗どちらも呼び出し側が判定できるようにする。
    public string ReadUntil(string pattern, double timeoutSec = DefaultTimeoutSec) {
        var regex = new Regex(pattern);
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSec);
        while (true) {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero) {
                break;
            }
            if (_lines.TryTake(out var line, remaining) && regex.IsMatch(line)) {
                return line;
            }
        }
        throw new TimeoutException($"timeout waiting for pattern: {pattern}");
    }

    // monitor コマンドを console 経由で発行し、^done/^error を待つ。
    public string Monitor(string cmd, double timeoutSec = DefaultTimeoutSec) {
        var escaped = cmd.Replace("\"", "\\\"");
        Send($"-interpreter-exec console \"monitor {escaped}\"");
        return ReadUntil(@"\^done|\^error", timeoutSec);
    }

    public void Close() {
        try {
            Send("-gdb-exit");
            Thread.Sleep(300);
        } catch {
        }
        try {
            _proc.Kill();
        } catch {
        }
    }
}