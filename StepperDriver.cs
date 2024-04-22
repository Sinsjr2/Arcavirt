public class StepperDriver {
    readonly PWMTimerDriver timer;
    readonly GPOutput1BitDriver dirPin;

    int startVelocity = 1000;
    int acc = 500;
    int dec = 1000;
    int minConstDistance = 1;
    /// <summary>
    /// 加速したときの上限
    /// </summary>
    int maxVelocity = 5000;

    DriveParameter driveParameter;

    /// <summary>
    /// 移動を開始した時点からの経過時間
    /// 速度を計算するために使用します。
    /// </summary>
    float currentTime;

    int targetPosition;

    /// <summary>
    /// 現在の軸の位置
    /// </summary>
    int currentPosition;

    /// <summary>
    /// 正転であると1
    /// 逆転であると-1
    /// </summary>
    int dir;

    /// <summary>
    /// 指定した位置に到達した場合に呼び出します。
    /// </summary>
    public event Action? OnReachedTarget;

    /// <summary>
    /// 位置が変化するごとに呼び出します。
    /// </summary>
    public event Action? OnChangedPosition;

    public readonly Clock.CountWatcher<int> CcwCountObserver =
        new Clock.CountWatcher<int>(0, (a, b) => a >= b);

    public readonly Clock.CountWatcher<int> CwCountObserver =
        new Clock.CountWatcher<int>(0, (a, b) => a <= b);

    /// <summary>
    /// 回転中かどうかを表します。
    /// 目標位置に到達すると停止します。
    /// </summary>
    bool isRunning;

    public bool IsRunning => isRunning;

    public StepperDriver(PWMTimerDriver timer, GPOutput1BitDriver dirPin) {
        this.timer = timer;
        this.dirPin = dirPin;
    }

    public int GetCurrentPosition() {
        return currentPosition;
    }

    public int GetTargetPosition() {
        return targetPosition;
    }

    public void SetTargetPosition(int targetPosition) {
        // 同じ位置であると動かす必要なし
        // TODO 逆回転禁止モードを追加する
        if (targetPosition == currentPosition) {
            return;
        }
        this.targetPosition = targetPosition;
        // TODO スレッドセーフにする必要あり
        var pulseCount = Math.Abs(currentPosition - targetPosition);
        bool dirSignal = currentPosition < targetPosition;
        dir = dirSignal ? 1 : -1;
        var speedHz = Math.Max(startVelocity, CurveDesigner.V(driveParameter, currentTime));
        currentTime = 0;
        driveParameter = CurveDesigner.CalcParameter(speedHz, startVelocity, acc, dec, maxVelocity, pulseCount, minConstDistance);
        dirPin.Set(dirSignal);
        if (!isRunning) {
            isRunning = true;
            // 内部でカウンターをリセットしているので、
            // 動作開始時のみ呼び出すようにする
            SetTrigger(driveParameter.StartVelocity);
            timer.Start();
        }
    }

    void SetTrigger(float cycle) {
        var cycleTimer = (int)(timer.TimerHz / cycle);
        // TODO パルスの幅を可変出来るようにする
        timer.Set(cycleTimer, 5);
    }

    public void OnCompareMatchedISR() {
        currentPosition += dir;
        var time = currentTime;
        var speedHz = CurveDesigner.V(driveParameter, time);
        var cycle = 1.0f / speedHz;
        SetTrigger(speedHz);
        time += cycle;
        currentTime = time;
        bool isReachedTarget = targetPosition == currentPosition;
        if (isReachedTarget) {
            isRunning = false;
            timer.Stop();
        }
        OnChangedPosition?.Invoke();
        (dir < 0 ? CcwCountObserver : CwCountObserver)
            .UpdateCount(currentPosition);
        if (isReachedTarget) {
            OnReachedTarget?.Invoke();
        }
    }
}
