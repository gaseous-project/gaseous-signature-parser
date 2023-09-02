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
    public RomSignatureObject ParseSignatureDAT(string PathToFile, SignatureParser Parser = SignatureParser.Auto) {
        SignatureParser DetectedSignatureType = SignatureParser.Auto;
        if (Parser == SignatureParser.Auto) {
            DetectedSignatureType = GetSignatureType(PathToFile);
        } else {
            DetectedSignatureType = Parser;
        }

        switch (DetectedSignatureType) {
            case SignatureParser.TOSEC:
                classes.parsers.TosecParser tosecParser = new classes.parsers.TosecParser();

                return tosecParser.Parse(PathToFile);
                break;
            case SignatureParser.Unknown:
            default:
                throw new Exception("Unknown parser type");
                break;
        }
    }

    private SignatureParser GetSignatureType(string PathToFile) {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(PathToFile);

        // check if TOSEC
        classes.parsers.TosecParser tosecParser = new classes.parsers.TosecParser();
        if (tosecParser.IsTOSEC(XmlDoc)) {
            Debug.WriteLine("TOSEC: " + PathToFile);
            return SignatureParser.TOSEC;
        }

        // unable to determine type
        return SignatureParser.Unknown;
    }

    public enum SignatureParser {
        Auto = 0,
        TOSEC = 1,
        Unknown = 100
    }
}
