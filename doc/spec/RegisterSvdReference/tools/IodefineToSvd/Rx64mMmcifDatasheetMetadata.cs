namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 50章(MultiMediaCard Interface (MMCIF)、p.2534-2557の50.2 Register Descriptions)
/// から手動で書き写したレジスタごとのリセット値・アクセス権限。MMCIFは1インスタンス
/// 構成(MMCIF @ 0008 8500h)で、iodefine.h上もMMCIF単一インスタンスがst_mmcif
/// struct(21レジスタ)として定義されている。50.2.1(CECMDSET)から50.2.18
/// (CEVERSION)まで18節が存在し、50.2.8がCERESP0〜CERESP3の4レジスタをまとめて
/// 説明しているため、18節で21レジスタとちょうど一致する。
///
/// CECMDSET・CEDATA・CEINT・CEDETECTはiodefine.h上でビットフィールド定義部分が
/// コメントアウトされている(union { unsigned long LONG; // struct {...} BIT; })が、
/// パーサーはunion自体は1レジスタとして解決するため、レジスタ単位のメタデータ
/// 付与に支障はない。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとに異なるR/W区分が
/// 混在するがread-writeとして扱った:
/// - CECLKCTRL: 大半がR/Wだがb31(MMCBUSBSY)のみR
/// - CEINT・CEINTEN: フラグビットがR/(W)(書き込みで0にのみできる)、予約ビットがR
/// - CEHOSTSTS2: フラグビットがR、予約ビットがR/W
/// - CEDETECT: フラグビットがR/(W)・R、予約ビットがR/W
///
/// 一部ビットのみ未定義のケースは「値+部分マスク」で表現した:
/// - CEHOSTSTS1: リセット値表でb30(CMDSIG)・b23-16(DATSIG[7:0])が"x"(Undefined)と
///   明記されているため、ResetValue=0x00000000、ResetMask=0xBF00FFFF
///   (b30・b23-16を除外)。全ビットRのためAccessはread-onlyとした。
/// - CEDETECT: リセット値表でb14(CDSIG)・b10(予約、ビット説明表でも
///   「This bit is undefined when read.」と明記)が"x"のため、ResetValue=0x00000000、
///   ResetMask=0xFFFFBBFF(b14・b10を除外)。
///
/// CERESP0〜CERESP3・CERESPCMD12は本文中に明記の「read-only registers」のため
/// read-onlyとした。
///
/// CEARG・CEARGCMD12・CEDATAはビット単位のR/W表が無い(コメントアウトされた
/// bitfieldか、単一フィールドのみ)が、コマンド引数・バッファアクセス用の
/// 設定/データレジスタとして読み書き可能と判断しread-writeとした
/// (ドキュメント上に明示のR/W表が無い推測であることに注意)。
///
/// CEVERSION: VERSION[15:0]は本文記載の通りリセット値表のb2ビットが1であることから
/// 0x0004と判断した(read-only fieldのVERSIONとread-writeのSWRST・予約ビットが
/// 混在するためAccess全体はread-writeとした)。
/// </summary>
public static class Rx64mMmcifDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 50.2.1 Command Setting Register (CECMDSET)
        ["CECMDSET"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.2 Argument Register (CEARG) - ビット単位R/W表は無いが引数設定用レジスタ
        ["CEARG"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.3 Automatically Issued CMD12 Argument Register (CEARGCMD12)
        ["CEARGCMD12"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.4 Command Control Register (CECMDCTRL)
        ["CECMDCTRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.5 Transfer Block Setting Register (CEBLOCKSET) - BLKSIZ[15:0]のb9=1が初期値
        ["CEBLOCKSET"] = new RegisterDatasheetMetadata(0x00000200, 0xFFFFFFFF, "read-write"),
        // 50.2.6 Clock Control Register (CECLKCTRL) - b31(MMCBUSBSY)のみR、他はR/W
        ["CECLKCTRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.7 Buffer Access Setting Register (CEBUFACC)
        ["CEBUFACC"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.8 Response Register 3 (CERESP3) - read-only
        ["CERESP3"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 50.2.8 Response Register 2 (CERESP2) - read-only
        ["CERESP2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 50.2.8 Response Register 1 (CERESP1) - read-only
        ["CERESP1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 50.2.8 Response Register 0 (CERESP0) - read-only
        ["CERESP0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 50.2.9 Automatically Issued CMD12 Response Register (CERESPCMD12) - read-only
        ["CERESPCMD12"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 50.2.10 Data Register (CEDATA) - ビット単位R/W表は無いがバッファアクセス用レジスタ
        ["CEDATA"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.11 Boot Operation Setting Register (CEBOOT)
        ["CEBOOT"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.12 Interrupt Status Flag Register (CEINT) - フラグはR/(W)、予約ビットはR
        ["CEINT"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.13 Interrupt Request Enable Register (CEINTEN) - 有効化ビットはR/W、予約ビットはR
        ["CEINTEN"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.14 Status Register 1 (CEHOSTSTS1) - read-only、b30(CMDSIG)・b23-16(DATSIG)が
        // Undefinedのため部分マスクで除外
        ["CEHOSTSTS1"] = new RegisterDatasheetMetadata(0x00000000, 0xBF00FFFF, "read-only"),
        // 50.2.15 Status Register 2 (CEHOSTSTS2) - フラグはR、予約ビットはR/W
        ["CEHOSTSTS2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.16 MMC Detection and Port Control Register (CEDETECT) - b14(CDSIG)・b10(予約、
        // "undefined when read"と明記)がUndefinedのため部分マスクで除外
        ["CEDETECT"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFBBFF, "read-write"),
        // 50.2.17 Special Mode Setting Register (CEADDMODE)
        ["CEADDMODE"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 50.2.18 Version Register (CEVERSION) - VERSION[15:0]=0004h(b2=1)が初期値
        ["CEVERSION"] = new RegisterDatasheetMetadata(0x00000004, 0xFFFFFFFF, "read-write"),
    };
}