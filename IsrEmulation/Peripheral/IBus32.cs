namespace Pheripheral; 

/// <summary>
/// size はバイト数
/// エンディアンは実装しているクラス依存です。
/// </summary>
public interface IBus32 {
    uint Read(uint address, int size);
    void Write(uint address, int size, uint value);
}
