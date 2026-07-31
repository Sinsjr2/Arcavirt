namespace IodefineToSvd;

/// <summary>
/// RX64M Group User's Manual: Hardware (R01UH0377EJ0120 Rev.1.20, Oct 17, 2022)
/// 38章(USB 2.0 FS Host/Function Module (USBb)、p.1735-1843の38.2 Register Descriptions)
/// から手動で書き写した、共有ブロック(st_usb、インスタンスUSB)のレジスタごとの
/// リセット値・アクセス権限。DPUSR0R/DPUSR1Rについてマニュアル本文と個別に
/// 突き合わせて確認済み(2026-08-01)。st_usb0(チャネルブロック、インスタンスUSB0)は
/// 別ファイル(Rx64mUsb0DatasheetMetadata.cs)を参照。
///
/// 部分マスク(Undefinedビット)を使用した箇所:
/// - DPUSR0R: DP0(b16)/DM0(b17)/DOVCA0(b20)/DOVCB0(b21)/DVBSTS0(b23)は、レジスタ図の
///   "Value after reset"行で明示的に"x"(Undefined)と記載されており、USB0のD+/D-/
///   OVRCURA/OVRCURB/VBUS各入力ピンの状態をそのまま反映するモニタフラグのため、
///   単一の固定リセット値を持たない。該当ビットをResetMaskから除外した
///   (ResetMask=0xFF4CFFFF)。
/// </summary>
public static class Rx64mUsbDatasheetMetadata {
    public static IReadOnlyDictionary<string, RegisterDatasheetMetadata> Registers => new Dictionary<string, RegisterDatasheetMetadata> {
        // 38.2.37 Deep Standby USB Transceiver Control/Pin Monitoring Register (DPUSR0R) - 上記コメント参照(部分マスク)
        ["DPUSR0R"] = new RegisterDatasheetMetadata(0x00000000, 0xFF4CFFFF, "read-write"),
        // 38.2.38 Deep Standby USB Suspend/Resume Interrupt Register (DPUSR1R)
        ["DPUSR1R"] = new RegisterDatasheetMetadata(0x00000000, 0xFFFFFFFF, "read-write"),
    };
}