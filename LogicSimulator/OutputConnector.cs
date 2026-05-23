namespace LogicSimulator;

/// <summary>
/// 回路一式をコンポーネントとして使い回す時に外部と接続するための出力コネクタ。
/// <para>
/// <see cref="DataBits"/> を省略した場合は、<see cref="LogicSimulation.TryBuild"/> 時に
/// 接続元ピンのビット幅から自動推論されます。
/// 明示指定した場合は接続元ピンとの整合性チェックが行われ、不一致があれば
/// <see cref="CircuitErrorKind.BitWidthMismatch"/> エラーとして報告されます。
/// </para>
/// </summary>
public record OutputConnector : ILogicElement {
    /// <summary>
    /// データのビット幅。<see langword="null"/> の場合は回路ビルド時に接続元ピンから自動推論されます。
    /// </summary>
    public int? DataBits { get; }
    public OutputConnector(int? dataBits = null) {
        if (dataBits.HasValue && dataBits.Value <= 0)
            throw new ArgumentException(
                $"OutputConnector DataBits must be greater than 0, but was {dataBits.Value}.");
        DataBits = dataBits;
    }
}
