﻿using System;
using System.Collections.Generic;

namespace gaseous_signature_parser.models.RomSignatureObject
{
    /// <summary>
    /// Object returned by all signature engines containing metadata about the ROM's in the data files
    ///
    /// This class was based on the TOSEC dataset, so may need to be expanded as new signature engines are added
    /// </summary>
	public class RomSignatureObject
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Version { get; set; }
        public string? Author { get; set; }
        public string? Email { get; set; }
        public string? Homepage { get; set; }
        public Uri? Url { get; set; }
        public string? SourceType { get; set; }
        public string SourceMd5 { get; set; } = "";
        public string SourceSHA1 { get; set; } = "";

        public List<Game> Games { get; set; } = new List<Game>();

        public class Game
        {
            public string? Id { get; set; }
            public string? CloneOfId { get; set; }
            public string? GameId { get; set; }
            public string? Category { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? Year { get; set; }
            public string? Publisher { get; set; }
            public DemoTypes Demo { get; set; }
            public string? System { get; set; }
            public string? SystemVariant { get; set; }
            public string? Video { get; set; }
            public string? CountryString { get; set; }
            public Dictionary<string, string>? Country { get; set; } = new Dictionary<string, string>();
            public string? LanguageString { get; set; }
            public Dictionary<string, string>? Language { get; set; } = new Dictionary<string, string>();
            public string? Copyright { get; set; }
            public List<Rom> Roms { get; set; } = new List<Rom>();
            public Dictionary<string, object> flags { get; set; } = new Dictionary<string, object>();
            public int RomCount
            {
                get
                {
                    return Roms.Count();
                }
            }

            public enum DemoTypes
            {
                NotDemo = 0,
                demo = 1,
                demo_kiosk = 2,
                demo_playable = 3,
                demo_rolling = 4,
                demo_slideshow = 5
            }

            public class Rom
            {
                public string? Id { get; set; }
                public string? Name { get; set; }
                public UInt64? Size { get; set; }
                public string? Crc { get; set; }
                public string? Md5 { get; set; }
                public string? Sha1 { get; set; }
                public string? Sha256 { get; set; }

                public string? Status { get; set; }

                public Dictionary<string, string>? Country { get; set; }
                public Dictionary<string, string>? Language { get; set; }

                public string? DevelopmentStatus { get; set; }

                public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

                public RomTypes RomType { get; set; }
                public string? RomTypeMedia { get; set; }
                public MediaType? MediaDetail
                {
                    get
                    {
                        if (RomTypeMedia != null)
                        {
                            return new MediaType(SignatureSource, RomTypeMedia);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                public string? MediaLabel { get; set; }

                public SignatureSourceType SignatureSource { get; set; }

                public enum SignatureSourceType
                {
                    None = 0,

                    /// <summary>
                    /// https://www.tosecdev.org
                    /// </summary>
                    TOSEC = 1,

                    /// <summary>
                    /// https://www.progettosnaps.net/index.php
                    /// </summary>
                    MAMEArcade = 2,

                    /// <summary>
                    /// https://www.progettosnaps.net/index.php
                    /// </summary>
                    MAMEMess = 3,

                    /// <summary>
                    /// https://no-intro.org
                    /// </summary>
                    NoIntros = 4,

                    /// <summary>
                    /// http://redump.org
                    /// </summary>
                    Redump = 5,

                    /// <summary>
                    /// https://github.com/BlitterStudio/amiberry/blob/master/whdboot/game-data/whdload_db.xml
                    /// </summary>
                    WHDLoad = 6,

                    /// <summary>
                    /// https://retroachievements.org
                    /// </summary>
                    RetroAchievements = 7,

                    /// <summary>
                    /// https://github.com/libretro/FBNeo/tree/master/dats
                    /// </summary>
                    FBNeo = 8,

                    /// <summary>
                    /// https://github.com/PureDOS/DAT
                    /// </summary>
                    PureDOSDAT = 9,

                    /// <summary>
                    /// https://github.com/pleasuredome/pleasuredome/tree/gh-pages
                    /// </summary>
                    Pleasuredome = 10,

                    /// <summary>
                    /// Generic parser, used for custom parsers
                    /// </summary>
                    Generic = 99
                }

                public enum RomTypes
                {
                    /// <summary>
                    /// Media type is unknown
                    /// </summary>
                    Unknown = 0,

                    /// <summary>
                    /// Optical media
                    /// </summary>
                    Disc = 1,

                    /// <summary>
                    /// Magnetic media
                    /// </summary>
                    Disk = 2,

                    /// <summary>
                    /// Individual files
                    /// </summary>
                    File = 3,

                    /// <summary>
                    /// Individual pars
                    /// </summary>
                    Part = 4,

                    /// <summary>
                    /// Tape base media
                    /// </summary>
                    Tape = 5,

                    /// <summary>
                    /// Side of the media
                    /// </summary>
                    Side = 6
                }

                public class MediaType
                {
                    public MediaType(SignatureSourceType Source, string MediaTypeString)
                    {
                        try
                        {
                            switch (Source)
                            {
                                case Rom.SignatureSourceType.TOSEC:
                                case Rom.SignatureSourceType.NoIntros:
                                case Rom.SignatureSourceType.Redump:
                                    string[] typeString = MediaTypeString.Split(" ");

                                    string inType = "";
                                    foreach (string typeStringVal in typeString)
                                    {
                                        if (inType == "")
                                        {
                                            switch (typeStringVal.ToLower())
                                            {
                                                case "disk":
                                                    Media = Rom.RomTypes.Disk;

                                                    inType = typeStringVal;
                                                    break;
                                                case "disc":
                                                    Media = Rom.RomTypes.Disc;

                                                    inType = typeStringVal;
                                                    break;
                                                case "file":
                                                    Media = Rom.RomTypes.File;

                                                    inType = typeStringVal;
                                                    break;
                                                case "part":
                                                    Media = Rom.RomTypes.Part;

                                                    inType = typeStringVal;
                                                    break;
                                                case "tape":
                                                    Media = Rom.RomTypes.Tape;

                                                    inType = typeStringVal;
                                                    break;
                                                case "of":
                                                    inType = typeStringVal;
                                                    break;
                                                case "side":
                                                    inType = typeStringVal;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            // check if typeStringVal.Trim(".,".ToCharArray()) is a number
                                            switch (inType.ToLower())
                                            {
                                                case "disk":
                                                case "disc":
                                                case "file":
                                                case "part":
                                                case "tape":
                                                    if (int.TryParse(typeStringVal.Trim(".,".ToCharArray()), out var numberOne))
                                                    {
                                                        Number = numberOne;
                                                    }
                                                    break;
                                                case "of":
                                                    if (int.TryParse(typeStringVal.Trim(".,".ToCharArray()), out var numberTwo))
                                                    {
                                                        Count = numberTwo;
                                                    }
                                                    break;
                                                case "side":
                                                    Side = typeStringVal;
                                                    break;
                                            }
                                            inType = "";
                                        }
                                    }

                                    break;

                                default:
                                    break;

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing MediaType from source {Source} with input '{MediaTypeString}': {ex.Message}");
                        }
                    }

                    public Rom.RomTypes? Media { get; set; }

                    public int? Number { get; set; }

                    public int? Count { get; set; }

                    public string? Side { get; set; }
                }
            }
        }
    }
}

