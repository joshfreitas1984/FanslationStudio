using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.Domain
{
    public class ScriptSearchResult
    {
        public string SourcePath { get; set; }
        public ScriptTranslation Script { get; set; }
        public ScriptTranslationItem Item { get; set; }
        public string Find { get; set; }
        public string Replace { get; set; }
    }
}
