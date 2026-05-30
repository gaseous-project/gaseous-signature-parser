using System;
using System.Diagnostics;
using System.Xml;
using System.IO;
using gaseous_signature_parser.models.RomSignatureObject;
using Newtonsoft.Json.Linq;

namespace gaseous_signature_parser;

public class parser
{
    /// <summary>
    /// List of XML parser types to try when detecting the signature type of a file. The order should be based on which formats are most common or easiest to check for, to optimize performance when trying to detect the signature type.
    /// </summary>
    public static readonly SignatureParser[] xmlParserTypesToCheck = new[]
    {
        SignatureParser.ScreenScraper,
        SignatureParser.TOSEC,
        SignatureParser.MAMEArcade, // MAMEParser handles both Arcade and Mess
        SignatureParser.MAMEMess,
        SignatureParser.NoIntro,
        SignatureParser.Redump,
        SignatureParser.WHDLoad,
        SignatureParser.RetroAchievements,
        SignatureParser.FBNeo,
        SignatureParser.PureDOSDAT,
        SignatureParser.Pleasuredome,
        SignatureParser.MAMERedump,
        SignatureParser.Generic
    };

    /// <summary>
    /// List of bracketed DAT parser types to try. These parsers will be used if the file doesn't look like XML or JSON, and will typically check for specific formatting or header data to determine if they can parse the file. The order should be based on which formats are most common or easiest to check for, to optimize performance when trying to detect the signature type.
    /// </summary>
    public static readonly SignatureParser[] bracketParserTypesToCheck = new[]
    {
        SignatureParser.TotalDOSCollection
    };

    /// <summary>
    /// Enum representing the supported signature parser types. The values correspond to the SignatureSourceType enum in RomSignatureObject.Game.Rom, with additional values for Auto detection and Unknown types.
    /// </summary>
    public enum SignatureParser
    {
        Auto = 0,
        TOSEC = RomSignatureObject.Game.Rom.SignatureSourceType.TOSEC,
        MAMEArcade = RomSignatureObject.Game.Rom.SignatureSourceType.MAMEArcade,
        MAMEMess = RomSignatureObject.Game.Rom.SignatureSourceType.MAMEMess,
        NoIntro = RomSignatureObject.Game.Rom.SignatureSourceType.NoIntros,
        Redump = RomSignatureObject.Game.Rom.SignatureSourceType.Redump,
        WHDLoad = RomSignatureObject.Game.Rom.SignatureSourceType.WHDLoad,
        RetroAchievements = RomSignatureObject.Game.Rom.SignatureSourceType.RetroAchievements,
        FBNeo = RomSignatureObject.Game.Rom.SignatureSourceType.FBNeo,
        PureDOSDAT = RomSignatureObject.Game.Rom.SignatureSourceType.PureDOSDAT,
        Pleasuredome = RomSignatureObject.Game.Rom.SignatureSourceType.Pleasuredome,
        MAMERedump = RomSignatureObject.Game.Rom.SignatureSourceType.MAMERedump,
        Generic = RomSignatureObject.Game.Rom.SignatureSourceType.Generic,
        ScreenScraper = RomSignatureObject.Game.Rom.SignatureSourceType.ScreenScraper,
        TotalDOSCollection = RomSignatureObject.Game.Rom.SignatureSourceType.TotalDOSCollection,
        Unknown = 100
    }

