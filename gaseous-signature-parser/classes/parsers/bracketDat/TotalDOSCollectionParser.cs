using gaseous_signature_parser.models.RomSignatureObject;
using System.IO;
using System.Text.RegularExpressions;

namespace gaseous_signature_parser.classes.bracketparsers
{
    public class TotalDOSCollectionParser : BaseParser
    {
        private const string HeaderTag = "DOSCenter";
        private static readonly string[] EntryTags = { "game" };
        private static readonly string[] ChildTags = { "file", "rom" };

        private sealed class ParsedTdcName
        {
            public string Title { get; set; } = string.Empty;
            public string? Year { get; set; }
            public string? Publisher { get; set; }
            public string? Category { get; set; }
            public List<string> Qualifiers { get; set; } = new List<string>();
            public List<string> Tags { get; set; } = new List<string>();
            public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string>();
            public string? Version { get; set; }
            public string? Release { get; set; }
            public bool KnownGood { get; set; }
            public string RawName { get; set; } = string.Empty;
        }

        public TotalDOSCollectionParser()
        {

        }

        public override parser.SignatureParser GetDatType(string dat)
        {
            // load the header data and check for the presence of a key-value pair with the key "homepage" and the value "http://totaldoscollection.org"
            Dictionary<string, string> headerData = ExtractHeaderData(dat, HeaderTag);
            if (headerData.TryGetValue("homepage", out string? typeValue) && string.Equals(typeValue, "http://totaldoscollection.org", StringComparison.OrdinalIgnoreCase))
            {
                return parser.SignatureParser.TotalDOSCollection;
            }
            return parser.SignatureParser.Unknown;
        }

        public override RomSignatureObject Parse(string datFile, Dictionary<string, object>? options = null)
        {
            using FileStream datStream = File.OpenRead(datFile);
            if (datStream.Length == 0)
            {
                throw new ArgumentException("The DAT file is empty.", nameof(datFile));
            }

            var hashes = Hash.GenerateHashes(datStream);
            Dictionary<string, string> headerData = ExtractHeaderData(datFile, HeaderTag);
            List<Dictionary<string, object>> entries = ExtractDataEntries(datFile, EntryTags, ChildTags);

            RomSignatureObject signatureObject = new RomSignatureObject
            {
                SourceType = "TotalDOSCollection",
                SourceMd5 = hashes.md5,
                SourceSHA1 = hashes.sha1,
                Games = new List<RomSignatureObject.Game>()
            };

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
                    System = "DOS",
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
                            SignatureSource = RomSignatureObject.Game.Rom.SignatureSourceType.TotalDOSCollection,
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

            string working = RemoveArchiveExtension(archiveName).Trim();

            if (TryPopTrailingGroup(ref working, '[', ']', out string? endTag) && string.Equals(endTag, "!", StringComparison.OrdinalIgnoreCase))
            {
                result.KnownGood = true;
            }
            else if (!string.IsNullOrWhiteSpace(endTag))
            {
                // Put it back if it was not a known-good marker.
                working = (working + " [" + endTag + "]").Trim();
            }

            if (TryPopTrailingGroup(ref working, '[', ']', out string? categoryGroup))
            {
                result.Category = categoryGroup?.Trim();
            }

            if (TryPopTrailingGroup(ref working, '(', ')', out string? publisherGroup))
            {
                result.Publisher = publisherGroup?.Trim();
            }

            if (TryPopTrailingGroup(ref working, '(', ')', out string? yearGroup))
            {
                string parsedYear = (yearGroup ?? string.Empty).Trim();
                if (Regex.IsMatch(parsedYear, "^(19|20)\\d{2}$") || parsedYear.Equals("19xx", StringComparison.OrdinalIgnoreCase) || parsedYear.Equals("20xx", StringComparison.OrdinalIgnoreCase))
                {
                    result.Year = parsedYear;
                }
                else
                {
                    // Not a year; put it back.
                    working = (working + " (" + parsedYear + ")").Trim();
                }
            }

            List<string> trailingTags = new List<string>();
            while (TryPopTrailingGroup(ref working, '[', ']', out string? tagGroup))
            {
                string tag = (tagGroup ?? string.Empty).Trim();
                if (tag.Length > 0)
                {
                    trailingTags.Add(tag);
                }
            }
            trailingTags.Reverse();
            result.Tags = trailingTags;

            List<string> trailingParenGroups = new List<string>();
            while (TryPopTrailingGroup(ref working, '(', ')', out string? parenGroup))
            {
                string token = (parenGroup ?? string.Empty).Trim();
                if (token.Length > 0)
                {
                    trailingParenGroups.Add(token);
                }
            }
            trailingParenGroups.Reverse();

            foreach (string token in trailingParenGroups)
            {
                if (TryParseLanguageToken(token, out KeyValuePair<string, string>? languagePair) && languagePair.HasValue)
                {
                    if (!result.Languages.ContainsKey(languagePair.Value.Key))
                    {
                        result.Languages[languagePair.Value.Key] = languagePair.Value.Value;
                    }
                }
                else
                {
                    result.Qualifiers.Add(token);
                }
            }

            result.Title = working.Trim();
            ParseVersionAndRelease(result);

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

        private static void ParseVersionAndRelease(ParsedTdcName result)
        {
            if (string.IsNullOrWhiteSpace(result.Title))
            {
                return;
            }

            Match versionMatch = Regex.Match(result.Title, "\\s+v([^\\s\\[\\]()]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                result.Version = versionMatch.Groups[1].Value;
            }

            Match releaseMatch = Regex.Match(result.Title, "\\s+r([^\\s\\[\\]()]+)", RegexOptions.IgnoreCase);
            if (releaseMatch.Success)
            {
                result.Release = releaseMatch.Groups[1].Value;
            }
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