using FanslationStudio.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FanslationStudio.Domain
{

    public class ProjectVersion
    {
        private Config _config;
        private string _projectFolder;
        private string _rawFolder;
        private string _translationFolder;
        private string _rawFolderVersion;
        private string _translationFolderVersion;

        public string Version { get; set; }
        public string RawInputFolder { get; set; }
        public bool OldGameTranslator { get; set; }
        public string GameTranslatorSource { get; set; }

        public void CalculateFolders(Config config, string name)
        {
            _config = config;

            //_projectFolder = $"{_config.WorkshopFolder}\\{name}";
            //_rawFolder = $"{_projectFolder}\\Raw";
            //_translationFolder = $"{_projectFolder}\\Translation";
            //_rawFolderVersion = $"{_rawFolder}\\{Version}";
            //_translationFolderVersion = $"{_translationFolder}\\{Version}";

            _projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, name);
            _rawFolder = ProjectFolderService.CalculateRawFolder(_projectFolder);
            _translationFolder = ProjectFolderService.CalculateTranslationFolder(_projectFolder);
            _rawFolderVersion = ProjectFolderService.CalculateRawVersionFolder(_projectFolder, this);
            _translationFolderVersion = ProjectFolderService.CalculateTranslationVersionFolder(_projectFolder, this);
        }

        public void InitialiseProductVersion()
        {
            //Make sure the project folder is created
            if (!Directory.Exists(_projectFolder))
                Directory.CreateDirectory(_projectFolder);

            //Ensure working folders exist 
            if (!Directory.Exists(_rawFolder))
                Directory.CreateDirectory(_rawFolder);

            if (!Directory.Exists(_translationFolder))
                Directory.CreateDirectory(_translationFolder);

            //Ensure version folders exist            
            if (!Directory.Exists(_rawFolderVersion))
                Directory.CreateDirectory(_rawFolderVersion);

            if (!Directory.Exists(_translationFolderVersion))
                Directory.CreateDirectory(_translationFolderVersion);

            //Copy raw input to raws folder
            foreach (var file in Directory.GetFiles(RawInputFolder, "*.*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(file);
                string fileSubFolder = Path.GetDirectoryName(file)
                    .Replace(RawInputFolder, "");
                string newFileName = $"{_rawFolderVersion}{fileSubFolder}\\{fileName}";

                string newFileFolder = Path.GetDirectoryName(newFileName);

                if (!Directory.Exists(newFileFolder))
                    Directory.CreateDirectory(newFileFolder);

                if (!File.Exists(newFileName))
                    File.Copy(file, newFileName, true);
            }
        }

        public void LoadTranslationsThatExist(Project project)
        {
        }

        public void LoadTranslationsFromRaws(Project project)
        {
            foreach (var scriptToTranslate in project.ScriptsToTranslate)
            {
                //Create Destination file
                string file = $"{_rawFolderVersion}\\{scriptToTranslate.SourcePath}";
                string fileName = Path.GetFileName(scriptToTranslate.SourcePath);
                string fileSubFolder = Path.GetDirectoryName(file)
                    .Replace(_rawFolderVersion, "");
                string newFileName = $"{_translationFolderVersion}{fileSubFolder}\\{fileName}.translate";
                string newFileFolder = Path.GetDirectoryName(newFileName);

                if (!Directory.Exists(newFileFolder))
                    Directory.CreateDirectory(newFileFolder);

                if (File.Exists(newFileName))
                    continue;
                //    File.Delete(newFileName);

                var lines = scriptToTranslate.GetTranslationLines(_rawFolderVersion);

                //Write translated items
                ScriptTranslationService.WriteFiles(newFileName, lines);                
            }
        }        
    }
}
