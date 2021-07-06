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
        public static void ExportBulkFiles(List<ScriptSearchResult> SearchResults, string projectFolder, ProjectVersion version)
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

        public static void ImportLines(Dictionary<string, List<ScriptTranslation>> scripts, string exportFolder)
        {
            var response = new List<ScriptTranslation>();

            if (!Directory.Exists(exportFolder))
                Directory.CreateDirectory(exportFolder);

            foreach (var sourceFile in scripts)
            {
                string fileName = Path.GetFileName(sourceFile.Key);

                //Get Export files that match
                foreach (var file in Directory.GetFiles(exportFolder, $"{fileName}.*", SearchOption.AllDirectories))
                {
                    var lines = File.ReadAllLines(file);

                    foreach (var line in lines)
                    {
                        //Skip blank lines
                        if (string.IsNullOrEmpty(line))
                            continue;

                        //Get id
                        int endIndex = line.IndexOf("/>");
                        if (endIndex == -1)
                            continue; 

                        string lineTag = line.Substring(0, endIndex);

                        //Validate the tag is valid too - Deepl can mess it up too
                        if (lineTag.IndexOf("id=") == -1 || lineTag.IndexOf("seq=") == -1)
                            continue;

                        var splits = lineTag.Split(" ");
                        string lineId = splits[0].Replace("<id=", "");
                        string sequence = splits[1].Replace("seq=", "");
                        string translatedLine = line.Substring(endIndex + 2, line.Length - endIndex - 2);

                        //If there is multiple id lines then its completely messed up that line (Frequent with DeepL free license)
                        if (translatedLine.IndexOf("<id=") >= 0)
                            continue;

                        //Get item and update it
                        var item = sourceFile.Value.Where(s => s.LineId == lineId).FirstOrDefault();
                        item?.UpdateFromExportLine(translatedLine, Convert.ToInt32(sequence));
                    }
                }
            }
        }
    }
}
