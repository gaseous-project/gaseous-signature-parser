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
        public NoIntrosParser()
        {

        }

        public RomSignatureObject Parse(string XMLFile, string? dbXMLFile)
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

            XmlDocument? noIntroDbXmlDoc;
            if (File.Exists(dbXMLFile))
            {
                // load NoIntros file
                noIntroDbXmlDoc = new XmlDocument();
                noIntroDbXmlDoc.Load(dbXMLFile);
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
                XmlAttribute idAttribute = xmlGame.Attributes["id"];
                if (idAttribute == null)
                {
                    continue;
                }

                if (long.TryParse(idAttribute.Value, out _) == true)
                {
                    gameObject.Id = long.Parse(idAttribute.Value).ToString();
                }
                else
                {
                    // string is not a valid int - convert each char to it's ascii code and make a new int from that
                    gameObject.Id = 0.ToString();
                    foreach (char c in xmlGame.Attributes["id"].Value)
                    {
                        gameObject.Id = (Convert.ToInt64(c) + Convert.ToInt64(gameObject.Id)).ToString();
                    }
                }

                XmlAttribute cloneIdAttribute = xmlGame.Attributes["cloneofid"];
                if (cloneIdAttribute != null)
                {
                    if (long.TryParse(cloneIdAttribute.Value, out _) == true)
                    {
                        gameObject.CloneOfId = long.Parse(cloneIdAttribute.Value).ToString();
                    }
                }

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

                        case "game_id":
                            gameObject.GameId = xmlGameDetail.InnerText;
                            break;

                        case "rom":
                            RomSignatureObject.Game.Rom romObject = new RomSignatureObject.Game.Rom();
                            romObject.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.NoIntros;

                            // set defaults
                            romObject.Attributes = new Dictionary<string, object>();
                            romObject.Size = 0;
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
                                            if (UInt64.TryParse(attribute.Value, out _) == true)
                                            {
                                                romObject.Size = UInt64.Parse(attribute.Value);
                                            }
                                            else
                                            {
                                                romObject.Size = 0;
                                            }
                                            break;

                                        case "crc":
                                            romObject.Crc = attribute.Value;
                                            break;

                                        case "md5":
                                            romObject.Md5 = attribute.Value;
                                            break;

                                        case "sha1":
                                            romObject.Sha1 = attribute.Value;
                                            break;

                                        case "sha256":
                                            romObject.Sha256 = attribute.Value;
                                            break;

                                        case "status":
                                            romObject.Status = attribute.Value;
                                            break;
                                    }
                                }


                                // check the db file if present for this md5 or sha1 or sha256
                                if (noIntroDbXmlDoc != null)
                                {
                                    RomSignatureObject.Game dbGame = SearchDB(noIntroDbXmlDoc, romObject.Md5, romObject.Sha1, romObject.Sha256);
                                    if (dbGame != null)
                                    {
                                        if (dbGame.Roms.Count > 0)
                                        {
                                            // a match was found in the db - copy the contents over the top of the currently built object
                                            gameObject.GameId = dbGame.GameId;
                                            gameObject.Category = dbGame.Category;
                                            gameObject.Name = dbGame.Name;
                                            gameObject.Country = dbGame.Country;
                                            gameObject.Language = dbGame.Language;
                                            gameObject.flags = dbGame.flags;

                                            romObject = dbGame.Roms[0];
                                        }
                                    }
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

        private RomSignatureObject.Game SearchDB(XmlDocument dbXml, string? md5, string? sha1, string? sha256)
        {
            Dictionary<string, string>? romCountryList = new Dictionary<string, string>();
            Dictionary<string, string>? romLanguageList = new Dictionary<string, string>();

            XmlNodeList xmlGames = dbXml.DocumentElement.SelectNodes($"/datafile/game[source/file[@md5='{md5}' or @sha1='{sha1}' or @sha256='{sha256}']]");
            RomSignatureObject.Game game = new RomSignatureObject.Game();
            game.Roms = new List<RomSignatureObject.Game.Rom>();

            // search for a game with a file with a matching md5 and/or sha1 and/or sha256
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
                                                string[] countries = attribute.Value.Split(",");
                                                if (game.Country == null)
                                                {
                                                    game.Country = new Dictionary<string, string>();
                                                }
                                                if (countries != null)
                                                {
                                                    foreach (string country in countries)
                                                    {
                                                        KeyValuePair<string, string>? countryItem = CountryLookup.ParseCountryString(country.Trim());
                                                        if (countryItem != null)
                                                        {
                                                            if (!game.Country.ContainsKey(countryItem.Value.Key))
                                                            {
                                                                game.Country.Add(countryItem.Value.Key, countryItem.Value.Value);
                                                            }
                                                            if (!romCountryList.ContainsKey(countryItem.Value.Key))
                                                            {
                                                                romCountryList.Add(countryItem.Value.Key, countryItem.Value.Value);
                                                            }
                                                        }
                                                    }
                                                }
                                                break;

                                            case "languages":
                                                string[] languages = attribute.Value.Split(",");
                                                if (game.Language == null)
                                                {
                                                    game.Language = new Dictionary<string, string>();
                                                }
                                                if (languages != null)
                                                {
                                                    foreach (string language in languages)
                                                    {
                                                        KeyValuePair<string, string>? languageItem = LanguageLookup.ParseLanguageString(language.Trim());
                                                        if (languageItem != null)
                                                        {
                                                            if (!game.Language.ContainsKey(languageItem.Value.Key))
                                                            {
                                                                game.Language.Add(languageItem.Value.Key, languageItem.Value.Value);
                                                            }
                                                            if (!romLanguageList.ContainsKey(languageItem.Value.Key))
                                                            {
                                                                romLanguageList.Add(languageItem.Value.Key, languageItem.Value.Value);
                                                            }
                                                        }
                                                    }
                                                }

                                                break;

                                            case "categories":
                                                game.Category = attribute.Value;
                                                break;

                                            case "additional":
                                                MediaData = attribute.Value;
                                                break;

                                            default:
                                                if (!game.flags.ContainsKey(xmlGameItem.Name + "." + attribute.Name))
                                                {
                                                    game.flags.Add(xmlGameItem.Name + "." + attribute.Name, attribute.Value);
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case "source":
                                    // provides details about the roms that belong to this game
                                    RomSignatureObject.Game.Rom rom = new RomSignatureObject.Game.Rom();
                                    rom.RomTypeMedia = MediaData;
                                    rom.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.NoIntros;

                                    rom.Country = romCountryList;
                                    rom.Language = romLanguageList;

                                    foreach (XmlNode xmlSource in xmlGameItem.ChildNodes)
                                    {
                                        switch (xmlSource.Name.ToLower())
                                        {
                                            case "details":
                                            case "serials":
                                                foreach (XmlAttribute detailAttribute in xmlSource.Attributes)
                                                {
                                                    switch (detailAttribute.Name.ToLower())
                                                    {
                                                        default:
                                                            if (!rom.Attributes.ContainsKey(xmlSource.Name + "." + detailAttribute.Name))
                                                            {
                                                                rom.Attributes.Add(xmlSource.Name + "." + detailAttribute.Name, detailAttribute.Value);
                                                            }
                                                            break;
                                                    }
                                                }
                                                break;

                                            case "file":
                                                bool forceNamePresent = false;
                                                foreach (XmlAttribute fileAttribute in xmlSource.Attributes)
                                                {
                                                    switch (fileAttribute.Name.ToLower())
                                                    {
                                                        case "id":
                                                            rom.Id = fileAttribute.Value;
                                                            break;

                                                        case "extension":
                                                            if (forceNamePresent == false)
                                                            {
                                                                if (NameOverwritten == true && MediaData.Length > 0)
                                                                {
                                                                    rom.Name = game.Name + " (" + MediaData + ")." + fileAttribute.Value;
                                                                }
                                                                else
                                                                {
                                                                    rom.Name = game.Name + "." + fileAttribute.Value;
                                                                }
                                                            }
                                                            break;

                                                        case "forcename":
                                                            rom.Name = fileAttribute.Value;
                                                            forceNamePresent = true;
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

                                                        case "sha256":
                                                            rom.Sha256 = fileAttribute.Value;
                                                            break;

                                                        case "status":
                                                            rom.Status = fileAttribute.Value;
                                                            break;

                                                        default:
                                                            if (!rom.Attributes.ContainsKey(xmlSource.Name + "." + fileAttribute.Name))
                                                            {
                                                                rom.Attributes.Add(xmlSource.Name + "." + fileAttribute.Name, fileAttribute.Value);
                                                            }
                                                            break;

                                                    }
                                                }
                                                break;
                                        }
                                    }

                                    if (rom.Md5 == md5 || rom.Sha1 == sha1 || rom.Sha256 == sha256)
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

        public parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            try
            {
                XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

                if (xmlHeader != null)
                {
                    if (xmlHeader.SelectSingleNode("homepage").InnerText.Equals("No-Intro", StringComparison.OrdinalIgnoreCase))
                    {
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

