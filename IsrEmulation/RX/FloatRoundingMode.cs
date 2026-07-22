namespace RX; 
/// <summary>
/// 浮動小数点の丸め方
/// </summary>
public enum RXFloatRoundingMode : byte {
    /// <summary>
    /// 最近値への丸め
    /// </summary>
    NearestEven = 0b00,
    /// <summary>
    /// 0 方向への丸め
    /// </summary>
    Zero = 0b01,
    /// <summary>
    /// + ∞方向への丸め
    /// </summary>
    PulseInf = 0b10,
    /// <summary>
    /// – ∞方向への丸め
    /// </summary>
    MinusInf = 0b11
}
