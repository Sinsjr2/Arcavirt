namespace Pheripheral; 

public interface IPheripheral {
    uint ReadUint32(uint offset);
    void WriteUint32(uint offset, uint value);
}
