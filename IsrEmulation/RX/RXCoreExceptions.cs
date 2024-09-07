namespace RX;

/// <summary>
/// RX コアで発生する例外(割り込み)のベクタのオフセットを表します。
/// </summary>
public enum RXCoreExceptions : byte {
    PrivilegedInstructionException = 0x50,
    AccessException = 0x54,
    UndefinedInstructionException = 0x5C,
    FloatingPointException = 0x64,
    NMIException = 0x78,
}
