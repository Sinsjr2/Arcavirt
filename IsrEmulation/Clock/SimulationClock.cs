using System;

// 参考 https://github.com/wokwi/rp2040js/blob/02bc892bd290ce860592b2a40861c7c9c1543bf0/src/clock/simulation-clock.ts

namespace Clock; 

public class ClockAlarm : IAlarm {
    public ClockAlarm? next = null;
    public double nanos = 0;
    public bool scheduled = false;
    readonly SimulationClock clock;
    public readonly Action callback;

    public ClockAlarm(SimulationClock clock, Action callback) {
        this.clock = clock;
        this.callback = callback;
    }

    public void Schedule(double deltaNanos) {
        if (scheduled) {
            this.Cancel();
        }
        clock.linkAlarm(deltaNanos, this);
    }

    public void Cancel() {
        clock.unlinkAlarm(this);
        scheduled = false;
    }
}

public class SimulationClock: IClock {
    ClockAlarm? nextAlarm= null;

    double nanosCounter = 0;
    readonly double frequency;

    public SimulationClock(double frequency = 125e6) {
        this.frequency = frequency;
    }

    public double Nanos =>
        nanosCounter;

    public double micros =>
        this.Nanos / 1000;

    public IAlarm CreateAlarm(Action callback) {
        return new ClockAlarm(this, callback);
    }

    public ClockAlarm linkAlarm(double nanos, ClockAlarm alarm) {
        alarm.nanos = this.Nanos + nanos;
        var alarmListItem = nextAlarm;
        ClockAlarm? lastItem = null;
        while (alarmListItem != null && alarmListItem.nanos < alarm.nanos) {
            lastItem = alarmListItem;
            alarmListItem = alarmListItem.next;
        }
        if (lastItem != null) {
            lastItem.next = alarm;
            alarm.next = alarmListItem;
        } else {
            nextAlarm = alarm;
            alarm.next = alarmListItem;
        }
        alarm.scheduled = true;
        return alarm;
    }

    public bool unlinkAlarm(ClockAlarm alarm) {
        var alarmListItem = nextAlarm;
        if (alarmListItem == null) {
            return false;
        }
        ClockAlarm? lastItem = null;
        while (alarmListItem != null) {
            if (alarmListItem == alarm) {
                if (lastItem != null) {
                    lastItem.next = alarmListItem.next;
                } else {
                    nextAlarm = alarmListItem.next;
                }
                return true;
            }
            lastItem = alarmListItem;
            alarmListItem = alarmListItem.next;
        }
        return false;
    }

    public void Tick(double deltaNanos) {
        var targetNanos = nanosCounter + deltaNanos;
        var alarm = nextAlarm;
        while (alarm != null && alarm.nanos <= targetNanos) {
            nextAlarm = alarm.next;
            nanosCounter = alarm.nanos;
            alarm.callback();
            alarm = nextAlarm;
        }
        nanosCounter = targetNanos;
    }

    public double nanosToNextAlarm =>
        nextAlarm != null
        ? nextAlarm.nanos - this.Nanos
        : 0;
}
