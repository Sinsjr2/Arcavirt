namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 24章(Multi-Function Timer Pulse Unit 3 (MTU3a))のMTU8専用レジスタ幅の差分。
///
/// MTU8のTCNT/TGRA/TGRB/TGRC/TGRDは、他チャンネル(MTU0〜4,6,7)と同名だが
/// iodefine.h上はunsigned long(32ビット)であり、データシート24.2.13/24.2.15も
/// MTU8のこれら5レジスタを32ビット(リセット値00000000h/FFFFFFFFh)と明記している
/// (Rx64mMtuDatasheetMetadataのdocコメント参照)。TMR0/TMR01/TMR1が幅・構成の
/// 異なるチャンネルを別プロバイダに分けているのと同じ方針で、Rx64mMtu
/// DatasheetMetadataの16ビット版辞書をベースに、この5レジスタだけ32ビット値で
/// 上書きしたものをMTU8専用として提供する。
/// </summary>
public static class Rx64mMtu8DatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers { get; } =
        new Dictionary<string, RegisterDatasheetMetadata>(Rx64mMtuDatasheetMetadata.Registers) {
            ["TCNT"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
            ["TGRA"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
            ["TGRB"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
            ["TGRC"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
            ["TGRD"] = new RegisterDatasheetMetadata(0xFFFFFFFF, 0xFFFFFFFF, "read-write"),
        };
}