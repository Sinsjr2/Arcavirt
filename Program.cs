// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Clock;
using ScottPlot;

public class MotorSignalContext {
    public readonly List<int> Time = new();
    public readonly List<int> StepClock = new();

    public readonly PWMTimer StepClockGenerator = new();

    public readonly BitLogger StepClockLogger = new();

    public readonly StepperMotor Motor = new();

    public readonly List<int> StepperPositionsTime = new List<int>();
    public readonly List<int> StepperPositions = new List<int>();
    public readonly List<float> StepperSpeeds = new List<float>();

}

/// <summary>
/// デジタルデータをグラフに表示するために矩形になるようにデータを生成します。
/// </summary>
public class BitLogger {
    bool prevValue;

    public void Write(List<int> dest, bool x) {
        dest.Add(BoolToInt(prevValue));
        dest.Add(BoolToInt(x));

        prevValue = x;
    }

    static int BoolToInt(bool x) => x ? 1 : 0;
}

internal class Program {

     static void Main() {
         var clock = new SimulationClock();
         var sim = new Simulator(clock);
        var irqList = new CPU.ISRActions();
        var pwm = new Pheripheral.PWM(clock, irqList, 1, 60_000_000);
        var port1 = new Pheripheral.GPIO8Bit();
        var gpioDirDriver = new GPOutput1BitDriver(port1, 0, 0);
        var pwmTimerDriver = new PWMTimerDriver(pwm, 0, 60_000_000);
        var stepperDriver = new StepperDriver(pwmTimerDriver, gpioDirDriver);
        irqList.SetCallback(1, stepperDriver.OnCompareMatchedISR);
        var stepperMot1 = new Device.StepperMotor(clock, pwm, port1.Pins[0], 100);

        var plot = new Plot();
        var motorLogger1 = new GraphLogger.StepperMotorLogger(
            clock, stepperMot1,
            plot.Add.DataLogger(),
            plot.Add.DataLogger());

        motorLogger1.StartLog();

        gpioDirDriver.ChangeToOutput();

        Console.WriteLine("アイドル開始");
        for (int i = 0; i < 10; i++) {
            sim.Step();
        }
        Console.WriteLine("アイドル終了");

        stepperDriver.SetTargetPosition(500);
        Console.WriteLine("モーター位置 500");

        var alarm = clock.CreateAlarm(() => { });
        for (int i = 0; i < 1000; i++) {
            sim.Step();
        }

        alarm.Schedule(100_000_000);

        for (int i = 0; i < 10; i++) {
            sim.Step();
        }

        stepperDriver.SetTargetPosition(300);
        Console.WriteLine("モーター位置 300");
        for (int i = 0; i < 10000; i++) {
            sim.Step();
        }


        plot.Render();
        plot.SavePng("quickstart.png", 400, 300);
    }

