using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    public class RedumpParser
    {
        public RedumpParser()
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

            // get hashes of Redump file
            var xmlStream = File.OpenRead(XMLFile);

            var md5 = MD5.Create();
            byte[] md5HashByte = md5.ComputeHash(xmlStream);
            string md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();

            var sha1 = SHA1.Create();
            byte[] sha1HashByte = sha1.ComputeHash(xmlStream);
            string sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();

            // load Redump file
            XmlDocument redumpXmlDoc = new XmlDocument();
            redumpXmlDoc.Load(XMLFile);

            RomSignatureObject redumpObject = new RomSignatureObject();

            // get header
            XmlNode xmlHeader = redumpXmlDoc.DocumentElement.SelectSingleNode("/datafile/header");
            redumpObject.SourceType = "Redump";
            redumpObject.SourceMd5 = md5Hash;
            redumpObject.SourceSHA1 = sha1Hash;
            foreach (XmlNode childNode in xmlHeader.ChildNodes)
            {
                switch (childNode.Name.ToLower())
                {
                    case "name":
                        redumpObject.Name = childNode.InnerText;
                        break;

                    case "description":
                        redumpObject.Description = childNode.InnerText;
                        break;

                    case "category":
                        redumpObject.Category = childNode.InnerText;
                        break;

                    case "version":
                        redumpObject.Version = childNode.InnerText;
                        break;

                    case "author":
                        redumpObject.Author = childNode.InnerText;
                        break;

                    case "email":
                        redumpObject.Email = childNode.InnerText;
                        break;

                    case "homepage":
                        redumpObject.Homepage = childNode.InnerText;
                        break;

                    case "url":
                        try
                        {
                            redumpObject.Url = new Uri(childNode.InnerText);
                        }
                        catch
                        {
                            redumpObject.Url = null;
                        }
                        break;
                }
            }

            // get games
            redumpObject.Games = new List<RomSignatureObject.Game>();
            XmlNodeList xmlGames = redumpXmlDoc.DocumentElement.SelectNodes("/datafile/game");
            foreach (XmlNode xmlGame in xmlGames)
            {
                RomSignatureObject.Game gameObject = new RomSignatureObject.Game();

                gameObject.System = redumpObject.Name;

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

                // get the game data
                string gameSerial = "";
                foreach (XmlNode gameNode in xmlGame.ChildNodes)
                {
                    switch (gameNode.Name.ToLower())
                    {
                        case "category":
                            gameObject.Category = gameNode.InnerText;
                            break;

                        case "serial":
                            gameSerial = gameNode.InnerText;
                            break;

                        case "rom":
                            // generate new ROM object
                            RomSignatureObject.Game.Rom romObject = new RomSignatureObject.Game.Rom();
                            romObject.SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.Redump;

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

                                    case "sha256":
                                        romObject.Sha256 = romAttribute.Value;
                                        break;

                                    case "status":
                                        romObject.Status = romAttribute.Value;
                                        break;

                                    default:
                                        // capture any other attributes as extra metadata
                                        romObject.Attributes.Add(romAttribute.Name, romAttribute.Value);
                                        break;
                                }
                            }

                            // apply serial to the rom object if it's not null
                            if (string.IsNullOrEmpty(gameSerial) == false)
                            {
                                romObject.Attributes.Add("serial", gameSerial);
                            }

                            gameObject.Roms.Add(romObject);
                            break;

                        default:
                            // capture any other tags as extra metadata
                            gameObject.flags.Add(gameNode.Name, gameNode.InnerText);
                            break;
                    }
                }


                // search for existing gameObject to update
                bool existingGameFound = false;
                foreach (RomSignatureObject.Game existingGame in redumpObject.Games)
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
                    redumpObject.Games.Add(gameObject);
                }
            }

            return redumpObject;
        }

        public parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            XmlNode xmlHeader = xml.DocumentElement.SelectSingleNode("/datafile/header");

            if (xmlHeader == null)
            {
                return parser.SignatureParser.Unknown;
            }

            var nodeHomepage = xmlHeader.SelectSingleNode("homepage");

            if (nodeHomepage != null && nodeHomepage.InnerText.Equals("redump.org", StringComparison.OrdinalIgnoreCase))
            {
                return parser.SignatureParser.Redump;
            }

            return parser.SignatureParser.Unknown;
        }
    }
}

