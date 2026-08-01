namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 32章(Realtime Clock (RTCd)、p.1440-1473)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。全レジスタについてマニュアル本文の個別レジスタ節
/// (32.2.1〜32.2.28)と突き合わせて確認済み。
///
/// 32.2節冒頭の注記(p.1441)に「リセット後の値がxと記載されているビットはリセットで
/// 初期化されない」と明記されている。時刻カウンタ(RSECCNT/RMINCNT/RHRCNT/RWKCNT等)・
/// 各種アラームレジスタ(RSECAR/RMINAR/RHRAR/RWKAR/RDAYAR/RMONAR/RYRAREN等)・
/// 時刻キャプチャレジスタ(RSECCPn/RMINCPn/RHRCPn/RDAYCPn/RMONCPn等)は、
/// 予約ビットを含め全ビットが"x"(不定)であるため、ResetValue: null, ResetMask: 0x00
/// として扱う。
///
/// 各レジスタ節の本文に「This register is set to 00h(0000h) by an RTC software reset.」
/// という記述が頻出するが、これはRCR2.RESETビットへの書き込みによる"RTCソフトウェア
/// リセット"の効果であり、iodefine.h/SVDが表現するハードウェアリセット後の値
/// ("Value after reset"欄)とは別のイベントである。本メタデータはハードウェアリセット
/// 後の値(Value after reset欄)のみを採用しており、ソフトウェアリセットの記述は
/// ResetValue/ResetMaskの判断には使用していない。
///
/// 予約ビットの一部がリセットで0固定になっているレジスタ(R64CNT.b7、RDAYCNT.b7-b6、
/// RMONCNT.b7-b5、RYRCNT/RYRAR/BCNT2AER.b15-b8、RCR1.b3,b1、RCR2.b3-b1、RCR3.b7-b4、
/// RCR4.b7-b1、RFRH.b15-b1)は、当該ビットのみResetMaskに含めResetValue=0とし、
/// 残りの不定ビットはResetMaskから除外する。
///
/// AccessはR64CNT・RSECCPn/BCNT0CPn等の時刻キャプチャレジスタは全ビットR(Read)のみ
/// のため"read-only"。それ以外(カウンタ・アラーム・制御レジスタ)は全ビットR/W
/// (一部予約ビットも「Set this bit to 0. It is read as the set value.」等でR/W扱い)の
/// ため"read-write"として扱う(write-onlyレジスタはRTCには存在しない)。
///
/// RSECCNT/BCNT0、RMINCNT/BCNT1、RHRCNT/BCNT2、RWKCNT/BCNT3、RSECAR/BCNT0AR、
/// RMINAR/BCNT1AR、RHRAR/BCNT2AR、RWKAR/BCNT3AR、RDAYAR/BCNT0AER、RMONAR/BCNT1AER、
/// RYRAR/BCNT2AER、RYRAREN/BCNT3AER、RSECCPn/BCNT0CPn、RMINCPn/BCNT1CPn、
/// RHRCPn/BCNT2CPn、RDAYCPn/BCNT3CPnは同一アドレスを共有する別名レジスタ(calendar
/// count mode/binary count modeの切り替え用)。メタデータ辞書はレジスタ名ごとに
/// フラットなKey-Valueであるため、両方に個別にエントリを持つ(抽出JSONLの
/// instance_groupsも各名称ごとに候補値を出しており、今回はいずれのペアも
/// 本文で確認した限り値・アクセス権限に差異はなかった)。
/// </summary>
public static class Rx64mRtcDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        ["R64CNT"] = new RegisterDatasheetMetadata(0x00, 0x80, "read-only"),
        ["RSECCNT"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT0"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RMINCNT"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT1"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RHRCNT"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT2"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RWKCNT"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT3"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RDAYCNT"] = new RegisterDatasheetMetadata(0x00, 0xC0, "read-write"),
        ["RMONCNT"] = new RegisterDatasheetMetadata(0x00, 0xE0, "read-write"),
        ["RYRCNT"] = new RegisterDatasheetMetadata(0x0000, 0xFF00, "read-write"),

        ["RSECAR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT0AR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RMINAR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT1AR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RHRAR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT2AR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RWKAR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT3AR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RDAYAR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT0AER"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RMONAR"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT1AER"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RYRAR"] = new RegisterDatasheetMetadata(0x0000, 0xFF00, "read-write"),
        ["BCNT2AER"] = new RegisterDatasheetMetadata(0x0000, 0xFF00, "read-write"),
        ["RYRAREN"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["BCNT3AER"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),

        ["RCR1"] = new RegisterDatasheetMetadata(0x00, 0x0A, "read-write"),
        ["RCR2"] = new RegisterDatasheetMetadata(0x00, 0x0E, "read-write"),
        ["RCR3"] = new RegisterDatasheetMetadata(0x00, 0xF0, "read-write"),
        ["RCR4"] = new RegisterDatasheetMetadata(0x00, 0xFE, "read-write"),
        ["RFRH"] = new RegisterDatasheetMetadata(0x0000, 0xFFFE, "read-write"),
        ["RFRL"] = new RegisterDatasheetMetadata(null, 0x0000, "read-write"),
        ["RADJ"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RTCCR0"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RTCCR1"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),
        ["RTCCR2"] = new RegisterDatasheetMetadata(null, 0x00, "read-write"),

        ["RSECCP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT0CP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RMINCP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT1CP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RHRCP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT2CP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RDAYCP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT3CP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RMONCP0"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),

        ["RSECCP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT0CP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RMINCP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT1CP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RHRCP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT2CP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RDAYCP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT3CP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RMONCP1"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),

        ["RSECCP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT0CP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RMINCP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT1CP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RHRCP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT2CP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RDAYCP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["BCNT3CP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
        ["RMONCP2"] = new RegisterDatasheetMetadata(null, 0x00, "read-only"),
    };
}