using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FanslationStudio.Domain
{
    public class Config
    {
        static string _fileName = "config.json";

        public string WorkshopFolder { get; set; }
        public List<SearchPattern> SearchPatterns { get; set; }

        public static Config LoadConfig()
        {
            if (!File.Exists(_fileName))
            {
                //Default Config
                return new Config()
                {
                    WorkshopFolder = @".\Workshop",
                };
            }

            string contents = File.ReadAllText(_fileName);
            return JsonConvert.DeserializeObject<Config>(contents,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                });
        }

        public void WriteConfig()
        {
            string jsonString = JsonConvert.SerializeObject(this,
                 Formatting.Indented,
                 new JsonSerializerSettings
                 {
                     TypeNameHandling = TypeNameHandling.Objects,
                     TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                 });

            if (File.Exists(_fileName))
                File.Delete(_fileName);

            File.WriteAllText(_fileName, jsonString);
        }
    }

    public class SearchPattern
    {
        public string Find { set; get; }
        public string Replacement { set; get; }
        public bool CaseSensitive { set; get; }

        [JsonIgnore]
        public bool IsRegex
        {
            get
            {
                return (Find.Contains("[") || Find.Contains("^") || Find.Contains("/"));
            }
        }
    }
}
