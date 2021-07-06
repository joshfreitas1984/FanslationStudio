namespace FanslationStudio.UserExperience.Events
{
    public class RawLineCopiedEvent
    {
        public string RawLine { get; set; }
        public string SourceLanguageCode { get; set; }
        public string TargetLanguageCode { get; set; }

        public RawLineCopiedEvent(string rawLine, string sourceLanguageCode, string targetLanugageCode)
        {
            RawLine = rawLine;
            SourceLanguageCode = sourceLanguageCode;
            TargetLanguageCode = targetLanugageCode;
        }
    }
}
