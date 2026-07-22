using System.Buffers;

namespace Clock;

//TODO ファイルを分割する

/// <summary>
/// ユークリッドの数学関数
/// </summary>
public static class EuclidMath {

    /// <summary>
    /// 最大公約数(Least Common Multiple)を求めます。
    /// </summary>
    public static ulong LCM(ulong a, ulong b) {
        return a * b / GCD(a, b);
    }

    /// <summary>
    /// 最大公約数(Greatest Common Divisor)を求めます。
    /// </summary>
    public static ulong GCD(ulong a, ulong b) {
        if (a < b) {
            var tmp = a;
            a = b;
            b = tmp;
        }
        while (b != 0) {
            var remainder = a % b;
            a = b;
            b = remainder;
        }
        return a;
    }
}

/// <summary>
/// 分数を表します。
/// </summary>
public record struct Fraction(
    ulong Numer,
    ulong Denom
) {
    public double Value => (double)Numer / Denom;

    public static Fraction operator *(Fraction a, Fraction b) {
        return ReduceFraction(new Fraction(a.Numer * b.Numer, a.Denom * b.Denom));
    }

    public static Fraction operator *(Fraction a, ulong b) {
        return ReduceFraction(new Fraction(a.Numer * b, a.Denom));
    }

    public static Fraction operator *(ulong a, Fraction b) {
        return ReduceFraction(new Fraction(a * b.Numer, b.Denom));
    }

    public static Fraction operator /(Fraction a, Fraction b) {
        return ReduceFraction(new Fraction(a.Numer * b.Denom, a.Denom * b.Numer));
    }

    public static Fraction operator /(Fraction a, ulong b) {
        return ReduceFraction(new Fraction(a.Numer, a.Denom * b));
    }

    public static Fraction operator /(ulong a, Fraction b) {
        return ReduceFraction(new Fraction(a * b.Denom, b.Numer));
    }

    /// <summary>
    /// 約分します。
    /// 約分できない場合はそのまま値を返します。
    /// </summary>
    static Fraction ReduceFraction(Fraction x) {
        var gcd = EuclidMath.GCD(x.Numer, x.Denom);
        if (1 < gcd) {
            return new Fraction(x.Numer / gcd, x.Denom);
        }
        return x;
    }
}

/// <summary>
/// クロックの周波数が変更されたことを通知します。
/// クロックは循環参照することは想定していません。
/// 循環した場合は、クロックの通知をすると無限ループに陥る可能性があります。
/// </summary>
public interface IClockFrequency {
    Fraction Frequency { get; }
    /// <summary>
    /// 変化する前の値と変化した後の値を渡します。
    /// このコールバックが呼ばれるときには、Frequencyは新しい値に更新済みつまり、第2引数と同じ値になっています。
    /// </summary>
    event Action<Fraction, Fraction>? OnChangedFrequency;
}

/// <summary>
/// クロックの発信源です。
/// 周波数を設定することができます。
/// </summary>
public class MainClock : IClockFrequency {
    public string Name { get;  }
    Fraction frequency;
    public Fraction Frequency {
        get => frequency;
        set {
            var prev = frequency;
            frequency = value;
            if (prev != value) {
                OnChangedFrequency?.Invoke(prev, value);
            }
        }
    }

    public event Action<Fraction, Fraction>? OnChangedFrequency;

    public MainClock(string name, Fraction initialFreq) {
        frequency = initialFreq;
        Name = name;
    }
}

/// <summary>
/// 分周や逓倍することができます。
/// ソースのインスタンスが回収された場合は、同じく回収されます(Dispose 必要なし)。
/// ソースのインスタンスを残したまま、このインスタンスを大量に作るとコールバックが解除されません。
/// Disposeはこのコールバックの解除のために使用します。
/// </summary>
public class SubClock : IClockFrequency, IDisposable {
    public string Name { get; }
    readonly IClockFrequency source;
    public Fraction Frequency { get; private set; }
    public event Action<Fraction, Fraction>? OnChangedFrequency;

    Fraction mulDiv;

