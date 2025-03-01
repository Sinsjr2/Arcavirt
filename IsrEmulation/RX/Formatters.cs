using System.Buffers;
using System.Buffers.Binary;
using Pheripheral;

namespace RX {

    /// <summary>
    /// リトルエンディアンで読み出します。
    /// </summary>
    public ref struct Reader {

        readonly IBus32 bus;
        public uint Position { private set; get; }

        public Reader(IBus32 bus, uint pos) {
            this.bus = bus;
            this.Position = pos;
        }

        public sbyte ReadI8() {
            return (sbyte)ReadU8();
        }

        public byte ReadU8() {
            var result = (byte)bus.Read(Position, 1);
            Position++;
            return result;
        }

        public byte FetchU8() {
            return (byte)bus.Read(Position, 1);
        }

        public short ReadI16() {
            return (short)ReadU16();
        }

        public ushort ReadU16() {
            var result = (ushort)bus.Read(Position, 2);
            Position += 2;
            return result;
        }

        public ushort FetchU16() {
            return (ushort)bus.Read(Position, 2);
        }

        public int ReadI32() {
            return (int)ReadU32();
        }

        public uint ReadU32() {
            var result = bus.Read(Position, 4);
            Position += 4;
            return result;
        }

        public uint FetchU32() {
            return bus.Read(Position, 4);
        }

        /// <summary>
        /// 符号あり整数として読み込みます。
        /// </summary>
        public uint ReadInteger(int size) {
            switch (size) {
                case 1: {
                    int immediate = ReadI8();
                    return (uint)immediate;
                }
                case 2: {
                    int immediate = ReadI16();
                    return (uint)immediate;
                }
                case 3: {
                    int immediate = (int)ReadI8() | ((int)ReadI8() << 8) | ((int)ReadI8() << 16);
                    return (uint)immediate;
                }
                case 4: {
                    int immediate = ReadI32();
                    return (uint)immediate;
                }
                default:
                        throw new InvalidOperationException($"size is not supported {size}");
            }
        }

        /// <summary>
        /// 4バイト以下のサイズを読み込みます。
        /// </summary>
        public uint ReadUInteger(int size) {
            switch (size) {
                case 1: {
                    int immediate = ReadU8();
                    return (uint)immediate;
                }
                case 2: {
                    int immediate = ReadU16();
                    return (uint)immediate;
                }
                case 3: {
                    uint immediate = ReadU8() | ((uint)ReadU8() << 8) | ((uint)ReadU8() << 16);
                    return (uint)immediate;
                }
                case 4: {
                    return ReadU32();
                }
                default:
                        throw new InvalidOperationException($"size is not supported {size}");
            }
        }

        public uint FetchUInteger(int size) {
            switch (size) {
                case 1: {
                    int immediate = FetchU8();
                    return (uint)immediate;
                }
                case 2: {
                    int immediate = FetchU16();
                    return (uint)immediate;
                }
                case 3: {
                    uint immediate = bus.Read(Position, 1) | (bus.Read(Position + 1, 1) << 8) | (bus.Read(Position + 2, 1) << 16);
                    return (uint)immediate;
                }
                case 4: {
                    return FetchU32();
                }
                default:
                    throw new InvalidOperationException($"size is not supported. {size}");
            }
        }

        public int FetchInteger(int size) {
            switch (size) {
                case 1: {
                    return (byte)bus.Read(Position, 1);
                }
                case 2: {
                    return (short)bus.Read(Position, 2);
                }
                case 3: {
                    int top = (byte)bus.Read(Position + 2, 1);
                    uint immediate = bus.Read(Position, 1) | (bus.Read(Position + 1, 1) << 8) | (((uint)top) << 16);
                    return (int)immediate;
                }
                case 4: {
                    return (int)bus.Read(Position, 4);
                }
                default:
                    throw new InvalidOperationException($"size is not supported. {size}");
            }
        }

    }

