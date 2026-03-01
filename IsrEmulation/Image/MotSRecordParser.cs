using System;
using System.Collections.Generic;

public class SRecord {
    public string Type { get; set; }
    public uint Address { get; set; }
    public byte[] Data { get; set; }
    public byte Checksum { get; set; }
    public SRecord(string type, uint address, byte[] data, byte checksum)
    {
        Type = type;
        Address = address;
        Data = data;
        Checksum = checksum;
    }

}

public class SRecordParser {
    public static SRecord Parse(string sRecordLine) {
        if (string.IsNullOrEmpty(sRecordLine) || sRecordLine[0] != 'S') throw new ArgumentException("Invalid S-Record format.");
        var type = sRecordLine.Substring(0, 2);
        int count = Convert.ToInt32(sRecordLine.Substring(2, 2), 16);
        int addressLength = type switch {
            "S0" => 4,
            "S1" => 4,
            "S2" => 6,
            "S3" => 8,
            "S5" => 4,
            "S7" => 8,
            "S8" => 6,
            "S9" => 4,
            _ => throw new NotSupportedException("Unsupported S-Record type.")
        };
        var address = addressLength > 0 ? Convert.ToUInt32(sRecordLine.Substring(4, addressLength), 16) : 0;
        int dataStartIndex = 4 + addressLength;
        int dataEndIndex = sRecordLine.Length - 2;
        var data = new List<byte>();
        for (int i = dataStartIndex; i < dataEndIndex; i += 2) data.Add(Convert.ToByte(sRecordLine.Substring(i, 2), 16));
        var checksum = Convert.ToByte(sRecordLine.Substring(dataEndIndex, 2), 16);
        return new SRecord(type, address, data.ToArray(), checksum);
    }

    public static bool ValidateChecksum(string sRecordLine) {
        byte sum = 0;
        for (int i = 2; i < sRecordLine.Length; i += 2) sum += Convert.ToByte(sRecordLine.Substring(i, 2), 16);
        return sum == 0xFF;
    }
}