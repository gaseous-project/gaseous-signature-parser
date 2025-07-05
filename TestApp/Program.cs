// parse command line
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using gaseous_signature_parser.models.RomSignatureObject;
using gaseous_signature_parser;

string[] commandLineArgs = Environment.GetCommandLineArgs();

string scanPath = "./";
string datPath = "";
string? dbPath = null;
string? outpath = null;

string inArgument = "";
foreach (string commandLineArg in commandLineArgs)
{
    if (commandLineArg != commandLineArgs[0])
    {
        if (inArgument == "")
        {
            switch (commandLineArg.ToLower())
            {

                case "-scanpath":
                case "-datpath":
                case "-dbpath":
                case "-outpath":
                    inArgument = commandLineArg.ToLower();
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (inArgument)
            {
                case "-scanpath":
                    scanPath = commandLineArg;
                    break;
                case "-datpath":
                    datPath = commandLineArg;
                    break;
                case "-dbpath":
                    dbPath = commandLineArg;
                    break;
                case "-outpath":
                    outpath = commandLineArg;
                    break;
                default:
                    break;
            }
            inArgument = "";
        }
    }
}

scanPath = Path.GetFullPath(scanPath);
Console.WriteLine("ROM search path: " + scanPath);

List<RomSignatureObject> romSignatures = new List<RomSignatureObject>();
System.Collections.ArrayList availablePlatforms = new System.Collections.ArrayList();

// load DAT XML files
if (datPath != null && datPath.Length > 0)
{
    datPath = Path.GetFullPath(datPath);
    Console.WriteLine("DATs are enabled");
    Console.WriteLine("DAT XML search path: " + datPath);
    if (dbPath != null)
    {
        Console.WriteLine("DB XML search path: " + dbPath);
    }

    string[] datPathContents = Directory.GetFiles(datPath, "*.dat");
    string[] dbPathContents = new string[0];
    if (dbPath != null)
    {
        dbPathContents = Directory.GetFiles(dbPath, "*.xml");
    }

    int lastCLILineLength = 0;
    for (UInt16 i = 0; i < datPathContents.Length; ++i)
    {
        string datPathFile = datPathContents[i];

        parser Parser = new parser();
        try
        {
            string? dbPathFile = null;
            string dbPathName = "";
            if (dbPathContents.Length > 0)
            {
                foreach (string dbFile in dbPathContents)
                {
                    string testFileName = Path.GetFileNameWithoutExtension(dbFile.Replace(" (DB Export)", ""));
                    if (testFileName == Path.GetFileNameWithoutExtension(datPathFile))
                    {
                        // match!
                        dbPathFile = dbFile;
                        dbPathName = Path.GetFileName(dbFile);
                    }
                }
            }

            RomSignatureObject datObject = Parser.ParseSignatureDAT(datPathFile, dbPathFile);

            if (datObject != null)
            {
                string statusOutput = (i + 1) + " / " + datPathContents.Length + " : " + Path.GetFileName(datPathFile) + " " + dbPathName;
                Console.Write("\r " + statusOutput.PadRight(lastCLILineLength, ' ') + "\r");
                lastCLILineLength = statusOutput.Length;

                foreach (RomSignatureObject.Game gameRom in datObject.Games)
                {
                    if (!availablePlatforms.Contains(gameRom.System))
                    {
                        availablePlatforms.Add(gameRom.System);
                    }
                }

                romSignatures.Add(datObject);

                if (outpath != null)
                {
                    if (Directory.Exists(outpath))
                    {
                        // write datObject as JSON to outpath
                        var jsonSerializerSettings = new JsonSerializerSettings();
                        jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        jsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                        jsonSerializerSettings.MaxDepth = 10;

                        string jsonOutput = Newtonsoft.Json.JsonConvert.SerializeObject(datObject, jsonSerializerSettings);
                        string outFileName = Path.Combine(outpath, Path.GetFileNameWithoutExtension(datPathFile) + ".json");
                        File.WriteAllText(outFileName, jsonOutput, Encoding.UTF8);
                    }
                }
            }
        }
        catch
        {

        }
    }
    Console.WriteLine("");
}
else
{
    Console.WriteLine("DATs are disabled.");
}
Console.WriteLine(romSignatures.Count + " DAT files loaded");

// Summarise signatures
if (availablePlatforms.Count > 0)
{
    availablePlatforms.Sort();
    Console.WriteLine("Platforms loaded:");
    foreach (string platform in availablePlatforms)
    {
        Console.WriteLine(" * " + platform);
    }
}

Console.WriteLine("Examining files");
string[] romPathContents = Directory.GetFiles(scanPath);
foreach (string romFile in romPathContents)
{
    Console.WriteLine("Checking " + romFile);

    var stream = File.OpenRead(romFile);

    var md5 = MD5.Create();
    byte[] md5HashByte = md5.ComputeHash(stream);
    string md5Hash = BitConverter.ToString(md5HashByte).Replace("-", "").ToLowerInvariant();

    var sha1 = SHA1.Create();
    byte[] sha1HashByte = sha1.ComputeHash(stream);
    string sha1Hash = BitConverter.ToString(sha1HashByte).Replace("-", "").ToLowerInvariant();

    bool gameFound = false;
    foreach (RomSignatureObject datList in romSignatures)
    {
        foreach (RomSignatureObject.Game gameObject in datList.Games)
        {
            foreach (RomSignatureObject.Game.Rom romObject in gameObject.Roms)
            {
                if (romObject.Md5 != null)
                {
                    if (md5Hash == romObject.Md5.ToLowerInvariant())
                    {
                        // match
                        gameFound = true;
                    }
                }
                if (romObject.Sha1 != null)
                {
                    if (md5Hash == romObject.Sha1.ToLowerInvariant())
                    {
                        // match
                        gameFound = true;
                    }
                }
                if (gameFound == true)
                {
                    Console.WriteLine(romObject.Name);

                    RomSignatureObject.Game gameSignature = gameObject;
                    gameSignature.Roms.Clear();
                    gameSignature.Roms.Add(romObject);

                    var jsonSerializerSettings = new JsonSerializerSettings();
                    jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(gameSignature, Newtonsoft.Json.Formatting.Indented, jsonSerializerSettings));
                    break;
                }
            }
            if (gameFound == true) { break; }
        }
        if (gameFound == true) { break; }
    }
    if (gameFound == false)
    {
        Console.WriteLine("File not found in DAT library");
    }
}

string SearchTitle = "Impossible Mission";
foreach (RomSignatureObject romSignatureObject in romSignatures)
{
    foreach (RomSignatureObject.Game gameObject in romSignatureObject.Games)
    {
        if (gameObject.Name == SearchTitle)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(gameObject, Newtonsoft.Json.Formatting.Indented, jsonSerializerSettings));
        }
    }
}