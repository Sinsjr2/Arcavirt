using System;

/// <summary>
/// 台形駆動の加速カーブを計算します。
/// </summary>
public class TrapezoidalDriveCurve {
    /// <summary>
    /// 時刻tにおける速度を計算します。
    /// </summary>
    public static float V(float t, float startVelocity, float acc) {
        return (float)(startVelocity + acc * t);
    }

    /// <summary>
    /// doc 時刻tにおける位置を返します。
    /// </summary>
    public static float X(float t, float startVelocity, float acc) {
        // 台形の計算式を用いる
        var pos = (startVelocity + V(t, startVelocity, acc)) * t / 2;
        return (float)pos;
    }

    /// <summary>
    /// 2次方程式の解の公式
    /// 正の値の方だけを返します。
    /// </summary>
    static double SolutionOfQuadraticQquation(double a, double b, double c) {
        return (-b + Math.Sqrt(b * b - 4 * a * c)) / (2.0 * a);
    }

    /// <summary>
    /// 指定された速度に到達するまでにかかる時間を返します。
    /// 減速するときは、加速度が負の値になっていることを想定しています。
    /// </summary>
    public static float TEnd(float startV, float acc, float targetV) {
        return (targetV - startV) / acc;
    }

    /// <summary>
    /// 指定された位置に到達するときの最大速度
    /// acc 加速度
    /// dec 減速時の加速度
    /// targetV 目標速度
    /// d 移動距離
    /// </summary>
    // public static float vMax(float startV, float acc, float dec, float targetV, float d) {
    //     var sinAcc = acc / Math.Sqrt(1.0 + acc * acc);
    //     var sinDec = dec / Math.Sqrt(1.0 + dec * dec);

    //     // https://keisan.casio.jp/exec/system/1204613833
    //     // 以上の計算式を参考に2つの角度と面積から底辺の長さを求めている
    //     var radAcc = Math.Atan(acc);
    //     var radDec = Math.Atan(dec);
    //     // 面積と角度(加速度、減速度)から三角形の底辺の長さを求めます。
    //     // 三角形の下に矩形部分(起動速度)を設けています。
    //     // 台形駆動をした時最大速度まで加速した後すぐに減速すると加速度が2倍になるので、それを防ぐために1引いている
    //     var l = SolutionOfQuadraticQquation(sinAcc * sinDec / (Math.Sin(radAcc + radDec) * 2.0), startV, -(d - 1.0));

    //     var tanAcc = acc;
    //     var tanDec = dec;
    //     // 全区間が台形駆動した場合の最大速度
    //     var maxTriangle = l * (tanAcc * tanDec) / (tanAcc + tanDec);

    //     // 目標速度以上は加速させない
    //     return (float)Math.Min(maxTriangle, targetV);
    // }

    // 指定した速度まで加速出来ない場合の等速で移動する距離を指定します。
    public static float vMax(float startV, float endV, float acc, float dec, float targetV, float d, float constSpeedDistance) {
        var triangleTop = Math.Sqrt((startV * startV / acc + endV * endV / dec + 2 * (d - constSpeedDistance)) * (acc * dec / (acc + dec)));
        return (float)Math.Min(targetV, triangleTop);
    }
}

/// <summary>
/// 加速 等速 減速 のいずれかを表します。
/// </summary>
public enum AccelationKind {
    Acc,
    /// <summary>
    /// 等速
    /// </summary>
    Uniform,
    Dec
}

/// <summary>
/// 各区間の開始してからの時間を表します。
/// </summary>
public struct AccelationTimeResult {
    public readonly AccelationKind Kind;
    public readonly float Time;

    public AccelationTimeResult(AccelationKind kind, float time) {
        Kind = kind;
        Time = time;
    }
}

/// <summary>
/// 軌道曲線の計算したパラメータを保持するために使用します。
/// </summary>
public record struct DriveParameter(
    // 起動速度
    float StartVelocity,
    // 停止完了とする速度
    float EndVelocity,
    float MaxSpeed,
    // 加速度
    float Acc,
    // 加速が終了する時間
    float AccTime,
    // 加速して移動する距離
    float AccDistance,
    // 減速加速度
    float Dec,
    // 減速が終了する時間
    float DecTime,
    // 減速して移動する距離
    float DecDistance,
    // 等速移動の時間
    float ConstSpeedTime,
    // 等速移動する距離
    float ConstSpeedDistance
) {
    /// <summary>
    /// 等速運動が終了する時刻
    /// </summary>
    public readonly float UniformMotionEndTime = AccTime + ConstSpeedTime;

    /// <summary>
    /// 動作が終了する時刻
    /// </summary>
    public readonly float EndTime = AccTime + ConstSpeedTime + DecTime;

    public AccelationTimeResult GetAccSection(float time) {
        if (time < AccTime) {
            return new AccelationTimeResult(AccelationKind.Acc, time);
        }
        if (time <= UniformMotionEndTime) {
            return new AccelationTimeResult(AccelationKind.Uniform, time - AccTime);
        }
        return new AccelationTimeResult(AccelationKind.Dec, time - UniformMotionEndTime);
    }
}

