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
}