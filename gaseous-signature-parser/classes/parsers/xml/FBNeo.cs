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
    public class FBNeoParser : BaseParser
    {
        public FBNeoParser()
        {

        }

        public override RomSignatureObject Parse(string XMLFile, Dictionary<string, object>? options = null)
        {
            // load dat file
            XmlDocument xmlDocument = InitializeFromFile(XMLFile, out string md5Hash, out string sha1Hash);

            RomSignatureObject.Game.Rom.SignatureSourceType signatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.FBNeo;

            // create object
            RomSignatureObject signatureObject = new RomSignatureObject();

            // read header
            XmlNode xmlHeader = xmlDocument.DocumentElement.SelectSingleNode("/datafile/header");
            ParseHeader(signatureObject, xmlHeader, signatureSource.ToString(), md5Hash, sha1Hash);

            // get machines
            signatureObject.Games = new List<RomSignatureObject.Game>();
            XmlNodeList xmlMachines = xmlDocument.DocumentElement.SelectNodes("/datafile/game");
            foreach (XmlNode xmlMachine in xmlMachines)
            {
                RomSignatureObject.Game machineObject = CreateGameObject();
                string systemName = signatureObject.Name;
                if (signatureObject.Name.Contains(" - "))
                {
                    string[] sigNameParts = signatureObject.Name.Split(" - ");
                    if (sigNameParts.Length > 1)
                    {
                        systemName = sigNameParts[1];
                    }
                }
                // strip " Games" from the end of systemName
                if (systemName.EndsWith(" Games"))
                {
                    systemName = systemName.Substring(0, systemName.Length - 6);
                }
                machineObject.System = systemName.Trim();

                machineObject.Name = xmlMachine.Attributes["name"].Value;
                machineObject.Description = xmlMachine.Attributes["name"].Value;

                if (machineObject.Name == "4pak")
                {
                    int debug = 1;
                }

                if (xmlMachine.Attributes["sourcefile"] != null)
                {
                    AddGameFlag(machineObject, "sourcefile", xmlMachine.Attributes["sourcefile"].Value);
                }

                foreach (XmlNode childNode in xmlMachine.ChildNodes)
                {
                    switch (childNode.Name.ToLower())
                    {
                        case "description":
                            // parse the name, details in parenthesis contains the language
                            if (childNode.InnerText.Contains("("))
                            {
                                string[] nameParts = childNode.InnerText.Split(new string[] { "(" }, StringSplitOptions.None);
                                machineObject.Name = nameParts[0].Trim();
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

                            machineObject.Description = childNode.InnerText;
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
            try
            {
                XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

                if (xmlHeader != null)
                {
                    if (xmlHeader.SelectSingleNode("author").InnerText.StartsWith("FinalBurn Neo"))
                    {
                        return parser.SignatureParser.FBNeo;
                    }
                }

                return parser.SignatureParser.Unknown;
            }
            catch
            {
                return parser.SignatureParser.Unknown;
            }
        }
    }
}