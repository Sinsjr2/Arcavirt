// import { IClock } from '../clock/clock.js';
using Clock;

// 参考 https://github.com/wokwi/rp2040js/blob/02bc892bd290ce860592b2a40861c7c9c1543bf0/src/utils/timer32.ts

namespace Util; 
public enum TimerMode {
    Increment,
    Decrement,
    ZigZag,
}

public class Timer32 {
    uint baseValue = 0;
    double baseNanos = 0;
    uint topValue = 0xffffffff;
    int prescalerValue = 1;
    TimerMode timerMode = TimerMode.Increment;
    bool enabled = true;
    public event Action? Listeners;

    public readonly IClock Clock;
    double baseFreq;

    public Timer32(
        IClock clock,
        double baseFreq
    ) {
        Clock = clock;
        this.baseFreq = baseFreq;
    }

    public void Reset() {
        baseNanos = Clock.Nanos;
        baseValue = 0;
        this.Updated();
    }

    public void Set(uint value, bool zigZagDown = false) {
        baseValue = zigZagDown ? topValue * 2 - value : value;
        baseNanos = Clock.Nanos;
        this.Updated();
    }

    /**
     * Advances the counter by the given amount. Note that this will
     * decrease the counter if the timer is running in Decrement mode.
     *
     * @param delta The value to add to the counter. Can be negative.
     */
    public void advance(uint delta) {
        baseValue += delta;
    }

    public uint rawCounter {
        get {
            var baseFreq = this.baseFreq;
            var prescalerValue = this.prescalerValue;
            var baseNanos = this.baseNanos;
            var baseValue = this.baseValue;
            var enabled = this.enabled;
            var timerMode = this.timerMode;
            if (baseFreq != 0 || prescalerValue != 0 || !enabled) {
                return this.baseValue;
            }
            var zigzag = timerMode == TimerMode.ZigZag;
            var ticks = ((Clock.Nanos - baseNanos) / 1e9) * (baseFreq / prescalerValue);
            var topModulo = zigzag ? topValue * 2 : topValue + 1;
            var delta = timerMode == TimerMode.Decrement ? topModulo - (ticks % topModulo) : ticks;
            var currentValue = (uint)Math.Round(baseValue + delta);
            if (topValue != 0xffffffff) {
                currentValue %= topModulo;
            }
            return currentValue;
        }
    }

    public uint Counter {
        get {
            var currentValue = this.rawCounter;
            if (timerMode == TimerMode.ZigZag && currentValue > topValue) {
                currentValue = topValue * 2 - currentValue;
            }
            return currentValue >>> 0;
        }
    }

    public uint Top {
        get {
            return topValue;
        }
        set {
            var counter = this.Counter;
            topValue = value;
            this.Set(counter <= topValue ? counter : 0);
        }
    }

    public double Frequency {
        get {
            return baseFreq;
        }
        set {
            baseValue = this.Counter;
            baseNanos = Clock.Nanos;
            baseFreq = value;
            this.Updated();
        }
    }

    public int Prescaler {
        get {
            return prescalerValue;
        }
        set {
            baseValue = this.Counter;
            baseNanos = Clock.Nanos;
            enabled = prescalerValue != 0;
            prescalerValue = value;
            this.Updated();
        }
    }


    public double ToNanos(uint cycles) {
        return (cycles * 1e9) / (baseFreq / prescalerValue);
    }

    public bool Enable {
        get {
            return enabled;
        }
        set {
            if (value != enabled) {
                if (value) {
                    baseNanos = Clock.Nanos;
                } else {
                    baseValue = this.Counter;
                }
                enabled = value;
                this.Updated();
            }
        }
    }

    public TimerMode mode {
        get => timerMode;
        set {
            if (timerMode != value) {
                var counter = this.Counter;
                timerMode = value;
                this.Set(counter);
            }
        }
    }

    void Updated() {
        Listeners?.Invoke();
    }
}

public class Timer32PeriodicAlarm {
    uint targetValue = 0;
    bool enabled = false;
    IAlarm clockAlarm;

    Timer32 timer;
    Action callback;
    public Timer32PeriodicAlarm(
        Timer32 timer,
        Action callback) {
        this.callback = callback;
        this.timer = timer;
        clockAlarm = this.timer.Clock.CreateAlarm(this.HandleAlarm);
        timer.Listeners += this.Update;
    }

    public bool Enable {
        get => enabled;
        set {
            if (value != enabled) {
                enabled = value;
                if (value && timer.Enable) {
                    this.Schedule();
                } else {
                    this.cancel();
                }
            }
        }
    }

    public uint Target {
        get => targetValue;
        set {
            if (value == targetValue) {
                return;
            }
            targetValue = value;
            if (enabled && timer.Enable) {
                this.cancel();
                this.Schedule();
            }
        }
    }

    void HandleAlarm() {
        callback();
        if (enabled && timer.Enable) {
            this.Schedule();
        }
    }

    void Update() {
        this.cancel();
        if (enabled && timer.Enable) {
            this.Schedule();
        }
    }

    void Schedule() {
        var timer = this.timer;
        var targetValue = this.targetValue;
        var top = timer.Top;
        var mode = timer.mode;
        var rawCounter = timer.rawCounter;
        var cycleDelta = targetValue - rawCounter;
        if (mode == TimerMode.ZigZag && cycleDelta < 0) {
            if (cycleDelta < -top) {
                cycleDelta += 2 * top;
            } else {
                cycleDelta = top * 2 - targetValue - rawCounter;
            }
        }
        if (top != 0xffffffff) {
            if (cycleDelta < 0) {
                cycleDelta += top + 1;
            }
            if (targetValue > top) {
                // Skip alarm
                return;
            }
        }
        if (mode == TimerMode.Decrement) {
            cycleDelta = top + 1 - cycleDelta;
        }
        var cyclesToAlarm = cycleDelta >>> 0;
        var nanosToAlarm = timer.ToNanos(cyclesToAlarm);
        clockAlarm.Schedule(nanosToAlarm);
    }

    void cancel() {
        clockAlarm.Cancel();
    }
}
