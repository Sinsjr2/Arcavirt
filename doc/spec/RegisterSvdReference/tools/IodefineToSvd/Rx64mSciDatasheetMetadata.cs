namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 40章(Serial Communications Interface (SCIg, SCIh), p.1966-2040)から
/// 手動で書き写したレジスタごとのリセット値・アクセス権限。
/// 各レジスタのビットフィールド名の集合は、iodefine.hから自動抽出した結果と
/// 完全一致することを確認済み(2026-07-28)。
///
/// SMR〜MDDRの17レジスタはst_sci0/st_sci12/st_smci0で共通(同一アドレス・
/// 同一リセット値)。ESMER以降の20レジスタはst_sci12のみに存在する
/// (Extended Serial Mode: 調歩同期拡張モード用の追加レジスタ)。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataは
/// レジスタ全体で1つのAccess文字列しか表現できない)、ビットごとにR/RW/W(1クリア)
/// が混在するレジスタ(SSR/SISR/CR0)は "read-write" として扱う。個別ビットの
/// 正確な挙動(例: TENDはR専用、MPBTのみR/W)はデータシート本文を参照のこと。
/// </summary>
public static class Rx64mSciDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // st_sci0 / st_sci12 / st_smci0 共通(同一物理レジスタ)
        ["SMR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["BRR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["SCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TDR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["SSR"] = new RegisterDatasheetMetadata(0x84, 0xFF, "read-write"),
        ["RDR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        ["SCMR"] = new RegisterDatasheetMetadata(0xF2, 0xFF, "read-write"),
        ["SEMR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SNFR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SIMR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SIMR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SIMR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // b2/b4/b5はリセット値未定義(データシートに"Undefined"と明記)
        ["SISR"] = new RegisterDatasheetMetadata(null, 0xCB, "read-write"),
        ["SPMR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TDRHL"] = new RegisterDatasheetMetadata(0xFFFF, 0xFFFF, "read-write"),
        ["RDRHL"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["MDDR"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),

        // st_sci12専用(Extended Serial Mode)
        ["ESMER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["PCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["ICR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["STR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        ["STCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CF0DR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CF0CR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CF0RR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        ["PCF1DR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["SCF1DR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CF1CR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["CF1RR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
        ["TCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TMR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        ["TPRE"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
        ["TCNT"] = new RegisterDatasheetMetadata(0xFF, 0xFF, "read-write"),
    };
}