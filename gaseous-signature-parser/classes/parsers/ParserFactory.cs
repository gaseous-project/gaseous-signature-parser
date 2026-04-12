using System;

namespace gaseous_signature_parser.classes.parsers
{
    /// <summary>
    /// Factory class for creating parser instances based on signature type
    /// </summary>
    public static class ParserFactory
    {
        /// <summary>
        /// Creates a parser instance based on the provided signature parser type
        /// </summary>
        /// <param name="parserType">The type of parser to create</param>
        /// <returns>An instance of IParser for the specified type</returns>
        /// <exception cref="ArgumentException">Thrown when an unknown or unsupported parser type is provided</exception>
        public static IParser CreateParser(parser.SignatureParser parserType)
        {
            return parserType switch
            {
                parser.SignatureParser.TOSEC => new TosecParser(),
                parser.SignatureParser.MAMEArcade => new MAMEParser(),
                parser.SignatureParser.MAMEMess => new MAMEParser(),
                parser.SignatureParser.NoIntro => new NoIntrosParser(),
                parser.SignatureParser.Redump => new RedumpParser(),
                parser.SignatureParser.WHDLoad => new WHDLoadParser(),
                parser.SignatureParser.RetroAchievements => new RetroAchievementsParser(),
                parser.SignatureParser.FBNeo => new FBNeoParser(),
                parser.SignatureParser.PureDOSDAT => new PureDOSDATParser(),
                parser.SignatureParser.Pleasuredome => new PleasuredomeParser(),
                parser.SignatureParser.MAMERedump => new MAMERedumpParser(),
                parser.SignatureParser.ScreenScraper => new ScreenScraperParser(),
                parser.SignatureParser.Generic => new GenericParser(),
                parser.SignatureParser.Unknown => throw new ArgumentException("Cannot create parser for Unknown type", nameof(parserType)),
                parser.SignatureParser.Auto => throw new ArgumentException("Cannot create parser for Auto type. Use GetSignatureType first.", nameof(parserType)),
                _ => throw new ArgumentException($"Unknown parser type: {parserType}", nameof(parserType))
            };
        }
    }
}
