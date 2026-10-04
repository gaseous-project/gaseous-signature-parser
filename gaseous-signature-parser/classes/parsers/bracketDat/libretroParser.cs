using gaseous_signature_parser.models.RomSignatureObject;
using System.IO;
using System.Text.RegularExpressions;

namespace gaseous_signature_parser.classes.bracketparsers
{
    public class libretroParser : BaseParser
    {
        private const string HeaderTag = "clrmamepro";
        private static readonly string[] EntryTags = { "game" };
        private static readonly string[] ChildTags = { "description", "releaseyear", "developer", "file", "rom" };

        private sealed class ParsedTdcName
        {
            public string Title { get; set; } = string.Empty;
            public string? Year { get; set; }
            public string? Publisher { get; set; }
            public string? Category { get; set; }
            public List<string> Qualifiers { get; set; } = new List<string>();
            public List<string> Tags { get; set; } = new List<string>();
            public Dictionary<string, string> Countries { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string>();
            public string? Version { get; set; }
            public string? Release { get; set; }
            public bool KnownGood { get; set; }
            public string RawName { get; set; } = string.Empty;
        }

        public libretroParser()
        {

        }

        public override parser.SignatureParser GetDatType(string dat)
        {
            // Since this is the libretro parser, we always return the libretro signature type as there is no unique way to determine it from the DAT file itself.
            return parser.SignatureParser.libretro;
        }

