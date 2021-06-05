using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FanslationStudio.UserExperience
{
    public class ImportBulkViewModel : Screen, IHasProjectAndVersionViewModel
    {
        #region IHasProjectAndVersionViewModel

        private Config _config;
        private Project _project;
        private ProjectVersion _version;

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
        public Project Project
        {
            get
            {
                return _project;
            }
            set
            {
                _project = value;
                NotifyOfPropertyChange(() => Project);
            }
        }
        public ProjectVersion Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                NotifyOfPropertyChange(() => Version);
            }
        }

        #endregion

        private Dictionary<string, List<ScriptTranslation>> _scripts;
        public Dictionary<string, List<ScriptTranslation>> Scripts
        {
            get
            {
                return _scripts;
            }
            set
            {
                _scripts = value;
            }
        }

        private bool _isImporting { get; set; }
        private bool _isImported { get; set; }
        private string _importFolder { get; set; }

        public bool IsImporting
        {
            get
            {
                return _isImporting;
            }
            set
            {
                _isImporting = value;
                NotifyOfPropertyChange(() => IsImporting);
            }
        }

        public bool IsImported
        {
            get
            {
                return _isImported;
            }
            set
            {
                _isImported = value;
                NotifyOfPropertyChange(() => IsImported);
            }
        }

        public string ImportFolder
        {
            get
            {
                return _importFolder;
            }
            set
            {
                _importFolder = value;
                NotifyOfPropertyChange(() => ImportFolder);
            }
        }

        public ImportBulkViewModel()
        {
            Activated += OnActivate;
        }

        private void OnActivate(object sender, ActivationEventArgs e)
        {
            IsImporting = false;
            IsImported = false;
            ImportFolder = _config.LastImportFolder;
        }

        public void BrowseFile()
        {
            var openFileDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportFolder = openFileDialog.SelectedPath;
                _config.LastImportFolder = ImportFolder;
                _config.WriteConfig();
            }
        }

        public void ImportFiles()
        {
            string projectFolder = ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _project.Name);
            string translationFolderVersion = ProjectFolderService.CalculateTranslationVersionFolder(projectFolder, _version);

            IsImporting = true;
            IsImported = false;

            Task.Run(() =>
            {
                ExportFilesService.ImportLines(_scripts, ImportFolder);

                foreach (var script in _scripts)
                {
                    string translatedFolder = $"{translationFolderVersion}\\{script.Key}";

                    //Go through each script and add it if its missing
                    ScriptTranslationService.WriteBulkScriptFiles(translatedFolder, script.Value, true);
                }

                IsImported = true;
                IsImporting = false;
            });
        }
    }
}
