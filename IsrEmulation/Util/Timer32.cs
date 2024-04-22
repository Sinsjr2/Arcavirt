// import { IClock } from '../clock/clock.js';
using Clock;

// 参考 https://github.com/wokwi/rp2040js/blob/02bc892bd290ce860592b2a40861c7c9c1543bf0/src/utils/timer32.ts

namespace Util {
    public enum TimerMode {
        Increment,
        Decrement,
        ZigZag,
    }

    public class Timer32 {
        private uint baseValue = 0;
        private double baseNanos = 0;
        private uint topValue = 0xffffffff;
        private int prescalerValue = 1;
        private TimerMode timerMode = TimerMode.Increment;
        private bool enabled = true;
        public event Action? Listeners;

        public readonly IClock Clock;
        double baseFreq;

        public Timer32(
            IClock clock,
            double baseFreq
        ) {
            this.Clock = clock;
            this.baseFreq = baseFreq;
        }

        public void Reset() {
            this.baseNanos = this.Clock.Nanos;
            this.baseValue = 0;
            this.Updated();
        }

        public void Set(uint value, bool zigZagDown = false) {
            this.baseValue = zigZagDown ? this.topValue * 2 - value : value;
            this.baseNanos = this.Clock.Nanos;
            this.Updated();
        }

        /**
         * Advances the counter by the given amount. Note that this will
         * decrease the counter if the timer is running in Decrement mode.
         *
         * @param delta The value to add to the counter. Can be negative.
         */
        public void advance(uint delta) {
            this.baseValue += delta;
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
                var ticks = ((this.Clock.Nanos - baseNanos) / 1e9) * (baseFreq / prescalerValue);
                var topModulo = zigzag ? this.topValue * 2 : this.topValue + 1;
                var delta = timerMode == TimerMode.Decrement ? topModulo - (ticks % topModulo) : ticks;
                var currentValue = (uint)Math.Round(baseValue + delta);
                if (this.topValue != 0xffffffff) {
                    currentValue %= topModulo;
                }
                return currentValue;
            }
        }

        public uint Counter {
            get {
                var currentValue = this.rawCounter;
                if (this.timerMode == TimerMode.ZigZag && currentValue > this.topValue) {
                    currentValue = this.topValue * 2 - currentValue;
                }
                return currentValue >>> 0;
            }
        }

        public uint Top {
            get {
                return this.topValue;
            }
            set {
                var counter = this.Counter;
                this.topValue = value;
                this.Set(counter <= this.topValue ? counter : 0);
            }
        }

        public double Frequency {
            get {
                return this.baseFreq;
            }
            set {
                this.baseValue = this.Counter;
                this.baseNanos = this.Clock.Nanos;
                this.baseFreq = value;
                this.Updated();
            }
        }

        public int Prescaler {
            get {
                return this.prescalerValue;
            }
            set {
                this.baseValue = this.Counter;
                this.baseNanos = this.Clock.Nanos;
                this.enabled = this.prescalerValue != 0;
                this.prescalerValue = value;
                this.Updated();
            }
        }


        public double ToNanos(uint cycles) {
            return (cycles * 1e9) / (baseFreq / prescalerValue);
        }

        public bool Enable {
            get {
                return this.enabled;
            }
            set {
                if (value != this.enabled) {
                    if (value) {
                        this.baseNanos = this.Clock.Nanos;
                    } else {
                        this.baseValue = this.Counter;
                    }
                    this.enabled = value;
                    this.Updated();
                }
            }
        }

        public TimerMode mode {
            get => this.timerMode;
            set {
                if (this.timerMode != value) {
                    var counter = this.Counter;
                    this.timerMode = value;
                    this.Set(counter);
                }
            }
        }

        void Updated() {
            Listeners?.Invoke();
        }
    }

    public class Timer32PeriodicAlarm {
        private uint targetValue = 0;
        private bool enabled = false;
        private IAlarm clockAlarm;

        Timer32 timer;
        Action callback;
        public Timer32PeriodicAlarm(
            Timer32 timer,
            Action callback) {
            this.callback = callback;
            this.timer = timer;
            this.clockAlarm = this.timer.Clock.CreateAlarm(this.HandleAlarm);
            timer.Listeners += this.Update;
        }

        public bool Enable {
            get => this.enabled;
            set {
                if (value != this.enabled) {
                    this.enabled = value;
                    if (value && this.timer.Enable) {
                        this.Schedule();
                    } else {
                        this.cancel();
                    }
                }
            }
        }

        public uint Target {
            get => this.targetValue;
            set {
                if (value == this.targetValue) {
                    return;
                }
                this.targetValue = value;
                if (this.enabled && this.timer.Enable) {
                    this.cancel();
                    this.Schedule();
                }
            }
        }

        void HandleAlarm() {
            this.callback();
            if (this.enabled && this.timer.Enable) {
                this.Schedule();
            }
        }

        void Update() {
            this.cancel();
            if (this.enabled && this.timer.Enable) {
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
            this.clockAlarm.Schedule(nanosToAlarm);
        }

        void cancel() {
            this.clockAlarm.Cancel();
        }
    }
}
