using FanslationStudio.ScriptToTranslate;
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
        private string _rawFolderVersion;
        private string _translationFolderVersion;

        public string Version { get; set; }
        public string RawInputFolder { get; set; }
        public bool OldGameTranslator { get; set; }
        public string GameTranslatorSource { get; set; }

        public void CreateWorkspaceFolders(Config config, Project project)
        {
            _config = config;

            _projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, project.Name);
            _rawFolderVersion = ProjectFolderService.CalculateRawVersionFolder(_projectFolder, this);
            _translationFolderVersion = ProjectFolderService.CalculateTranslationVersionFolder(_projectFolder, this);

            //Make sure the project folder is created
            if (!Directory.Exists(_projectFolder))
                Directory.CreateDirectory(_projectFolder);

            //Ensure version folders exist            
            if (!Directory.Exists(_rawFolderVersion))
                Directory.CreateDirectory(_rawFolderVersion);

            if (!Directory.Exists(_translationFolderVersion))
                Directory.CreateDirectory(_translationFolderVersion);
        }

        public void CopyRawsIntoWorkspaceFolder()
        {
            //Copy raw input to raws folder
            foreach (var file in Directory.GetFiles(RawInputFolder, "*.*", SearchOption.AllDirectories))
            {
                string newFileName = file.Replace(RawInputFolder, _rawFolderVersion);
                string newFileFolder = Path.GetDirectoryName(newFileName);

                if (!Directory.Exists(newFileFolder))
                    Directory.CreateDirectory(newFileFolder);

                if (!File.Exists(newFileName))
                    File.Copy(file, newFileName, true);
            }
        }

        public void ImportRawLinesAsTranslations(Project project)
        {
            foreach (var scriptToTranslate in project.ScriptsToTranslate)
            {
                string translatedFolder = $"{_translationFolderVersion}\\{scriptToTranslate.SourcePath}";
                
                //Load raw script entries
                var scripts = scriptToTranslate.GetTranslationLines(_rawFolderVersion);

                //Go through each script and add it if its missing
                ScriptTranslationService.WriteBulkScriptFiles(translatedFolder, scripts, false);
            }
        }    
        
        public void GenerateOutput(Project project)
        {
            string outputFolder = ProjectFolderService.CalculateOutputVersionFolder(_projectFolder, this);

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            foreach (var scriptToTranslate in project.ScriptsToTranslate)
            {
                string translatedFolder = $"{_translationFolderVersion}\\{scriptToTranslate.SourcePath}";

                //Load last saved translation
                var scripts = ScriptTranslationService.LoadBulkScriptTranslations(translatedFolder);
                scriptToTranslate.OutputLines(scripts, outputFolder);
            }
        }
    }
}
