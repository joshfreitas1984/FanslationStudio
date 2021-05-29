using Caliburn.Micro;
using FanslationStudio.Domain;
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

            //project.CalculateFolders(_config);

            //foreach (var version in project.Versions)
            //{
            //    version.InitialiseProductVersion();
            //    //version.LoadTranslationsThatExist(project);
            //    //version.LoadTranslationsFromRaws(project);

            //    //if (version.OldGameTranslator)
            //    //    version.MigrateGtRaws(project);
            //}

            //Migrate 1.29 to 1.33
            //MergeVersionService.MergeOldVersion(_config, project, project.Versions[0], project.Versions[1]);

            ShowProject();
        }

        public void SelectProject(string fileName)
        {
            _currentProject = Project.LoadProjectFile(@"X:\FanslationStudioWorkshop\HLTS.project");
            _currentProject.CalculateFolders(_config);

            SelectVersion(_currentProject.Versions.Last());
        }

        public void SelectVersion(ProjectVersion version)
        {
            _currentVersion = version;
            version.InitialiseProductVersion();
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
