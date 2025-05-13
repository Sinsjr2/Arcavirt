using Clock;
using IsrEmulation.CommandInterpreter;

namespace IsrEmulation.StepperCommand;

public class StepperInterpreter {
    readonly StepperDriver stepper;
    readonly List<ChangeVelocityCommand> changeVelocityCommands = new();

    readonly ICountObserver<int> cwCountObserver;
    readonly ICountObserver<int> ccwCountObserver;

    // TODO 回転中に最大速度が変更されたときにでも反映できるようにする
    public int MaxVelocity { get; set; }

    public StepperInterpreter(StepperDriver stepper) {
        this.stepper = stepper;
        cwCountObserver = stepper.CwCountObserver.Create(OnChangePositionForCW);
        ccwCountObserver = stepper.CcwCountObserver.Create(OnChangePositionForCCW);
    }

    /// <summary>
    /// 移動する速度プロファイルを設定します。
    /// </summary>
    public void SetVelocityProfile(IEnumerable<ChangeVelocityCommand> commands) {
        changeVelocityCommands.Clear();
        changeVelocityCommands.AddRange(commands.DistinctBy(x => x.Position));
        changeVelocityCommands.Sort((a, b) => a.Position - b.Position);
    }

    void OnChangePositionForCW(int currentPosition) {
        var index = CollectionUtil.UpperBound(changeVelocityCommands, new ChangeVelocityCommand(currentPosition + 1, null));
        if (index < changeVelocityCommands.Count) {
            var pair = changeVelocityCommands[index];
            stepper.SetMaxVelocity(pair.NewVelocity ?? MaxVelocity);
            cwCountObserver.Schedule(pair.Position);
        }
        else {
            stepper.SetMaxVelocity(MaxVelocity);
        }
    }

    void OnChangePositionForCCW(int currentPosition) {
        var index = CollectionUtil.LowerBound(changeVelocityCommands, new ChangeVelocityCommand(currentPosition - 1, null));
        if (index < changeVelocityCommands.Count) {
            var pair = changeVelocityCommands[index];
            stepper.SetTargetPosition(pair.NewVelocity ?? MaxVelocity);
            cwCountObserver.Schedule(pair.Position);
        }
        else {
            stepper.SetMaxVelocity(MaxVelocity);
        }
    }

    public void StartMove(int targetPosition) {
        // 次の位置更新の時に速度を反映させるために設定する
        // TODO 1ステップ目の速度がコマンドで設定されている値にならないので修正が必要
        ccwCountObserver.Schedule(1);
        cwCountObserver.Schedule(1);
        stepper.SetTargetPosition(targetPosition);
    }
}