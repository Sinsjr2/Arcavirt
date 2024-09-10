namespace GPIO;

/// <summary>
/// マルチプレクサ
/// 指定された信号を選択します。
/// </summary>
public static class Multiplexer {

    /// <summary>
    /// selectSignal により選択する信号が変化します。
    /// false => x1 を選択
    /// true => x2 を選択
    /// </summary>
    public static bool Select(bool selectSignal, bool x1, bool x2) => selectSignal ? x2 : x1;

    /// <summary>
    /// selectSignal により選択する信号が変化します。
    /// 0 => x1 を選択
    /// 1 => x2 を選択
    /// ビット演算で最大32ビット分同時に計算します。
    /// 以下は真理値表です。
    /// s: selectSignal
    /// 1: x1
    /// 2: x2
    /// r: return 戻り値
    /// s 1 2 r
    /// 0 0 0 0
    /// 0 0 1 0
    /// 0 1 1 1
    /// 0 1 0 1
    /// 1 1 0 0
    /// 1 1 1 1
    /// 1 0 1 1
    /// 1 0 0 0
    /// </summary>
    public static int Select(int selectSignal, int x1, int x2) {
        var s = selectSignal;
        return (~s & x1) |
            (s & x2);
    }
}
