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
}