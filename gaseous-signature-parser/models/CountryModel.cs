namespace gaseous_signature_parser.models
{
    public class CountryItem
    {
        public string Code { get; set; }
        private string _Name { get; set; }
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                string[] nameParts = value.Split("|");
                _Name = nameParts[0].Trim();
                if (nameParts.Length > 1)
                {
                    _Redirection = nameParts[1].Trim();
                }
                else
                {
                    _Redirection = "";
                }
            }
        }
        private string _Redirection { get; set; }
        public string Redirection
        {
            get
            {
                return _Redirection;
            }
        }
    }
}