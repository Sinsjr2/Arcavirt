using Pheripheral;

namespace Peripheral.Renesas;

public record Register32MappingInfo(uint Offset, IRegisterValue32 Register);

class UIntResolver {
    static readonly Dictionary<Type, object> converter = new() {
        { typeof(bool), (uint x) => x != 0 ? 1 : 0 },
        { typeof(byte), (uint x) => (byte)x },
        { typeof(ushort), (uint x) => (ushort)x },
        { typeof(uint), (uint x) => x },
    };

    public static Func<uint, T> Find<T>()
    {
        return (Func<uint, T>)converter[typeof(T)];
    }
}

public static class UIntConverter<T> {
    public static readonly Func<uint, T> ConverterToT = UIntResolver.Find<T>();
}

public interface IRegisterValue32 {
    /// <summary>
    /// このレジスタのバイト数を返します。
    /// TODO 実装すること
    /// </summary>
    // int ByteSize { get; }
    void Write(uint value);
    uint Read();

    // TODO レジスタごとにリセットを掛けても各ペリフェラルの内部状態まで、クリアできるわけでは無いので
    // 削除する
    // 別のインターフェースが必要
    // void Reset();
}

public interface IRegisterValue32<T> : IRegisterValue32 {
    T Value { get; set; }
}

/// <summary>
/// 書き込むときのコールバックを登録すると、コールバック内で新しい値を設定して下さい。
/// しない場合は、値は更新されません。
/// </summary>
public class RegisterValue32<T> : IRegisterValue32<T> where T : struct, IConvertible {
    static readonly Func<uint, T> convert = UIntConverter<T>.ConverterToT;
    public T InitialValue { get; set; }
    public T Value { get; set; }

    readonly Action<RegisterValue32<T>, T>? onWrite;
    readonly Func<RegisterValue32<T>, T>? onRead;
    readonly Action<RegisterValue32<T>, T>? onReset;

    public RegisterValue32(T initialValue,
                         Action<RegisterValue32<T>, T>? onWrite = null,
                         Func<RegisterValue32<T>, T>? onRead = null,
                         Action<RegisterValue32<T>, T>? onReset = null) {
        InitialValue = initialValue;
        Value = initialValue;
        this.onWrite = onWrite;
        this.onRead = onRead;
        this.onReset = onReset;
    }

    public void Reset() {
        if (onReset is null) {
            Value = InitialValue;
        }
        else {
            onReset(this, InitialValue);
        }
    }

    public void Write(uint value) {
        var current = convert(value);
        if (onWrite is null) {
            Value = current;
        }
        else {
            onWrite(this, current);
        }
    }

    public uint Read() {
        var newValue = onRead is null ? Value : onRead(this);
        return newValue.ToUInt32(null);
    }
}

public interface IRegisterBit32 {
    byte BitLength { get; }
    void Write(uint value);
    uint Read();
    void Reset();
}

public interface IRegisterBit32<T> : IRegisterBit32 {
    public T Value { get; set; }
}

// public class RegisterBit32_Bool {
//     public bool InitialValue { get; set; }
//     public bool Value { get; set; }
//     public byte BitLength { get; }

//     public void Write(uint value) {
//     }

//     public uint Read() {
//         return 0;
//     }

//     public void Reset() {
//         Value = InitialValue;
//     }
// }

public class RegisterBit32<T> : IRegisterBit32<T> where T : struct, IConvertible {
    static readonly Func<uint, T> convert = UIntConverter<T>.ConverterToT;
    public byte BitLength { get; }
    public T InitialValue { get; set; }
    public T Value { get; set; }
    readonly uint mask;

    readonly Action<RegisterBit32<T>, T>? onWrite;
    readonly Func<RegisterBit32<T>, T>? onRead;
    readonly Action<RegisterBit32<T>, T>? onReset;

    public RegisterBit32(byte bitLength, T initialValue,
                         Action<RegisterBit32<T>, T>? onWrite = null,
                         Func<RegisterBit32<T>, T>? onRead = null,
                         Action<RegisterBit32<T>, T>? onReset = null) {
        InitialValue = initialValue;
        Value = initialValue;
        BitLength = bitLength;
        this.mask = NumUtil.FlagFF(BitLength);
        this.onWrite = onWrite;
        this.onRead = onRead;
        this.onReset = onReset;
    }

    public void Write(uint value) {
        var converted = convert(value & mask);
        if (onWrite is null) {
            Value = converted;
        }
        else {
            onWrite(this, converted);
        }
    }

    public uint Read() {
        var result = onRead is null ? Value : onRead(this);
        return result.ToUInt32(null);
    }

    public void Reset() {
        if (onReset is null) {
            Value = InitialValue;
        }
        else {
            onReset(this, InitialValue);
        }
    }
}

/// <summary>
/// 書き込まれた値を無視します。
/// </summary>
public class ReadOnlyRegisterBit32<T> : IRegisterBit32<T> where T : struct, IConvertible {
    static readonly Func<uint, T> convert = UIntConverter<T>.ConverterToT;
    public byte BitLength { get; }
    public T InitialValue { get; set; }
    public T Value { get; set; }
    readonly uint mask;

    readonly Func<ReadOnlyRegisterBit32<T>, T>? onRead;
    readonly Func<ReadOnlyRegisterBit32<T>, T, T>? onReset;

    public ReadOnlyRegisterBit32(byte bitLength, T initialValue,
                         Func<ReadOnlyRegisterBit32<T>, T>? onRead = null,
                         Func<ReadOnlyRegisterBit32<T>, T, T>? onReset = null) {
        InitialValue = initialValue;
        Value = initialValue;
        BitLength = bitLength;
        this.mask = NumUtil.FlagFF(BitLength);
        this.onRead = onRead;
        this.onReset = onReset;
    }

