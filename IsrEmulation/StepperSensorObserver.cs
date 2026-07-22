
/// <summary>
/// チャタリング除去を行います。
/// </summary>
public class BitChattering {
    readonly int[] data;

    // チャタリング除去を行う回数を指定して下さい
    // 初期値が必要です。
    public BitChattering(int maxChatterCount, int initialOutput) {
        if (maxChatterCount <= 0) {
            throw new ArgumentException($"maxLoopCount: {maxChatterCount}", nameof(maxChatterCount));
        }
        data = new int[maxChatterCount];
        data.AsSpan().Fill(initialOutput);
    }

    public int Get(int index) {
        return data[index];
    }

    public void Update(int current) {
        // data[2] = (current & data[1]) |
        //     (data[1] & data[2]) |
        //     (current & data[2]);

        for (int i = data.Length - 1; 0 < i; i--) {
            data[1] = (current & data[i - 1]) |
                (data[i - 1] & data[i]) |
                (current & data[i]);
        }
        // data[1] = (current & data[0]) |
        //     (data[0] & data[1]) |
        //     (current & data[1]);
        data[0] = current;
    }
}

public class BitChatteringTest {
    void ChartteringTest(int chatterCount, int initial, int[] inputs, int expected) {
        var chattering = new BitChattering(chatterCount, initial);
        for (int i = 0; i < inputs.Length; i++) {
            chattering.Update(inputs[i]);
        }
        if (!(chattering.Get(chatterCount - 1) == expected)) {
            throw new Exception();
        }

    }

    public void Test_2Match() {

        // 前 今
        var colHeader = new int[][] {
            new[] { 0, 0 },
            new[] { 0, 1 },
            new[] { 1, 1 },
            new[] { 1, 0 }
        };

        var rowHeader = new int[] {
            0,
            1
        };

        //心理値表
        var expected = new int[,] {
            // 前 今
            // 00 01 11 10
            {   0, 0, 1, 0 }, // 0 現在の出力
            {   0, 1, 1, 1 }  // 1
        };

        for (int i = 0; i < colHeader.Length; i++) {
            for (int j = 0; j < rowHeader.Length; j++) {
                ChartteringTest(2, rowHeader[j], colHeader[i], expected[i, j]);
            }
        }
    }

    public void Test_3Match() {

        // 前々 前 今
        var colHeader = new int[][] {
            new[] { 0, 0, 0 },
            new[] { 0, 0, 1 },
            new[] { 0, 1, 1 },
            new[] { 0, 1, 0 },
            new[] { 1, 1, 0 },
            new[] { 1, 1, 1 },
            new[] { 1, 0, 1 },
            new[] { 1, 0, 0 },
        };

        var rowHeader = new int[] {
            0,
            1
        };

        //心理値表
        var expected = new int[,] {
            // 前々 前 今
            // 000 001 011 010 110 111 101 100
            {    0,  0,  0,  0,  0,  1,  0,  0 }, // 0 現在の出力
            {    0,  1,  1,  1,  1,  1,  1,  1 }  // 1
        };

        for (int i = 0; i < colHeader.Length; i++) {
            for (int j = 0; j < rowHeader.Length; j++) {
                ChartteringTest(3, rowHeader[j], colHeader[i], expected[i, j]);
            }
        }

    }

}

/// <summary>
/// センサーが反応したときのステップ数を計測します。
/// </summary>
public class StepperSensorObserver {

    readonly StepperDriver motor;

    /// <summary>
    /// 検知する対象のセンサー
    /// </summary>
    readonly GPInput1BitDriver sensorPin;

    /// <summary>
    /// true 一度信号を検知すると
    /// 検出をやめるモード
    /// false 何度もラッチするモード
    /// </summary>
    bool isSingleLatch;

    bool isRigingLached;

    bool isFallingLached;

    int lachedRigingPos;

    int lachedFallingPos;

    bool prevSensorValue;

    public event Action<(int pos, bool isRiging)>? OnLachedPos;

    void OnChangedPosition() {
        var sensorValue = sensorPin.Get();
        bool isRiging = !prevSensorValue && sensorValue;
        bool isFalling = prevSensorValue && !sensorValue;
        prevSensorValue = sensorValue;

        if (isFalling) {
            if (!isSingleLatch || !isFallingLached) {
                lachedFallingPos = motor.GetCurrentPosition();
                isFallingLached = true;
                OnLachedPos?.Invoke((lachedFallingPos, false));
            }
        }
        else if (isRiging) {
            if (!isSingleLatch || !isFallingLached) {
                lachedRigingPos = motor.GetCurrentPosition();
                isRigingLached = true;
                OnLachedPos?.Invoke((lachedFallingPos, true));
            }
        }
    }
}
