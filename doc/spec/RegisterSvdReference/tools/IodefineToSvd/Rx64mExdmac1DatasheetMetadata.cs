namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 19章(EXDMA Controller (EXDMACa)、p.659-677の19.2.1-19.2.15)から手動で書き写した、
/// チャネル1のレジスタ群(st_exdmac1、インスタンスEXDMAC1)のレジスタごとの
/// リセット値・アクセス権限。EXDMAC0(st_exdmac0)と同一節・同一リセット値/アクセス
/// 権限のレジスタのみで構成される(EDMOFRが無い点のみst_exdmac0と異なる。EDMAMD本文の
/// 「Offset addition can be specified only for EXDMAC0」の記述どおり)ため、
/// Rx64mExdmac0DatasheetMetadata.csの値をそのまま踏襲した。
/// </summary>
public static class Rx64mExdmac1DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 19.2.1 EXDMA Source Address Register (EDMSAR)
        ["EDMSAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 19.2.2 EXDMA Destination Address Register (EDMDAR)
        ["EDMDAR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 19.2.3 EXDMA Transfer Count Register (EDMCRA)
        ["EDMCRA"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 19.2.4 EXDMA Block Transfer Count Register (EDMCRB)
        ["EDMCRB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 19.2.5 EXDMA Transfer Mode Register (EDMTMD)
        ["EDMTMD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 19.2.6 EXDMA Output Setting Register (EDMOMD)
        ["EDMOMD"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.7 EXDMA Interrupt Setting Register (EDMINT)
        ["EDMINT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.8 EXDMA Address Mode Register (EDMAMD)
        ["EDMAMD"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 19.2.10 EXDMA Transfer Enable Register (EDMCNT)
        ["EDMCNT"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.11 EXDMA Software Start Register (EDMREQ)
        ["EDMREQ"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.12 EXDMA Status Register (EDMSTS) - Rx64mExdmac0DatasheetMetadata参照
        ["EDMSTS"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.13 EXDMA External Request Sense Mode Register (EDMRMD)
        ["EDMRMD"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.14 EXDMA External Request Flag Register (EDMERF)
        ["EDMERF"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 19.2.15 EXDMA Peripheral Request Flag Register (EDMPRF)
        ["EDMPRF"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}