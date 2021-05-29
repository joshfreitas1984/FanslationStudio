using Newtonsoft.Json;

namespace FanslationStudio.Domain
{
    //Optimise serialisation to speed it up
    public class ScriptTranslationItem
    {
        [JsonProperty("s")]
        public int ItemSequence { get; set; }

        [JsonProperty("r")]
        public string Raw { get; set; }

        [JsonProperty("c")]
        public string CleanedUpLine { get; set; }

        [JsonProperty("rq")]
        public bool RequiresTranslation { get; set; }

        [JsonProperty("it")]
        public string InitialTranslation { get; set; }

        [JsonProperty("mt")]
        public string ManualTranslation { get; set; }

        [JsonProperty("mc")]
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
