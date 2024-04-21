using Clock;

public class Simulator {

    readonly SimulationClock clock;

    public Simulator(SimulationClock clock) {
        this.clock = clock;
    }

    public void Step() {
        var nanosToNextAlarm = clock.nanosToNextAlarm;
        clock.tick(nanosToNextAlarm);
    }
}
