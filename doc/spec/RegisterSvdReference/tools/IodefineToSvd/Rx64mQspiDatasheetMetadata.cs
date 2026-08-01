namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 45章(Quad Serial Peripheral Interface (QSPI), p.2377-2397)から手動で書き写した
/// レジスタごとのリセット値・アクセス権限。全22レジスタについてマニュアル本文の
/// 個別レジスタ節(45.2.1〜45.2.16)と突き合わせて確認済み(2026-08-01)。
///
/// SPDRはst_qspiの無名union(WORD.H/BYTE.HHという"BIT"以外のビットフィールドを
/// 持たない複数視点)がCStructBodyParserにより1つのフィールド無しレジスタ"SPDR"に
/// 統合されるため、メタデータのキーは"SPDR"のみとした("SPDR.H"等は不要)。
/// 全ビットがリセット後"x"(未定義)と明記されているため、MDMONR(rx64m-system)と
/// 同じ扱いで ResetValue=null、ResetMask=0x00000000 とした。アクセス権限は
/// 本文に明示的なビット単位R/W表が無いが、「transmit/receive data buffer」であり
/// 読み書き両方の動作が説明されているため read-write とした。
///
/// SPBRは本文にビット単位のR/W表が無いが、「SPBR register sets the bit rate」
///「If this register is modified」という記述からレジスタ設定用途であることが
/// 明らかなため read-write とした(RSPI(c0i)のRx64mRspiDatasheetMetadata.csの
/// SPBRも同じ構造(サブフィールド名無しのビットレート設定レジスタ)で read-write と
/// されており、同じ方針とした)。
///
/// SPSR/SPSSR/SPBDCRはビット単位でR/(W)・R・R/Wが混在するため、レジスタ全体では
/// read-writeとして扱う(SCIFA(c0i.9)のFDR(データカウントレジスタ、Rのカウンタ
/// ビットとR/Wの予約ビットの混在)と同じ方針)。特にSPBDCR(45.2.15)は抽出
/// スクリプトの候補値が"read-only"だったが、本文表ではRXBC/TXBC(データカウンタ)が
/// R、予約ビット(b7,b6,b15,b14)がR/Wと明記されており、混在のため read-write に
/// 訂正した。
/// </summary>
public static class Rx64mQspiDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["SPCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SSLP"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SPPCR"] = new RegisterDatasheetMetadata(0x06, 0xFF, "read-write"),
        ["SPSR"] = new RegisterDatasheetMetadata(0x60, 0xFF, "read-write"),
        ["SPDR"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
        ["SPSCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SPSSR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SPBR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["SPDCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SPCKD"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SSLND"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SPND"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SPCMD0"] = new RegisterDatasheetMetadata(0xE001, 0xFFFF, "read-write"),
        ["SPCMD1"] = new RegisterDatasheetMetadata(0xE001, 0xFFFF, "read-write"),
        ["SPCMD2"] = new RegisterDatasheetMetadata(0xE001, 0xFFFF, "read-write"),
        ["SPCMD3"] = new RegisterDatasheetMetadata(0xE001, 0xFFFF, "read-write"),
        ["SPBFCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SPBDCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["SPBMUL0"] = new RegisterDatasheetMetadata(0x00000001, 0xFFFFFFFF, "read-write"),
        ["SPBMUL1"] = new RegisterDatasheetMetadata(0x00000001, 0xFFFFFFFF, "read-write"),
        ["SPBMUL2"] = new RegisterDatasheetMetadata(0x00000001, 0xFFFFFFFF, "read-write"),
        ["SPBMUL3"] = new RegisterDatasheetMetadata(0x00000001, 0xFFFFFFFF, "read-write"),
    };
}