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
    public class PleasuredomeParser : BaseParser
    {
        public PleasuredomeParser()
        {

        }

        public override RomSignatureObject Parse(string XMLFile, Dictionary<string, object>? options = null)
        {
            // load dat file
            XmlDocument xmlDocument = InitializeFromFile(XMLFile, out string md5Hash, out string sha1Hash);

            // create object
            RomSignatureObject signatureObject = new RomSignatureObject();

            // read header
            XmlNode xmlHeader = xmlDocument.DocumentElement.SelectSingleNode("/datafile/header");
            ParseHeader(signatureObject, xmlHeader, "Pleasuredome", md5Hash, sha1Hash);

            // get machines
            signatureObject.Games = new List<RomSignatureObject.Game>();
            XmlNodeList xmlMachines = xmlDocument.DocumentElement.SelectNodes("/datafile/machine");
            foreach (XmlNode xmlMachine in xmlMachines)
            {
                RomSignatureObject.Game machineObject = CreateGameObject();
                machineObject.System = signatureObject.Name;
                machineObject.Name = xmlMachine.Attributes["name"].Value;
                machineObject.Description = xmlMachine.Attributes["name"].Value;

                if (xmlMachine.Attributes["sourcefile"] != null)
                {
                    if (!machineObject.flags.ContainsKey("sourcefile"))
                    {
                        machineObject.flags.Add("sourcefile", xmlMachine.Attributes["sourcefile"].Value);
                    }
                }

                foreach (XmlNode childNode in xmlMachine.ChildNodes)
                {
                    switch (childNode.Name.ToLower())
                    {
                        case "description":
                            if (childNode.InnerText.Contains("("))
                            {
                                string[] nameParts = childNode.InnerText.Split(new string[] { " (" }, StringSplitOptions.None);
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

                        case "disk":
                            RomSignatureObject.Game.Rom rom = ParseRomAttributes(childNode, RomSignatureObject.Game.Rom.SignatureSourceType.Pleasuredome);
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

                // search for existing gameObject to update
                bool existingGameFound = false;
                foreach (RomSignatureObject.Game existingGame in signatureObject.Games)
                {
                    if (existingGame.SortingName == machineObject.SortingName &&
                        existingGame.Year == machineObject.Year &&
                        existingGame.Publisher == machineObject.Publisher // &&
                                                                          // existingGame.Country == machineObject.Country &&
                                                                          // existingGame.Language == machineObject.Language
                        )
                    {
                        existingGame.Roms.AddRange(machineObject.Roms);
                        existingGameFound = true;
                        break;
                    }
                }
                if (existingGameFound == false)
                {
                    signatureObject.Games.Add(machineObject);
                }
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

            XmlNodeList xmlMachine = xml.DocumentElement.SelectNodes("/datafile/machine");

            if (xmlMachine == null)
            {
                return parser.SignatureParser.Unknown;
            }

            XmlNode authorNode = xmlHeader.SelectSingleNode("author");
            if (authorNode != null && authorNode.InnerText.ToLowerInvariant().Contains("pleasuredome"))
            {
                return parser.SignatureParser.Pleasuredome;
            }

            return parser.SignatureParser.Unknown;
        }
    }
}