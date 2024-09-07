using Clock;
using GPIO;
using CPU;
using Util;

namespace Pheripheral {
    [Flags]
    public enum PWMControlRegister {
        /// <summary>
        /// 1 タイマーがスタートします。
        /// 0 タイマーが停止します。
        /// </summary>
        Enable = 1 << 0,

        /// <summary>
        /// 1 割り込みが発生するようになります。
        /// </summary>
        InterruptEnable = 1 << 1,

        /// <summary>
        /// 1 コンペアマッチ時の出力を反転させます。
        /// </summary>
        InvertOutput = 1 << 2,
    }

    public enum PWMRegister {
        Control,

        /// <summary>
        /// 現在のタイマのカウント値
        /// </summary>
        TimerCount,

        /// <summary>
        /// この値以下であれば、出力は0
        /// それ以外は1
        /// InvertOutput が1のときは反転します。
        /// </summary>
        CompareMatch,

        /// <summary>
        /// この値を超えるとタイマカウントが0になります。
        /// </summary>
        Top,
    }

    public class PWM : IPheripheral, IGPIO {

        readonly IClock clock;
        IISRNotify isr;

        readonly int onReachedTopIsrNo;
        readonly Timer32 timer;
        readonly Timer32PeriodicAlarm compareMatchedTimer;
        readonly Timer32PeriodicAlarm bottom;

        bool interruptEnable = false;
        bool invertOutput = false;

        public event Action? OnChangedGPIOValue;

        bool gpioValue;

        public bool GPIOValue {
            get => gpioValue;
            // 外部から出力を変えることはできないので捨てる
            set {}
        }

        public PWM(IClock clock, IISRNotify isr, int onReachedTopIsrNo, double frequency) {
            this.clock = clock;
            this.isr = isr;
            timer = new Timer32(clock, frequency);
            compareMatchedTimer = new Timer32PeriodicAlarm(timer, OnCompareMatched);
            bottom = new Timer32PeriodicAlarm(timer, OnReachedTop);
            compareMatchedTimer.Enable = true;
            bottom.Enable = true;
            timer.Enable = false;
            this.onReachedTopIsrNo = onReachedTopIsrNo;
        }

        void SetOutputValue(bool value) {
            var converted = invertOutput ? !value : value;
            bool isChanged = gpioValue != converted;
            gpioValue = converted;
            if (isChanged) {
                OnChangedGPIOValue?.Invoke();
            }
        }

        void OnCompareMatched() {
            SetOutputValue(true);
        }

        void OnReachedTop() {
            SetOutputValue(false);
            if (interruptEnable) {
                isr.SetInterrupt(onReachedTopIsrNo, true);
            }
        }

        public uint ReadUint32(uint offset) {
            var regNo = (PWMRegister)offset;
            switch (regNo) {
                case PWMRegister.Control: {
                    var ctrl = (timer.Enable ? PWMControlRegister.Enable : 0) |
                        (interruptEnable ? PWMControlRegister.InterruptEnable : 0) |
                        (invertOutput ? PWMControlRegister.InvertOutput : 0);
                    return (uint)ctrl;
                }
                case PWMRegister.TimerCount:
                    return timer.Counter;
                case PWMRegister.CompareMatch:
                    return compareMatchedTimer.Target;
                case PWMRegister.Top:
                    return timer.Top;
                default:
                    return 0;
            }
        }

        public void WriteUint32(uint offset, uint value) {
            var regNo = (PWMRegister)offset;
            switch (regNo) {
                case PWMRegister.Control:
                    timer.Enable = (value & (uint)PWMControlRegister.Enable) != 0;
                    //Console.WriteLine("timer " + timer.Enable);
                    interruptEnable = (value & (uint)PWMControlRegister.InterruptEnable) != 0;
                    invertOutput = (value & (uint)PWMControlRegister.InvertOutput) != 0;
                    if (!interruptEnable) {
                        // 割り込み禁止にした場合は、割り込み要求だ発生したことをなかったことにする
                        isr.SetInterrupt(onReachedTopIsrNo, false);
                    }
                    return;
                case PWMRegister.TimerCount:
                    timer.Set(0xFFFF & value);
                    return;
                case PWMRegister.CompareMatch:
                    //Console.WriteLine("timer cmp " + (0xFFFF & value));
                    compareMatchedTimer.Target = 0xFFFF & value;
                    return;
                case PWMRegister.Top:
                    //Console.WriteLine("timer count " + value + " " + (0xFFFF & value));
                    bottom.Target = 0xFFFF & value;
                    timer.Top = 0xFFFF & value;
                    return;
                default:
                    return;
            }
        }
    }
}
