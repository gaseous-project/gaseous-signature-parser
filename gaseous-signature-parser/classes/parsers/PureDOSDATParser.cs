using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    public class PureDOSDATParser
    {
        public PureDOSDATParser()
        {

        }

        public RomSignatureObject Parse(string XMLFile)
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

                // Compute MD5 and SHA1 hashes
                using (var md5 = MD5.Create())
                {
                    byte[] md5HashByte = md5.ComputeHash(xmlStream);
                    md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();
                }

                // Reset the stream position to the beginning before computing the SHA1 hash
                xmlStream.Position = 0;

                using (var sha1 = SHA1.Create())
                {
                    byte[] sha1HashByte = sha1.ComputeHash(xmlStream);
                    sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();
                }

                // Reset the stream position to the beginning for XML loading
                xmlStream.Position = 0;

                // load PureDOSDAT file
                XmlDocument pureDosDatXmlDoc = new XmlDocument();
                pureDosDatXmlDoc.Load(xmlStream);

                RomSignatureObject pureDosDatObject = new RomSignatureObject();

                // get header
                XmlNode xmlHeader = pureDosDatXmlDoc.DocumentElement.SelectSingleNode("/datafile/header");
                pureDosDatObject.SourceType = "PureDOSDAT";
                pureDosDatObject.SourceMd5 = md5Hash;
                pureDosDatObject.SourceSHA1 = sha1Hash;
                foreach (XmlNode childNode in xmlHeader.ChildNodes)
                {
                    switch (childNode.Name.ToLower())
                    {
                        case "name":
                            pureDosDatObject.Name = childNode.InnerText;
                            break;

                        case "description":
                            pureDosDatObject.Description = childNode.InnerText;
                            break;

                        case "category":
                            pureDosDatObject.Category = childNode.InnerText;
                            break;

                        case "version":
                            pureDosDatObject.Version = childNode.InnerText;
                            break;

                        case "author":
                            pureDosDatObject.Author = childNode.InnerText;
                            break;

                        case "email":
                            pureDosDatObject.Email = childNode.InnerText;
                            break;

                        case "homepage":
                            pureDosDatObject.Homepage = childNode.InnerText;
                            break;

                        case "url":
                            try
                            {
                                string uriString = childNode.InnerText;
                                if (uriString.StartsWith("http://") || uriString.StartsWith("https://"))
                                {
                                    pureDosDatObject.Url = new Uri(uriString);
                                }
                                else
                                {
                                    pureDosDatObject.Url = new Uri("http://" + uriString);
                                }
                            }
                            catch
                            {
                                pureDosDatObject.Url = null;
                            }
                            break;
                    }
                }

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
                                                romObject.Name = attribute.Value;
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
                        if (existingGame.Name == gameObject.Name &&
                            existingGame.Year == gameObject.Year &&
                            existingGame.Publisher == gameObject.Publisher &&
                            existingGame.Country == gameObject.Country &&
                            existingGame.Language == gameObject.Language)
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

        public parser.SignatureParser GetXmlType(XmlDocument xml)
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