    public void Write(uint value) {
    }

    public uint Read() {
        var result = onRead is null ? Value : onRead(this);
        return result.ToUInt32(null);
    }

    public void Reset() {
        if (onReset is null) {
            Value = InitialValue;
        }
        else {
            onReset(this, InitialValue);
        }
    }
}

public static class NumUtil {
    /// <summary>
    /// 指定されたバイト数でマスク処理をします。
    /// サイズが4以上の場合はvalueをそのまま返します。
    /// 0以下の場合は0を返します。
    /// </summary>
    public static uint MaskSize(int size, uint value) {
        if (size <= 0) {
            return 0;
        }
        if (4 <= size) {
            return value;
        }
        return ((~0u) >> ((4 - size) * 8)) & value;
    }

    /// <summary>
    /// 指定したビット数立てます。
    /// 0以下を指定した場合は0を返します。
    /// 32以上を設定した場合は全てのビットが1の値を返します。
    /// </summary>
    public static uint FlagFF(int bitLength) {
        if (bitLength <= 0) {
            return 0;
        }
        if (32 <= bitLength) {
            return ~0u;
        }
        return (~0u) >> (32 - bitLength);
    }
}

public class RegisterBitField32 : IRegisterValue32 {
    /// <summary>
    /// 読み出す時に割り当てていないビットの値を決定するために使用します。
    /// </summary>
    readonly uint initialValue;
    readonly ReadOnlyMemory<(int bitPos, uint mask, byte bitLength, IRegisterBit32 value)> fields;

    public RegisterBitField32(IEnumerable<(int bitPos, IRegisterBit32 value)> fields, uint initialValue = 0) {
        this.fields = fields
            .Select(t => (t.bitPos, NumUtil.FlagFF(t.value.BitLength) << t.bitPos, t.value.BitLength, t.value))
            .ToArray();
        this.initialValue = initialValue;
    }

    public void Write(uint value) {
        foreach (var field in fields.Span) {
            var fieldValue = (value & field.mask) >> field.bitPos;
            field.value.Write(fieldValue);
        }
    }

    public uint Read() {
        var result = initialValue;
        foreach (var field in fields.Span) {
            var regValue = field.value;
            result = (result & field.mask) |
                (field.mask & (regValue.Read() << field.bitPos));
        }
        return result;
    }

    public void Reset() {
        foreach (var field in fields.Span) {
            field.value.Reset();
        }
    }
}

/// <summary>
/// 読み込みのみ対応しています。
/// 書き込んだ値は捨てます。
/// </summary>
public class ReadOnlyRegisterValue<T> : IRegisterValue32 where T : struct, IConvertible {
    public T InitialValue { get; set; }
    public T Value { get; set; }

    readonly Func<ReadOnlyRegisterValue<T>, T>? onRead;
    readonly Func<ReadOnlyRegisterValue<T>, T, T>? onReset;

    public ReadOnlyRegisterValue(T initialValue,
                                 Func<ReadOnlyRegisterValue<T>, T>? onRead = null,
                                 Func<ReadOnlyRegisterValue<T>, T, T>? onReset = null) {
        InitialValue = initialValue;
        Value = initialValue;
        this.onRead = onRead;
        this.onReset = onReset;
    }

    public void Reset() {
        Value = onReset is null
            ? InitialValue
            : onReset(this, InitialValue);
    }

    public uint Read() {
        var newValue = onRead is null ? Value : onRead(this);
        return newValue.ToUInt32(null);
    }

    public void Write(uint value) {
    }
}

/// <summary>
/// 範囲指定したアドレスへのアクセスをするときは、
/// 範囲の初めを0としたアドレスでアクセスするようにします。
/// </summary>
public class BusManager : IBus32 {
    readonly List<(uint beginAddress, uint endAddress, IBus32 target)> rangedMapping = new();
    readonly Dictionary<uint, IRegisterValue32> registers = new();

    /// <summary>
    /// 範囲指定されたアドレスをバスアクセス出来るように登録します。
    /// </summary>
    public void AddRangedAddressMapping(uint beginAddress, uint endAddress, IBus32 target) {
        rangedMapping.Add((beginAddress, endAddress, target));
    }

    /// <summary>
    /// 指定のアドレスにアクセスできるレジスタを登録します。
    /// </summary>
    public void AddMapping(uint address, IRegisterValue32 register) {
        registers[address] = register;
    }

    public uint Read(uint address, int size) {
        foreach (var t in rangedMapping)
        {
            if (t.beginAddress <= address && address <= t.endAddress) {
                var targetAddress = address - t.beginAddress;
                return t.target.Read(targetAddress, size);
            }
        }
        // TODO 検索に失敗した時にログを出力する
        if (!registers.TryGetValue(address, out var regValue)) {
            return 0;
        }
        return NumUtil.MaskSize(size, regValue.Read());
    }

    public void Write(uint address, int size, uint value) {
        foreach (var t in rangedMapping)
        {
            if (t.beginAddress <= address && address <= t.endAddress) {
                t.target.Write(address, size, value);
                return;
            }
        }
        // TODO 検索に失敗した時にログを出力する
        if (!registers.TryGetValue(address, out var regValue)) {
            return;
        }
        regValue.Write(NumUtil.MaskSize(size, value));
    }

    // public void Reset() {
    //     foreach (var reg in registers.Values) {
    //         reg.Reset();
    //     }
    // }
}
