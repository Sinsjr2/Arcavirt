namespace CPU;

public interface IInstructionStep {
    /// <summary>
    /// NextStepを実行をスキップするか
    /// </summary>
    bool Waiting { get; }

    /// <summary>
    /// 命令1クロック当たりの速度(ナノ秒)
    /// </summary>
    double CycleNanoSec { get; }

    /// <summary>
    /// 命令を実行し、その命令の実行にかかったクロック数を返します。
    /// </summary>
    int NextStep();

    void Start();

    void Stop();
}