/// <summary>
/// 加速カーブの計算を行います。
/// </summary>
public class CurveDesigner {

    public static DriveParameter CalcParameter(float startV, float endV, float acc, float dec, float targetV, float d, float minConstSpeedDistance) {
        var maxSpeed = TrapezoidalDriveCurve.vMax(startV, endV, acc, dec, targetV, d, minConstSpeedDistance);
        var accTime = TrapezoidalDriveCurve.TEnd(startV, acc, maxSpeed);
        var decTime = TrapezoidalDriveCurve.TEnd(endV, dec, maxSpeed);
        var accDistance = TrapezoidalDriveCurve.X(accTime, startV, acc);
        var decDistance = TrapezoidalDriveCurve.X(decTime, maxSpeed, -dec);
        var constSpeedDistance = d - (accDistance + decDistance);
        var constSpeedTime = constSpeedDistance / maxSpeed;

        return new DriveParameter(
            StartVelocity: startV,
            EndVelocity: endV,
            MaxSpeed: maxSpeed,
            Acc: acc,
            AccTime: accTime,
            AccDistance: accDistance,
            Dec: dec,
            DecTime: decTime,
            DecDistance: decDistance,
            ConstSpeedTime: constSpeedTime,
            ConstSpeedDistance: constSpeedDistance);
    }

    /// <summary>
    /// 時刻tにおける速度を計算します。
    /// d 移動距離
    /// </summary>
    public static float V(DriveParameter param, float t) {
        var section = param.GetAccSection(t);
        return section.Kind switch {
            AccelationKind.Acc => TrapezoidalDriveCurve.V(section.Time, param.StartVelocity, param.Acc),
            AccelationKind.Uniform => param.MaxSpeed,
            AccelationKind.Dec => Math.Max(param.EndVelocity, TrapezoidalDriveCurve.V(section.Time, param.MaxSpeed, -param.Dec)),
            _ => throw new NotSupportedException()
        };
    }


    /// <summary>
    /// 時刻tにおける位置を計算します。
    /// </summary>
    public static float X(float t, float startV, float endV, float acc, float dec, float targetV, float d, float constSpeedDistance) {
        var param = CalcParameter(startV, endV, acc, dec, targetV, d, constSpeedDistance);
        return t < param.AccTime ? TrapezoidalDriveCurve.X(t, startV, acc)
            : t <= param.AccTime + param.ConstSpeedTime ? param.AccDistance + param.MaxSpeed * (t - param.AccTime)
            : param.AccDistance + param.ConstSpeedDistance + TrapezoidalDriveCurve.X(t - (param.AccTime + param.ConstSpeedTime), param.MaxSpeed, -dec);
    }

    // /// <summary>
    // /// 指定した距離の搬送が完了するまでにかかる時間を返します。
    // /// </summary>
    // public static float End(float startV, float endV, float acc, float dec, float targetV, float d, float constSpeedDistance) {
    //     var param = CalcParameter(startV, endV, acc, dec, targetV, d, constSpeedDistance);
    //     return param.AccTime + param.ConstSpeedTime + param.DecTime;
    // }
}


// public class Program {

//     record struct Vec2D(float X, float Y);

//     public static void GenerateMotion() {
//         var d = 4000;
//         var startV = 1000;
//         var endV = 1000;
//         var targetV = 12000;
//         var acc = 100000;
//         var dec = 200000;
//         var minConstDistance = 1;

//         var endTime = CurveDesigner.End(startV, endV, acc, dec, targetV, d, minConstDistance);

//         var datas = new List<(double time, double speedHz, double pos)>();
//         var time = 0.0f;
//         int step = 0;
//         while (time <= endTime) {
//             step++;
//             var speedHz = CurveDesigner.V(time, startV, endV, acc, dec, targetV, d, minConstDistance);
//             var pos = CurveDesigner.X(time, startV, endV, acc, dec, targetV, d, minConstDistance);
//             datas.Add((time, speedHz, pos));
//             time += 1.0f / speedHz;
//         }

//         GnuPlot.HoldOn();
//         GnuPlot.Plot(datas.Select(t => t.time).ToArray(), datas.Select(t => t.speedHz).ToArray(), "with lines");
//         GnuPlot.Plot(datas.Select(t => t.time).ToArray(), datas.Select(t => t.pos).ToArray(), "with lines");
//         Console.WriteLine($"endTime: {endTime}ms");
//         Console.WriteLine($"total Step: {step}");

//         Console.ReadLine();
//     }

//     public static void Main() {
//         new PDOAnalizer().Create();
//     }
// }
