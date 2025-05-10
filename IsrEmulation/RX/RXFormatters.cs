
namespace RX {

    /// <summary>
    /// RX ASM 用の即値用のフォーマッター
    /// 可変長データ
    /// 指定されたバイト数(4バイト以下)読み込み(読み進める)、
    /// 右シフトした後に2ビット(liの値)を取り出し即値のサイズをもとめ取り出し、進めます。
    /// </summary>
    public class ImmediateValueFormatter : IAssemblyCode32Formatter {

        readonly byte liReadSize;
        readonly byte shift;

        public ImmediateValueFormatter(byte liReadSize, byte shift) {
            this.liReadSize = liReadSize;
            this.shift = shift;
        }

        static int GetLIByteLength(uint li) {
            return li switch {
                0b01 => 1,
                0b10 => 2,
                0b11 => 3,
                0b00 => 4,
                _ => throw new InvalidOperationException($"li value is not supported {li}")
            };
        }

        public void Deserialize(ref Reader reader, List<uint> result) {
            var x = reader.ReadUInteger(liReadSize);
            var li = (x >> shift) & 0b11;
            var byteLength = GetLIByteLength(li);
            var immediate = reader.ReadInteger(byteLength);
            result.Add(li);
            result.Add(immediate);
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var li = result.Dequeue();
            var length = GetLIByteLength(li);
            var immediate = result.Dequeue();
            writer.WriteMaskedUInteger(liReadSize, 0b11U << shift, li << shift);
            writer.WriteUInteger(length, immediate);
        }
    }

    /// <summary>
    /// RX ASM 用の相対アドレッシングの値のフォーマッター
    /// 可変長データ
    /// 指定されたバイト数(4バイト以下)読み込み(読み進める)、
    /// 右シフトした後に2ビット(dspの値)を取り出しサイズをもとめ取り出し、進めます。
    /// </summary>
    public class DisplacementValueFormatter : IAssemblyCode32Formatter {
        readonly byte dspReadSize;
        readonly byte shift;

        public DisplacementValueFormatter(byte dspReadSize, byte shift) {
            this.dspReadSize = dspReadSize;
            this.shift = shift;
        }

        public static int GetDspByteLength(uint ld) {
            return ld switch {
                (uint)LengthOfDisplacement.Reg => 0,
                (uint)LengthOfDisplacement.RefReg => 0,
                (uint)LengthOfDisplacement.DSP8Reg => 1,
                (uint)LengthOfDisplacement.DSP16Reg => 2,
                _ => throw new InvalidOperationException($"ld value is not supported {ld}")
            };
        }

        public void Deserialize(ref Reader reader, List<uint> result) {
            var x = reader.ReadUInteger(dspReadSize);
            var ld = (x >> shift) & 0b11;
            var byteLength = GetDspByteLength(ld);

            if (byteLength == 0) {
                result.Add(ld);
            }
            else {
                var dsp = reader.ReadUInteger(byteLength);
                result.Add(ld);
                result.Add(dsp);
            }
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var ld = result.Dequeue();
            var byteLength = GetDspByteLength(ld);

            if (byteLength == 0) {
                writer.WriteMaskedUInteger(dspReadSize, 0b11U << shift, ld << shift);
            }
            else {
                var dsp = result.Dequeue();
                writer.WriteMaskedUInteger(dspReadSize, 0b11U << shift, ld << shift);
                writer.WriteUInteger(byteLength, dsp);
            }
        }
    }

    /// <summary>
    /// <see name="DisplacementValueFormatter"/> が1オペレータに2つあるタイプの命令を解析します。
    /// dspはなしの場合は、配列の要素に0を入れます。(オペランド用の配列の長さは一定になります。)
    /// </summary>
    public class TwoDisplacementValueFormatter : IAssemblyCode32Formatter {
        readonly byte dspReadSize;
        readonly byte shiftA;
        readonly byte shiftB;

        public TwoDisplacementValueFormatter(byte dspReadSize, byte shiftA, byte shiftB) {
            this.dspReadSize = dspReadSize;
            this.shiftA = shiftA;
            this.shiftB = shiftB;
        }

        public void Deserialize(ref Reader reader, List<uint> result) {
            var x = reader.ReadUInteger(dspReadSize);
            var ldA = (x >> shiftA) & 0b11;
            var ldB = (x >> shiftB) & 0b11;
            var byteLengthA = DisplacementValueFormatter.GetDspByteLength(ldA);
            var byteLengthB = DisplacementValueFormatter.GetDspByteLength(ldB);

            var dspA = byteLengthA == 0
                ? 0
                : reader.ReadUInteger(byteLengthA);
            var dspB = byteLengthB == 0
                ? 0
                : reader.ReadUInteger(byteLengthB);

            result.Add(ldA);
            result.Add(dspA);
            result.Add(ldB);
            result.Add(dspB);
        }

        public void Serialize(Queue<uint> result, ref AssemblyWriter writer) {
            var ldA = result.Dequeue();
            var dspA = result.Dequeue();
            var ldB = result.Dequeue();
            var dspB = result.Dequeue();
            var byteLengthA = DisplacementValueFormatter.GetDspByteLength(ldA);
            var byteLengthB = DisplacementValueFormatter.GetDspByteLength(ldB);

            writer.SetMaskedUInteger(dspReadSize, 0b11U << shiftA, ldA << shiftA);
            writer.SetMaskedUInteger(dspReadSize, 0b11U << shiftB, ldB << shiftB);
            writer.Skip(dspReadSize);
            if (byteLengthA != 0) {
                writer.WriteUInteger(byteLengthA, dspA);
            }
            if (byteLengthB != 0) {
                writer.WriteUInteger(byteLengthB, dspB);
            }
        }
    }
}
