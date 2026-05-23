using System.Runtime.CompilerServices;

namespace LogicSimulator.Runtime;

/// <summary>
/// 複数の素子の複数のピンをあわらします。
/// </summary>
public struct LogicPins {
    /// <summary>
    /// ピンの信号
    /// </summary>
    public LogicSignal[] Pins;

    /// <summary>
    /// ピン番号から素子を識別できる番号に変換するためのテーブル
    /// </summary>
    public int[] PinNumberToLogicNumber;

    /// <summary>
    /// 各同じ素子の入力もしくは出力のピン数を表します。
    /// ピン情報にアクセスするために配列のオフセットとそこからの長さを保持しています。
    /// </summary>
    public (int arrayOffset, int length)[] NumOfPins;

    /// <summary>
    /// ピンの配列のインデックスを返します。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPinIndex(int logicNumber, int pinNumber) {
        var num = NumOfPins[logicNumber];
        if (num.length <= pinNumber) {
            throw new IndexOutOfRangeException($"length: {num.length}, actual: {pinNumber}");
        }
        return num.arrayOffset + pinNumber;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPinsLength(int logicNumber) {
        return NumOfPins[logicNumber].length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogicSignal ReadBit(int logicNumber, int pinNumber) {
        var num = NumOfPins[logicNumber];
        if (num.length <= pinNumber) {
            throw new IndexOutOfRangeException($"length: {num.length}, actual: {pinNumber}");
        }
        var index = num.arrayOffset + pinNumber;
        return Pins[index];
    }

    /// <summary>
    /// 今回書き込んだ値と変化したかどうかを返します。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool WriteBit(int logicNumber, int pinNumber, LogicSignal value) {
        var num = NumOfPins[logicNumber];
        if (num.length <= pinNumber) {
            throw new IndexOutOfRangeException($"length: {num.length}, actual: {pinNumber}");
        }
        var index = num.arrayOffset + pinNumber;
        var currentValue = Pins[index];
        bool isChanged = currentValue != value;
        if (isChanged) {
            Pins[index] = value;
        }
        return isChanged;
    }
}
