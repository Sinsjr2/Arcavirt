#!/usr/bin/env python3
"""rx64m-inspect.gdbinit の出力ログを判定する。

RX64M 固有の値（デバイス名やレジスタの中身など）には一切依存せず、
INSPECT:BEGIN/END マーカーの対応関係と ALL_DONE の有無、既知のエラー
文字列のテキストマッチだけで合否を判定する設計にしている。これにより
マイコンや接続構成が変わっても判定ロジックを変更せずに使い回せる。
"""

import argparse
import re
import sys

# monitor コマンドはターゲットとの通信に失敗しても gdb 自体の終了コードには
# 影響しないことがある（gdb は monitor の結果を単なる文字列出力として扱う
# ため）。したがって終了コードではなくテキストパターンでのエラー検出が必須。
DEFAULT_ERROR_PATTERNS = [
    "cannot access memory",
    "not connected",
    "communication error",
    "could not connect",
    "connection timed out",
    "protocol error",
    "remote communication error",
    "target is not connected",
    "junk in cache",
]

BEGIN_RE = re.compile(r"^INSPECT:BEGIN:(\S+)")
END_RE = re.compile(r"^INSPECT:END:(\S+)")
ALL_DONE_MARKER = "INSPECT:ALL_DONE"


def judge(text: str, extra_error_patterns=None) -> tuple[bool, list[str], list[str]]:
    """ログ本文を判定する純粋関数。

    戻り値は (合否, 問題点リスト, 完了した手順リスト)。
    """
    problems: list[str] = []
    completed_steps: list[str] = []

    error_patterns = list(DEFAULT_ERROR_PATTERNS)
    if extra_error_patterns:
        error_patterns.extend(extra_error_patterns)
    compiled_error_patterns = [re.compile(p, re.IGNORECASE) for p in error_patterns]

    lines = text.splitlines()

    # BEGIN/END の対応関係を検査する。各手順は入れ子にならない前提のため、
    # 「現在開いている手順名」を1つだけ保持すればよい。
    open_step = None
    open_step_line = None
    for lineno, line in enumerate(lines, start=1):
        begin_match = BEGIN_RE.search(line)
        end_match = END_RE.search(line)

        if begin_match:
            name = begin_match.group(1)
            if open_step is not None:
                problems.append(
                    f"{lineno}行目: 手順 '{open_step}' ({open_step_line}行目開始) が "
                    f"END で閉じられる前に手順 '{name}' が開始されました"
                )
            open_step = name
            open_step_line = lineno

        if end_match:
            name = end_match.group(1)
            if open_step is None:
                problems.append(
                    f"{lineno}行目: 手順 '{name}' の END がありますが、"
                    "対応する BEGIN が開いていません"
                )
            elif open_step != name:
                problems.append(
                    f"{lineno}行目: 手順 '{open_step}' ({open_step_line}行目開始) が "
                    f"閉じられずに手順 '{name}' の END が出現しました"
                )
                open_step = None
                open_step_line = None
            else:
                completed_steps.append(name)
                open_step = None
                open_step_line = None

    if open_step is not None:
        problems.append(
            f"手順 '{open_step}' ({open_step_line}行目開始) が最後まで "
            "END で閉じられませんでした（途中で異常終了/ハングした可能性）"
        )

    # ALL_DONE の存在確認
    if not any(ALL_DONE_MARKER in line for line in lines):
        problems.append(
            f"'{ALL_DONE_MARKER}' が出力に存在しません（最後まで完走していません）"
        )

    # 既知のエラー文字列の検出（1行に複数パターンが一致しても報告は1回にする）
    for lineno, line in enumerate(lines, start=1):
        if any(pattern.search(line) for pattern in compiled_error_patterns):
            problems.append(f"{lineno}行目: エラー文字列を検出しました: {line.strip()!r}")

    ok = len(problems) == 0
    return ok, problems, completed_steps


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="rx64m-inspect.gdbinit の出力ログを判定する（マイコン非依存）。",
    )
    parser.add_argument(
        "logfile",
        nargs="?",
        help="判定対象のログファイルパス（省略時は標準入力から読む）",
    )
    parser.add_argument(
        "--error-pattern",
        action="append",
        dest="error_patterns",
        default=None,
        help="デフォルトのエラーパターンに追加する正規表現（複数指定可）",
    )
    return parser


def main() -> int:
    parser = build_arg_parser()
    args = parser.parse_args()

    if args.logfile:
        with open(args.logfile, "r", encoding="utf-8", errors="replace") as f:
            text = f.read()
    else:
        text = sys.stdin.read()

    ok, problems, completed_steps = judge(text, extra_error_patterns=args.error_patterns)

    print("=== 完了した手順一覧 ===")
    if completed_steps:
        for step in completed_steps:
            print(f"  - {step}")
    else:
        print("  (なし)")

    print()
    print("=== 判定結果 ===")
    if ok:
        print("OK: すべての手順が正常に完了し、エラーは検出されませんでした。")
    else:
        print("NG: 以下の問題が見つかりました。")
        for problem in problems:
            print(f"  - {problem}")

    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
