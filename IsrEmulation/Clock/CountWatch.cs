using System;
using System.Numerics;

// 参考 https://github.com/wokwi/rp2040js/blob/02bc892bd290ce860592b2a40861c7c9c1543bf0/src/clock/simulation-clock.ts

namespace Clock; 

public interface ICountObserver<T> {
    void Schedule(T count);
    void Cancel();
}

public interface ICountObservable<T> {
    /// <summary>
    /// コールバックの引数で現在のカウント値を通知します。
    /// </summary>
    ICountObserver<T> Create(Action<T> callback);
}

public class CountWatcher<T> : ICountObservable<T>
    where T : INumber<T> {
    class CountObserver : ICountObserver<T> {
        public CountObserver? next = null;
        public T count = T.AdditiveIdentity;
        public bool scheduled = false;
        readonly CountWatcher<T> clock;
        public readonly Action<T> callback;

        public CountObserver(CountWatcher<T> clock, Action<T> callback) {
            this.clock = clock;
            this.callback = callback;
        }

        public void Schedule(T count) {
            if (scheduled) {
                this.Cancel();
            }
            clock.LinkAlarm(count, this);
        }

        public void Cancel() {
            clock.UnlinkAlarm(this);
            scheduled = false;
        }
    }

    CountObserver? nextAlarm= null;

    T counter;

    Func<T, T, bool> isPass;

    public CountWatcher(T initialCount, Func<T, T, bool> isPass) {
        counter = initialCount;
        this.isPass = isPass;
    }

    public T Count =>
        counter;

    /// <summary>
    /// 指定した値を超えたかどうかを判定するためのハンドラを設定します。
    /// </summary>
    // public void SetComparer(Func<T, T, bool> isPass) {
    //     this.isPass = isPass;
    // }

    public ICountObserver<T> Create(Action<T> callback) {
        return new CountObserver(this, callback);
    }

    CountObserver LinkAlarm(T nanos, CountObserver alarm) {
        alarm.count = nanos;
        var alarmListItem = nextAlarm;
        CountObserver? lastItem = null;
        while (alarmListItem != null && alarmListItem.count < alarm.count) {
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

    bool UnlinkAlarm(CountObserver alarm) {
        var alarmListItem = nextAlarm;
        if (alarmListItem == null) {
            return false;
        }
        CountObserver? lastItem = null;
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

    public void UpdateCount(T newCount) {
        var alarm = nextAlarm;
        counter = newCount;
        while (alarm != null && isPass(alarm.count, newCount)) {
            nextAlarm = alarm.next;
            counter = alarm.count;
            alarm.callback(newCount);
            alarm = nextAlarm;
        }
    }

    // public T nanosToNextAlarm =>
    //     this.nextAlarm != null
    //     ? this.nextAlarm.count - this.Count
    //     : T.AdditiveIdentity;
}
