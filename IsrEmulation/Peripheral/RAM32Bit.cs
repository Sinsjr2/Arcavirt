using System.Buffers.Binary;

namespace Pheripheral {

    // TODO メモリのアライメント違反を検出するかどうかを切り替えられるようにする
    public class RAM32Bit : IBus32 {
        readonly string name;
        readonly byte[] memory;

        public RAM32Bit(string name, int memorySize) {
            this.memory = new byte[memorySize];
            this.name = name;
        }

        public uint Read(uint address, int size) {
            if (memory.Length <= address) {
                throw new ArgumentOutOfRangeException(nameof(address), $"name: {name} memory size: 0x{memory.Length:X} addr: 0x{address:X}");
            }
            return size switch {
                1 => memory[address],
                2 => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan((int)address, 2)),
                4 => BinaryPrimitives.ReadUInt32LittleEndian(memory.AsSpan((int)address, 4)),
                _ => throw new ArgumentException($"invalid size access. name: {name} addr: 0x{address:X} size:{size}", nameof(size))
            };
        }

        public void Write(uint address, int size, uint value) {
            if (memory.Length <= address) {
                throw new ArgumentOutOfRangeException(nameof(address), $"name: {name} memory size: 0x{memory.Length:X} addr: 0x{address:X}");
            }
            switch(size) {
                case 1:
                    memory[address] = (byte)value;
                    break;
                case 2:
                    BinaryPrimitives.WriteUInt16LittleEndian(memory.AsSpan().Slice((int)address), (ushort)value);
                    break;
                case 4:
                    BinaryPrimitives.WriteUInt32LittleEndian(memory.AsSpan().Slice((int)address), value);
                    break;
                default:
                    throw new ArgumentException($"invalid size access. name: {name} addr: 0x{address:X} size:{size}", nameof(size));
            };
        }
    }
}
