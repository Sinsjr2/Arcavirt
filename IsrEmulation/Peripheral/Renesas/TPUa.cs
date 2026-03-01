using Clock;
using GPIO;

namespace Peripheral.Renesas;


// TODO 外部端子のクロックによりカウントアップを実装する
public class TPUa {


    // struct st_tpu0 {
    public readonly RegisterValue32<byte> NFCR;
    // union {
    //     	unsigned char BYTE;
    //     	struct {
    //     		unsigned char :2;
    //     		unsigned char NFCS:2;
    //     		unsigned char NFDEN:1;
    //     		unsigned char NFCEN:1;
    //     		unsigned char NFBEN:1;
    //     		unsigned char NFAEN:1;
    //     	} BIT;
    //     } NFCR;
    // char           wk0[7];

    public readonly RegisterValue32<byte> TCR;
    static readonly MaskedBit32 TCR_TPSC = new(0x7, 0);
    static readonly MaskedBit32 TCR_CKEG = new(0x3, 3);
    static readonly MaskedBit32 TCR_CCLR = new(0x7, 5);

    // union {
    //     	unsigned char BYTE;
    //     	struct {
    //     		unsigned char CCLR:3;
    //     		unsigned char CKEG:2;
    //     		unsigned char TPSC:3;
    //     	} BIT;
    //     } TCR;
    static readonly MaskedBit32 TMDR_MDMask = new(0xF, 0);
    // 現在は、 PWMモード1と通常動作のみ対応している
    public readonly RegisterValue32<byte> TMDR;

    enum TMDR_MD : byte {
        Normal   = 0b000,
        PWMMode1 = 0b0010
    }

    // union {
    //     	unsigned char BYTE;
    //     	struct {
    //     		unsigned char ICSELD:1;
    //     		unsigned char ICSELB:1;
    //     		unsigned char BFB:1;
    //     		unsigned char BFA:1;
    //     		unsigned char MD:4;
    //     	} BIT;
    //     } TMDR;
    enum TIOR_IO {
        OutputDisable1                 = 0b0000,
        InitialLow_CompareMatchLow     = 0b0001,
        InitialLow_CompareMatchHigh    = 0b0010,
        InitialLow_CompareMatchToggle  = 0b0011,
        OutputDisable2                 = 0b0100,
        InitialHigh_CompareMatchLow    = 0b0101,
        InitialHigh_ComapreMatchHigh   = 0b0110,
        InitialHigh_CompareMatchToggle = 0b0111
    }

    public readonly RegisterValue32<byte> TIORH;
    static readonly MaskedBit32 TIOR_IOA = new(0xF, 0);
    static readonly MaskedBit32 TIOR_IOB = new(0xF, 4);
    // union {
    // 	unsigned char BYTE;
    // 	struct {
    // 		unsigned char IOB:4;
    // 		unsigned char IOA:4;
    // 	} BIT;
    // } TIORH;
    public readonly RegisterValue32<byte> TIORL;
    // union {
    //     	unsigned char BYTE;
    //     	struct {
    //     		unsigned char IOD:4;
    //     		unsigned char IOC:4;
    //     	} BIT;
    //     } TIORL;
    public readonly RegisterValue32<byte> TIER;
    // union {
    //     	unsigned char BYTE;
    //     	struct {
    //     		unsigned char TTGE:1;
    //     		unsigned char :2;
    //     		unsigned char TCIEV:1;
    //     		unsigned char TGIED:1;
    //     		unsigned char TGIEC:1;
    //     		unsigned char TGIEB:1;
    //     		unsigned char TGIEA:1;
    //     	} BIT;
    //     } TIER;
    public readonly RegisterValue32<byte> TSR;
    // union {
    //     	unsigned char BYTE;
    //     	struct {
    //     		unsigned char :3;
    //     		unsigned char TCFV:1;
    //     		unsigned char TGFD:1;
    //     		unsigned char TGFC:1;
    //     		unsigned char TGFB:1;
    //     		unsigned char TGFA:1;
    //     	} BIT;
    //     } TSR;
    public readonly RegisterValue32<ushort> TCNT;
    // unsigned short TCNT;
    public readonly RegisterValue32<ushort> TGRA;
    // unsigned short TGRA;
    public readonly RegisterValue32<ushort> TGRB;
    // unsigned short TGRB;
    public readonly RegisterValue32<ushort> TGRC;
    // unsigned short TGRC;
    public readonly RegisterValue32<ushort> TGRD;
    // unsigned short TGRD;
    // };

    /// <summary>
    /// タイマーを動作させるために分周するための設定を書き込む対象
    /// </summary>
    readonly SubClock timerClock;

    readonly CountTimer timer;

    public readonly IReadOnlyList<OutputSignalBit> OutputTIOCx;

    public readonly IReadOnlyList<OutputSignalBit> SignalIRQs;