    private static void Main_(string[] args) {
        int tickCount = 0;

        var contexts = new[] { new MotorSignalContext() };

        var allowTransportDelay = new PWMTimer();

        foreach (var motorSignal in contexts) {
            motorSignal.StepClockGenerator.OnChangedOutputSignal += () => {
                var count = tickCount;
                var output = motorSignal.StepClockGenerator.OutputSignal;
                motorSignal.Time.Add(count);
                motorSignal.Time.Add(count);
                motorSignal.StepClockLogger.Write(motorSignal.StepClock, output);
            };
        }

        foreach (var context in contexts) {
            var motor = context.Motor;
            motor.OnPositionChanged += () => {
                context.StepperPositionsTime.Add(tickCount);
                context.StepperPositions.Add(motor.Position);
                context.StepperSpeeds.Add(motor.PulsePerSecond);
            };
        }

        var transportMotor1 = new StepperController(contexts[0].StepClockGenerator, dirSignal => contexts[0].Motor.CCW_CW = dirSignal);
        contexts[0].StepClockGenerator.OnCompareMatchTriggerB += transportMotor1.OnCompareMatched;

        // var transportMotor2 = new StepperController(contexts[1].StepClockGenerator, dirSignal => contexts[1].Motor.CCW_CW = dirSignal);
        // contexts[1].StepClockGenerator.OnCompareMatchTriggerB += transportMotor2.OnCompareMatched;
        var transportManager = new TransportBeltManager(new TransportBelt(10, 500));
        int? prevPos = 0;
        void UpdateNextStopPos() {
            transportManager.Update(transportMotor1.GetCurrentPosition());
            var stopPos = transportManager.GetNextStopPosition();
            if (prevPos != stopPos) {
                Console.WriteLine(stopPos);
            }
            prevPos = stopPos;
            if (stopPos.HasValue) {
                transportMotor1.SetTargetPosition(stopPos.Value);
            }
        }
        transportMotor1.OnChangedPosition +=
            () => {
                // TODO 1ステップごとに呼び出すと計算量が多いので減らすようにしたい
                UpdateNextStopPos();
            };

        transportManager.PutObjectOnBelt(new TransportObjectInfo(70), new TransportModeSetting[] {
                 new(0, TransportMode.Go, 30),
                 new(1, TransportMode.Stop, 100),
                 new(2, TransportMode.Stop, 570),
             });

        transportManager.OnChangedTransportStatus += (jobNo, timingNo, status) => {
            Console.WriteLine($"jobNo: {jobNo}, timingNo: {timingNo}, {status}");
            if (timingNo != 1) {
                return;
            }
            void AllowTransport() {
                transportManager.AllowTransport(jobNo, timingNo);
                allowTransportDelay.OnCompareMatchTriggerB -= AllowTransport;
                UpdateNextStopPos();
            };
            allowTransportDelay.OnCompareMatchTriggerB += AllowTransport;
            allowTransportDelay.SetEnable(true);
        };

        allowTransportDelay.TriggerB = 5000;


        UpdateNextStopPos();

        // transportMotor1.SetTargetPosition(40);
        // transportMotor1.OnChangedPosition += OnCangedPos;
        // void OnCangedPos() {
        //     if (35 <= transportMotor1.GetCurrentPosition()) {
        //         transportMotor1.OnChangedPosition -= OnCangedPos;
        //         transportMotor1.SetTargetPosition(100);
        //     }
        // }

        // transportMotor2.SetTargetPosition(80);
        // transportMotor2.OnChangedPosition += OnCangedPos2;
        // void OnCangedPos2() {
        //     if (70 <= transportMotor2.GetCurrentPosition()) {
        //         transportMotor2.OnChangedPosition -= OnCangedPos2;
        //         transportMotor2.SetTargetPosition(140);
        //     }
        // }

        var timers = new List<PWMTimer> { allowTransportDelay };

        for (tickCount = 0; tickCount < 1000000; tickCount++) {
            for (int i = 0; i < contexts.Length; i++) {
                contexts[i].StepClockGenerator.Tick();
                contexts[i].Motor.Clock = contexts[i].StepClockGenerator.OutputSignal;
                contexts[i].Motor.Tick();
            }
            foreach (var timer in timers) {
                timer.Tick();
            }
        }

        Console.WriteLine(transportMotor1.GetCurrentPosition());

        Plot myPlot = new();
        foreach (var context in contexts) {
            myPlot.Add.Scatter(context.Time, context.StepClock);
            myPlot.Add.Scatter(context.StepperPositionsTime, context.StepperPositions);
            myPlot.Add.Scatter(context.StepperPositionsTime, context.StepperSpeeds);
        }
        //Console.WriteLine(string.Join("\n", stepperPositions));
        myPlot.SavePng("quickstart.png", 400, 300);
    }
}

public class StepperMotor {

    public event Action? OnPositionChanged;

    /// <summary>
    /// 軸を回転させるためのクロック信号
    /// </summary>
    public bool Clock = false;

    /// <summary>
    /// 軸の回転方向を決定するための信号
    /// </summary>
    public bool CCW_CW = false;

    bool prevClock = false;

    public int Position { get; private set; }

    int rigingWidth;

    /// <summary>
    /// ステッピングモーターの回転速度
    /// </summary>
    public float PulsePerSecond { get; private set; } = 0.0f;

    public void Tick() {
        var riging = !prevClock && Clock;
        prevClock = Clock;
        if (riging) {
            rigingWidth = 0;
            Position = CCW_CW ? Position + 1 : Position - 1;
            OnPositionChanged?.Invoke();
        }
        // TODO 1チック当たりの時間を決めて切り替えられるようにする
        rigingWidth = Math.Min(rigingWidth+1, 100000);
        PulsePerSecond = 100000.0f / rigingWidth;
    }
}

