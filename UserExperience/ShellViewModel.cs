using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FanslationStudio.UserExperience
{

    public class ShellViewModel : Conductor<object>
    {
        private Config _config;
        private Project _currentProject;
        private ProjectVersion _currentVersion;
        private string _title = "Fanslation Studio - Loading...";
        private Dictionary<string, List<ScriptTranslation>> _scripts;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public ShellViewModel()
        {
        }

        protected override void OnViewLoaded(object view)
        {
            _config = Config.LoadConfig();
            _config.WriteConfig();

            //Start Loading on a background thread
            Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(_config.LastProjectFile))
                {
                    SelectProject(_config.LastProjectFile);
                }
                else
                {
                    var openFileDialog = new System.Windows.Forms.OpenFileDialog()
                    {
                        InitialDirectory = _config.WorkshopFolder,
                        Filter = "Project files (*.project)|*.project|All files (*.*)|*.*",
                    };

                    if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        SelectProject(openFileDialog.FileName);
                }

                _currentProject.CreateWorkspaceFolder(_config);

                ShowProject();
            });

            //Testing logic
            //foreach (var version in _currentProject.Versions)
            //{
            //    //version.CopyRawsIntoWorkspaceFolder();
            //    //version.ImportRawLinesAsTranslations(_currentProject);

            //    //var x = version.LoadTranslationsThatExist(_currentProject);

            //    // if (version.OldGameTranslator)
            //    //    MigrateGameTranslatorFormatService.MigrateGtRaws(_config, _currentProject, version);

            //    //version.GenerateOutput(_currentProject);
            //}

            //Migrate 1.29 to 1.33
            //MergeVersionService.MergeOldVersion(_config, _currentProject, 
            //    _currentProject.Versions[0], _currentProject.Versions[1]);
        }

        public void SelectProject(string fileName)
        {
            _currentProject = Project.LoadProjectFile(fileName);
            _currentProject.CreateWorkspaceFolder(_config);

            SelectVersion(_currentProject.Versions.Last());

            //Save last config file
            _config.LastProjectFile = fileName;
            _config.WriteConfig();
        }

        public void SelectVersion(ProjectVersion version)
        {
            _currentVersion = version;

            string projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _currentProject.Name);
            string folder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, _currentVersion);
            _scripts = ScriptTranslationService.LoadTranslationsThatExist(_currentProject, folder);
            
            //Remove Post Processing
            //foreach (var script in _scripts)
            //{
            //    string translatedFolder = $"{folder}\\{script.Key}";

            //    foreach (var item in script.Value)
            //    {
            //        foreach (var newItem in item.Items)
            //        {
            //            //Remove post processing
            //            if (newItem.RequiresTranslation && newItem.InitialTranslation != null && newItem.InitialTranslation.Contains("<size"))
            //                newItem.InitialTranslation = newItem.InitialTranslation
            //                    .Replace("<size=18>", "")
            //                    .Replace("<size=20>", "")
            //                    .Replace("<size=16>", "")
            //                    .Replace("</size>", "");
            //        }
            //    }

            //    //Go through each script and add it if its missing
            //    ScriptTranslationService.WriteBulkScriptFiles(translatedFolder, script.Value, true);
            //}

            SetTitle();            
        }

        public void SetTitle()
        {
            Title = $"FanslationStudio :: {_currentProject.Name} - Version: {_currentVersion.Version}";
        }

        //Method to handle navigation because we cant get caliburn to pipe through
        public void TabSelectionChanged(object args)
        {
            if (args is TabControl)
            {
                string name = ((args as TabControl).SelectedItem as TabItem).Name;
                switch (name)
                {
                    case "Project":
                        ShowProject();
                        break;
                    case "ManualTranslate":
                        ShowManualTranslate();
                        break;
                    case "ImportBulk":
                        ShowImportBulk();
                        break;
                    case "GenerateOutput":
                        ShowGenerateOutput();
                        break;
                }
            }
        }

        public async void ShowProject()
        {
            var vm = IoC.Get<ProjectViewModel>();
            vm.Config = _config;
            vm.Project = _currentProject;
            vm.Version = _currentVersion;

            await ActivateItemAsync(vm);
        }

        public async void ShowManualTranslate()
        {
            var vm = IoC.Get<ManualTranslateViewModel>();
            vm.Config = _config;
            vm.Project = _currentProject;
            vm.Version = _currentVersion;
            vm.Scripts = _scripts;

            await ActivateItemAsync(vm);
        }

        public async void ShowImportBulk()
        {
            var vm = IoC.Get<ImportBulkViewModel>();
            vm.Config = _config;
            vm.Project = _currentProject;
            vm.Version = _currentVersion;
            vm.Scripts = _scripts;

            await ActivateItemAsync(vm);
        }

        public async void ShowGenerateOutput()
        {
            var vm = IoC.Get<GenerateOutputViewModel>();
            vm.Config = _config;
            vm.Project = _currentProject;
            vm.Version = _currentVersion;
            vm.Scripts = _scripts;

            await ActivateItemAsync(vm);
        }
    }
}
