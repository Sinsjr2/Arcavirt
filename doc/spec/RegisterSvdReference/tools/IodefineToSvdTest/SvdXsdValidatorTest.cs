using System.Runtime.CompilerServices;
using System.Xml.Linq;
using IodefineToSvd;
using NUnit.Framework;

namespace IodefineToSvdTest;

public class SvdXsdValidatorTest {
    static string GetRepoPath(string relativePath, [CallerFilePath] string sourceFilePath = "") {
        var testProjectDir = Path.GetDirectoryName(sourceFilePath)!;
        var repoRoot = Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "..", "..", ".."));
        return Path.Combine(repoRoot, relativePath);
    }

    /// <summary>
    /// Arcavirt-c0i.1で生成したCMTパイロット出力(rx64m-cmt-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mCmtPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-cmt-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.8で生成したPORTパイロット出力(rx64m-port-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// PIDRのようにresetValueを持たないレジスタが混在していても
    /// XSD検証が通ることを確認する意味も持つ。
    /// </summary>
    [Test]
    public void Validate_Rx64mPortPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-port-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.9で生成したSCIパイロット出力(rx64m-sci-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// SMCIn(SCInと同一ベースアドレスのalternatePeripheral)が混在していても
    /// XSD検証が通ることを確認する意味も持つ。
    /// </summary>
    [Test]
    public void Validate_Rx64mSciPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-sci-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.11で生成したICUパイロット出力(rx64m-icu-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// IR[256]等のdim/dimIncrement付きレジスタ(このプロジェクト初のdim使用例)が
    /// 混在していてもXSD検証が通ることを確認する意味も持つ。
    /// </summary>
    [Test]
    public void Validate_Rx64mIcuPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-icu-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.12で生成したSYSTEMパイロット出力(rx64m-system-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// リセット要因依存・オプション設定メモリ依存等でResetValueがnullの
    /// レジスタが混在していてもXSD検証が通ることを確認する意味も持つ。
    /// </summary>
    [Test]
    public void Validate_Rx64mSystemPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-system-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.10で生成したSCIFAパイロット出力(rx64m-scifa-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// BRR/MDDR(同一アドレスのalternateRegister、このプロジェクト初のレジスタ単位
    /// エイリアス表現)が混在していてもXSD検証が通ることを確認する意味も持つ。
    /// </summary>
    [Test]
    public void Validate_Rx64mScifaPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-scifa-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.13で生成したCACパイロット出力(rx64m-cac-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mCacPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-cac-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.14で生成したCRCパイロット出力(rx64m-crc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mCrcPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-crc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.15で生成したDAパイロット出力(rx64m-da-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mDaPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-da-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.16で生成したDOCパイロット出力(rx64m-doc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mDocPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-doc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.17で生成したTEMPSパイロット出力(rx64m-temps-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mTempsPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-temps-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.18で生成したWDTパイロット出力(rx64m-wdt-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mWdtPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-wdt-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.19で生成したIWDTパイロット出力(rx64m-iwdt-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mIwdtPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-iwdt-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.20で生成したMPUパイロット出力(rx64m-mpu-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mMpuPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-mpu-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.21で生成したECCRAMパイロット出力(rx64m-eccram-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mEccramPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-eccram-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.22で生成したELCパイロット出力(rx64m-elc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mElcPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-elc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.23で生成したBSCパイロット出力(rx64m-bsc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mBscPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-bsc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.24で生成したPDCパイロット出力(rx64m-pdc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mPdcPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-pdc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.25で生成したSSIパイロット出力(rx64m-ssi-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mSsiPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-ssi-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.26で生成したSRCパイロット出力(rx64m-src-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mSrcPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-src-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.27で生成したPPGパイロット出力(rx64m-ppg-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mPpgPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-ppg-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.28で生成したGPTパイロット出力(rx64m-gpt-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mGptPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-gpt-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.29で生成したRIICパイロット出力(rx64m-riic-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mRiicPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-riic-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.30で生成したETHERCパイロット出力(rx64m-etherc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mEthercPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-etherc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.31で生成したMMCIFパイロット出力(rx64m-mmcif-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mMmcifPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-mmcif-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.33で生成したRSPIパイロット出力(rx64m-rspi-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mRspiPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-rspi-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.34で生成したSDHIパイロット出力(rx64m-sdhi-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mSdhiPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-sdhi-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.35で生成したDMACパイロット出力(rx64m-dmac-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mDmacPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-dmac-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.36で生成したEXDMACパイロット出力(rx64m-exdmac-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mExdmacPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-exdmac-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.37で生成したPOEパイロット出力(rx64m-poe-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mPoePilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-poe-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.38で生成したPTPEDMACパイロット出力(rx64m-ptpedmac-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。TRSCER・IOSRの
    /// ようにaccess/resetValueを持たないレジスタが混在していてもXSD検証が通ることを
    /// 確認する意味も持つ。
    /// </summary>
    [Test]
    public void Validate_Rx64mPtpedmacPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-ptpedmac-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.39で生成したTMR0パイロット出力(rx64m-tmr0-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mTmr0PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-tmr0-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.39で生成したTMR01パイロット出力(rx64m-tmr01-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mTmr01PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-tmr01-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.39で生成したTMR1パイロット出力(rx64m-tmr1-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mTmr1PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-tmr1-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.41で生成したMPCパイロット出力(rx64m-mpc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mMpcPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-mpc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.40で生成したUSBパイロット出力(rx64m-usb-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mUsbPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-usb-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.40で生成したUSB0パイロット出力(rx64m-usb0-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mUsb0PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-usb0-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.42で生成したDMAC0パイロット出力(rx64m-dmac0-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mDmac0PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-dmac0-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.42で生成したDMAC1パイロット出力(rx64m-dmac1-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mDmac1PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-dmac1-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.43で生成したDTCパイロット出力(rx64m-dtc-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mDtcPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-dtc-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.44で生成したEDMACパイロット出力(rx64m-edmac-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mEdmacPilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-edmac-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.45で生成したEXDMAC0パイロット出力(rx64m-exdmac0-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mExdmac0PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-exdmac0-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }

    /// <summary>
    /// Arcavirt-c0i.45で生成したEXDMAC1パイロット出力(rx64m-exdmac1-pilot.svd)が、
    /// CMSIS-SVD公式XSD(v1.3.9)に構造的に適合することを確認する。
    /// </summary>
    [Test]
    public void Validate_Rx64mExdmac1PilotSvd_PassesCmsisSvdXsd() {
        var xsdPath = GetRepoPath("doc/spec/RegisterSvdReference/CMSIS-SVD.xsd");
        var svdPath = GetRepoPath("doc/spec/RegisterSvdReference/rx64m-exdmac1-pilot.svd");

        var document = XDocument.Load(svdPath);
        var errors = SvdXsdValidator.Validate(document, xsdPath);

        Assert.That(errors, Is.Empty);
    }
}