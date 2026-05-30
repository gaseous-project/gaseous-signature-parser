namespace gaseous_signature_parser.Tests;

/// <summary>
/// Integration tests that drive the full auto-detection + parse pipeline via
/// <see cref="parser.ParseSignatureDAT"/> with <c>SignatureParser.Auto</c>.
/// </summary>
public class AutoDetectionTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string TestData(params string[] segments) =>
        Path.Combine(
            new[] { AppDomain.CurrentDomain.BaseDirectory, "TestData" }.Concat(segments).ToArray());

    private static models.RomSignatureObject.RomSignatureObject AutoParse(string relativePath)
    {
        var p = new parser();
        return p.ParseSignatureDAT(TestData(relativePath), null, parser.SignatureParser.Auto);
    }

    private static void AssertAutoResult(
        models.RomSignatureObject.RomSignatureObject result,
        string expectedSourceType)
    {
        Assert.NotNull(result);
        Assert.Equal(expectedSourceType, result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);
    }

    // -------------------------------------------------------------------------
    // One test per parser
    // -------------------------------------------------------------------------

    [Fact]
    public void AutoDetect_TOSEC()
    {
        AssertAutoResult(AutoParse("tosec/sample.dat"), "TOSEC");
    }

    [Fact]
    public void AutoDetect_MAMEArcade()
    {
        AssertAutoResult(AutoParse("mame-arcade/sample.dat"), "MAMEArcade");
    }

    [Fact]
    public void AutoDetect_MAMEMess()
    {
        AssertAutoResult(AutoParse("mame-mess/sample.dat"), "MAMEMess");
    }

    [Fact]
    public void AutoDetect_NoIntro()
    {
        AssertAutoResult(AutoParse("nointro/sample.dat"), "No-Intro");
    }

    [Fact]
    public void AutoDetect_Redump()
    {
        AssertAutoResult(AutoParse("redump/sample.dat"), "Redump");
    }

    [Fact]
    public void AutoDetect_WHDLoad()
    {
        var result = AutoParse("whdload/sample.dat");
        Assert.NotNull(result);
        Assert.Equal("WHDLoad", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);
    }

    [Fact]
    public void AutoDetect_RetroAchievements()
    {
        AssertAutoResult(AutoParse("retroachievements/sample.dat"), "RetroAchievements");
    }

    [Fact]
    public void AutoDetect_FBNeo()
    {
        AssertAutoResult(AutoParse("fbneo/sample.dat"), "FBNeo");
    }

    [Fact]
    public void AutoDetect_PureDOSDAT()
    {
        AssertAutoResult(AutoParse("puredosdat/sample.dat"), "PureDOSDAT");
    }

    [Fact]
    public void AutoDetect_Pleasuredome()
    {
        var result = AutoParse("pleasuredome/sample.dat");
        Assert.NotNull(result);
        Assert.Equal("Pleasuredome", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);
    }

    [Fact]
    public void AutoDetect_MAMERedump()
    {
        var result = AutoParse("mame-redump/sample.dat");
        Assert.NotNull(result);
        Assert.Equal("MAMERedump", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);
    }

    [Fact]
    public void AutoDetect_Generic()
    {
        AssertAutoResult(AutoParse("generic/sample.dat"), "Generic");
    }

    [Fact]
    public void AutoDetect_ScreenScraper()
    {
        var result = AutoParse("screenscraper/sample.dat");
        Assert.NotNull(result);
        Assert.Equal("ScreenScraper", result.SourceType);
    }

    [Fact]
    public void AutoDetect_TotalDOSCollection()
    {
        AssertAutoResult(AutoParse("totaldoscollection/sample.dat"), "TotalDOSCollection");
    }
}