        public override RomSignatureObject Parse(string datFile, Dictionary<string, object>? options = null)
        {
            using FileStream datStream = File.OpenRead(datFile);
            if (datStream.Length == 0)
            {
                throw new ArgumentException("The DAT file is empty.", nameof(datFile));
            }

            // libretro's only consistent platform identifier is the file name - strip the directory and extension to get the base name
            string platformName = Path.GetFileNameWithoutExtension(datFile);

            var hashes = Hash.GenerateHashes(datStream);
            Dictionary<string, string> headerData = ExtractHeaderData(datFile, HeaderTag);
            List<Dictionary<string, object>> entries = ExtractDataEntries(datFile, EntryTags, ChildTags);

            RomSignatureObject signatureObject = new RomSignatureObject
            {
                SourceType = "libretro",
                SourceMd5 = hashes.md5,
                SourceSHA1 = hashes.sha1,
                Games = new List<RomSignatureObject.Game>()
            };

            // pre-load metadata files for this platform if present
            // get the path to the datFile's directory
            string datDirectory = Path.GetDirectoryName(datFile) ?? string.Empty;
            Dictionary<string, RomSignatureObject> metadataContent = new Dictionary<string, RomSignatureObject>();
            if (options != null && options.ContainsKey("PathToDBFile") && options["PathToDBFile"] != null)
            {
                // load supplementary metadata files if they exist
                string metadataDeveloperFile = Path.Combine(options["PathToDBFile"].ToString(), "developer", Path.GetFileName(datFile));
                string metadataHardwareFile = Path.Combine(options["PathToDBFile"].ToString(), "enhancement_hw", Path.GetFileName(datFile));
                string metadataESRBFile = Path.Combine(options["PathToDBFile"].ToString(), "esrb", Path.GetFileName(datFile));
                string metadataGenreFile = Path.Combine(options["PathToDBFile"].ToString(), "genre", Path.GetFileName(datFile));
                string metadataMaxUsersFile = Path.Combine(options["PathToDBFile"].ToString(), "maxusers", Path.GetFileName(datFile));
                string metadataNoIntroFile = Path.Combine(options["PathToDBFile"].ToString(), "nointro", Path.GetFileName(datFile));
                string metadataPublisherFile = Path.Combine(options["PathToDBFile"].ToString(), "publisher", Path.GetFileName(datFile));
                string metadataReleaseMonthFile = Path.Combine(options["PathToDBFile"].ToString(), "releasemonth", Path.GetFileName(datFile));
                string metadataReleaseYearFile = Path.Combine(options["PathToDBFile"].ToString(), "releaseyear", Path.GetFileName(datFile));
                string metadataSerialFile = Path.Combine(options["PathToDBFile"].ToString(), "serial", Path.GetFileName(datFile));
                string metadataTosecFile = Path.Combine(options["PathToDBFile"].ToString(), "tosec", Path.GetFileName(datFile));
                string metadataRumbleFile = Path.Combine(options["PathToDBFile"].ToString(), "rumble", Path.GetFileName(datFile));
                string metadataHomebrewFile = Path.Combine(options["PathToDBFile"].ToString(), "homebrew", Path.GetFileName(datFile));
                string metadataHeaderedFile = Path.Combine(options["PathToDBFile"].ToString(), "headered", Path.GetFileName(datFile));
                string metadataHacksFile = Path.Combine(options["PathToDBFile"].ToString(), "hacks", Path.GetFileName(datFile));

                if (File.Exists(metadataDeveloperFile))
                {
                    metadataContent["developer"] = Parse(metadataDeveloperFile);
                }
                if (File.Exists(metadataHardwareFile))
                {
                    metadataContent["enhancement_hw"] = Parse(metadataHardwareFile);
                }
                if (File.Exists(metadataESRBFile))
                {
                    metadataContent["esrb"] = Parse(metadataESRBFile);
                }
                if (File.Exists(metadataGenreFile))
                {
                    metadataContent["genre"] = Parse(metadataGenreFile);
                }
                if (File.Exists(metadataMaxUsersFile))
                {
                    metadataContent["maxusers"] = Parse(metadataMaxUsersFile);
                }
                if (File.Exists(metadataNoIntroFile))
                {
                    metadataContent["nointro"] = Parse(metadataNoIntroFile);
                }
                if (File.Exists(metadataPublisherFile))
                {
                    metadataContent["publisher"] = Parse(metadataPublisherFile);
                }
                if (File.Exists(metadataReleaseMonthFile))
                {
                    metadataContent["releasemonth"] = Parse(metadataReleaseMonthFile);
                }
                if (File.Exists(metadataReleaseYearFile))
                {
                    metadataContent["releaseyear"] = Parse(metadataReleaseYearFile);
                }
                if (File.Exists(metadataSerialFile))
                {
                    metadataContent["serial"] = Parse(metadataSerialFile);
                }
                if (File.Exists(metadataTosecFile))
                {
                    metadataContent["tosec"] = Parse(metadataTosecFile);
                }
                if (File.Exists(metadataRumbleFile))
                {
                    metadataContent["rumble"] = Parse(metadataRumbleFile);
                }
                if (File.Exists(metadataHomebrewFile))
                {
                    metadataContent["homebrew"] = Parse(metadataHomebrewFile);
                }
                if (File.Exists(metadataHeaderedFile))
                {
                    metadataContent["headered"] = Parse(metadataHeaderedFile);
                }
                if (File.Exists(metadataHacksFile))
                {
                    metadataContent["hacks"] = Parse(metadataHacksFile);
                }
            }

            foreach (KeyValuePair<string, string> headerItem in headerData)
            {
                switch (headerItem.Key.ToLowerInvariant())
                {
                    case "name":
                        signatureObject.Name = headerItem.Value;
                        break;

                    case "description":
                        signatureObject.Description = headerItem.Value;
                        break;

                    case "version":
                        signatureObject.Version = headerItem.Value;
                        break;

                    case "author":
                        signatureObject.Author = headerItem.Value;
                        break;

                    case "homepage":
                        signatureObject.Homepage = headerItem.Value;
                        signatureObject.Url = ParseUri(headerItem.Value);
                        break;

                    case "email":
                        signatureObject.Email = headerItem.Value;
                        break;

                    case "id":
                        signatureObject.Id = headerItem.Value;
                        break;

                    case "category":
                        signatureObject.Category = headerItem.Value;
                        break;
                }
            }

            foreach (Dictionary<string, object> entry in entries)
            {
                RomSignatureObject.Game game = new RomSignatureObject.Game
                {
                    System = platformName,
                    Roms = new List<RomSignatureObject.Game.Rom>(),
                    flags = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
                    Country = new Dictionary<string, string>(),
                    Language = new Dictionary<string, string>()
                };

                if (entry.TryGetValue("attributes", out object? attributesObject) &&
                    attributesObject is Dictionary<string, string> attributes)
                {
                    foreach (KeyValuePair<string, string> attribute in attributes)
                    {
                        switch (attribute.Key.ToLowerInvariant())
                        {
                            case "name":
                                ApplyParsedGameName(game, attribute.Value);

                                break;

                            case "description":
                                game.Description = attribute.Value;
                                break;

                            case "year":
                            case "releaseyear":
                                game.Year = attribute.Value;
                                break;

                            case "publisher":
                            case "developer":
                            case "manufacturer":
                                game.Publisher = attribute.Value;
                                break;

                            default:
                                if (!game.flags.ContainsKey(attribute.Key))
                                {
                                    game.flags[attribute.Key] = attribute.Value;
                                }
                                break;
                        }
                    }
                }

                if (entry.TryGetValue("children", out object? childrenObject) &&
                    childrenObject is List<Dictionary<string, object>> children)
                {
                    foreach (Dictionary<string, object> child in children)
                    {
                        if (!child.TryGetValue("tag", out object? tagObject) || tagObject is not string childTag)
                        {
                            continue;
                        }

                        if (!string.Equals(childTag, "file", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(childTag, "rom", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        RomSignatureObject.Game.Rom rom = new RomSignatureObject.Game.Rom
                        {
                            SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.libretro,
                            Attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
                            Country = new Dictionary<string, string>(),
                            Language = new Dictionary<string, string>()
                        };

                        if (child.TryGetValue("attributes", out object? childAttributesObject) &&
                            childAttributesObject is Dictionary<string, string> childAttributes)
                        {
                            foreach (KeyValuePair<string, string> childAttribute in childAttributes)
                            {
                                switch (childAttribute.Key.ToLowerInvariant())
                                {
                                    case "name":
                                        rom.Name = childAttribute.Value;
                                        rom.RomType = null;

                                        // parse the rom name
                                        // remove the extension
                                        string romName = childAttribute.Value;
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
                                            // strip everything after the ")"
                                            string part;
                                            int closingParenIndex = romNamePart.IndexOf(')');
                                            if (closingParenIndex >= 0)
                                            {
                                                part = romNamePart.Substring(0, closingParenIndex).Trim();
                                            }
                                            else
                                            {
                                                part = romNamePart.Trim();
                                            }

                                            // check for a year (e.g., 1995) - but only if the game doesn't have a year yet
                                            if (String.IsNullOrEmpty(game.Year))
                                            {
                                                // check if this part is a year (e.g., 1995)
                                                if (part.Length == 4 && int.TryParse(part, out var year))
                                                {
                                                    game.Year = year.ToString();

                                                    // move to the next part
                                                    continue;
                                                }
                                            }

                                            // we're checking roms, so we're not interested in regions or languages, only disk numbers and disk names
                                            if (part.StartsWith("disc", StringComparison.OrdinalIgnoreCase) || part.StartsWith("disk", StringComparison.OrdinalIgnoreCase) || part.StartsWith("side", StringComparison.OrdinalIgnoreCase))
                                            {
                                                // disk number
                                                rom.RomType = RomSignatureObject.Game.Rom.RomTypes.Disc;
                                                rom.RomTypeMedia = part;
                                                readyForDiskName = true;
                                            }
                                            else
                                            {
                                                if (readyForDiskName)
                                                {
                                                    // disk name
                                                    rom.MediaLabel = part;
                                                }
                                                else
                                                {
                                                    // check for language or country
                                                    var language = LanguageLookup.ParseLanguageString(part);
                                                    if (language != null)
                                                    {
                                                        if (!rom.Language.ContainsKey(language.Value.Key))
                                                        {
                                                            rom.Language[language.Value.Key] = language.Value.Value;
                                                            continue;
                                                        }
                                                    }

                                                    var country = CountryLookup.ParseCountryString(part);
                                                    if (country != null)
                                                    {
                                                        if (!rom.Country.ContainsKey(country.Value.Key))
                                                        {
                                                            rom.Country[country.Value.Key] = country.Value.Value;
                                                            continue;
                                                        }
                                                    }

                                                    if (developmentStatusFound == false)
                                                    {
                                                        var devStatus = DevelopmentStatusLookup.ParseStatusString(part);
                                                        if (devStatus != null)
                                                        {
                                                            rom.DevelopmentStatus = devStatus.Code;
                                                            developmentStatusFound = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        break;

                                    case "size":
                                        if (UInt64.TryParse(childAttribute.Value, out ulong sizeValue))
                                        {
                                            rom.Size = sizeValue;
                                        }
                                        break;

                                    case "crc":
                                        rom.Crc = childAttribute.Value;
                                        break;

                                    case "md5":
                                        rom.Md5 = childAttribute.Value.ToLowerInvariant();
                                        break;

                                    case "sha1":
                                        rom.Sha1 = childAttribute.Value.ToLowerInvariant();
                                        break;

                                    case "sha256":
                                        rom.Sha256 = childAttribute.Value.ToLowerInvariant();
                                        break;

                                    case "status":
                                        rom.Status = childAttribute.Value;
                                        break;

                                    default:
                                        if (!rom.Attributes.ContainsKey(childAttribute.Key))
                                        {
                                            rom.Attributes[childAttribute.Key] = childAttribute.Value;
                                        }
                                        break;
                                }
                            }
                        }

                        // search supplementary metadata for this game by searching for the CRC, MD5, or SHA1 hash in the supplementary metadata files
                        if (metadataContent.Count > 0)
                        {
                            Dictionary<string, RomSignatureObject.Game> supplementaryMetadataByHash = new Dictionary<string, RomSignatureObject.Game>();
                            foreach (var metadata in metadataContent)
                            {
                                // search for this game's CRC, MD5, or SHA1 in the supplementary metadata
                                RomSignatureObject.Game? gameMetadata = metadata.Value.Games.FirstOrDefault(g => g.Roms.Any(r => r.Crc == rom.Crc || r.Md5 == rom.Md5 || r.Sha1 == rom.Sha1));
                                if (gameMetadata != null)
                                {
                                    supplementaryMetadataByHash[metadata.Key] = gameMetadata;
                                }
                            }

                            if (supplementaryMetadataByHash.Count > 0)
                            {
                                foreach (var supplementaryMetadata in supplementaryMetadataByHash)
                                {
                                    // merge each supplementary metadata game into the current game object
                                    // this could involve copying over attributes, roms, etc.
                                    switch (supplementaryMetadata.Key)
                                    {
                                        case "developer":
                                            if (string.IsNullOrWhiteSpace(game.Publisher))
                                            {
                                                game.Publisher = supplementaryMetadata.Value.Publisher;
                                            }
                                            break;
                                        case "releaseyear":
                                            if (string.IsNullOrWhiteSpace(game.Year))
                                            {
                                                if (supplementaryMetadata.Value.Year != null)
                                                {
                                                    game.Year = supplementaryMetadata.Value.Year;
                                                    game.flags["releaseyear"] = supplementaryMetadata.Value.Year;
                                                }
                                            }
                                            break;
                                        default:
                                            // copy any other flags from the supplementary metadata if they don't already exist in the game object
                                            foreach (var flag in supplementaryMetadata.Value.flags)
                                            {
                                                if (!game.flags.ContainsKey(flag.Key))
                                                {
                                                    game.flags[flag.Key] = flag.Value;
                                                }
                                            }
                                            break;
                                    }
                                }
                                // Console.WriteLine($"Merged supplementary metadata for game: {game.Name}");
                            }
                        }

                        game.Roms.Add(rom);
                    }
                }

                signatureObject.Games.Add(game);
            }

            return signatureObject;
        }

        private static void ApplyParsedGameName(RomSignatureObject.Game game, string archiveName)
        {
            ParsedTdcName parsed = ParseTdcArchiveName(archiveName);

            game.Name = parsed.Title;

            if (string.IsNullOrWhiteSpace(game.Year) && !string.IsNullOrWhiteSpace(parsed.Year))
            {
                game.Year = parsed.Year;
            }

            if (string.IsNullOrWhiteSpace(game.Publisher) && !string.IsNullOrWhiteSpace(parsed.Publisher))
            {
                game.Publisher = parsed.Publisher;
            }

            if (string.IsNullOrWhiteSpace(game.Category) && !string.IsNullOrWhiteSpace(parsed.Category))
            {
                game.Category = parsed.Category;
            }

            if (game.Country == null)
            {
                game.Country = new Dictionary<string, string>();
            }

            foreach (KeyValuePair<string, string> country in parsed.Countries)
            {
                if (!game.Country.ContainsKey(country.Key))
                {
                    game.Country[country.Key] = country.Value;
                }
            }

            if (game.Country.Count > 0)
            {
                game.CountryString = string.Join(",", game.Country.Keys);
            }

            if (game.Language == null)
            {
                game.Language = new Dictionary<string, string>();
            }

            foreach (KeyValuePair<string, string> language in parsed.Languages)
            {
                if (!game.Language.ContainsKey(language.Key))
                {
                    game.Language[language.Key] = language.Value;
                }
            }

            if (game.Language.Count > 0)
            {
                game.LanguageString = string.Join(",", game.Language.Keys);
            }

            game.flags["archiveName"] = parsed.RawName;

            if (parsed.Tags.Count > 0)
            {
                game.flags["tags"] = parsed.Tags;
            }

            if (parsed.Qualifiers.Count > 0)
            {
                if (parsed.Qualifiers.Contains("Demo"))
                {
                    game.Demo = RomSignatureObject.Game.DemoTypes.demo;
                }

                game.flags["qualifiers"] = parsed.Qualifiers;
            }

            if (!string.IsNullOrWhiteSpace(parsed.Version))
            {
                game.flags["version"] = parsed.Version;
            }

            if (!string.IsNullOrWhiteSpace(parsed.Release))
            {
                game.flags["release"] = parsed.Release;
            }

            if (parsed.KnownGood)
            {
                game.flags["knownGoodDump"] = true;
            }
        }

        private static ParsedTdcName ParseTdcArchiveName(string archiveName)
        {
            ParsedTdcName result = new ParsedTdcName
            {
                RawName = archiveName
            };

            if (archiveName.StartsWith("3D Fight", StringComparison.OrdinalIgnoreCase) || archiveName.StartsWith("3 Guerra Mundial", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("ROM name");
            }

            string working = RemoveArchiveExtension(archiveName).Trim();

            // split working into tokens - each token may represent a language, country, qualifier, or other metadata, and is contained within parentheses or brackets.
            // working = new string(working.Reverse().ToArray());
            string[] tokens = working.Split(new[] { '(', ')', '[', ']' }).Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            // process tokens backwards to correctly associate metadata with the main title.
            tokens = tokens.Reverse().ToArray();

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                // don't process the last token - it's the name of the game itself.
                if (i == tokens.Length - 1)
                {
                    break;
                }

                if (TryParseLanguageToken(token, out KeyValuePair<string, string>? language))
                {
                    if (language.HasValue)
                    {
                        result.Languages[language.Value.Key] = language.Value.Value;
                        continue; // move to the next token after successfully parsing a language token
                    }
                }

                if (TryParseCountryToken(token, out KeyValuePair<string, string>? country))
                {
                    if (country.HasValue)
                    {
                        result.Countries[country.Value.Key] = country.Value.Value;
                        continue; // move to the next token after successfully parsing a country token
                    }
                }

                if (token == "Demo")
                {
                    result.Qualifiers.Add("Demo");
                    continue; // move to the next token after recognizing a demo token
                }

                Match revMatch = Regex.Match(token, "^Rev\\.?\\s*([^\\s\\[\\]()]+)$", RegexOptions.IgnoreCase);
                if (revMatch.Success)
                {
                    result.Release = revMatch.Groups[1].Value;
                    continue;
                }

                Match versionMatch = Regex.Match(result.Title, "\\s+v([^\\s\\[\\]()]+)", RegexOptions.IgnoreCase);
                if (versionMatch.Success)
                {
                    result.Version = versionMatch.Groups[1].Value;
                    continue;
                }

                // If the token was not recognized as a language or country, it's likely the developer
                result.Publisher = token; // assign the unrecognized token as the developer
            }

            result.Title = tokens[tokens.Length - 1]; // the last token is the main title

            return result;
        }

        private static string RemoveArchiveExtension(string name)
        {
            string value = name.Trim();
            if (value.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 4);
            }

            return value.Trim();
        }

        private static bool TryParseLanguageToken(string token, out KeyValuePair<string, string>? language)
        {
            language = null;

            string trimmed = token.Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            if (Regex.IsMatch(trimmed, "^Multi-\\d+$", RegexOptions.IgnoreCase))
            {
                language = new KeyValuePair<string, string>(trimmed.ToLowerInvariant(), trimmed);
                return true;
            }

            KeyValuePair<string, string>? parsedLanguage = LanguageLookup.ParseLanguageString(trimmed);
            if (parsedLanguage.HasValue)
            {
                language = parsedLanguage;
                return true;
            }

            // TDC occasionally uses non-ISO or legacy short language identifiers such as (Se) or (Kr).
            if (Regex.IsMatch(trimmed, "^[A-Za-z]{2}$"))
            {
                language = new KeyValuePair<string, string>(trimmed.ToLowerInvariant(), trimmed);
                return true;
            }

            return false;
        }

        private static bool TryParseCountryToken(string token, out KeyValuePair<string, string>? country)
        {
            country = null;

            string trimmed = token.Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            if (Regex.IsMatch(trimmed, "^Multi-\\d+$", RegexOptions.IgnoreCase))
            {
                country = new KeyValuePair<string, string>(trimmed.ToLowerInvariant(), trimmed);
                return true;
            }

            KeyValuePair<string, string>? parsedCountry = CountryLookup.ParseCountryString(trimmed);
            if (parsedCountry.HasValue)
            {
                country = parsedCountry;
                return true;
            }

            // TDC occasionally uses non-ISO or legacy short country identifiers such as (Se) or (Kr).
            if (Regex.IsMatch(trimmed, "^[A-Za-z]{2}$"))
            {
                country = new KeyValuePair<string, string>(trimmed.ToLowerInvariant(), trimmed);
                return true;
            }

            return false;
        }

        private static bool TryPopTrailingGroup(ref string value, char openChar, char closeChar, out string? groupContent)
        {
            groupContent = null;

            string working = value.TrimEnd();
            if (working.Length == 0 || working[^1] != closeChar)
            {
                return false;
            }

            int depth = 0;
            for (int index = working.Length - 1; index >= 0; index--)
            {
                char current = working[index];
                if (current == closeChar)
                {
                    depth++;
                    continue;
                }

                if (current == openChar)
                {
                    depth--;
                    if (depth == 0)
                    {
                        groupContent = working.Substring(index + 1, working.Length - index - 2);
                        value = working.Substring(0, index).TrimEnd();
                        return true;
                    }
                }
            }

            return false;
        }

        private static Uri? ParseUri(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string uriValue = value.Trim();
            if (!uriValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !uriValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                uriValue = "http://" + uriValue;
            }

            if (Uri.TryCreate(uriValue, UriKind.Absolute, out Uri? uri))
            {
                return uri;
            }

            return null;
        }
    }
}