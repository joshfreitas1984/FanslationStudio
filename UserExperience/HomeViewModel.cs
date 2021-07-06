using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Domain.ScriptToTranslate;
using FanslationStudio.UserExperience.Events;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience
{
    public class HomeViewModel : Screen
    {
        #region IHasProjectAndVersionViewModel

        private Config _config;

        public Config Config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
                NotifyOfPropertyChange(() => Config);
            }
        }

        #endregion

        private IEventAggregator _eventAggregator;
        private string _projectName;
        private string _versionName;
        private bool _dialogOpen;
        public string _rawInputFolder;

        public string ProjectName
        {
            get
            {
                return _projectName;
            }
            set
            {
                _projectName = value;
                NotifyOfPropertyChange(() => ProjectName);
            }
        }

        public string RawInputFolder
        {
            get
            {
                return _rawInputFolder;
            }

            set
            {
                _rawInputFolder = value;
                NotifyOfPropertyChange(() => RawInputFolder);
            }
        }

        public string VersionName
        {
            get
            {
                return _versionName;
            }

            set
            {
                _versionName = value;
                NotifyOfPropertyChange(() => VersionName);
            }
        }

        public bool IsDialogOpen
        {
            get
            {
                return _dialogOpen;
            }
            set
            {
                _dialogOpen = value;
                NotifyOfPropertyChange(() => IsDialogOpen);
            }
        }

        public HomeViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            Activated += OnActivate;
        }

        private void OnActivate(object sender, ActivationEventArgs e)
        {
            ProjectName = string.Empty;
            VersionName = "1.0";
        }

        public async void OpenProject()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                InitialDirectory = _config.WorkshopFolder,
                Filter = "Project files (*.project)|*.project|All files (*.*)|*.*",
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var project = Project.LoadProjectFile(openFileDialog.FileName);

                await _eventAggregator.PublishOnCurrentThreadAsync(
                    new SelectProjectEvent()
                    {
                        SelectedProject = project,
                    });
            }
        }

        public void CreateNewProject()
        {
            IsDialogOpen = true;
        }

        public async void AcceptNewProject()
        {
            IsDialogOpen = false;

            var project = new Project()
            {
                Name = ProjectName,
                Versions = new List<ProjectVersion>()
                {
                    new ProjectVersion()
                    {
                        Version = "1.0",
                        RawInputFolder = RawInputFolder,
                        SourceLanguageCode = "ZH",
                        TargetLanguageCode = "EN",
                    },
                },
                ScriptsToTranslate = new List<IScriptToTranslate>(),
            };

            project.CreateWorkspaceFolder(_config);
            project.WriteProjectFile();

            await _eventAggregator.PublishOnCurrentThreadAsync(
                new SelectProjectEvent()
                {
                    SelectedProject = project,
                });
        }

        public void CancelNewProject()
        {
            IsDialogOpen = false;
        }

        public void BrowseRawInput()
        {
            var openFileDialog = new CommonOpenFileDialog()
            {
                InitialDirectory = _config.WorkshopFolder,
                IsFolderPicker = true,
            };

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RawInputFolder = openFileDialog.FileName;
            }
        }
    }
}
