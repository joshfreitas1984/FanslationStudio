using FanslationStudio.Domain;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace FanslationStudio.Services
{
    public class ScriptTranslationService
    {
        public static List<ScriptTranslation> LoadScriptTranslations(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            string contents = File.ReadAllText(fileName);

           return JsonConvert.DeserializeObject<List<ScriptTranslation>>(contents,
               new JsonSerializerSettings
               {
                   TypeNameHandling = TypeNameHandling.Objects
               });
        }

        public static void WriteFiles(string newFileName, List<ScriptTranslation> lines)
        {
            //Write translated items
            var jsonString = JsonConvert.SerializeObject(lines,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                });

            File.WriteAllText(newFileName, jsonString);
        }
    }
}
