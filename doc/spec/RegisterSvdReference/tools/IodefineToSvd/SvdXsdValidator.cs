using System.Xml.Linq;
using System.Xml.Schema;

namespace IodefineToSvd;

public static class SvdXsdValidator {
    public static IReadOnlyList<string> Validate(XDocument document, string xsdPath) {
        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(null, xsdPath);

        var errors = new List<string>();
        document.Validate(schemaSet, (_, e) => errors.Add(e.Message));
        return errors;
    }
}