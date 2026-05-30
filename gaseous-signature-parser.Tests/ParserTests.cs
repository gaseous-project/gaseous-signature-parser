using gaseous_signature_parser;
using gaseous_signature_parser.classes.bracketparsers;
using gaseous_signature_parser.classes.parsers;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.Tests;

/// <summary>
/// Tests that each parser's Parse() method returns a well-formed, populated
/// RomSignatureObject from its own fixture file, and verifies key field values.
/// </summary>
public class ParserTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string TestData(params string[] segments) =>
        Path.Combine(
            new[] { AppDomain.CurrentDomain.BaseDirectory, "TestData" }.Concat(segments).ToArray());

    private static void AssertBasicShape(RomSignatureObject result, string expectedSourceType)
    {
        Assert.NotNull(result);
        Assert.Equal(expectedSourceType, result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);
        Assert.False(string.IsNullOrWhiteSpace(result.Games[0].Name),
            "First game should have a non-empty Name.");
        Assert.NotEmpty(result.Games[0].Roms);
    }

    // -------------------------------------------------------------------------
    // TOSEC
    // -------------------------------------------------------------------------

    [Fact]
    public void TOSEC_Parse_ReturnsExpectedData()
    {
        var result = new TosecParser().Parse(TestData("tosec", "sample.dat"));
        AssertBasicShape(result, "TOSEC");

        // The TOSEC parser strips the parenthesised metadata from the game title
        var game = result.Games.Single(g => g.Name == "Space Invaders");
        Assert.Equal("1978", game.Year);
        Assert.Equal("Taito", game.Publisher);
        Assert.Equal("abcd1234", game.Roms[0].Crc, StringComparer.OrdinalIgnoreCase);
    }

    // -------------------------------------------------------------------------
    // MAME Arcade
    // -------------------------------------------------------------------------

    [Fact]
    public void MAMEArcade_Parse_ReturnsExpectedData()
    {
        var options = new Dictionary<string, object>
        {
            { "DocumentType", parser.SignatureParser.MAMEArcade }
        };
        var result = new MAMEParser().Parse(TestData("mame-arcade", "sample.dat"), options);
        AssertBasicShape(result, "MAMEArcade");

        var roms = result.Games.SelectMany(g => g.Roms).ToList();
        Assert.All(roms, r => Assert.Equal(
            RomSignatureObject.Game.Rom.SignatureSourceType.MAMEArcade, r.SignatureSource));
    }

    // -------------------------------------------------------------------------
    // MAME Mess
    // -------------------------------------------------------------------------

    [Fact]
    public void MAMEMess_Parse_ReturnsExpectedData()
    {
        var options = new Dictionary<string, object>
        {
            { "DocumentType", parser.SignatureParser.MAMEMess }
        };
        var result = new MAMEParser().Parse(TestData("mame-mess", "sample.dat"), options);
        AssertBasicShape(result, "MAMEMess");

        var roms = result.Games.SelectMany(g => g.Roms).ToList();
        Assert.All(roms, r => Assert.Equal(
            RomSignatureObject.Game.Rom.SignatureSourceType.MAMEMess, r.SignatureSource));
    }

    // -------------------------------------------------------------------------
    // No-Intro
    // -------------------------------------------------------------------------

    [Fact]
    public void NoIntro_Parse_ReturnsExpectedData()
    {
        var result = new NoIntrosParser().Parse(TestData("nointro", "sample.dat"));
        AssertBasicShape(result, "No-Intro");

        var tetris = result.Games.Single(g => g.Name == "Tetris");
        Assert.NotEmpty(tetris.Roms);
        Assert.Equal("46df91ad", tetris.Roms[0].Crc, StringComparer.OrdinalIgnoreCase);
    }

    // -------------------------------------------------------------------------
    // Redump
    // -------------------------------------------------------------------------

    [Fact]
    public void Redump_Parse_ReturnsExpectedData()
    {
        var result = new RedumpParser().Parse(TestData("redump", "sample.dat"));
        AssertBasicShape(result, "Redump");

        var game = result.Games.Single(g => g.Name == "007 - Agent Under Fire");
        Assert.NotEmpty(game.Roms);
        Assert.Equal(RomSignatureObject.Game.Rom.SignatureSourceType.Redump,
            game.Roms[0].SignatureSource);
    }

    // -------------------------------------------------------------------------
    // WHDLoad
    // -------------------------------------------------------------------------

    [Fact]
    public void WHDLoad_Parse_ReturnsExpectedData()
    {
        var result = new WHDLoadParser().Parse(TestData("whdload", "sample.dat"));
        Assert.NotNull(result);
        Assert.Equal("WHDLoad", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);

        var aladdin = result.Games.Single(g => g.Name == "Aladdin");
        Assert.NotEmpty(aladdin.Roms);
        Assert.Equal("aabbccddeeff00112233445566778899aabbccdd",
            aladdin.Roms[0].Sha1, StringComparer.OrdinalIgnoreCase);
    }

    // -------------------------------------------------------------------------
    // RetroAchievements
    // -------------------------------------------------------------------------

    [Fact]
    public void RetroAchievements_Parse_ReturnsExpectedData()
    {
        var result = new RetroAchievementsParser().Parse(TestData("retroachievements", "sample.dat"));
        AssertBasicShape(result, "RetroAchievements");

        var roms = result.Games.SelectMany(g => g.Roms).ToList();
        Assert.All(roms, r => Assert.Equal(
            RomSignatureObject.Game.Rom.SignatureSourceType.RetroAchievements, r.SignatureSource));
    }

    // -------------------------------------------------------------------------
    // FBNeo
    // -------------------------------------------------------------------------

    [Fact]
    public void FBNeo_Parse_ReturnsExpectedData()
    {
        var result = new FBNeoParser().Parse(TestData("fbneo", "sample.dat"));
        AssertBasicShape(result, "FBNeo");

        var roms = result.Games.SelectMany(g => g.Roms).ToList();
        Assert.All(roms, r => Assert.Equal(
            RomSignatureObject.Game.Rom.SignatureSourceType.FBNeo, r.SignatureSource));
    }

    // -------------------------------------------------------------------------
    // PureDOSDAT
    // -------------------------------------------------------------------------

    [Fact]
    public void PureDOSDAT_Parse_ReturnsExpectedData()
    {
        var result = new PureDOSDATParser().Parse(TestData("puredosdat", "sample.dat"));
        AssertBasicShape(result, "PureDOSDAT");

        var keen = result.Games.Single(g => g.Name == "Commander Keen - Marooned on Mars");
        Assert.Equal("1990", keen.Year);
        Assert.Equal("id Software", keen.Publisher);
    }

    // -------------------------------------------------------------------------
    // Pleasuredome
    // -------------------------------------------------------------------------

    [Fact]
    public void Pleasuredome_Parse_ReturnsExpectedData()
    {
        var result = new PleasuredomeParser().Parse(TestData("pleasuredome", "sample.dat"));
        Assert.NotNull(result);
        Assert.Equal("Pleasuredome", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);

        var roms = result.Games.SelectMany(g => g.Roms).ToList();
        Assert.All(roms, r => Assert.Equal(
            RomSignatureObject.Game.Rom.SignatureSourceType.Pleasuredome, r.SignatureSource));
    }

    // -------------------------------------------------------------------------
    // MAMERedump
    // -------------------------------------------------------------------------

    [Fact]
    public void MAMERedump_Parse_ReturnsExpectedData()
    {
        var result = new MAMERedumpParser().Parse(TestData("mame-redump", "sample.dat"));
        Assert.NotNull(result);
        Assert.Equal("MAMERedump", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);

        var crash = result.Games.Single(g => g.Name == "Crash Bandicoot");
        Assert.NotEmpty(crash.Roms);
    }

    // -------------------------------------------------------------------------
    // Generic
    // -------------------------------------------------------------------------

    [Fact]
    public void Generic_Parse_ReturnsExpectedData()
    {
        var result = new GenericParser().Parse(TestData("generic", "sample.dat"));
        Assert.NotNull(result);
        Assert.Equal("Generic", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);

        Assert.Equal(2, result.Games.Count);
        var roms = result.Games.SelectMany(g => g.Roms).ToList();
        Assert.All(roms, r => Assert.Equal(
            RomSignatureObject.Game.Rom.SignatureSourceType.Generic, r.SignatureSource));
    }

    // -------------------------------------------------------------------------
    // ScreenScraper
    // -------------------------------------------------------------------------

    [Fact]
    public void ScreenScraper_Parse_ReturnsNonNull()
    {
        // ScreenScraper parsing builds a single-game object from the jeu node.
        var result = new ScreenScraperParser().Parse(TestData("screenscraper", "sample.dat"));
        Assert.NotNull(result);
        Assert.Equal("ScreenScraper", result.SourceType);
    }

    // -------------------------------------------------------------------------
    // Total DOS Collection (bracket DAT)
    // -------------------------------------------------------------------------

    [Fact]
    public void TotalDOSCollection_Parse_ReturnsExpectedData()
    {
        var result = new TotalDOSCollectionParser().Parse(TestData("totaldoscollection", "sample.dat"));
        Assert.NotNull(result);
        Assert.Equal("TotalDOSCollection", result.SourceType);
        Assert.NotNull(result.Games);
        Assert.NotEmpty(result.Games);

        // Game names are parsed from the archive filename (Title (Year)(Publisher).zip)
        var keen = result.Games.Single(g => g.Name!.Contains("Commander Keen", StringComparison.OrdinalIgnoreCase));
        Assert.NotEmpty(keen.Roms);
    }
}
