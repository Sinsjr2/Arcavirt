namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 39章(USB2.0 Full-Speed Host/Function Module (USBA)、39.2 Register Descriptions)から
/// 全レジスタ(74個、iodefine.hのst_usba実物と1件ずつ突き合わせ済み)について、マニュアル
/// 本文の"Value after reset"図とBit/Symbol/R/W表を1ビットずつ照合して書き写した、
/// レジスタごとのリセット値・アクセス権限。st_usb0/st_usb(USBbモジュール)とは別の
/// ペリフェラル(USBA)であり、共通の親コードはあるが個々のレジスタのリセット値は
/// 独立に確認している。
///
/// ResetMaskの判定方針(重要、抽出スクリプトの「たたき台」から機械的に転記していない):
/// Bit/Symbol表の説明文に「undefined when read」(またはビット単位の同義表現)と明記されて
/// いるビットのみをResetMaskから除外する。逆に「0 when read」「1 when read」「read as 0」
/// など具体的な固定値が明記されているビットは、"Value after reset"図がノイズで"x"表示に
/// なっていても既知(0/1固定)として扱う。この方針は、"Value after reset"図の一部ビットが
/// pdftotextの桁ずれ・脚注干渉で"x"と"0/1"が矛盾する箇所が多数見つかったため採用した
/// (例: PHYSETのReserved bit群、PIPECFG/PIPEBUF/PIPEMAXP/PIPEPERIのReserved bit群は、
/// 本文が明確に"0 when read"と述べているにもかかわらず図では"x"表示になっている)。
/// 本文に固定値の記載が一切なく、かつ外部ピン/ライン状態を反映するモニタフラグ
/// (SYSSTS0.LNST/IDMON/OVCMON等)は、"Value after reset"図の"x"をそのまま採用した。
///
/// 抽出スクリプトの「たたき台」(access_candidate/reset_value_candidate/reset_mask_candidate)
/// から、本文照合の結果修正した主な箇所:
/// - BUSWAIT: たたき台はReset値0x0000/Mask0x3030としていたが、本文はBWAIT[3:0]の初期値が
///   1111b(明記)、b11-b8が"1 when read"、b5-b4/b13-b12が"0 when read"と明記しており、
///   Reset値0x0F0F/Mask0x3F3Fが正しい(b7-b6・b15-b14のみ真にundefined)。
/// - CFIFO/D0FIFO/D1FIFO: たたき台はMask0x7FFFFFFF(bit31欠落)だったが、32bit全ビットが
///   FIFO Portデータビット(R/W、undefined記載なし)のため0xFFFFFFFFが正しい。
/// - CFIFOSEL: たたき台はMask0xCD0Fだったが、ISEL(b5)は"undefined when read"の記載がなく
///   明確な機能ビットのため既知に含め、0xCD2Fが正しい。
/// - DCPCTR: たたき台はMask0xE0F3だったが、本文を1ビットずつ確認した結果、SQSET(b7)・
///   SQCLR(b8)・SUREQCLR(b11)は"This bit is read as 0."、reserved(b13)は"0 when read"と
///   明記されており既知(値0)に含め、reserved(b4,b3)・reserved(b10,b9)・reserved(b12)は
///   "undefined when read"のため除外し、Mask0xE9E7が正しい。
/// - PHYSET: たたき台はMask0x8B3Bだったが、全Reservedビットが"0 when read"/"read as 0"と
///   明記されており"undefined when read"の記載が1つもないため、全16bit既知(Mask0xFFFF)が
///   正しい。
/// - PIPECFG/PIPEBUF/PIPEMAXP/PIPEPERI: たたき台は部分マスクだったが、(1) いずれのレジスタ
///   にも"undefined when read"の記載が無く全Reservedビットが"0 when read"と明記されている
///   こと、(2) 39.2.31 Pipe Window Select Register(PIPESEL)の本文に「PIPESEL[3:0]が0000bの
///   とき、PIPECFG・PIPEBUF・PIPEMAXP・PIPEPERIレジスタの読み出し値は全ビット0」と明記
///   されており、MCUリセット直後はPIPESELが0000bであること、の二重の根拠により、
///   4レジスタとも全ビット既知・値0(Mask=0xFFFF、Value=0x0000)とした。
/// - PIPEnCTR(n = 1 to 9): たたき台はMask0x47E3(BSTS(b15)を除外)だったが、BSTSは
///   "undefined when read"の記載がない意味のあるRフラグのため既知に含め、Mask0xC7E3が
///   正しい(Value=0x0040、SQMON(b6)がリセット時1)。
///   判断に迷った点: 本文のNote 3に「ATREPM(b10)ビットとINBUFM(b14)フラグは、
///   PIPE6CTR~PIPE9CTRレジスタでは予約(undefined when read)」と明記されている。
///   本ファイルはPIPE1CTR~PIPE9CTRを1つの共通定義として扱う既存方針(Rx64mUsb0DatasheetMetadata.cs
///   と同様)に倣い、PIPE1CTR~PIPE5CTRの定義(ATREPM/INBUFMを意味のあるビットとして既知に
///   含める)を代表値として採用した。PIPE6CTR~PIPE9CTRについては、この2ビットが
///   実際にはundefinedである点で本メタデータのMaskがわずかに楽観的(過大)になる。
/// - BCCTRL: たたき台はMask0x031Fだったが、"undefined when read"の記載が1つもないため
///   全16bit既知(Mask0xFFFF)とした。
/// - INTSTS0: たたき台はReset値0x1010としていたが、これは"Value after reset"図中の
///   DVST(b12)・DVSQ[0](b4)の表記"0/1*1"(脚注: 「MCUリセット時は0、USBバスリセット後は1」)
///   を"1"と誤読したものと判断した。脚注はMCUリセット直後の値を明確に0と述べているため、
///   Reset値は0x0000が正しい(Mask0xFF7Fは変更なし、VBSTS(b7)はUSBA_VBUSピンの状態を
///   反映するモニタフラグのためundefined)。
/// - USBADDR: 本文のBit/Symbol表でUSBADDR[6:0]がRのみ(R/Wではない)と明記されているため
///   read-onlyとした(姉妹モジュールUSB0のUSBADDRはread-writeだが、モジュールが異なる
///   ため別々に確認した結果である)。
/// - USBREQ/USBVAL/USBINDX/USBLENG: たたき台はBit/Symbol表が検出できずaccess=nullだったが、
///   本文に"R/W *1"(ホストコントローラ動作時は読み書き可、ファンクションコントローラ
///   動作時は読み出しのみで書き込み無効)と明記されているためread-writeとした。
///
/// 繰り返しレジスタ(PIPEnCTR n=1~9、PIPEnTRE/PIPEnTRN n=1~5、DEVADDm m=0~5)は、
/// 本文の代表節の値を対象レジスタ全てにコピーしている。
/// </summary>
public static class Rx64mUsbaDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 39.2.1 System Configuration Control Register (SYSCFG)
        ["SYSCFG"] = new RegisterDatasheetMetadata(0x0020, 0x01F1, "read-write"),
        // 39.2.2 CPU Bus Wait Register (BUSWAIT) - 上記コメント参照(たたき台修正)
        ["BUSWAIT"] = new RegisterDatasheetMetadata(0x0F0F, 0x3F3F, "read-write"),
        // 39.2.3 System Configuration Status Register (SYSSTS0) - 全ビットR
        ["SYSSTS0"] = new RegisterDatasheetMetadata(0x0000, 0x0060, "read-only"),
        // 39.2.4 PLL Status Register (PLLSTA) - 全ビットR
        ["PLLSTA"] = new RegisterDatasheetMetadata(0x0000, 0x0001, "read-only"),
        // 39.2.5 Device State Control Register 0 (DVSTCTR0)
        ["DVSTCTR0"] = new RegisterDatasheetMetadata(0x0000, 0x0FF7, "read-write"),
        // 39.2.6 CFIFO/D0FIFO/D1FIFO Port Register - 上記コメント参照(たたき台修正、bit31欠落)
        ["CFIFO"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["D0FIFO"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["D1FIFO"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 39.2.7 CFIFO Port Select Register (CFIFOSEL) - 上記コメント参照(たたき台修正)
        ["CFIFOSEL"] = new RegisterDatasheetMetadata(0x0000, 0xCD2F, "read-write"),
        // 39.2.9 CFIFO/D0FIFO/D1FIFO Port Control Register
        ["CFIFOCTR"] = new RegisterDatasheetMetadata(0x0000, 0xEFFF, "read-write"),
        // 39.2.8 D0FIFO/D1FIFO Port Select Register
        ["D0FIFOSEL"] = new RegisterDatasheetMetadata(0x0000, 0xFD0F, "read-write"),
        ["D0FIFOCTR"] = new RegisterDatasheetMetadata(0x0000, 0xEFFF, "read-write"),
        ["D1FIFOSEL"] = new RegisterDatasheetMetadata(0x0000, 0xFD0F, "read-write"),
        ["D1FIFOCTR"] = new RegisterDatasheetMetadata(0x0000, 0xEFFF, "read-write"),
        // 39.2.10 Interrupt Enable Register 0 (INTENB0)
        ["INTENB0"] = new RegisterDatasheetMetadata(0x0000, 0xFF00, "read-write"),
        // 39.2.11 Interrupt Enable Register 1 (INTENB1)
        ["INTENB1"] = new RegisterDatasheetMetadata(0x0000, 0xDB71, "read-write"),
        // 39.2.12 BRDY Interrupt Enable Register (BRDYENB)
        ["BRDYENB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.13 NRDY Interrupt Enable Register (NRDYENB)
        ["NRDYENB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.14 BEMP Interrupt Enable Register (BEMPENB)
        ["BEMPENB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.15 SOF Output Configuration Register (SOFCFG)
        ["SOFCFG"] = new RegisterDatasheetMetadata(0x0000, 0x0170, "read-write"),
        // 39.2.16 PHY Setting Register (PHYSET) - 上記コメント参照(たたき台修正)
        ["PHYSET"] = new RegisterDatasheetMetadata(0x0033, 0xFFFF, "read-write"),
        // 39.2.17 Interrupt Status Register 0 (INTSTS0) - 上記コメント参照(たたき台修正)
        ["INTSTS0"] = new RegisterDatasheetMetadata(0x0000, 0xFF7F, "read-write"),
        // 39.2.18 Interrupt Status Register 1 (INTSTS1)
        ["INTSTS1"] = new RegisterDatasheetMetadata(0x0000, 0xDB71, "read-write"),
        // 39.2.19 BRDY Interrupt Status Register (BRDYSTS)
        ["BRDYSTS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.20 NRDY Interrupt Status Register (NRDYSTS)
        ["NRDYSTS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.21 BEMP Interrupt Status Register (BEMPSTS)
        ["BEMPSTS"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.22 Frame Number Register (FRMNUM)
        ["FRMNUM"] = new RegisterDatasheetMetadata(0x0000, 0xC7FF, "read-write"),
        // 39.2.23 USB Address Register (USBADDR) - 上記コメント参照(全ビットR)
        ["USBADDR"] = new RegisterDatasheetMetadata(0x0000, 0x007F, "read-only"),
        // 39.2.24 USB Request Type Register (USBREQ) - 上記コメント参照(たたき台修正)
        ["USBREQ"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.25 USB Request Value Register (USBVAL) - 上記コメント参照(たたき台修正)
        ["USBVAL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.26 USB Request Index Register (USBINDX) - 上記コメント参照(たたき台修正)
        ["USBINDX"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.27 USB Request Length Register (USBLENG) - 上記コメント参照(たたき台修正)
        ["USBLENG"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.28 Default Control Pipe Configuration Register (DCPCFG)
        ["DCPCFG"] = new RegisterDatasheetMetadata(0x0000, 0x0190, "read-write"),
        // 39.2.29 Default Control Pipe Maximum Packet Size Register (DCPMAXP) - MXPS[6:0]初期値40h(64バイト)
        ["DCPMAXP"] = new RegisterDatasheetMetadata(0x0040, 0xF07F, "read-write"),
        // 39.2.30 Default Control Pipe Control Register (DCPCTR) - 上記コメント参照(たたき台修正)
        ["DCPCTR"] = new RegisterDatasheetMetadata(0x0000, 0xE9E7, "read-write"),
        // 39.2.31 Pipe Window Select Register (PIPESEL)
        ["PIPESEL"] = new RegisterDatasheetMetadata(0x0000, 0x000F, "read-write"),
        // 39.2.32 Pipe Configuration Register (PIPECFG) - 上記コメント参照(たたき台修正、PIPESEL=0000bで全ビット0確定)
        ["PIPECFG"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.33 Pipe Buffer Register (PIPEBUF) - 上記コメント参照(たたき台修正、PIPESEL=0000bで全ビット0確定)
        ["PIPEBUF"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.34 Pipe Maximum Packet Size Register (PIPEMAXP) - 上記コメント参照(たたき台修正、PIPESEL=0000bで全ビット0確定)
        ["PIPEMAXP"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.35 Pipe Cycle Control Register (PIPEPERI) - 上記コメント参照(たたき台修正、PIPESEL=0000bで全ビット0確定)
        ["PIPEPERI"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.36 Pipe n Control Registers (PIPEnCTR) (n = 1 to 9) - 上記コメント参照(たたき台修正、Note3の判断に迷った点あり)
        ["PIPE1CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE2CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE3CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE4CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE5CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE6CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE7CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE8CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        ["PIPE9CTR"] = new RegisterDatasheetMetadata(0x0040, 0xC7E3, "read-write"),
        // 39.2.37 Pipe n Transaction Counter Enable Register (PIPEnTRE) (n = 1 to 5)
        // 39.2.38 Pipe n Transaction Counter Register (PIPEnTRN) (n = 1 to 5)
        ["PIPE1TRE"] = new RegisterDatasheetMetadata(0x0000, 0x0300, "read-write"),
        ["PIPE1TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE2TRE"] = new RegisterDatasheetMetadata(0x0000, 0x0300, "read-write"),
        ["PIPE2TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE3TRE"] = new RegisterDatasheetMetadata(0x0000, 0x0300, "read-write"),
        ["PIPE3TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE4TRE"] = new RegisterDatasheetMetadata(0x0000, 0x0300, "read-write"),
        ["PIPE4TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["PIPE5TRE"] = new RegisterDatasheetMetadata(0x0000, 0x0300, "read-write"),
        ["PIPE5TRN"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.39 Device Address m Configuration Register (DEVADDm) (iodefine.hはm = 0 to 5のみ)
        ["DEVADD0"] = new RegisterDatasheetMetadata(0x0000, 0x00C0, "read-write"),
        ["DEVADD1"] = new RegisterDatasheetMetadata(0x0000, 0x00C0, "read-write"),
        ["DEVADD2"] = new RegisterDatasheetMetadata(0x0000, 0x00C0, "read-write"),
        ["DEVADD3"] = new RegisterDatasheetMetadata(0x0000, 0x00C0, "read-write"),
        ["DEVADD4"] = new RegisterDatasheetMetadata(0x0000, 0x00C0, "read-write"),
        ["DEVADD5"] = new RegisterDatasheetMetadata(0x0000, 0x00C0, "read-write"),
        // 39.2.40 Low Power Control Register (LPCTRL)
        ["LPCTRL"] = new RegisterDatasheetMetadata(0x0000, 0x0180, "read-write"),
        // 39.2.41 Low Power Status Register (LPSTS)
        ["LPSTS"] = new RegisterDatasheetMetadata(0x0000, 0x510B, "read-write"),
        // 39.2.42 Battery Charging Control Register (BCCTRL) - 上記コメント参照(たたき台修正)
        ["BCCTRL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 39.2.43 Function L1 Control Register 1 (PL1CTRL1)
        ["PL1CTRL1"] = new RegisterDatasheetMetadata(0x0000, 0x4FFF, "read-write"),
        // 39.2.44 Function L1 Control Register 2 (PL1CTRL2)
        ["PL1CTRL2"] = new RegisterDatasheetMetadata(0x0000, 0x1F00, "read-write"),
        // 39.2.45 Host L1 Control Register 1 (HL1CTRL1)
        ["HL1CTRL1"] = new RegisterDatasheetMetadata(0x0000, 0x0007, "read-write"),
        // 39.2.46 Host L1 Control Register 2 (HL1CTRL2)
        ["HL1CTRL2"] = new RegisterDatasheetMetadata(0x0000, 0x9F0F, "read-write"),
        // 39.2.47 Deep Standby USB Transceiver Control/Pin Monitor Register (DPUSR0R) - undefined記載なし、全ビット既知
        ["DPUSR0R"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 39.2.48 Deep Standby USB Suspend/Resume Interrupt Register (DPUSR1R)
        ["DPUSR1R"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}