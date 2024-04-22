public enum TransportMode {

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

    /// <summary>
    /// 指定した位置に到達すると速度を変更するモード
    /// </summary>
    ChangeSpeed
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

// public class TransportController {

//     /// <summary>
//     /// キーは搬送JOB番号、タイミング番号
//     /// </summary>
//     Dictionary<byte, Dictionary<ushort, TransportParam>> RunningStatus = new();

//     public void StartTransportJob(ushort jobNo) {
//     }

//     /// <summary>
//     /// 停止する位置が設定されていない場合は、null を返します。
//     /// </summary>
//     int? GetNextStopPosition() {
//         // 次に停止する最小の位置を検索する
//         int? nextStopPos = null;
//         foreach (var job in RunningStatus.Values) {
//             foreach (var x in job.Values) {
//                 if (x.Mode == TransportMode.Stop) {
//                     nextStopPos ??= int.MaxValue;
//                     if (x.Position < nextStopPos) {
//                         nextStopPos = x.Position;
//                     }
//                 }
//             }
//         }
//         return nextStopPos;
//     }
// }
