using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 数値の範囲を表します。
/// </summary>
public record struct RangeD(double Min, double Max);

public record DeviceID(Type DeviceType, string ID);

// ローラーやデバイスごとのたるみ量を表す
public record BetweenDevice(DeviceID ID1, DeviceID ID2, double Sagging);

/// <summary>
/// 搬送している物体の現在位置や長さを表す。
/// </summary>
/// <param name="ObjectID">搬送している物体を識別するためのID</param>
/// <param name="SolenoidJunctionOns">
///   分岐点のソレノイドを動作させるかのどうかを指定するために使用します。経路を確定するために使用します。
///   エントリーがない場合は、オフとして扱います。
/// </param>
/// <param name="PathID">搬送している物体の後端がどの搬送パス上にいるかを表すための名前</param>
/// <param name="PathPosition">その搬送パスの先頭からどの位置に後端が居るのかを表すための情報</param>
public record TransportObject(
    string ObjectID,
    double Length,
    double PathLength,
    IReadOnlyDictionary<string, bool> SolenoidJunctionOns,
    // 現在位置からの各ローラ間のたるみ量を保持している
    IReadOnlyList<double> Saggings,
    string PathID,
    double PathPosition
    // たわみ量を考慮したパスの長さを返します。
    ) {

    // /// <summary>
    // /// 物体の先頭位置を取得します。
    // /// </summary>
    // public double TopPosition =>
    //     PathPosition + Length - Saggings.Values.Sum(x => -x);

    // /// <summary>
    // /// たわみ量を考慮したパスの長さを返します。
    // /// </summary>
    // public double PathLength =>
    //     Length - Saggings.Values.Sum(x => -x);
}

public record TransportPath(
    // TransportDevice から参照する時に使用するパスの名前
    string PathID,
    // 搬送の経路
    IReadOnlyList<Point> Path
);

/// <summary>
/// 行き先2方向をソレノイドにより切り替えます
/// オンで先に指定した経路に切り替わります。
/// </summary>
public record SolenoidTransportPathJunction(
    string JunctionID,
    string SrcPathID,
    int SrcPathPosition,
    string DestPathID
);

/// <summary>
/// 反対方向に搬送出来ません
/// </summary>
public record MergeTransportPath(
    IReadOnlyList<string> SrcPathIDs,
    int DestPathPosition,
    string DestPathID
);

public record TransportDevice(
    // 配置するときの対象となる搬送経路の名前
    string PathID,
    // 搬送経路の先頭基準で配置する位置
    double Position,
    ITransportDevice Device
);

public interface ITransportDevice {}

public record TransportSensor(
    // このセンサーの識別子
    string SensorID,
    // 紙の通過を検出してから信号が出るまでの遅延時間
    TimeSpan ResponseDelay,
    // 表示する時にどこに表示するのか
    float DisplayAngle
) : ITransportDevice;

public record TransportRoller(
    // ローラーの識別子
    string RollerID,
    // ローラーを回すモーター名
    string PowerSource,
    // // モーター1パルス当たりの紙の進む距離
    // シミュレータ上での単位を決めていないので、まだ使用しない
    // float MMPerPulse,
    // 表示するときの搬送経路のどこに表示するのか
    float DisplayAngle,
    // 表示するときのローラーの大きさ
    int DisplaySize
) : ITransportDevice;

public record Cutter : ITransportDevice;

public record Creaser : ITransportDevice;
