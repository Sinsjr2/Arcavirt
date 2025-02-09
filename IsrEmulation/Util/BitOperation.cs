namespace Util;

/// <summary>
/// ビット操作関連のユーティリティメソッドを含みます。
/// </summary>
public static class BitOperation {

    /// <summary>
    /// 下byteを任意のバイト長で取り出します。
    /// </summary>
    public static uint GetLowerBits(uint x, uint byteLength) {
        if (!(byteLength <= 4)) {
            throw new ArgumentException($"{nameof(byteLength)} <= 4, actual: {byteLength}", nameof(byteLength));
        }
        var shift = (4 - (int)byteLength) * 8;
        return (x << shift) >> shift;
    }
}
