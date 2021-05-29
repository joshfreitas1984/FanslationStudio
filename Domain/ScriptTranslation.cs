using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.Domain
{
    public class ScriptTranslation
    {
        public string LineId { get; set; }
        public string RawLine { get; set; }

        public List<ScriptTranslationItem> Items { get; set; }
    }
}
