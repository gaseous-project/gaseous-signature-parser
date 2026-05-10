using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    public class PureDOSDATParser : BaseParser
    {
        public PureDOSDATParser()
        {

        }

        public override RomSignatureObject Parse(string XMLFile, Dictionary<string, object>? options = null)
        {
            // get hashes of PureDOSDAT file
            string md5Hash = string.Empty;
            string sha1Hash = string.Empty;
            using (var xmlStream = File.OpenRead(XMLFile))
            {
                if (xmlStream.Length == 0)
                {
                    throw new ArgumentException("The XML file is empty.", nameof(XMLFile));
                }

                // get hashes of the XML file
                var hashes = Hash.GenerateHashes(xmlStream);
                md5Hash = hashes.md5;
                sha1Hash = hashes.sha1;

                // load PureDOSDAT file
                XmlDocument pureDosDatXmlDoc = new XmlDocument();
                pureDosDatXmlDoc.Load(xmlStream);

                RomSignatureObject pureDosDatObject = new RomSignatureObject();

                // get header
                XmlNode xmlHeader = pureDosDatXmlDoc.DocumentElement.SelectSingleNode("/datafile/header");
                ParseHeader(pureDosDatObject, xmlHeader, "PureDOSDAT", md5Hash, sha1Hash);

                // get games
                pureDosDatObject.Games = new List<RomSignatureObject.Game>();
                XmlNodeList xmlGames = pureDosDatXmlDoc.DocumentElement.SelectNodes("/datafile/game");
                foreach (XmlNode xmlGame in xmlGames)
                {
                    RomSignatureObject.Game gameObject = new RomSignatureObject.Game();

                    // parse game name
                    string[] gameNameTitleParts = xmlGame.Attributes["name"].Value.Split("[");
                    string gameName = gameNameTitleParts[0];
                    string[] gameNameTokens = gameName.Split("(");
                    // game title should be first item
                    gameObject.Name = gameNameTokens[0].Trim();

                    gameObject.System = "DOS";

                    gameObject.Roms = new List<RomSignatureObject.Game.Rom>();

                    // get the roms
                    foreach (XmlNode xmlGameDetail in xmlGame.ChildNodes)
                    {
                        switch (xmlGameDetail.Name.ToLower())
                        {
                            case "description":
                                gameObject.Description = xmlGameDetail.InnerText;
                                break;

                            case "year":
                                gameObject.Year = xmlGameDetail.InnerText;
                                break;

                            case "developer":
                                gameObject.Publisher = xmlGameDetail.InnerText;
                                break;

                            case "link":
                                // gameObject.flags["link"] is a dictionary
                                if (!gameObject.flags.ContainsKey("link"))
                                {
                                    gameObject.flags.Add("link", new List<Dictionary<string, string>>());
                                }

                                // add the link to the dictionary
                                Dictionary<string, string> link = new Dictionary<string, string>();
                                foreach (XmlAttribute attribute in xmlGameDetail.Attributes)
                                {
                                    link.Add(attribute.Name, attribute.Value);
                                }
                                link.Add("url", xmlGameDetail.InnerText);
                                ((List<Dictionary<string, string>>)gameObject.flags["link"]).Add(link);

                                break;

                            case "comment":
                            case "comment_dosc":
                                if (!gameObject.flags.ContainsKey(xmlGameDetail.Name.ToLower()))
                                {
                                    gameObject.flags.Add(xmlGameDetail.Name.ToLower(), xmlGameDetail.InnerText);
                                }
                                break;

                            case "parent":
                                if (!gameObject.flags.ContainsKey("parent"))
                                {
                                    gameObject.flags.Add("parent", xmlGameDetail.InnerText);
                                }
                                break;

                            case "variant":
                                if (!gameObject.flags.ContainsKey("variant"))
                                {
                                    gameObject.flags.Add("variant", xmlGameDetail.InnerText);
                                }
                                break;

                            case "rom":
                                RomSignatureObject.Game.Rom romObject = new RomSignatureObject.Game.Rom();
                                romObject.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.PureDOSDAT;
                                romObject.Attributes = new Dictionary<string, object>();
                                if (xmlGameDetail != null)
                                {
                                    foreach (XmlAttribute attribute in xmlGameDetail.Attributes)
                                    {
                                        switch (attribute.Name.ToLower())
                                        {
                                            case "name":
                                                if (attribute.Value.Contains("Alone in the Dark 3"))
                                                {
                                                    int debug = 1;
                                                }

                                                romObject.Name = attribute.Value;
                                                RomSignatureObject.Game.Rom romData = ParseRomAttributes(xmlGameDetail, RomSignatureObject.Game.Rom.SignatureSourceType.PureDOSDAT);
                                                romObject.Country = romData.Country;
                                                romObject.Language = romData.Language;

                                                // bubble up country and language to gameObject flags
                                                if (romObject.Country != null)
                                                {
                                                    if (gameObject.Country == null)
                                                    {
                                                        gameObject.Country = new Dictionary<string, string>();
                                                    }
                                                    foreach (KeyValuePair<string, string> country in romObject.Country)
                                                    {
                                                        if (!gameObject.Country.ContainsKey(country.Key))
                                                        {
                                                            gameObject.Country.Add(country.Key, country.Value);
                                                        }
                                                    }
                                                }
                                                if (romObject.Language != null)
                                                {
                                                    if (gameObject.Language == null)
                                                    {
                                                        gameObject.Language = new Dictionary<string, string>();
                                                    }
                                                    foreach (KeyValuePair<string, string> language in romObject.Language)
                                                    {
                                                        if (!gameObject.Language.ContainsKey(language.Key))
                                                        {
                                                            gameObject.Language.Add(language.Key, language.Value);
                                                        }
                                                    }
                                                }

                                                break;

                                            case "size":
                                                if (UInt64.TryParse(attribute.Value, out ulong size))
                                                {
                                                    romObject.Size = size;
                                                }
                                                else
                                                {
                                                    romObject.Size = null;
                                                }
                                                break;

                                            case "crc":
                                                romObject.Crc = attribute.Value;
                                                break;

                                            case "md5":
                                                romObject.Md5 = attribute.Value.ToLowerInvariant();
                                                break;

                                            case "sha1":
                                                romObject.Sha1 = attribute.Value.ToLowerInvariant();
                                                break;

                                            case "sha256":
                                                romObject.Sha256 = attribute.Value.ToLowerInvariant();
                                                break;

                                            default:
                                                // add to attributes dictionary
                                                if (!romObject.Attributes.ContainsKey(attribute.Name))
                                                {
                                                    romObject.Attributes.Add(attribute.Name, attribute.Value);
                                                }
                                                break;
                                        }
                                    }
                                }

                                gameObject.Roms.Add(romObject);
                                break;
                        }
                    }

                    // search for existing gameObject to update
                    bool existingGameFound = false;
                    foreach (RomSignatureObject.Game existingGame in pureDosDatObject.Games)
                    {
                        if (existingGame.SortingName == gameObject.SortingName &&
                            existingGame.Year == gameObject.Year &&
                            existingGame.Publisher == gameObject.Publisher) // &&
                        // existingGame.Country == gameObject.Country &&
                        // existingGame.Language == gameObject.Language)
                        {
                            existingGame.Roms.AddRange(gameObject.Roms);
                            existingGameFound = true;
                            break;
                        }
                    }
                    if (existingGameFound == false)
                    {
                        pureDosDatObject.Games.Add(gameObject);
                    }
                }

                return pureDosDatObject;
            }
        }

        public override parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

            if (xmlHeader == null)
            {
                return parser.SignatureParser.Unknown;
            }

            var nodeHomepage = xmlHeader.SelectSingleNode("homepage");

            if (nodeHomepage == null)
            {
                return parser.SignatureParser.Unknown;
            }

            if (nodeHomepage.InnerText.Equals("Pure DOS DAT", StringComparison.OrdinalIgnoreCase))
            {
                return parser.SignatureParser.PureDOSDAT;
            }

            return parser.SignatureParser.Unknown;
        }
    }
}