using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Domain.PostProcessing;
using FanslationStudio.Domain.PreProcessing;
using FanslationStudio.Domain.ScriptToTranslate;
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
                NotifyOfPropertyChange(() => ProjectName);
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
        private ObservableCollection<ProjectVersion> _versions;
        private ObservableCollection<IScriptToTranslate> _scriptsToTranslate;
        private ObservableCollection<IPreProcessing> _preProcessingItems;
        private ObservableCollection<IPostProcessing> _postProcessingItems;

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
                return _scriptsToTranslate;
            }
            set
            {
                _scriptsToTranslate = value;
                NotifyOfPropertyChange(() => ScriptsToTranslate);
            }
        }

        public ObservableCollection<IPreProcessing> PreProcessingItems
        {
            get
            {
                return _preProcessingItems;
            }
            set
            {
                _preProcessingItems = value;
                NotifyOfPropertyChange(() => PreProcessingItems);
            }
        }

        public ObservableCollection<IPostProcessing> PostProcessingItems
        {
            get
            {
                return _postProcessingItems;
            }
            set
            {
                _postProcessingItems = value;
                NotifyOfPropertyChange(() => PostProcessingItems);
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
            PreProcessingItems = new ObservableCollection<IPreProcessing>(Project.PreProcessingItems);
            PostProcessingItems = new ObservableCollection<IPostProcessing>(Project.PostProcessingItems);
        }

        #region Dialog Management

        private bool _isScriptDialogOpen;
        private bool _isVersionDialogOpen;
        private bool _isPreProcessingDialogOpen;
        private bool _isPostProcessingDialogOpen;
        private bool _isBulkImportFileDialogOpen;
        private bool _isMergVersionDialogOpen;

        public bool IsDialogOpen
        {

            get
            {
                return _isVersionDialogOpen || _isScriptDialogOpen || _isPostProcessingDialogOpen || _isPreProcessingDialogOpen || _isBulkImportFileDialogOpen || _isMergVersionDialogOpen;
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
                ResetDialogState();
                _isVersionDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsMergeVersionDialogOpen
        {

            get
            {
                return _isMergVersionDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isMergVersionDialogOpen = value;
                NotifyDialogState();
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
                ResetDialogState();
                _isScriptDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsPreProcessingDialogOpen
        {

            get
            {
                return _isPreProcessingDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isPreProcessingDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsPostProcessingDialogOpen
        {

            get
            {
                return _isPostProcessingDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isPostProcessingDialogOpen = value;
                NotifyDialogState();
            }
        }

        public bool IsBulkImportFileDialogOpen
        {

            get
            {
                return _isBulkImportFileDialogOpen;
            }
            set
            {
                ResetDialogState();
                _isBulkImportFileDialogOpen = value;
                NotifyDialogState();
            }
        }

        public void ResetDialogState()
        {
            _isVersionDialogOpen = false;
            _isScriptDialogOpen = false;
            _isPostProcessingDialogOpen = false;
            _isPreProcessingDialogOpen = false;
            _isBulkImportFileDialogOpen = false;
            _isMergVersionDialogOpen = false;
        }

        public void NotifyDialogState()
        {
            NotifyOfPropertyChange(() => IsVersionDialogOpen);
            NotifyOfPropertyChange(() => IsMergeVersionDialogOpen);
            NotifyOfPropertyChange(() => IsScriptDialogOpen);
            NotifyOfPropertyChange(() => IsPreProcessingDialogOpen);
            NotifyOfPropertyChange(() => IsPostProcessingDialogOpen);
            NotifyOfPropertyChange(() => IsBulkImportFileDialogOpen);
            NotifyOfPropertyChange(() => IsDialogOpen);
        }

        #endregion

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

        public void OutputVersion(ProjectVersion version)
        {
            _eventAggregator.PublishOnUIThreadAsync(new Events.GenerateOutputFilesEvent()
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
                SourceLanguageCode = lastVersion?.SourceLanguageCode,
                TargetLanguageCode = lastVersion?.TargetLanguageCode,
            };

            SelectedVersion = version;
            VersionName = version.Version;
            SourceLanguageCode = version.SourceLanguageCode;
            TargetLanguageCode = version.TargetLanguageCode;
            RawInputFolder = version.RawInputFolder;
            IsNewVersion = true;

            IsVersionDialogOpen = true;
        }

        public void MergeVersion()
        {
            IsMergeVersionDialogOpen = true;
        }  

        public void EditVersion(ProjectVersion version)
        {
            SelectedVersion = version;
            VersionName = version.Version;
            SourceLanguageCode = version.SourceLanguageCode;
            TargetLanguageCode = version.TargetLanguageCode;
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

        public void ImportBulkFiles(ProjectVersion version)
        {
            BulkImportFolder = _project.LastBulkImportFolder;
            IsBulkImportFileDialogOpen = true;
        }

        #endregion

        #region Version Dialog

        private string _versionName;
        private string _sourceLanguageCode;
        private string _targetLanguageCode;
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

        public string SourceLanguageCode
        {
            get
            {
                return _sourceLanguageCode;
            }

            set
            {
                _sourceLanguageCode = value;
                NotifyOfPropertyChange(() => SourceLanguageCode);
            }
        }

        public string TargetLanguageCode
        {
            get
            {
                return _targetLanguageCode;
            }

            set
            {
                _targetLanguageCode = value;
                NotifyOfPropertyChange(() => TargetLanguageCode);
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
                SelectedVersion.SourceLanguageCode = SourceLanguageCode;
                SelectedVersion.TargetLanguageCode = TargetLanguageCode;
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

        #region Merge Version Dialog

        private ProjectVersion _selectedVersionToMerge;

        public ProjectVersion SelectedVersionToMerge
        {
            get
            {
                return _selectedVersionToMerge;
            }
            set
            {
                _selectedVersionToMerge = value;
                NotifyOfPropertyChange(() => SelectedVersionToMerge);
            }
        }

        public void AcceptMergeVersion()
        {
            _eventAggregator.PublishOnUIThreadAsync(new Events.MergeVersionEvent()
            {
                Project = _project,
                TargetVersion = Version,
                VersionToMerge = SelectedVersionToMerge
            });

            IsMergeVersionDialogOpen = false;
        }

        public void CancelMergeVersion()
        {
            IsMergeVersionDialogOpen = false;
        }

        #endregion

        #region Bulk Import Dialog

        private string _bulkImportFolder;

        public string BulkImportFolder
        {
            get
            {
                return _bulkImportFolder;
            }
            set
            {
                _bulkImportFolder = value;
                NotifyOfPropertyChange(() => BulkImportFolder);
            }
        }

        public void BrowseBulkImportFolder()
        {
            var openFileDialog = new CommonOpenFileDialog()
            {
                InitialDirectory = BulkImportFolder,
                IsFolderPicker = true,
            };

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                BulkImportFolder = openFileDialog.FileName;
            }
        }

        public void AcceptBulkImport()
        {
            _project.LastBulkImportFolder = BulkImportFolder;
            _project.WriteProjectFile();

            _eventAggregator.PublishOnUIThreadAsync(new Events.ImportExportFilesEvent()
            {
                ImportFolder = BulkImportFolder,
            });

            IsBulkImportFileDialogOpen = false;
        }

        public void CancelBulkImport()
        {
            IsBulkImportFileDialogOpen = false;
        }

        #endregion

        #region Pre & Post Processing

        private IPreProcessing _selectedPreProcessing;
        private IPostProcessing _selectedPostProcessing;

        public IPreProcessing SelectedPreProcessing
        {
            get
            {
                return _selectedPreProcessing;
            }
            set
            {
                _selectedPreProcessing = value;
                NotifyOfPropertyChange(() => SelectedPreProcessing);
            }
        }

        public IPostProcessing SelectedPostProcessing
        {
            get
            {
                return _selectedPostProcessing;
            }
            set
            {
                _selectedPostProcessing = value;
                NotifyOfPropertyChange(() => SelectedPostProcessing);
            }
        }

        public void AddPreProcessing()
        {
            //Default to Last Scripts stuff
            var lastItem = Project.PreProcessingItems.LastOrDefault(s => s is PreFindReplace) as PreFindReplace;
            var item = new PreFindReplace()
            {
                Find = lastItem?.Find,
                Replacement = lastItem?.Replacement,
                CaseSensitive = lastItem?.CaseSensitive ?? false,
                Comment = lastItem?.Comment,
                Enabled = true,
            };
            
            SelectedPreProcessing = item;            
            ProcessingFind = item.Find;
            ProcessingReplacement = item.Replacement;
            ProcessingCaseSensitive = item.CaseSensitive;
            ProcessingComment = item.Comment;
            ProcessingEnabled = item.Enabled;

            _isNewProcessing = true;
            IsPreProcessingDialogOpen = true;
        }

        public void EditPreProcessing(IPreProcessing sourceItem)
        {
            var item = sourceItem as PreFindReplace;
            if (item != null)
            {
                SelectedPreProcessing = item;
                ProcessingFind = item.Find;
                ProcessingReplacement = item.Replacement;
                ProcessingCaseSensitive = item.CaseSensitive;
                ProcessingComment = item.Comment;
                ProcessingEnabled = item.Enabled;

                _isNewProcessing = false;
                IsPreProcessingDialogOpen = true;
            }
        }

        public void RemovePreProcessing(IPreProcessing item)
        {
            PreProcessingItems.Remove(item);
            Project.PreProcessingItems.Remove(item);
            Project.WriteProjectFile();
        }

        public void MoveUpPreProcessing(IPreProcessing item)
        {
            int index = PreProcessingItems.IndexOf(item);

            if (index > 0) //Not First Index
                PreProcessingItems.Move(index, index - 1);
        }

        public void MoveDownPreProcessing(IPreProcessing item)
        {
            int index = PreProcessingItems.IndexOf(item);

            if (index < PreProcessingItems.Count - 1) //Not Last Index
                PreProcessingItems.Move(index, index + 1);
        }

        public void AddPostProcessing()
        {
            //Default to Last Scripts stuff
            var lastItem = Project.PostProcessingItems.LastOrDefault(s => s is PostFindReplace) as PostFindReplace;
            var item = new PostFindReplace()
            {
                Find = lastItem?.Find,
                Replacement = lastItem?.Replacement,
                CaseSensitive = lastItem?.CaseSensitive ?? false,
                Comment = lastItem?.Comment,
                Enabled = true,
            };

            SelectedPostProcessing = item;
            ProcessingFind = item.Find;
            ProcessingReplacement = item.Replacement;
            ProcessingCaseSensitive = item.CaseSensitive;
            ProcessingComment = item.Comment;
            ProcessingEnabled = item.Enabled;

            _isNewProcessing = true;
            IsPostProcessingDialogOpen = true;
        }

        public void EditPostProcessing(IPostProcessing sourceItem)
        {
            var item = sourceItem as PostFindReplace;
            if (item != null)
            {
                SelectedPostProcessing = item;
                ProcessingFind = item.Find;
                ProcessingReplacement = item.Replacement;
                ProcessingCaseSensitive = item.CaseSensitive;
                ProcessingComment = item.Comment;
                ProcessingEnabled = item.Enabled;

                _isNewProcessing = false;
                IsPostProcessingDialogOpen = true;
            }
        }

        public void RemovePostProcessing(IPostProcessing item)
        {
            PostProcessingItems.Remove(item);
            Project.PostProcessingItems.Remove(item);
            Project.WriteProjectFile();
        }

        public void MoveUpPostProcessing(IPostProcessing item)
        {
            int index = PostProcessingItems.IndexOf(item);

            if (index > 0) //Not First Index
                PostProcessingItems.Move(index, index - 1);
        }

        public void MoveDownPostProcessing(IPostProcessing item)
        {
            int index = PostProcessingItems.IndexOf(item);

            if (index < PostProcessingItems.Count - 1) //Not Last Index
                PostProcessingItems.Move(index, index + 1);
        }

        #endregion

        #region Pre & Post Processing Dialog

        private string _processingFind;
        private string _processingReplacement;        
        private string _processingComment;
        private bool _processingCaseSensitive;
        private bool _processingEnabled;
        private bool _isNewProcessing;

        public string ProcessingFind
        {
            get
            {
                return _processingFind;
            }
            set
            {
                _processingFind = value;
                NotifyOfPropertyChange(() => ProcessingFind);
            }
        }

        public string ProcessingReplacement
        {
            get
            {
                return _processingReplacement;
            }
            set
            {
                _processingReplacement = value;
                NotifyOfPropertyChange(() => ProcessingReplacement);
            }
        }

        public string ProcessingComment
        {
            get
            {
                return _processingComment;
            }
            set
            {
                _processingComment = value;
                NotifyOfPropertyChange(() => ProcessingComment);
            }
        }

        public bool ProcessingCaseSensitive
        {
            get
            {
                return _processingCaseSensitive;
            }
            set
            {
                _processingCaseSensitive = value;
                NotifyOfPropertyChange(() => ProcessingCaseSensitive);
            }
        }

        public bool ProcessingEnabled
        {
            get
            {
                return _processingEnabled;
            }
            set
            {
                _processingEnabled = value;
                NotifyOfPropertyChange(() => ProcessingEnabled);
            }
        }

        public void AcceptPreFindReplace()
        {
            if (_isNewProcessing)
            {
                var item = new PreFindReplace()
                {
                    Find = ProcessingFind,
                    Replacement = ProcessingReplacement,
                    CaseSensitive = ProcessingCaseSensitive,
                    Comment = ProcessingComment,
                    Enabled = ProcessingEnabled,
                };

                PreProcessingItems.Add(item);
                Project.PreProcessingItems.Add(item);
            }
            else
            {
                var item = SelectedPreProcessing as PreFindReplace;
                if (item != null)
                {
                    item.Find = ProcessingFind;
                    item.Replacement = ProcessingReplacement;
                    item.CaseSensitive = ProcessingCaseSensitive;
                    item.Comment = ProcessingComment;
                    item.Enabled = ProcessingEnabled;
                }
            }

            Project.WriteProjectFile();
            IsPreProcessingDialogOpen = false;
        }

        public void CancelPreFindReplace()
        {
            IsPreProcessingDialogOpen = false;
        }

        public void AcceptPostFindReplace()
        {
            if (_isNewProcessing)
            {
                var item = new PostFindReplace()
                {
                    Find = ProcessingFind,
                    Replacement = ProcessingReplacement,
                    CaseSensitive = ProcessingCaseSensitive,
                    Comment = ProcessingComment,
                    Enabled = ProcessingEnabled,
                };

                PostProcessingItems.Add(item);
                Project.PostProcessingItems.Add(item);
            }
            else
            {
                var item = SelectedPostProcessing as PostFindReplace;
                if (item != null)
                {
                    item.Find = ProcessingFind;
                    item.Replacement = ProcessingReplacement;
                    item.CaseSensitive = ProcessingCaseSensitive;
                    item.Comment = ProcessingComment;
                    item.Enabled = ProcessingEnabled;
                }
            }

            Project.WriteProjectFile();
            IsPostProcessingDialogOpen = false;
        }

        public void CancelPostFindReplace()
        {
            IsPostProcessingDialogOpen = false;
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
