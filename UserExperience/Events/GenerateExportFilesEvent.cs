using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience.Events
{
    public class GenerateExportFilesEvent
    {
        public List<ScriptSearchResult> SearchResults { get; set; }
    }
}
