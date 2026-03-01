using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace USB;

public enum PID : byte {
#region token
    OUT = 0b001,
    IN = 0b1001,
    SOF = 0b0101,
    SETUP = 0b1101,
#endregion
#region data
    DATA0 = 0b0011,
    DATA1 = 0b1011,
    DATA2 = 0b0111,
    MDATA = 0b1111,
#endregion
#region handshake
    ACK = 0b0010,
    NAK = 0b1010,
    STALL = 0b1110,
    NYET = 0b0110,
#endregion
#region special
    PRE = 0b1100,
    ERR = 0b1100,
    SPLIT = 0b1000,
    PING = 0b0100
#endregion
}

/// <summary>
/// 以下のパケットで共通利用します。
/// トークンパケット
/// データパケット
/// ハンドシェークパケット
/// </summary>
public record struct USBPacket(
    PID PID,
    ushort FrameNo,
    byte Address,
    byte EndPoint,
    ReadOnlyMemory<byte>? Data) {

    public static USBPacket SOF(ushort frameNo) {
        return new USBPacket(PID.SOF, frameNo, 0, 0, null);
    }

    public static USBPacket TokenPacket(PID pid, byte address, byte endPoint) {
        return new USBPacket(pid, 0, address, endPoint, null);
    }

    public static USBPacket DataPacket(int dataKind, ReadOnlyMemory<byte> data) {
        var pid = dataKind switch {
            0 => PID.DATA0,
            1 => PID.DATA1,
            2 => PID.DATA2,
            _ => throw new ArgumentException($"actual: {dataKind}")
        };
        return new USBPacket(pid, 0, 0, 0, data);
    }

    public static USBPacket HandShakePacket(PID pid) {
        return new USBPacket(pid, 0, 0, 0, null);
    }
}


public enum DeviceState {
    Powered,
    Default,
    Address,
    Configured,
    Attached,
    Suspend
}

/// <summary>
/// バスの速度を表します。
/// </summary>
public enum USBSpeed {
    LowSpeed,
    FullSpeed,
    HiSpeed,
}

public interface IUSBBusFactory {
    IUSBBus CreateBus(Action<USBPacket>? OnRecieve);
}

public enum USBPacketReadStatus : byte {
    /// <summary>
    /// 正常に受信できた場合
    /// </summary>
    OK,
    /// <summary>
    /// 指定されたバッファーサイズ以上のデータが受信されたため、データがドロップした場合
    /// </summary>
    DataBufferOverflow,
    /// <summary>
    /// 応答がなくタイムアウトした場合
    /// </summary>
    NoResponse
}

public record struct USBPacketReadResult(
    USBPacket Packet,
    USBPacketReadStatus Status
);

public interface IUSBBus {
    /// <summary>
    /// バスと接続しているかどうかを取得するために使用します。
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// バスの速度を検出するために使用します。
    /// </summary>
    USBSpeed Speed { get; }

    /// <summary>
    /// 対応している速度をを返します。
    /// </summary>
    IReadOnlyList<USBSpeed> SupportedSpeeds { get; }

    /// <summary>
    /// リセット信号を設定します。
    /// リセット信号が true の場合は、 IsConnected 以外は操作出来ません。
    /// </summary>
    bool ResetSignal { get; set; }

    /// <summary>
    /// パケットを送信し、送信が完了したことを ValueTask により検知します。
    /// </summary>
    ValueTask WritePacket(USBPacket data);

    /// <summary>
    /// 指定したバッファにパケットのデータ部を詰め込み、戻り値で返します。
    /// バッファのサイズが1パケットのデータ部のサイズよりも小さい場合は、残りのデータは捨てられます。
    /// データ部がない場合はnullを指定して下さい。
    /// </summary>
    ValueTask<USBPacketReadResult> ReadPacket(Memory<byte>? dataBuf, CancellationToken token);
}

 [Flags]
public enum RequestType : byte {
    //D7: Data transfer direction
    HostToDevice = 0 << 7,
    DeviceToHost = 1 << 7,

    //D6...5: Type
    Standard = 0 << 5,
    Class = 1 << 5,
    Vendor = 2 << 5,

    //D4...0: Recipient
    Device = 0 << 0,
    Interface = 1 << 0,
    Endpoint = 2 << 0,
    Other = 3 << 0
}

