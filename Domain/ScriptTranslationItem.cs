using System.Text.Json.Serialization;

namespace FanslationStudio.Domain
{
    public class ScriptTranslationItem
    {
        public int ItemSequence { get; set; }
        public string Raw { get; set; }
        public string CleanedUpLine { get; set; }
        public bool RequiresTranslation { get; set; }
        public string InitialTranslation { get; set; }
        public string ManualTranslation { get; set; }
        public bool MergeHadRawChanges { get; set; }

        [JsonIgnore]
        public string ResultingTranslation
        {
            get
            {
                if (!string.IsNullOrEmpty(ManualTranslation))
                    return ManualTranslation;
                else if (!string.IsNullOrEmpty(InitialTranslation))
                    return InitialTranslation;
                else
                    return Raw;
            }
        }
    }
}
