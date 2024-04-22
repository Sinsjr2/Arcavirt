namespace Clock {
    
    public interface IClock {
        double Nanos { get; }
        IAlarm CreateAlarm(Action onTimeElapsed);
    }
}
