using System.Reflection;

namespace gaseous_signature_parser.classes
{
    public static class LanguageLookup
    {
        private static Dictionary<string, models.LanguageItem> languageList = null;

        static void LoadLanguages()
        {
            if (languageList == null)
            {
                languageList = new Dictionary<string, models.LanguageItem>();

                // load resources
                var assembly = Assembly.GetExecutingAssembly();

                // load languages list
                List<string> languages = new List<string>();
                string resourceName = "gaseous_signature_parser.support.parsers.tosec.Language.txt";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    languages = reader.ReadToEnd().Split(Environment.NewLine).ToList<string>();
                }

                // load languages table into dictionary
                languageList.Clear();
                foreach (string language in languages)
                {
                    string[] languageSplit = language.Split(",");
                    if (languageSplit.Length == 2)
                    {
                        // add to dictionary
                        if (languageSplit[0].Trim() == "" || languageSplit[1].Trim() == "")
                        {
                            continue;
                        }

                        models.LanguageItem languageItem = new models.LanguageItem
                        {
                            Code = languageSplit[0].Trim(),
                            Name = languageSplit[1].Trim()
                        };
                        languageList.Add(languageItem.Code, languageItem);
                    }
                }
            }
        }

        public static KeyValuePair<string, string>? ParseLanguageString(string languageString)
        {
            LoadLanguages();

            // search for the language item
            models.LanguageItem? returnItem = null;
            // check if languageString is a code
            if (languageList.ContainsKey(languageString.ToLower()))
            {
                returnItem = languageList[languageString.ToLower()];
            }
            else
            {
                // check if languageString is a name
                var item = languageList.Values.FirstOrDefault(x => x.Name.Equals(languageString, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    returnItem = item;
                }
            }

            // check if null
            if (returnItem == null)
            {
                return null;
            }

            // check if languageString is a redirection
            if (returnItem.Redirection == "")
            {
                return new KeyValuePair<string, string>(returnItem.Code, returnItem.Name);
            }
            else
            {
                // check if redirection is a code
                if (languageList.ContainsKey(returnItem.Redirection))
                {
                    returnItem = languageList[returnItem.Redirection];
                    return new KeyValuePair<string, string>(returnItem.Code, returnItem.Name);
                }
                else
                {
                    // invalid response, throw an error
                    throw new Exception("Invalid language code or name: " + languageString);
                }
            }
        }
    }
}