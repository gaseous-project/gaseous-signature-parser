using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
	public class NoIntrosParser
	{
        public NoIntrosParser() {

        }

		public RomSignatureObject Parse(string XMLFile)
		{
            // get hashes of NoIntros file
            var xmlStream = File.OpenRead(XMLFile);

            var md5 = MD5.Create();
            byte[] md5HashByte = md5.ComputeHash(xmlStream);
            string md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();

            var sha1 = SHA1.Create();
            byte[] sha1HashByte = sha1.ComputeHash(xmlStream);
            string sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();

            // load NoIntros file
            XmlDocument noIntroXmlDoc = new XmlDocument();
            noIntroXmlDoc.Load(XMLFile);

            RomSignatureObject noIntrosObject = new RomSignatureObject();

            // get header
            XmlNode xmlHeader = noIntroXmlDoc.DocumentElement.SelectSingleNode("/datafile/header");
            noIntrosObject.SourceType = "No-Intro";
            noIntrosObject.SourceMd5 = md5Hash;
            noIntrosObject.SourceSHA1 = sha1Hash;
            string SystemName = "";
            foreach (XmlNode childNode in xmlHeader.ChildNodes)
            {
                switch (childNode.Name.ToLower())
                {
                    case "id":
                        noIntrosObject.Id = childNode.InnerText;
                        break;

                    case "name":
                        noIntrosObject.Name = childNode.InnerText;
                        SystemName = noIntrosObject.Name;
                        break;

                    case "description":
                        noIntrosObject.Description = childNode.InnerText;
                        break;

                    case "category":
                        noIntrosObject.Category = childNode.InnerText;
                        break;

                    case "version":
                        noIntrosObject.Version = childNode.InnerText;
                        break;

                    case "author":
                        noIntrosObject.Author = childNode.InnerText;
                        break;

                    case "email":
                        noIntrosObject.Email = childNode.InnerText;
                        break;

                    case "homepage":
                        noIntrosObject.Homepage = childNode.InnerText;
                        break;

                    case "url":
                        try
                        {
                            noIntrosObject.Url = new Uri(childNode.InnerText);
                        }
                        catch
                        {
                            noIntrosObject.Url = null;
                        }
                        break;
                }
            }

            // check for NoIntros DB file
            string dbFileName = noIntrosObject.Name + " (DB Export) (" + noIntrosObject.Version + ").xml";
            string dbPathName = Path.Combine(Path.GetDirectoryName(XMLFile), dbFileName);

            XmlDocument? noIntroDbXmlDoc;
            if (File.Exists(dbPathName))
            {
                // load NoIntros file
                noIntroDbXmlDoc = new XmlDocument();
                noIntroDbXmlDoc.Load(dbPathName);
            }
            else
            {
                noIntroDbXmlDoc = null;
            }

            // get games
            noIntrosObject.Games = new List<RomSignatureObject.Game>();
            XmlNodeList xmlGames = noIntroXmlDoc.DocumentElement.SelectNodes("/datafile/game");
            foreach (XmlNode xmlGame in xmlGames)
            {
                RomSignatureObject.Game gameObject = new RomSignatureObject.Game();

                gameObject.Id = xmlGame.Attributes["id"].Value;

                gameObject.System = SystemName;

                // parse game name
                string[] gameNameTitleParts = xmlGame.Attributes["name"].Value.Split("(");
                string gameName = gameNameTitleParts[0];

                string[] gameNameTokens = gameName.Split("(");
                // game title should be first item
                gameObject.Name = gameNameTokens[0].Trim();

                gameObject.Roms = new List<RomSignatureObject.Game.Rom>();

                // get the roms
                foreach (XmlNode xmlGameDetail in xmlGame.ChildNodes)
                {
                    switch (xmlGameDetail.Name.ToLower())
                    {
                        case "category":
                            gameObject.Category = xmlGameDetail.InnerText;
                            break;

                        case "description":
                            gameObject.Description = xmlGameDetail.InnerText;
                            break;

                        case "rom":
                            RomSignatureObject.Game.Rom romObject = new RomSignatureObject.Game.Rom();
                            romObject.Attributes = new List<KeyValuePair<string, object>>();
                            if (xmlGameDetail != null)
                            {
                                romObject.Name = xmlGameDetail.Attributes["name"]?.Value;
                                if (xmlGameDetail.Attributes["size"]?.Value != null)
                                {
                                    romObject.Size = UInt64.Parse(xmlGameDetail.Attributes["size"]?.Value);
                                }
                                else
                                {
                                    romObject.Size = 0;
                                }
                                romObject.Crc = xmlGameDetail.Attributes["crc"]?.Value;
                                romObject.Md5 = xmlGameDetail.Attributes["md5"]?.Value;
                                romObject.Sha1 = xmlGameDetail.Attributes["sha1"]?.Value;
                                romObject.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.NoIntros;

                                // check the db file if present for this md5 or sha1
                                if (noIntroDbXmlDoc != null)
                                {
                                    RomSignatureObject.Game dbGame = SearchDB(noIntroDbXmlDoc, romObject.Md5, romObject.Sha1);
                                }
                            }

                            gameObject.Roms.Add(romObject);
                            break;
                    }
                }

                // search for existing gameObject to update
                bool existingGameFound = false;
                foreach (RomSignatureObject.Game existingGame in noIntrosObject.Games)
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
                    noIntrosObject.Games.Add(gameObject);
                }
            }

            return noIntrosObject;
        }

        private RomSignatureObject.Game SearchDB(XmlDocument dbXml, string? md5, string? sha1)
        {
            XmlNodeList xmlGames = dbXml.DocumentElement.SelectNodes("/datafile/game");
            RomSignatureObject.Game game = new RomSignatureObject.Game();
            game.Roms = new List<RomSignatureObject.Game.Rom>();

            // search for a game with a file with a matching md5 and/or sha1
            foreach (XmlNode xmlGame in xmlGames)
            {
                switch (xmlGame.Name.ToLower())
                {
                    case "game":
                        bool NameOverwritten = false;
                        foreach (XmlAttribute gameAttr in xmlGame.Attributes)
                        {
                            switch (gameAttr.Name.ToLower())
                            {
                                case "name":
                                    game.Name = gameAttr.Value;
                                    NameOverwritten = true;
                                    break;
                            }
                        }

                        string MediaData = "";
                        foreach (XmlNode xmlGameItem in xmlGame.ChildNodes)
                        {
                            switch (xmlGameItem.Name.ToLower())
                            {
                                case "archive":
                                    // provides details about the game
                                    foreach (XmlAttribute attribute in xmlGameItem.Attributes)
                                    {
                                        switch (attribute.Name.ToLower())
                                        {
                                            case "number":
                                                game.Id = attribute.Value;
                                                break;

                                            case "gameid1":
                                                game.GameId = attribute.Value;
                                                break;

                                            case "name":
                                                game.Name = attribute.Value;
                                                break;

                                            case "region":
                                                game.Country = attribute.Value;
                                                break;

                                            case "languages":
                                                game.Language = attribute.Value;
                                                break;

                                            case "categories":
                                                game.Category = attribute.Value;
                                                break;

                                            case "additional":
                                                MediaData = attribute.Value;
                                                break;

                                        }
                                    }
                                    break;
                                case "source":
                                    // provides details about the roms that belong to this game
                                    RomSignatureObject.Game.Rom rom = new RomSignatureObject.Game.Rom();
                                    rom.RomTypeMedia = MediaData;
                                    rom.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.NoIntros;

                                    foreach (XmlNode xmlSource in xmlGameItem.ChildNodes)
                                    {
                                        switch (xmlSource.Name.ToLower())
                                        {
                                            case "details":
                                                break;

                                            case "serials":
                                                break;

                                            case "file":
                                                foreach (XmlAttribute fileAttribute in xmlSource.Attributes)
                                                {
                                                    switch (fileAttribute.Name.ToLower())
                                                    {
                                                        case "id":
                                                            rom.Id = fileAttribute.Value;
                                                            break;

                                                        case "extension":
                                                            if (NameOverwritten == true && MediaData.Length > 0)
                                                            {
                                                                rom.Name = game.Name + " (" + MediaData + ")." + fileAttribute.Value;
                                                            }
                                                            else
                                                            {
                                                                rom.Name = game.Name + "." + fileAttribute.Value;
                                                            }
                                                            break;

                                                        case "size":
                                                            rom.Size = ulong.Parse(fileAttribute.Value);
                                                            break;

                                                        case "crc32":
                                                            rom.Crc = fileAttribute.Value;
                                                            break;

                                                        case "md5":
                                                            rom.Md5 = fileAttribute.Value;
                                                            break;

                                                        case "sha1":
                                                            rom.Sha1 = fileAttribute.Value;
                                                            break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    
                                    if (rom.Md5 == md5 || rom.Sha1 == sha1)
                                    {
                                        game.Roms.Add(rom);
                                        return game;
                                    }

                                    break;
                            }
                        }
                        break;
                }
            }

            return null;
        }

        public parser.SignatureParser GetXmlType(XmlDocument xml) {
            try
            {
                XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

                if (xmlHeader != null) {
                    if (xmlHeader.SelectSingleNode("homepage").InnerText.Equals("No-Intro", StringComparison.OrdinalIgnoreCase)) {
                        return parser.SignatureParser.NoIntro;
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

