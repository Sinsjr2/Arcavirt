using System.Runtime.CompilerServices;

namespace IsrEmulation.CommandInterpreter;

public interface ICommandData {}

/// <summary>
/// 与えられたコマンドを並列して実行します。
/// </summary>
public record ParallelCommand(IReadOnlyList<ICommandData> Commands) : ICommandData;

/// <summary>
/// 一連のコマンドを順番に実行するプロセスを表します。
/// 名前は、デバッグ時に使用することやプロセス名で特定のプロセスを呼び出す時に使用することを想定しています。
/// </summary>
public record CommandProcess(string ProcessName, IReadOnlyList<ICommandData> Commands);

public readonly record struct CommandInfo(ushort ProcessID, ushort Priority) : IComparable<CommandInfo> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(CommandInfo other) {
        if (ProcessID == other.ProcessID) {
            return Priority - other.Priority;
        }
        return ProcessID - other.ProcessID;
    }
}

public interface IExecutableCommandData : ICommandData;

/// <summary>
/// コマンドの実行順を表し、実行するコマンドのデータのペア
/// </summary>
public record CommandPair(CommandInfo Info, IExecutableCommandData Data);

public static class CollectionUtil {
    public static int BinarySearch<T, TComparable>(
        IReadOnlyList<T> source, TComparable comparable) where TComparable : IComparable<T> {
        int lo = 0;
        int hi = source.Count - 1;
        // If length == 0, hi == -1, and loop will not be entered
        while (lo <= hi) {
            // PERF: `lo` or `hi` will never be negative inside the loop,
            //       so computing median using uints is safe since we know
            //       `length <= int.MaxValue`, and indices are >= 0
            //       and thus cannot overflow an uint.
            //       Saves one subtraction per loop compared to
            //       `int i = lo + ((hi - lo) >> 1);`
            int i = (int)(((uint)hi + (uint)lo) >> 1);

            int c = comparable.CompareTo(source[i]);
            if (c == 0) {
                return i;
            }
            else if (c > 0) {
                lo = i + 1;
            }
            else {
                hi = i - 1;
            }
        }
        // If none found, then a negative number that is the bitwise complement
        // of the index of the next element that is larger than or, if there is
        // no larger element, the bitwise complement of `length`, which
        // is `lo` at this point.
        return ~lo;
    }

    readonly struct LowerBoundComparable<T, TComparable>(TComparable value) : IComparable<T> where TComparable : IComparable<T> {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(T? other) => 0 <= value.CompareTo(other) ? 1 : -1;
    }

    public static int LowerBound<T, TComparable>(IReadOnlyList<T> source, TComparable comparable)
        where TComparable : IComparable<T> {
        var index = BinarySearch(source, new LowerBoundComparable<T, TComparable>(comparable));
        if (index < 0) {
            // 補数を戻して返却
            return Math.Abs(index + 1);
        }
        return index;

    }

    readonly struct UpperBoundComparable<T, TComparable>(TComparable value) : IComparable<T>
        where TComparable : IComparable<T> {
        public int CompareTo(T? other) => 0 < value.CompareTo(other) ? 1 : -1;
    }

    public static int UpperBound<T, TComparable>(IReadOnlyList<T> source, TComparable comparable)
        where TComparable : IComparable<T> {
        var index = BinarySearch(source, new UpperBoundComparable<T, TComparable>(comparable));
        if (index < 0) {
            // 補数を戻して返却
            return Math.Abs(index + 1);
        }
        return index;
    }
}

public class CommandInterpreter {
    readonly Action<object, Func<object, ValueTask>> runOnLoop;

    readonly List<CommandPair> commands = new();

    readonly struct CommandPairComparable(CommandInfo info) : IComparable<CommandPair> {
        public int CompareTo(CommandPair? other) => other != null ? info.CompareTo(other.Info) : 1;
    }

    /// <summary>
    /// 実行が完了しても削除されないコマンドとして登録します。
    /// </summary>
    public void AddAsPersistentCommands(IReadOnlyList<CommandPair> commands) {
        this.commands.AddRange(commands);
        this.commands.Sort(static (a, b) => a.Info.CompareTo(b.Info));
    }

    public void ClearCommands() {
        commands.Clear();
    }

    public void RunProcess(ushort processID) {
        var index = CollectionUtil.LowerBound(commands, new CommandPairComparable(new CommandInfo(processID, 0)));
    }
}