    /// <summary>
    /// Parse a supported signature file into a RomSignatureObject.
    /// </summary>
    /// <param name="PathToFile">The full path to the signature file to attempt to parse</param>
    /// <param name="Parser">Which parser to use when parsing the provided signature file</param>
    /// <returns></returns>
    public RomSignatureObject ParseSignatureDAT(string PathToFile, string? PathToDBFile = null, SignatureParser Parser = SignatureParser.Auto)
    {
        SignatureParser DetectedSignatureType = SignatureParser.Auto;
        if (Parser == SignatureParser.Auto)
        {
            try
            {
                Debug.WriteLine("Checking: " + PathToFile);
                DetectedSignatureType = GetSignatureType(PathToFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unknown file type");
                return null;
            }
        }
        else
        {
            DetectedSignatureType = Parser;
        }

        if (xmlParserTypesToCheck.Contains(DetectedSignatureType))
        {
            // Use factory to create the appropriate parser
            classes.parsers.IParser parser = classes.parsers.ParserFactory.CreateParser(DetectedSignatureType);

            // Prepare options dictionary for parsers that need extra parameters
            Dictionary<string, object>? options = null;

            // MAME parsers need to know which type they are
            if (DetectedSignatureType == SignatureParser.MAMEArcade || DetectedSignatureType == SignatureParser.MAMEMess)
            {
                options = new Dictionary<string, object>
            {
                { "DocumentType", DetectedSignatureType }
            };
            }
            // NoIntro parser needs the optional database file
            else if (DetectedSignatureType == SignatureParser.NoIntro && PathToDBFile != null)
            {
                options = new Dictionary<string, object>
            {
                { "PathToDBFile", PathToDBFile }
            };
            }

            return parser.Parse(PathToFile, options);
        }
        else if (bracketParserTypesToCheck.Contains(DetectedSignatureType))
        {
            // Use factory to create the appropriate parser
            classes.bracketparsers.IParser parser = classes.bracketparsers.ParserFactory.CreateParser(DetectedSignatureType);
            return parser.Parse(PathToFile);
        }
        else
        {
            Debug.WriteLine("Unsupported or unknown signature type");
            return null;
        }
    }

    private SignatureParser GetSignatureType(string PathToFile)
    {
        // check if file starts with { or [ to determine if it's JSON before trying to parse as XML
        char firstChar = ReadFirstNonWhitespaceCharacter(PathToFile);
        if (firstChar == '{' || firstChar == '[')
        {
            return GetJsonSignatureType(PathToFile);
        }

        // check if the file starts with a string followed by a (, which is a common format for bracketed DATs like TotalDOSCollection
        if (char.IsLetter(firstChar))
        {
            string? firstLine = File.ReadLines(PathToFile).FirstOrDefault();
            if (firstLine != null && firstLine.Contains('('))
            {
                var bracketParserTypesToCheck = new[]
                {
                    SignatureParser.TotalDOSCollection
                };

                foreach (var parserType in bracketParserTypesToCheck)
                {
                    try
                    {
                        classes.bracketparsers.IParser parser = classes.bracketparsers.ParserFactory.CreateParser(parserType);
                        SignatureParser detectedType = parser.GetDatType(PathToFile);

                        if (detectedType != SignatureParser.Unknown)
                        {
                            Debug.WriteLine($"{detectedType}: {PathToFile}");
                            return detectedType;
                        }
                    }
                    catch
                    {
                        // If parser creation fails, continue to next type
                        continue;
                    }
                }

                // unable to determine type
                return SignatureParser.Unknown;
            }
        }

        // fallback to XML parsing if it doesn't look like JSON or bracketed DAT
        XmlDocument XmlDoc = new XmlDocument();
        try
        {
            XmlDoc.Load(PathToFile);
        }
        catch (Exception ex)
        {
            throw new Exception("Not an XML file", ex);
        }

        foreach (var parserType in xmlParserTypesToCheck)
        {
            try
            {
                classes.parsers.IParser parser = classes.parsers.ParserFactory.CreateParser(parserType);
                SignatureParser detectedType = parser.GetXmlType(XmlDoc);

                if (detectedType != SignatureParser.Unknown)
                {
                    Debug.WriteLine($"{detectedType}: {PathToFile}");
                    return detectedType;
                }
            }
            catch
            {
                // If parser creation fails, continue to next type
                continue;
            }
        }

        // unable to determine type
        return SignatureParser.Unknown;
    }

    private static char ReadFirstNonWhitespaceCharacter(string path)
    {
        using StreamReader reader = new StreamReader(path);
        int value;
        while ((value = reader.Read()) != -1)
        {
            char current = (char)value;
            if (!char.IsWhiteSpace(current))
            {
                return current;
            }
        }

        return '\0';
    }

    private static SignatureParser GetJsonSignatureType(string pathToFile)
    {
        try
        {
            string content = File.ReadAllText(pathToFile);
            if (classes.parsers.ScreenScraperParser.IsScreenScraperJson(content))
            {
                return SignatureParser.ScreenScraper;
            }

            JToken.Parse(content);
            return SignatureParser.Unknown;
        }
        catch
        {
            return SignatureParser.Unknown;
        }
    }

    public static Dictionary<string, object> ConvertXmlNodeToDictionary(XmlNode node)
    {
        Dictionary<string, object> map = new Dictionary<string, object>();

        // get node attributes first
        if (node.Attributes != null)
        {
            foreach (XmlAttribute attribute in node.Attributes)
            {
                map.Add(attribute.Name, attribute.Value);
            }
        }

        // get children
        if (node.ChildNodes != null && node.ChildNodes.Count > 0)
        {
            foreach (XmlNode xmlNode in node.ChildNodes)
            {
                map.Add(xmlNode.Name, ConvertXmlNodeToDictionary(xmlNode));
            }
        }

        return map;
    }
}
