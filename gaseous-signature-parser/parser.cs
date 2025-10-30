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

        switch (DetectedSignatureType)
        {
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

            case SignatureParser.Redump:
                classes.parsers.RedumpParser redumpParser = new classes.parsers.RedumpParser();

                return redumpParser.Parse(PathToFile);

            case SignatureParser.WHDLoad:
                classes.parsers.WHDLoadParser whdloadParser = new classes.parsers.WHDLoadParser();

                return whdloadParser.Parse(PathToFile);

            case SignatureParser.RetroAchievements:
                classes.parsers.RetroAchievementsParser retroAchievementsParser = new classes.parsers.RetroAchievementsParser();

                return retroAchievementsParser.Parse(PathToFile);

            case SignatureParser.FBNeo:
                classes.parsers.FBNeoParser fbNeoParser = new classes.parsers.FBNeoParser();

                return fbNeoParser.Parse(PathToFile);

            case SignatureParser.PureDOSDAT:
                classes.parsers.PureDOSDATParser pureDOSDATParser = new classes.parsers.PureDOSDATParser();

                return pureDOSDATParser.Parse(PathToFile);

            case SignatureParser.Pleasuredome:
                classes.parsers.PleasuredomeParser pleasuredomeParser = new classes.parsers.PleasuredomeParser();

                return pleasuredomeParser.Parse(PathToFile);

            case SignatureParser.Generic:
                classes.parsers.GenericParser genericParser = new classes.parsers.GenericParser();

                return genericParser.Parse(PathToFile);

            case SignatureParser.Unknown:
            default:
                throw new Exception("Unknown parser type");

        }
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

        // check if TOSEC
        classes.parsers.TosecParser tosecParser = new classes.parsers.TosecParser();
        if (tosecParser.GetXmlType(XmlDoc) == SignatureParser.TOSEC)
        {
            Debug.WriteLine("TOSEC: " + PathToFile);
            return SignatureParser.TOSEC;
        }

        // check if MAMEArcade
        classes.parsers.MAMEParser mAMEArcadeParser = new classes.parsers.MAMEParser();
        SignatureParser mameSigType = mAMEArcadeParser.GetXmlType(XmlDoc);
        if (mameSigType != SignatureParser.Unknown)
        {
            Debug.WriteLine(mameSigType.ToString() + ": " + PathToFile);
            return mameSigType;
        }

        // check if NoIntro
        classes.parsers.NoIntrosParser noIntroParser = new classes.parsers.NoIntrosParser();
        if (noIntroParser.GetXmlType(XmlDoc) == SignatureParser.NoIntro)
        {
            Debug.WriteLine("No-Intro: " + PathToFile);
            return SignatureParser.NoIntro;
        }

        // check if Redump
        classes.parsers.RedumpParser redumpParser = new classes.parsers.RedumpParser();
        if (redumpParser.GetXmlType(XmlDoc) == SignatureParser.Redump)
        {
            Debug.WriteLine("Redump: " + PathToFile);
            return SignatureParser.Redump;
        }

        // check if WHDLoad
        classes.parsers.WHDLoadParser whdloadParser = new classes.parsers.WHDLoadParser();
        if (whdloadParser.GetXmlType(XmlDoc) == SignatureParser.WHDLoad)
        {
            Debug.WriteLine("WHDLoad: " + PathToFile);
            return SignatureParser.WHDLoad;
        }

        // check if RetroAchievements
        classes.parsers.RetroAchievementsParser retroAchievementsParser = new classes.parsers.RetroAchievementsParser();
        if (retroAchievementsParser.GetXmlType(XmlDoc) == SignatureParser.RetroAchievements)
        {
            Debug.WriteLine("RetroAchievements: " + PathToFile);
            return SignatureParser.RetroAchievements;
        }

        // check if FBNeo
        classes.parsers.FBNeoParser fbNeoParser = new classes.parsers.FBNeoParser();
        if (fbNeoParser.GetXmlType(XmlDoc) == SignatureParser.FBNeo)
        {
            Debug.WriteLine("FBNeo: " + PathToFile);
            return SignatureParser.FBNeo;
        }

        // check if PureDOS DAT
        classes.parsers.PureDOSDATParser pureDOSDATParser = new classes.parsers.PureDOSDATParser();
        if (pureDOSDATParser.GetXmlType(XmlDoc) == SignatureParser.PureDOSDAT)
        {
            Debug.WriteLine("PureDOS DAT: " + PathToFile);
            return SignatureParser.PureDOSDAT;
        }

        // check if Pleasuredome
        classes.parsers.PleasuredomeParser pleasuredomeParser = new classes.parsers.PleasuredomeParser();
        if (pleasuredomeParser.GetXmlType(XmlDoc) == SignatureParser.Pleasuredome)
        {
            Debug.WriteLine("Pleasuredome: " + PathToFile);
            return SignatureParser.Pleasuredome;
        }

        // check if MAMERedump
        classes.parsers.MAMERedumpParser mAMERedumpParser = new classes.parsers.MAMERedumpParser();
        if (mAMERedumpParser.GetXmlType(XmlDoc) == SignatureParser.MAMERedump)
        {
            Debug.WriteLine("MAMERedump: " + PathToFile);
            return SignatureParser.MAMERedump;
        }

        // check if Generic
        classes.parsers.GenericParser genericParser = new classes.parsers.GenericParser();
        if (genericParser.GetXmlType(XmlDoc) == SignatureParser.Generic)
        {
            Debug.WriteLine("Generic: " + PathToFile);
            return SignatureParser.Generic;
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
