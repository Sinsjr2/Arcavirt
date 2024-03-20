using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace TransportSimulatorAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Render(canvas);
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
                return result.Append((path, transportPath.PathID, srcPathPos, pathLength)).ToArray();
            }
            return new[] { (path, srcPathID, srcPathPos, length) };
        }
        else {
            // 分岐点はなかったので、のこりのパスの長さを計測し、与えれれた長さのパスを返せるか計算
            var path = CreatePathLine(transportPath.Path.ToArray(), length, srcPathPos);
            var pathLength = CalcPathLength(path);
            if (pathLength < length) {
                // 指定した長さよりもパスが短かったので、合流した先のパスも長さに含める
                // 1つの終端に複数の合流点があることを想定していない
                var mergePoint = mergePoints
                    .Where(x => x.SrcPathIDs.Any(x2 => x2 == transportPath.PathID))
                    .FirstOrDefault();
                if (mergePoint is not null) {
                    var result = CreateTransportObjPath(
                        transportPaths, solenoidJunctions, solenoidOns, mergePoints, mergePoint.DestPathID,
                        mergePoint.DestPathPosition, length - pathLength);
                    return result.Append((path, transportPath.PathID, srcPathPos, pathLength)).ToArray();
                }
            }
            // 指定した長さのパスが見つかったので処理を終了する
            // もしくは、長さを満たさなかったが、パスの続きがなかったので終了
            return new[] { (path , transportPath.PathID, srcPathPos, pathLength) };
        }
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
            .First();

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


    void Render(Canvas canvas) {

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

            new("aaa", 100, new TransportRoller("motor1", 0, 15)),
            new("aaa", 130, new TransportRoller("motor1", 0, 15)),

            new("aaa", 170, new TransportRoller("motor2", 0, 15)),
            new("aaa", 200, new TransportRoller("motor2", 0, 15)),
            new("aaa", 240, new TransportRoller("motor2", 0, 15)),
            new("aaa", 280, new TransportRoller("motor2", 0, 15)),
            new("aaa", 320, new TransportRoller("motor2", 0, 15)),
            new("aaa", 360, new TransportRoller("motor2", 0, 15)),

            new("aaa", 400, new TransportRoller("motor3", 0, 15)),
            new("aaa", 440, new TransportRoller("motor3", 0, 15)),

            new("bbb",  15, new TransportRoller("motor4", 0, 15)),
            new("bbb",  60, new TransportRoller("motor4", 0, 15)),
            new("bbb", 100, new TransportRoller("motor4", 0, 15)),
            new("bbb", 140, new TransportRoller("motor4", 0, 15)),
            new("bbb", 180, new TransportRoller("motor4", 0, 15)),
            new("bbb", 220, new TransportRoller("motor4", 0, 15)),
            new("bbb", 260, new TransportRoller("motor4", 0, 15)),
            new("bbb", 300, new TransportRoller("motor4", 0, 15)),
            new("bbb", 340, new TransportRoller("motor4", 0, 15)),
            new("bbb", 380, new TransportRoller("motor4", 0, 15)),
            new("bbb", 420, new TransportRoller("motor4", 0, 15)),
            new("bbb", 440, new TransportRoller("motor4", 0, 15)),
        };

        var transportObjects = new TransportObject[] {
            // new("transport obj 1", 80, solenoidOns, "bbb", 300),
            Move(transportPaths, new("transport obj 2", 150, solenoidOns, "aaa", 10), 500, junctions, solenoidOns, mergePoints)
        };

        var transpotObjPaths = transportObjects
            .Select(obj => CreateTransportObjPath(transportPaths, junctions, obj.SolenoidJunctionOns, mergePoints, obj.PathID, obj.PathPosition, obj.Length)
            .ToArray());

        var transportObjPoints = transpotObjPaths
            .Select(obj => obj.Select(objPath => objPath.path));

        var sensorOns = transportDevices
            .Select(x => (x, sensor: x.Device as TransportSensor))
            .Where(t => t.sensor is not null)
            .ToDictionary(
                x => x.sensor!.SensorID,
                x => transpotObjPaths.SelectMany(xs => xs)
                // 搬送している物体のパスとセンサーが同じ場所にあるかでセンサーが反応しているかを判定する
                .Any(path => path.pathID == x.x.PathID &&
                     path.pathPos <= x.x.Position &&
                     x.x.Position <= path.pathPos + path.length));

        // 経路の描画
        foreach (var transportPath in transportPaths.Values) {
            canvas.Children.Add(new Polyline() { Points = transportPath.Path.ToArray(), Stroke = Brushes.Black });
        }

        // 搬送用ローラーの描画
        foreach (var t in transportDevices
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
            var pos = CreatePathPoint(transportPaths[t.x.PathID].Path, t.x.Position);
            Canvas.SetLeft(rollerCircle, pos.pos.X);
            Canvas.SetTop(rollerCircle, pos.pos.Y);
        }

        // 分岐点の描画
        foreach (var targetJunction in junctions) {
            var junctionPath = transportPaths[targetJunction.SrcPathID];
            var junctionPos = CreatePathPoint(junctionPath.Path, targetJunction.SrcPathPosition);
            var junctionDispSize = 10;
            var circle = new Ellipse() {
                Fill = solenoidOns[targetJunction.JunctionID] ? Brushes.Blue : Brushes.White,
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
        foreach (var transportMerge in mergePoints) {
            var transportPath = transportPaths[transportMerge.DestPathID];
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
        foreach (var t in transportDevices
                 .Select(x => (x, sensor: x.Device as TransportSensor))
                 .Where(t => t.sensor is not null)) {
            var sensorDispSize = 10;
            var sensorCircle = new Ellipse() {
                Fill = sensorOns[t.sensor!.SensorID] ? Brushes.Orange : Brushes.White,
                StrokeThickness = 2,
                Stroke = Brushes.Orange,
                Width = sensorDispSize,
                Height = sensorDispSize,
                Margin = new Thickness(-(sensorDispSize / 2), -(sensorDispSize / 2))
            };
            canvas.Children.Add(sensorCircle);
            var pos = CreatePathPoint(transportPaths[t.x.PathID].Path, t.x.Position);
            Canvas.SetLeft(sensorCircle, pos.pos.X);
            Canvas.SetTop(sensorCircle, pos.pos.Y);
        }

        // 物体の描画
        foreach (var paths in transportObjPoints) {
            foreach (var path in paths) {
                canvas.Children.Add(new Polyline() { Points = path, Stroke = Brushes.Red });
            }
        }
    }
}
