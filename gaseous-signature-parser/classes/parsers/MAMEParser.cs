using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;
using RestEase;
using System.Diagnostics;

namespace gaseous_signature_parser.classes.parsers
{
    public class MAMEParser : BaseParser
    {
        public MAMEParser()
        {

        }

        public override RomSignatureObject Parse(string XMLFile, Dictionary<string, object>? options = null)
        {
            // Extract DocumentType from options
            parser.SignatureParser DocumentType = parser.SignatureParser.Unknown;
            if (options != null && options.ContainsKey("DocumentType"))
            {
                DocumentType = (parser.SignatureParser)options["DocumentType"];
            }

            return ParseInternal(XMLFile, DocumentType);
        }

        private RomSignatureObject ParseInternal(string XMLFile, parser.SignatureParser DocumentType)
        {
            // load dat file
            XmlDocument xmlDocument = InitializeFromFile(XMLFile, out string md5Hash, out string sha1Hash);

            // get document type - need to know if it's MAME arcade, mess, etc
            RomSignatureObject.Game.Rom.SignatureSourceType signatureSource = new RomSignatureObject.Game.Rom.SignatureSourceType();
            switch (DocumentType)
            {
                case parser.SignatureParser.MAMEArcade:
                    signatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.MAMEArcade;
                    break;

                case parser.SignatureParser.MAMEMess:
                    signatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.MAMEMess;
                    break;
            }

            // create object
            RomSignatureObject signatureObject = new RomSignatureObject();

            // read header
            XmlNode xmlHeader = xmlDocument.DocumentElement.SelectSingleNode("/datafile/header");
            ParseHeader(signatureObject, xmlHeader, DocumentType.ToString(), md5Hash, sha1Hash);

            // get machines
            signatureObject.Games = new List<RomSignatureObject.Game>();
            XmlNodeList xmlMachines = xmlDocument.DocumentElement.SelectNodes("/datafile/machine");
            foreach (XmlNode xmlMachine in xmlMachines)
            {
                RomSignatureObject.Game machineObject = CreateGameObject();
                machineObject.System = "Arcade";
                machineObject.Name = xmlMachine.Attributes["name"].Value;
                machineObject.Description = xmlMachine.Attributes["name"].Value;

                if (xmlMachine.Attributes["sourcefile"] != null)
                {
                    AddGameFlag(machineObject, "sourcefile", xmlMachine.Attributes["sourcefile"].Value);
                }

                foreach (XmlNode childNode in xmlMachine.ChildNodes)
                {
                    switch (childNode.Name.ToLower())
                    {
                        case "description":
                            if (childNode.InnerText.Contains("("))
                            {
                                string[] nameParts = childNode.InnerText.Split(new string[] { "(" }, StringSplitOptions.None);
                                machineObject.Name = nameParts[0].Trim();
                                if (nameParts.Length == 1)
                                {
                                    break;
                                }
                                string titleDetails = nameParts[1].TrimEnd(')');

                                // split the details by comma
                                string[] detailsParts = titleDetails.Split(new string[] { "," }, StringSplitOptions.None);

                                // check if any of the details are in the country list
                                foreach (string detail in detailsParts)
                                {
                                    string trimmedDetail = detail.Trim();
                                    KeyValuePair<string, string>? countryItem = CountryLookup.ParseCountryString(trimmedDetail);
                                    if (countryItem != null)
                                    {
                                        if (countryItem.HasValue && !machineObject.Country.ContainsKey(countryItem.Value.Key))
                                        {
                                            machineObject.Country.Add(countryItem.Value.Key, countryItem.Value.Value);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                machineObject.Name = childNode.InnerText;
                            }
                            break;

                        case "year":
                            machineObject.Year = childNode.InnerText;
                            break;

                        case "manufacturer":
                            machineObject.Publisher = childNode.InnerText;
                            break;

                        case "rom":
                            RomSignatureObject.Game.Rom rom = ParseRomAttributes(childNode, signatureSource);
                            machineObject.Roms.Add(rom);
                            break;

                        default:
                            if (!machineObject.flags.ContainsKey(childNode.Name))
                            {
                                machineObject.flags.Add(childNode.Name, parser.ConvertXmlNodeToDictionary(childNode));
                            }
                            break;
                    }
                }
                signatureObject.Games.Add(machineObject);
            }

            return signatureObject;
        }

        public override parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

            if (xmlHeader == null)
            {
                return parser.SignatureParser.Unknown;
            }

            var nodeName = xmlHeader.SelectSingleNode("name");

            if (nodeName == null)
            {
                return parser.SignatureParser.Unknown;
            }

            var nodeDescription = xmlHeader.SelectSingleNode("description");

            if (nodeDescription == null)
            {
                return parser.SignatureParser.Unknown;
            }

            if (nodeName.InnerText.Equals("MAME", StringComparison.OrdinalIgnoreCase))
            {
                if (nodeDescription.InnerText.StartsWith("MAME Arcade"))
                {
                    return parser.SignatureParser.MAMEArcade;
                }

                if (nodeDescription.InnerText.StartsWith("MAME Home"))
                {
                    return parser.SignatureParser.MAMEMess;
                }
            }

            if (nodeName.InnerText.Equals("MESS", StringComparison.OrdinalIgnoreCase) && nodeDescription.InnerText.StartsWith("MAME Home"))
            {
                return parser.SignatureParser.MAMEMess;
            }

            return parser.SignatureParser.Unknown;
        }
    }
}