using System;
using System.Diagnostics;
using System.Xml;
using System.IO;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser;

public class parser
{
    /// <summary>
    /// Parse the XML DAT file into a RomSignatureObject.
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

    private SignatureParser GetSignatureType(string PathToFile)
    {
        XmlDocument XmlDoc = new XmlDocument();
        try
        {
            XmlDoc.Load(PathToFile);
        }
        catch (Exception ex)
        {
            throw new Exception("Not an XML file", ex);
        }

        // List of parser types to try (ordered by most common to least common for performance)
        var parserTypesToCheck = new[]
        {
            SignatureParser.TOSEC,
            SignatureParser.MAMEArcade, // MAMEParser handles both Arcade and Mess
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

        foreach (var parserType in parserTypesToCheck)
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

    public enum SignatureParser
    {
        Auto = 0,
        TOSEC = 1,
        MAMEArcade = 2,
        MAMEMess = 3,
        NoIntro = 4,
        Redump = 5,
        WHDLoad = 6,
        RetroAchievements = 7,
        FBNeo = 8,
        PureDOSDAT = 9,
        Pleasuredome = 10,
        MAMERedump = 11,
        Generic = 99,
        Unknown = 100
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
