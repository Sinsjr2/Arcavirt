using IodefineToSvd;

if (args.Length < 3) {
    Console.Error.WriteLine("使い方: IodefineToSvd <iodefine.hのパス> <出力SVDパス> <deviceName> <対象struct名...>");
    return 1;
}

var iodefinePath = args[0];
var outputPath = args[1];
var deviceName = args[2];
var structNames = args[3..].ToHashSet();

var source = File.ReadAllText(iodefinePath);

var structs = IodefineHeaderParser.ParseStructs(source, structNames);
var instances = IodefineHeaderParser.ParseInstances(source, structNames);

var document = SvdDocumentBuilder.Build(
    deviceName,
    $"{deviceName} peripheral registers generated from iodefine.h (pilot).",
    instances,
    structs);

document.Save(outputPath);

return 0;