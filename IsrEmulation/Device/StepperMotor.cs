using Clock;
using GPIO;

namespace Device; 

/// <summary>
/// ステップ信号とDIR信号を受け取り、軸の回転を行うステッピングモーターです。
/// </summary>
public class StepperMotor {
    readonly IClock clock;
    readonly IGPIO step;
    readonly IGPIO dir;

    /// <summary>
    /// 速度が0になったことを通知するために使用します。
    /// </summary>
    readonly IAlarm onSpeedZeroTimer;

    bool prevClock = false;
    double? prevRiseTime;

    /// <summary>
    /// この値よりも小さくなると速度を0になったと判定します。
    /// </summary>
    readonly double minSpeed;

    /// <summary>
    /// 速度が0になったときやステップ数が変化した時に呼び出します。
    /// </summary>
    public event Action? OnChanged;
    public double Position { get; set; } = 0.0;

    double speed;

    /// <summary>
    /// 逆回転すると速度はマイナスになります。
    /// </summary>
    public double Speed {
        get {
            // if (!prevRiseTime.HasValue) {
            //     return 0.0;
            // }
            // var currentTime = clock.Nanos;
            // var deltaTime = currentTime - prevRiseTime.Value;
            // var deltaTimeSec = (currentTime - prevRiseTime.Value) / 1000000000.0;
            // var speed = PulseWidthToCycle(deltaTimeSec);
            // if (speed < minSpeed) {
            //     return 0.0;
            // }
            return speed;
        }
    }

    public StepperMotor(IClock clock, IGPIO step, IGPIO dir, double minSpeed) {
        if (minSpeed <= 0) {
            throw new ArgumentException($"{nameof(minSpeed)} は 0よりも大きくして下さい。");
        }
        this.clock = clock;
        this.step = step;
        this.dir = dir;
        this.minSpeed = minSpeed;

        step.OnChangedGPIOValue += Update;
        dir.OnChangedGPIOValue += Update;
        onSpeedZeroTimer = clock.CreateAlarm(() => {
            // パルスが停止してから一定時間経過すると速度を0にするための処理
            prevRiseTime = null;
            speed = 0.0;
            onSpeedZeroTimer!.Cancel();
            OnChanged?.Invoke();
        });
    }

    /// <summary>
    /// パルスの幅から周期をを求めます。
    /// </summary>
    static double PulseWidthToCycle(double pulseWidth) {
        return 1.0 / pulseWidth;
    }

    void Update() {
        var currentStepSignal = step.GPIOValue;
        var riging = !prevClock && currentStepSignal;
        prevClock = currentStepSignal;
        if (riging) {
            if (!prevRiseTime.HasValue) {
                // 速度が0になっていることを運転開始前に通知するため
                OnChanged?.Invoke();
                prevRiseTime = clock.Nanos;
                return;
            }
            var currentTime = clock.Nanos;
            var deltaTime = currentTime - prevRiseTime.Value;
            var dirSignal = dir.GPIOValue;
            var deltaTimeSec = deltaTime / 1000000000.0;
            var speed = PulseWidthToCycle(deltaTimeSec);
            this.speed = dirSignal ? speed : -speed;
            Position = dirSignal ? Position + 1 : Position - 1;
            prevRiseTime = currentTime;
            onSpeedZeroTimer.Schedule(1.0 / minSpeed * 1_000_000_000);
            OnChanged?.Invoke();
        }
    }
}
