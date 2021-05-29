using FanslationStudio.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FanslationStudio.Services
{
    public static class MigrateGameTranslatorFormatService
    {
        //Lets Migrate the old HLTS files to new ones
        public static void MigrateGtRaws(Config config, Project project, ProjectVersion version)
        {
            foreach (var scriptToTranslate in project.ScriptsToTranslate)
            {
                //Calculate folders
                var projectFolder = ProjectFolderService.CalculateProjectFolder(config.WorkshopFolder, project.Name);
                var translatedVersionFolder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, version);

                //var lines = scriptToTranslate.GetTranslationLines(rawVersionFolder);
                var oldTransLines = scriptToTranslate.GetTranslationLines(version.GameTranslatorSource);
                
                foreach(var oldGtScript in oldTransLines)
                {
                    var folder = $"{translatedVersionFolder}\\{scriptToTranslate.SourcePath}";
                    string fileName = ScriptTranslationService.CalculateScriptFileName(folder, oldGtScript);

                    if (File.Exists(fileName))
                    {
                        var newRawScript = ScriptTranslationService.LoadIndividualScriptTranslation(fileName);

                        MergeGameTranslatorScript(newRawScript, oldGtScript);

                        ScriptTranslationService.WriteIndividualScriptFile(folder, newRawScript, true);
                    }
                }
            }
        }

        private static void MergeGameTranslatorScript(ScriptTranslation rawScript, ScriptTranslation gtScript)
        {
            for (int i = 0; i < rawScript.Items.Count; i++)
            {
                var item = rawScript.Items[i];
                var gtItem = gtScript.Items[i];

                if (item.RequiresTranslation)
                    item.InitialTranslation = gtItem.Raw;
            }
        }
    }
}
