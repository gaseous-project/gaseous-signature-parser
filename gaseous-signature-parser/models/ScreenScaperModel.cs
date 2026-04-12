namespace gaseous_signature_parser.models.provider
{
    public class ScreenScaperModel
    {
        /// <summary>
        /// Standard header class for ScreenScraper API results
        /// </summary>
        public class ssHeader
        {
            public string? APIversion { get; set; }
            public DateTime? dateTime { get; set; }
            public string? commandRequested { get; set; }
            public bool? success { get; set; }
            public string? error { get; set; }
        }

        /// <summary>
        /// Provides information about the ScreenScraper API servers
        /// </summary>
        public class ssServeurs
        {
            /// <summary>
            /// CPU usage of server 1 (average of the last 5 minutes)
            /// </summary>
            public string? cpu1 { get; set; }
            /// <summary>
            /// CPU usage of server 2 (average of the last 5 minutes)
            /// </summary>
            public string? cpu2 { get; set; }
            /// <summary>
            /// CPU usage of server 3 (average of the last 5 minutes)
            /// </summary>
            public string? cpu3 { get; set; }
            /// <summary>
            /// CPU usage of server 4 (average of the last 5 minutes)
            /// </summary>
            public string? cpu4 { get; set; }
            /// <summary>
            /// Number of accesses to the API since the last minute
            /// </summary>
            public string? threadsmin { get; set; }
            /// <summary>
            /// Number of scrapers using the api since the last minute
            /// </summary>
            public string? nbscrapeurs { get; set; }
            /// <summary>
            /// Number of accesses to the API in the current day (GMT+1)
            /// </summary>
            public string? apiacces { get; set; }
            /// <summary>
            /// Closed API for anonymous (unregistered or unidentified) (0: open / 1: closed)
            /// </summary>
            public string? closefornomember { get; set; }
            /// <summary>
            /// Closed API for non-participating members (no validated proposal) (0: open / 1: closed)
            /// </summary>
            public string? closeforleecher { get; set; }
            /// <summary>
            /// Maximum number of threads opened for anonymous (unregistered or unidentified) at the same time by the api
            /// </summary>
            public string? maxthreadfornonmember { get; set; }
            /// <summary>
            /// Current number of threads opened by anonymous (unregistered or unidentified) at the same time by the api
            /// </summary>
            public string? threadfornonmember { get; set; }
            /// <summary>
            /// Maximum number of threads open for members at the same time by the api
            /// </summary>
            public string? maxthreadformember { get; set; }
            /// <summary>
            /// Current number of threads opened by members at the same time by the api
            /// </summary>
            public string? threadformember { get; set; }
        }

        /// <summary>
        /// Provides information about the ScreenScraper API user, including API usage and rate limit information. This class is used to track how many API calls have been made and when the limits will reset to manage API rate limits effectively. 
        /// </summary>
        public class ssUser
        {
            /// <summary>
            /// username of the user on ScreenScraper
            /// </summary>
            public string? id { get; set; }
            /// <summary>
            /// user's digital identifier on ScreenScraper
            /// </summary>
            public string? numid { get; set; }
            /// <summary>
            /// user level on ScreenScraper
            /// </summary>
            public string? niveau { get; set; }
            /// <summary>
            /// level of financial contribution on ScreenScraper (2 = 1 Additional Thread / 3 and + = 5 Additional Threads)
            /// </summary>
            public string? contribution { get; set; }
            /// <summary>
            /// Counter of valid contributions (system media) proposed by the user
            /// </summary>
            public string? uploadsysteme { get; set; }
            /// <summary>
            /// Valid contribution counter (text info) proposed by the user
            /// </summary>
            public string? uploadinfos { get; set; }
            /// <summary>
            /// Valid contributions counter (association of roms) proposed by the user
            /// </summary>
            public string? romasso { get; set; }
            /// <summary>
            /// Counter of valid contributions (game media) proposed by the user
            /// </summary>
            public string? uploadmedia { get; set; }
            /// <summary>
            /// Number of user proposals validated by a moderator
            /// </summary>
            public string? propositionok { get; set; }
            /// <summary>
            /// Number of user proposals rejected by a moderator
            /// </summary>
            public string? propositionko { get; set; }
            /// <summary>
            /// Percentage of refusal of the user's proposal
            /// </summary>
            public string? quotarefu { get; set; }
            /// <summary>
            /// Number of threads allowed for the user (also indicated for non-registered)
            /// </summary>
            public string? maxthreads { get; set; }
            /// <summary>
            /// Download speed (in KB/s) allowed for the user (also indicated for non-registered)
            /// </summary>
            public string? maxdownloadspeed { get; set; }
            /// <summary>
            /// Total number of calls to the api during the day in short GMT+1 (resets at 0:00 GMT+1)
            /// </summary>
            public string? requeststoday { get; set; }
            /// <summary>
            /// Number of calls to the api with negative feedback (rom/game not found) during the day in short GMT+1 (resets at 0:00 GMT+1)
            /// </summary>
            public string? requestskotoday { get; set; }
            /// <summary>
            /// Maximum number of API calls allowed per minute for the user
            /// </summary>
            public string? maxrequestspermin { get; set; }
            /// <summary>
            /// Maximum number of calls to the API allowed per day for the user
            /// </summary>
            public string? maxrequestsperday { get; set; }
            /// <summary>
            /// Number of calls to the api with a negative feedback (rom/game not found) maximum allowed per day for the user
            /// </summary>
            public string? maxrequestskoperday { get; set; }
            /// <summary>
            /// number of user visits to ScreenScraper
            /// </summary>
            public string? visites { get; set; }
            /// <summary>
            /// date of the user's last visit to ScreenScraper (format: yyyy-mm-dd hh:mm:ss)
            /// </summary>
            public string? datedernierevisite { get; set; }
            /// <summary>
            /// favorite region of user visits on ScreenScraper (france,europe,usa,japon)
            /// </summary>
            public string? favregion { get; set; }
        }

        /// <summary>
        /// Represents a regional text item for the ScreenScraper API, containing information about the region and the associated text. This class is used to deserialize regional text data from the ScreenScraper API responses, allowing for structured access to localized information based on different regions. The region property indicates the specific region (e.g., France, Europe, USA, Japan) associated with the text, while the text property contains the localized information relevant to that region.
        /// </summary>
        public class ssRegionalText
        {
            /// <summary>
            /// Region associated with the text, such as France, Europe, USA, or Japan. This property is used to identify the specific region for which the text information is relevant, allowing for localized metadata retrieval based on regional preferences and differences in game releases or information.
            /// </summary>
            public string? region { get; set; }
            /// <summary>
            /// Text associated with the region, containing localized information relevant to that region.
            /// </summary>
            public string? text { get; set; }
        }

        /// <summary>
        /// Represents a text item for the ScreenScraper API, containing an identifier and the associated text. This class is used to deserialize text data from the ScreenScraper API responses, allowing for structured access to various pieces of information based on their identifiers. The id property serves as a unique identifier for the specific piece of information, while the text property contains the actual information or description associated with that identifier. This structure allows for flexible handling of different types of text information returned by the ScreenScraper API,
        /// </summary>
        public class ssTextId
        {
            /// <summary>
            /// Identifier for the text item, which can be used to categorize or reference specific pieces of information returned by the ScreenScraper API. This identifier allows for structured access to different types of text information, enabling the application to handle various metadata fields effectively based on their unique IDs.
            /// </summary>
            public string? id { get; set; }
            /// <summary>
            /// Text associated with the identifier, containing the actual information or description relevant to that identifier.
            /// </summary>
            public string? text { get; set; }
        }

        /// <summary>
        /// Represents a language-specific text item for the ScreenScraper API, containing the language code and the associated text. This class is used to deserialize language-specific text data from the ScreenScraper API responses, allowing for structured access to localized information based on different languages. The langue property indicates the specific language (e.g., "en" for English, "fr" for French) associated with the text, while the text property contains the localized information relevant to that language. This structure enables the application to handle multilingual metadata effectively based on the language preferences of users or regional differences in game information.
        /// </summary>
        public class ssLanguageText
        {
            /// <summary>
            /// Language code associated with the text, such as "en" for English or "fr" for French. This property is used to identify the specific language for which the text information is relevant, allowing for localized metadata retrieval based on language preferences and differences in game releases or information across different languages.
            /// </summary>
            public string? langue { get; set; }
            /// <summary>
            /// Text associated with the language code, containing the localized information relevant to that language.
            /// </summary>
            public string? text { get; set; }
        }

        /// <summary>
        /// Represents a game classification item for the ScreenScraper API, containing the type of classification and the associated text. This class is used to deserialize game classification data from the ScreenScraper API responses, allowing for structured access to different classifications or categories associated with a game. The type property indicates the specific type of classification (e.g., genre, theme, etc.), while the text property contains the information or description relevant to that classification type. This structure enables the application to handle various classifications of games effectively based on the information returned by the ScreenScraper API.
        /// </summary>
        public class ssGameClassification
        {
            /// <summary>
            /// Type of classification for the game, such as genre, theme, or other categories used by the ScreenScraper API to classify games. This property allows for structured access to different classifications associated with a game, enabling the application to organize and present metadata based on these classifications effectively.
            /// </summary>
            public string? type { get; set; }
            /// <summary>
            /// Text associated with the classification type, containing the information or description relevant to that classification.
            /// </summary>
            public string? text { get; set; }
        }

        /// <summary>
        /// Represents a game item for the ScreenScraper API, containing various properties such as ID, ROM ID, names in different regions, and other metadata fields. This class is used to deserialize game data from the ScreenScraper API responses, allowing for structured access to detailed information about games based on their ROM hashes or IDs. The properties include identifiers, names in different regions, developer and publisher information, player counts, ratings, and classifications, providing a comprehensive representation of a game as returned by the ScreenScraper API.
        /// </summary>
        public class ssGameDate
        {
            /// <summary>
            /// Region associated with the release date, such as France, Europe, USA, or Japan. This property is used to identify the specific region for which the release date information is relevant, allowing for localized metadata retrieval based on regional differences in game release dates.
            /// </summary>
            public string? region { get; set; }
            /// <summary>
            /// Release date of the game for the associated region, providing information about when the game was released in that specific region. This property allows for structured access to release date information based on regional differences, enabling the application to present accurate metadata about game releases across different regions as returned by the ScreenScraper API.
            /// </summary>
            public string? date { get; set; }
        }

        /// <summary>
        /// Represents a game genre item for the ScreenScraper API, containing properties such as ID, name, and parent-child relationships between genres. This class is used to deserialize game genre data from the ScreenScraper API responses, allowing for structured access to genre information associated with games. The properties include identifiers, names in different languages, and relationships between genres, providing a comprehensive representation of game genres as returned by the ScreenScraper API.
        /// </summary>
        public class ssGameGenre
        {
            /// <summary>
            /// ID of the genre, serving as a unique identifier for the genre in the ScreenScraper API. This property allows for structured access to genre information based on its unique ID, enabling the application to reference and organize genres effectively based on the data returned by the ScreenScraper API.
            /// </summary>
            public string? id { get; set; }
            /// <summary>
            /// Short name of the genre, providing a concise identifier for the genre. This property is used to access a brief name for the genre, which can be useful for display purposes or when referencing genres in a more compact form based on the data returned by the ScreenScraper API.
            /// </summary>
            public string? nomcourt { get; set; }
            /// <summary>
            /// Indicates whether this genre is the main genre for a game. This property can be used to identify the primary genre associated with a game, allowing for structured access to genre information based on its significance or relevance to the game as returned by the ScreenScraper API.
            /// </summary>
            public string? principale { get; set; }
            /// <summary>
            /// ID of the parent genre, if applicable, indicating a hierarchical relationship between genres. This property allows for structured access to genre information based on parent-child relationships, enabling the application to organize genres effectively based on their relationships as returned by the ScreenScraper API.
            /// </summary>
            public string? parentid { get; set; }
            /// <summary>
            /// List of names for the genre in different languages, providing localized information about the genre based on language preferences. This property allows for structured access to genre names in various languages, enabling the application to present genre information effectively based on the language preferences of users or regional differences in game information as returned by the ScreenScraper API.
            /// </summary>
            public List<ssLanguageText>? noms { get; set; }
        }

        /// <summary>
        /// Represents a game mode item for the ScreenScraper API, containing properties such as ID, name, and parent-child relationships between game modes. This class is used to deserialize game mode data from the ScreenScraper API responses, allowing for structured access to game mode information associated with games. The properties include identifiers, names in different languages, and relationships between game modes, providing a comprehensive representation of game modes as returned by the ScreenScraper API.
        /// </summary>
        public class ssGameMode
        {
            /// <summary>
            /// ID of the game mode, serving as a unique identifier for the game mode in the ScreenScraper API. This property allows for structured access to game mode information based on its unique ID, enabling the application to reference and organize game modes effectively based on the data returned by the ScreenScraper API.
            /// </summary>
            public string? id { get; set; }
            /// <summary>
            /// Short name of the game mode, providing a concise identifier for the game mode. This property is used to access a brief name for the game mode, which can be useful for display purposes or when referencing game modes in a more compact form based on the data returned by the ScreenScraper API.
            /// </summary>
            public string? nomcourt { get; set; }
            /// <summary>
            /// Indicates whether this game mode is the main mode for a game. This property can be used to identify the primary game mode associated with a game, allowing for structured access to game mode information based on its significance or relevance to the game as returned by the ScreenScraper API.
            /// </summary>
            public string? principale { get; set; }
            /// <summary>
            /// ID of the parent game mode, if applicable, indicating a hierarchical relationship between game modes. This property allows for structured access to game mode information based on parent-child relationships, enabling the application to organize game modes effectively based on their relationships as returned by the ScreenScraper API.
            /// </summary>
            public string? parentid { get; set; }
            /// <summary>
            /// List of names for the game mode in different languages, providing localized information about the game mode based on language preferences. This property allows for structured access to game mode names in various languages, enabling the application to present game mode information effectively based on the language preferences of users or regional differences in game information as returned by the ScreenScraper API.
            /// </summary>
            public List<ssLanguageText>? noms { get; set; }
        }

        /// <summary>
        /// Represents a game franchise item for the ScreenScraper API, containing properties such as ID, name, and parent-child relationships between franchises. This class is used to deserialize game franchise data from the ScreenScraper API responses, allowing for structured access to game franchise information associated with games. The properties include identifiers, names in different languages, and relationships between franchises, providing a comprehensive representation of game franchises as returned by the ScreenScraper API.
        /// </summary>
        public class ssGameFranchise
        {
            /// <summary>
            /// ID of the franchise, serving as a unique identifier for the franchise in the ScreenScraper API. This property allows for structured access to franchise information based on its unique ID, enabling the application to reference and organize franchises effectively based on the data returned by the ScreenScraper API.
            /// </summary>
            public string? id { get; set; }
            /// <summary>
            /// Short name of the franchise, providing a concise identifier for the franchise. This property is used to access a brief name for the franchise, which can be useful for display purposes or when referencing franchises in a more compact form based on the data returned by the ScreenScraper API.
            /// </summary>
            public string? nomcourt { get; set; }
            /// <summary>
            /// Indicates whether this franchise is the main franchise for a game. This property can be used to identify the primary franchise associated with a game, allowing for structured access to franchise information based on its significance or relevance to the game as returned by the ScreenScraper API.
            /// </summary>
            public string? principale { get; set; }
            /// <summary>
            /// ID of the parent franchise, if applicable, indicating a hierarchical relationship between franchises. This property allows for structured access to franchise information based on parent-child relationships, enabling the application to organize franchises effectively based on their relationships as returned by the ScreenScraper API.
            /// </summary>
            public string? parentid { get; set; }
            /// <summary>
            /// List of names for the franchise in different languages, providing localized representations of the franchise name. This property allows for structured access to franchise names based on language, enabling the application to display franchise names appropriately for different locales as returned by the ScreenScraper API.
            /// </summary>
            public List<ssLanguageText>? noms { get; set; }
        }

        /// <summary>
        /// Represents a media item for the ScreenScraper API, containing properties such as type, URL, region, and various hash values. This class is used to deserialize media data from the ScreenScraper API responses, allowing for structured access to media information. The properties include the type of media (e.g., screenshot, box art), the URL where the media can be accessed, the region associated with the media, and various hash values (CRC, MD5, SHA1) for verifying the integrity of the media file. This structure provides a comprehensive representation of media as returned by the ScreenScraper API.
        /// </summary>
        public class ssMedia
        {
            /// <summary>
            /// Type of media, such as "screenshot", "boxart", "banner", etc., indicating the category or purpose of the media item. This property allows for structured access to media information based on its type, enabling the application to organize and present media effectively based on the type of media returned by the ScreenScraper API.
            /// </summary>
            public string? type { get; set; }
            /// <summary>
            /// URL where the media can be accessed, providing a direct link to the media file associated with the game. This property allows for structured access to media information based on its URL, enabling the application to retrieve and display media effectively based on the URL provided by the ScreenScraper API.
            /// </summary>
            public string? parent { get; set; }
            /// <summary>
            /// URL where the media can be accessed, providing a direct link to the media file associated with the game. This property allows for structured access to media information based on its URL, enabling the application to retrieve and display media effectively based on the URL provided by the ScreenScraper API.
            /// </summary>
            public string? url { get; set; }
            /// <summary>
            /// Region associated with the media, such as France, Europe, USA, or Japan. This property is used to identify the specific region for which the media information is relevant, allowing for localized metadata retrieval based on regional differences in game releases or information as returned by the ScreenScraper API.
            /// </summary>
            public string? region { get; set; }
            /// <summary>
            /// Indicates whether the media is the main media for the game (0 = no / 1 = yes). This property can be used to identify the primary media associated with a game, allowing for structured access to media information based on its significance or relevance to the game as returned by the ScreenScraper API.
            /// </summary>
            public string? support { get; set; }
            /// <summary>
            /// CRC hash value for the media file, used for verifying the integrity of the media file. This property allows for structured access to media information based on its CRC hash, enabling the application to validate the media file effectively based on the hash value provided by the ScreenScraper API.
            /// </summary>
            public string? crc { get; set; }
            /// <summary>
            /// MD5 hash value for the media file, used for verifying the integrity of the media file. This property allows for structured access to media information based on its MD5 hash, enabling the application to validate the media file effectively based on the hash value provided by the ScreenScraper API.
            /// </summary>
            public string? md5 { get; set; }
            /// <summary>
            /// SHA1 hash value for the media file, used for verifying the integrity of the media file. This property allows for structured access to media information based on its SHA1 hash, enabling the application to validate the media file effectively based on the hash value provided by the ScreenScraper API.
            /// </summary>
            public string? sha1 { get; set; }
            /// <summary>
            /// Size of the media file, providing information about the file's storage requirements. This property allows for structured access to media information based on its size, enabling the application to manage storage and display media effectively based on the size information provided by the ScreenScraper API.
            /// </summary>
            public string? size { get; set; }
            /// <summary>
            /// Format of the media file, indicating the file type or encoding used. This property allows for structured access to media information based on its format, enabling the application to handle and display media effectively based on the format information provided by the ScreenScraper API.
            /// </summary>
            public string? format { get; set; }
        }

        public class ssRom
        {
            /// <summary>
            /// numeric identifier of the rom
            /// </summary>
            public long? Id { get; set; }
            /// <summary>
            /// support number (ex: 1 = floppy disk 01 or CD 01)
            /// </summary>
            public int? Romnumsupport { get; set; }
            /// <summary>
            /// total number of supports (ex: 2 = 2 floppy disks or 2 CDs)
            /// </summary>
            public int? romtotalsupport { get; set; }
            /// <summary>
            /// name of the rom file or folder
            /// </summary>
            public string? romfilename { get; set; }
            /// <summary>
            /// octect size of the rom file or size of the contents of the folder
            /// </summary>
            public int? romsize { get; set; }
            /// <summary>
            /// result of the CRC32 calculation of the rom file or the largest file of the "rom" folder
            /// </summary>
            public string? romcrc { get; set; }
            /// <summary>
            /// result of the MD5 calculation of the rom file or the largest file of the "rom" folder
            /// </summary>
            public string? rommd5 { get; set; }
            /// <summary>
            /// result of the SHA1 calculation of the rom file or the largest file of the "rom" folder
            /// </summary>
            public string? romsha1 { get; set; }
            /// <summary>
            /// digital identifier of the parent rom if the rom is a clone (Arcade Systems)
            /// </summary>
            public long? romcloneof { get; set; }
            /// <summary>
            /// Beta version of the game (0 = no / 1 = yes)
            /// </summary>
            public int? Beta { get; set; }
            /// <summary>
            /// Demo version of the game (0 = no / 1 = yes)
            /// </summary>
            public int? Demo { get; set; }
            /// <summary>
            /// Translated version of the game (0 = no / 1 = yes)
            /// </summary>
            public int? trad { get; set; }
            /// <summary>
            /// Modified version of the game (0 = no / 1 = yes)
            /// </summary>
            public int? hack { get; set; }
            /// <summary>
            /// Game not "Official" (0 = no / 1 = yes)
            /// </summary>
            public int? Unl { get; set; }
            /// <summary>
            /// Alternative version of the game (0 = no / 1 = yes)
            /// </summary>
            public int? alt { get; set; } = 0;
            /// <summary>
            /// Best version of the game (0 = no / 1 = yes)
            /// </summary>
            public int? best { get; set; }
            /// <summary>
            /// Compatible Retro Achievement (0 = no / 1 = yes)
            /// </summary>
            public int? Retroachievement { get; set; }
            /// <summary>
            /// Gamelink compatible (0 = no / 1 = yes)
            /// </summary>
            public int? Gamelink { get; set; }
            /// <summary>
            /// Total number of times scraped
            /// </summary>
            public int? nbscrap { get; set; }
            /// <summary>
            /// List of supported languages
            /// </summary>
            public Dictionary<string, List<string>>? langues { get; set; }
            /// <summary>
            /// List of supported regions
            /// </summary>
            public Dictionary<string, List<string>>? regions { get; set; }
        }

        /// <summary>
        /// Represents a game item for the ScreenScraper API, containing various properties such as ID, ROM ID, names in different regions, and other metadata fields. This class is used to deserialize game data from the ScreenScraper API responses, allowing for structured access to detailed information about games based on their ROM hashes or IDs. The properties include identifiers, names in different regions, developer and publisher information, player counts, ratings, classifications, release dates, genres, modes, franchises, and associated media, providing a comprehensive representation of a game as returned by the ScreenScraper API.
        /// </summary>
        public class ssGame
        {
            /// <summary>
            /// ID of the game, serving as a unique identifier for the game in the ScreenScraper API. This property allows for structured access to game information based on its unique ID, enabling the application to reference and organize games effectively based on the data returned by the ScreenScraper API.
            /// </summary>
            public long? id { get; set; }
            /// <summary>
            /// ROM ID associated with the game, providing a reference to the specific ROM for which the metadata is relevant. This property allows for structured access to game information based on its ROM ID, enabling the application to manage and present metadata effectively based on the ROM information provided by the ScreenScraper API.
            /// </summary>
            public long? romid { get; set; }
            /// <summary>
            /// Indicates whether the item is not a game, which can be used to filter out non-game items from the metadata results. This property allows for structured access to game information based on its classification as a game or non-game item, enabling the application to manage and present metadata effectively based on the type of item returned by the ScreenScraper API.
            /// </summary>
            public bool? notgame { get; set; }
            /// <summary>
            /// List of names for the game in different regions, providing localized information about the game's title based on regional preferences. This property allows for structured access to game names in various regions, enabling the application to present game information effectively based on regional differences in game titles as returned by the ScreenScraper API.
            /// </summary>
            public List<ssRegionalText>? noms { get; set; }
            /// <summary>
            /// Indicates if the game is a clone of another game, which can be used to identify and manage metadata for games that are variations or derivatives of other games. This property allows for structured access to game information based on its classification as a clone, enabling the application to handle and present metadata effectively based on the relationships between games as returned by the ScreenScraper API.
            /// </summary>
            public string? cloneof { get; set; }
            /// <summary>
            /// System associated with the game, providing information about the platform or console for which the game was released. This property allows for structured access to game information based on its associated system, enabling the application to manage and present metadata effectively based on the platform information provided by the ScreenScraper API.
            /// </summary>
            public ssTextId? systeme { get; set; }
            /// <summary>
            /// Publisher of the game, providing information about the company or entity responsible for publishing the game. This property allows for structured access to game information based on its publisher, enabling the application to manage and present metadata effectively based on the publisher information provided by the ScreenScraper API.
            /// </summary>
            public ssTextId? editeur { get; set; }
            /// <summary>
            /// Developer of the game, providing information about the company or entity responsible for developing the game. This property allows for structured access to game information based on its developer, enabling the application to manage and present metadata effectively based on the developer information provided by the ScreenScraper API.
            /// </summary>
            public ssTextId? developpeur { get; set; }
            /// <summary>
            /// Number of players supported by the game, providing information about the multiplayer capabilities of the game. This property allows for structured access to game information based on its player count, enabling the application to manage and present metadata effectively based on the multiplayer information provided by the ScreenScraper API.
            /// </summary>
            public KeyValuePair<string, string>? joueurs { get; set; }
            /// <summary>
            /// Rating of the game, providing information about the game's quality or popularity based on user ratings or reviews. This property allows for structured access to game information based on its rating, enabling the application to manage and present metadata effectively based on the rating information provided by the ScreenScraper API.
            /// </summary>
            public KeyValuePair<string, string>? note { get; set; }
            /// <summary>
            /// Top staff associated with the game, providing information about key personnel involved in the game's development or production. This property allows for structured access to game information based on its top staff, enabling the application to manage and present metadata effectively based on the personnel information provided by the ScreenScraper API.
            /// </summary>
            public string? topstaff { get; set; }
            /// <summary>
            /// Rotation of the game, providing information about the orientation or display settings for the game. This property allows for structured access to game information based on its rotation, enabling the application to manage and present metadata effectively based on the display information provided by the ScreenScraper API.
            /// </summary>
            public string? rotation { get; set; }
            /// <summary>
            /// Synopsis of the game, providing a brief description or summary of the game's plot, gameplay, or features. This property allows for structured access to game information based on its synopsis, enabling the application to manage and present metadata effectively based on the descriptive information provided by the ScreenScraper API.
            /// </summary>
            public List<ssLanguageText>? synopsis { get; set; }
            /// <summary>
            /// List of classifications associated with the game, providing information about the various categories or classifications that apply to the game. This property allows for structured access to game information based on its classifications, enabling the application to manage and present metadata effectively based on the classification information provided by the ScreenScraper API.
            /// </summary>
            public List<ssGameClassification>? classifications { get; set; }
            /// <summary>
            /// List of release dates for the game in different regions, providing information about when the game was released in various regions. This property allows for structured access to game information based on its release dates, enabling the application to manage and present metadata effectively based on regional differences in game release dates as returned by the ScreenScraper API.
            /// </summary>
            public List<ssGameDate>? dates { get; set; }
            /// <summary>
            /// List of genres associated with the game, providing information about the various genres that apply to the game. This property allows for structured access to game information based on its genres, enabling the application to manage and present metadata effectively based on the genre information provided by the ScreenScraper API.
            /// </summary>
            public List<ssGameGenre>? genres { get; set; }
            /// <summary>
            /// List of game modes associated with the game, providing information about the various modes of play that apply to the game. This property allows for structured access to game information based on its game modes, enabling the application to manage and present metadata effectively based on the game mode information provided by the ScreenScraper API.
            /// </summary>
            public List<ssGameMode>? modes { get; set; }
            /// <summary>
            /// List of franchises associated with the game, providing information about the various franchises that apply to the game. This property allows for structured access to game information based on its franchises, enabling the application to manage and present metadata effectively based on the franchise information provided by the ScreenScraper API.
            /// </summary>
            public List<ssGameFranchise>? familles { get; set; }
            /// <summary>
            /// List of media associated with the game, providing information about the various media types that apply to the game. This property allows for structured access to game information based on its media, enabling the application to manage and present metadata effectively based on the media information provided by the ScreenScraper API.
            /// </summary>
            public List<ssMedia>? medias { get; set; }
            /// <summary>
            /// List of ROMs associated with the game, providing information about the various ROM files that apply to the game. This property allows for structured access to game information based on its ROMs, enabling the application to manage and present metadata effectively based on the ROM information provided by the ScreenScraper API.
            /// </summary>
            public List<ssRom>? roms { get; set; }
        }

        /// <summary>
        /// Represents a platform item for the ScreenScraper API, containing properties such as ID, parent ID, and names in different languages. This class is used to deserialize platform data from the ScreenScraper API responses, allowing for structured access to platform information associated with games. The properties include identifiers, parent-child relationships between platforms, and names in different languages, providing a comprehensive representation of platforms as returned by the ScreenScraper API.
        /// </summary>
        public class ssPlatform
        {
            /// <summary>
            /// digital identifier of the system (to be provided again in other API requests)
            /// </summary>
            public long? id { get; set; }
            /// <summary>
            /// digital identifier of the parent system
            /// </summary>
            public long? parentid { get; set; }
            /// List of names for the platform in different languages, providing localized information about the platform based on language preferences. This property allows for structured access to platform names in various languages, enabling the application to present platform information effectively based on the language preferences of users or regional differences in platform information as returned by the ScreenScraper API.
            /// </summary>
            public Dictionary<string, string> noms { get; set; }
            /// <summary>
            /// extensions of usable rom files (all emulators combined)
            /// </summary>
            public string? extensions { get; set; }
            /// <summary>
            /// Name of the system production company
            /// </summary>
            public string? compagnie { get; set; }
            /// <summary>
            /// System type (Arcade,Console,Portable Console,Arcade Emulation,Fipper,Online,Computer,Smartphone)
            /// </summary>
            public string? type { get; set; }
            /// <summary>
            /// Year of production start
            /// </summary>
            public string? datedebut { get; set; }
            /// <summary>
            /// Year of end of production
            /// </summary>
            public string? datefin { get; set; }
            /// <summary>
            /// Type(s) of roms
            /// </summary>
            public string? romtype { get; set; }
            /// <summary>
            /// Type of the original system media
            /// </summary>
            public string? romTypesListe { get; set; }
            /// <summary>
            /// List of media associated with the platform
            /// </summary>
            public List<ssMedia>? medias { get; set; }
        }

        /// <summary>
        /// Maps to the ssuser ScreenScraper API object, used to guage how many API calls are being made and to manage the API rate limits by tracking the number of calls made and the time until the next reset.
        /// </summary>
        public class UserItem
        {
            /// <summary>
            /// Standard header for ScreenScraper API responses, containing information about the API version, request time, command requested, success status, and any error messages. This header is included in the user information response to provide context about the API request and response.
            /// </summary>
            public ssHeader? header { get; set; }

            /// <summary>
            /// Contains information about the ScreenScraper API servers and the user, including API usage and rate limit information. This information is used to manage API rate limits effectively by tracking how many calls have been made and when the limits will reset. The server information provides insights into the current load on the API servers, while the user information tracks the API usage for the specific user account, allowing the application to avoid exceeding the limits and ensure smooth operation.
            /// </summary>
            public UserInfoResponse? response { get; set; }

            /// <summary>
            /// Represents the response from the ScreenScraper API when fetching user information, including server status and user API usage details. This class is used to deserialize the JSON response from the API and provides structured access to the server and user information needed to manage API rate limits effectively.
            /// </summary>
            public class UserInfoResponse
            {
                /// <summary>
                /// Information about the ScreenScraper API servers, including CPU usage, thread counts, and API access details. This information helps gauge the current load on the API servers and can be used to make informed decisions about when to make API calls to avoid overloading the servers.
                /// </summary>
                public ssServeurs? serveurs { get; set; }
                /// <summary>
                /// Information about the ScreenScraper API user, including API usage and rate limit details. This information is crucial for managing API rate limits effectively by tracking how many calls have been made and when the limits will reset, allowing the application to avoid exceeding the limits and ensure smooth operation.
                /// </summary>
                public ssUser? ssuser { get; set; }
            }
        }

        /// <summary>
        /// Represents a game item for the ScreenScraper API, providing a method to construct the API endpoint URL for retrieving game information based on either the game ID or ROM hashes (MD5 and SHA1). This class is used to generate the correct endpoint for fetching game metadata from the ScreenScraper API, allowing for flexible searching by either ID or hash values. The method ensures that the necessary parameters are provided and constructs the appropriate URL for the API request.
        /// </summary>
        public class GameItem
        {
            /// <summary>
            /// Standard header for ScreenScraper API responses, containing information about the API version, request time, command requested, success status, and any error messages. This header is included in the user information response to provide context about the API request and response.
            /// </summary>
            public ssHeader? header { get; set; }

            /// <summary>
            /// Contains information about the ScreenScraper API servers and the user, including API usage and rate limit information, as well as detailed information about the game retrieved from the API. This information is used to manage API rate limits effectively by tracking how many calls have been made and when the limits will reset, while also providing structured access to comprehensive game metadata based on the data returned by the ScreenScraper API. The server information provides insights into the current load on the API servers, while the user information tracks the API usage for the specific user account, allowing the application to avoid exceeding the limits and ensure smooth operation. The game information includes various metadata fields such as names, developer, publisher, player counts, ratings, classifications, release dates, genres, modes, franchises, and associated media, enabling the application to manage and present metadata effectively based on the detailed information provided for each game.
            /// </summary>
            public GameInfoResponse? response { get; set; }

            /// <summary>
            /// Represents the response from the ScreenScraper API when fetching game information, including server status, user API usage details, and comprehensive game metadata. This class is used to deserialize the JSON response from the API and provides structured access to the server, user, and game information needed to manage API rate limits effectively while also providing detailed metadata about the game as returned by the ScreenScraper API.
            /// </summary>
            public class GameInfoResponse
            {
                /// <summary>
                /// Information about the ScreenScraper API servers, including CPU usage, thread counts, and API access details. This information helps gauge the current load on the API servers and can be used to make informed decisions about when to make API calls to avoid overloading the servers.
                /// </summary>
                public ssServeurs? serveurs { get; set; }
                /// <summary>
                /// Information about the ScreenScraper API user, including API usage and rate limit details. This information is crucial for managing API rate limits effectively by tracking how many calls have been made and when the limits will reset, allowing the application to avoid exceeding the limits and ensure smooth operation.
                /// </summary>
                public ssUser? ssuser { get; set; }
                /// <summary>
                /// Detailed information about the game retrieved from the ScreenScraper API, including various metadata fields such as names, developer, publisher, player counts, ratings, classifications, release dates, genres, modes, franchises, and associated media. This property allows for structured access to comprehensive game information based on the data returned by the ScreenScraper API, enabling the application to manage and present metadata effectively based on the detailed information provided for each game.
                /// </summary>
                public ssGame? jeu { get; set; }
            }
        }

        public class PlatformItem
        {
            /// <summary>
            /// Standard header for ScreenScraper API responses, containing information about the API version, request time, command requested, success status, and any error messages. This header is included in the user information response to provide context about the API request and response.
            /// </summary>
            public ssHeader? header { get; set; }

            /// <summary>
            /// Contains information about the ScreenScraper API servers and the user, including API usage and rate limit information, as well as detailed information about the platforms retrieved from the API. This information is used to manage API rate limits effectively by tracking how many calls have been made and when the limits will reset, while also providing structured access to comprehensive platform metadata based on the data returned by the ScreenScraper API. The server information provides insights into the current load on the API servers, while the user information tracks the API usage for the specific user account, allowing the application to avoid exceeding the limits and ensure smooth operation. The platform information includes various metadata fields such as platform names, release dates, manufacturers, and other relevant details, enabling the application to manage and present metadata effectively based on the detailed information provided for each platform.
            /// </summary>
            public PlatformInfoResponse? response { get; set; }

            /// <summary>
            /// Represents the response from the ScreenScraper API when fetching platform information, including server status, user API usage details, and comprehensive platform metadata. This class is used to deserialize the JSON response from the API and provides structured access to the server, user, and platform information needed to manage API rate limits effectively while also providing detailed metadata about the platforms as returned by the ScreenScraper API.
            /// </summary>
            public class PlatformInfoResponse
            {
                /// <summary>
                /// Information about the ScreenScraper API servers, including CPU usage, thread counts, and API access details. This information helps gauge the current load on the API servers and can be used to make informed decisions about when to make API calls to avoid overloading the servers.
                /// </summary>
                public ssServeurs? serveurs { get; set; }

                /// <summary>
                /// Information about the ScreenScraper API user, including API usage and rate limit details. This information is crucial for managing API rate limits effectively by tracking how many calls have been made and when the limits will reset, allowing the application to avoid exceeding the limits and ensure smooth operation.
                /// </summary>
                public List<ssPlatform>? systemes { get; set; }
            }
        }
    }
}