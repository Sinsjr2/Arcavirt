using System;
using System.Numerics;

public class QuaternionUtil {

    /// <summary>
    /// XY平面上の角度に変換します。
    /// </summary>
    public static float Get2DRotationFromQuaternion(Quaternion q) {
        // 正規化（数値誤差対策）
        var normalized = Quaternion.Normalize(q);

        // 回転行列に変換
        Matrix4x4 m = Matrix4x4.CreateFromQuaternion(normalized);

        // XY平面上の向き（atan2で角度取得）
        float angle = MathF.Atan2(m.M21, m.M11); // ラジアン
        return angle;
    }
}