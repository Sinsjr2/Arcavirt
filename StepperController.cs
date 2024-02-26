public class StepperController {
    readonly PWMTimer timer;

    int startVelocity = 100;
    int acc = 50;
    int dec = 100;
    int minConstDistance = 1;
    /// <summary>
    /// 加速したときの上限
    /// </summary>
    int maxVelocity = 1000;

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

    /// <summary>
    /// 回転中かどうかを表します。
    /// 目標位置に到達すると停止します。
    /// </summary>
    bool isRunning;

    /// <summary>
    /// 回転方向を変更します。
    /// true は正転 false は逆転
    /// </summary>
    readonly Action<bool> changeDir;

    public StepperController(PWMTimer timer, Action<bool> changeDir) {
        this.timer = timer;
        this.changeDir = changeDir;
    }

    public int GetCurrentPosition() {
        return currentPosition;
    }

    public void SetTargetPosition(int targetPosition) {
        // 同じ位置であると動かす必要なし
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
        changeDir(dirSignal);
        if (!isRunning) {
            isRunning = true;
            // 内部でカウンターをリセットしているので、
            // 動作開始時のみ呼び出すようにする
            SetTrigger(1.0f / driveParameter.StartVelocity);
            timer.SetEnable(true);
        }
    }

    void SetTrigger(float cycle) {
        // TODO タイマーの値と周波数をタイマーの動作周波数から計算するようにする
        var cycleTimer = (ushort)(cycle * 10000.0f);
        timer.TriggerA = cycleTimer;
        // TODO パルスの幅を可変出来るようにする
        timer.TriggerB = (ushort)(cycleTimer + 5);

    }

    public void OnCompareMatched() {
        currentPosition += dir;

        var time = currentTime;
        var speedHz = CurveDesigner.V(driveParameter, time);
        var cycle = 1.0f / speedHz;
        SetTrigger(cycle);
        time += cycle;
        currentTime = time;
        OnChangedPosition?.Invoke();
        if (targetPosition == currentPosition) {
            timer.SetEnable(false);
            isRunning = false;
            OnReachedTarget?.Invoke();
            return;
        }
    }
}

// public class StepperManager {
//     readonly PWMTimer timer;

//     public StepperManager(PWMTimer timer) {
//         this.timer = timer;
//     }

//     public readonly List<StepperAgent> Steppers = new();

//     public void OnCompareMatched() {
        
//     }
// }
