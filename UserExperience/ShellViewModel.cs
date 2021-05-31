﻿using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FanslationStudio.UserExperience
{

    public class ShellViewModel : Conductor<object>
    {       
        private Config _config;
        private Project _currentProject;
        private ProjectVersion _currentVersion;
        private string _title = "Title";
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
            _config = Config.LoadConfig();
            _config.WriteConfig();

            SelectProject(@"X:\FanslationStudioWorkshop\HLTS.project");

            _currentProject.CreateWorkspaceFolder(_config);

            //Lets regenerate everything
            foreach (var version in _currentProject.Versions)
            {
                //version.CopyRawsIntoWorkspaceFolder();
                //version.ImportRawLinesAsTranslations(_currentProject);

                //var x = version.LoadTranslationsThatExist(_currentProject);

                // if (version.OldGameTranslator)
                //    MigrateGameTranslatorFormatService.MigrateGtRaws(_config, _currentProject, version);

                //version.GenerateOutput(_currentProject);
            }

            //Migrate 1.29 to 1.33
            //MergeVersionService.MergeOldVersion(_config, _currentProject, 
            //    _currentProject.Versions[0], _currentProject.Versions[1]);

            ShowProject();
        }

        public void SelectProject(string fileName)
        {
            _currentProject = Project.LoadProjectFile(@"X:\FanslationStudioWorkshop\HLTS.project");
            _currentProject.CreateWorkspaceFolder(_config);

            SelectVersion(_currentProject.Versions.Last());
        }

        public void SelectVersion(ProjectVersion version)
        {
            _currentVersion = version;

            string projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _currentProject.Name);
            string folder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, _currentVersion);
            _scripts = ScriptTranslationService.LoadTranslationsThatExist(_currentProject, folder);

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
                switch(name)
                {
                    case "Project":
                        ShowProject();
                        break;
                    case "ManualTranslate":
                        ShowManualTranslate();
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
    }
}
