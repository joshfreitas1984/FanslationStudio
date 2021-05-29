﻿using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FanslationStudio.UserExperience
{
    public class ShellViewModel : Conductor<object>
    {
        private Config _config;
        private Project _currentProject;
        private ProjectVersion _currentVersion;
        private string _title;

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
            SetTitle();
        }

        public void SetTitle()
        {
            Title = $"FanslationStudio :: {_currentProject.Name} - Version: {_currentVersion.Version}";
        }

        public async void ShowProject()
        {
            await ActivateItemAsync(new ProjectViewModel());
        }
    }
}
