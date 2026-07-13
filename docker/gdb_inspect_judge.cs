#!/usr/bin/env -S dotnet run
// rx64m-inspect.gdbinit の出力ログを判定する。
//
// RX64M 固有の値（デバイス名やレジスタの中身など）には一切依存せず、
// INSPECT:BEGIN/END マーカーの対応関係と ALL_DONE の有無、既知のエラー
// 文字列のテキストマッチだけで合否を判定する設計にしている。これにより
// マイコンや接続構成が変わっても判定ロジックを変更せずに使い回せる。

using System.Text.RegularExpressions;

// monitor コマンドはターゲットとの通信に失敗しても gdb 自体の終了コードには
// 影響しないことがある（gdb は monitor の結果を単なる文字列出力として扱う
// ため）。したがって終了コードではなくテキストパターンでのエラー検出が必須。
string[] defaultErrorPatterns =
[
    "cannot access memory",
    "not connected",
    "communication error",
    "could not connect",
    "connection timed out",
    "protocol error",
    "remote communication error",
    "target is not connected",
    "junk in cache",
];

var beginRe = new Regex(@"^INSPECT:BEGIN:(\S+)");
var endRe = new Regex(@"^INSPECT:END:(\S+)");
const string AllDoneMarker = "INSPECT:ALL_DONE";

(bool Ok, List<string> Problems, List<string> CompletedSteps) Judge(string text, List<string>? extraErrorPatterns) {
    var problems = new List<string>();
    var completedSteps = new List<string>();

    var errorPatterns = new List<string>(defaultErrorPatterns);
    if (extraErrorPatterns is not null) {
        errorPatterns.AddRange(extraErrorPatterns);
    }
    var compiledErrorPatterns = errorPatterns.Select(p => new Regex(p, RegexOptions.IgnoreCase)).ToList();

    var lines = text.Split('\n');

    // BEGIN/END の対応関係を検査する。各手順は入れ子にならない前提のため、
    // 「現在開いている手順名」を1つだけ保持すればよい。
    string? openStep = null;
    int openStepLine = 0;
    for (int i = 0; i < lines.Length; i++) {
        int lineno = i + 1;
        string line = lines[i];
        var beginMatch = beginRe.Match(line);
        var endMatch = endRe.Match(line);

        if (beginMatch.Success) {
            string name = beginMatch.Groups[1].Value;
            if (openStep is not null) {
                problems.Add(
                    $"{lineno}行目: 手順 '{openStep}' ({openStepLine}行目開始) が " +
                    $"END で閉じられる前に手順 '{name}' が開始されました");
            }
            openStep = name;
            openStepLine = lineno;
        }

        if (endMatch.Success) {
            string name = endMatch.Groups[1].Value;
            if (openStep is null) {
                problems.Add(
                    $"{lineno}行目: 手順 '{name}' の END がありますが、" +
                    "対応する BEGIN が開いていません");
            } else if (openStep != name) {
                problems.Add(
                    $"{lineno}行目: 手順 '{openStep}' ({openStepLine}行目開始) が " +
                    $"閉じられずに手順 '{name}' の END が出現しました");
                openStep = null;
                openStepLine = 0;
            } else {
                completedSteps.Add(name);
                openStep = null;
                openStepLine = 0;
            }
        }
    }

    if (openStep is not null) {
        problems.Add(
            $"手順 '{openStep}' ({openStepLine}行目開始) が最後まで " +
            "END で閉じられませんでした（途中で異常終了/ハングした可能性）");
    }

    // ALL_DONE の存在確認
    if (!lines.Any(line => line.Contains(AllDoneMarker))) {
        problems.Add($"'{AllDoneMarker}' が出力に存在しません（最後まで完走していません）");
    }

    // 既知のエラー文字列の検出（1行に複数パターンが一致しても報告は1回にする）
    for (int i = 0; i < lines.Length; i++) {
        int lineno = i + 1;
        string line = lines[i];
        if (compiledErrorPatterns.Any(p => p.IsMatch(line))) {
            problems.Add($"{lineno}行目: エラー文字列を検出しました: '{line.Trim()}'");
        }
    }

    bool ok = problems.Count == 0;
    return (ok, problems, completedSteps);
}

string? logfile = null;
var extraErrorPatterns = new List<string>();
for (int i = 0; i < args.Length; i++) {
    if (args[i] == "--error-pattern") {
        i++;
        extraErrorPatterns.Add(args[i]);
    } else {
        logfile = args[i];
    }
}

string text = logfile is not null
    ? File.ReadAllText(logfile)
    : Console.In.ReadToEnd();

var (ok, problems, completedSteps) = Judge(text, extraErrorPatterns.Count > 0 ? extraErrorPatterns : null);

Console.WriteLine("=== 完了した手順一覧 ===");
if (completedSteps.Count > 0) {
    foreach (var step in completedSteps) {
        Console.WriteLine($"  - {step}");
    }
} else {
    Console.WriteLine("  (なし)");
}

Console.WriteLine();
Console.WriteLine("=== 判定結果 ===");
if (ok) {
    Console.WriteLine("OK: すべての手順が正常に完了し、エラーは検出されませんでした。");
} else {
    Console.WriteLine("NG: 以下の問題が見つかりました。");
    foreach (var problem in problems) {
        Console.WriteLine($"  - {problem}");
    }
}

return ok ? 0 : 1;