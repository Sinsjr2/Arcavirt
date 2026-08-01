namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 57章(12-Bit A/D Converter (S12ADC)、57.2 Register Descriptions、
/// 57.2.1〜57.2.29、p.2631-2673)から手動で書き写したレジスタごとのリセット値・
/// アクセス権限。
///
/// RX64Mの12ビットA/DコンバータはS12AD(ユニット0)とS12AD1(ユニット1)の2ユニット
/// あり、iodefine.hではst_s12ad/st_s12ad1という別々の構造体だが、データシートは
/// 両ユニットを同じ章・同じレジスタ節でまとめて説明しており(節ごとに
/// "S12AD.xxx"と"S12AD1.xxx"のアドレスが並記される)、レジスタ構成・リセット値・
/// アクセス権限は同名レジスタであれば両ユニットで共通である。そのためこのファイルは
/// レジスタ名をキーとする単一の辞書を提供し、Program.csの datasheetProviders には
/// st_s12ad/st_s12ad1どちらのパース結果に対しても同じ辞書を割り当てる設計とする。
/// ユニット間で構成が異なる箇所(例: ADDR0〜ADDR7はユニット0、ADDR8〜ADDR20は
/// ユニット1のみに存在)は、辞書には和集合の全レジスタ名(65個)を登録することで
/// 対応する(存在しないユニット側ではそもそもパース結果に現れないため問題ない)。
///
/// ADDRy(y=0〜20)・ADDBLDR・ADDBLDRA・ADDBLDRB・ADTSDR・ADOCDR・ADRDの7種は、
/// 57.2.1/57.2.2の本文冒頭に "ADDRy registers ... are 16-bit read-only registers"
/// "ADDBLDR register is a 16-bit read-only register" のように明示されている
/// read-onlyレジスタである。ビット表ではAD[x:0](変換値本体)がR、予約ビットが
/// "read as 0. The write value should be 0."という説明付きでR/Wと記載されているが、
/// これはレジスタ全体がread-writeであることを意味せず、予約ビットへの書き込みが
/// 無害(0を書けばよい)という注記に過ぎない。本文の冒頭で明示的にread-onlyと
/// 宣言されているため、ビット単位のR/W混在ルール(規約上read-writeに集約する
/// ルール)よりもこの明示を優先し、read-onlyとして扱った。"Value after reset"行は
/// いずれも全ビット0(不定"x"ではない)と明記されているため、ResetValueは
/// 0x0000固定・ResetMaskは0xFFFF(全ビット既知)とした。
///
/// 一方、A/D比較データレジスタADCMPDRy(y=0,1、57.2.26)は名前が似ているが
/// A/D変換結果を格納するレジスタではなく、比較用の基準値をソフトウェアが設定する
/// レジスタであるため、ビット表でも本体ビット(CMPD[x:0])が明示的にR/Wと
/// 記載されている。したがってADCMPDRyはread-writeのままとした
/// (read-only化の対象はADDRy系のみ)。
///
/// ADSSTRn(n=0〜7,L,T,O、57.2.14)はサンプリングステートレジスタの配列で、
/// 節見出し自体が"(n = 0 to 7, L, T, O)"となっており、Address(es)欄にADSSTR0〜
/// ADSSTR7・ADSSTRL・ADSSTRT・ADSSTROの全アドレスが列挙された上で、ビット表・
/// "Value after reset"行(0x0B)は1つだけが示されている。章内(Table 57.8含む)を
/// 検索してもL/T/O個別のリセット値の例外注記は見当たらないため、このグループの
/// 全11レジスタに同一のリセット値0x0B・ResetMask 0xFF・read-writeを適用した。
///
/// A/D比較状態レジスタ群(ADCMPSR0/ADCMPSR1/ADCMPSER、57.2.27〜57.2.29)は
/// 本文中に「1をCMPFxnビットへ書き込むことはできない」「1読み出し後に0を
/// 書き込むことでクリアされる」という説明があり、実質的にはステータス/フラグ
/// レジスタだが、ビット表自体は該当ビットをR/Wと明記しているため、規約上は
/// read-writeとして扱った(read-onlyへの読み替えは本文が明示的にread-onlyと
/// 宣言している場合のみに限定し、ここでは適用しない)。
/// </summary>
public static class Rx64mS12adDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 57.2.1 A/D Data Registers y (ADDRy) 他 - 本文冒頭でread-onlyと明示。Value after resetは全ビット0。
        ["ADDR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR2"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR3"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR4"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR5"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR6"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR7"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR8"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR9"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR10"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR11"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR12"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR13"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR14"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR15"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR16"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR17"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR18"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR19"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDR20"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDBLDR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDBLDRA"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADDBLDRB"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADTSDR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),
        ["ADOCDR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),

        // 57.2.2 A/D Self-Diagnosis Data Register (ADRD) - 本文冒頭でread-onlyと明示。
        ["ADRD"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-only"),

        // 57.2.3 A/D Control Register (ADCSR)
        ["ADCSR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.4〜57.2.7 A/D Channel Select Registers (ADANSA0/ADANSA1/ADANSB0/ADANSB1)
        ["ADANSA0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADANSA1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADANSB0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADANSB1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.8〜57.2.9 A/D-Converted Value Addition/Average Mode Select Registers (ADADS0/ADADS1)
        ["ADADS0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADADS1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.10 A/D-Converted Value Addition/Average Count Select Register (ADADC)
        ["ADADC"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 57.2.11 A/D Control Extended Register (ADCER)
        ["ADCER"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.12 A/D Start Trigger Select Register (ADSTRGR)
        ["ADSTRGR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.13 A/D Conversion Extended Input Control Register (ADEXICR) - ユニット1のみ
        ["ADEXICR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.14 A/D Sampling State Register n (ADSSTRn, n = 0 to 7, L, T, O)
        // Address(es)欄に11レジスタ全てが列挙され、Value after reset行は1つ(0x0B)を共有する。
        ["ADSSTR0"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTR1"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTR2"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTR3"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTR4"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTR5"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTR6"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTR7"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTRL"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTRT"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),
        ["ADSSTRO"] = new RegisterDatasheetMetadata(0x0B, 0xFF, "read-write"),

        // 57.2.15〜57.2.16 A/D Sample-and-Hold Circuit Registers (ADSHCR/ADSHMSR) - ユニット0のみ
        ["ADSHCR"] = new RegisterDatasheetMetadata(0x0018, 0xFFFF, "read-write"),
        ["ADSHMSR"] = new RegisterDatasheetMetadata(0x08, 0xFF, "read-write"),

        // 57.2.17 A/D Disconnection Detection Control Register (ADDISCR)
        ["ADDISCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 57.2.18 A/D Group Scan Priority Control Register (ADGSPCR)
        ["ADGSPCR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.19 A/D Compare Control Register (ADCMPCR)
        ["ADCMPCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 57.2.20〜57.2.22 A/D Compare Channel Select Registers (ADCMPANSR0/ADCMPANSR1/ADCMPANSER)
        ["ADCMPANSR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADCMPANSR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADCMPANSER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 57.2.23〜57.2.25 A/D Compare Level Registers (ADCMPLR0/ADCMPLR1/ADCMPLER)
        ["ADCMPLR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADCMPLR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADCMPLER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

        // 57.2.26 A/D Compare Data Register y (ADCMPDRy, y = 0, 1) - 比較基準値はソフト設定のためread-write。
        ["ADCMPDR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADCMPDR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

        // 57.2.27〜57.2.29 A/D Compare Status Registers (ADCMPSR0/ADCMPSR1/ADCMPSER)
        ["ADCMPSR0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADCMPSR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        ["ADCMPSER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}