public enum Request : byte {
    GET_STATUS = 0,
    CLEAR_FEATURE,
    SET_FEATURE = 3,
    SET_ADDRESS = 5,
    GET_DESCRIPTOR,
    SET_DESCRIPTOR,
    GET_CONFIGURATION,
    SET_CONFIGURATION,
    GET_INTERFACE,
    SET_INTERFACE,
    SYNCH_FRAME
}

public enum DescriptorTypes : ushort {
    DEVICE = 1,
    CONFIGURATION,
    STRING,
    INTERFACE,
    ENDPOINT,
    DEVICE_QUALIFIER,
    OTHER_SPEED_CONFIGURATION,
    INTERFACE_POWER
}

public record struct DeviceDescriptor(
    //BCD     USB Specification Release Number in Binary-Coded Decimal.
    ushort bcdUSB,
    //Class   1       Class code (assigned by the USB-IF).
    byte bDeviceClass,
    //SubClass        Subclass code (assigned by the USB-IF).
    byte bDeviceSubClass,
    //Protocol        Protocol code (assigned by the USB-IF).
    byte bDeviceProtocol,
    //Number  Maximum packet size for endpoint zero (only 8, 16, 32, or 64 are valid)
    byte bMaxPacketSize0,
    //ID      Vendor ID (assigned by the USB-IF)
    ushort idVendor,
    //ID      Product ID (assigned by the manufacturer)
    ushort idProduct,
    //BCD     Device release number in binary-coded decimal
    ushort bcdDevice,
    //Index   Index of string descriptor describing manufacturer
    byte iManufacturer,
    //Index   Index of string descriptor describing product
    byte iProduct,
    //Index   Index of string descriptor describing the device’s serial number
    byte iSerialNumber,
    //Number  Number of possible configurations
    byte bNumConfigurations
) {
}

public record struct ControlSetupPacket(
    byte bmRequestType,
    byte bRequest,
    ushort wValue,
    ushort wIndex,
    ushort wLength) {

    /// <summary>
    /// バイト配列に変換したときの長さ
    /// </summary>
    public static readonly int Length = 8;

    /// <summary>
    /// 必要な長さが以上であること前提でデータを書き込みます。
    /// 長さが不足していた場合は、例外が発生します。
    /// </summary>
    public void Write(Span<byte> dest) {
        if (dest.Length < Length) {
            throw new ArgumentException($"actual: {dest.Length}, Length: {Length}");
        }
        dest[0] = bmRequestType;
        dest[1] = bRequest;
        BinaryPrimitives.WriteUInt16LittleEndian(dest.Slice(2), wValue);
        BinaryPrimitives.WriteUInt16LittleEndian(dest.Slice(4), wIndex);
        BinaryPrimitives.WriteUInt16LittleEndian(dest.Slice(6), wLength);
    }

    public static bool TryRead(Span<byte> src, [MaybeNullWhen(false)] out ControlSetupPacket result) {
        if (src.Length < Length) {
            result = default;
            return false;
        }
        result = new(
            bmRequestType: src[0],
            bRequest: src[1],
            wValue: BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(2)),
            wIndex: BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(4)),
            wLength: BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(6))
        );
        return true;
    }
}

/// <summary>
/// USBの転送の結果を表します。
/// </summary>
public enum USBTransferStatus: byte {
    /// <summary>
    /// 応答された結果のデータ部のサイズが期待するデータよりも大きかったためにデータが
    /// 欠損したことを表します。
    /// </summary>
    DataBufferOverflow,
    OK,
    NAK,
    STALL,
    ERROR
}

/// <summary>
/// データステージの読み込んだときの情報を表します。
/// </summary>
/// <param name="ReadLength">読み込んだバイト配列の長さ</param>
public record struct USBReadTransactionResult(
    USBTransferStatus Status,
    int ReadLendght);

public static class USBHostCommon {

    static ValueTask SendUSBPacket(IUSBBus bus, USBPacket packet, CancellationToken token) {
        return bus.WritePacket(packet);
    }

    static USBTransferStatus ConvertToTransferStatus(PID pid) {
        return pid switch {
            PID.ACK => USBTransferStatus.OK,
            PID.NAK => USBTransferStatus.NAK,
            PID.STALL => USBTransferStatus.STALL,
            _ => USBTransferStatus.ERROR
        };
    }

