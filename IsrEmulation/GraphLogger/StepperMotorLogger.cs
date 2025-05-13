using Clock;
using ScottPlot.Plottables;

namespace GraphLogger {

    public class StepperMotorLogger {
        readonly Device.StepperMotor motor;
        readonly IClock clock;

        ScottPlot.Plottables.DataLogger positionLogger;
        ScottPlot.Plottables.DataLogger speedLogger;

        public StepperMotorLogger(IClock clock,
                                  Device.StepperMotor motor,
                                  DataLogger positionLogger,
                                  DataLogger speedLogger) {
            this.motor = motor;
            this.clock = clock;
            this.positionLogger = positionLogger;
            this.speedLogger = speedLogger;
        }

        void AddData() {
            var sec = clock.Nanos / 1000_000_000;
            positionLogger.Add(sec, motor.Position);
            speedLogger.Add(sec, motor.Speed);
        }

        /// <summary>
        /// グラフの更新を開始します。
        /// </summary>
        public void StartLog() {
            motor.OnChanged += AddData;
        }

        /// <summary>
        /// グラフの更新を停止します。
        /// </summary>
        public void StopLog() {
            motor.OnChanged -= AddData;
        }

        /// <summary>
        /// 蓄積されたログを削除します。
        /// </summary>
        public void ClearLog() {
            positionLogger.Data.Clear();
            speedLogger.Data.Clear();
        }
    }
}
