using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    public class WHDLoadParser
    {
        public WHDLoadParser()
        {

        }

        public RomSignatureObject Parse(string XMLFile)
        {
            // get hashes of WHDLoad file
            var xmlStream = File.OpenRead(XMLFile);

            var md5 = MD5.Create();
            byte[] md5HashByte = md5.ComputeHash(xmlStream);
            string md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();

            var sha1 = SHA1.Create();
            byte[] sha1HashByte = sha1.ComputeHash(xmlStream);
            string sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();

            // load WHDLoad file
            XmlDocument whdloadXmlDoc = new XmlDocument();
            whdloadXmlDoc.Load(XMLFile);

            RomSignatureObject whdloadObject = new RomSignatureObject();

            whdloadObject.Name = "WHDLoad";
            whdloadObject.SourceType = "WHDLoad";
            whdloadObject.SourceMd5 = md5Hash;
            whdloadObject.SourceSHA1 = sha1Hash;

            whdloadObject.Games = new List<RomSignatureObject.Game>();

            Dictionary<string, RomSignatureObject.Game> games = new Dictionary<string, RomSignatureObject.Game>();

            XmlNodeList xmlGame = whdloadXmlDoc.DocumentElement.SelectNodes("/whdbooter/game");
            if (xmlGame != null)
            {
                // loop each game node
                foreach (XmlNode childGameNode in xmlGame)
                {
                    // get child node "name" and check whdloadObject.Games for existing game
                    string gameName = childGameNode.SelectSingleNode("name").InnerText;
                    RomSignatureObject.Game game;
                    if (games.ContainsKey(gameName))
                    {
                        game = games[gameName];
                    }
                    else
                    {
                        // if game doesn't exist, create it
                        game = new RomSignatureObject.Game();
                        game.Name = gameName;
                        game.Id = childGameNode.SelectSingleNode("subpath").InnerText;
                        game.System = "Commodore Amiga";
                        game.Roms = new List<RomSignatureObject.Game.Rom>();
                    }

                    // create rom object
                    RomSignatureObject.Game.Rom rom = new RomSignatureObject.Game.Rom();
                    rom.Name = childGameNode.Attributes["filename"].Value;
                    rom.Sha1 = childGameNode.Attributes["sha1"].Value;
                    rom.Md5 = childGameNode.Attributes["md5"]?.Value;
                    rom.Sha256 = childGameNode.Attributes["sha256"]?.Value;
                    rom.Status = childGameNode.Attributes["status"]?.Value;
                    if (childGameNode.Attributes["size"] != null)
                    {
                        rom.Size = (ulong?)long.Parse(childGameNode.Attributes["size"]?.Value);
                    }
                    else
                    {
                        rom.Size = null;
                    }
                    rom.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.WHDLoad;

                    // add remaining child nodes to rom.attributes dictionary with the node names as keys
                    foreach (XmlNode romNode in childGameNode.ChildNodes)
                    {
                        if (
                            romNode.Name == "name" ||
                            romNode.Name == "subpath"
                            )
                        {
                            continue;
                        }

                        // if node has childnodes, build a dictionary of child nodes
                        string nodeName = romNode.Name;
                        foreach (XmlAttribute attribute in romNode.Attributes)
                        {
                            if (attribute.Name.ToLower() == "number")
                            {
                                nodeName = $"{romNode.Name}.{attribute.Value}";
                            }
                        }

                        // process attribute elements
                        foreach (XmlNode childNode in romNode.ChildNodes)
                        {
                            string childNodeName = $"{nodeName}";
                            if (childNode.GetType() == typeof(XmlElement))
                            {
                                childNodeName = $"{childNodeName}.{childNode.Name}";
                                rom.Attributes.Add(childNodeName, childNode.InnerText);
                            }
                            else
                            {
                                rom.Attributes.Add(childNodeName, childNode.InnerText);
                            }
                        }
                    }

                    // add rom to game
                    if (games.ContainsKey(gameName))
                    {
                        games[gameName].Roms.Add(rom);
                    }
                    else
                    {
                        game.Roms.Add(rom);
                        games.Add(gameName, game);
                    }
                }
            }

            // copy games from dictionary to whdloadObject
            foreach (KeyValuePair<string, RomSignatureObject.Game> game in games)
            {
                whdloadObject.Games.Add(game.Value);
            }

            return whdloadObject;
        }

        public parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            try
            {
                XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/whdbooter");

                if (xmlHeader != null)
                {
                    return parser.SignatureParser.WHDLoad;
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

