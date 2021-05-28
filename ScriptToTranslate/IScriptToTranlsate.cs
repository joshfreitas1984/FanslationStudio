using FanslationStudio.Domain;
using System.Collections.Generic;

namespace FanslationStudio.ScriptToTranslate
{
    public interface IScriptToTranlsate
    {
        string SourcePath { get; set; }
        List<ScriptTranslation> GetTranslationLines(string rawFolder);
    }
}
