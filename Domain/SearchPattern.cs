using Newtonsoft.Json;

namespace FanslationStudio.Domain
{
    public class SearchPattern
    {
        public string Find { set; get; }
        public string Replacement { set; get; }
        public bool CaseSensitive { set; get; }

        [JsonIgnore]
        public bool IsRegex
        {
            get
            {
                return (Find.Contains("[") || Find.Contains("^") || Find.Contains("/"));
            }
        }
    }
}