    /// <summary>
    /// ハンドシェークパケットを受信するまで待機します。
    /// </summary>
    // TODO タイムアウトを追加する
    static async ValueTask<USBTransferStatus> WaitHandShakePacket(IUSBBus bus, CancellationToken token) {
        var packet = await bus.ReadPacket(null, token);
        return packet.Status != USBPacketReadStatus.OK
            ? USBTransferStatus.ERROR
            : ConvertToTransferStatus(packet.Packet.PID);
    }

    /// <summary>
    /// コントロール転送セットアップトランザクションを実行します。
    /// 応答が ACKで返答された場合は、true それ以外の場合はfalseを返します。
    /// </summary>
    public static async ValueTask<bool> ExecuteControl_SetupTransaction(
        IUSBBus bus,
        byte address, ControlSetupPacket packet, CancellationToken token
    ) {
        var pool = MemoryPool<byte>.Shared;

        await SendUSBPacket(bus, USBPacket.TokenPacket(PID.SETUP, address, 0), token);

        using var dataPacket_data = pool.Rent(ControlSetupPacket.Length);
        packet.Write(dataPacket_data.Memory.Span);
        await SendUSBPacket(bus, USBPacket.DataPacket(0, dataPacket_data.Memory.Slice(ControlSetupPacket.Length)), token);

        return await WaitHandShakePacket(bus, token) == USBTransferStatus.OK;
    }

    /// <summary>
    /// コントロールWrite転送データトランザクションを実行します。
    /// </summary>
    public static async ValueTask<USBTransferStatus> ExecuteControlWrite_DataTransaction(
        IUSBBus bus,
        byte address,
        int dataKind,
        ReadOnlyMemory<byte> data,
        CancellationToken token
    ) {
        await SendUSBPacket(bus, USBPacket.TokenPacket(PID.OUT, address, 0), token);
        await SendUSBPacket(bus, USBPacket.DataPacket(dataKind, data), token);
        return await WaitHandShakePacket(bus, token);
    }

    /// <summary>
    /// </summary>
    public static async ValueTask<USBTransferStatus> ExecuteControlWrite_StatusTransaction(
        IUSBBus bus,
        byte address,
        CancellationToken token
    ) {
        await SendUSBPacket(bus, USBPacket.TokenPacket(PID.IN, address, 0), token);
        var packet = await bus.ReadPacket(null, token);
        if (!(packet.Status is USBPacketReadStatus.DataBufferOverflow or USBPacketReadStatus.OK)) {
            return USBTransferStatus.ERROR;
        }
        if (!(packet.Packet.PID is PID.DATA1)) {
            // 応答がおかしかった場合や NAK STALL が発生した場合
            return ConvertToTransferStatus(packet.Packet.PID);
        }
        await SendUSBPacket(bus, USBPacket.HandShakePacket(PID.ACK), token);
        return packet.Status == USBPacketReadStatus.DataBufferOverflow
            ? USBTransferStatus.DataBufferOverflow
            : USBTransferStatus.OK;
    }

    public static async ValueTask<USBReadTransactionResult> ExecuteControlRead_DataTransaction(
        IUSBBus bus,
        byte address,
        Memory<byte> buf,
        CancellationToken token
    ) {
        await SendUSBPacket(bus, USBPacket.TokenPacket(PID.IN, address, 0), token);
        var packet = await bus.ReadPacket(buf, token);
        if (!(packet.Status is USBPacketReadStatus.DataBufferOverflow or USBPacketReadStatus.OK)) {
            return new(USBTransferStatus.ERROR, 0);
        }
        if (!(packet.Packet.PID is PID.DATA1)) {
            // 応答がおかしかった場合や NAK STALL が発生した場合
            return new(ConvertToTransferStatus(packet.Packet.PID), 0);
        }
        await SendUSBPacket(bus, USBPacket.HandShakePacket(PID.ACK), token);
        if (packet.Status == USBPacketReadStatus.DataBufferOverflow) {
            return new(USBTransferStatus.DataBufferOverflow, packet.Packet.Data?.Length ?? 0);
        }
        return new(USBTransferStatus.OK, packet.Packet.Data?.Length ?? 0);
    }

    public static async ValueTask<USBTransferStatus> ExecuteControlRead_StatusTransaction(
        IUSBBus bus,
        byte address,
        CancellationToken token
    ) {
        await SendUSBPacket(bus, USBPacket.TokenPacket(PID.OUT, address, 0), token);
        await SendUSBPacket(bus, USBPacket.DataPacket(1, Array.Empty<byte>()), token);
        return await WaitHandShakePacket(bus, token);
    }
}

