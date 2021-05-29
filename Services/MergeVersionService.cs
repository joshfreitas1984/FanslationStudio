﻿using FanslationStudio.Domain;
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

                ScriptTranslation previousTrans = ScriptTranslationService.LoadIndividualScriptTranslation(oldFile);
                ScriptTranslation newTrans = ScriptTranslationService.LoadIndividualScriptTranslation(newFile);

                MergeVersions(previousTrans, newTrans);

                ScriptTranslationService.WriteIndividualScriptFile(Path.GetDirectoryName(newFile), newTrans, true);
            }
        }

        private static void MergeVersions(ScriptTranslation previousVersionTrans, ScriptTranslation newVersionTrans)
        {
            //WARNING: Will only work if the splits are the same
            for(int i = 0; i < previousVersionTrans.Items.Count; i++)
            {
                var prevItem = previousVersionTrans.Items[i];
                var newItem = newVersionTrans.Items[i];

                if (newItem.RequiresTranslation)
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
