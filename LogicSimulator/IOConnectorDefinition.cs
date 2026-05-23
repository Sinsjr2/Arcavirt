namespace LogicSimulator;

public record IOConnectorDefinition(
    string LogicID,
    IReadOnlyList<PinDefinition> InputPins,
    IReadOnlyList<PinDefinition> OutputPins);
