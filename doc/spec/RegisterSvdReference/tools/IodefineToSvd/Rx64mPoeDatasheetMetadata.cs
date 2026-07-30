namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 25章(Port Output Enable 3 (POE3a)、p.1092-1121の25.2 Register Descriptions)
/// から手動で書き写したレジスタごとのリセット値・アクセス権限。POE3は1インスタンス
/// 構成であり、iodefine.h上もst_poeの1インスタンス(POE3)のみが定義されている。
///
/// レジスタ単位でAccessを1つに集約する都合上、以下はビットごとにR/W・R/(W)・
/// R/W*1(リセット後1回のみ変更可)混在だが、いずれも書き込み可能なビットを含むため
/// read-writeとして扱った:
/// - ICSR1-6: 割り込み許可ビット(R/W)とR/(W)のフラグビット(1を読み出した後の
///   0書き込みでのみクリア可能)が混在。
/// - OCSR1/OCSR2: 同様にR/(W)フラグビット(OSF1/OSF2)を含む。
///
/// POECR4/POECR5は、抽出スクリプトがビット位置と値の対応表(bits辞書)を
/// 取得できておらず(原因未特定。ビット名が2行に折り返される箇所があり、
/// 列位置の対応付けに失敗した可能性がある)、「たたき台」ではreset_value_candidate=0x00・
/// reset_mask_candidate=0x00となっていたため、本文中の"Value after reset"行を
/// 直接読み、ビット位置ごとの値からリセット値を再計算した(POECR4はb10・b1が
/// 予約済み1固定ビットのためリセット値0x0402、POECR5はb3が予約済み1固定ビットの
/// ためリセット値0x0008)。ビット説明表の"This bit is read as 1"という記載とも
/// 整合していることを確認済み。
/// </summary>
public static class Rx64mPoeDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 25.2.1 Input Level Control/Status Register 1 (ICSR1)
        ["ICSR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.7 Output Level Control/Status Register 1 (OCSR1)
        ["OCSR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.2 Input Level Control/Status Register 2 (ICSR2)
        ["ICSR2"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.8 Output Level Control/Status Register 2 (OCSR2)
        ["OCSR2"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.3 Input Level Control/Status Register 3 (ICSR3)
        ["ICSR3"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.10 Software Port Output Enable Register (SPOER)
        ["SPOER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.11 Port Output Enable Control Register 1 (POECR1)
        ["POECR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.12 Port Output Enable Control Register 2 (POECR2)
        ["POECR2"] = new RegisterDatasheetMetadata(0x0707, 0xFFFF, "read-write"),
        // 25.2.13 Port Output Enable Control Register 3 (POECR3)
        ["POECR3"] = new RegisterDatasheetMetadata(0x0303, 0xFFFF, "read-write"),
        // 25.2.14 Port Output Enable Control Register 4 (POECR4) - 上記コメント参照
        ["POECR4"] = new RegisterDatasheetMetadata(0x0402, 0xFFFF, "read-write"),
        // 25.2.15 Port Output Enable Control Register 5 (POECR5) - 上記コメント参照
        ["POECR5"] = new RegisterDatasheetMetadata(0x0008, 0xFFFF, "read-write"),
        // 25.2.16 Port Output Enable Control Register 6 (POECR6)
        ["POECR6"] = new RegisterDatasheetMetadata(0x2010, 0xFFFF, "read-write"),
        // 25.2.4 Input Level Control/Status Register 4 (ICSR4)
        ["ICSR4"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.5 Input Level Control/Status Register 5 (ICSR5)
        ["ICSR5"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.9 Active Level Setting Register 1 (ALR1)
        ["ALR1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.6 Input Level Control/Status Register 6 (ICSR6)
        ["ICSR6"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
        // 25.2.17 GPT0 Pin Select Register (G0SELR)
        ["G0SELR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.18 GPT1 Pin Select Register (G1SELR)
        ["G1SELR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.19 GPT2 Pin Select Register (G2SELR)
        ["G2SELR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.20 GPT3 Pin Select Register (G3SELR)
        ["G3SELR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.21 MTU0 Pin Select Register 1 (M0SELR1)
        ["M0SELR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.22 MTU0 Pin Select Register 2 (M0SELR2)
        ["M0SELR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        // 25.2.23 MTU3 Pin Select Register (M3SELR)
        ["M3SELR"] = new RegisterDatasheetMetadata(0x55, 0xFF, "read-write"),
        // 25.2.24 MTU4 Pin Select Register 1 (M4SELR1)
        ["M4SELR1"] = new RegisterDatasheetMetadata(0x55, 0xFF, "read-write"),
        // 25.2.25 MTU4 Pin Select Register 2 (M4SELR2)
        ["M4SELR2"] = new RegisterDatasheetMetadata(0x55, 0xFF, "read-write"),
        // 25.2.26 MTU/GPT Pin Function Select Register (MGSELR)
        ["MGSELR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
    };
}