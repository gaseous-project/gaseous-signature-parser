using System.Xml;
using gaseous_signature_parser;
using gaseous_signature_parser.classes.bracketparsers;
using gaseous_signature_parser.classes.parsers;

namespace gaseous_signature_parser.Tests;

/// <summary>
/// Tests that each parser's GetXmlType / GetDatType correctly identifies
/// its own fixture file and returns Unknown for mismatched files.
/// </summary>
public class DetectionTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string TestData(params string[] segments) =>
        Path.Combine(
            new[] { AppDomain.CurrentDomain.BaseDirectory, "TestData" }.Concat(segments).ToArray());

    private static XmlDocument LoadXml(string path)
    {
        var doc = new XmlDocument();
        doc.Load(path);
        return doc;
    }

    // -------------------------------------------------------------------------
    // TOSEC
    // -------------------------------------------------------------------------

    [Fact]
    public void TOSEC_Detects_Own_Fixture()
    {
        var p = new TosecParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.TOSEC, result);
    }

    [Fact]
    public void TOSEC_Returns_Unknown_For_Mismatch()
    {
        var p = new TosecParser();
        var result = p.GetXmlType(LoadXml(TestData("nointro", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // MAME Arcade
    // -------------------------------------------------------------------------

    [Fact]
    public void MAMEArcade_Detects_Own_Fixture()
    {
        var p = new MAMEParser();
        var result = p.GetXmlType(LoadXml(TestData("mame-arcade", "sample.dat")));
        Assert.Equal(parser.SignatureParser.MAMEArcade, result);
    }

    [Fact]
    public void MAMEArcade_Returns_Unknown_For_Mismatch()
    {
        var p = new MAMEParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // MAME Mess
    // -------------------------------------------------------------------------

    [Fact]
    public void MAMEMess_Detects_Own_Fixture()
    {
        var p = new MAMEParser();
        var result = p.GetXmlType(LoadXml(TestData("mame-mess", "sample.dat")));
        Assert.Equal(parser.SignatureParser.MAMEMess, result);
    }

    [Fact]
    public void MAMEMess_Returns_Unknown_For_Mismatch()
    {
        var p = new MAMEParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // No-Intro
    // -------------------------------------------------------------------------

    [Fact]
    public void NoIntro_Detects_Own_Fixture()
    {
        var p = new NoIntrosParser();
        var result = p.GetXmlType(LoadXml(TestData("nointro", "sample.dat")));
        Assert.Equal(parser.SignatureParser.NoIntro, result);
    }

    [Fact]
    public void NoIntro_Returns_Unknown_For_Mismatch()
    {
        var p = new NoIntrosParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // Redump
    // -------------------------------------------------------------------------

    [Fact]
    public void Redump_Detects_Own_Fixture()
    {
        var p = new RedumpParser();
        var result = p.GetXmlType(LoadXml(TestData("redump", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Redump, result);
    }

    [Fact]
    public void Redump_Returns_Unknown_For_Mismatch()
    {
        var p = new RedumpParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // WHDLoad
    // -------------------------------------------------------------------------

    [Fact]
    public void WHDLoad_Detects_Own_Fixture()
    {
        var p = new WHDLoadParser();
        var result = p.GetXmlType(LoadXml(TestData("whdload", "sample.dat")));
        Assert.Equal(parser.SignatureParser.WHDLoad, result);
    }

    [Fact]
    public void WHDLoad_Returns_Unknown_For_Mismatch()
    {
        var p = new WHDLoadParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // RetroAchievements
    // -------------------------------------------------------------------------

    [Fact]
    public void RetroAchievements_Detects_Own_Fixture()
    {
        var p = new RetroAchievementsParser();
        var result = p.GetXmlType(LoadXml(TestData("retroachievements", "sample.dat")));
        Assert.Equal(parser.SignatureParser.RetroAchievements, result);
    }

    [Fact]
    public void RetroAchievements_Returns_Unknown_For_Mismatch()
    {
        var p = new RetroAchievementsParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // FBNeo
    // -------------------------------------------------------------------------

    [Fact]
    public void FBNeo_Detects_Own_Fixture()
    {
        var p = new FBNeoParser();
        var result = p.GetXmlType(LoadXml(TestData("fbneo", "sample.dat")));
        Assert.Equal(parser.SignatureParser.FBNeo, result);
    }

    [Fact]
    public void FBNeo_Returns_Unknown_For_Mismatch()
    {
        var p = new FBNeoParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // PureDOSDAT
    // -------------------------------------------------------------------------

    [Fact]
    public void PureDOSDAT_Detects_Own_Fixture()
    {
        var p = new PureDOSDATParser();
        var result = p.GetXmlType(LoadXml(TestData("puredosdat", "sample.dat")));
        Assert.Equal(parser.SignatureParser.PureDOSDAT, result);
    }

    [Fact]
    public void PureDOSDAT_Returns_Unknown_For_Mismatch()
    {
        var p = new PureDOSDATParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // Pleasuredome
    // -------------------------------------------------------------------------

    [Fact]
    public void Pleasuredome_Detects_Own_Fixture()
    {
        var p = new PleasuredomeParser();
        var result = p.GetXmlType(LoadXml(TestData("pleasuredome", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Pleasuredome, result);
    }

    [Fact]
    public void Pleasuredome_Returns_Unknown_For_Mismatch()
    {
        var p = new PleasuredomeParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // MAMERedump
    // -------------------------------------------------------------------------

    [Fact]
    public void MAMERedump_Detects_Own_Fixture()
    {
        var p = new MAMERedumpParser();
        var result = p.GetXmlType(LoadXml(TestData("mame-redump", "sample.dat")));
        Assert.Equal(parser.SignatureParser.MAMERedump, result);
    }

    [Fact]
    public void MAMERedump_Returns_Unknown_For_Mismatch()
    {
        var p = new MAMERedumpParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // Generic
    // -------------------------------------------------------------------------

    [Fact]
    public void Generic_Detects_Own_Fixture()
    {
        var p = new GenericParser();
        var result = p.GetXmlType(LoadXml(TestData("generic", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Generic, result);
    }

    [Fact]
    public void Generic_Returns_Unknown_For_Mismatch()
    {
        // TOSEC has no <machine> element, so Generic should not claim it
        var p = new GenericParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // ScreenScraper
    // -------------------------------------------------------------------------

    [Fact]
    public void ScreenScraper_Detects_Own_Fixture()
    {
        var p = new ScreenScraperParser();
        var result = p.GetXmlType(LoadXml(TestData("screenscraper", "sample.dat")));
        Assert.Equal(parser.SignatureParser.ScreenScraper, result);
    }

    [Fact]
    public void ScreenScraper_Returns_Unknown_For_Mismatch()
    {
        var p = new ScreenScraperParser();
        var result = p.GetXmlType(LoadXml(TestData("tosec", "sample.dat")));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }

    // -------------------------------------------------------------------------
    // Total DOS Collection (bracket DAT)
    // -------------------------------------------------------------------------

    [Fact]
    public void TotalDOSCollection_Detects_Own_Fixture()
    {
        var p = new TotalDOSCollectionParser();
        var result = p.GetDatType(TestData("totaldoscollection", "sample.dat"));
        Assert.Equal(parser.SignatureParser.TotalDOSCollection, result);
    }

    [Fact]
    public void TotalDOSCollection_Returns_Unknown_For_Mismatch()
    {
        // XML files are not bracket DATs — pass a TOSEC XML fixture
        var p = new TotalDOSCollectionParser();
        var result = p.GetDatType(TestData("tosec", "sample.dat"));
        Assert.Equal(parser.SignatureParser.Unknown, result);
    }
}