    /// <summary>
    /// 分周や逓倍する比率を変更します。
    /// </summary>
    public Fraction MulDiv {
        set {
            var prevFreq = CalcCurrentFrequency(Frequency, mulDiv);
            mulDiv = value;
            var newFreq = CalcCurrentFrequency(Frequency, value);
            NotifyFrequencyIfChanged(prevFreq, newFreq);
        }
        get => mulDiv;
    }

    static Fraction CalcCurrentFrequency(Fraction freq, Fraction mulDiv) {
        return freq * mulDiv;
    }

    public SubClock(string name,
                    IClockFrequency source,
                    Fraction initialDivMul) {
        this.source = source;
        source.OnChangedFrequency += OnChangedSourceFrequency;
        MulDiv = initialDivMul;
        Name = name;
    }

    void OnChangedSourceFrequency(Fraction prevFreq, Fraction newFreq) {
        var prevF = CalcCurrentFrequency(prevFreq, mulDiv);
        var newF = CalcCurrentFrequency(newFreq, mulDiv);
        NotifyFrequencyIfChanged(prevF, newF);
    }

    void NotifyFrequencyIfChanged(Fraction prevFreq, Fraction newFreq) {
        Frequency = newFreq;
        if (prevFreq != newFreq) {
            OnChangedFrequency?.Invoke(prevFreq, newFreq);
        }
    }

    public void Dispose() {
        source.OnChangedFrequency -= OnChangedSourceFrequency;
    }
}

/// <summary>
/// クロックのソースを変更します。
/// ソースのインスタンスが回収された場合は、同じく回収されます(Dispose 必要なし)。
/// ソースのインスタンスを残したまま、このインスタンスを大量に作るとコールバックが解除されません。
/// Disposeはこのコールバックの解除のために使用します。
/// </summary>
public class ClockSelector : IClockFrequency, IDisposable {
    class Inner {
        public readonly int No;
        public readonly IClockFrequency Source;
        readonly ClockSelector parent;

        public Inner(ClockSelector parent, int no, IClockFrequency source) {
            this.parent = parent;
            No = no;
            Source = source;
        }

        public void OnChangedSourceFrequency(Fraction prevFreq, Fraction newFreq) {
            if (parent.currentSource != No)
            {
                return;
            }
            parent.NotifyIfChanged(parent.Frequency, newFreq);
        }
    }

    public string Name { get; }

    readonly IReadOnlyList<Inner> inners;

    public Fraction Frequency { get; private set; }

    public event Action<Fraction, Fraction>? OnChangedFrequency;

    /// <summary>
    /// 変更できるインデックスの範囲を取得するために使用します。
    /// </summary>
    public int SourceCount => inners.Count;

    int currentSource;

    /// <summary>
    /// ソースの配列は、初期の周波数を決定するために1以上である必要があります。
    /// 1未満の場合は例外が発生します。
    /// </summary>
    public ClockSelector(string name,
                         IReadOnlyList<IClockFrequency> sources,
                         int sourceNo) {
        if (sources.Count < 1) {
            throw new IndexOutOfRangeException($"length: {sources.Count} actual: {sourceNo}");
        }
        currentSource = sourceNo;
        Name = name;
        inners = sources.Select((src, i) => {
            var inner = new Inner(this, i, src);
            src.OnChangedFrequency += inner.OnChangedSourceFrequency;
            return inner;
        }).ToArray();
        ChangeSource(sourceNo);
    }

    /// <summary>
    /// コンストラクタに渡した配列の番号で
    /// クロックの入力元を変更します。
    /// </summary>
    public void ChangeSource(int no) {
        if (!(0 <= no && no < inners.Count)) {
            throw new IndexOutOfRangeException($"length: {inners.Count} actual: {no}");
        }
        var prevFreq = Frequency;
        var newFreq = inners[no].Source.Frequency;
        currentSource = no;
        NotifyIfChanged(prevFreq, newFreq);
    }

    void OnChangedSourceFrequency(Fraction prevFreq, Fraction newFreq) {
        NotifyIfChanged(Frequency, newFreq);
    }

    void NotifyIfChanged(Fraction prevFreq, Fraction newFreq) {
        Frequency = newFreq;
        if (prevFreq != newFreq) {
            OnChangedFrequency?.Invoke(prevFreq, newFreq);
        }
    }

    public void Dispose() {
        foreach (var inner in inners) {
            inner.Source.OnChangedFrequency -= inner.OnChangedSourceFrequency;
        }
    }
}

