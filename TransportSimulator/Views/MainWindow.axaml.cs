using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using MoreLinq;

namespace TransportSimulatorAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //Loop(canvas);
        Dispatcher.UIThread.InvokeAsync(() => Loop(canvas).AsTask());
    }

    /// <summary>
    /// パスの先頭から指定した距離の座標を返します。
    /// パスの全長よりの大きい値を指定するとパスの最後の座標を返します。
    /// また、その時の前方の座標のインデックスも返します。
    /// 要素数が1以下の場合はインデックスを-1で返します。
    /// </summary>
    (Point pos, int index) CreatePathPoint(IReadOnlyList<Point> transportPaths, double pos) {
        double totalLength = 0;
        int index = -1;
        for (int i = 1; i < transportPaths.Count; i++) {
            Vector linePath = transportPaths[i] - transportPaths[i - 1];
            if (pos <= totalLength + linePath.Length) {
                index = i;
                break;
            }
            totalLength += linePath.Length;
        }

        if (index < 0) {
            return transportPaths.Any()
                ? (transportPaths[transportPaths.Count - 1], transportPaths.Count - 1)
                : (new Point(0, 0), -1);
        }
        Vector x1 = transportPaths[index - 1];
        Vector x2 = transportPaths[index];
        var line = (x2 - x1).Normalize() * (pos - totalLength);
        return ((Point)(x1 + line), index - 1);
    }

    Point[] CreatePathLine(Point[] transportPath, double objLength, double endPos) {
        if (!transportPath.Any()) {
            return Array.Empty<Point>();
        }

        var result1 = CreatePathPoint(transportPath, endPos);
        var result2 = CreatePathPoint(transportPath, endPos + objLength);
        return new Point[] { result1.pos }
            .Concat(transportPath[(result1.index + 1)..(result2.index + 1)])
            .Append(result2.pos)
            .ToArray();
    }

    /// <summary>
    /// パスの全長を返します。
    /// </summary>
    double CalcPathLength(IReadOnlyList<Point> path) {
        if (!path.Any()) {
            return 0;
        }
        Vector prev = path[0];
        double total = 0;
        foreach (var x in path.Skip(1)) {
            Vector vec = x;
            total += (vec - prev).Length;
            prev = vec;
        }
        return total;
    }


    /// <summary>
    /// 指定されたパスの先頭からの位置を指定することで、
    /// 現在の分岐点の状態を反映して、指定した長さ分のパスを返します。
    ///
    /// 戻り値として 1パス分のパスの開始点とそこからの長さを返します。
    ///
    /// 分岐点のソレノイドのオンオフのエントリーがない場合はオフとして扱います。
    /// </summary>
    (Point[] path, string pathID, double pathPos, double length)[] CreateTransportObjPath(
        IReadOnlyDictionary<string, TransportPath> transportPaths,
        IReadOnlyList<SolenoidTransportPathJunction> solenoidJunctions,
        IReadOnlyDictionary<string, bool> solenoidOns,
        IReadOnlyList<MergeTransportPath> mergePoints,
        string srcPathID,
        double srcPathPos,
        double length
    ) {
        var transportPath = transportPaths[srcPathID];

        // 切り替わる分岐点を探す
        var junction = solenoidJunctions
            .Where(t => solenoidOns.TryGetValue(t.JunctionID, out var isOn) &&
                   isOn &&
                   // 現在地点よりも遠くにある分岐点を探す
                   srcPathPos <= t.SrcPathPosition &&
                   t.SrcPathID == srcPathID)
            .Select(x => (founds: true, x))
            .DefaultIfEmpty((founds: false, x: new SolenoidTransportPathJunction("", "", 0, "")))
            .MinBy(x => x.x.SrcPathPosition);
        if (junction.founds) {
            // パス上に分岐点があった場合
            // 分岐点までで指定された距離のパスを作れるかどうかを判定する
            // 作れる場合は、終了
            var pathLength = Math.Min(junction.x.SrcPathPosition - srcPathPos, length);
            var path = CreatePathLine(transportPath.Path.ToArray(), pathLength, srcPathPos);
            if (pathLength < length) {
                var result = CreateTransportObjPath(
                    transportPaths, solenoidJunctions, solenoidOns, mergePoints, junction.x.DestPathID, 0, length - pathLength);
                return result.Prepend((path, transportPath.PathID, srcPathPos, pathLength)).ToArray();
            }
            return new[] { (path, srcPathID, srcPathPos, length) };
        }
        else {
            var paths = transportPath.Path.ToArray();
            var totalLength = CalcPathLength(paths);
            // 分岐点はなかったので、のこりのパスの長さを計測し、与えれれた長さのパスを返せるか計算
            var path = CreatePathLine(paths, length, srcPathPos);
            var pathLength = CalcPathLength(path);
            // パスの最後まで伸ばしたが距離が足りなかった場合
            // 取得したパスに対して計算すると計算誤差で、パスの端に達する前に以下の処理をされてしまう
            if ((totalLength - srcPathPos) < length) {
                // 指定した長さよりもパスが短かったので、合流した先のパスも長さに含める
                // 1つの終端に複数の合流点があることを想定していない
                var mergePoint = mergePoints
                    .Where(x => x.SrcPathIDs.Any(x2 => x2 == transportPath.PathID))
                    .FirstOrDefault();
                if (mergePoint is not null) {
                    var result = CreateTransportObjPath(
                        transportPaths, solenoidJunctions, solenoidOns, mergePoints, mergePoint.DestPathID,
                        mergePoint.DestPathPosition, length - pathLength);
                    return result.Prepend((path, transportPath.PathID, srcPathPos, pathLength)).ToArray();
                }
            }
            // 指定した長さのパスが見つかったので処理を終了する
            // もしくは、長さを満たさなかったが、パスの続きがなかったので終了
            return new[] { (path , transportPath.PathID, srcPathPos, pathLength) };
        }
    }

    /// <summary>
    /// ローラ間のたるみ量を計算します。
    /// </summary>
    static double CalcSagging(double roller1DeltaStep, double roller2DeltaStep) {
        if (0 <= roller1DeltaStep && 0 <= roller2DeltaStep) {
            return roller1DeltaStep - roller2DeltaStep;
        }
        if (roller1DeltaStep < 0 && roller2DeltaStep < 0) {
            return -roller1DeltaStep + roller2DeltaStep;
        }
        // お互い逆回転で引っ張り合う場合
        return -(Math.Abs(roller1DeltaStep) + Math.Abs(roller2DeltaStep));
    }

    /// <summary>
    /// 現在のたるみ量を加味して、ローラーのステップ数の上限を制限した値を計算します。
    /// </summary>
    static (double deltaStepA, double deltaStepB) CalcUpperLimittedRollerDeltaStep(
        double upperLimit, double currentSagging, double deltaStepA, double deltaStepB) {
        // 互いに逆回転してたわむ場合
        var delta = upperLimit - Math.Min(currentSagging, upperLimit);
        if (0.0 < deltaStepA && deltaStepB < 0.0) {
            var a = delta * deltaStepA / (deltaStepA + (-deltaStepB));
            return (a, -(delta - a));
        }
        // お互い同じ方向に回転していた場合 正
        if (0.0 <= deltaStepA && 0.0 <= deltaStepB) {
            var stepUpperLimit = delta + Math.Min(deltaStepA, deltaStepB);
            return (Math.Min(deltaStepA, stepUpperLimit), Math.Min(deltaStepB, stepUpperLimit));
        }
        // お互い同じ方向に回転していた場合 負
        if (deltaStepA <= 0.0 && deltaStepB <= 0.0) {
            var stepUpperLimit = -(delta - Math.Max(deltaStepA, deltaStepB));
            return (Math.Max(deltaStepA, stepUpperLimit), Math.Max(deltaStepB, stepUpperLimit));
        }

        // 引っ張る場合は未対応
        // たるまない場合も同様
        return (deltaStepA, deltaStepB);
    }

    static (TransportObject obj, double moveDistance) MoveAndCalcSagging(TransportObject obj,
                                              // 搬送経路の内十分に移動範囲内のローラーが含まれていることを想定しています。
                                              IReadOnlyList<double> deltaRollerStepps,
                                              IReadOnlyList<double> rollerPositions,
                                              // IReadOnlyList<double> currentSaggings,
                                              // たるみ量の下限は0以下の数値であることを前提としている
                                              IReadOnlyList<RangeD> sagginLimits) {
        // ローラーと接していない部分前半
        // 搬送する物体とはじめて接するローラーを探索する
        var firstRoller = rollerPositions
            .Index()
            .FirstOrDefault(x => obj.PathPosition <= x.Value, new(-1, 0.0));

        // ローラーと一つも接していない場合は移動させない
        if (firstRoller.Key < 0) {
            return (obj, 0);
        }

        // 搬送する物体と接していないローラーを除外する
        var rollerPiches = rollerPositions.Skip(firstRoller.Key).Pairwise((a, b) => Math.Abs(b - a)).ToArray();
        // var nextSaggings = deltaRollerStepps.Skip(firstRoller.Key).Pairwise((a, b) => CalcSagging(a, b))
        //     .Zip(currentSaggings.Skip(firstRoller.Key), sagginLimits.Skip(firstRoller.Key))
        //     .Select(t => {
        //         var (current, prev, limit) = t;
        //         return (sagging: current + prev, limit);
        //     })
        //     .ToArray();

        // 引っ張り具合を反映した搬送する物体に対するローラーの回転量
        var calclatedRollerDeltaSteps = deltaRollerStepps.Skip(firstRoller.Key).ToArray();
        double[] nextSaggings;
        bool shouldLoop = true;
        // nextSaggings = calclatedRollerDeltaSteps.Skip(firstRoller.Key). obj.Saggings.Skip(firstRoller.Key).ToArray();
        nextSaggings = calclatedRollerDeltaSteps.Skip(firstRoller.Key).Pairwise((a, b) => 0)
            // 既にたるみ量が設定されている場合は追加する
            .ZipLongest(obj.Saggings.Skip(firstRoller.Key), (a, b) => b)
            .ToArray();
        Console.WriteLine(">>");
        while(shouldLoop) {
            shouldLoop = false;
            int? linkBegin = null;
            for (int i = 0; i < rollerPiches.Length; i++) {
                // < にすることで値の範囲を超えない限りたわみの下限を超えたと判定しないようにしている
                var currentSagging = nextSaggings[i] + CalcSagging(calclatedRollerDeltaSteps[i], calclatedRollerDeltaSteps[i + 1]);
                var isLowerLimit = currentSagging < sagginLimits[i].Min;
                var isUpperLimit = sagginLimits[i].Max < currentSagging;
                nextSaggings[i] = Math.Min(Math.Max(currentSagging, sagginLimits[i].Min), sagginLimits[i].Max);
                if (!linkBegin.HasValue && isLowerLimit) {
                    // たわみの下限を超えたので隣接するローラーの影響を受ける
                    linkBegin = i;
                    Console.WriteLine($"--------------------{i}--------------");
                }
                else if (linkBegin.HasValue && (!isLowerLimit ||rollerPiches.Length - 1 <= i)) {
                    Console.WriteLine("lower");
                    // たるみ状態の区間を発見したので、ひっぱり具合を確定させる
                    // もしくは、配列の最後までひっぱり状態の場合
                    var linkEnd = i + 1;
                    var averageMovement = deltaRollerStepps
                        .Slice(linkBegin.Value, linkEnd - linkBegin.Value)
                        .Average();
                    for (int j = linkBegin.Value; j < linkEnd; j++) {
                        calclatedRollerDeltaSteps[j] = averageMovement;
                    }
                    // nextSaggings = calclatedRollerDeltaSteps.Skip(firstRoller.Key).Pairwise((a, b) => CalcSagging(a, b))
                        // .ToArray();
                    linkBegin = null;
                    // ひっぱり状態を変更したので
                    // 別の部分でひっぱり状態になっている可能性があるので再度計算する
                    shouldLoop = true;
                }
                else if (isUpperLimit) {
                    var limitedStep = CalcUpperLimittedRollerDeltaStep(
                        sagginLimits[i].Max,
                        currentSagging,
                        calclatedRollerDeltaSteps[i],
                        calclatedRollerDeltaSteps[i + 1]);
                    Console.WriteLine("upper " + limitedStep.deltaStepA);
                    calclatedRollerDeltaSteps[i] = limitedStep.deltaStepA;
                    calclatedRollerDeltaSteps[i + 1] = limitedStep.deltaStepB;
                    // nextSaggings[i] += CalcSagging(limitedStep.deltaStepA, limitedStep.deltaStepB);;
                    // たわみでつかえた状態を変更したので
                    // 別の部分でつかえた状態になっている可能性があるので再度計算する
                    shouldLoop = true;
                }
            }
        }
        Console.WriteLine("<<");

        // var nextSaggings = deltaRollerStepps.Skip(firstRoller.Key).Pairwise((a, b) => CalcSagging(a, b))
        //     .Zip(currentSaggings.Skip(firstRoller.Key), sagginsLimits.Skip(firstRoller.Key))
        //     .Select(t => {
        //         var (current, prev, limit) = t;
        //         var sagging =  current + prev;
        //         var limited = Math.Min(Math.Max(sagging, limit.saggingMin), limit.saggingMax);
        //         return limited;
        //     });

        // var distanceBegin = (firstRoller.Value - obj.PathPosition);
        var transportDistanceWithSagging =
            sagginLimits.Zip(rollerPiches, nextSaggings)
            .Select(t => {
                var (limit, pitch, sagging) = t;
                // たるみ量が負になったときの計算を成り立たせるために
                // ローラー間の搬送距離をあらかじめ足しておく
                var distance = pitch - limit.Min;
                return distance + sagging;
            })
            // 搬送ローラと物体が接している長さを計測する
            .Scan(rollerPositions[firstRoller.Key], (total, distance) => total + distance).Skip(1)
            .TakeWhile(pos => pos <= obj.PathPosition + obj.Length)
            // たるみ分だけパスを短くする
            .Zip(nextSaggings, (_, sagging) => Math.Max(0.0, sagging))
            .ToArray();

        var totalSagging = transportDistanceWithSagging
            .Sum();

        var moveDistance = calclatedRollerDeltaSteps[firstRoller.Key];
        // var nextEndPos = obj.PathPosition + moveDistance;
        // var nextTopPos = nextEndPos + obj.Length - totalSagging;

        // Console.WriteLine(distanceBegin);
        Console.WriteLine(moveDistance);
        Console.WriteLine(obj.Length - totalSagging);
        Console.WriteLine(string.Join(",", nextSaggings.Take(transportDistanceWithSagging.Length)));

        return (obj with {
                Saggings = nextSaggings.Take(transportDistanceWithSagging.Length).ToArray(),
                PathLength = obj.Length - totalSagging
                }, moveDistance);

        // // 移動後の搬送する物体の先頭の位置
        // // たるみ量込みの搬送距離
        // // ローラと接していない部分の距離 前半
        // var distanceBegin = (firstRoller.Value - obj.PathPosition);
        // var transportDistanceWithSagging =
        //     sagginLimits.Zip(rollerPiches, nextSaggings)
        //     .Select(t => {
        //         var (limit, pitch, sagging) = t;
        //         // たるみ量が負になったときの計算を成り立たせるために
        //         // ローラー間の搬送距離をあらかじめ足しておく
        //         var distance = pitch - limit.Min;
        //         return distance + sagging;
        //     })
        //     // 搬送ローラと物体が接している長さを計測する
        //     .Scan(distanceBegin, (total, distance) => total + distance)
        //     .TakeWhile(distance => obj.Length <= distance)
        //     // たるみ量を抜いた搬送距離を計算するためにまとめる
        //     .Zip(rollerPiches.Scan(0.0, (total, distance) => total + distance))
        //     .DefaultIfEmpty((0.0, 0.0))
        //     .Last();

        // var moveDistance = calclatedRollerDeltaSteps[firstRoller.Key];
        // var nextEndPos = obj.PathPosition + moveDistance;
        // var nextTopPos = nextEndPos +
        //     distanceBegin +
        //     // ローラーと接している部分の距離
        //     transportDistanceWithSagging.Item2 +
        //     // ローラーと接していない部分の距離 後半
        //     (obj.Length - transportDistanceWithSagging.Item1);

        // Console.WriteLine(nextTopPos - nextEndPos);
        // Console.WriteLine(string.Join(",", nextSaggings));

        // return (obj with {
        //         Saggings = nextSaggings,
        //         PathLength = nextTopPos - nextEndPos
        //         }, moveDistance);
    }

    /// <summary>
    /// ローラーの回転によって搬送物を移動させます。
    /// </summary>
    TransportObject MoveWithRoller(
        TransportObject obj,
        IReadOnlyDictionary<string, TransportPath> transportPaths,
        IReadOnlyList<SolenoidTransportPathJunction> solenoidJunctions,
        IReadOnlyList<MergeTransportPath> mergePoints,
        IReadOnlyDictionary<string, bool> solenoidOns,
        IReadOnlyList<TransportDevice> transportDevices,
        // 各ローラーごとの移動量 ステップ数単位
        IReadOnlyDictionary<string, double> deltaRollerStepps) {

        // パス内にある分岐点を抽出し、分岐方向を通過中に変わらないようにする
        // どれだけ移動するか計算していないので、
        // 移動距離分のパスを取得するために搬送物の長さの2倍分にしている
        var objPaths = CreateTransportObjPath(transportPaths, solenoidJunctions, obj.SolenoidJunctionOns,
                                              mergePoints, obj.PathID, obj.PathPosition, obj.Length * 2);

        if (!objPaths.Any()) {
            return obj;
        }

        // パスの順番に従って
        // 物体と接しているローラーを抽出
        var rollers = objPaths
            .Zip(
                objPaths
                .Scan(objPaths[0].pathPos + objPaths[0].length, (total, path) => total + path.length)
                .Prepend(0.0),
                (path, pos) =>
                transportDevices
                .Select(x => (x, roller: x.Device as TransportRoller))
                .Where(t => t.roller is not null &&
                       path.pathID == t.x.PathID && path.pathPos <= t.x.Position &&
                       t.x.Position <= path.pathPos + path.length)
                .Select(t => (t.x, roller: t.roller!, pos: pos + t.x.Position))
                // 隣接するローラーになるように並び替える
                .OrderBy(t => t.x.Position))
            .SelectMany(xs => xs)
            .ToArray();

        //objPaths.Select(path => path.length).Scan(0.0, (total, x) => total + x);

        var transportPathLengths = transportPaths.Values
            .ToDictionary(
                path => path.PathID,
                path => CalcPathLength(path.Path));

        // 複数区間つなぎ合わせるので、ローラーの位置を絶対位置に変換する
        // var rollerPositions  = new List<double>();
        // {
        //     var prevPathID = rollers.Any() ? rollers[0].x.PathID : "";
        //     var offset = 0.0;
        //     foreach (var roller in rollers) {
        //         if (prevPathID != roller.x.PathID) {
        //             offset += transportPathLengths[roller.x.PathID];
        //             prevPathID = roller.x.PathID;
        //         }
        //         rollerPositions.Add(offset + roller.x.Position);
        //     }
        // }
        var rollerPositions = rollers.Select(t => t.pos).ToArray();

        var newObj = MoveAndCalcSagging(
            obj with { PathID = objPaths[0].pathID, PathPosition = objPaths[0].pathPos },
            // 引数で指定したローラの移動量の内ローラーとして含まれていないものは移動量を0として扱う
            rollers.Select(roller => deltaRollerStepps.TryGetValue(roller.roller.RollerID, out var step)
                           ? step
                           : 0.0)
            .ToArray(),
            rollerPositions,
            // TODO たわみの限界はまだ設定値を作成していないので、仮
            Enumerable.Repeat(new RangeD(-1.0, 5.0), 500).ToArray());

        var nextStartPos = CreateTransportObjPath(
            transportPaths,
            solenoidJunctions,
            obj.SolenoidJunctionOns,
            mergePoints,
            obj.PathID,
            obj.PathPosition, newObj.moveDistance)
            .Last();


        return newObj.obj with {
            SolenoidJunctionOns = solenoidOns,
            PathID = nextStartPos.pathID,
            PathPosition = nextStartPos.pathPos + nextStartPos.length
        };

        // var solenoidJunctionOns = objPaths.SelectMany(
        //     path => solenoidJunctions
        //     .Where(x => path.pathID == x.SrcPathID &&
        //            path.pathPos <= x.SrcPathPosition &&
        //            x.SrcPathPosition <= path.pathPos + path.length))
        //     .DistinctBy(x => x.JunctionID)
        //     .ToDictionary(x => x.JunctionID,
        //                   x => solenoidOns.TryGetValue(x.JunctionID, out var isOn) && isOn);

        // return newObj with { SolenoidJunctionOns = solenoidJunctionOns };
        // var rollerPitches = rollers.Pairwise((a, b) => b.x.Position - a.x.Position);
        // foreach (var t in transportDevices
        //          .Select(x => (x, roller: x.Device as TransportRoller))
        //          .Where(t => t.roller is not null)) {
        // }
        // return null;
    }

    /// <summary>
    /// 指定した物体を指定した距離移動させます。
    ///
    /// TODO 物体のたわみも計算に含めること
    /// </summary>
    TransportObject Move(IReadOnlyDictionary<string, TransportPath> transportPaths,
                         TransportObject obj,
                         double moveDistance,
                         IReadOnlyList<SolenoidTransportPathJunction> solenoidJunctions,
                         IReadOnlyDictionary<string, bool> solenoidOns,
                         IReadOnlyList<MergeTransportPath> mergePoints) {
        // 移動した後の後端のいち計算する
        var nextStartPos = CreateTransportObjPath(
            transportPaths,
            solenoidJunctions,
            obj.SolenoidJunctionOns,
            mergePoints,
            obj.PathID,
            obj.PathPosition,
            moveDistance)
            .Last();

        var newTransportPath = CreateTransportObjPath(
            transportPaths,
            solenoidJunctions,
            obj.SolenoidJunctionOns,
            mergePoints,
            nextStartPos.pathID,
            nextStartPos.pathPos + nextStartPos.length,
            obj.Length);

        // パス内にある分岐点を抽出し、分岐方向を通過中に変わらないようにする
        var solenoidJunctionOns = newTransportPath.SelectMany(
            path => solenoidJunctions
            .Where(x => path.pathID == x.SrcPathID &&
                   path.pathPos <= x.SrcPathPosition &&
                   x.SrcPathPosition <= path.pathPos + path.length))
            .DistinctBy(x => x.JunctionID)
            .ToDictionary(x => x.JunctionID,
                          x => solenoidOns.TryGetValue(x.JunctionID, out var isOn) && isOn);

        return obj with {
            SolenoidJunctionOns = solenoidJunctionOns,
            PathID = nextStartPos.pathID,
            PathPosition = nextStartPos.pathPos + nextStartPos.length
        };
    }

    async ValueTask Loop(Canvas canvas) {
        var transportPath1 = new TransportPath("aaa", new Point[] {
                new(0, 40),
                new(600, 40),
            });

        var transportPath2 = new TransportPath("bbb", new Point[] {
                new(380, 60),
                new(400, 90),
                new(400, 110),
                new(80, 110),
                new(80, 90),
                new(150, 60),
            });

        var solenoidOns = new Dictionary<string, bool> {
            { "junction", true }
        };
        var junctions = new SolenoidTransportPathJunction[] {
            new("junction", "aaa", 380, "bbb")
        };
        var mergePoints = new MergeTransportPath[] {
            new(new[] { "bbb" }, 150, "aaa")
        };

        var transportPaths = new[] {
            transportPath1,
            transportPath2
        }.ToDictionary(path => path.PathID);

        var transportDevices = new TransportDevice[] {
            new("aaa", 110, new TransportSensor("sensor1", TimeSpan.Zero, 0)),
            new("aaa", 185, new TransportSensor("sensor2", TimeSpan.Zero, 0)),
            new("aaa", 340, new TransportSensor("sensor3", TimeSpan.Zero, 0)),
            new("aaa", 420, new TransportSensor("sensor4", TimeSpan.Zero, 0)),
            new("bbb",  40, new TransportSensor("sensor5", TimeSpan.Zero, 0)),
            new("bbb", 360, new TransportSensor("sensor6", TimeSpan.Zero, 0)),

            new("aaa", 100, new TransportRoller("roller1_1", "motor1", 0, 15)),
            new("aaa", 130, new TransportRoller("roller1_2", "motor1", 0, 15)),

            new("aaa", 170, new TransportRoller("roller2_1", "motor2", 0, 15)),
            new("aaa", 200, new TransportRoller("roller2_2", "motor2", 0, 15)),
            new("aaa", 240, new TransportRoller("roller2_3", "motor2", 0, 15)),
            new("aaa", 280, new TransportRoller("roller2_4", "motor2", 0, 15)),
            new("aaa", 320, new TransportRoller("roller2_5", "motor2", 0, 15)),
            new("aaa", 360, new TransportRoller("roller2_6", "motor2", 0, 15)),

            new("aaa", 400, new TransportRoller("roller3_1", "motor3", 0, 15)),
            new("aaa", 440, new TransportRoller("roller3_2", "motor3", 0, 15)),

            new("bbb",  15, new TransportRoller("roller4_1" , "motor4", 0, 15)),
            new("bbb",  60, new TransportRoller("roller4_2" , "motor4", 0, 15)),
            new("bbb", 100, new TransportRoller("roller4_3" , "motor4", 0, 15)),
            new("bbb", 140, new TransportRoller("roller4_4" , "motor4", 0, 15)),
            new("bbb", 180, new TransportRoller("roller4_5" , "motor4", 0, 15)),
            new("bbb", 220, new TransportRoller("roller4_6" , "motor4", 0, 15)),
            new("bbb", 260, new TransportRoller("roller4_7" , "motor4", 0, 15)),
            new("bbb", 300, new TransportRoller("roller4_8" , "motor4", 0, 15)),
            new("bbb", 340, new TransportRoller("roller4_9" , "motor4", 0, 15)),
            new("bbb", 380, new TransportRoller("roller4_10", "motor4", 0, 15)),
            new("bbb", 420, new TransportRoller("roller4_11", "motor4", 0, 15)),
            new("bbb", 440, new TransportRoller("roller4_12", "motor4", 0, 15)),
        };


        var transportObjects = new TransportObject[] {
            new("transport obj 1", 150, 150, solenoidOns, Array.Empty<double>(), "aaa", 0),
            new("transport obj 2", 70, 70, solenoidOns, Array.Empty<double>(), "aaa", 200),
            // new("transport obj 1", 150, 150, solenoidOns, Array.Empty<double>(), "aaa", 200),
            // new("transport obj 2", 70, 70, solenoidOns, Array.Empty<double>(), "aaa", 200),

            //Move(transportPaths, new("transport obj 3", 150, solenoidOns, "bbb", 410), 1, junctions, solenoidOns, mergePoints)
            // new("transport obj 1", 80, solenoidOns, "bbb", 450),
            //Move(transportPaths, new("transport obj 2", 150, solenoidOns, "aaa", 10), 500, junctions, solenoidOns, mergePoints)
        };

        var rollerDeltaSteps = (
            from device in transportDevices
            where device.Device is TransportRoller
            let roller = (TransportRoller)device.Device
            //where roller.RollerID.StartsWith("roller1") || roller.RollerID.StartsWith("roller2")// || roller.RollerID.StartsWith("roller3")
            select (roller.RollerID, delta: 5.0))
            .ToDictionary(t => t.RollerID, t => t.delta);

        while (true) {

            var transpotObjPaths = transportObjects
                .Select(obj => (
                            obj.ObjectID,
                            path: CreateTransportObjPath(transportPaths, junctions, obj.SolenoidJunctionOns, mergePoints, obj.PathID, obj.PathPosition, obj.PathLength)
                            .ToArray()));

            var transportObjPoints = transpotObjPaths
                .Select(obj => (
                            obj.ObjectID,
                            (IReadOnlyList<IReadOnlyList<Point>>)obj.path.Select(objPath => (IReadOnlyList<Point>)objPath.path).ToArray()))
                .ToArray();

            var sensorOns = transportDevices
                .Select(x => (x, sensor: x.Device as TransportSensor))
                .Where(t => t.sensor is not null)
                .ToDictionary(
                    x => x.sensor!.SensorID,
                    x => transpotObjPaths.SelectMany(xs => xs.path)
                    // 搬送している物体のパスとセンサーが同じ場所にあるかでセンサーが反応しているかを判定する
                    .Any(path => path.pathID == x.x.PathID &&
                         path.pathPos <= x.x.Position &&
                         x.x.Position <= path.pathPos + path.length));

            var state = new TransportSimulatorModel(transportPaths, junctions, mergePoints, transportDevices, solenoidOns, sensorOns, transportObjPoints);
            Render(canvas, state);
            await Task.Delay(30);

            transportObjects = transportObjects
                .Select(obj => MoveWithRoller(obj, transportPaths, junctions, mergePoints, solenoidOns, transportDevices, rollerDeltaSteps))
                .ToArray();
            // transportObjects = transportObjects
            //     .Select(obj => Move(transportPaths, obj, 5, junctions, solenoidOns, mergePoints))
            //     .ToArray();

        }
    }


    void Render(Canvas canvas, TransportSimulatorModel state) {
        canvas.Children.Clear();

        // 経路の描画
        foreach (var transportPath in state.TransportPaths.Values) {
            canvas.Children.Add(new Polyline() { Points = transportPath.Path.ToArray(), Stroke = Brushes.Black });
        }

        // 搬送用ローラーの描画
        foreach (var t in state.TransportDevices
                 .Select(x => (x, roller: x.Device as TransportRoller))
                 .Where(t => t.roller is not null)) {
            var rollerDispSize = t.roller!.DisplaySize;
            var rollerCircle = new Ellipse() {
                Fill = Brushes.Black,
                Width = rollerDispSize,
                Height = rollerDispSize,
                Margin = new Thickness(-(rollerDispSize / 2), 0)
            };
            canvas.Children.Add(rollerCircle);
            var pos = CreatePathPoint(state.TransportPaths[t.x.PathID].Path, t.x.Position);
            Canvas.SetLeft(rollerCircle, pos.pos.X);
            Canvas.SetTop(rollerCircle, pos.pos.Y);
        }

        // 分岐点の描画
        foreach (var targetJunction in state.Junctions) {
            var junctionPath = state.TransportPaths[targetJunction.SrcPathID];
            var junctionPos = CreatePathPoint(junctionPath.Path, targetJunction.SrcPathPosition);
            var junctionDispSize = 10;
            var circle = new Ellipse() {
                Fill = state.SolenoidJunctionOns[targetJunction.JunctionID] ? Brushes.Blue : Brushes.White,
                StrokeThickness = 3,
                Stroke = Brushes.Blue,
                Width = junctionDispSize,
                Height = junctionDispSize,
                Margin = new Thickness(-(junctionDispSize / 2), -(junctionDispSize / 2))
            };
            canvas.Children.Add(circle);
            Canvas.SetLeft(circle, junctionPos.pos.X);
            Canvas.SetTop(circle, junctionPos.pos.Y);
        }

        // 合流点の描画
        foreach (var transportMerge in state.MergePoints) {
            var transportPath = state.TransportPaths[transportMerge.DestPathID];
            var mergePos = CreatePathPoint(transportPath.Path, transportMerge.DestPathPosition);
            var mergeDispSize = 10;
            var circle = new Ellipse() {
                Fill = Brushes.Green,
                Width = mergeDispSize,
                Height = mergeDispSize,
                Margin = new Thickness(-(mergeDispSize / 2), -(mergeDispSize / 2))
            };
            canvas.Children.Add(circle);
            Canvas.SetLeft(circle, mergePos.pos.X);
            Canvas.SetTop(circle, mergePos.pos.Y);
        }

        // センサーの描画
        foreach (var t in state.TransportDevices
                 .Select(x => (x, sensor: x.Device as TransportSensor))
                 .Where(t => t.sensor is not null)) {
            var sensorDispSize = 10;
            var sensorCircle = new Ellipse() {
                Fill = state.SensorOns[t.sensor!.SensorID] ? Brushes.Orange : Brushes.White,
                StrokeThickness = 2,
                Stroke = Brushes.Orange,
                Width = sensorDispSize,
                Height = sensorDispSize,
                Margin = new Thickness(-(sensorDispSize / 2), -(sensorDispSize / 2))
            };
            canvas.Children.Add(sensorCircle);
            var pos = CreatePathPoint(state.TransportPaths[t.x.PathID].Path, t.x.Position);
            Canvas.SetLeft(sensorCircle, pos.pos.X);
            Canvas.SetTop(sensorCircle, pos.pos.Y);
        }

        // 物体の描画
        foreach (var paths in state.TransportObjectPoints) {
            foreach (var path in paths.Points) {
                canvas.Children.Add(new Polyline() { Points = path.ToArray(), Stroke = Brushes.Red, StrokeThickness = 3 });
            }
        }
    }
}
