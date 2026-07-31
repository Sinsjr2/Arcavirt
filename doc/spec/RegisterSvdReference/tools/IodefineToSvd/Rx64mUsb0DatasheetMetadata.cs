namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 38章(USB 2.0 FS Host/Function Module (USBb)、p.1735-1843の38.2 Register Descriptions)
/// から手動で書き写した、チャネルブロック(st_usb0、インスタンスUSB0)のレジスタごとの
/// リセット値・アクセス権限。レジスタ数が63個と多いため、SYSCFG・DVSTCTR0・INTSTS0・
/// FRMNUM・DCPCTR・PIPEnCTR・PHYSLEWなど代表的なレジスタについてマニュアル本文と
/// 個別に突き合わせて確認済み(2026-08-01)。全レジスタを1つずつ完全に照合した
/// わけではない。st_usb(共有ブロック、インスタンスUSB)は別ファイル
/// (Rx64mUsbDatasheetMetadata.cs)を参照。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとにR/R(/W)/R(/W)*1
/// (1回のみ変更可)混在だがread-writeとして扱った:
/// - CFIFOCTR/D0FIFOCTR/D1FIFOCTR: DTLN[8:0]・FRDYはR、BCLRは"R/W*1"(読み出しは常に
///   0)、BVALと予約ビットはR/W。
/// - INTSTS0: CTSQ[2:0]・DVSQ[2:0]・BRDY・NRDY・BEMP・VBSTSはR、CTRT/DVST/SOFR/RESM/
///   VBINTは"R/W*4"(該当フラグに0を書き込んだ場合のみクリア)、VALIDと予約ビットは
///   R/W。
/// - INTSTS1・BRDYSTS・NRDYSTS・BEMPSTS: 各ステータスビットは"R/W*1"(該当フラグに
///   0を書き込んだ場合のみクリア)、予約ビットはR/W。
/// - FRMNUM: FRNM[10:0]はR、CRCE・OVRNは"R/W*1"(0を書き込んでクリア)、予約ビットは
///   R/W。
/// - DCPCTR/PIPEnCTR(n = 1 to 9): PBUSY・SQMON・BSTS(・INBUFM)はR、SQSET・SQCLRは
///   "R/W*1"(読み出しは常に0)、その他の制御ビット・予約ビットはR/W。
/// - DPUSR系は別ファイル(st_usb)のため対象外。
///
/// 抽出スクリプトの「たたき台」から修正した箇所:
/// - FRMNUM: 「たたき台」はaccess_candidate="read-only"としていたが、上記の通り
///   CRCE/OVRNが書き込み可能な"R/W*1"ビットであるため、read-writeに修正した。
/// - USBVAL/USBINDX/USBLENG: Bit/Symbol表を持たないプレーンなレジスタのため
///   access_candidate=nullだったが、本文に「ホストコントローラ選択時は書き込み、
///   ファンクションコントローラ選択時は読み出し」の旨が明記されており、Undefinedの
///   記載もないため、read-writeとした。
/// - PIPE1TRN-PIPE5TRN: 同様にBit/Symbol表を持たないプレーンなレジスタで
///   access_candidate=nullだったが、本文に「書き込み時は設定、読み出し時は
///   カウント値を示す」旨が明記されているため、read-writeとした。
/// - INTSTS0: 「たたき台」はbit6(DVSQ[2:0]の一部)をnull検出し
///   reset_value_candidate=0x0010・reset_mask_candidate=0xFFBFとしていたが、これは
///   b12(DVST)・b4(DVSQ[2:0]の最下位ビット)の"0/1"表記(注記: MCUリセット時は0、
///   USBバスリセット後は1/001b)がPDFの2行折り返しで隣接ビット列にずれ込んだ抽出
///   誤りと判断した。本プロジェクトではSVDの"value after reset"をMCUリセット直後の
///   値として扱う方針のため、当該ビットも含め全ビット0(ResetMask=0xFFFF、部分マスク
///   なし)とした。
/// - PIPEMAXP: 「たたき台」はbit6をnull検出しreset_mask_candidate=0xFFBFとしていたが、
///   これも同様にNote 1(「PIPESELで未選択のときは0000h、選択時は0040h」)の"0/1*1"
///   表記の抽出誤りと判断した。本文のPIPESEL節に「PIPESEL[3:0]=0000bのときPIPECFG・
///   PIPEMAXP・PIPEPERIの全ビットは0を読み出す」と明記されており、MCUリセット直後は
///   PIPESELが0000bであるため、MCUリセット直後の値は全ビット0で確定する
///   (ResetMask=0xFFFF、部分マスクなし)。
/// </summary>
public static class Rx64mUsb0DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 38.2.1 System Configuration Control Register (SYSCFG)
        ["SYSCFG"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.2 System Configuration Status Register 0 (SYSSTS0) - 全ビットR
        ["SYSSTS0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        // 38.2.3 Device State Control Register 0 (DVSTCTR0)
        ["DVSTCTR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.4 CFIFO Port Register (CFIFO)
        ["CFIFO"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.4 D0FIFO Port Register (D0FIFO)
        ["D0FIFO"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.4 D1FIFO Port Register (D1FIFO)
        ["D1FIFO"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.5 CFIFO Port Select Register (CFIFOSEL)
        ["CFIFOSEL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.6 CFIFO Port Control Register (CFIFOCTR) - 上記コメント参照
        ["CFIFOCTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.5 D0FIFO Port Select Register (D0FIFOSEL)
        ["D0FIFOSEL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.6 D0FIFO Port Control Register (D0FIFOCTR) - 上記コメント参照
        ["D0FIFOCTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.5 D1FIFO Port Select Register (D1FIFOSEL)
        ["D1FIFOSEL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.6 D1FIFO Port Control Register (D1FIFOCTR) - 上記コメント参照
        ["D1FIFOCTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.7 Interrupt Enable Register 0 (INTENB0)
        ["INTENB0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.8 Interrupt Enable Register 1 (INTENB1)
        ["INTENB1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.9 BRDY Interrupt Enable Register (BRDYENB)
        ["BRDYENB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.10 NRDY Interrupt Enable Register (NRDYENB)
        ["NRDYENB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.11 BEMP Interrupt Enable Register (BEMPENB)
        ["BEMPENB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.12 SOF Output Configuration Register (SOFCFG)
        ["SOFCFG"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.13 Interrupt Status Register 0 (INTSTS0) - 上記コメント参照(たたき台修正)
        ["INTSTS0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.14 Interrupt Status Register 1 (INTSTS1)
        ["INTSTS1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.15 BRDY Interrupt Status Register (BRDYSTS)
        ["BRDYSTS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.16 NRDY Interrupt Status Register (NRDYSTS)
        ["NRDYSTS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.17 BEMP Interrupt Status Register (BEMPSTS)
        ["BEMPSTS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.18 Frame Number Register (FRMNUM) - 上記コメント参照(たたき台修正)
        ["FRMNUM"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.19 Device State Change Register (DVCHGR)
        ["DVCHGR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.20 USB Address Register (USBADDR)
        ["USBADDR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.21 USB Request Type Register (USBREQ)
        ["USBREQ"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.22 USB Request Value Register (USBVAL) - 上記コメント参照(たたき台修正)
        ["USBVAL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.23 USB Request Index Register (USBINDX) - 上記コメント参照(たたき台修正)
        ["USBINDX"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.24 USB Request Length Register (USBLENG) - 上記コメント参照(たたき台修正)
        ["USBLENG"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.25 DCP Configuration Register (DCPCFG)
        ["DCPCFG"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.26 DCP Maximum Packet Size Register (DCPMAXP) - MXPS[6:0]初期値40h(64バイト)
        ["DCPMAXP"] = new RegisterDatasheetMetadata(0x0040, 0xFFFF, "read-write"),
        // 38.2.27 DCP Control Register (DCPCTR) - SQMON(b6)=1がリセット値
        ["DCPCTR"] = new RegisterDatasheetMetadata(0x0040, 0xFFFF, "read-write"),
        // 38.2.28 Pipe Window Select Register (PIPESEL)
        ["PIPESEL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.29 Pipe Configuration Register (PIPECFG)
        ["PIPECFG"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.30 Pipe Maximum Packet Size Register (PIPEMAXP) - 上記コメント参照(たたき台修正)
        ["PIPEMAXP"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.31 Pipe Cycle Control Register (PIPEPERI)
        ["PIPEPERI"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.32 Pipe n Control Registers (PIPEnCTR) (n = 1 to 9) - 上記コメント参照
        ["PIPE1CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE2CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE3CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE4CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE5CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE6CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE7CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE8CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE9CTR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.33 Pipe n Transaction Counter Enable Register (PIPEnTRE) (n = 1 to 5)
        ["PIPE1TRE"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.34 Pipe n Transaction Counter Register (PIPEnTRN) (n = 1 to 5) - 上記コメント参照(たたき台修正)
        ["PIPE1TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE2TRE"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE2TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE3TRE"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE3TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE4TRE"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE4TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE5TRE"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE5TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.35 Device Address n Configuration Register (DEVADDn) (n = 0 to 5)
        ["DEVADD0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DEVADD1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DEVADD2"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DEVADD3"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DEVADD4"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["DEVADD5"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 38.2.36 PHY Cross Point Adjustment Register (PHYSLEW) - SLEWR00=0/SLEWR01=SLEWF00=SLEWF01=1
        ["PHYSLEW"] = new RegisterDatasheetMetadata(0x0000000E, 0xFFFFFFFF, "read-write"),
    };
}