    public TPUa(string name, IClock clock, SubClock clkIn, int numOfOutputSignals) {
        timerClock = new SubClock($"{name}.clkIn", clkIn, new Fraction(1, 1));
        timer = new CountTimer(
            timerClock, clock,
            new TickTimerTriggerAction[] {
                // new(new CountTimerTrigger(TimerTriggerKind.None, 0), () => OutputTIOCx[0].SetOutputSignal())
            },
            TimerUpperLimitTriggerKind.ResetZero, ushort.MaxValue, null);

        // timer.SetTrigger()
    }

    void WriteTCR(RegisterValue32<byte> reg, byte value) {
        var tpsc = TCR_TPSC & value;
        switch (tpsc) {
            case 0:
                timerClock.MulDiv = new Fraction(1, 1);
                break;
            case 1:
                timerClock.MulDiv = new Fraction(1, 4);
                break;
            case 2:
                timerClock.MulDiv = new Fraction(1, 16);
                break;
            case 3:
                timerClock.MulDiv = new Fraction(1, 64);
                break;
            default:
                Console.Error.WriteLine($"tpsc: {tpsc} not implemented");
                break;
        }
        // TODO CKEG  CCLR を実装する
    }

    /// <summary>
    /// タイマーのモードにより出力信号に初期値を設定しようとします。
    /// </summary>
    void TrySetInitialOutputSignal(TIOR_IO io, OutputSignalBit outputSignal, bool isRunningTimer) {
        switch (io) {
            case TIOR_IO.OutputDisable1:
            case TIOR_IO.OutputDisable2:
                // 出力を禁止する
                outputSignal.SetOutputSignal(false);
                return;
        }
        // PWMモード1はタイマーが停止している時に出力を設定します。
        if ((TMDR.Value & TMDR_MDMask) == (byte)TMDR_MD.PWMMode1 && !isRunningTimer) {
            switch (io) {
                case TIOR_IO.InitialLow_CompareMatchLow:
                case TIOR_IO.InitialLow_CompareMatchHigh:
                case TIOR_IO.InitialLow_CompareMatchToggle:
                    outputSignal.SetOutputSignal(false);
                    break;
                case TIOR_IO.InitialHigh_CompareMatchLow:
                case TIOR_IO.InitialHigh_ComapreMatchHigh:
                case TIOR_IO.InitialHigh_CompareMatchToggle:
                    outputSignal.SetOutputSignal(true);
                    break;
            }
        }
    }

    void WriteTMDR(RegisterValue32<byte> reg, byte value,
                   OutputSignalBit outputA, OutputSignalBit outputB,
                   bool isRunningTimer) {
        reg.Value = value;
        var ioa = (TIOR_IO)(value & TIOR_IOA);
        var iob = (TIOR_IO)(value & TIOR_IOB);
        TrySetInitialOutputSignal(ioa, outputA, isRunningTimer);
        TrySetInitialOutputSignal(ioa, outputB, isRunningTimer);
    }

    void WriteTIOR(RegisterValue32<byte> reg, byte value,
                   OutputSignalBit outputA, OutputSignalBit outputB,
                   bool isRunningTimer) {
        reg.Value = value;
        var ioa = (TIOR_IO)(value & TIOR_IOA);
        var iob = (TIOR_IO)(value & TIOR_IOB);
        TrySetInitialOutputSignal(ioa, outputA, isRunningTimer);
        TrySetInitialOutputSignal(ioa, outputB, isRunningTimer);
    }

    void OnMatchTrigger(TIOR_IO io, OutputSignalBit outputSignal, bool enableIRQ, OutputSignalBit signal) {
        switch ((TIOR_IO)io) {
            case TIOR_IO.InitialLow_CompareMatchLow:
            case TIOR_IO.InitialHigh_CompareMatchLow:
                outputSignal.SetOutputSignal(false);
                break;
            case TIOR_IO.InitialLow_CompareMatchHigh:
            case TIOR_IO.InitialHigh_ComapreMatchHigh:
                outputSignal.SetOutputSignal(true);
                break;
            case TIOR_IO.InitialLow_CompareMatchToggle:
            case TIOR_IO.InitialHigh_CompareMatchToggle:
                outputSignal.SetOutputSignal(!outputSignal.OutputSignal);
                break;
        }

        if (enableIRQ) {
            signal.SetOutputSignal(true);
            signal.SetOutputSignal(false);
        }
    }

    void WriteTCNT(RegisterValue32<ushort> reg, ushort value) {
        timer.Count = value;
    }

    ushort ReadTCNT(RegisterValue32<ushort> reg) {
        return (ushort)timer.Count;
    }

    void WriteTGR(RegisterValue32<ushort> reg, ushort value, int no) {
        timer.SetTrigger(no, timer.GetTrigger(no) with { TriggerCount = value });
    }
}
