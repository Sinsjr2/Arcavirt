namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 23章(Multi-Function Pin Controller (MPC), 23.2 Register Descriptions, p.798-849)
/// から手動で書き写したレジスタごとのリセット値・アクセス権限。
///
/// PWPR(23.2.1)は本文の"Value after reset"行(b7 B0WI=1, b6 to b0=0)から
/// 0x80(B0WI固定で1、他ビットは0)であることを直接確認済み。全ビットR/Wのため
/// access="read-write"。
///
/// P0nPFS〜PJnPFS(23.2.2〜23.2.19、n = 0 to 7の組み合わせで119レジスタ)は、
/// いずれもPSEL[5:0]・ISEL・ASEL等のビット構成の違いはあるものの、"Value after
/// reset"は全ビット0・全ビットR/Wで統一されている。代表として23.2.2(P0nPFS)・
/// 23.2.9(P7nPFS、PSEL[5:0]のみでb7/b6が予約"read as 0"であることを確認)・
/// 23.2.11(P9nPFS)・23.2.18(PGnPFS)の4節を本文と個別に突き合わせて確認済み。
/// 残りのグループは抽出スクリプトの出力(全て0x00, 0xFF, read-write)をそのまま
/// 採用しており、119レジスタ全ての逐一照合は行っていない。
///
/// PFCSE/PFCSS0/PFAOE0/PFAOE1/PFBCR0/PFBCR1/PFENET(23.2.20, 23.2.21,
/// 23.2.23〜23.2.27)は本文のビット表・"Value after reset"行がいずれも
/// 0x00・全ビットR/Wであることを確認済み。
/// PFCSS1(23.2.22)は抽出スクリプトがCSnS[1:0]の2ビット幅シンボルの並びを
/// フィールド表として認識できず`access_candidate: null`になっていたが、
/// 本文のビット表(CS4S[1:0]〜CS7S[1:0]、全ビットR/W)とValue after reset行
/// (全ビット0)を直接確認し、他の個別制御レジスタと同じ0x00, 0xFF,
/// read-writeを採用した。
/// </summary>
public static class Rx64mMpcDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => BuildRegisters();

    static Dictionary<string, RegisterDatasheetMetadata> BuildRegisters() {
        var registers = new Dictionary<string, RegisterDatasheetMetadata> {
            // 個別制御レジスタ(23.2.1, 23.2.20〜23.2.27)
            ["PWPR"] = new RegisterDatasheetMetadata(0x80, 0xFF, "read-write"),
            ["PFCSE"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["PFCSS0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["PFCSS1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["PFAOE0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["PFAOE1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["PFBCR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["PFBCR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["PFENET"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        };

        // P0nPFS〜PJnPFS(23.2.2〜23.2.19): iodefine.h上の実際のレジスタ名
        // (ポートグループごとに存在するピンのみ)。119レジスタ全て同一定義
        // (0x00, 0xFF, read-write)。
        string[] pxxPfsNames = [
            "P00PFS", "P01PFS", "P02PFS", "P03PFS", "P05PFS", "P07PFS",
            "P10PFS", "P11PFS", "P12PFS", "P13PFS", "P14PFS", "P15PFS", "P16PFS", "P17PFS",
            "P20PFS", "P21PFS", "P22PFS", "P23PFS", "P24PFS", "P25PFS", "P26PFS", "P27PFS",
            "P30PFS", "P31PFS", "P32PFS", "P33PFS", "P34PFS",
            "P40PFS", "P41PFS", "P42PFS", "P43PFS", "P44PFS", "P45PFS", "P46PFS", "P47PFS",
            "P50PFS", "P51PFS", "P52PFS", "P54PFS", "P55PFS", "P56PFS",
            "P60PFS", "P66PFS", "P67PFS",
            "P71PFS", "P72PFS", "P73PFS", "P74PFS", "P75PFS", "P76PFS", "P77PFS",
            "P80PFS", "P81PFS", "P82PFS", "P83PFS", "P86PFS", "P87PFS",
            "P90PFS", "P91PFS", "P92PFS", "P93PFS", "P94PFS", "P95PFS", "P96PFS", "P97PFS",
            "PA0PFS", "PA1PFS", "PA2PFS", "PA3PFS", "PA4PFS", "PA5PFS", "PA6PFS", "PA7PFS",
            "PB0PFS", "PB1PFS", "PB2PFS", "PB3PFS", "PB4PFS", "PB5PFS", "PB6PFS", "PB7PFS",
            "PC0PFS", "PC1PFS", "PC2PFS", "PC3PFS", "PC4PFS", "PC5PFS", "PC6PFS", "PC7PFS",
            "PD0PFS", "PD1PFS", "PD2PFS", "PD3PFS", "PD4PFS", "PD5PFS", "PD6PFS", "PD7PFS",
            "PE0PFS", "PE1PFS", "PE2PFS", "PE3PFS", "PE4PFS", "PE5PFS", "PE6PFS", "PE7PFS",
            "PF0PFS", "PF1PFS", "PF2PFS", "PF5PFS",
            "PG0PFS", "PG1PFS", "PG2PFS", "PG3PFS", "PG4PFS", "PG5PFS", "PG6PFS", "PG7PFS",
            "PJ3PFS", "PJ5PFS",
        ];

        foreach (var name in pxxPfsNames) {
            registers[name] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write");
        }

        return registers;
    }
}