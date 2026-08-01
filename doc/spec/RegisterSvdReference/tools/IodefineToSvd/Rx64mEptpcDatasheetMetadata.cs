namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 36章(PTP Module for the Ethernet Controller (EPTPC)、36.2 Register Descriptions、
/// 36.2.1〜36.2.84、p.1560-1649)から手動で書き写したレジスタごとのリセット値・
/// アクセス権限。全レジスタについてマニュアル本文の個別レジスタ節と突き合わせて
/// 確認済み。
///
/// iodefine.hにはst_eptpc型・st_eptpc0型の2つの構造体がある。st_eptpc型は
/// インスタンスがEPTPC(0xC0500)の1つのみで、STCA(Synchronized Time Clock and
/// Address)共通制御・パルス出力タイマ・PRC-TC(中継)関連のレジスタ(PTRSTR、
/// STCSELR、MIESR等)を含む。st_eptpc0型はEPTPC0(0xC4800)・EPTPC1(0xC4C00)の
/// 2インスタンスがあり、SYNFP(ポートごとのPTPメッセージ処理)関連のレジスタ
/// (SYSR、SYIPR等)を含む。データシートの各レジスタ節も、st_eptpc0型のレジスタは
/// "Address(es): EPTPC0.xxx ..., EPTPC1.xxx ..."という形で2アドレスを併記した上で
/// ビット表・Value after reset欄を1つだけ示しており、EPTPC0/EPTPC1間で構成・
/// リセット値・アクセス権限に差異がないことを裏付けている。
///
/// st_eptpc型とst_eptpc0型のレジスタ名は全75個(st_eptpc)・全57個(st_eptpc0)を
/// 突き合わせた結果、1つも重複がない(接頭辞がST/EL/LC/PL/ML/TM/PR/TR系と
/// SY/AN/DY/MT/OF/MP/GM/CU/SR/PP/PD/PE/PG/FF/FMAC/DASYM/TS系で完全に分かれている)。
/// そのためMTU/MTU8のような値の食い違いに対応する専用の上書きファイル
/// (Rx64mMtu8DatasheetMetadataのようなもの)は不要で、本ファイル1つの辞書を
/// Program.csでst_eptpc・st_eptpc0の両方に共通の"rx64m-eptpc"プロバイダとして
/// 割り当てる設計とする。
///
/// アクセス権限の判定にあたっては、抽出スクリプト(extract_registers.py)の
/// access_candidateが以下の11レジスタで実際の本文と食い違っていたため、本文の
/// ビット単位R/W表に基づき訂正した(いずれもステータス/ディレクティブ系レジスタで、
/// [[project_rx_svd_datasheet_extraction_quirks]]と同種の誤検出)。
///   - MIESR: 候補read-only→実際はCYC0〜5ビットがR/W*1(1書き込みでクリア)のため
///     read-write。
///   - STSR: 候補read-only→SYNC/SYNCOUT/SYNTOUT/W10DビットがR/W*1のためread-write。
///   - STCFR: 候補read-only→STCF[1:0]ビットがR/Wのためread-write。
///   - STCHSELR: 候補read-only→SYSELビットがR/W*1のためread-write。
///   - LCIVLDR: 候補read-only→LOADビットがW*1(書き込み専用の1ビットディレクティブ、
///     予約ビットはR)のためread-write(下記「W*1ディレクティブビット」の判断参照)。
///   - GETW10R: 候補read-only→GW10ビットがR/W*1のためread-write。
///   - TMCYCR0〜5: 候補null(表のレイアウト崩れでR/W列が読み取れなかった)→本文で
///     CYC[29:0]ビットがR/W、予約ビットがRと確認できたためread-write。
///   - TRNDISR: 候補read-only→TDIS[1:0]ビットがR/Wのためread-write。
///   - PPIPR: 候補null→本文でビット全体がR/Wと明記されているためread-write。
///   - PDIPR: 候補null→本文でビット全体がR/Wと明記されているためread-write。
///   - PGUDPR: 候補read-only→GEUPT[15:0]ビットがR/Wのためread-write。
///
/// 「W*1ディレクティブビット」の扱いについて: LCIVLDR.LOAD、SYRVLDR.BMUP/STUP/ANUPは
/// ビット単位R/W表で「R/W」ではなく「W」(書き込み専用)と明記されている。この場合
/// でも予約ビットはRとして読めるため、レジスタ全体としては読み出しも書き込みも可能で
/// あり、規約上の「ビット単位でR/(W)・R・R/Wが混在する場合はレジスタ全体をread-write
/// として扱う」というルールに準じ、W(書き込み専用)とRの混在も同様にread-writeとして
/// 扱った(レジスタ全体がwrite-onlyと明記されているFTDR等とは異なり、予約ビットは
/// 読み出し可能であるため)。
///
/// PRSR(36.2.30)とSYSR(36.2.37)は、一部の予約ビット(PRSR: b16-b27、SYSR: b18-b23)
/// についてビット説明表に "These bits are read as undefined." と明記されている。
/// これらのビットは値が不定であるためResetMaskから除外し、PRSRはResetMask=
/// 0xF000FFFF、SYSRはResetMask=0xFF03FFFFとした(抽出スクリプトのundefined_bits
/// 検出は本文の記載と一致しており、この2レジスタについては候補をそのまま採用した)。
///
/// 上記以外の全レジスタは"Value after reset"欄にxビット(不定)が1つも存在せず、
/// 全ビットがリセットで既知の値(大半は0、一部PPMACRU/PDMACRU/PDMACRL/PETYPER/
/// PPIPR/PDIPR/PPTTLR/PDTTLR/PEUDPR/PGUDPR/FFLTR/ELIPPR/TRNCTTDR/SYSPVRR/SYTLIR/
/// STCSELRのようにIEEE1588既定値等の非ゼロ値)であるため、ResetMask=0xFFFFFFFF
/// として扱う。
/// </summary>
public static class Rx64mEptpcDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // ---- st_eptpc (EPTPCインスタンスのみ、0xC0500台) ----
        // 36.2.83 PTP Reset Register (PTRSTR)
        ["PTRSTR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.84 STCA Clock Select Register (STCSELR)
        ["STCSELR"] = new RegisterDatasheetMetadata(0x00000006, 0xFFFFFFFF, "read-write"),
        // 36.2.1 MINT Interrupt Source Status Register (MIESR) - CYC0〜5がR/W*1のためread-write
        ["MIESR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.2 MINT Interrupt Request Enable Register (MIEIPR)
        ["MIEIPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.3 ELC Output/IPLS Interrupt Request Enable Register (ELIPPR)
        ["ELIPPR"] = new RegisterDatasheetMetadata(0x00003F3F, 0xFFFFFFFF, "read-write"),
        // 36.2.4 ELC Output/IPLS Interrupt Enable Automatic Clearing Register (ELIPACR)
        ["ELIPACR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.5 STCA Status Register (STSR) - SYNC/SYNCOUT/SYNTOUT/W10DがR/W*1のためread-write
        ["STSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.6 STCA Status Notification Enable Register (STIPR)
        ["STIPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.7 STCA Clock Frequency Setting Register (STCFR) - STCF[1:0]がR/Wのためread-write
        ["STCFR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.8 STCA Operating Mode Register (STMR)
        ["STMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.9 Sync Message Reception Timeout Register (SYNTOR)
        ["SYNTOR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.10 IPLS Interrupt Request Timer Select Register (IPTSELR)
        ["IPTSELR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.11 MINT Interrupt Request Timer Select Register (MITSELR)
        ["MITSELR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.12 ELC Output Timer Select Register (ELTSELR)
        ["ELTSELR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.13 Time Synchronization Channel Select Register (STCHSELR) - SYSELがR/W*1のためread-write
        ["STCHSELR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.14 Slave Time Synchronization Start Register (SYNSTARTR)
        ["SYNSTARTR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.15 Local Clock Counter Initial Value Load Directive Register (LCIVLDR) - LOADはW*1(予約ビットR)のためread-write
        ["LCIVLDR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.16 Synchronization Loss Detection Threshold Registers (SYNTDARU, SYNTDARL)
        ["SYNTDARU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["SYNTDARL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.17 Synchronization Detection Threshold Registers (SYNTDBRU, SYNTDBRL)
        ["SYNTDBRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["SYNTDBRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.18 Local Clock Counter Initial Value Registers (LCIVRU, LCIVRM, LCIVRL)
        ["LCIVRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["LCIVRM"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["LCIVRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.19 Worst 10 Acquisition Directive Register (GETW10R) - GW10がR/W*1のためread-write
        ["GETW10R"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.20 Positive Gradient Limit Registers (PLIMITRU, PLIMITRM, PLIMITRL)
        ["PLIMITRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["PLIMITRM"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["PLIMITRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.21 Negative Gradient Limit Registers (MLIMITRU, MLIMITRM, MLIMITRL)
        ["MLIMITRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["MLIMITRM"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["MLIMITRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.22 Statistical Information Retention Control Register (GETINFOR)
        ["GETINFOR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.23 Local Clock Counters (LCCVRU, LCCVRM, LCCVRL) - 全ビットRのためread-only
        ["LCCVRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["LCCVRM"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["LCCVRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 36.2.24 Positive Gradient Worst 10 Value Registers (PW10VRU, PW10VRM, PW10VRL) - 全ビットR
        ["PW10VRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["PW10VRM"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["PW10VRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 36.2.25 Negative Gradient Worst 10 Value Registers (MW10RU, MW10RM, MW10RL) - 全ビットR
        ["MW10RU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["MW10RM"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["MW10RL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 36.2.26 Timer Start Time Setting Registers m (TMSTTRUm, TMSTTRLm) (m = 0 to 5)
        ["TMSTTRU0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRL0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRU1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRL1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRU2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRL2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRU3"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRL3"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRU4"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRL4"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRU5"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMSTTRL5"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.27 Timer Cycle Setting Registers m (TMCYCRm) (m = 0 to 5) - CYC[29:0]がR/Wのためread-write
        ["TMCYCR0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMCYCR1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMCYCR2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMCYCR3"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMCYCR4"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMCYCR5"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.28 Timer Pulse Width Setting Register m (TMPLSRm) (m = 0 to 5)
        ["TMPLSR0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMPLSR1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMPLSR2"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMPLSR3"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMPLSR4"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["TMPLSR5"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.29 Timer Start Register (TMSTARTR)
        ["TMSTARTR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.30 PRC-TC Status Register (PRSR) - b16-b27は本文で"read as undefined"と明記のためResetMaskから除外
        ["PRSR"] = new RegisterDatasheetMetadata(0x00000000, 0xF000FFFF, "read-write"),
        // 36.2.31 PRC-TC Status Notification Enable Register (PRIPR)
        ["PRIPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.32 Channel 0 Local MAC Address Registers (PRMACRU0, PRMACRL0)
        ["PRMACRU0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["PRMACRL0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.33 Channel 1 Local MAC Address Registers (PRMACRU1, PRMACRL1)
        ["PRMACRU1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["PRMACRL1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.34 Packet Transmission Control Register (TRNDISR) - TDIS[1:0]がR/Wのためread-write
        ["TRNDISR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.35 Relay Mode Register (TRNMR)
        ["TRNMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.36 Cut-Through Transfer Start Threshold Register (TRNCTTDR)
        ["TRNCTTDR"] = new RegisterDatasheetMetadata(0x00000060, 0xFFFFFFFF, "read-write"),

        // ---- st_eptpc0 (EPTPC0/EPTPC1の2インスタンス、0xC4800/0xC4C00台) ----
        // 36.2.37 SYNFP Status Register (SYSR) - b18-b23は本文で"read as undefined"と明記のためResetMaskから除外
        ["SYSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFF03FFFF, "read-write"),
        // 36.2.38 SYNFP Status Notification Enable Register (SYIPR)
        ["SYIPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.39 SYNFP MAC Address Registers (SYMACRU, SYMACRL)
        ["SYMACRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["SYMACRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.40 SYNFP Local IP Address Register (SYIPADDRR)
        ["SYIPADDRR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.41 SYNFP Specification Version Setting Register (SYSPVRR)
        ["SYSPVRR"] = new RegisterDatasheetMetadata(0x00000002, 0xFFFFFFFF, "read-write"),
        // 36.2.42 SYNFP Domain Number Setting Register (SYDOMR)
        ["SYDOMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.43 Announce Message Flag Field Setting Register (ANFR)
        ["ANFR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.44 Sync Message Flag Field Setting Register (SYNFR)
        ["SYNFR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.45 Delay_Req Message Flag Field Setting Register (DYRQFR)
        ["DYRQFR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.46 Delay_Resp Message Flag Field Setting Register (DYRPFR)
        ["DYRPFR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.47 SYNFP Local Clock ID Registers (SYCIDRU, SYCIDRL)
        ["SYCIDRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["SYCIDRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.48 SYNFP Local Port Number Register (SYPNUMR)
        ["SYPNUMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.49 SYNFP Register Value Load Directive Register (SYRVLDR) - BMUP/STUP/ANUPはW(予約ビットR)のためread-write
        ["SYRVLDR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.50 SYNFP Reception Filter Register 1 (SYRFL1R)
        ["SYRFL1R"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.51 SYNFP Reception Filter Register 2 (SYRFL2R)
        ["SYRFL2R"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.52 SYNFP Transmission Enable Register (SYTRENR)
        ["SYTRENR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.53 Master Clock ID Registers (MTCIDU, MTCIDL)
        ["MTCIDU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["MTCIDL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.54 Master Clock Port Number Register (MTPID)
        ["MTPID"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.55 SYNFP Transmission Interval Setting Register (SYTLIR)
        ["SYTLIR"] = new RegisterDatasheetMetadata(0x00000001, 0xFFFFFFFF, "read-write"),
        // 36.2.56 SYNFP Received logMessageInterval Value Indication Register (SYRLIR) - 全ビットRのためread-only
        ["SYRLIR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 36.2.57 offsetFromMaster Value Registers (OFMRU, OFMRL) - 全ビットR
        ["OFMRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["OFMRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 36.2.58 meanPathDelay Value Registers (MPDRU, MPDRL) - 全ビットR
        ["MPDRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        ["MPDRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
        // 36.2.59 grandmasterPriority Field Setting Register (GMPR)
        ["GMPR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.60 grandmasterClockQuality Field Setting Register (GMCQR)
        ["GMCQR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.61 grandmasterIdentity Field Setting Registers (GMIDRU, GMIDRL)
        ["GMIDRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["GMIDRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.62 currentUtcOffset/timeSource Field Setting Register (CUOTSR)
        ["CUOTSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.63 stepsRemoved Field Setting Register (SRR)
        ["SRR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.64 PTP-primary Message Destination MAC Address Setting Registers (PPMACRU, PPMACRL) - 既定値01:1B:19:00:00:00
        ["PPMACRU"] = new RegisterDatasheetMetadata(0x00011B19, 0xFFFFFFFF, "read-write"),
        ["PPMACRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.65 PTP-pdelay Message MAC Address Setting Registers (PDMACRU, PDMACRL) - 既定値01:80:C2:00:00:0E
        ["PDMACRU"] = new RegisterDatasheetMetadata(0x000180C2, 0xFFFFFFFF, "read-write"),
        ["PDMACRL"] = new RegisterDatasheetMetadata(0x0000000E, 0xFFFFFFFF, "read-write"),
        // 36.2.66 PTP Message Ethertype Setting Register (PETYPER) - 既定値0000 88F7h
        ["PETYPER"] = new RegisterDatasheetMetadata(0x000088F7, 0xFFFFFFFF, "read-write"),
        // 36.2.67 PTP-primary Message Destination IP Address Setting Register (PPIPR) - 全ビットR/W、既定値224.0.1.129
        ["PPIPR"] = new RegisterDatasheetMetadata(0xE0000181, 0xFFFFFFFF, "read-write"),
        // 36.2.68 PTP-pdelay Message Destination IP Address Setting Register (PDIPR) - 全ビットR/W、既定値224.0.0.107
        ["PDIPR"] = new RegisterDatasheetMetadata(0xE000006B, 0xFFFFFFFF, "read-write"),
        // 36.2.69 PTP Event Message TOS Setting Register (PETOSR)
        ["PETOSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.70 PTP general Message TOS Setting Register (PGTOSR)
        ["PGTOSR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.71 PTP-primary Message TTL Setting Register (PPTTLR) - 既定値80h
        ["PPTTLR"] = new RegisterDatasheetMetadata(0x00000080, 0xFFFFFFFF, "read-write"),
        // 36.2.72 PTP-pdelay Message TTL Setting Register (PDTTLR) - 既定値01h
        ["PDTTLR"] = new RegisterDatasheetMetadata(0x00000001, 0xFFFFFFFF, "read-write"),
        // 36.2.73 PTP Event Message UDP Destination Port Number Setting Register (PEUDPR) - 既定値319(013Fh)
        ["PEUDPR"] = new RegisterDatasheetMetadata(0x0000013F, 0xFFFFFFFF, "read-write"),
        // 36.2.74 PTP general Message UDP Destination Port Number Setting Register (PGUDPR) - GEUPT[15:0]がR/Wのためread-write、既定値320(0140h)
        ["PGUDPR"] = new RegisterDatasheetMetadata(0x00000140, 0xFFFFFFFF, "read-write"),
        // 36.2.75 Frame Reception Filter Setting Register (FFLTR) - 既定値EXTPRM=1
        ["FFLTR"] = new RegisterDatasheetMetadata(0x00010000, 0xFFFFFFFF, "read-write"),
        // 36.2.76 Frame Reception Filter MAC Address 0 Setting Registers (FMAC0RU, FMAC0RL)
        ["FMAC0RU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["FMAC0RL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.77 Frame Reception Filter MAC Address 1 Setting Registers (FMAC1RU, FMAC1RL)
        ["FMAC1RU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["FMAC1RL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.78 Asymmetric Delay Setting Registers (DASYMRU, DASYMRL)
        ["DASYMRU"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        ["DASYMRL"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.79 Timestamp Latency Setting Register (TSLATR)
        ["TSLATR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.80 SYNFP Operation Setting Register (SYCONFR)
        ["SYCONFR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.81 SYNFP Frame Format Setting Register (SYFORMR)
        ["SYFORMR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
        // 36.2.82 Response Message Reception Timeout Register (RSTOUTR)
        ["RSTOUTR"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}