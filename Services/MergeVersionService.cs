using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FanslationStudio.Services
{

    public class MergeVersionService
    {
        public static void MergeOldVersion(Config config, Project project, ProjectVersion previousVersion, ProjectVersion newVersion)
        {
            //Calculate folders
            var projectFolder = ProjectFolderService.CalculateProjectFolder(config.WorkshopFolder, project.Name);
            var previousVersionFolder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, previousVersion);
            var newVersionFolder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, newVersion);

            //Deserialise the files
            foreach (var oldFile in Directory.GetFiles(previousVersionFolder, "*.*", SearchOption.AllDirectories))
            {
                string newFile = oldFile.Replace(previousVersionFolder, newVersionFolder);

                List<ScriptTranslation> previousTrans = ScriptTranslationService.LoadScriptTranslations(oldFile);
                List<ScriptTranslation> newTrans = ScriptTranslationService.LoadScriptTranslations(newFile);

                MergeVersions(previousTrans, newTrans);

                File.Delete(newFile);
                ScriptTranslationService.WriteFiles(newFile, newTrans);
            }
        }

        private static void MergeVersions(List<ScriptTranslation> previousVersionTrans, List<ScriptTranslation> newVersionTrans)
        {
            var previousVersionLines = new Dictionary<string, ScriptTranslation>();
            var newVersionLines = new Dictionary<string, ScriptTranslation>();

            //Look at dictionaries
            foreach (var line in previousVersionTrans)
                previousVersionLines.Add(line.LineId, line);

            foreach (var line in newVersionTrans)
                newVersionLines.Add(line.LineId, line);

            foreach (var prevLine in previousVersionLines)
            {
                //Merg lines
                if (newVersionLines.ContainsKey(prevLine.Key))
                {
                    var newLine = newVersionLines[prevLine.Key];

                    for (int i = 0; i < prevLine.Value.Items.Count; i++)
                    {
                        var prevItem = prevLine.Value.Items[i];
                        var newItem = newLine.Items[i];

                        if (prevItem.RequiresTranslation)
                        {
                            newItem.InitialTranslation = prevItem.ResultingTranslation;
                            newItem.ManualTranslation = string.Empty;

                            if (prevItem.Raw != newItem.Raw)
                                newItem.MergeHadRawChanges = true;
                        }
                    }
                }
            }
        }
    }
}
