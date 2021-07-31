using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FanslationStudio.UserExperience
{
    public class ManualTranslateViewModel : Screen, IHasProjectAndVersionViewModel, IHandle<Events.DeeplTransEvent>
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

        #region Panels

        private bool _showMtlPanel;
        private bool _showSearchPanel;
        public bool ShowMtlPanel
        {
            get
            {
                return _showMtlPanel;
            }
            set
            {
                _showMtlPanel = value;
                NotifyOfPropertyChange(() => ShowMtlPanel);
            }
        }
        public bool ShowSearchPanel
        {
            get
            {
                return _showSearchPanel;
            }
            set
            {
                _showSearchPanel = value;
                NotifyOfPropertyChange(() => ShowSearchPanel);
            }
        }

        #endregion

        private IEventAggregator _eventAggregator;
        private ObservableCollection<ScriptSearchResult> _searchResults;
        private ScriptSearchResult _selectedItem;
        private ObservableCollection<SearchPattern> _searchPatterns;
        private string _projectFolder;
        private string _translationVersionFolder;
        private string _scratchZone;
        private string _quickFindTerm;
        private string _quickReplaceTerm;
        private string _lastSentLine;
        private bool _searchUntranslated;
        private bool _searchMismatchTranslation;
        private bool _searchMerge;
        private bool _isSearchingRaw;

        public ObservableCollection<ScriptSearchResult> SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                _searchResults = value;
                NotifyOfPropertyChange(() => SearchResults);
            }
        }
        public ScriptSearchResult SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
                NotifyOfPropertyChange(() => SelectedRaw);
                NotifyOfPropertyChange(() => SelectedCurrentTranslation);
            }
        }
        public string QuickFindTerm
        {
            get
            {
                return _quickFindTerm;
            }
            set
            {
                _quickFindTerm = value;
                NotifyOfPropertyChange(() => QuickFindTerm);
            }
        }
        public string QuickReplaceTerm
        {
            get
            {
                return _quickReplaceTerm;
            }
            set
            {
                _quickReplaceTerm = value;
                NotifyOfPropertyChange(() => QuickReplaceTerm);
            }
        }
        public string SelectedRaw
        {
            get
            {
                return _selectedItem?.Item.Raw;
            }
        }
        public string SelectedCurrentTranslation
        {
            get
            {
                return _selectedItem?.Item.ResultingTranslation;
            }
        }
        public string ScratchZone
        {
            get
            {
                return _scratchZone;
            }
            set
            {
                _scratchZone = value;
                NotifyOfPropertyChange(() => ScratchZone);
            }
        }
        public ObservableCollection<SearchPattern> SearchPatterns
        {
            get
            {
                return _searchPatterns;
            }
            set
            {
                _searchPatterns = value;
                NotifyOfPropertyChange(() => SearchPatterns);
            }
        }
        public bool SearchUntranslated
        {
            get
            {
                return _searchUntranslated;
            }
            set
            {
                _searchUntranslated = value;
                NotifyOfPropertyChange(() => SearchUntranslated);
            }
        }
        public bool SearchMismatchTranslation
        {
            get
            {
                return _searchMismatchTranslation;
            }
            set
            {
                _searchMismatchTranslation = value;
                NotifyOfPropertyChange(() => SearchMismatchTranslation);
            }
        }
        public bool SearchMerge
        {
            get
            {
                return _searchMerge;
            }
            set
            {
                _searchMerge = value;
                NotifyOfPropertyChange(() => SearchMerge);
            }
        }
        public bool IsSearchingRaw
        {
            get
            {
                return _isSearchingRaw;
            }
            set
            {
                _isSearchingRaw = value;
                NotifyOfPropertyChange(() => IsSearchingRaw);
                NotifyOfPropertyChange(() => IsSearchingCaption);
            }
        }
        public string IsSearchingCaption
        {
            get { return _isSearchingRaw ? "Search Raws" : "Search Ids";  }
        }

        public ManualTranslateViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            _isSearchingRaw = true;
            Activated += OnActivate;
        }

        private void OnActivate(object sender, ActivationEventArgs e)
        {
            _projectFolder = Services.ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _project.Name);
            _translationVersionFolder = Services.ProjectFolderService.CalculateTranslationVersionFolder(_projectFolder, _version);

            SearchPatterns = new ObservableCollection<SearchPattern>(_project.SearchPatterns);
            ShowMtlPanel = true;
        }

        public async void SelectSearchResult(ScriptSearchResult item)
        {
            SelectedItem = item;
            QuickFindTerm = item?.Find;
            QuickReplaceTerm = item?.Replace;
            ScratchZone = string.Empty;

            string cleanedUpLine = item?.Item.CleanedUpLine
                .Replace("<<", "<").Replace(">>", ">"); //Temp clean up

            if (item != null && _lastSentLine != cleanedUpLine)
            {
                _lastSentLine = cleanedUpLine;
                await _eventAggregator.PublishOnUIThreadAsync(new Events.RawLineCopiedEvent(cleanedUpLine, Version.SourceLanguageCode, Version.TargetLanguageCode));
            }
        }

        public async void CopyFromDeepL()
        {
            await _eventAggregator.PublishOnUIThreadAsync(new Events.RequestDeeplResult());
        }

        public void ShowSearchFiles()
        {
            ShowSearchPanel = true;
        }

        public void CopyRawToScratch()
        {
            ScratchZone = SelectedRaw;
        }

        public void CopyCurrentToScratch()
        {
            ScratchZone = SelectedCurrentTranslation;
        }

        public void WriteScratchZone()
        {
            SelectedItem.Item.ManualTranslation = ScratchZone;
            string folder = $"{_translationVersionFolder}\\{SelectedItem.SourcePath}";
            ScriptTranslationService.WriteIndividualScriptFile(folder, 
                SelectedItem.Script, true);

            NotifyOfPropertyChange(() => SelectedCurrentTranslation);

            _eventAggregator.PublishOnUIThreadAsync(new Events.MoveToNextGridItemEvent());

            
        }

        public void CreateSearchPattern()
        {
            string find = SelectedCurrentTranslation;
            if (!SearchPatterns.Any(s => s.Find == find))
            {
                //Do this after deepl
                SearchPatterns.Add(new SearchPattern()
                {
                    Find = SelectedCurrentTranslation,
                    Replacement = ScratchZone,
                });
            }
        }

        public void QuickReplace()
        {
            ScratchZone = ScratchZone.Replace(QuickFindTerm, QuickReplaceTerm);
        }

        public void AddSearchPattern()
        {
            var lastPattern = SearchPatterns.LastOrDefault();

            if (!string.IsNullOrEmpty(lastPattern?.Find))
                SearchPatterns.Add(new SearchPattern());
        }

        public void RemoveSearchPattern(SearchPattern toRemove)
        {
            SearchPatterns.Remove(toRemove);
            WriteSearchPatterns();
        }

        public void WriteSearchPatterns()
        {
            _project.SearchPatterns = SearchPatterns
                .Where(p => p.Find != string.Empty)
                .ToList();

            _project.WriteProjectFile();
        }

        public void SortSearchPattern()
        {
            var patterns = SearchPatterns.OrderBy(s => s.Replacement).ToArray();
            SearchPatterns.Clear();

            foreach (var pattern in patterns)
            {
                SearchPatterns.Add(pattern);
            }

            WriteSearchPatterns();
        }

        public void QuickSearch()
        {
            var foundResults = SearchScriptService.QuickSearch(_scripts, IsSearchingRaw, SearchMerge, SearchUntranslated, SearchMismatchTranslation, QuickFindTerm);
            SearchResults = new ObservableCollection<ScriptSearchResult>(foundResults);
            ShowSearchPanel = false;                     
        }

        public void SearchWithPatterns()
        {
            var foundResults = SearchScriptService.SearchWithPatterns(_scripts, SearchPatterns.ToList()); 
            SearchResults = new ObservableCollection<ScriptSearchResult>(foundResults);
            WriteSearchPatterns();
            ShowSearchPanel = false;
        }

        public Task HandleAsync(Events.DeeplTransEvent message, CancellationToken cancellationToken)
        {
            ScratchZone = message.TransLine;

            return Task.CompletedTask;
        }
        
        public void ExportBatchFiles()
        {
            //We do this on the shell because we dont want people moving around while we're exporting
            _eventAggregator.PublishOnUIThreadAsync(new Events.GenerateExportFilesEvent()
            {
                SearchResults = SearchResults.ToList(),
            });
        }
    }
}
