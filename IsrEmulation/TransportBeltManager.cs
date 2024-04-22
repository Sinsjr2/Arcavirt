
/// <summary>
/// 物体を搬送するためのベルトの設置座標や長さを保持します。
/// ベルトとベルトの間は0以上離れていることを想定しています。
/// </summary>
public record TransportBelt(int Position, int Length);

/// <summary>
/// 搬送する物体の情報
/// 物体がベルトの端に到達したかや次のベルトに渡されたかどうかを判定するために使用します。
/// </summary>
public record TransportObjectInfo(int Length);

public record TransportModeSetting(ushort TimingNo, TransportMode Mode, int Position);

public class TransportingObject {
    /// <summary>
    /// 物体先頭の現在座標を知るための基準となるモーターの位置
    /// ベルトの先頭に物体先頭が乗ったときのモーター位置でもある
    /// </summary>
    public int BaseMotorPosition;

    /// <summary>
    /// 物体を識別するための番号
    /// </summary>
    public uint ObjectNo;

    public readonly TransportObjectInfo Info;

    /// <summary>
    /// キーはタイミング番号
    /// </summary>
    public Dictionary<ushort, TransportParam> RunningStatus;

    public TransportingObject(int baseMotorPosition, uint objectNo,
                              TransportObjectInfo info,
                              IEnumerable<TransportModeSetting> settings) {
        BaseMotorPosition = baseMotorPosition;
        ObjectNo = objectNo;
        Info = info;
        RunningStatus = settings
            .ToDictionary(
                x => x.TimingNo,
                x => new TransportParam(x.Mode, TransportStatus.NotArrival, x.Position, 0));
    }
}

public class TransportBeltManager {

    /// <summary>
    /// ベルトに載せている物体を識別する番号を発行するために使用します。
    /// </summary>
    uint latestObjectNo = 0;

    /// <summary>
    /// 搬送するベルトの情報
    /// </summary>
    readonly TransportBelt beltConfig;

    int motorCurrentPosition = 0;

    readonly Dictionary<uint, TransportingObject> transportObjects = new();

    /// <summary>
    /// 搬送中の物体の番号を返します。
    /// </summary>
    public IEnumerable<uint> TransportingObjectNumbers => transportObjects.Keys;

    public TransportBeltManager(TransportBelt beltConfig) {
        this.beltConfig = beltConfig;
    }

    // /// <summary>
    // /// 搬送が完了してベルトの範囲外に出た時に呼び出します。
    // /// </summary>
    // public event Action<int>? OnTransportCompleted;

    /// <summary>
    /// 搬送している物体の搬送ステータスが変化したことを通知します。
    /// </summary>
    public event Action<uint, ushort, TransportStatus>? OnChangedTransportStatus;

    /// <summary>
    /// 搬送している物体を削除するための情報を一時保存するために使用します。
    /// GC回数を減らすためにオブジェクトを使い回す
    /// </summary>
    readonly List<uint> removeObjectNumbers = new();

    /// <summary>
    /// ベルトの先頭に物体を載せ、その管理番号を返します。
    /// </summary>
    public uint PutObjectOnBelt(TransportObjectInfo info, IEnumerable<TransportModeSetting> modeSettings) {
        latestObjectNo++;
        transportObjects[latestObjectNo] =
            new TransportingObject(motorCurrentPosition, latestObjectNo, info, modeSettings);
        return latestObjectNo;
    }

    /// <summary>
    /// 指定したオブジェクト番号の物体の先頭座標を求めます。
    /// </summary>
    public int GetTransportObjectPosition(uint objectNo) {
        var transportObj = transportObjects[objectNo];
        return motorCurrentPosition - transportObj.BaseMotorPosition;
    }

    /// <summary>
    /// 停止する位置が設定されていない場合は、null を返します。
    /// 1軸だけで搬送していた場合を想定しています。搬送が完了してベルトの範囲外に出た時に呼び出します。
    /// また、搬送が指定した位置に到達していない場合は停止位置を返します。
    /// </summary>
    public int? GetNextStopPosition() {
        // 次に停止する最小の位置を検索する
        int? nextStopPos = null;
        foreach (var obj in transportObjects.Values) {
            foreach (var timingParam in obj.RunningStatus.Values) {
                if (timingParam.Mode != TransportMode.Go && (
                    timingParam.Mode == TransportMode.Stop ||
                    timingParam.Status == TransportStatus.NotArrival)) {
                    nextStopPos ??= int.MaxValue;
                    nextStopPos = Math.Min(timingParam.Position, nextStopPos.Value);
                }
            }
        }
        return nextStopPos;
    }

    /// <summary>
    /// 指定したタイミング番号の搬送モードを搬送再開に変更します。
    /// </summary>
    public void AllowTransport(uint transportJobNo, ushort timingNo) {
        transportObjects[transportJobNo].RunningStatus[timingNo].Mode = TransportMode.Go;
    }

    public void Update(int motorPosition) {
        this.motorCurrentPosition = motorPosition;

        removeObjectNumbers.Clear();

        // 先頭は右にあることを想定している
        // 右に進むことを想定している
        foreach (var obj in transportObjects.Values) {
            // ベルトの先頭を基準とした物体の先頭と後端の相対座標
            var objTop = this.motorCurrentPosition - obj.BaseMotorPosition;
            var objEnd = objTop - obj.Info.Length;

            // 物体がベルトの駆動外まで搬送されたことを確認
            bool isOutRange = objTop < 0 || beltConfig.Length < objEnd;

            var currentObjPos = GetTransportObjectPosition(obj.ObjectNo);

            foreach (var pair in obj.RunningStatus) {
                var timingParam = pair.Value;
                if (timingParam.Status == TransportStatus.NotArrival &&
                    timingParam.Position <= currentObjPos) {
                    timingParam.Status = TransportStatus.Arrival;
                    OnChangedTransportStatus?.Invoke(obj.ObjectNo, pair.Key, timingParam.Status);
                }
            }

            if (isOutRange) {
                Console.WriteLine($"delete {objTop} {objEnd} {obj.ObjectNo}");
                removeObjectNumbers.Add(obj.ObjectNo);
            }
        }

        foreach (var no in removeObjectNumbers) {
            transportObjects.Remove(no);
        }
        removeObjectNumbers.Clear();
    }
}
