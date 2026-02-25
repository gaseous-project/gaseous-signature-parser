using System.Reflection;
using gaseous_signature_parser.models;

namespace gaseous_signature_parser.classes
{
    public static class DevelopmentStatusLookup
    {
        static Dictionary<string, string>? statusList = null;

        static void LoadStatuses()
        {
            if (statusList == null)
            {
                statusList = new Dictionary<string, string>();

                // load resources
                var assembly = Assembly.GetExecutingAssembly();

                // load status list
                List<string> statuses = new List<string>();
                string resourceName = "gaseous_signature_parser.support.parsers.tosec.DevelopmentStatus.txt";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    statuses = reader.ReadToEnd().Split(Environment.NewLine).ToList<string>();
                }

                // load status table into dictionary
                statusList.Clear();
                foreach (string status in statuses)
                {
                    string[] statusSplit = status.Split(",");
                    if (statusSplit.Length == 2)
                    {
                        // add to dictionary
                        if (statusSplit[0].Trim() == "" || statusSplit[1].Trim() == "")
                        {
                            continue;
                        }

                        statusList.Add(statusSplit[0].Trim(), statusSplit[1].Trim());
                    }
                }
            }
        }

        public static DevelopmentStatusItem? ParseStatusString(string statusString)
        {
            LoadStatuses();

            // search for the status item
            DevelopmentStatusItem? returnItem = null;
            // check if statusString is a code
            if (statusList.ContainsKey(statusString.ToLower()))
            {
                returnItem = new DevelopmentStatusItem
                {
                    Code = statusString,
                    Name = statusList[statusString.ToLower()]
                };
            }
            else
            {
                // check if statusString is a name
                var item = statusList.FirstOrDefault(x => x.Value.ToLower() == statusString.ToLower());
                if (item.Key != null)
                {
                    returnItem = new DevelopmentStatusItem
                    {
                        Code = item.Key,
                        Name = item.Value
                    };
                }
            }

            // check if null
            if (returnItem == null)
            {
                // if null, check if the string starts with one of our keys
                var item = statusList.FirstOrDefault(x => statusString.StartsWith(x.Key, StringComparison.OrdinalIgnoreCase));
                if (item.Key != null)
                {
                    returnItem = new DevelopmentStatusItem
                    {
                        Code = item.Key,
                        Name = item.Value
                    };
                }
                else
                {
                    return null;
                }
            }

            return returnItem;
        }
    }
}