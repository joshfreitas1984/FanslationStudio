namespace FanslationStudio.MigratedCatTool.Events
{
    public class RawLineCopiedEvent
    {
        public string RawLine;

        public RawLineCopiedEvent(string rawLine)
        {
            RawLine = rawLine;
        }
    }
}
