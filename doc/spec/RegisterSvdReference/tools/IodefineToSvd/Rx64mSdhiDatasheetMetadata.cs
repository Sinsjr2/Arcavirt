namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 49章(SD Host Interface (SDHI)、p.2479-2532の49.2 Register Descriptions)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。SDHIは1インスタンス
/// 構成であり、iodefine.h上もst_sdhiの1インスタンス(SDHI)のみが定義されている。
///
/// 49.2.5節(Response Register 10/32/54/76)はデータシート本文では1節にまとまって
/// 記載されているが、iodefine.h上はSDRSP10/SDRSP32/SDRSP54/SDRSP76の4つの独立した
/// レジスタである。本文は「SDRSP10, SDRSP32, SDRSP54」と「SDRSP76」を2つの図に
/// 分けて示しているが、いずれもリセット値(全ビット0)・アクセス権限(read-only)は
/// 共通と明記されているため、4エントリに分けた上で同一の値を設定した。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとにR/R(/W)/R(/W)混在
/// だがread-writeとして扱った:
/// - SDSTS1: RSPEND/ACEND/SDCDRM/SDCDIN/SDD3RM/SDD3INはR/(W)(1を読み出した後に0を
///   書き込むことでのみクリア可能)、SDCDMON/SDWPMON/SDD3MONはR(外部ピンの
///   モニタフラグ)。
/// - SDSTS2: CMDE/CRCE/ENDE/DTO/ILW/ILR/RSPTO/BRE/BWE/ILAはR/(W)、b11(予約)はR/W、
///   その他予約ビット・SDD0MON/SDCLKCREN/CBSYはR。
/// - SDIOSTS: IOIRQ/EXPUB52/EXWTはR/(W)、b2-b1(予約)はR/W、その他予約ビットはR。
///
/// 部分マスク(Undefinedビット)を使用した箇所:
/// - SDSTS1: SDCDMON(b5)/SDWPMON(b7)/SDD3MON(b10)はリセット値表で"x"と明記され、
///   本文にも「SDHI_D3ピンの状態に応じて値が変わる」等の記載があるため、部分マスク
///   で除外した(ResetMask=0xFFFFFB5F)。
/// - SDSTS2: SDD0MON(b7、SDHI_D0ピンステータス)はリセット値表で"x"と明記されて
///   いるため、部分マスクで除外した(ResetMask=0xFFFFFF7F)。
/// - SDERSTS1: b31-b16は本文に"These bits are undefined when read."と明記されて
///   いるため、部分マスクで除外した(ResetMask=0x0000FFFF)。
/// - SDIOSTS: b2, b1(予約ビット)は本文に"These bits are undefined when read."と
///   明記されているため、部分マスクで除外した(ResetMask=0xFFFFFFF9)。抽出スクリプト
///   の「たたき台」はb0(IOIRQ)をUndefinedと誤検出していたが、本文ではIOIRQのリセット
///   値は0固定であり、undefinedなのはb2・b1の予約ビットであることを本文で確認した。
///
/// null表現を使用した箇所:
/// - SDBUFR: リセット値表全体が"Undefined"と明記されており、単一の固定値を持たない
///   ため、ResetValue=null・ResetMask=0とした。SDBUFRはSDカードへの書き込み・
///   SDカードからの読み出しの両方に使われる旨が本文に明記されているためread-write
///   とした。
///
/// 抽出スクリプトの「たたき台」から修正した箇所(ビット表を持たないプレーンな
/// レジスタで、本文にUndefinedの明記がないもの):
/// - SDARG: 抽出スクリプトはBit/Symbol表を持たないためaccess_candidate=null、かつ
///   一部ビットがnullの部分マスク候補を出していたが、本文にUndefinedの明記はなく、
///   単なる引数設定レジスタ(全ビット0でリセット)であるため、ResetMask=0xFFFFFFFF・
///   read-writeとした。
/// - SDSIZE: 抽出スクリプトはb21をnull(部分マスク候補)としていたが、本文の
///   ビット説明表ではb31-b12はすべて「0 when read and cannot be modified」と明記
///   されており、Undefinedの記載はない。PDFの段組みの乱れによる抽出誤りと判断し、
///   ResetMask=0xFFFFFFFFとした。
/// - SDRSP10/32/54/76: 抽出スクリプトはb20をnull(部分マスク候補)としていたが、
///   本文のビット説明表はb23-b0を「SD card応答値」、b31-b24を「Reserved、0 when
///   read」とのみ記載しており、Undefinedの記載はない。SDSIZEと同様にPDFの段組みの
///   乱れによる抽出誤りと判断し、ResetMask=0xFFFFFFFFとした。
/// </summary>
public static class Rx64mSdhiDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 49.2.1 Command Register (SDCMD)
        ["SDCMD"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 49.2.2 Argument Register (SDARG)
        ["SDARG"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 49.2.3 Data Stop Register (SDSTOP)
        ["SDSTOP"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 49.2.4 Block Count Register (SDBLKCNT) - 本文に「readable/writable register」と明記
        ["SDBLKCNT"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 49.2.5 Response Register 10/32/54/76 (SDRSP10/SDRSP32/SDRSP54/SDRSP76)
        // 4レジスタとも同一のリセット値(全ビット0)・アクセス権限(read-only)であることを本文で確認済み
        ["SDRSP10"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["SDRSP32"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["SDRSP54"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["SDRSP76"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 49.2.6 SD Status Register 1 (SDSTS1)
        // SDCDMON(b5)/SDWPMON(b7)/SDD3MON(b10)はUndefinedのため部分マスクで除外
        ["SDSTS1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFB5F, "read-write"),
        // 49.2.7 SD Status Register 2 (SDSTS2) - SDCLKCREN(b13)=1がリセット値
        // SDD0MON(b7)はUndefinedのため部分マスクで除外
        ["SDSTS2"] = new RegisterDatasheetMetadata(0x00002000, 0xFFFFFF7F, "read-write"),
        // 49.2.8 SD Interrupt Mask Register 1 (SDIMSK1)
        ["SDIMSK1"] = new RegisterDatasheetMetadata(0x0000031D, 0xFFFFFFFF, "read-write"),
        // 49.2.9 SD Interrupt Mask Register 2 (SDIMSK2)
        ["SDIMSK2"] = new RegisterDatasheetMetadata(0x00008B7F, 0xFFFFFFFF, "read-write"),
        // 49.2.10 SDHI Clock Control Register (SDCLKCR) - CLKSEL[7:0]=0x20がリセット値
        ["SDCLKCR"] = new RegisterDatasheetMetadata(0x00000020, 0xFFFFFFFF, "read-write"),
        // 49.2.11 Transfer Data Size Register (SDSIZE)
        ["SDSIZE"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 49.2.12 Card Access Option Register (SDOPT) - CTOP[3:0]/TOP[3:0]/予約ビットがリセット値
        ["SDOPT"] = new RegisterDatasheetMetadata(0x000040EE, 0xFFFFFFFF, "read-write"),
        // 49.2.13 SD Error Status Register 1 (SDERSTS1) - read-only
        // b31-b16は本文に"undefined when read"と明記されているため部分マスクで除外
        ["SDERSTS1"] = new RegisterDatasheetMetadata(0x00002000, 0x0000FFFF, "read-only"),
        // 49.2.14 SD Error Status Register 2 (SDERSTS2) - read-only
        ["SDERSTS2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 49.2.15 SD Buffer Register (SDBUFR) - リセット値表全体が"Undefined"のためnull表現
        ["SDBUFR"] = new RegisterDatasheetMetadata(null, 0x00000000, "read-write"),
        // 49.2.16 SDIO Mode Control Register (SDIOMD)
        ["SDIOMD"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 49.2.17 SDIO Status Register (SDIOSTS)
        // b2, b1(予約ビット)は本文に"undefined when read"と明記されているため部分マスクで除外
        ["SDIOSTS"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFF9, "read-write"),
        // 49.2.18 SDIO Interrupt Mask Register (SDIOIMSK) - EXWTM(b15)/EXPUB52M(b14)=1がリセット値
        ["SDIOIMSK"] = new RegisterDatasheetMetadata(0x0000C007, 0xFFFFFFFF, "read-write"),
        // 49.2.19 DMA Transfer Enable Register (SDDMAEN) - b4/b12(予約, 固定値1)がリセット値
        ["SDDMAEN"] = new RegisterDatasheetMetadata(0x00001010, 0xFFFFFFFF, "read-write"),
        // 49.2.20 SDHI Software Reset Register (SDRST) - b2,b1(予約, 固定値1)/SDRST(b0)=1がリセット値
        ["SDRST"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),
        // 49.2.21 Version Register (SDVER) - read-only
        ["SDVER"] = new RegisterDatasheetMetadata(0x00008A0D, 0xFFFFFFFF, "read-only"),
        // 49.2.22 Swap Control Register (SDSWAP)
        ["SDSWAP"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}