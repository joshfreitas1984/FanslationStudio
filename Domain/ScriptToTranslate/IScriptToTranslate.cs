using FanslationStudio.Domain;
using System.Collections.Generic;

namespace FanslationStudio.Domain.ScriptToTranslate
{
    public interface IScriptToTranslate
    {
        string SourcePath { get; set; }
        List<ScriptTranslation> GetTranslationLines(string rawFolder, Project project);
        void OutputLines(List<ScriptTranslation> scripts, string outputFolder, Project project);
    }
}
