using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using gaseous_signature_parser.models.RomSignatureObject;
using gaseous_signature_parser.models.provider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gaseous_signature_parser.classes.parsers
{
    public class ScreenScraperParser : BaseParser
    {
        public ScreenScraperParser()
        {

        }

        public override RomSignatureObject Parse(string xmlFile, Dictionary<string, object>? options = null)
        {
            using var stream = File.OpenRead(xmlFile);
            var hashes = Hash.GenerateHashes(stream);

            ScreenScaperModel.ssGame game = ParseFileToScreenScraperGame(xmlFile);
            return MapToRomSignatureObject(game, hashes.md5, hashes.sha1);
        }

        public override parser.SignatureParser GetXmlType(XmlDocument xml)
        {
            if (xml.DocumentElement == null)
            {
                return parser.SignatureParser.Unknown;
            }

            XmlNode? dataGame = xml.DocumentElement.SelectSingleNode("/Data/jeu");
            if (dataGame != null)
            {
                return parser.SignatureParser.ScreenScraper;
            }

            XmlNode? responseGame = xml.DocumentElement.SelectSingleNode("//response/jeu");
            if (responseGame != null)
            {
                return parser.SignatureParser.ScreenScraper;
            }

            return parser.SignatureParser.Unknown;
        }

        internal static bool IsScreenScraperJson(string content)
        {
            try
            {
                JToken? token = JToken.Parse(content);
                if (token.Type != JTokenType.Object)
                {
                    return false;
                }

                JObject root = (JObject)token;

                if (root["response"]?["jeu"] != null)
                {
                    return true;
                }

                return root["id"] != null
                    && root["romid"] != null
                    && root["noms"] is JArray;
            }
            catch
            {
                return false;
            }
        }

        private static ScreenScaperModel.ssGame ParseFileToScreenScraperGame(string filePath)
        {
            string content = File.ReadAllText(filePath);
            char firstChar = content.FirstOrDefault(c => !char.IsWhiteSpace(c));

            if (firstChar == '{' || firstChar == '[')
            {
                return ParseJsonToGame(content);
            }

            if (firstChar == '<')
            {
                return ParseXmlToGame(content);
            }

            throw new FormatException("Unsupported ScreenScraper payload format.");
        }

        private static ScreenScaperModel.ssGame ParseJsonToGame(string content)
        {
            JToken root = JToken.Parse(content);

            if (root.Type != JTokenType.Object)
            {
                throw new FormatException("Unsupported ScreenScraper JSON payload.");
            }

            JObject obj = (JObject)root;
            JToken? gameToken = obj["response"]?["jeu"] ?? obj;

            ScreenScaperModel.ssGame? game = gameToken.ToObject<ScreenScaperModel.ssGame>();
            if (game == null)
            {
                throw new FormatException("Unable to deserialize ScreenScraper JSON payload.");
            }

            return game;
        }

        private static ScreenScaperModel.ssGame ParseXmlToGame(string content)
        {
            XDocument doc = XDocument.Parse(content, LoadOptions.PreserveWhitespace);
            XElement? gameNode = doc.Root?.Element("jeu") ?? doc.Root?.Descendants("jeu").FirstOrDefault();

            if (gameNode == null)
            {
                throw new FormatException("ScreenScraper XML payload does not contain a jeu node.");
            }

            ScreenScaperModel.ssGame game = new ScreenScaperModel.ssGame
            {
                id = ParseLong(GetAttributeOrElement(gameNode, "id")),
                notgame = ParseBool(GetAttributeOrElement(gameNode, "notgame")),
                noms = ParseRegionalTexts(gameNode.Element("noms")),
                cloneof = GetElementValue(gameNode, "cloneof"),
                systeme = ParseTextId(gameNode.Element("systeme")),
                editeur = ParseTextId(gameNode.Element("editeur")),
                developpeur = ParseTextId(gameNode.Element("developpeur")),
                joueurs = ParseStringPair(gameNode.Element("joueurs")?.Value),
                note = ParseStringPair(gameNode.Element("note")?.Value),
                topstaff = GetElementValue(gameNode, "topstaff"),
                rotation = GetElementValue(gameNode, "rotation"),
                synopsis = ParseLanguageTexts(gameNode.Element("synopsis"), "synopsis"),
                classifications = ParseClassifications(gameNode.Element("classifications")),
                dates = ParseDates(gameNode.Element("dates")),
                genres = ParseTaxonomy(gameNode.Element("genres"), "genre"),
                modes = ParseModes(gameNode.Element("modes")),
                familles = ParseFranchises(gameNode.Element("familles")),
                medias = ParseMedias(gameNode.Element("medias")),
                roms = ParseRoms(gameNode.Element("roms"))
            };

            return game;
        }

        private static RomSignatureObject MapToRomSignatureObject(ScreenScaperModel.ssGame game, string md5Hash, string sha1Hash)
        {
            var signatureObject = new RomSignatureObject
            {
                SourceType = "ScreenScraper",
                SourceMd5 = md5Hash,
                SourceSHA1 = sha1Hash,
                Name = PickBestGameName(game.noms),
                Description = PickBestSynopsis(game.synopsis),
                Games = new List<RomSignatureObject.Game>()
            };

            var gameObject = new RomSignatureObject.Game
            {
                Id = game.id?.ToString(CultureInfo.InvariantCulture),
                CloneOfId = game.cloneof,
                GameId = game.romid?.ToString(CultureInfo.InvariantCulture),
                Name = PickBestGameName(game.noms),
                Description = PickBestSynopsis(game.synopsis),
                Publisher = game.editeur?.text,
                System = game.systeme?.text,
                Year = PickBestYear(game.dates),
                Country = ParseCountriesFromNames(game.noms),
                Language = ParseLanguages(game.synopsis),
                Roms = new List<RomSignatureObject.Game.Rom>(),
                flags = new Dictionary<string, object>()
            };

            gameObject.flags["screenscraper.notgame"] = game.notgame ?? false;
            gameObject.flags["screenscraper.topstaff"] = game.topstaff ?? string.Empty;
            gameObject.flags["screenscraper.rotation"] = game.rotation ?? string.Empty;

            if (game.medias != null)
            {
                gameObject.flags["screenscraper.medias"] = game.medias;
            }

            if (game.roms != null)
            {
                foreach (ScreenScaperModel.ssRom sourceRom in game.roms)
                {
                    var rom = new RomSignatureObject.Game.Rom
                    {
                        Id = sourceRom.Id?.ToString(CultureInfo.InvariantCulture),
                        Name = sourceRom.romfilename,
                        Size = sourceRom.romsize.HasValue ? (ulong?)sourceRom.romsize.Value : null,
                        Crc = sourceRom.romcrc,
                        Md5 = sourceRom.rommd5,
                        Sha1 = sourceRom.romsha1,
                        SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.ScreenScraper,
                        Attributes = new Dictionary<string, object>()
                    };

                    if (sourceRom.langues != null)
                    {
                        rom.Language = FlattenLocalizedMap(sourceRom.langues);
                    }

                    if (sourceRom.regions != null)
                    {
                        rom.Country = FlattenLocalizedMap(sourceRom.regions);
                    }

                    rom.Attributes["screenscraper.best"] = sourceRom.best ?? 0;
                    rom.Attributes["screenscraper.beta"] = sourceRom.Beta ?? 0;
                    rom.Attributes["screenscraper.demo"] = sourceRom.Demo ?? 0;
                    rom.Attributes["screenscraper.hack"] = sourceRom.hack ?? 0;
                    rom.Attributes["screenscraper.unl"] = sourceRom.Unl ?? 0;

                    gameObject.Roms.Add(rom);
                }
            }

            signatureObject.Games.Add(gameObject);
            return signatureObject;
        }

        private static string? PickBestGameName(List<ScreenScaperModel.ssRegionalText>? names)
        {
            if (names == null || names.Count == 0)
            {
                return null;
            }

            string[] preferredRegions = { "ss", "wor", "us", "eu", "jp" };
            foreach (string region in preferredRegions)
            {
                string? match = names.FirstOrDefault(n => string.Equals(n.region, region, StringComparison.OrdinalIgnoreCase))?.text;
                if (!string.IsNullOrWhiteSpace(match))
                {
                    return match;
                }
            }

            return names.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n.text))?.text;
        }

        private static string? PickBestSynopsis(List<ScreenScaperModel.ssLanguageText>? synopsis)
        {
            if (synopsis == null || synopsis.Count == 0)
            {
                return null;
            }

            string[] preferredLanguages = { "en", "fr" };
            foreach (string language in preferredLanguages)
            {
                string? match = synopsis.FirstOrDefault(n => string.Equals(n.langue, language, StringComparison.OrdinalIgnoreCase))?.text;
                if (!string.IsNullOrWhiteSpace(match))
                {
                    return match;
                }
            }

            return synopsis.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n.text))?.text;
        }

        private static string? PickBestYear(List<ScreenScaperModel.ssGameDate>? dates)
        {
            if (dates == null || dates.Count == 0)
            {
                return null;
            }

            foreach (ScreenScaperModel.ssGameDate date in dates)
            {
                if (string.IsNullOrWhiteSpace(date.date))
                {
                    continue;
                }

                if (DateTime.TryParse(date.date, out DateTime parsedDate))
                {
                    return parsedDate.Year.ToString(CultureInfo.InvariantCulture);
                }

                if (date.date.Length >= 4)
                {
                    return date.date.Substring(0, 4);
                }
            }

            return null;
        }

        private static Dictionary<string, string> ParseCountriesFromNames(List<ScreenScaperModel.ssRegionalText>? names)
        {
            var countries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (names == null)
            {
                return countries;
            }

            foreach (ScreenScaperModel.ssRegionalText item in names)
            {
                if (string.IsNullOrWhiteSpace(item.region))
                {
                    continue;
                }

                if (!countries.ContainsKey(item.region))
                {
                    countries[item.region] = item.region;
                }
            }

            return countries;
        }

        private static Dictionary<string, string> ParseLanguages(List<ScreenScaperModel.ssLanguageText>? languages)
        {
            var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (languages == null)
            {
                return parsed;
            }

            foreach (ScreenScaperModel.ssLanguageText item in languages)
            {
                if (string.IsNullOrWhiteSpace(item.langue))
                {
                    continue;
                }

                if (!parsed.ContainsKey(item.langue))
                {
                    parsed[item.langue] = item.langue;
                }
            }

            return parsed;
        }

        private static Dictionary<string, string>? FlattenLocalizedMap(Dictionary<string, List<string>>? value)
        {
            if (value == null)
            {
                return null;
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, List<string>> item in value)
            {
                if (item.Value == null || item.Value.Count == 0)
                {
                    continue;
                }

                map[item.Key] = item.Value[0];
            }

            return map;
        }

        private static string? GetAttributeOrElement(XElement element, string name)
        {
            return element.Attribute(name)?.Value ?? element.Element(name)?.Value;
        }

        private static string? GetElementValue(XElement element, string name)
        {
            return element.Element(name)?.Value;
        }

        private static long? ParseLong(string? value)
        {
            if (long.TryParse(value, out long parsed))
            {
                return parsed;
            }

            return null;
        }

        private static bool? ParseBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (bool.TryParse(value, out bool parsed))
            {
                return parsed;
            }

            if (value == "1")
            {
                return true;
            }

            if (value == "0")
            {
                return false;
            }

            return null;
        }

        private static List<ScreenScaperModel.ssRegionalText>? ParseRegionalTexts(XElement? parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements().Select(e => new ScreenScaperModel.ssRegionalText
            {
                region = e.Attribute("region")?.Value,
                text = e.Value
            }).ToList();
        }

        private static ScreenScaperModel.ssTextId? ParseTextId(XElement? element)
        {
            if (element == null)
            {
                return null;
            }

            return new ScreenScaperModel.ssTextId
            {
                id = element.Attribute("id")?.Value,
                text = element.Value
            };
        }

        private static KeyValuePair<string, string>? ParseStringPair(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return new KeyValuePair<string, string>(string.Empty, value.Trim());
        }

        private static List<ScreenScaperModel.ssLanguageText>? ParseLanguageTexts(XElement? parent, string elementName)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Descendants(elementName).Select(e => new ScreenScaperModel.ssLanguageText
            {
                langue = e.Attribute("langue")?.Value,
                text = e.Value
            }).Where(x => !string.IsNullOrWhiteSpace(x.langue) || !string.IsNullOrWhiteSpace(x.text)).ToList();
        }

        private static List<ScreenScaperModel.ssGameClassification>? ParseClassifications(XElement? parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements("classification").Select(e => new ScreenScaperModel.ssGameClassification
            {
                type = e.Attribute("type")?.Value,
                text = e.Value
            }).ToList();
        }

        private static List<ScreenScaperModel.ssGameDate>? ParseDates(XElement? parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements("date").Select(e => new ScreenScaperModel.ssGameDate
            {
                region = e.Attribute("region")?.Value,
                date = e.Value
            }).ToList();
        }

        private static List<ScreenScaperModel.ssGameGenre>? ParseTaxonomy(XElement? parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements(childName).Select(e => new ScreenScaperModel.ssGameGenre
            {
                id = e.Attribute("id")?.Value,
                nomcourt = e.Attribute("nomcourt")?.Value,
                principale = e.Attribute("principale")?.Value,
                parentid = e.Attribute("parentid")?.Value,
                noms = new List<ScreenScaperModel.ssLanguageText>
                {
                    new ScreenScaperModel.ssLanguageText
                    {
                        langue = e.Attribute("langue")?.Value,
                        text = e.Value
                    }
                }
            }).ToList();
        }

        private static List<ScreenScaperModel.ssGameMode>? ParseModes(XElement? parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements("mode").Select(e => new ScreenScaperModel.ssGameMode
            {
                id = e.Attribute("id")?.Value,
                nomcourt = e.Attribute("nomcourt")?.Value,
                principale = e.Attribute("principale")?.Value,
                parentid = e.Attribute("parentid")?.Value,
                noms = new List<ScreenScaperModel.ssLanguageText>
                {
                    new ScreenScaperModel.ssLanguageText
                    {
                        langue = e.Attribute("langue")?.Value,
                        text = e.Value
                    }
                }
            }).ToList();
        }

        private static List<ScreenScaperModel.ssGameFranchise>? ParseFranchises(XElement? parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements("famille").Select(e => new ScreenScaperModel.ssGameFranchise
            {
                id = e.Attribute("id")?.Value,
                nomcourt = e.Attribute("nomcourt")?.Value,
                principale = e.Attribute("principale")?.Value,
                parentid = e.Attribute("parentid")?.Value,
                noms = new List<ScreenScaperModel.ssLanguageText>
                {
                    new ScreenScaperModel.ssLanguageText
                    {
                        langue = e.Attribute("langue")?.Value,
                        text = e.Value
                    }
                }
            }).ToList();
        }

        private static List<ScreenScaperModel.ssMedia>? ParseMedias(XElement? parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements("media").Select(e => new ScreenScaperModel.ssMedia
            {
                parent = e.Attribute("parent")?.Value,
                type = e.Attribute("type")?.Value,
                region = e.Attribute("region")?.Value,
                crc = e.Attribute("crc")?.Value,
                md5 = e.Attribute("md5")?.Value,
                sha1 = e.Attribute("sha1")?.Value,
                size = e.Attribute("size")?.Value,
                format = e.Attribute("format")?.Value,
                url = e.Value
            }).ToList();
        }

        private static List<ScreenScaperModel.ssRom>? ParseRoms(XElement? parent)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements("rom").Select(e => new ScreenScaperModel.ssRom
            {
                Id = ParseLong(GetAttributeOrElement(e, "id")),
                Romnumsupport = ParseInt(GetAttributeOrElement(e, "romnumsupport")),
                romtotalsupport = ParseInt(GetAttributeOrElement(e, "romtotalsupport")),
                romfilename = GetAttributeOrElement(e, "romfilename"),
                romsize = ParseInt(GetAttributeOrElement(e, "romsize")),
                romcrc = GetAttributeOrElement(e, "romcrc"),
                rommd5 = GetAttributeOrElement(e, "rommd5"),
                romsha1 = GetAttributeOrElement(e, "romsha1"),
                romcloneof = ParseLong(GetAttributeOrElement(e, "romcloneof")),
                Beta = ParseInt(GetAttributeOrElement(e, "beta")),
                Demo = ParseInt(GetAttributeOrElement(e, "demo")),
                trad = ParseInt(GetAttributeOrElement(e, "trad")),
                hack = ParseInt(GetAttributeOrElement(e, "hack")),
                Unl = ParseInt(GetAttributeOrElement(e, "unl")),
                alt = ParseInt(GetAttributeOrElement(e, "alt")),
                best = ParseInt(GetAttributeOrElement(e, "best")),
                Retroachievement = ParseInt(GetAttributeOrElement(e, "retroachievement")),
                Gamelink = ParseInt(GetAttributeOrElement(e, "gamelink")),
                nbscrap = ParseInt(GetAttributeOrElement(e, "nbscrap"))
            }).ToList();
        }

        private static int? ParseInt(string? value)
        {
            if (int.TryParse(value, out int parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}