using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.ScriptToTranslate;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FanslationStudio.UserExperience
{
    public class ProjectViewModel : Screen, IHasProjectAndVersionViewModel
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
                NotifyOfPropertyChange(() => ProjectFile);
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

        private IEventAggregator _eventAggregator;
        private bool _isScriptDialogOpen;
        private bool _isVersionDialogOpen;
        private ObservableCollection<ProjectVersion> _versions;
        private ObservableCollection<IScriptToTranslate> scriptsToTranslate;

        public bool IsDialogOpen
        {

            get
            {
                return _isVersionDialogOpen || _isScriptDialogOpen;
            }
        }

        public bool IsVersionDialogOpen
        {

            get
            {
                return _isVersionDialogOpen;
            }
            set
            {
                _isVersionDialogOpen = value;
                _isScriptDialogOpen = false;
                NotifyOfPropertyChange(() => IsVersionDialogOpen);
                NotifyOfPropertyChange(() => IsScriptDialogOpen);
                NotifyOfPropertyChange(() => IsDialogOpen);
            }
        }

        public bool IsScriptDialogOpen
        {

            get
            {
                return _isScriptDialogOpen;
            }
            set
            {
                _isScriptDialogOpen = value;
                _isVersionDialogOpen = false;
                NotifyOfPropertyChange(() => IsVersionDialogOpen);
                NotifyOfPropertyChange(() => IsScriptDialogOpen);
                NotifyOfPropertyChange(() => IsDialogOpen);
            }
        }

        public string ProjectName
        {
            get
            {
                return _project.Name;
            }
            set
            {
                _project.Name = value;
                NotifyOfPropertyChange(() => ProjectName);
            }
        }

        public string ProjectFile
        {
            get
            {
                return _project.ProjectFile;
            }
        }

        public ObservableCollection<ProjectVersion> Versions
        {
            get
            {
                return _versions;
            }
            set
            {
                _versions = value;
                NotifyOfPropertyChange(() => Versions);
            }
        }

        public ObservableCollection<IScriptToTranslate> ScriptsToTranslate
        {
            get
            {
                return scriptsToTranslate;
            }
            set
            {
                scriptsToTranslate = value;
                NotifyOfPropertyChange(() => ScriptsToTranslate);
            }
        }

        public ProjectViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            Activated += OnActivate;
        }

        private void OnActivate(object sender, ActivationEventArgs e)
        {
            Versions = new ObservableCollection<ProjectVersion>(Project.Versions);
            ScriptsToTranslate = new ObservableCollection<IScriptToTranslate>(Project.ScriptsToTranslate);
        }

        #region Version

        private ProjectVersion _selectedVersion;

        public ProjectVersion SelectedVersion
        {
            get
            {
                return _selectedVersion;
            }
            set
            {
                _selectedVersion = value;
                NotifyOfPropertyChange(() => SelectedVersion);
            }
        }

        public void LoadVersion(ProjectVersion version)
        {
            _eventAggregator.PublishOnUIThreadAsync(new Events.SelectProjectVersionEvent()
            {
                SelectedVersion = version
            });

            _version = version;
        }

        public void ImportVersion(ProjectVersion version)
        {
            _eventAggregator.PublishOnUIThreadAsync(new Events.ImportProjectVersionEvent()
            {
                SelectedVersion = version
            });
        }

        public void AddVersion()
        {
            //Default to Last Versions stuff
            var lastVersion = Project.Versions.LastOrDefault();
            var version = new ProjectVersion()
            {
                Version = lastVersion?.Version,
                RawInputFolder = lastVersion?.RawInputFolder,
            };

            SelectedVersion = version;
            VersionName = version.Version;
            RawInputFolder = version.RawInputFolder;
            IsNewVersion = true;

            IsVersionDialogOpen = true;
        }

        public void EditVersion(ProjectVersion version)
        {
            SelectedVersion = version;
            VersionName = version.Version;
            RawInputFolder = version.RawInputFolder;
            IsNewVersion = false;

            IsVersionDialogOpen = true;
        }

        public void DeleteVersion(ProjectVersion version)
        {
            if (version != _version)
            {
                Versions.Remove(version);
                Project.Versions.Remove(version);
                Project.WriteProjectFile();
            }
        }

        #endregion

        #region Version Dialog

        private string _versionName;
        private string _rawInputFolder;
        private bool _isNewVersion;

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

        public bool IsNewVersion
        {
            get
            {
                return _isNewVersion;
            }

            set
            {
                _isNewVersion = value;
                NotifyOfPropertyChange(() => IsNewVersion);
            }
        }

        public void AcceptVersion()
        {
            if (_isNewVersion)
            {
                var version = new ProjectVersion()
                {
                    Version = VersionName,
                    RawInputFolder = RawInputFolder,
                };

                Versions.Add(version);
                Project.Versions.Add(version);
            }
            else
            {
                SelectedVersion.RawInputFolder = RawInputFolder;
            }

            Project.WriteProjectFile();
            IsVersionDialogOpen = false;
        }

        public void CancelVersion()
        {
            IsVersionDialogOpen = false;
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

        #endregion

        #region Scripts To Translate

        private IScriptToTranslate _selectedScriptToTranslate;

        public IScriptToTranslate SelectedScriptToTranslate
        {
            get
            {
                return _selectedScriptToTranslate;
            }
            set
            {
                _selectedScriptToTranslate = value;
                NotifyOfPropertyChange(() => SelectedScriptToTranslate);
            }
        }

        public void AddScript()
        {
            //Default to Last Scripts stuff
            var lastScript = Project.ScriptsToTranslate.LastOrDefault(s => s is SplitTextFileToTranslate) as SplitTextFileToTranslate;
            var script = new SplitTextFileToTranslate()
            {
                SourcePath = lastScript?.SourcePath,
                SplitCharacters = lastScript?.SplitCharacters,
                LineIdIndex = lastScript?.LineIdIndex ?? 0,
                SplitIndexes = new List<int>() { 1 },
                UseIndexForLineId = lastScript?.UseIndexForLineId ?? true,
                OverrideFontSize = 0,
            };

            SelectedScriptToTranslate = script;
            IsNewScript = true;
            SourcePath = script.SourcePath;
            SplitCharacters = script.SplitCharacters;
            LineIdIndex = script.LineIdIndex;
            SplitIndexes = new ObservableCollection<SplitIndexValue>(script.SplitIndexes.Select(s => new SplitIndexValue() { Value = s }));
            UseIndexForLineId = script.UseIndexForLineId;
            OverrideFontSize = script.OverrideFontSize;

            IsScriptDialogOpen = true;
        }

        public void EditScript(IScriptToTranslate scriptToTranslate)
        {
            var script = scriptToTranslate as SplitTextFileToTranslate;
            if (script != null)
            {
                SelectedScriptToTranslate = script;
                IsNewScript = false;
                SourcePath = script.SourcePath;
                SplitCharacters = script.SplitCharacters;
                LineIdIndex = script.LineIdIndex;
                SplitIndexes = new ObservableCollection<SplitIndexValue>(script.SplitIndexes.Select(s => new SplitIndexValue() { Value = s }));
                UseIndexForLineId = script.UseIndexForLineId;
                OverrideFontSize = script.OverrideFontSize;

                IsScriptDialogOpen = true;
            }
        }

        public void DeleteScript(IScriptToTranslate script)
        {
            ScriptsToTranslate.Remove(script);
            Project.ScriptsToTranslate.Remove(script);
            Project.WriteProjectFile();
        }

        #endregion

        #region Scripts To Translate Dialogs

        //TODO: At some point support differrent file format dialogs - probably easy with a down popout menu and each action goes to seperate dialog

        private bool _isNewScript;
        private string _sourcePath;
        private string _splitCharacters;
        private ObservableCollection<SplitIndexValue> _splitIndexes;
        private bool _useIndexForLineId;
        private int _lineIdIndex;
        private int _overrideFontSize;

        public class SplitIndexValue
        {
            public int Value { get; set; }
        }

        public bool IsNewScript
        {
            get
            {
                return _isNewScript;
            }
            set
            {
                _isNewScript = value;
                NotifyOfPropertyChange(() => IsNewScript);
            }
        }

        public string SourcePath
        {
            get
            {
                return _sourcePath;
            }
            set
            {
                _sourcePath = value;
                NotifyOfPropertyChange(() => SourcePath);
            }
        }

        public string SplitCharacters
        {
            get
            {
                return _splitCharacters;
            }
            set
            {
                _splitCharacters = value;
                NotifyOfPropertyChange(() => SplitCharacters);
            }
        }

        public ObservableCollection<SplitIndexValue> SplitIndexes
        {
            get
            {
                return _splitIndexes;
            }
            set
            {
                _splitIndexes = value;
                NotifyOfPropertyChange(() => SplitIndexes);
            }
        }

        public bool UseIndexForLineId
        {
            get
            {
                return _useIndexForLineId;
            }
            set
            {
                _useIndexForLineId = value;
                NotifyOfPropertyChange(() => UseIndexForLineId);
            }
        }

        public int LineIdIndex
        {
            get
            {
                return _lineIdIndex;
            }
            set
            {
                _lineIdIndex = value;
                NotifyOfPropertyChange(() => LineIdIndex);
            }
        }

        public int OverrideFontSize
        {
            get
            {
                return _overrideFontSize;
            }
            set
            {
                _overrideFontSize = value;
                NotifyOfPropertyChange(() => OverrideFontSize);
            }
        }

        public void BrowseSourcePath()
        {
            string initialFolder = $"{Version.RawInputFolder}\\{SourcePath}";
           
            var openFileDialog = new CommonOpenFileDialog()
            {
                InitialDirectory = System.IO.Path.GetDirectoryName(initialFolder), //Should put us in the same folder as the last one
                IsFolderPicker = false,
            };

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SourcePath = openFileDialog.FileName.Replace(Version.RawInputFolder, ""); //Make it relative to Project raws
            }
        }

        public void AcceptScript()
        {
            if (_isNewScript)
            {
                var script = new SplitTextFileToTranslate()
                {
                    SourcePath = SourcePath,
                    SplitCharacters = SplitCharacters,
                    SplitIndexes = SplitIndexes.Select(s => s.Value).ToList(),
                    UseIndexForLineId = UseIndexForLineId,
                    LineIdIndex = LineIdIndex,
                    OverrideFontSize = OverrideFontSize,
                };

                ScriptsToTranslate.Add(script);
                Project.ScriptsToTranslate.Add(script);                
            }
            else
            {
                var script = SelectedScriptToTranslate as SplitTextFileToTranslate;
                if (script != null)
                {
                    script.SourcePath = SourcePath;
                    script.SplitCharacters = SplitCharacters;
                    script.SplitIndexes = SplitIndexes.Select(s => s.Value).ToList();
                    script.UseIndexForLineId = UseIndexForLineId;
                    script.LineIdIndex = LineIdIndex;
                    script.OverrideFontSize = OverrideFontSize;
                }
            }

            Project.WriteProjectFile();
            IsScriptDialogOpen = false;
        }

        public void CancelScript()
        {
            IsScriptDialogOpen = false;
        }

        public void AddSplitIndex()
        {
            int newIndex = SplitIndexes.Count > 0 ? SplitIndexes.Max(s => s.Value) + 1 : 0;
            SplitIndexes.Add(new SplitIndexValue() { Value = newIndex });
        }

        public void DeleteSplitIndex(SplitIndexValue index)
        {
            if (SplitIndexes.Count > 1)
                SplitIndexes.Remove(index);
        }

        #endregion

    }
}
