public struct MotorLinkSetting {
    // 各同期グループが主軸となったときのその軸に対するギア比
    public float RatioNumerator;
    public uint RatioDenominator;
}

public class MotorLinker {
    // 1つめは 連動グループ番号 2つめはリンクする軸
    readonly bool[,] links;

    bool[,] linkMatrix;

    public MotorLinker(int numOfAxis) {
        this.links = new bool[numOfAxis, numOfAxis];
        this.linkMatrix = new bool[numOfAxis, numOfAxis];
    }

    /// <summary>
    /// 指定した軸番号同士を連動して動作するようにします。
    /// 既に連動していた場合は何もしません。
    /// </summary>
    public void Link(int axis1, int axis2) {
        if (axis1 < axis2) {
            linkMatrix[axis2, axis1] = true;
        }
        else {
            linkMatrix[axis1, axis2] = true;
        }

        ApplyToLinks();
    }

    void ApplyToLinks() {
        Array.Clear(links, 0, links.Length);

        for (int nextSearcAxisNo = 0; nextSearcAxisNo < links.GetLength(2); nextSearcAxisNo++) {
            bool alreadyLinked = false;
            for (int groupNo = 0; groupNo < links.Length;) {
                if (links[groupNo, nextSearcAxisNo]) {
                    // 既に同期済みの軸は処理をスキップする
                    alreadyLinked = true;
                    break;
                }
            }
            if (alreadyLinked) {
                continue;
            }
            DepthFirstSearch(nextSearcAxisNo, nextSearcAxisNo);
            nextSearcAxisNo++;
        }
    }

    void  DepthFirstSearch(int axisNo, int groupNo) {
        bool linked1 = links[groupNo, axisNo];
        links[groupNo, axisNo] = true;
        if (linked1) {
            return;
        }
        for (int i = 0; i < linkMatrix.Length; i++) {

            if (linkMatrix[axisNo, i]) {
                bool linked2 = links[groupNo, axisNo];
                links[groupNo, i] = true;
                if (linked2) {
                    DepthFirstSearch(i, groupNo);
                }
            }
        }
        for (int i = 0; i < linkMatrix.GetLength(2); i++) {
            if (linkMatrix[i, axisNo]) {
                bool linked2 = links[groupNo, axisNo];
                links[groupNo, i] = true;
                if (linked2) {
                    DepthFirstSearch(i, groupNo);
                }
            }
        }
    }

    /// <summary>
    /// 指定した軸同士の連動を解除します。
    /// 連動していない場合は何もしません。
    /// </summary>
    public void Unlink(int axis1, int axis2) {
        if (axis1 < axis2) {
            linkMatrix[axis2, axis1] = false;
        }
        else {
            linkMatrix[axis1, axis2] = false;
        }
        ApplyToLinks();
    }

    /// <summary>
    /// tryVisit はすでに訪れたことがあるノードであればfalseを返します。
    /// </summary>
    static void  DepthFirstSearch<T>(T value,  Func<T, IEnumerable<T>> getChildren, Func<T, bool> tryVisit) {
        if (!tryVisit(value)) {
            return;
        }
        foreach (var child in getChildren(value)) {
            if (tryVisit(value)) {
                DepthFirstSearch(value, getChildren, tryVisit);
            }
        }
    }

    /// <summary>
    /// 同期が必要なモーターに対して、目標位置を設定します。
    /// </summary>
    public void Apply(StepperController[] steppers) {
        // モーターは動き続けるため、計算中に変わる可能性があるので事前に位置を確定させておく
        var currentStatuses = steppers
            .Select(stepper => (currentPos: stepper.GetCurrentPosition(),
                                targetPos: stepper.GetTargetPosition(),
                                isRunning: stepper.IsRunning))
            .ToArray();

        // TODO 回転方向が同じ事を想定している 倍率を変更できるようにする
        for (int i = 0; i < links.Length; i++) {
            //各モーターの現在位置と目標位置が一番近い距離を探す
            int minLength = int.MaxValue;
            for (int j = 0; j < links.GetLength(2); j++) {
                if (!links[i, j]) {
                    continue;
                }
                if (!currentStatuses[j].isRunning) {
                    // 同期しているモーターが停止していた場合は全てのモーターを停止するのが確定する
                    minLength = 0;
                    break;
                }
                minLength = Math.Min(minLength, currentStatuses[j].targetPos - currentStatuses[j].currentPos);
            }
            // 各グループごとに停止する位置を設定する
            for (int j = 0; j < links.GetLength(2); j++) {
                if (!links[i, j] || !currentStatuses[j].isRunning) {
                    continue;
                }
                steppers[j].SetTargetPosition(currentStatuses[j].currentPos + minLength);
            }
        }
    }
}