/// <summary>
/// タイマーのカウントが指定された値と一致(いわゆるコンペアマッチ)したときのカウンターの動作を決定します。
/// 
/// </summary>
public enum TimerTriggerKind {
    /// <summary>
    /// 一致しても何もしない
    /// </summary>
    None,
    /// <summary>
    /// 一致した場合、タイマーのカウントを0でリセットします。
    /// </summary>
    ResetZero,
}

/// <summary>
/// タイマーのカウント値が上限に達したときの動作を決定します。
/// </summary>
public enum TimerUpperLimitTriggerKind {
    /// <summary>
    /// カウンターが上限と一致するとカウンターを0にします。
    /// </summary>
    ResetZero,
    /// <summary>
    /// カウンターが上限と一致するとタイマーを停止します。
    /// カウンターはそのままです。
    /// </summary>
    Stop
}

/// <summary>
/// </summary>
public record struct CountTimerTrigger (
    TimerTriggerKind Trigger,
    ulong TriggerCount
);

/// <param name="OnMatchCount">
///  カウントが一致した時にコールバックを実行します。
/// </param>
public record struct TickTimerTriggerAction(
    CountTimerTrigger Trigger,
    Action OnMatchCount
);

/*
必要な機能
- 周波数を変更した時に予め設定した時間をその変更した周波数に応じて時間を修正する
- 新しい時間はクロック数で設定する
- タイマーをストップすることができる
- 経過したクロック数を取得できること
- クロックカウントを保持した状態でタイマーを停止し、スタートさせることができる
- タイマーが動作(カウントアップ動作)しているかがわかること
  周期動作の場合はenableかどうかによって反映される
  ワンショット動作の場合は、指定したカウント値に到達した場合にfalseになる
  ワンショット動作の場合は、enableをfalseにしてからtrueにすると動作を開始する。
  ワンショット動作が完了した後、enableを false true した場合でも、完了のコールバックが実行される。
  このとき、コールバックが実行されるまでの間はisRunningはtrueになる。
- 経過したクロック数①を個別で設定できる
  ただし、コールバックを呼び出すときのカウント値②よりも大きな値は設定できない。設定すると②の値内に丸め込む。
  つまり、②①の順番で設定する必要がある。
 */

//TODO クロックが供給されていない場合の動作にも対応する必要がある。
public class CountTimer {
    /// <summary>
    /// このタイマーの進める速度を決定する周波数
    /// </summary>
    readonly IClockFrequency clockFreq;
    readonly IClock clock;
    readonly IAlarm alarm;

    /// <summary>
    /// 「タイマーをスタート」・「カウントを変更」してから周波数が変わるまで加算されたクロック数
    /// </summary>
    ulong elapsedClock;

    /// <summary>
    /// 周波数が変化したときやタイマの時間を変更した時に更新します。
    /// クロック数と時間の変換のために使用します。
    /// この時間の初めから周波数が変化していないこと前提で上記を相互に変換します。
    /// </summary>
    double freqBeginTimeNS = 0;

    readonly TickTimerTriggerAction[] triggers;

    public int TriggersLength => triggers.Length;

    bool enable = false;

    /// <summary>
    /// 上限のカウントに達したことを通知するときの識別子
    /// </summary>
    static readonly int upperLimitTriggerNo = -1;

    /// <summary>
    /// カウンターが一致した時に実行する<see name="triggers"/>のインデックスです。
    /// 同じカウント値の場合は、複数登録されます。
    /// 0未満は、予約値です。
    /// </summary>
    readonly List<int> notifies = new();

    /// <summary>
    /// タイマーのカウントをスタートしたり、停止したりします。
    /// </summary>
    public bool Enable {
        get => enable;
        set {
            bool isChanged = enable != value;
            enable = value;
            if (isChanged) {
                if (value) {
                    var currentTime = clock.Nanos;
                    freqBeginTimeNS = currentTime;
                    ApplyNextTime();
                }
                else {
                    var currentTime = clock.Nanos;
                    freqBeginTimeNS = currentTime;
                    elapsedClock = CalcCurrentElapsedClock(UpperLimit, elapsedClock, currentTime, freqBeginTimeNS, clockFreq.Frequency);
                    alarm.Cancel();
                }
            }
        }
    }

