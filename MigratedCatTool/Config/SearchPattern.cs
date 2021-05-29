namespace FanslationStudio.MigratedCatTool.Config
{
    public class SearchPattern
    {
        public string Regex { set; get; }
        public string Replacement { set; get; }
        public bool CaseSensitive { set; get; }
        public bool IsRegex { private set; get; }

        public string Display
        {
            get
            {
                string caseSens = CaseSensitive ? "Case Sensitive" : "Case Insensitive";
                return $"[{Replacement}]   {Regex}  [{caseSens}]";
            }
        }

        public SearchPattern()
        {
        }

        public SearchPattern(string regex, string replacement, bool caseSensitive)
        {
            Regex = regex;
            Replacement = replacement;
            CaseSensitive = caseSensitive;

            if (regex.Contains("[") || regex.Contains("^") || regex.Contains("/"))
                IsRegex = true;
        }
    }
}
