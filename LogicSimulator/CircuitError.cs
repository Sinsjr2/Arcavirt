namespace LogicSimulator;

/// <summary>回路構築時のエラー種別を表します。</summary>
public enum CircuitErrorKind {
    /// <summary>入力ピンが未接続のままです。</summary>
    UnconnectedInput,

    /// <summary>1つの入力ピンに対して複数の出力ピンが接続されています。</summary>
    MultipleSourceConnections,

    /// <summary>接続されているピン同士のビット幅が一致しません。</summary>
    BitWidthMismatch,

    /// <summary>
    /// InputConnector または OutputConnector のビット幅を接続から推論できませんでした。
    /// </summary>
    UnresolvableConnector,

    /// <summary>
    /// 接続定義に存在しない LogicID・ピン名が参照されているか、
    /// または回路ライブラリに存在しない CustomCircuit 名が指定されています。
    /// </summary>
    InvalidNodeReference,

    /// <summary>回路内で同じ LogicID を持つノードが複数登録されています。</summary>
    DuplicateNodeId,

    /// <summary>
    /// 回路に含まれる ILogicElement の実装が、登録済みのファクトリに存在しません。
    /// </summary>
    UnregisteredLogicElement,

    /// <summary>
    /// スカラーピン（IsIndexed=false）に対してインデックス記法でアクセスしたか、
    /// バスピン（IsIndexed=true）に対して有効範囲外インデックスでアクセスした。
    /// </summary>
    InvalidPinAccess,

    /// <summary>JunctionConnector の InputBits.Sum() と OutputBits.Sum() が一致しません。</summary>
    JunctionBitSumMismatch,
}

public record CircuitError(
    string NodeId,
    string? PinName,
    CircuitErrorKind Kind,
    string Message
);
