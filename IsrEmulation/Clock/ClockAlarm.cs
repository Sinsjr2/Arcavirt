// namespace Clock {
//     public class ClockAlarm : IAlarm {
//         next: ClockAlarm | null = null;
//         nanos: number = 0;
//         scheduled = false;

//         constructor(
//             private readonly clock: SimulationClock,
//             readonly callback: AlarmCallback,
//         ) {}

//         schedule(deltaNanos: number): void {
//             if (this.scheduled) {
//                 this.cancel();
//             }
//             this.clock.linkAlarm(deltaNanos, this);
//         }

//         public void Cancel() {
//             this.clock.unlinkAlarm(this);
//             this.scheduled = false;
//         }
//     }
// }
