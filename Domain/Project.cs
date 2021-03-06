using FanslationStudio.Domain.PostProcessing;
using FanslationStudio.Domain.PreProcessing;
using FanslationStudio.Domain.ScriptToTranslate;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace FanslationStudio.Domain
{
    public class Project
    {
        private Config _config;

        public string Name { get; set; }

        public List<IScriptToTranslate> ScriptsToTranslate { get; set; }

        public List<ProjectVersion> Versions { get; set; }

        public List<IPreProcessing> PreProcessingItems { get; set; }

        public List<IPostProcessing> PostProcessingItems { get; set; }

        public List<SearchPattern> SearchPatterns { get; set; }

        public string ProjectFile { get; set; }

        public string LastBulkImportFolder { get; set; }

        public Project()
        {
            PreProcessingItems = new List<IPreProcessing>();
            PostProcessingItems = new List<IPostProcessing>();
            SearchPatterns = new List<SearchPattern>();
            Versions = new List<ProjectVersion>();
            ScriptsToTranslate = new List<IScriptToTranslate>();
        }

        /// <summary>
        /// Loads project file for JSON file and returns project object
        /// </summary>
        public static Project LoadProjectFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            string contents = File.ReadAllText(fileName);

            var result =  JsonConvert.DeserializeObject<Project>(contents,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });

            //Override the file name in case we copied it
            result.ProjectFile = fileName;

            return result;
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

            if (File.Exists(ProjectFile))
                File.Delete(ProjectFile);

            File.WriteAllText(ProjectFile, jsonString);
        }

        public void CreateWorkspaceFolder(Config config)
        {
            _config = config;

            //Initialise Project file in the workshop folder
            ProjectFile = $"{_config.WorkshopFolder}\\{Name}.project";

            foreach (var version in Versions)
            {
                version.CreateWorkspaceFolders(_config, this);
            }
        }        
    }
}
