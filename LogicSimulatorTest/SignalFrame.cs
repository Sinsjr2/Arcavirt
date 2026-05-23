namespace LogicSimulatorTest;
public record SignalFrame(IReadOnlyList<PinValue> Inputs, IReadOnlyList<PinValue> Expecteds);
