using System;
using System.Xml;
using System.IO;
using gaseous_signature_parser.models.RomSignatureObject;

namespace gaseous_signature_parser.classes.parsers
{
    /// <summary>
    /// Base class for all signature parsers providing common XML parsing and data extraction functionality
    /// </summary>
    public abstract class BaseParser : IParser
    {
        /// <summary>
        /// Parse the XML file and return a RomSignatureObject
        /// </summary>
        /// <param name="xmlFile">Path to the XML signature file</param>
        /// <param name="options">Optional parameters specific to the parser implementation</param>
        /// <returns>Parsed RomSignatureObject</returns>
        public abstract RomSignatureObject Parse(string xmlFile, Dictionary<string, object>? options = null);

        /// <summary>
        /// Determine if the provided XML document matches this parser's signature type
        /// </summary>
        /// <param name="xml">XML document to check</param>
        /// <returns>SignatureParser type if matched, otherwise Unknown</returns>
        public abstract parser.SignatureParser GetXmlType(XmlDocument xml);

        /// <summary>
        /// Initialize from XML file, calculating hashes and loading the document
        /// </summary>
        /// <param name="xmlFile">Path to the XML file</param>
        /// <param name="md5Hash">Output: MD5 hash of the file</param>
        /// <param name="sha1Hash">Output: SHA1 hash of the file</param>
        /// <returns>Loaded XmlDocument</returns>
        protected XmlDocument InitializeFromFile(string xmlFile, out string md5Hash, out string sha1Hash)
        {
            var xmlStream = File.OpenRead(xmlFile);
            var hashes = Hash.GenerateHashes(xmlStream);
            md5Hash = hashes.md5;
            sha1Hash = hashes.sha1;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlFile);
            return xmlDocument;
        }

        /// <summary>
        /// Parse the header section of the XML file and populate the RomSignatureObject
        /// </summary>
        /// <param name="signatureObject">The RomSignatureObject to populate</param>
        /// <param name="headerNode">The header XML node</param>
        /// <param name="sourceType">The type of source (e.g., "Generic", "TOSEC", "Redump")</param>
        /// <param name="md5Hash">MD5 hash of the source file</param>
        /// <param name="sha1Hash">SHA1 hash of the source file</param>
        protected void ParseHeader(RomSignatureObject signatureObject, XmlNode headerNode, string sourceType, string md5Hash, string sha1Hash)
        {
            signatureObject.SourceType = sourceType;
            signatureObject.SourceMd5 = md5Hash;
            signatureObject.SourceSHA1 = sha1Hash;

            if (headerNode == null)
                return;

            foreach (XmlNode childNode in headerNode.ChildNodes)
            {
                switch (childNode.Name.ToLower())
                {
                    case "name":
                        signatureObject.Name = childNode.InnerText;
                        break;

                    case "description":
                        signatureObject.Description = childNode.InnerText;
                        break;

                    case "category":
                        signatureObject.Category = childNode.InnerText;
                        break;

                    case "version":
                        signatureObject.Version = childNode.InnerText;
                        break;

                    case "author":
                        signatureObject.Author = childNode.InnerText;
                        break;

                    case "email":
                        signatureObject.Email = childNode.InnerText;
                        break;

                    case "homepage":
                        signatureObject.Homepage = childNode.InnerText;
                        break;

                    case "url":
                        signatureObject.Url = ParseUri(childNode.InnerText);
                        break;

                    case "id":
                        signatureObject.Id = childNode.InnerText;
                        break;
                }
            }
        }