    public TimerUpperLimitTriggerKind UpperLimitTrigger { get; set; }

    /// <summary>
    /// 上限にカウンターが一致したときのコールバック
    /// </summary>
    readonly Action? onMatchUpperLimit;

    /// <summary>
    /// 時間カウントできる上限の値
    /// この値を超えて値を設定することは出来ません。
    /// </summary>
    public ulong UpperLimit { get; }

    /// <summary>
    /// 呼び出された時刻のタイマーをスタートしてからの経過したクロック数
    /// 常にカウントアップします。
    /// <see name="UpperLimit"/>よりも大きな値を設定すると例外が発生します。
    /// </summary>
    public ulong Count {
        get => enable
        ? CalcCurrentElapsedClock(UpperLimit, elapsedClock, clock.Nanos, freqBeginTimeNS, clockFreq.Frequency)
        : elapsedClock;
        set {
            if (UpperLimit < value) {
                // 上限時間よりも大きくなった場合、通知時間の計算ができなくなるのでガードする
                throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(UpperLimit)}: {UpperLimit}, actual: {value}");
            }
            if (!enable) {
                elapsedClock = value;
            }
            else {
                // 動作中であったので、通知する時刻を変更する
                var currentTimeNS = clock.Nanos;
                var nowCount = CalcCurrentElapsedClock(UpperLimit, elapsedClock, currentTimeNS, freqBeginTimeNS, clockFreq.Frequency);
                if (nowCount == value) {
                    // タイマーの更新処理負荷軽減のため
                    return;
                }
                elapsedClock = value;
                freqBeginTimeNS = currentTimeNS;
                ApplyNextTime();
            }
        }
    }

    public CountTimer(IClockFrequency clockFrequency, IClock clock,
                     IEnumerable<TickTimerTriggerAction> triggers,
                     TimerUpperLimitTriggerKind upperLimitTrigger,
                     ulong upperLimit,
                     Action? onMatchUpperLimit) {
        clockFreq = clockFrequency;
        clockFreq.OnChangedFrequency += OnChangeFrequency;
        this.clock = clock;
        alarm = clock.CreateAlarm(OnElapsedTime);
        UpperLimitTrigger = upperLimitTrigger;
        foreach (var trigger in triggers) {
            if (UpperLimit < trigger.Trigger.TriggerCount) {
                throw new ArgumentOutOfRangeException(
                    nameof(triggers),
                    $"{nameof(upperLimit)}: {upperLimit}, actual: {trigger.Trigger.TriggerCount}");
            }
        }
        this.triggers = triggers.ToArray();
        UpperLimit = upperLimit;
        this.onMatchUpperLimit = onMatchUpperLimit;
    }

    public CountTimerTrigger GetTrigger(int no) {
        return triggers[no].Trigger;
    }

    /// <summary>
    /// 指定されたトリガーの通知タイミングやカウントが一致したときの処理を変更します。
    /// 通知するタイミングは、<see name="UpperLimit"/>よりも大きいと例外が発生します。
    /// </summary>
    public void SetTrigger(int no, CountTimerTrigger newTrigger) {
        if (UpperLimit < newTrigger.TriggerCount) {
            throw new ArgumentOutOfRangeException(
                nameof(newTrigger),
                $"{nameof(UpperLimit)}: {UpperLimit}, actual: {newTrigger.TriggerCount}");
        }
        var triggerAct = triggers[no];
        var isChangedCount = triggerAct.Trigger.TriggerCount != newTrigger.TriggerCount;
        triggers[no] = triggerAct with { Trigger = newTrigger };
        if (isChangedCount) {
            // 時間が変化していないので呼び出す時刻を変更する必要なし
            // 処理負荷軽減のため
            ApplyNextTime();
        }
    }

    /// <summary>
    /// 時間経過を通知するためにタイマーの時間を変更します。
    /// 現在の時間からの差分クロックを指定すると現在の周波数に応じて時間を計算し適用します。
    /// </summary>
    void Schedule(ulong newDeltaClock) {
        // 単位を秒からnsecに変換する
        var nextAlarmTimeNS = Math.Max(
            0,
            ((newDeltaClock / clockFreq.Frequency) * 1_000_000_000).Value);
        alarm.Schedule(nextAlarmTimeNS);
    }

    /// <summary>
    /// 次に呼ばれる様にタイマーを更新します。
    /// タイマーが停止していた場合は何もしません。
    /// </summary>
    void ApplyNextTime() {
        if (!Enable) {
            return;
        }
        var currentCount = this.Count;
        var upperLimit = UpperLimit;

        // 次に一致する最小のカウントを探索する
        ulong nextMatchTriggerCount = ulong.MaxValue;
        foreach (var trigger in triggers) {
            var triggerCount = trigger.Trigger.TriggerCount;
            if (currentCount <= triggerCount && triggerCount <= upperLimit &&
                triggerCount < nextMatchTriggerCount) {
                nextMatchTriggerCount = triggerCount;
            }
        }
        ulong nextMatchCount = Math.Min(upperLimit, nextMatchTriggerCount);
        // 次に通知する時間とコールバックを抽出する
        for (int i = 0; i < triggers.Length; i++) {
            if (nextMatchCount == triggers[i].Trigger.TriggerCount) {
                notifies.Add(i);
            }
        }
        if (nextMatchCount == upperLimit) {
            notifies.Add(upperLimitTriggerNo);
        }
        Schedule(nextMatchCount - currentCount);
    }

    void OnElapsedTime() {
        bool hasUpperLimitTrigger = false;
        ArrayPool<int>? pool = null;
        int[]? buf = null;
        try {
            pool = ArrayPool<int>.Shared;
            buf = pool.Rent(notifies.Count);
            notifies.CopyTo(buf);
            int notifyCount = notifies.Count;
            notifies.Clear();
            foreach (var no in buf.AsSpan(0, notifyCount)) {
                if (no < 0) {
                    if (no == upperLimitTriggerNo) {
                        hasUpperLimitTrigger = true;
                    }
                }
                switch (triggers[no].Trigger.Trigger) {
                    case TimerTriggerKind.None:
                        // 何もしない設定である
                        break;
                    case TimerTriggerKind.ResetZero:
                        Count = 0;
                        break;
                }
            }
            ApplyNextTime();
            // ↑トリガーのコールバック内で時間の変更をする可能性があるので、事前にトリガーの設定を反映させておく

            foreach (var no in buf.AsSpan(0, notifyCount)) {
                if (no < 0) {
                    continue;
                }
                triggers[no].OnMatchCount();
            }

            if (hasUpperLimitTrigger) {
                OnMatchUpperLimit();
            }
        }
        finally {
            if (pool != null && buf != null) {
                pool.Return(buf);
            }
        }
    }

    void OnMatchUpperLimit() {
        switch (UpperLimitTrigger) {
            case TimerUpperLimitTriggerKind.ResetZero:
                Count = 0;
                break;
            case TimerUpperLimitTriggerKind.Stop:
                Enable = false;
                break;
            default:
                throw new NotSupportedException($"not supperted value: {UpperLimitTrigger}");
        }
        onMatchUpperLimit?.Invoke();
    }

    void OnChangeFrequency(Fraction prevFreq, Fraction newFreq) {
        if (!enable) {
            return;
        }

        var currentTimeNS = clock.Nanos;
        elapsedClock = CalcCurrentElapsedClock(UpperLimit, elapsedClock, currentTimeNS, freqBeginTimeNS, prevFreq);
        freqBeginTimeNS = currentTimeNS;
        ApplyNextTime();
    }

    /// <summary>
    /// 現在のクロックの周波数を考慮して今までに経過したクロック数を算出します。
    /// </summary>
    static ulong CalcCurrentElapsedClock(
        ulong alarmClockCount,
        ulong elapsedClock, double currentTimeNS, double frequencyBeginTimeNS, Fraction freqency) {
        // 周波数が同じ区間のクロック数を求める
        var deltaTimeNS = currentTimeNS - frequencyBeginTimeNS;
        // 秒からnsecに変換してクロック数を求める
        var deltaClock = ((ulong)deltaTimeNS / freqency) / 1_000_000_000;
        // 指定したカウント数以上にならないように制限を設ける
        return Math.Min(
            alarmClockCount,
            elapsedClock + (ulong)deltaClock.Value);
    }
}
