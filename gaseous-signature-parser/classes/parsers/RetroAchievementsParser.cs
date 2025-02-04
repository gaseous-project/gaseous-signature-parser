using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    public class RetroAchievementsParser
    {
        public RetroAchievementsParser()
        {

        }

        public RomSignatureObject Parse(string XMLFile)
        {
            // load resources
            var assembly = Assembly.GetExecutingAssembly();
            // load systems list
            List<string> TOSECSystems = new List<string>();
            var resourceName = "gaseous_signature_parser.support.parsers.tosec.Systems.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                TOSECSystems = reader.ReadToEnd().Split(Environment.NewLine).ToList<string>();
            }
            // load video list
            List<string> TOSECVideo = new List<string>();
            resourceName = "gaseous_signature_parser.support.parsers.tosec.Video.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                TOSECVideo = reader.ReadToEnd().Split(Environment.NewLine).ToList<string>();
            }
            // load country list
            Dictionary<string, string> TOSECCountry = new Dictionary<string, string>();
            resourceName = "gaseous_signature_parser.support.parsers.tosec.Country.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                do
                {
                    string[] line = reader.ReadLine().Split(",");
                    TOSECCountry.Add(line[0], line[1]);
                } while (reader.EndOfStream == false);
            }
            // load language list
            Dictionary<string, string> TOSECLanguage = new Dictionary<string, string>();
            resourceName = "gaseous_signature_parser.support.parsers.tosec.Language.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                do
                {
                    string[] line = reader.ReadLine().Split(",");
                    TOSECLanguage.Add(line[0], line[1]);
                } while (reader.EndOfStream == false);
            }
            // load copyright list
            Dictionary<string, string> TOSECCopyright = new Dictionary<string, string>();
            resourceName = "gaseous_signature_parser.support.parsers.tosec.Copyright.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                do
                {
                    string[] line = reader.ReadLine().Split(",");
                    TOSECCopyright.Add(line[0], line[1]);
                } while (reader.EndOfStream == false);
            }
            // load development status list
            Dictionary<string, string> TOSECDevelopment = new Dictionary<string, string>();
            resourceName = "gaseous_signature_parser.support.parsers.tosec.DevelopmentStatus.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                do
                {
                    string[] line = reader.ReadLine().Split(",");
                    TOSECDevelopment.Add(line[0], line[1]);
                } while (reader.EndOfStream == false);
            }

            // get hashes of RetroAchievements file
            var xmlStream = File.OpenRead(XMLFile);

            var md5 = MD5.Create();
            byte[] md5HashByte = md5.ComputeHash(xmlStream);
            string md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();

            var sha1 = SHA1.Create();
            byte[] sha1HashByte = sha1.ComputeHash(xmlStream);
            string sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();

            // load RetroAchievements file
            XmlDocument retroachievementsXmlDoc = new XmlDocument();
            retroachievementsXmlDoc.Load(XMLFile);

            RomSignatureObject retroachievementsObject = new RomSignatureObject();

            // get header
            XmlNode xmlHeader = retroachievementsXmlDoc.DocumentElement.SelectSingleNode("/datafile/header");
            retroachievementsObject.SourceType = "RetroAchievements";
            retroachievementsObject.SourceMd5 = md5Hash;
            retroachievementsObject.SourceSHA1 = sha1Hash;
            foreach (XmlNode childNode in xmlHeader.ChildNodes)
            {
                switch (childNode.Name.ToLower())
                {
                    case "name":
                        retroachievementsObject.Name = childNode.InnerText;
                        break;

                    case "description":
                        retroachievementsObject.Description = childNode.InnerText;
                        break;

                    case "category":
                        retroachievementsObject.Category = childNode.InnerText;
                        break;

                    case "version":
                        retroachievementsObject.Version = childNode.InnerText;
                        break;

                    case "author":
                        retroachievementsObject.Author = childNode.InnerText;
                        break;

                    case "email":
                        retroachievementsObject.Email = childNode.InnerText;
                        break;

                    case "homepage":
                        retroachievementsObject.Homepage = childNode.InnerText;
                        break;

                    case "url":
                        try
                        {
                            retroachievementsObject.Url = new Uri(childNode.InnerText);
                        }
                        catch
                        {
                            retroachievementsObject.Url = null;
                        }
                        break;
                }
            }

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
                            // remove trailing ')'
                            string part = gameNameParts[i].Trim().TrimEnd(')');

                            string[] subParts = part.Split(',');
                            if (subParts.Length > 0)
                            {
                                // check if this is a country
                                if (countryFound == false)
                                {
                                    if (
                                        TOSECCountry.Values.Any(v => v.Equals(subParts[0].Trim(), StringComparison.OrdinalIgnoreCase)) ||
                                        TOSECCountry.Keys.Any(v => v.Equals(subParts[0].Trim(), StringComparison.OrdinalIgnoreCase))
                                        )
                                    {
                                        string[] countries = part.Trim().Split("-");
                                        if (countries.Length > 1)
                                        {
                                            if (TOSECCountry.ContainsKey(countries[0]))
                                            {
                                                gameObject.CountryString = part.Trim();

                                                foreach (string country in countries)
                                                {
                                                    if (TOSECCountry.ContainsKey(country))
                                                    {
                                                        gameObject.Country.Add(country, TOSECCountry[country]);
                                                        romCountryList.Add(country, TOSECCountry[country]);
                                                    }
                                                }
                                            }
                                        }

                                        gameNameParts[i] = "";
                                        countryFound = true;
                                    }
                                }

                                // check if this is a language
                                if (languageFound == false)
                                {
                                    if (TOSECLanguage.Keys.Any(v => v.Equals(subParts[0].Trim(), StringComparison.OrdinalIgnoreCase)))
                                    {
                                        string[] languages = part.Trim().Split("-");
                                        if (languages.Length > 1)
                                        {
                                            if (TOSECLanguage.ContainsKey(languages[0]))
                                            {
                                                gameObject.LanguageString = part.Trim();


                                                foreach (string language in languages)
                                                {
                                                    if (TOSECLanguage.ContainsKey(language))
                                                    {
                                                        gameObject.Language.Add(language, TOSECLanguage[language]);
                                                        romLanguageList.Add(language, TOSECLanguage[language]);
                                                    }
                                                }
                                            }
                                        }

                                        gameNameParts[i] = "";
                                        languageFound = true;
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
                                        bool readyForDiskName = false;
                                        bool developmentStatusFound = false;
                                        foreach (string romNamePart in romNameParts)
                                        {
                                            // remove trailing ')'
                                            string part = romNamePart.Trim().TrimEnd(')');

                                            // we're checking roms, so we're not interested in regions or languages, only disk numbers and disk names
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
                                                    // check for development status
                                                    if (developmentStatusFound == false)
                                                    {
                                                        if (TOSECDevelopment.Keys.Any(v => v.Equals(part.Trim(), StringComparison.OrdinalIgnoreCase)))
                                                        {
                                                            romObject.DevelopmentStatus = TOSECDevelopment[part.ToLower()];
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
                    retroachievementsObject.Games.Add(gameObject);
                }
            }

            return retroachievementsObject;
        }

        public parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            try
            {
                XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

                if (xmlHeader != null)
                {
                    if (xmlHeader.SelectSingleNode("category").InnerText.Equals("RetroAchievements", StringComparison.OrdinalIgnoreCase))
                    {
                        return parser.SignatureParser.RetroAchievements;
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

