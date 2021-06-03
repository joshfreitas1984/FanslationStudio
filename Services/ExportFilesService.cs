using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FanslationStudio.Services
{
    public static class ExportFilesService
    {
        public static void ExportBatchFiles(List<ScriptSearchResult> SearchResults, string projectFolder, ProjectVersion version)
        {
            int characterLimit = 4500; //For some reason the character calculator is off so we go under the 5k limit

            int currentCharacters = 0;
            var exportItems = new List<string>();
            string exportFolder = ProjectFolderService.CalculateExportVersionFolder(projectFolder, version);

            if (!Directory.Exists(exportFolder))
                Directory.CreateDirectory(exportFolder);

            var items = SearchResults.OrderBy(s => s.SourcePath);
            string currentFile = items.FirstOrDefault()?.SourcePath;
            int currentFileIndex = 1;

            foreach (var result in items)
            {
                //Primary Read
                if (currentFile != result.SourcePath)
                {
                    WriteExportLines(exportItems, exportFolder, currentFile, currentFileIndex);

                    exportItems.Clear();
                    currentFile = result.SourcePath;
                    currentFileIndex = 1;
                    currentCharacters = 0;
                }

                string exportLine = result.Script.ToExportLine(result.Item);
                currentCharacters += exportLine.Length;

                if (currentCharacters > characterLimit)
                {
                    WriteExportLines(exportItems, exportFolder, currentFile, currentFileIndex);
                    exportItems.Clear();
                    currentFileIndex++;
                    currentCharacters = 0;
                }

                exportItems.Add(exportLine);
            }

            //Trailing Write
            if (exportItems.Any())
            {
                WriteExportLines(exportItems, exportFolder, currentFile, currentFileIndex);
            }
        }

        private static void WriteExportLines(List<string> exportItems, string exportFolder, string currentFile, int fileIndex)
        {
            //Write Export Items
            var file = ScriptTranslationService.CalculateExportFileName(exportFolder, currentFile, fileIndex);
            string folder = Path.GetDirectoryName(file);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllLines(file, exportItems);
        }
    }
}
