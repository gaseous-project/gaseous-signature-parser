using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    public class RetroAchievementsParser : BaseParser
    {
        public RetroAchievementsParser()
        {

        }

        public override RomSignatureObject Parse(string XMLFile, Dictionary<string, object>? options = null)
        {
            // load RetroAchievements file
            XmlDocument retroachievementsXmlDoc = InitializeFromFile(XMLFile, out string md5Hash, out string sha1Hash);

            RomSignatureObject retroachievementsObject = new RomSignatureObject();

            // get header
            XmlNode xmlHeader = retroachievementsXmlDoc.DocumentElement.SelectSingleNode("/datafile/header");
            ParseHeader(retroachievementsObject, xmlHeader, "RetroAchievements", md5Hash, sha1Hash);

            // get games
            retroachievementsObject.Games = new List<RomSignatureObject.Game>();
            XmlNodeList xmlGames = retroachievementsXmlDoc.DocumentElement.SelectNodes("/datafile/game");
            foreach (XmlNode xmlGame in xmlGames)
            {
                RomSignatureObject.Game gameObject = new RomSignatureObject.Game();

                gameObject.System = retroachievementsObject.Name.Replace("RetroAchievements - ", "");

                Dictionary<string, string>? romCountryList = new Dictionary<string, string>();
                Dictionary<string, string>? romLanguageList = new Dictionary<string, string>();

                // get the game name
                foreach (XmlAttribute gameAttribute in xmlGame.Attributes)
                {
                    if (gameAttribute.Name.ToLower() == "name")
                    {
                        // parse the game name
                        // everything before the first '(' is the game name
                        string gameName = gameAttribute.Value;
                        int openParenIndex = gameName.IndexOf('(');
                        if (openParenIndex > 0)
                        {
                            gameName = gameName.Substring(0, openParenIndex).Trim();
                        }
                        gameObject.Name = gameName;
                        gameObject.Description = gameName;

                        // break the rom name into parts, each part is contained between parentheses
                        // these parts can be:
                        // - Comma separated list of regions
                        // - Comma separated list of languages (identified by two letters)
                        // - Disk number - always denoted by the word "Disc" followed by a number
                        // - Disk name - always the last item
                        List<string> gameNameParts = gameAttribute.Value.Split('(').Skip(1).ToList<string>();
                        // remove all parts from gameNameParts after and including the first found part that starts with "disk "
                        int diskIndex = gameNameParts.FindIndex(x => x.Trim().StartsWith("disc", StringComparison.OrdinalIgnoreCase));
                        if (diskIndex > 0)
                        {
                            gameNameParts.RemoveRange(diskIndex, gameNameParts.Count - diskIndex);
                        }

                        bool countryFound = false;
                        bool languageFound = false;
                        bool dateFound = false;
                        for (int i = 0; i < gameNameParts.Count; i++)
                        {
                            // remove the ')' and everything after it
                            string part = gameNameParts[i].Trim();
                            int closeParenIndex = part.IndexOf(')');
                            if (closeParenIndex > 0)
                            {
                                part = part.Substring(0, closeParenIndex).Trim();
                            }

                            string[] subParts = part.Split(',');
                            if (subParts.Length > 0)
                            {
                                // check if this is a country
                                if (countryFound == false)
                                {
                                    string[] countries = part.Trim().Split("-");
                                    if (gameObject.Country == null)
                                    {
                                        gameObject.Country = new Dictionary<string, string>();
                                    }
                                    if (countries != null)
                                    {
                                        foreach (string country in countries)
                                        {
                                            KeyValuePair<string, string>? countryItem = CountryLookup.ParseCountryString(country);
                                            if (countryItem != null)
                                            {
                                                if (!gameObject.Country.ContainsKey(countryItem.Value.Key))
                                                {
                                                    gameObject.Country.Add(countryItem.Value.Key, countryItem.Value.Value);
                                                }
                                                if (!romCountryList.ContainsKey(countryItem.Value.Key))
                                                {
                                                    romCountryList.Add(countryItem.Value.Key, countryItem.Value.Value);
                                                }
                                                gameNameParts[i] = "";
                                                countryFound = true;
                                            }
                                        }
                                    }
                                }

                                // check if this is a language
                                if (languageFound == false)
                                {
                                    string[] languages = part.Trim().Split(",");
                                    if (gameObject.Language == null)
                                    {
                                        gameObject.Language = new Dictionary<string, string>();
                                    }
                                    if (languages != null)
                                    {
                                        foreach (string language in languages)
                                        {
                                            KeyValuePair<string, string>? languageItem = LanguageLookup.ParseLanguageString(language);
                                            if (languageItem != null)
                                            {
                                                if (!gameObject.Language.ContainsKey(languageItem.Value.Key))
                                                {
                                                    gameObject.Language.Add(languageItem.Value.Key, languageItem.Value.Value);
                                                }
                                                if (!romLanguageList.ContainsKey(languageItem.Value.Key))
                                                {
                                                    romLanguageList.Add(languageItem.Value.Key, languageItem.Value.Value);
                                                }
                                                gameNameParts[i] = "";
                                                languageFound = true;
                                            }
                                        }
                                    }
                                }

                                // check if this is a date
                                if (dateFound == false)
                                {
                                    if (DateTime.TryParse(subParts[0].Trim(), out DateTime date))
                                    {
                                        gameObject.Year = date.ToString();
                                        gameNameParts[i] = "";
                                        dateFound = true;
                                    }
                                }

                                // check if this is a demo
                                if (subParts[0].Trim().StartsWith("demo", StringComparison.OrdinalIgnoreCase))
                                {
                                    gameObject.Demo = RomSignatureObject.Game.DemoTypes.demo;
                                    gameNameParts[i] = "";
                                }
                            }
                        }
                    }
                }

                // get the game data
                foreach (XmlNode gameNode in xmlGame.ChildNodes)
                {
                    switch (gameNode.Name.ToLower())
                    {
                        case "category":
                            gameObject.Category = gameNode.InnerText;
                            break;

                        case "rom":
                            // generate new ROM object
                            RomSignatureObject.Game.Rom romObject = new RomSignatureObject.Game.Rom();
                            romObject.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.RetroAchievements;
                            romObject.Attributes = new Dictionary<string, object>();

                            // get the ROM data
                            foreach (XmlAttribute romAttribute in gameNode.Attributes)
                            {
                                switch (romAttribute.Name.ToLower())
                                {
                                    case "name":
                                        romObject.Name = romAttribute.Value;

                                        // parse the rom name
                                        // remove the extension
                                        string romName = romAttribute.Value;
                                        int extensionIndex = romName.LastIndexOf('.');
                                        if (extensionIndex > 0)
                                        {
                                            romName = romName.Substring(0, extensionIndex).Trim();
                                        }

                                        // break the rom name into parts, each part is contained between parentheses
                                        string[] romNameParts = romName.Split('(').Skip(1).ToArray();
                                        bool countryFound = false;
                                        if (romObject.Country == null)
                                        {
                                            romObject.Country = new Dictionary<string, string>();
                                        }
                                        bool languageFound = false;
                                        if (romObject.Language == null)
                                        {
                                            romObject.Language = new Dictionary<string, string>();
                                        }
                                        bool readyForDiskName = false;
                                        bool developmentStatusFound = false;
                                        foreach (string romNamePart in romNameParts)
                                        {
                                            // remove the ')' and everything after it
                                            string part = romNamePart.Trim();
                                            int closeParenIndex = part.IndexOf(')');
                                            if (closeParenIndex > 0)
                                            {
                                                part = part.Substring(0, closeParenIndex).Trim();
                                            }

                                            if (!countryFound)
                                            {
                                                // check if this is a country
                                                KeyValuePair<string, string>? countryItem = CountryLookup.ParseCountryString(part);
                                                if (countryItem != null)
                                                {
                                                    if (!romObject.Country.ContainsKey(countryItem.Value.Key))
                                                    {
                                                        romObject.Country.Add(countryItem.Value.Key, countryItem.Value.Value);
                                                    }
                                                    if (!gameObject.Country.ContainsKey(countryItem.Value.Key))
                                                    {
                                                        gameObject.Country.Add(countryItem.Value.Key, countryItem.Value.Value);
                                                    }
                                                    part = "";
                                                    countryFound = true;
                                                }
                                            }

                                            if (!languageFound)
                                            {
                                                // check if this is a language
                                                KeyValuePair<string, string>? languageItem = LanguageLookup.ParseLanguageString(part);
                                                if (languageItem != null)
                                                {
                                                    if (!romObject.Language.ContainsKey(languageItem.Value.Key))
                                                    {
                                                        romObject.Language.Add(languageItem.Value.Key, languageItem.Value.Value);
                                                    }
                                                    if (!gameObject.Language.ContainsKey(languageItem.Value.Key))
                                                    {
                                                        gameObject.Language.Add(languageItem.Value.Key, languageItem.Value.Value);
                                                    }
                                                    part = "";
                                                    languageFound = true;
                                                }
                                            }

                                            if (part.StartsWith("disc", StringComparison.OrdinalIgnoreCase))
                                            {
                                                // disk number
                                                romObject.RomType = RomSignatureObject.Game.Rom.RomTypes.Disc;
                                                romObject.RomTypeMedia = part;
                                                readyForDiskName = true;
                                            }
                                            else
                                            {
                                                if (readyForDiskName)
                                                {
                                                    // disk name
                                                    romObject.MediaLabel = part;
                                                }
                                                else
                                                {
                                                    if (developmentStatusFound == false)
                                                    {
                                                        var devStatus = DevelopmentStatusLookup.ParseStatusString(part);
                                                        if (devStatus != null)
                                                        {
                                                            romObject.DevelopmentStatus = devStatus.Code;
                                                            developmentStatusFound = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        break;

                                    case "size":
                                        romObject.Size = (ulong?)Convert.ToInt64(romAttribute.Value);
                                        break;

                                    case "crc":
                                        romObject.Crc = romAttribute.Value;
                                        break;

                                    case "md5":
                                        romObject.Md5 = romAttribute.Value;
                                        break;

                                    case "sha1":
                                        romObject.Sha1 = romAttribute.Value;
                                        break;

                                    case "sha256":
                                        romObject.Sha256 = romAttribute.Value;
                                        break;

                                    case "status":
                                        romObject.Status = romAttribute.Value;
                                        break;

                                    default:
                                        if (romAttribute.Value.Length > 0)
                                        {
                                            romObject.Attributes.Add(romAttribute.Name, romAttribute.Value);
                                        }
                                        break;
                                }
                            }
                            gameObject.Roms.Add(romObject);
                            break;
                    }
                }

                // search for existing gameObject to update
                bool existingGameFound = false;
                foreach (RomSignatureObject.Game existingGame in retroachievementsObject.Games)
                {
                    if (existingGame.SortingName == gameObject.SortingName &&
                        existingGame.Year == gameObject.Year &&
                        existingGame.Publisher == gameObject.Publisher // &&
                                                                       // existingGame.Country == gameObject.Country &&
                                                                       // existingGame.Language == gameObject.Language
                        )
                    {
                        existingGame.Roms.AddRange(gameObject.Roms);
                        existingGameFound = true;
                        break;
                    }
                }
                if (existingGameFound == false)
                {
                    retroachievementsObject.Games.Add(gameObject);
                }
            }

            return retroachievementsObject;
        }

        public override parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            if (xml.DocumentElement == null)
            {
                return parser.SignatureParser.Unknown;
            }

            XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

            if (xmlHeader == null)
            {
                return parser.SignatureParser.Unknown;
            }

            var nodeCategory = xmlHeader.SelectSingleNode("category");

            if (nodeCategory == null)
            {
                return parser.SignatureParser.Unknown;
            }

            if (nodeCategory.InnerText.Equals("RetroAchievements", StringComparison.OrdinalIgnoreCase))
            {
                return parser.SignatureParser.RetroAchievements;
            }

            return parser.SignatureParser.Unknown;
        }
    }
}
