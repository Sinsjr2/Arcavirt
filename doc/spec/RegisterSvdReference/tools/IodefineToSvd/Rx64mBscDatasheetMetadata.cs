namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 16章(Buses、p.482-596)から手動で書き写したレジスタごとのリセット値・アクセス権限。
/// アドレス・ビット構成は代表的なレジスタ(CS0CR/CS1CR/CSRECEN/SDRFCR/SDIR/BERSR1/
/// BUSPRI等)についてマニュアル本文と個別に突き合わせて確認済み(2026-07-29)。
/// 全58レジスタの網羅的な逐一照合は行っていない。
///
/// CSnMOD/CSnWCR1/CSnWCR2/CSnREC(n = 0 to 7)は、iodefine.h上はnの値ごとに個別の
/// structメンバーとして展開されているが、データシート上は1つのアドレス一覧・1つの
/// ビット表・1つの"Value after reset"行でn=0〜7をまとめて汎用的に記載されており、
/// 全インスタンスで同一のリセット値・アクセス権限を持つことを確認済み(CS0のみ異なる
/// 旨の記載はこれらのレジスタには無い)。
///
/// CSnCR(n = 0 to 7)は例外で、CS0CRのみ他のCS1CR〜CS7CRと異なるリセット値を持つ:
/// - CS0CR: EXENBビット(b0)がリセット後1(操作有効)。データシート本文に「リセット後、
///   エリア0のみ操作が有効(EXENB = 1)になり、他のエリアは無効になる」と明記。
///   さらにBSIZE[1:0]ビット(b5, b4)は「エリア0のバス幅はリセット後、動作モードの
///   バス幅設定に依存する」と明記されており、単一の固定値ではない
///   (SYSTEM(c0i.7)のSYSCR0等、起動時の外部ピン・動作モードに依存するケースと同じ
///   性質)。ただしEXENBや予約ビットはCS0CR固有の値として明確に固定されているため、
///   レジスタ全体をnull化するのではなく、MPU(c0i.20)のRSPAGEn/REPAGEnと同じ「値+
///   部分マスク」でBSIZE[1:0]の2ビットのみをマスク対象外とした
///   (ResetValue=0x0001(EXENBのみ)、ResetMask=0xFFCF)。
/// - CS1CR〜CS7CR: 全ビットがリセット後0に固定(EXENB=0で他エリアは無効)。
///
/// BERSR1・BERSR2は、ステータス・予約ビットを含む全ビットがR(書込み不可、予約ビットも
/// 「書込みは無効」と明記)のため、例外的にread-onlyとした。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、以下はビットごとにW/R/R(/W)混在だが
/// read-writeとして扱った:
/// - BERCLR: STSCLR(b0)は「1書込みのみ有効、0書込みは無効」の(W)、予約ビットはR/W
/// - SDSR: MRSST/INIST/SRFSTはR(ステータス読み出し専用)、予約ビットはR/W
/// - BUSPRI: BPRA〜BPEBは「DTC/DMAC/EXDMAC/EDMAC停止中に1回のみ書込み可」のR(/W)、
///   予約ビットはR/W
/// </summary>
public static class Rx64mBscDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 16.3.19 Bus Error Status Clear Register (BERCLR)
        ["BERCLR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 16.3.20 Bus Error Monitoring Enable Register (BEREN)
        ["BEREN"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 16.3.21 Bus Error Status Register 1 (BERSR1) - 全ビットRのみ
        ["BERSR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),

        // 16.3.22 Bus Error Status Register 2 (BERSR2) - 全ビットRのみ
        ["BERSR2"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),

        // 16.3.23 Bus Priority Control Register (BUSPRI)
        ["BUSPRI"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 16.3.4 CSn Mode Register (CSnMOD) (n = 0 to 7) - CS0を含め全インスタンス同一
        ["CS0MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 16.3.5 CSn Wait Control Register 1 (CSnWCR1) (n = 0 to 7)
        // CSPWWAIT/CSPRWAIT/CSWWAIT/CSRWAIT全て7(0b111)がリセット値
        ["CS0WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        // 16.3.6 CSn Wait Control Register 2 (CSnWCR2) (n = 0 to 7)
        // CSROFF[2:0]=7(0b111)、他は0がリセット値
        ["CS0WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        ["CS1MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS1WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        ["CS1WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        ["CS2MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS2WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        ["CS2WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        ["CS3MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS3WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        ["CS3WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        ["CS4MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS4WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        ["CS4WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        ["CS5MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS5WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        ["CS5WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        ["CS6MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS6WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        ["CS6WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        ["CS7MOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS7WCR1"] = new RegisterDatasheetMetadata(0x07070707, 0xFFFFFFFF, "read-write"),
        ["CS7WCR2"] = new RegisterDatasheetMetadata(0x00000007, 0xFFFFFFFF, "read-write"),

        // 16.3.1 CSn Control Register (CSnCR) (n = 0 to 7)
        // CS0CRのみ例外: EXENB(b0)=1固定、BSIZE[1:0](b5,b4)は動作モードのバス幅設定に
        // 依存し単一の固定値が無いため部分マスクで除外(値+部分マスク)。
        ["CS0CR"] = new RegisterDatasheetMetadata(0x0001, 0xFFCF, "read-write"),
        // 16.3.2 CSn Recovery Cycle Register (CSnREC) (n = 0 to 7) - 全インスタンス同一
        ["CS0REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // CS1CR〜CS7CRは全ビット0固定(area0以外はEXENB=0でリセット後操作無効)
        ["CS1CR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS1REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        ["CS2CR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS2REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        ["CS3CR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS3REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        ["CS4CR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS4REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        ["CS5CR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS5REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        ["CS6CR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS6REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        ["CS7CR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["CS7REC"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 16.3.3 CS Recovery Cycle Insertion Enable Register (CSRECEN)
        // RCVENM[7:5]/RCVENM1=1, RCVEN[7:5]/RCVEN1=1がリセット値(0x3E3E)
        ["CSRECEN"] = new RegisterDatasheetMetadata(0x3E3E, 0xFFFF, "read-write"),

        // 16.3.7 SDC Control Register (SDCCR)
        ["SDCCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 16.3.8 SDC Mode Register (SDCMOD)
        ["SDCMOD"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 16.3.9 SDRAM Access Mode Register (SDAMOD)
        ["SDAMOD"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 16.3.10 SDRAM Self-Refresh Control Register (SDSELF)
        ["SDSELF"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 16.3.11 SDRAM Refresh Control Register (SDRFCR) - RFC[11:0]=1がリセット値
        ["SDRFCR"] = new RegisterDatasheetMetadata(0x0001, 0xFFFF, "read-write"),
        // 16.3.12 SDRAM Auto-Refresh Control Register (SDRFEN)
        ["SDRFEN"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 16.3.13 SDRAM Initialization Sequence Control Register (SDICR)
        ["SDICR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 16.3.14 SDRAM Initialization Register (SDIR) - ARFC[3:0]=1がリセット値(0x0010)
        ["SDIR"] = new RegisterDatasheetMetadata(0x0010, 0xFFFF, "read-write"),
        // 16.3.15 SDRAM Address Register (SDADR)
        ["SDADR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 16.3.16 SDRAM Timing Register (SDTR) - CL[2:0]=2がリセット値(0x00000002)
        ["SDTR"] = new RegisterDatasheetMetadata(0x00000002, 0xFFFFFFFF, "read-write"),
        // 16.3.17 SDRAM Mode Register (SDMOD)
        ["SDMOD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 16.3.18 SDRAM Status Register (SDSR) - ステータスビットはR、予約ビットはR/W
        ["SDSR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}