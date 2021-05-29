using FanslationStudio.Domain;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace FanslationStudio.Services
{
    public class ScriptTranslationService
    {
        public static List<ScriptTranslation> LoadBulkScriptTranslations(string translatedFolder)
        {
            var response = new List<ScriptTranslation>();

            if (!Directory.Exists(translatedFolder))
                Directory.CreateDirectory(translatedFolder);

            foreach (var file in Directory.GetFiles(translatedFolder, "*.*", SearchOption.AllDirectories))
            {
                var script = LoadIndividualScriptTranslation(file);
                response.Add(script);
            }

            return response;
        }

        public static ScriptTranslation LoadIndividualScriptTranslation(string file)
        {
            string contents = File.ReadAllText(file);

            var script = JsonConvert.DeserializeObject<ScriptTranslation>(contents,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
            return script;
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
    }
}
