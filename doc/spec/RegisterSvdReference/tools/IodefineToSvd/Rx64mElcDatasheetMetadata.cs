namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 21章(Event Link Controller (ELC), p.757-776)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。アドレス・ビット構成は代表的なレジスタ
/// (ELCR/ELSRn/ELOPA/PGC1/PEL0/ELSEGR等)についてマニュアル本文と個別に突き合わせて
/// 確認済み(2026-07-29)。全51レジスタの網羅的な逐一照合は行っていない。
///
/// ELSRn(Event Link Setting Register n)は、iodefine.h上はnの値ごとに個別のstruct
/// メンバーとして展開されているが、データシート上は「ELSRn」(n = 0, 3, 4, 7, 10 to 13,
/// 15, 16, 18 to 28, 33, 35 to 38, 41 to 45)のようにまとめて1回だけ汎用的に記載されて
/// おり、全インスタンスで同一のリセット値(0x00)・アクセス権限(read-write)を持つことを
/// 確認済み。データシートに記載されたnの範囲はiodefine.h上に実在する番号と一致して
/// おり(欠番はiodefine.h側にも存在しない)、欠番にエントリは作成していない。
///
/// このモジュールのレジスタは全てリセット値が単一の固定値として明記されており、
/// PIDR(c0i.8)やMPDEA/MPSA(c0i.20)のようなnull表現・部分マスク表現を要する
/// レジスタは無かった。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataはレジスタ
/// 全体で1つのAccess文字列しか表現できない)、ELSEGR(WI/SEGはWのみ、WEおよび予約
/// ビットはR/W)のようにビットごとにW/RWが混在するレジスタも"read-write"として扱う。
/// </summary>
public static class Rx64mElcDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 21.2.1 Event Link Control Register (ELCR)
        ["ELCR"] = new RegisterDatasheetMetadata(0x7F, 0xFF, "read-write"),

        // 21.2.2 Event Link Setting Register n (ELSRn)
        // (n = 0, 3, 4, 7, 10 to 13, 15, 16, 18 to 28, 33, 35 to 38, 41 to 45)
        ["ELSR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR4"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR7"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR10"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR11"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR12"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR13"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR15"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR16"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR18"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR19"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR20"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR21"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR22"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR23"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR24"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR25"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR26"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR27"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR28"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR33"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR35"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR36"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR37"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR38"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR41"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR42"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR43"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR44"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ELSR45"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 21.2.3-21.2.6, 21.2.7-21.2.10 Event Link Option Setting Register A/B/C/D/F/H/I/J
        // (ELOPA/ELOPB/ELOPC/ELOPD/ELOPF/ELOPH/ELOPI/ELOPJ)
        // 各MDビットは「00: 開始/01: 再開始/10: 各種操作/11: イベント出力無効」を
        // 意味し、予約ビットを含め未使用時は全ビット1(イベント出力無効相当)がリセット値。
        ["ELOPA"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["ELOPB"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["ELOPC"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["ELOPD"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["ELOPF"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["ELOPH"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["ELOPI"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["ELOPJ"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),

        // 21.2.11 Port Group Setting Register n (PGRn) (n = 1, 2)
        ["PGR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["PGR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 21.2.12 Port Group Control Register n (PGCn) (n = 1, 2)
        // b7=1, b6-4(PGCO)=0, b3=1, b2(PGCOVE)=0, b1-0(PGCI)=0 -> 0x88
        ["PGC1"] = new RegisterDatasheetMetadata(0x88, 0xFF, "read-write"),
        ["PGC2"] = new RegisterDatasheetMetadata(0x88, 0xFF, "read-write"),

        // 21.2.13 Port Buffer Register n (PDBFn) (n = 1, 2)
        ["PDBF1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["PDBF2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 21.2.14 Event Link Port Setting Register m (PELm) (m = 0 to 3)
        // b7(予約)=1, 他は0 -> 0x80
        ["PEL0"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
        ["PEL1"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
        ["PEL2"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
        ["PEL3"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),

        // 21.2.15 Event Link Software Event Generation Register (ELSEGR)
        // b7(WI)=1, b6(WE)=0, b5-1(予約)=1, b0(SEG)=0 -> 0xBE
        // WI/SEGはWのみ、WEおよび予約ビットはR/Wのため、レジスタ単位ではread-writeとした。
        ["ELSEGR"] = new RegisterDatasheetMetadata(0xBE, 0xFF, "read-write"),
    };
}