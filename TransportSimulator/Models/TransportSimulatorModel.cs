using System.Collections.Generic;
using Avalonia;

public record TransportSimulatorModel(
    IReadOnlyDictionary<string, TransportPath> TransportPaths,
    IReadOnlyList<SolenoidTransportPathJunction> Junctions,
    IReadOnlyList<MergeTransportPath> MergePoints,
    IReadOnlyList<TransportDevice> TransportDevices,

    IReadOnlyDictionary<string, bool> SolenoidJunctionOns,
    IReadOnlyDictionary<string, bool> SensorOns,
    IReadOnlyList<(string ObjectID, IReadOnlyList<IReadOnlyList<Point>> Points)> TransportObjectPoints
);
