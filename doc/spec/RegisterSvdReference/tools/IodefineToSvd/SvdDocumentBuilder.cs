using System.Xml.Linq;

namespace IodefineToSvd;

public static class SvdDocumentBuilder {
    public static XDocument Build(
        string deviceName,
        string description,
        IReadOnlyList<PeripheralInstance> instances,
        IReadOnlyDictionary<string, CStruct> structsByName) {

        var peripheralsElement = new XElement("peripherals");
        foreach (var instance in instances) {
            var structDef = structsByName[instance.StructTypeName];
            var registers = RegisterLayoutBuilder.Resolve(structDef);
            var totalSize = RegisterLayoutBuilder.TotalByteSize(structDef);

            var registersElement = new XElement("registers");
            foreach (var register in registers) {
                registersElement.Add(BuildRegisterElement(register));
            }

            var peripheralElement = new XElement("peripheral",
                new XElement("name", instance.Name),
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

    static XElement BuildRegisterElement(ResolvedRegister register) {
        var registerElement = new XElement("register",
            new XElement("name", register.Name),
            new XElement("addressOffset", $"0x{register.AddressOffset:X}"),
            new XElement("size", register.ByteSize * 8));

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
}