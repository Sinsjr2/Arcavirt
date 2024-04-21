using System;

// 参考 https://github.com/wokwi/rp2040js/blob/02bc892bd290ce860592b2a40861c7c9c1543bf0/src/clock/simulation-clock.ts

namespace Clock {

    public class ClockAlarm : IAlarm {
        public ClockAlarm? next = null;
        public double nanos = 0;
        public bool scheduled = false;
        private readonly SimulationClock clock;
        public readonly Action callback;

        public ClockAlarm(SimulationClock clock, Action callback) {
            this.clock = clock;
            this.callback = callback;
        }

        public void Schedule(double deltaNanos) {
            if (this.scheduled) {
                this.Cancel();
            }
            this.clock.linkAlarm(deltaNanos, this);
        }

        public void Cancel() {
            this.clock.unlinkAlarm(this);
            this.scheduled = false;
        }
    }

    public class SimulationClock: IClock {
        private ClockAlarm? nextAlarm= null;

        private double nanosCounter = 0;
        readonly double frequency;

        public SimulationClock(double frequency = 125e6) {
            this.frequency = frequency;
        }

        public double Nanos =>
            this.nanosCounter;

        public double micros =>
            this.Nanos / 1000;

        public IAlarm CreateAlarm(Action callback) {
            return new ClockAlarm(this, callback);
        }

        public ClockAlarm linkAlarm(double nanos, ClockAlarm alarm) {
            alarm.nanos = this.Nanos + nanos;
            var alarmListItem = this.nextAlarm;
            ClockAlarm? lastItem = null;
            while (alarmListItem != null && alarmListItem.nanos < alarm.nanos) {
                lastItem = alarmListItem;
                alarmListItem = alarmListItem.next;
            }
            if (lastItem != null) {
                lastItem.next = alarm;
                alarm.next = alarmListItem;
            } else {
                this.nextAlarm = alarm;
                alarm.next = alarmListItem;
            }
            alarm.scheduled = true;
            return alarm;
        }

        public bool unlinkAlarm(ClockAlarm alarm) {
            var alarmListItem = this.nextAlarm;
            if (alarmListItem == null) {
                return false;
            }
            ClockAlarm? lastItem = null;
            while (alarmListItem != null) {
                if (alarmListItem == alarm) {
                    if (lastItem != null) {
                        lastItem.next = alarmListItem.next;
                    } else {
                        this.nextAlarm = alarmListItem.next;
                    }
                    return true;
                }
                lastItem = alarmListItem;
                alarmListItem = alarmListItem.next;
            }
            return false;
        }

        public void tick(double deltaNanos) {
            var targetNanos = this.nanosCounter + deltaNanos;
            var alarm = this.nextAlarm;
            while (alarm != null && alarm.nanos <= targetNanos) {
                this.nextAlarm = alarm.next;
                this.nanosCounter = alarm.nanos;
                alarm.callback();
                alarm = this.nextAlarm;
            }
            this.nanosCounter = targetNanos;
        }

        public double nanosToNextAlarm =>
            this.nextAlarm != null
            ? this.nextAlarm.nanos - this.Nanos
            : 0;
    }
}
