using System.Xml.Linq;

namespace IodefineToSvd;

public static class SvdDocumentBuilder {
    public static XDocument Build(
        string deviceName,
        string description,
        IReadOnlyList<PeripheralInstance> instances,
        IReadOnlyDictionary<string, CStruct> structsByName,
        IReadOnlyDictionary<string, RegisterDatasheetMetadata>? datasheetMetadata = null) {

        var peripheralsElement = new XElement("peripherals");
        foreach (var instance in instances) {
            var structDef = structsByName[instance.StructTypeName];
            var registers = RegisterLayoutBuilder.Resolve(structDef);
            var clusters = RegisterLayoutBuilder.ResolveClusters(structDef);
            var totalSize = RegisterLayoutBuilder.TotalByteSize(structDef);

            var registersElement = new XElement("registers");
            foreach (var register in registers) {
                registersElement.Add(BuildRegisterElement(register, datasheetMetadata));
            }
            foreach (var cluster in clusters) {
                registersElement.Add(BuildClusterElement(cluster, datasheetMetadata));
            }

            var peripheralElement = new XElement("peripheral",
                new XElement("name", instance.Name),
                instance.AlternatePeripheral is { } alternatePeripheral
                    ? new XElement("alternatePeripheral", alternatePeripheral)
                    : null,
                new XElement("baseAddress", $"0x{instance.BaseAddress:X}"),
                new XElement("addressBlock",
                    new XElement("offset", "0x0"),
                    new XElement("size", $"0x{totalSize:X}"),
                    new XElement("usage", "registers")),
                registersElement);

            peripheralsElement.Add(peripheralElement);
        }

        var deviceElement = new XElement("device",
            new XAttribute("schemaVersion", "1.3"),
            new XElement("name", deviceName),
            new XElement("version", "1.0"),
            new XElement("description", description),
            new XElement("addressUnitBits", "8"),
            new XElement("width", "32"),
            peripheralsElement);

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), deviceElement);
    }

    static XElement BuildRegisterElement(ResolvedRegister register, IReadOnlyDictionary<string, RegisterDatasheetMetadata>? datasheetMetadata) {
        // CMSIS-SVDのregisterType内では、dim/dimIncrementはname要素より前に
        // 置く必要がある(dimElementGroupがシーケンスの先頭)。
        var registerElement = new XElement("register",
            register.ArrayCount is { } arrayCount
                ? new XElement("dim", arrayCount)
                : null,
            register.ArrayCount is not null
                ? new XElement("dimIncrement", $"0x{register.ByteSize:X}")
                : null,
            new XElement("name", register.ArrayCount is not null ? $"{register.Name}%s" : register.Name),
            register.AlternateRegister is { } alternateRegister
                ? new XElement("alternateRegister", alternateRegister)
                : null,
            new XElement("addressOffset", $"0x{register.AddressOffset:X}"),
            new XElement("size", register.ByteSize * 8));

        if (datasheetMetadata is not null && datasheetMetadata.TryGetValue(register.Name, out var metadata)) {
            registerElement.Add(new XElement("access", metadata.Access));

            if (metadata.ResetValue is { } resetValue) {
                registerElement.Add(new XElement("resetValue", $"0x{resetValue:X}"));

                var fullMask = register.ByteSize * 8 >= 32 ? 0xFFFFFFFFUL : (1UL << (register.ByteSize * 8)) - 1;
                if (metadata.ResetMask != fullMask) {
                    registerElement.Add(new XElement("resetMask", $"0x{metadata.ResetMask:X}"));
                }
            }
        }

        if (register.Fields.Count > 0) {
            var fieldsElement = new XElement("fields");
            foreach (var field in register.Fields) {
                fieldsElement.Add(new XElement("field",
                    new XElement("name", field.Name),
                    new XElement("bitOffset", field.BitOffset),
                    new XElement("bitWidth", field.BitWidth)));
            }
            registerElement.Add(fieldsElement);
        }

        return registerElement;
    }

    static XElement BuildClusterElement(ResolvedCluster cluster, IReadOnlyDictionary<string, RegisterDatasheetMetadata>? datasheetMetadata) {
        // CMSIS-SVDのclusterTypeもregisterTypeと同様、dim/dimIncrementはname要素より
        // 前に置く必要がある。dim付きレジスタ名の"%s"付与規約(BuildRegisterElement)を
        // クラスター名にもそのまま適用する。
        var clusterElement = new XElement("cluster",
            new XElement("dim", cluster.ArrayCount),
            new XElement("dimIncrement", $"0x{cluster.ByteSize:X}"),
            new XElement("name", $"{cluster.Name}%s"),
            new XElement("description", $"{cluster.Name} register cluster, {cluster.ArrayCount} elements."),
            new XElement("addressOffset", $"0x{cluster.AddressOffset:X}"));

        foreach (var register in cluster.Registers) {
            clusterElement.Add(BuildRegisterElement(register, datasheetMetadata));
        }

        return clusterElement;
    }
}