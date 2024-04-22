namespace Clock {

    public interface IAlarm {
        void Schedule(double deltaNanos);
        void Cancel();
    }
}
