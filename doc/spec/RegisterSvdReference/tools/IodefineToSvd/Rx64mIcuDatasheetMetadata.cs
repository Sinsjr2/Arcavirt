namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 15章(Interrupt Controller (ICUA), p.412-481)から手動で書き写したレジスタごとの
/// リセット値・アクセス権限。アドレス・ビット構成は代表的なレジスタ
/// (IR/NMIER/NMICLR/GCRBE0/SLIPRCR等)についてマニュアル本文と個別に
/// 突き合わせて確認済み(2026-07-28)。全190レジスタの網羅的な逐一照合は
/// 行っていない。
///
/// PIBR0-A/PIAR0-B/SLIBXR128-143/SLIBR144-207/SLIAR208-255は、iodefine.h上は
/// レジスタ名末尾の番号ごとに個別のstructメンバーとして展開されている
/// (CMT/PORTのような「同名レジスタをインスタンス間で共有」ではなく、
/// レジスタ名自体が番号ごとに異なる)が、データシート上は
/// 「PIBRk (k = 0h to Ah)」のように1回だけ汎用的に記載され、全て同一の
/// リセット値・アクセス権限を持つことを確認済み。CMT/PORTと同じ手書き列挙では
/// 150件超の重複行になり転記ミスの温床になるため、ここではループで生成する。
///
/// レジスタ単位でAccessを1つに集約する都合上(RegisterDatasheetMetadataは
/// レジスタ全体で1つのAccess文字列しか表現できない)、ビットごとにR/RW/W(1クリア)
/// が混在するレジスタ(IR/DTCER/NMIER/NMICLR/GCRBE0/SLIPRCR等)は
/// "read-write"として扱う。個別ビットの正確な挙動はデータシート本文を参照のこと。
/// GRPBE0/GRPBL0/GRPBL1/GRPAL0/GRPAL1(全ビットR、グループ割り込みステータス)は
/// 唯一の例外でread-only。
/// </summary>
public static class Rx64mIcuDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => BuildRegisters();

    static Dictionary<string, RegisterDatasheetMetadata> BuildRegisters() {
        var registers = new Dictionary<string, RegisterDatasheetMetadata> {
            // 配列レジスタ(dim対応、IodefineToSvdでは1要素分のメタデータが
            // 全インデックスに適用される)
            ["IR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DTCER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["IER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["IPR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["IRQCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

            ["FIR"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
            ["SWINTR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["SWINT2R"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

            ["DMRSR0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DMRSR1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DMRSR2"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DMRSR3"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DMRSR4"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DMRSR5"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DMRSR6"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["DMRSR7"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

            ["IRQFLTE0"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["IRQFLTE1"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["IRQFLTC0"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),
            ["IRQFLTC1"] = new RegisterDatasheetMetadata(0x0000, 0xFFFF, "read-write"),

            ["NMISR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-only"),
            ["NMIER"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["NMICLR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["NMICR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["NMIFLTE"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["NMIFLTC"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),

            ["GRPBE0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
            ["GRPBL0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
            ["GRPBL1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
            ["GRPAL0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),
            ["GRPAL1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-only"),

            ["GENBE0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
            ["GENBL0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
            ["GENBL1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
            ["GENAL0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
            ["GENAL1"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),

            ["GCRBE0"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),

            ["SELEXDR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
            ["SLIPRCR"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write"),
        };

        // PIBRk (k = 0h to Ah): 15.2.23、全11レジスタ同一定義(0x00, read-write)
        foreach (var suffix in "0123456789A") {
            registers[$"PIBR{suffix}"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write");
        }

        // PIARk (k = 0h to Bh): 15.2.24、全12レジスタ同一定義(0x00, read-write)
        foreach (var suffix in "0123456789AB") {
            registers[$"PIAR{suffix}"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write");
        }

        // SLIBXRn (n = 128 to 143): 15.2.25、全16レジスタ同一定義(0x00, read-write)
        for (var n = 128; n <= 143; n++) {
            registers[$"SLIBXR{n}"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write");
        }

        // SLIBRn (n = 144 to 207): 15.2.26、全64レジスタ同一定義(0x00, read-write)
        for (var n = 144; n <= 207; n++) {
            registers[$"SLIBR{n}"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write");
        }

        // SLIARn (n = 208 to 255): 15.2.27、全48レジスタ同一定義(0x00, read-write)
        for (var n = 208; n <= 255; n++) {
            registers[$"SLIAR{n}"] = new RegisterDatasheetMetadata(0x00, 0xFF, "read-write");
        }

        return registers;
    }
}