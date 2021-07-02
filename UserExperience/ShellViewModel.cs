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

    public class ShellViewModel : Conductor<object>, IHandle<SelectProjectEvent>, IHandle<SelectProjectVersionEvent>, IHandle<ImportProjectVersionEvent>
    {
        private IEventAggregator _eventAggregator;
        private Config _config;
        private Project _currentProject;
        private ProjectVersion _currentVersion;
        private string _title = "Fanslation Studio - Select a Project";
        private Dictionary<string, List<ScriptTranslation>> _scripts;
        private bool _isDialogOpen;
        private TabItem _selectedTabItem;

        public bool IsDialogOpen
        {
            get
            {
                return _isDialogOpen;
            }
            set
            {
                _isDialogOpen = value;
                NotifyOfPropertyChange(() => IsDialogOpen);
            }
        }

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
            IsDialogOpen = true;

            Task.Run(() =>
            {
                string projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _currentProject.Name);
                string folder = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, _currentVersion);
                _scripts = ScriptTranslationService.LoadTranslationsThatExist(_currentProject, folder);

                Title = $"FanslationStudio :: {_currentProject.Name} - Version: {_currentVersion.Version}";

                Thread.Sleep(500); //Avoid flicker
                IsDialogOpen = false;
            });            
        }

        public void ImportVersion(ProjectVersion version)
        {            
            IsDialogOpen = true;

            Task.Run(() =>
            {
                version.CreateWorkspaceFolders(_config, _currentProject);
                version.CopyRawsIntoWorkspaceFolder();
                version.ImportRawLinesAsTranslations(_currentProject);

                Thread.Sleep(500); //Avoid flicker
                IsDialogOpen = false;
            });            
        }

        //Method to handle navigation because we cant get caliburn to pipe through
        public void TabSelectionChanged(object args)
        {
            if (args is TabControl)
            {
                _selectedTabItem = ((args as TabControl).SelectedItem as TabItem);
                string name = _selectedTabItem.Name;

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
                    case "ImportBulk":
                        ShowImportBulk();
                        break;
                    case "GenerateOutput":
                        ShowGenerateOutput();
                        break;
                }
            }
        }

        public async void ShowHome()
        {
            var vm = IoC.Get<HomeViewModel>();
            vm.Config = _config;

            await ActivateItemAsync(vm);
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
    }
}
