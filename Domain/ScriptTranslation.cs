using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FanslationStudio.Domain
{
    public class ScriptTranslation
    {
        public string LineId { get; set; }
        public string RawLine { get; set; }

        public List<ScriptTranslationItem> Items { get; set; }

        public string ToExportLine(ScriptTranslationItem item)
        {
            string exportFormat = "<id={0} seq={1}/>{2}";

            return string.Format(exportFormat,
                LineId, item.ItemSequence, item.CleanedUpLine);
        }

        public void UpdateFromExportLine(string exportLine, int sequence)
        {
            var item = Items.FirstOrDefault(i => i.ItemSequence == sequence);

            if (item != null)
                item.ManualTranslation = exportLine;
        }
    }
}