public record struct TimerCount(ushort Count, ushort TriggerA, ushort TriggerB);

public class PWMTimer {

    public bool OutputSignal => TriggerA < count;
    public event Action? OnChangedOutputSignal;

    public event Action? OnCompareMatchTriggerB;

    public volatile ushort TriggerA;
    public volatile ushort TriggerB;
    volatile bool isRunning;

    volatile ushort count;

    public ushort Count => count;

    public void SetEnable(bool enable) {
        isRunning = enable;
        count = 0;
        OnChangedOutputSignal?.Invoke();
    }

    public void Tick() {
        if (!isRunning || TriggerB <= 0) {
            return;
        }
        var prevOutputSignal = OutputSignal;
        var incremented = count + 1;
        count = (ushort)(incremented % TriggerB);
        if (incremented == TriggerB) {
            OnCompareMatchTriggerB?.Invoke();
        }
        if (prevOutputSignal != OutputSignal) {
            OnChangedOutputSignal?.Invoke();
        }
    }
}


/// <summary>
/// 出力はトリガーAで立ち上がり、トリガーBで立ち下がることを想定しています。
/// カウンターはトリガーBと一致した時に0でリセットされます。
/// </summary>
public class PWMTimer2 {

    /// <summary>
    /// システムが起動してからの時間
    /// </summary>
    static readonly Stopwatch SystemTime = Stopwatch.StartNew();


    volatile ushort triggerA;
    volatile ushort triggerB;
    volatile int isRunning;

    volatile ushort count;

    /// <summary>
    /// タイマーのカウントアップ周期
    /// この周波数でカウンターの値がカウントアップします。
    /// </summary>
    readonly int countHz;

    Task runningTask = Task.CompletedTask;

    // triggerB のコンペアマッチが発生した時に呼び出されます。
    public event Action? CompareMatchB;

    /// <summary>
    /// コンペアマッチが発生し、出力が変化する時に呼び出します。
    /// </summary>
    public event Action<TimerCount>? OnChangeOutput;

    /// <summary>
    /// タイマーがスタートもしくはストップしたときの時刻とスタートしたのかストップしたのを通知します。
    /// </summary>
    public event Action<TimeSpan, bool>? OnStartOrStop;

    // a と b に同じ値を設定するとコンペアマッチが発生しても出力は変化しません。
    public void SetTrigger(ushort a, ushort b) {
        triggerA = a;
        triggerB = b;
    }

    public void Start() {
        if (isRunning != 0) {
            return;
        }
        // もし前のタイマーが動作中であるとタスクを2重で起動することになるので待機する
        runningTask.Wait();
        if (Interlocked.Exchange(ref isRunning, 1) == 0) {
            OnStartOrStop?.Invoke(SystemTime.Elapsed, true);
            runningTask = Task.Run(Do);
        }
    }

    public void Stop() {
        if (Interlocked.Exchange(ref isRunning, 0) != 0) {
            OnStartOrStop?.Invoke(SystemTime.Elapsed, false);
        }
    }

    static long TimeSpanToTickCount(TimeSpan x, int countHz) {
        return (long)(x.TotalSeconds / 1.0f / countHz);
    }

    void Do() {
        Stopwatch sw = Stopwatch.StartNew();
        while (true) {
            var elapsed = sw.Elapsed;
            sw.Restart();
            var deltaCount = TimeSpanToTickCount(elapsed, countHz);
            for (int i = 0; isRunning != 0 && i < deltaCount; i++, count++) {
                if (count == triggerB) {
                    count = 0;
                    try {
                        // 割り込みが発生する前に出力が変化する仕様であるので先に値を変化させる。
                        OnChangeOutput?.Invoke(new TimerCount(count, triggerA, triggerB));
                        CompareMatchB?.Invoke();
                    }
                    catch (Exception ex) {
                        Console.Error.WriteLine(ex);
                    }
                }
            }
            if (isRunning == 0) {
                break;
            }
            Thread.Sleep(1);
        }
    }
}
