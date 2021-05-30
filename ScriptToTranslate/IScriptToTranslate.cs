using FanslationStudio.Domain;
using System.Collections.Generic;

namespace FanslationStudio.ScriptToTranslate
{
    public interface IScriptToTranslate
    {
        string SourcePath { get; set; }
        List<ScriptTranslation> GetTranslationLines(string rawFolder);
        void OutputLines(List<ScriptTranslation> scripts, string outputFolder);
    }
}
