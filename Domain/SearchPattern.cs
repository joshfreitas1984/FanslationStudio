using Caliburn.Micro;
using Newtonsoft.Json;

namespace FanslationStudio.Domain
{
    public class SearchPattern: PropertyChangedBase //Needed for grid refresh
    {
        private string _find;
        private string _replacement;
        private string _comment;
        private bool _caseSensitive;
        private bool _enabled;

        [JsonProperty("Find")] //Inheriting from PropertyChangedBase seems to hide all serialisation
        public string Find
        {
            get
            {
                return _find;
            }
            set
            {
                _find = value;
                NotifyOfPropertyChange(() => Find);
            }
        }

        [JsonProperty("Replacement")]
        public string Replacement
        {
            get
            {
                return _replacement;
            }
            set
            {
                _replacement = value;
                NotifyOfPropertyChange(() => Replacement);
            }
        }

        [JsonProperty("CaseSensitive")]
        public bool CaseSensitive
        {
            get
            {
                return _caseSensitive;
            }
            set
            {
                _caseSensitive = value;
                NotifyOfPropertyChange(() => CaseSensitive);
            }
        }

        [JsonProperty("Comment")]
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
                NotifyOfPropertyChange(() => Comment);
            }
        }

        [JsonProperty("Enabled")]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                NotifyOfPropertyChange(() => Enabled);
            }
        }

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
