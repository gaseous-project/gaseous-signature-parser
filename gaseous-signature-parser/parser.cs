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
    public RomSignatureObject ParseSignatureDAT(string PathToFile, string? PathToDBFile = null, SignatureParser Parser = SignatureParser.Auto) {
        SignatureParser DetectedSignatureType = SignatureParser.Auto;
        if (Parser == SignatureParser.Auto) {
            try {
                Debug.WriteLine("Checking: " + PathToFile);
                DetectedSignatureType = GetSignatureType(PathToFile);
            }
            catch (Exception ex) {
                Debug.WriteLine("Unknown file type");
                return null;
            }
        } else {
            DetectedSignatureType = Parser;
        }

        switch (DetectedSignatureType) {
            case SignatureParser.TOSEC:
                classes.parsers.TosecParser tosecParser = new classes.parsers.TosecParser();

                return tosecParser.Parse(PathToFile);

            case SignatureParser.MAMEArcade:
            case SignatureParser.MAMEMess:
                classes.parsers.MAMEParser mAMEParser = new classes.parsers.MAMEParser();

                return mAMEParser.Parse(PathToFile, DetectedSignatureType);

            case SignatureParser.NoIntro:
                classes.parsers.NoIntrosParser noIntrosParser = new classes.parsers.NoIntrosParser();

                return noIntrosParser.Parse(PathToFile, PathToDBFile);

            case SignatureParser.Unknown:
            default:
                throw new Exception("Unknown parser type");

        }
    }

    private SignatureParser GetSignatureType(string PathToFile) {
        XmlDocument XmlDoc = new XmlDocument();
        try {
            XmlDoc.Load(PathToFile);
        }
        catch (Exception ex) {
            throw new Exception("Not an XML file", ex);
        }

        // check if TOSEC
        classes.parsers.TosecParser tosecParser = new classes.parsers.TosecParser();
        if (tosecParser.GetXmlType(XmlDoc) == SignatureParser.TOSEC) {
            Debug.WriteLine("TOSEC: " + PathToFile);
            return SignatureParser.TOSEC;
        }

        // check if MAMEArcade
        classes.parsers.MAMEParser mAMEArcadeParser = new classes.parsers.MAMEParser();
        SignatureParser mameSigType = mAMEArcadeParser.GetXmlType(XmlDoc);
        if (mameSigType != SignatureParser.Unknown) {
            Debug.WriteLine(mameSigType.ToString() + ": " + PathToFile);
            return mameSigType;
        }

        // check if NoIntro
        classes.parsers.NoIntrosParser noIntroParser = new classes.parsers.NoIntrosParser();
        if (noIntroParser.GetXmlType(XmlDoc) == SignatureParser.NoIntro) {
            Debug.WriteLine("No-Intro: " + PathToFile);
            return SignatureParser.NoIntro;
        }

        // unable to determine type
        return SignatureParser.Unknown;
    }

    public enum SignatureParser {
        Auto = 0,
        TOSEC = 1,
        MAMEArcade = 2,
        MAMEMess = 3,
        NoIntro = 4,
        Unknown = 100
    }

    public static Dictionary<string, object> ConvertXmlNodeToDictionary(XmlNode node)
    {
        Dictionary<string, object> map = new Dictionary<string, object>();
        
        // get node attributes first
        foreach (XmlAttribute attribute in node.Attributes)
        {
            map.Add(attribute.Name, attribute.Value);
        }

        // get children
        foreach (XmlNode xmlNode in node.ChildNodes) {
            map.Add(xmlNode.Name, ConvertXmlNodeToDictionary(xmlNode));
        }

        return map;
    }
}
