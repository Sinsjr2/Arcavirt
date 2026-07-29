using IodefineToSvd;

if (args.Length < 3) {
    Console.Error.WriteLine("使い方: IodefineToSvd <iodefine.hのパス> <出力SVDパス> <deviceName> [--datasheet=<プロバイダ名>] <対象struct名...>");
    return 1;
}

var iodefinePath = args[0];
var outputPath = args[1];
var deviceName = args[2];

var datasheetProviders = new Dictionary<string, IReadOnlyDictionary<string, RegisterDatasheetMetadata>> {
    ["rx64m-cmt"] = Rx64mCmtDatasheetMetadata.Registers,
    ["rx64m-port"] = Rx64mPortDatasheetMetadata.Registers,
    ["rx64m-sci"] = Rx64mSciDatasheetMetadata.Registers,
    ["rx64m-icu"] = Rx64mIcuDatasheetMetadata.Registers,
    ["rx64m-system"] = Rx64mSystemDatasheetMetadata.Registers,
    ["rx64m-scifa"] = Rx64mScifaDatasheetMetadata.Registers,
    ["rx64m-cac"] = Rx64mCacDatasheetMetadata.Registers,
    ["rx64m-crc"] = Rx64mCrcDatasheetMetadata.Registers,
    ["rx64m-da"] = Rx64mDaDatasheetMetadata.Registers,
    ["rx64m-doc"] = Rx64mDocDatasheetMetadata.Registers,
    ["rx64m-temps"] = Rx64mTempsDatasheetMetadata.Registers,
};

var datasheetOptionPrefix = "--datasheet=";
var datasheetOption = args[3..].FirstOrDefault(a => a.StartsWith(datasheetOptionPrefix, StringComparison.Ordinal));

IReadOnlyDictionary<string, RegisterDatasheetMetadata>? datasheetMetadata = null;
if (datasheetOption is not null) {
    var providerName = datasheetOption[datasheetOptionPrefix.Length..];
    if (!datasheetProviders.TryGetValue(providerName, out datasheetMetadata)) {
        Console.Error.WriteLine($"未知のdatasheetプロバイダです: {providerName}");
        return 1;
    }
}

var structNames = args[3..]
    .Where(a => !a.StartsWith(datasheetOptionPrefix, StringComparison.Ordinal))
    .ToHashSet();

var source = File.ReadAllText(iodefinePath);

var structs = IodefineHeaderParser.ParseStructs(source, structNames);
var instances = IodefineHeaderParser.ParseInstances(source, structNames);

var document = SvdDocumentBuilder.Build(
    deviceName,
    $"{deviceName} peripheral registers generated from iodefine.h (pilot).",
    instances,
    structs,
    datasheetMetadata);

document.Save(outputPath);

return 0;