        /// <summary>
        /// Safely parse a URI string, returning null if invalid
        /// </summary>
        /// <param name="uriString">The URI string to parse</param>
        /// <returns>Parsed Uri or null if invalid</returns>
        protected Uri? ParseUri(string uriString)
        {
            try
            {
                // Handle cases where http:// or https:// is missing
                if (!uriString.StartsWith("http://") && !uriString.StartsWith("https://"))
                {
                    uriString = "http://" + uriString;
                }
                return new Uri(uriString);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create and initialize a Game object with default collections
        /// </summary>
        /// <returns>A new Game object with initialized collections</returns>
        protected RomSignatureObject.Game CreateGameObject()
        {
            return new RomSignatureObject.Game
            {
                Roms = new List<RomSignatureObject.Game.Rom>(),
                flags = new Dictionary<string, object>(),
                Language = new Dictionary<string, string>(),
                Country = new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Parse ROM attributes from an XML node, extracting standard hash and size values
        /// </summary>
        /// <param name="romNode">The XML node containing ROM attributes</param>
        /// <param name="signatureSource">The source type for this ROM</param>
        /// <returns>A populated Rom object</returns>
        protected RomSignatureObject.Game.Rom ParseRomAttributes(XmlNode romNode, RomSignatureObject.Game.Rom.SignatureSourceType signatureSource)
        {
            RomSignatureObject.Game.Rom rom = new RomSignatureObject.Game.Rom
            {
                Attributes = new Dictionary<string, object>(),
                SignatureSource = signatureSource
            };
            rom.Country = new Dictionary<string, string>();
            rom.Language = new Dictionary<string, string>();

            if (romNode.Attributes == null)
            {
                return rom;
            }

            foreach (XmlAttribute romAttribute in romNode.Attributes)
            {
                switch (romAttribute.Name.ToLower())
                {
                    case "name":
                        rom.Name = romAttribute.Value;

                        // extract values from file name - they'll be surrounded by parentheses
                        string[] nameParts = romAttribute.Value.Split(new string[] { "(" }, StringSplitOptions.None);
                        foreach (string namePart in nameParts)
                        {
                            string[] detailsPart = namePart.Split(new string[] { ")" }, StringSplitOptions.None);
                            if (detailsPart.Length > 0)
                            {
                                string detailsString = detailsPart[0];

                                // check if it's a country code or country name
                                string[] countryDetailsPart = detailsString.Split(new string[] { "," }, StringSplitOptions.None);
                                foreach (string countryDetail in countryDetailsPart)
                                {
                                    string trimmedCountryDetail = countryDetail.Trim();
                                    KeyValuePair<string, string>? countryItem = CountryLookup.ParseCountryString(trimmedCountryDetail);
                                    if (countryItem != null)
                                    {
                                        if (countryItem.HasValue && !rom.Country.ContainsKey(countryItem.Value.Key))
                                        {
                                            rom.Country.Add(countryItem.Value.Key, countryItem.Value.Value);
                                        }
                                    }
                                }

                                // check if it's a language code or language name
                                string[] languageDetailsPart = detailsString.Split(new string[] { "," }, StringSplitOptions.None);
                                foreach (string languageDetail in languageDetailsPart)
                                {
                                    string trimmedLanguageDetail = languageDetail.Trim();
                                    KeyValuePair<string, string>? languageItem = LanguageLookup.ParseLanguageString(trimmedLanguageDetail);
                                    if (languageItem != null)
                                    {
                                        if (languageItem.HasValue && !rom.Language.ContainsKey(languageItem.Value.Key))
                                        {
                                            rom.Language.Add(languageItem.Value.Key, languageItem.Value.Value);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case "size":
                        if (UInt64.TryParse(romAttribute.Value, out ulong sizeValue))
                        {
                            rom.Size = sizeValue;
                        }
                        else
                        {
                            rom.Size = 0;
                        }
                        break;

                    case "crc":
                        rom.Crc = romAttribute.Value;
                        break;

                    case "md5":
                        rom.Md5 = romAttribute.Value;
                        break;

                    case "sha1":
                        rom.Sha1 = romAttribute.Value;
                        break;

                    case "sha256":
                        rom.Sha256 = romAttribute.Value;
                        break;

                    case "status":
                        rom.Status = romAttribute.Value;
                        break;

                    default:
                        if (!rom.Attributes.ContainsKey(romAttribute.Name))
                        {
                            rom.Attributes.Add(romAttribute.Name, romAttribute.Value);
                        }
                        break;
                }
            }

            return rom;
        }

        /// <summary>
        /// Add a flag to a game object if it doesn't already exist
        /// </summary>
        /// <param name="gameObject">The game object to update</param>
        /// <param name="flagName">The flag name</param>
        /// <param name="flagValue">The flag value</param>
        protected void AddGameFlag(RomSignatureObject.Game gameObject, string flagName, object flagValue)
        {
            if (!gameObject.flags.ContainsKey(flagName))
            {
                gameObject.flags.Add(flagName, flagValue);
            }
        }

        /// <summary>
        /// Add a country to a collection if it doesn't already exist
        /// </summary>
        /// <param name="countryDict">The country dictionary to update</param>
        /// <param name="countryCode">The country code</param>
        /// <param name="countryName">The country name</param>
        protected void AddCountry(Dictionary<string, string> countryDict, string countryCode, string countryName)
        {
            if (!countryDict.ContainsKey(countryCode))
            {
                countryDict.Add(countryCode, countryName);
            }
        }

        /// <summary>
        /// Add a language to a collection if it doesn't already exist
        /// </summary>
        /// <param name="languageDict">The language dictionary to update</param>
        /// <param name="languageCode">The language code</param>
        /// <param name="languageName">The language name</param>
        protected void AddLanguage(Dictionary<string, string> languageDict, string languageCode, string languageName)
        {
            if (!languageDict.ContainsKey(languageCode))
            {
                languageDict.Add(languageCode, languageName);
            }
        }
    }
}
