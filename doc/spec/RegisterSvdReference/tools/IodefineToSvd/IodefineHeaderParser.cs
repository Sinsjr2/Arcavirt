using System.Text.RegularExpressions;

namespace IodefineToSvd;

public static class IodefineHeaderParser {
    static readonly Regex LittleEndianBlockPattern = new(
        @"#ifdef __RX_LITTLE_ENDIAN__\r?\n(?<keep>.*?)\r?\n#else\r?\n.*?\r?\n#endif",
        RegexOptions.Singleline | RegexOptions.Compiled);

    static readonly Regex DefineInstancePattern = new(
        @"#define\s+(?<name>\w+)\s*\(\*\(volatile\s+struct\s+(?<type>\w+)\s*\*\)(?<addr>0x[0-9A-Fa-f]+)\)",
        RegexOptions.Compiled);

    public static IReadOnlyDictionary<string, CStruct> ParseStructs(string source, IReadOnlySet<string> structNames) {
        var normalized = NormalizeLittleEndianBlocks(source);
        var result = new Dictionary<string, CStruct>();
        foreach (var structName in structNames) {
            var body = ExtractStructBody(normalized, structName);
            var tokens = CTokenizer.Tokenize(body);
            var members = CStructBodyParser.Parse(tokens);
            result[structName] = new CStruct(structName, members);
        }
        return result;
    }

    public static IReadOnlyList<PeripheralInstance> ParseInstances(string source, IReadOnlySet<string> structNames) {
        var instances = new List<PeripheralInstance>();
        foreach (Match match in DefineInstancePattern.Matches(source)) {
            var structType = match.Groups["type"].Value;
            if (!structNames.Contains(structType)) {
                continue;
            }
            var name = match.Groups["name"].Value;
            var address = Convert.ToUInt64(match.Groups["addr"].Value, 16);
            instances.Add(new PeripheralInstance(name, structType, address));
        }
        return instances;
    }

    static string NormalizeLittleEndianBlocks(string source) {
        return LittleEndianBlockPattern.Replace(source, "${keep}");
    }

    static string ExtractStructBody(string source, string structName) {
        var headerPattern = new Regex(@"struct\s+" + Regex.Escape(structName) + @"\s*\{");
        var headerMatch = headerPattern.Match(source);
        if (!headerMatch.Success) {
            throw new InvalidOperationException($"struct {structName} が見つかりません。");
        }

        var bodyStart = headerMatch.Index + headerMatch.Length;
        var depth = 1;
        var i = bodyStart;
        while (depth > 0) {
            if (source[i] == '{') {
                depth++;
            } else if (source[i] == '}') {
                depth--;
            }
            i++;
        }
        var bodyEnd = i - 1;
        return source[bodyStart..bodyEnd];
    }
}