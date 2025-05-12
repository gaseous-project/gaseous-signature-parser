using System.Reflection;

namespace gaseous_signature_parser.classes
{
    public static class CountryLookup
    {
        static Dictionary<string, models.CountryItem>? countryList = null;

        static void LoadCountries()
        {
            if (countryList == null)
            {
                countryList = new Dictionary<string, models.CountryItem>();

                // load resources
                var assembly = Assembly.GetExecutingAssembly();

                // load countries list
                List<string> countries = new List<string>();
                string resourceName = "gaseous_signature_parser.support.parsers.tosec.Country.txt";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    countries = reader.ReadToEnd().Split(Environment.NewLine).ToList<string>();
                }

                // load country table into dictionary
                countryList.Clear();
                foreach (string country in countries)
                {
                    string[] countrySplit = country.Split(",");
                    if (countrySplit.Length == 2)
                    {
                        // add to dictionary
                        if (countrySplit[0].Trim() == "" || countrySplit[1].Trim() == "")
                        {
                            continue;
                        }

                        models.CountryItem countryItem = new models.CountryItem
                        {
                            Code = countrySplit[0].Trim(),
                            Name = countrySplit[1].Trim()
                        };
                        countryList.Add(countryItem.Code, countryItem);
                    }
                }
            }
        }

        public static KeyValuePair<string, string>? ParseCountryString(string countryString)
        {
            LoadCountries();

            // search for the country item
            models.CountryItem? returnItem = null;
            // check if countryString is a code
            if (countryList.ContainsKey(countryString))
            {
                returnItem = countryList[countryString];
            }
            else
            {
                // check if countryString is a name
                var item = countryList.FirstOrDefault(x => x.Value.Name == countryString);
                if (item.Key != null)
                {
                    returnItem = item.Value;
                }
            }

            // check if null
            if (returnItem == null)
            {
                return null;
            }

            // check if countryString is a redirection
            if (returnItem.Redirection == "")
            {
                return new KeyValuePair<string, string>(returnItem.Code, returnItem.Name);
            }
            else
            {
                // check if redirection is a code
                if (countryList.ContainsKey(returnItem.Redirection))
                {
                    returnItem = countryList[returnItem.Redirection];
                    return new KeyValuePair<string, string>(returnItem.Code, returnItem.Name);
                }
                else
                {
                    // invalid response, throw an error
                    throw new Exception("Invalid country code or name: " + countryString);
                }
            }
        }
    }
}