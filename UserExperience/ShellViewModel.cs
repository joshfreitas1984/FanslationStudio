using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using FanslationStudio.UserExperience.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FanslationStudio.UserExperience
{

    public class ShellViewModel : Conductor<object>, IHandle<SelectProjectEvent>, IHandle<SelectProjectVersionEvent>, 
        IHandle<ImportProjectVersionEvent>, IHandle<GenerateOutputFilesEvent>, IHandle<GenerateExportFilesEvent>,
        IHandle<ImportExportFilesEvent>, IHandle<MergeVersionEvent>
    {
        private IEventAggregator _eventAggregator;
        private Config _config;
        private Project _currentProject;
        private ProjectVersion _currentVersion;
        private string _title = "Fanslation Studio - Select a Project";
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

        public bool HasProjectVersion
        {
            get
            {
                return (_currentVersion != null);
            }
        }

        public ShellViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected override void OnViewLoaded(object view)
        {
            _config = Config.LoadConfig();
            _config.WriteConfig();

            ShowHome();

            Task.Run(() =>
            {
                new TcpMessagingService().RunServer(_eventAggregator);
            });
        }     

        public void SelectProject(Project project)
        {
            _currentProject = project;
            SelectVersion(_currentProject.Versions.Last());

            //Save last config file
            _config.LastProjectFile = _currentProject.ProjectFile;
            _config.WriteConfig();
        }

        public void SelectVersion(ProjectVersion version)
        {
            _currentVersion = version;
            IsLoadingProjectDialogOpen = true;

            Task.Run(() =>
            {
                string projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _currentProject.Name);
                string folder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, _currentVersion);
                _scripts = ScriptTranslationService.LoadTranslationsThatExist(_currentProject, folder);

                Title = $"FanslationStudio :: {_currentProject.Name} - Version: {_currentVersion.Version}";

                Thread.Sleep(500); //Avoid flicker
                IsLoadingProjectDialogOpen = false;
                NotifyOfPropertyChange(() => HasProjectVersion);
            });            
        }

        public void ImportVersion(ProjectVersion version)
        {            
            IsImportingRawDialogOpen = true;

            Task.Run(() =>
            {
                version.CreateWorkspaceFolders(_config, _currentProject);
                version.CopyRawsIntoWorkspaceFolder();
                version.ImportRawLinesAsTranslations(_currentProject);

                Thread.Sleep(500); //Avoid flicker
                IsImportingRawDialogOpen = false;
            });            
        }

        public void OuputFiles(ProjectVersion version)
        {
            IsGeneratingOutputDialogOpen = true;

            Task.Run(() =>
            {
                version.CreateWorkspaceFolders(_config, _currentProject);
                version.GenerateOutput(_currentProject);

                Thread.Sleep(500); //Avoid flicker
                IsGeneratingOutputDialogOpen = false;
            });
        }

        public void ImportBulkFiles(string importFolder)
        {
            string projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _currentProject.Name);
            string translationFolderVersion = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, _currentVersion);

            IsImportingBulkFilesDialogOpen = true;

            Task.Run(() =>
            {
                ExportFilesService.ImportLines(_scripts, importFolder);

                foreach (var script in _scripts)
                {
                    string translatedFolder = $"{translationFolderVersion}\\{script.Key}";

                    //Go through each script and add it if its missing
                    ScriptTranslationService.WriteBulkScriptFiles(translatedFolder, script.Value, true);
                }

                Thread.Sleep(500); //Avoid flicker
                IsImportingBulkFilesDialogOpen = false;
            });
        }

        public void ExportBulkFiles(List<ScriptSearchResult> searchResults)
        {
            IsGeneratingExportDialogOpen = true;

            Task.Run(() =>
            {
                string projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _currentProject.Name);
                ExportFilesService.ExportBulkFiles(searchResults, projectFolder, _currentVersion);

                Thread.Sleep(500); //Avoid flicker
                IsGeneratingExportDialogOpen = false;
            });
        }

        public void MergeVersions(Project project, ProjectVersion versionToMerge, ProjectVersion targetVersion)
        {
            IsImportingRawDialogOpen = true;

            Task.Run(() =>
            {
                MergeVersionService.MergeOldVersion(_config, project, versionToMerge, targetVersion);

                Thread.Sleep(500); //Avoid flicker
                IsImportingRawDialogOpen = false;
            });
        }

        //Method to handle navigation because we cant get caliburn to pipe through
        public void TabSelectionChanged(TabControl tabControl)
        {
            var name = (tabControl.SelectedItem as TabItem)?.Name;

            switch (name)
            {
                case "Home":
                    ShowHome();
                    break;
                case "Project":
                    ShowProject();
                    break;
                case "ManualTranslate":
                    ShowManualTranslate();
                    break;
            }
        }

        public async void ShowHome()
        {
            var vm = IoC.Get<HomeViewModel>();
            vm.Config = _config;

            await ActivateItemAsync(vm);
            await _eventAggregator.PublishOnUIThreadAsync(new SelectTabForViewEvent("Home"));
        }

        public async void ShowProject()
        {
            var vm = IoC.Get<ProjectViewModel>();
            vm.Config = _config;
            vm.Project = _currentProject;
            vm.Version = _currentVersion;

            await ActivateItemAsync(vm);
            await _eventAggregator.PublishOnUIThreadAsync(new SelectTabForViewEvent("Project"));
        }

        public async void ShowManualTranslate()
        {
            var vm = IoC.Get<ManualTranslateViewModel>();
            vm.Config = _config;
            vm.Project = _currentProject;
            vm.Version = _currentVersion;
            vm.Scripts = _scripts;

            await ActivateItemAsync(vm);
            await _eventAggregator.PublishOnUIThreadAsync(new SelectTabForViewEvent("ManualTranslate"));
        }

        public async Task HandleAsync(SelectProjectEvent message, CancellationToken cancellationToken)
        {
            SelectProject(message.SelectedProject);
            ShowProject();

            await Task.CompletedTask;
        }

        public async Task HandleAsync(SelectProjectVersionEvent message, CancellationToken cancellationToken)
        {
            SelectVersion(message.SelectedVersion);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(ImportProjectVersionEvent message, CancellationToken cancellationToken)
        {
            ImportVersion(message.SelectedVersion);
            await Task.CompletedTask;
        }
        
        public async Task HandleAsync(ImportExportFilesEvent message, CancellationToken cancellationToken)
        {
            ImportBulkFiles(message.ImportFolder);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(GenerateExportFilesEvent message, CancellationToken cancellationToken)
        {
            ExportBulkFiles(message.SearchResults);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(GenerateOutputFilesEvent message, CancellationToken cancellationToken)
        {
            OuputFiles(message.SelectedVersion);
            await Task.CompletedTask;
        }


        public async Task HandleAsync(MergeVersionEvent message, CancellationToken cancellationToken)
        {
            MergeVersions(message.Project, message.VersionToMerge, message.TargetVersion);
            await Task.CompletedTask;
        }

        #region Dialog Management

        private bool _isLoadingProjectDialogOpen;
        private bool _isGeneratingOutputDialogOpen;
        private bool _isGeneratingExportDialogOpen;
        private bool _isImportingRawDialogOpen;
        private bool _isImportingBulkFilesDialogOpen;

        public bool IsDialogOpen
        {
            get
            {
                return _isLoadingProjectDialogOpen || _isGeneratingOutputDialogOpen || _isGeneratingExportDialogOpen || _isImportingRawDialogOpen || _isImportingBulkFilesDialogOpen;
            }
        }

        public bool IsLoadingProjectDialogOpen
        {

            get
            {
                return _isLoadingProjectDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isLoadingProjectDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsGeneratingOutputDialogOpen
        {

            get
            {
                return _isGeneratingOutputDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isGeneratingOutputDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsGeneratingExportDialogOpen
        {

            get
            {
                return _isGeneratingExportDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isGeneratingExportDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsImportingRawDialogOpen
        {

            get
            {
                return _isImportingRawDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isImportingRawDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsImportingBulkFilesDialogOpen
        {

            get
            {
                return _isImportingBulkFilesDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isImportingBulkFilesDialogOpen = value;
                NotifyDialogState();
            }
        }

        public void ResetDialogState()
        {
            _isLoadingProjectDialogOpen = false;
            _isGeneratingOutputDialogOpen = false;
            _isGeneratingExportDialogOpen = false;
            _isImportingRawDialogOpen = false;
            _isImportingBulkFilesDialogOpen = false;
        }

        public void NotifyDialogState()
        {
            NotifyOfPropertyChange(() => IsLoadingProjectDialogOpen);
            NotifyOfPropertyChange(() => IsGeneratingOutputDialogOpen);
            NotifyOfPropertyChange(() => IsGeneratingExportDialogOpen);
            NotifyOfPropertyChange(() => IsImportingRawDialogOpen);
            NotifyOfPropertyChange(() => IsImportingBulkFilesDialogOpen);
            NotifyOfPropertyChange(() => IsDialogOpen);
        }

        #endregion
    }
}
