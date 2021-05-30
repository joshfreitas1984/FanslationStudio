using FanslationStudio.ScriptToTranslate;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace FanslationStudio.Domain
{
    public class Project
    {
        private Config _config;
        private string _projectFile;

        public string Name { get; set; }

        public IScriptToTranslate[] ScriptsToTranslate { get; set; }

        public List<ProjectVersion> Versions { get; set; }

        public string ProjectFile {  get { return _projectFile;  } }

        /// <summary>
        /// Loads project file for JSON file and returns project object
        /// </summary>
        public static Project LoadProjectFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            string contents = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<Project>(contents,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
        }

        /// <summary>
        /// Saves Project file into workshop directory
        /// </summary>
        public void WriteProjectFile()
        {
            string jsonString = JsonConvert.SerializeObject(this,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                });

            if (File.Exists(_projectFile))
                File.Delete(_projectFile);

            File.WriteAllText(_projectFile, jsonString);
        }

        public void CreateWorkspaceFolder(Config config)
        {
            _config = config;
            _projectFile = $"{_config.WorkshopFolder}\\{Name}.project";

            foreach (var version in Versions)
            {
                version.CreateWorkspaceFolders(_config, Name);
            }
        }
    }
}