    public ref struct AssemblyWriter {

        readonly IBufferWriter<byte> writer;

        public AssemblyWriter(IBufferWriter<byte> writer) {
            this.writer = writer;
        }

        // public void SetMaskedU8(byte mask, byte value) {
        //     throw new NotImplementedException();
        // }

        // public void WriteU8(byte value) {
        //     throw new NotImplementedException();
        // }

        // public void SetMaskedI8(sbyte mask, sbyte value) {
        //     throw new NotImplementedException();
        // }

        // public void WriteI8(sbyte value) {
        //     throw new NotImplementedException();
        // }

        // public void SetMaskedI16(short mask, short value) {
        //     throw new NotImplementedException();
        // }


        // public void WriteI16(short value) {
        //     throw new NotImplementedException();
        // }

        // public void SetMaskedU32(uint mask, uint value) {
        //     SetMaskedUInteger(4, mask, value);
        // }

        /// <summary>
        /// バッファにマスク掛けた上で値を書き込みます。
        /// リトルエンディアンとして書き込みます。
        /// 書き進めません。
        /// </summary>
        public void SetMaskedUInteger(int size, uint mask, uint value) {
            if (size is  < 0 or > 4) {
                throw new ArgumentException($"0 <= {nameof(size)} <= 4. actual: {size} ", nameof(size));
            }
            var currentBuffer = writer.GetSpan(size);
            uint current = 0;
            for (int i = 0; i < size; i++) {
                current |= (uint)currentBuffer[i] << (i * 8);
            }
            var masked = (current & ~mask) | (value & mask);
            for (int i = 0; i < size; i++) {
                currentBuffer[i] = (byte)((masked >> (i * 8)) & 0xFF);
            }
        }

        // /// <summary>
        // /// バッファにマスク掛けた上で値を書き込みます。
        // /// 書き進めます。
        // /// </summary>
        // public void WriteMaskedU32(uint mask, uint value) {
        //     throw new NotImplementedException();
        // }

        /// <summary>
        /// 4バイト以下で指定されたバイト数(下の桁)を書き込みます。
        /// </summary>
        public void WriteUInteger(int size, uint value) {
            SetMaskedUInteger(size, 0xFFFFFFFF, value);
            writer.Advance(size);
        }

        public void WriteMaskedUInteger(int size, uint mask, uint value) {
            SetMaskedUInteger(size, mask, value);
            writer.Advance(size);
        }

        /// <summary>
        /// バッファに対して何も操作せずに書き込み位置を進めます。
        /// </summary>
        public void Skip(int offset) {
            writer.Advance(offset);
        }
    }

    public interface IAssemblyCode32Formatter {
        void Serialize(Queue<uint> result, ref AssemblyWriter writer);
        void Deserialize(ref Reader reader, Queue<uint> result);
    }

    // public interface IOpCode32 : IAssemblyCode32Formatter {
    //     uint OpCodeKind { get; }
    //     uint Mask { get; }
    //     uint OpCode { get; }
    //     string Name { get; }
    // }

    public record struct OpCode32Value(uint Mask, uint OpCode);

    /// <summary>
    /// 読み進めません。
    /// </summary>
    public class OpCode32/* : IOpCode32*/ {
        // public uint Mask { get; }
        // public uint OpCode { get; }
        public readonly string Name;

        /// <summary>
        /// オペコードをデコードする時に使用します。
        /// </summary>
        public readonly IReadOnlyList<OpCode32Value> DecodeOpCodes;

        /// <summary>
        /// OpCodeKind から変換する時に使用します。
        /// </summary>
        public readonly OpCode32Value EncodeOpCode;

        /// <summary>
        /// デコードした時にオペコードを識別するためにresultに格納するために使用します。
        /// または、エンコードするときにどの識別するために使用します。
        /// </summary>
        public uint OpCodeKind { get; }

