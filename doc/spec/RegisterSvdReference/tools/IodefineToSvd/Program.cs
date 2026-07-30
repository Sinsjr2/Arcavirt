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
    ["rx64m-wdt"] = Rx64mWdtDatasheetMetadata.Registers,
    ["rx64m-iwdt"] = Rx64mIwdtDatasheetMetadata.Registers,
    ["rx64m-mpu"] = Rx64mMpuDatasheetMetadata.Registers,
    ["rx64m-eccram"] = Rx64mEccramDatasheetMetadata.Registers,
    ["rx64m-elc"] = Rx64mElcDatasheetMetadata.Registers,
    ["rx64m-bsc"] = Rx64mBscDatasheetMetadata.Registers,
    ["rx64m-pdc"] = Rx64mPdcDatasheetMetadata.Registers,
    ["rx64m-ssi"] = Rx64mSsiDatasheetMetadata.Registers,
    ["rx64m-src"] = Rx64mSrcDatasheetMetadata.Registers,
    ["rx64m-ppg"] = Rx64mPpgDatasheetMetadata.Registers,
    ["rx64m-gpt"] = Rx64mGptDatasheetMetadata.Registers,
    ["rx64m-riic"] = Rx64mRiicDatasheetMetadata.Registers,
    ["rx64m-etherc"] = Rx64mEthercDatasheetMetadata.Registers,
    ["rx64m-mmcif"] = Rx64mMmcifDatasheetMetadata.Registers,
    ["rx64m-rspi"] = Rx64mRspiDatasheetMetadata.Registers,
    ["rx64m-sdhi"] = Rx64mSdhiDatasheetMetadata.Registers,
    ["rx64m-dmac"] = Rx64mDmacDatasheetMetadata.Registers,
    ["rx64m-exdmac"] = Rx64mExdmacDatasheetMetadata.Registers,
    ["rx64m-poe"] = Rx64mPoeDatasheetMetadata.Registers,
    ["rx64m-ptpedmac"] = Rx64mPtpedmacDatasheetMetadata.Registers,
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