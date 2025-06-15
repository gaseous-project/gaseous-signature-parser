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
    public class FBNeoParser
    {
        public FBNeoParser()
        {

        }

        public RomSignatureObject Parse(string XMLFile)
        {
            // get hashes of provided file
            var xmlStream = File.OpenRead(XMLFile);

            var md5 = MD5.Create();
            byte[] md5HashByte = md5.ComputeHash(xmlStream);
            string md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();

            var sha1 = SHA1.Create();
            byte[] sha1HashByte = sha1.ComputeHash(xmlStream);
            string sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();

            // load dat file
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(XMLFile);

            RomSignatureObject.Game.Rom.SignatureSourceType signatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.FBNeo;

            // create object
            RomSignatureObject signatureObject = new RomSignatureObject();

            // read header
            XmlNode xmlHeader = xmlDocument.DocumentElement.SelectSingleNode("/datafile/header"); ;
            signatureObject.SourceType = signatureSource.ToString();
            signatureObject.SourceMd5 = md5Hash;
            signatureObject.SourceSHA1 = sha1Hash;
            foreach (XmlNode xmlNode in xmlHeader.ChildNodes)
            {
                switch (xmlNode.Name.ToLower())
                {
                    case "name":
                        signatureObject.Name = xmlNode.InnerText;
                        break;

                    case "description":
                        signatureObject.Description = xmlNode.InnerText;
                        break;

                    case "category":
                        signatureObject.Category = xmlNode.InnerText;
                        break;

                    case "version":
                        signatureObject.Version = xmlNode.InnerText;
                        break;

                    case "author":
                        signatureObject.Author = xmlNode.InnerText;
                        break;

                    case "email":
                        signatureObject.Email = xmlNode.InnerText;
                        break;

                    case "homepage":
                        signatureObject.Homepage = xmlNode.InnerText;
                        break;

                    case "url":
                        try
                        {
                            signatureObject.Url = new Uri(xmlNode.InnerText);
                        }
                        catch
                        {
                            signatureObject.Url = null;
                        }
                        break;
                }
            }

            // get machines
            signatureObject.Games = new List<RomSignatureObject.Game>();
            XmlNodeList xmlMachines = xmlDocument.DocumentElement.SelectNodes("/datafile/game");
            foreach (XmlNode xmlMachine in xmlMachines)
            {
                RomSignatureObject.Game machineObject = new RomSignatureObject.Game();
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

                machineObject.Roms = new List<RomSignatureObject.Game.Rom>();
                machineObject.flags = new Dictionary<string, object>();
                machineObject.Language = new Dictionary<string, string>();
                machineObject.Country = new Dictionary<string, string>();

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
                            // parse the name, details in parenthesis contains the language
                            if (childNode.InnerText.Contains("("))
                            {
                                string[] nameParts = childNode.InnerText.Split(new string[] { " (" }, StringSplitOptions.None);
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
                            RomSignatureObject.Game.Rom rom = new RomSignatureObject.Game.Rom();
                            rom.Attributes = new Dictionary<string, object>();
                            rom.SignatureSource = signatureSource;
                            foreach (XmlAttribute romAttribute in childNode.Attributes)
                            {
                                switch (romAttribute.Name.ToLower())
                                {
                                    case "name":
                                        rom.Name = childNode.Attributes[romAttribute.Name].Value;
                                        break;
                                    case "size":
                                        rom.Size = UInt64.Parse(childNode.Attributes[romAttribute.Name]?.Value);
                                        break;
                                    case "crc":
                                        rom.Crc = childNode.Attributes[romAttribute.Name]?.Value;
                                        break;
                                    case "md5":
                                        rom.Md5 = childNode.Attributes[romAttribute.Name]?.Value;
                                        break;
                                    case "sha1":
                                        rom.Sha1 = childNode.Attributes[romAttribute.Name]?.Value;
                                        break;
                                    case "sha256":
                                        rom.Sha256 = childNode.Attributes[romAttribute.Name]?.Value;
                                        break;
                                    case "status":
                                        rom.Status = childNode.Attributes[romAttribute.Name]?.Value;
                                        break;

                                    default:
                                        if (!rom.Attributes.ContainsKey(romAttribute.Name))
                                        {
                                            rom.Attributes.Add(romAttribute.Name, childNode.Attributes[romAttribute.Name]?.Value);
                                        }
                                        break;
                                }
                            }
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

        public parser.SignatureParser GetXmlType(XmlDocument xml)
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