        /// <summary>
        /// opcode は 「 」「.」、「0」、1の3つの文字列を使用して、ビッグエンディアンで記述します。
        /// 「.」はマスクするビットを表します。
        /// 「0」と「1」は一致するパターンを記述するために使用します。
        /// 「 」 は無視します。区切りめがわかりやすいようにするために使用します。
        ///
        /// decodeOpCodes
        /// 同じ位置にマスクがあるオペコードの場合、複数同じ opCodeKind を登録する場合があります。
        /// しかし、 opCodeKind からバイナリ表現を取得できなくなるので個別に設定できるようにしています。
        /// 空の場合は、 opcode と同じになります。
        /// </summary>
        public OpCode32(string name, uint opCodeKind, string opcode, params string[] decodeOpCodes) {
            var strippedOpecode = opcode
                .Replace(" ", "");

            if (32 < strippedOpecode.Length) {
                throw new ArgumentException($"too long(32 < length). actual opcode: {strippedOpecode}");
            }

            EncodeOpCode = FromString(opcode);
            DecodeOpCodes = decodeOpCodes.Any()
                ? decodeOpCodes.Select(FromString).ToArray()
                : new[] { FromString(opcode) };


            OpCodeKind = opCodeKind;
            Name = name;
        }

        static OpCode32Value FromString(string code) {
            var strippedCode = code
                .Replace(" ", "");

            if (32 < strippedCode.Length) {
                throw new ArgumentException($"too long(32 < length). actual code: {strippedCode}");
            }

            uint codeID = 0;
            uint codeMask = 0;
            for (int i = 0; i < strippedCode.Length; i++) {
                int shift = 31 - i;
                switch (strippedCode[i]) {
                    case '.':
                        break;
                    case '0':
                        codeMask |= 1u << shift;
                        break;
                    case '1':
                        codeMask |= 1u << shift;
                        codeID |= 1u << shift;
                        break;
                    default:
                        throw new ArgumentException($"{code} is invalid.");
                }
            }
            return new OpCode32Value(
                BinaryPrimitives.ReverseEndianness(codeMask),
                BinaryPrimitives.ReverseEndianness(codeID));
        }
    }

    public record OpCodePair32(OpCode32 OpCode, IAssemblyCode32Formatter Operand);

    /// <summary>
    /// オぺコードと一致する <see name="OpCode32Formatter" /> を検索します。
    /// </summary>
    public class OpCode32PatternMatchFormatter : IAssemblyCode32Formatter {
        /// <summary>
        /// アセンブラを解析する時に使用します。
        /// </summary>
        readonly OpCodePair32[]?[] formatters;

        /// <summary>
        /// シリアライズする時に使用します。
        /// </summary>
        readonly IReadOnlyDictionary<uint, OpCodePair32> opcodeKindToFormatter;

        /// <summary>
        /// オペコードがformattersにない場合の識別子として使用します。
        /// </summary>
        readonly uint unknownOpcodeKind;

        /// <summary>
        /// マスク値で有効なバイト数を取得します。
        /// </summary>
        static int GetOpcodeLength(uint opcodeMask) {
            int bitLength = 0;
            for (int i = 0; i < 32; i++) {
                var mask = 1u << i;
                if ((opcodeMask & mask) != 0) {
                    bitLength = i;
                }
            }
            return bitLength == 0
                ? 0
                // 切り上げる
                : (bitLength + 7) / 8;
        }

