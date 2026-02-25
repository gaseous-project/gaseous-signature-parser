namespace gaseous_signature_parser.models
{
    public class NameParseModel
    {
        public string Name { get; set; } = "";
        public Dictionary<string, string> Country { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Language { get; set; } = new Dictionary<string, string>();
        public DateTime? ReleaseDate { get; set; } = null;
        public DevelopmentStatusItem? DevelopmentStatus { get; set; } = null;
    }
}