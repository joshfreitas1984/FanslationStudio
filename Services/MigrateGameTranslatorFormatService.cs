using FanslationStudio.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FanslationStudio.Services
{
    class MigrateGameTranslatorFormatService
    {
        //Lets Migrate the old HLTS files to new ones
        public static void MigrateGtRaws(Config config, Project project, ProjectVersion version)
        {
            foreach (var scriptToTranslate in project.ScriptsToTranslate)
            {
                //Calculate folders
                var projectFolder = ProjectFolderService.CalculateProjectFolder(config.WorkshopFolder, project.Name);
                var rawVersionFolder = ProjectFolderService.CalculateRawVersionFolder(projectFolder, version);
                var translatedVersionFolder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, version);

                var lines = scriptToTranslate.GetTranslationLines(rawVersionFolder);
                var gtlines = scriptToTranslate.GetTranslationLines(version.GameTranslatorSource);

                //Create Destination file
                string file = $"{rawVersionFolder}\\{scriptToTranslate.SourcePath}";
                string fileName = Path.GetFileName(scriptToTranslate.SourcePath);
                string fileSubFolder = Path.GetDirectoryName(file)
                    .Replace(rawVersionFolder, "");
                string newFileName = $"{translatedVersionFolder}{fileSubFolder}\\{fileName}.translate";
                string newFileFolder = Path.GetDirectoryName(newFileName);

                if (!Directory.Exists(newFileFolder))
                    Directory.CreateDirectory(newFileFolder);

                MergeGameTranslatorLines(lines, gtlines);

                //Write translated items
                if (File.Exists(newFileName))
                    File.Delete(newFileName);

                ScriptTranslationService.WriteFiles(newFileName, lines);
            }
        }

        private static void MergeGameTranslatorLines(List<ScriptTranslation> rawTrans, List<ScriptTranslation> gtTrans)
        {
            var rawLines = new Dictionary<string, ScriptTranslation>();
            var gtLines = new Dictionary<string, ScriptTranslation>();

            foreach (var line in rawTrans)
                rawLines.Add(line.LineId, line);

            foreach (var line in gtTrans)
                gtLines.Add(line.LineId, line);

            foreach (var line in rawLines)
            {
                if (gtLines.ContainsKey(line.Key))
                {
                    var gtLine = gtLines[line.Key];

                    for (int i = 0; i < line.Value.Items.Count; i++)
                    {
                        var item = line.Value.Items[i];
                        var gtItem = gtLine.Items[i];

                        if (item.RequiresTranslation)
                            item.InitialTranslation = gtItem.Raw;
                    }
                }
            }
        }
    }
}
