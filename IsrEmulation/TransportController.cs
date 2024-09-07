using Clock;

public enum TransportMode {
    None,

    /// <summary>
    /// 指定した位置で停止し、
    /// 停止したことを通知するモード
    /// </summary>
    Stop,

    /// <summary>
    /// 停止した時に搬送再開するモード
    /// 停止しません。
    /// 初めからこのモードにしておくと、
    /// 指定された位置を通過したことを通知することにも使用できます。
    /// </summary>
    Go,

    // /// <summary>
    // /// 指定した位置に到達すると速度を変更するモード
    // /// </summary>
    // ChangeSpeed
}

public enum TransportStatus {
    NotArrival,
    Arrival,
}

public class TransportParam {
    public TransportMode Mode;
    public TransportStatus Status;

    /// <summary>
    /// 搬送モードを適用する位置
    /// </summary>
    public int Position;

    /// <summary>
    /// ChangeSpeed モードで合った時に変更する速度
    /// </summary>
    public int ChangedSpeed;

    public TransportParam(TransportMode mode, TransportStatus status, int position, int changedSpeed) {
        Mode = mode;
        Status = status;
        Position = position;
        ChangedSpeed = changedSpeed;
    }
}

public class TimingParam {
    public readonly int TimingNo;
    public readonly Clock.ICountObserver<int> Observer;
    public TransportMode Mode;
    public TransportStatus Status;
    public int Position;

    public TimingParam(int timingNo, ICountObserver<int> observer, TransportMode mode, TransportStatus status, int position) {
        TimingNo = timingNo;
        Observer = observer;
        Status = status;
        Position = position;
        Mode = mode;
    }
}

public class TransportController {

    readonly StepperDriver[] drivers;

    readonly Clock.CountWatcher<int> nextStopPositionManager = new(0, (a, b) => a < b);

    readonly TimingParam[] timingParams;

    /// <summary>
    /// キーは搬送JOB番号、タイミング番号
    /// </summary>
    Dictionary<byte, Dictionary<ushort, TransportParam>> RunningStatus = new();

    public event Action<int>? OnChangedTransportStatus;

    public TransportController(StepperDriver[] drivers) {
        this.drivers = drivers;

        timingParams = Enumerable.Range(0, 50)
            .Select(i => {
                var observer = nextStopPositionManager.Create(pos => { OnChangedTransportStatus?.Invoke(i); });
                var param = new TimingParam(i, observer, TransportMode.None, TransportStatus.NotArrival, 0);
                return param;
            })
            .ToArray();
    }

    public void StartTransportJob() {
        var currentPos = drivers[0].GetCurrentPosition();
        int maxPosition = 0;
        foreach (var param in timingParams) {
            if (param.Mode != TransportMode.None) {
                break;
            }
            // 次に停止もしくは、通過を通知するための位置を設定する
            param.Observer.Schedule(param.Position + currentPos);
        }
    }

    public void AddTransportJob(int timingNo, TransportMode mode, int position) {
        var timingParam = timingParams[timingNo];
        timingParam.Mode = mode;
        timingParam.Position = position;
    }

    /// <summary>
    /// 停止する位置が設定されていない場合は、null を返します。
    /// </summary>
    int? GetNextStopPosition() {
        // 次に停止する最小の位置を検索する
        int? nextStopPos = null;
        foreach (var job in RunningStatus.Values) {
            foreach (var x in job.Values) {
                if (x.Mode == TransportMode.Stop) {
                    nextStopPos ??= int.MaxValue;
                    if (x.Position < nextStopPos) {
                        nextStopPos = x.Position;
                    }
                }
            }
        }
        return nextStopPos;
    }
}
