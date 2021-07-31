using Caliburn.Micro;
using Newtonsoft.Json;

namespace FanslationStudio.Domain
{
    //Optimise serialisation to speed it up
    public class ScriptTranslationItem : PropertyChangedBase
    {
        private int _itemSequence;
        private string _raw;
        private string _cleanedUpLine;
        private bool _requiresTranslation;
        private string _initialTranslation;
        private string _manualTranslation;
        private bool _mergeHadRawChanges;

        [JsonProperty("s")]
        public int ItemSequence
        {
            get => _itemSequence;
            set
            {
                _itemSequence = value;
                NotifyOfPropertyChange(() => ItemSequence);
            }
        }

        [JsonProperty("r")]
        public string Raw
        {
            get => _raw; 
            set
            {
                _raw = value;
                NotifyOfPropertyChange(() => Raw);
            }
        }

        [JsonProperty("c")]
        public string CleanedUpLine
        {
            get => _cleanedUpLine;
            set
            {
                _cleanedUpLine = value;
                NotifyOfPropertyChange(() => CleanedUpLine);
            }
        }

        [JsonProperty("rq")]
        public bool RequiresTranslation
        {
            get => _requiresTranslation; 
            set
            {
                _requiresTranslation = value;
                NotifyOfPropertyChange(() => RequiresTranslation);
            }
        }

        [JsonProperty("it")]
        public string InitialTranslation
        {
            get => _initialTranslation; 
            set
            {
                _initialTranslation = value;
                NotifyOfPropertyChange(() => InitialTranslation);
                NotifyOfPropertyChange(() => ResultingTranslation);
            }
        }

        [JsonProperty("mt")]
        public string ManualTranslation
        {
            get => _manualTranslation; 
            set
            {
                _manualTranslation = value;
                NotifyOfPropertyChange(() => ManualTranslation);
                NotifyOfPropertyChange(() => ResultingTranslation);
            }
        }

        [JsonProperty("mc")]
        public bool MergeHadRawChanges
        {
            get => _mergeHadRawChanges; 
            set
            {
                _mergeHadRawChanges = value;
                NotifyOfPropertyChange(() => MergeHadRawChanges);
            }
        }

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
                    return string.Empty;
                //    return Raw;
            }
        }
    }
}
