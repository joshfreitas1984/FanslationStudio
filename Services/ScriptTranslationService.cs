using FanslationStudio.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FanslationStudio.Services
{
    public class ScriptTranslationService
    {
        public static List<ScriptTranslation> LoadBulkScriptTranslations(string translatedFolder)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var response = new List<ScriptTranslation>();

            if (!Directory.Exists(translatedFolder))
                Directory.CreateDirectory(translatedFolder);

            foreach (var file in Directory.GetFiles(translatedFolder, "*.*", SearchOption.AllDirectories))
            {
                var script = LoadIndividualScriptTranslation(file);
                response.Add(script);
            }

            Console.WriteLine($"Using LoadBulkScriptTranslations, Time : {stopwatch.Elapsed.TotalMilliseconds}");

            return response;
        }

        public static ScriptTranslation LoadIndividualScriptTranslation(string file)
        {
            string contents = File.ReadAllText(file);
            return DeserializeContents(contents);
        }

        private static ScriptTranslation DeserializeContents(string contents)
        {
            return JsonConvert.DeserializeObject<ScriptTranslation>(contents,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
        }

        public static Dictionary<string, List<ScriptTranslation>> LoadTranslationsThatExist(Project project, string translatedFolder)
        {
            var stopwatch = Stopwatch.StartNew();

            var files = new Dictionary<string, List<string>>();
            var response = new Dictionary<string, List<ScriptTranslation>>();

            foreach (var scriptToTranslate in project.ScriptsToTranslate)
            {
                var fileContents = new List<string>();
                string folder = $"{translatedFolder}\\{scriptToTranslate.SourcePath}";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                foreach (var file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                {
                    fileContents.Add(File.ReadAllText(file));
                }

                files.Add(scriptToTranslate.SourcePath, fileContents);
            }
            
            Debug.WriteLine($"Using Loaded Files, Time : {stopwatch.Elapsed.TotalMilliseconds}");
            stopwatch.Restart();

            foreach (var file in files)
            {
                var deserialisedList = new ConcurrentBag<ScriptTranslation>();

                Parallel.ForEach(file.Value, indiv =>
                {
                    deserialisedList.Add(DeserializeContents(indiv));
                });

                response.Add(file.Key, deserialisedList.ToList());
            };

            Debug.WriteLine($"Using Serialised Files, Time : {stopwatch.Elapsed.TotalMilliseconds}");

            return response;
        }

        public static void WriteBulkScriptFiles(string translateFileFolder, List<ScriptTranslation> scripts, bool overwriteFiles)
        {
            if (!Directory.Exists(translateFileFolder))
                Directory.CreateDirectory(translateFileFolder);

            foreach (var script in scripts)
            {
                WriteIndividualScriptFile(translateFileFolder, script, overwriteFiles);
            }
        }

        public static void WriteIndividualScriptFile(string translateFileFolder, ScriptTranslation script, bool overwriteFiles)
        {
            string fileName = CalculateScriptFileName(translateFileFolder, script);

            if (File.Exists(fileName))
            {
                if (overwriteFiles)
                    File.Delete(fileName);
                else
                    return; //skip line
            }

            //Write translated items
            var jsonString = JsonConvert.SerializeObject(script,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                });

            File.WriteAllText(fileName, jsonString);
        }

        public static string CalculateScriptFileName(string translateFileFolder, ScriptTranslation script)
        {
            return $"{translateFileFolder}\\{script.LineId}.json";
        }

        public static string CalculateExportFileName(string folder, string sourcePath, int fileIndex)
        {
            return $"{folder}\\{sourcePath}.{fileIndex}.txt";
        }
    }
}
