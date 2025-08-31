namespace Peripheral.Renesas;

/// <summary>
/// デバッガー経由で通信する時に使用するポート
/// </summary>
public class IceDebug {

    /// <summary>
    /// Debug Virtual Console TX data
    /// </summary>
    public readonly RegisterValue32<uint> TxData;

    /// <summary>
    /// Debug Virtual Console RX data
    /// </summary>
    public readonly RegisterValue32<uint> RxData;

    /// <summary>
    /// Debug Virtual Console Status
    /// </summary>
    public readonly RegisterValue32<uint> ControlStatus;

    public IceDebug() {
        TxData = new(0, WriteTxData);
        RxData = new(0);
        ControlStatus = new(0);
    }

    void WriteTxData(RegisterValue32<uint> reg, uint value) {
        reg.Value = value;
        // TODO 任意の出力先に切り替えられるようにする また、送信中に待つことも出来るようにする
        Console.Write((char)(byte)value);
    }
}

public class IceDebugMapping {

    /// <summary>
    /// 全CPU共通のデバッグポート
    /// </summary>
    public static IReadOnlyCollection<Register32MappingInfo> CreateMapping(IceDebug iceDebug) {
        var mappingInfos = new Register32MappingInfo[] {
            new(0, iceDebug.TxData),
            new(16, iceDebug.RxData),
            new(64, iceDebug.ControlStatus)
        };
        return mappingInfos;
    }
}