        public OpCode32PatternMatchFormatter(uint unknownOpcodeKind, IReadOnlyList<OpCodePair32> formatters_) {
            formatters = new OpCodePair32[ushort.MaxValue + 1][];
            var formatterArray = formatters_.ToArray();
            int twoLevelCount = 0;
            for (uint i = 0; i < formatters.Length; i++) {
                bool twolevel = false;
                foreach (var pair in formatters_) {
                    uint opcode = i;
                    foreach (var instraction in pair.OpCode.DecodeOpCodes) {
                        if ((opcode & instraction.Mask) == (instraction.OpCode & 0xffff)) {
                            if (GetOpcodeLength(instraction.Mask) > 2) {
                                twoLevelCount++;
                                twolevel = true;
                            } else if (formatters[i] is not null) {
                                throw new ArgumentException(
                                    $"Instruction already exists for code 0x{opcode:X} at {pair.OpCode.Name} {formatters[i]![0].OpCode.Name}");
                            } else {
                                formatters[i] = new[] { pair };
                            }
                        }
                    }
                }
                if (twolevel) {
                    if (formatters[i] is not null) {
                        throw new ArgumentException(
                            $"Twolevel sub table slot already busy: {formatters[i]![0]!.OpCode.Name}");
                    }
                    var subFormatters = new OpCodePair32[byte.MaxValue + 1];
                    formatters[i] = subFormatters;
                    for (int j = 0; j < subFormatters.Length; j++) {
                        var subCode = i | ((uint)j << 16);
                        foreach (var subPair in formatters_) {
                            foreach (var instr in subPair.OpCode.DecodeOpCodes) {
                                if ((subCode & instr.Mask) == instr.OpCode) {
                                    if (GetOpcodeLength(instr.Mask) < 3) {
                                        throw new ArgumentException("Short instruction in 3 byte instr. subtab");
                                    } else {
                                        subFormatters[j] = subPair;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            opcodeKindToFormatter = formatters_
                .ToDictionary(formatter => formatter.OpCode.OpCodeKind);
            this.unknownOpcodeKind = unknownOpcodeKind;
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var opcode = result.Dequeue();
            // 未定義のオペコードが含まれることは、本来はありえないので気が付けるように例外を発生させる
            if (!opcodeKindToFormatter.TryGetValue(opcode, out var formatter)) {
                throw new ArgumentException($"not found. {opcode}");
            }

            writer.SetMaskedUInteger(
                4,
                formatter.OpCode.EncodeOpCode.Mask,
                formatter.OpCode.EncodeOpCode.OpCode);
            formatter.Operand.Serialize(result, ref writer);
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            uint code = reader.FetchU32();
            var sub = formatters[code & 0xFFFF];
            if (sub == null) {
                result.Enqueue(unknownOpcodeKind);
                return;
            }
            if (sub.Length == 1) {
                result.Enqueue(sub[0].OpCode.OpCodeKind);
                sub[0].Operand.Deserialize(ref reader, result);
                return;
            }
            var pair = sub[(code >> 16) & 0xFF];
            result.Enqueue(pair.OpCode.OpCodeKind);
            pair.Operand.Deserialize(ref reader, result);
        }
    }

    /// <summary>
    /// 指定したバイト数を読み進めます。
    /// </summary>
    public class SkipFormatter : IAssemblyCode32Formatter {
        readonly int skipByte;

        public SkipFormatter(int skipByte) {
            // reader に 4バイト以上読み込むためのメソッドが存在しないための正弦
            if (skipByte is < 0 or > 4) {
                throw new ArgumentException($"actual: {skipByte}", nameof(skipByte));
            }
            this.skipByte = skipByte;
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            writer.Skip(skipByte);
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            reader.ReadUInteger(skipByte);
        }
    }

    /// <summary>
    /// 複数のフォーマッターを順番に実行していきます。
    /// </summary>
    public class CompositeFormatter : IAssemblyCode32Formatter {
        IReadOnlyList<IAssemblyCode32Formatter> formatters;

        public CompositeFormatter(IReadOnlyList<IAssemblyCode32Formatter> formatters) {
            this.formatters = formatters;
        }

        public CompositeFormatter(params IAssemblyCode32Formatter[] formatters) {
            this.formatters = formatters;
        }


        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            foreach (var formatter in formatters) {
                formatter.Serialize(result, ref writer);
            }
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            foreach (var formatter in formatters) {
                formatter.Deserialize(ref reader, result);
            }
        }
    }

    /// <summary>
    /// 32ビット以下の符号なし整数として操作します。
    /// 読み進めません。
    /// リトルエンディアンとして扱います。
    /// LE: Little Endian
    /// </summary>
    public class LEUIntegerFormatter : IAssemblyCode32Formatter {
        readonly byte shift;
        readonly byte byteLength;
        readonly uint mask;

        public LEUIntegerFormatter(int shift, int bitLength, int byteLength) {
            if (byteLength is < 0 or > 4) {
                throw new ArgumentException($"actual: {byteLength}", nameof(byteLength));
            }
            if (bitLength is > 32) {
                throw new ArgumentException($"actual: {bitLength}", nameof(bitLength));
            }
            if (shift is < 0 or > 32) {
                throw new ArgumentException($"actual: {shift}", nameof(shift));
            }
            this.shift = (byte)shift;
            this.byteLength = (byte)byteLength;
            this.mask = ((~0u) << (32 - bitLength)) >> (32 - bitLength);
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var value = result.Dequeue();
            writer.SetMaskedUInteger(byteLength, mask << shift, value << shift);
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            var integer = reader.FetchUInteger(byteLength);
            var masked = mask & (integer >> shift);
            result.Enqueue(masked);
        }
    }

    /// <summary>
    /// 32ビット以下の符号あり整数として操作します。
    /// 読み進めません。
    /// リトルエンディアンとして扱います。
    /// LE: Little Endian
    /// </summary>
    public class LEIntegerFormatter : IAssemblyCode32Formatter {
        readonly uint mask;
        readonly byte shift;
        readonly byte byteLength;
        readonly byte bitLength;

        public LEIntegerFormatter(int shift, int bitLength, int byteLength) {
            if (byteLength is < 0 or > 4) {
                throw new ArgumentException($"actual: {byteLength}", nameof(byteLength));
            }
            if (bitLength is > 32) {
                throw new ArgumentException($"actual: {bitLength}", nameof(bitLength));
            }
            if (shift is < 0 or > 32) {
                throw new ArgumentException($"actual: {shift}", nameof(shift));
            }
            this.bitLength = (byte)bitLength;
            this.shift = (byte)shift;
            this.byteLength = (byte)byteLength;
            this.mask = ((~0u) << (32 - bitLength)) >> (32 - bitLength);
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var value = result.Dequeue();
            writer.SetMaskedUInteger(byteLength, mask << shift, value << shift);
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            int integer = (int)reader.FetchUInteger(byteLength);
            var masked = (integer << (32 - (shift + bitLength))) >> (32 - bitLength);
            result.Enqueue((uint)masked);
        }
    }

    /// <summary>
    /// 32ビット以下の符号なし整数として操作します。
    /// 読み進めません。
    /// ビックエンディアンとして扱います。
    /// BE: Big Endian
    /// </summary>
    public class BEUIntegerFormatter : IAssemblyCode32Formatter {
        readonly byte shift;
        readonly byte byteLength;
        readonly uint mask;

        public BEUIntegerFormatter(int shift, int bitLength, int byteLength) {
            if (byteLength is < 0 or > 4) {
                throw new ArgumentException($"actual: {byteLength}", nameof(byteLength));
            }
            if (bitLength is > 32) {
                throw new ArgumentException($"actual: {bitLength}", nameof(bitLength));
            }
            if (shift is < 0 or > 32) {
                throw new ArgumentException($"actual: {shift}", nameof(shift));
            }
            this.shift = (byte)shift;
            this.byteLength = (byte)byteLength;
            this.mask = ((~0u) << (32 - bitLength)) >> (32 - bitLength);
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var value = result.Dequeue();
            writer.SetMaskedUInteger(
                byteLength,
                BinaryPrimitives.ReverseEndianness(mask << shift) >> ((4 - byteLength) * 8),
                BinaryPrimitives.ReverseEndianness(value << shift) >> ((4 - byteLength) * 8));
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            uint integer = BinaryPrimitives.ReverseEndianness(
                reader.FetchUInteger(byteLength))
                >> ((4 - byteLength) * 8);
            var masked = mask & (integer >> shift);
            result.Enqueue(masked);
        }
    }

    /// <summary>
    /// 何もしません。
    /// </summary>
    public class EmptyFormatter : IAssemblyCode32Formatter {
        public static readonly EmptyFormatter Instance = new();

        EmptyFormatter() {}

        public void Deserialize(ref Reader reader, Queue<uint> result) {
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
        }
    }

    /// <summary>
    /// 離れたビット同士をシフト演算することでつなぎ合わせます。
    /// リトルエンディアン符号なしとして演算します。
    ///
    /// 複数のビットの位置が重複することは想定していません。
    /// </summary>
    public class LEUBitConnectionFormatter : IAssemblyCode32Formatter {
        /// <summary>
        /// ビット演算する対象の数値を読み出す長さ
        /// </summary>
        readonly int readLength;

        readonly ReadOnlyMemory<(int shift, uint mask)> calcPatterns;

        public LEUBitConnectionFormatter(
            int readLength,
            (int bitLength, int shift)[] calcPatterns) {

            this.readLength = readLength;
            this.calcPatterns = calcPatterns
                // ビットの長さ分ビットを立てる
                .Select(t => (t.shift, ((~0u) >> (32 - t.bitLength)) << t.shift))
                .ToArray();
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            var x = reader.FetchUInteger(readLength);
            uint shifted = 0;
            foreach (var pattern in calcPatterns.Span) {
                shifted |= (x & pattern.mask) >> pattern.shift;
            }
            result.Enqueue(shifted);
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var x = result.Dequeue();
            uint mask = 0;
            uint expanded = 0;
            foreach (var pattern in calcPatterns.Span) {
                expanded |= (x << pattern.shift) & pattern.mask;
                mask |= pattern.mask;
            }
            writer.WriteMaskedUInteger(readLength, mask, expanded);
        }
    }

        /// <summary>
    /// 離れたビット同士をシフト演算することでつなぎ合わせます。
    /// ビッグエンディアン符号なしとして演算します。
    ///
    /// 複数のビットの位置が重複することは想定していません。
    /// </summary>
    public class BEUBitConnectionFormatter : IAssemblyCode32Formatter {
        /// <summary>
        /// ビット演算する対象の数値を読み出す長さ
        /// </summary>
        readonly int readLength;

        readonly ReadOnlyMemory<(int bitLength, int shift, uint mask)> calcPatterns;

        public BEUBitConnectionFormatter(
            int readLength,
            (int bitLength, int shift)[] calcPatterns) {

            this.readLength = readLength;
            this.calcPatterns = calcPatterns
                // ビットの長さ分ビットを立てる
                .Select(t => (t.bitLength, t.shift, ((0xFFFF_FFFF) >> (32 - t.bitLength)) << t.shift))
                .ToArray();
        }

        public void Deserialize(ref Reader reader, Queue<uint> result) {
            var x = BinaryPrimitives.ReverseEndianness(reader.FetchUInteger(readLength)) >>
                ((4 - readLength) * 8);
            uint shifted = 0;
            int pos = 0;
            foreach (var pattern in calcPatterns.Span) {
                shifted |= ((x & pattern.mask) >> pattern.shift) << pos;
                pos += pattern.bitLength;
            }
            result.Enqueue(shifted);
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var x = result.Dequeue();
            uint mask = 0;
            uint expanded = 0;
            int pos = 0;
            foreach (var pattern in calcPatterns.Span) {
                expanded |= ((x >> pos) << pattern.shift) & pattern.mask;
                mask |= pattern.mask;
                pos += pattern.bitLength;
            }
            writer.SetMaskedUInteger(
                readLength,
                BinaryPrimitives.ReverseEndianness(mask) >> ((4 - readLength) * 8),
                BinaryPrimitives.ReverseEndianness(expanded) >> ((4 - readLength) * 8));
        }
    }


    // /// <summary>
    // /// 読み進めません。
    // /// 1バイト読み込み右シフトしてNビットを取り出します。
    // /// </summary>
    // public class OperandU1NBitFormatter {
    //     readonly byte shift;
    //     readonly byte bitLength;

    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandU8Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandI8Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandU16Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandI16Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandU24Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandI24Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandU32Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }

    // /// <summary>
    // /// 読み進めません。
    // /// </summary>
    // public class OperandI32Formatter {
    //     void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
    //     }

    //     void Deserialize(ref Reader reader, Queue<uint> result) {
    //     }
    // }
}
