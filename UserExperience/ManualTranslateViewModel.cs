using Caliburn.Micro;
using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FanslationStudio.UserExperience
{
    public class ManualTranslateViewModel : Screen, IHasProjectAndVersionViewModel
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

        private List<ScriptTranslationItem> _searchResults;
        private ScriptTranslationItem _selectedItem;
        private ObservableCollection<SearchPattern> _searchPatterns;
        private string _scratchZone;
        private string _quickFindTerm;
        private string _quickReplaceTerm;
        private bool _searchUntranslated;
        private bool _searchMerge;

        public List<ScriptTranslationItem> SearchResults
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
        public ScriptTranslationItem SelectedItem
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
                return _selectedItem?.Raw;
            }
        }
        public string SelectedCurrentTranslation
        {
            get
            {
                return _selectedItem?.ResultingTranslation;
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

        public ManualTranslateViewModel()
        {
            this.Activated += OnActivate;

        }

        private void OnActivate(object sender, ActivationEventArgs e)
        {
            string _projectFolder = Services.ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _project.Name);
            string _translationFolderVersion = Services.ProjectFolderService.CalculateTranslationVersionFolder(_projectFolder, _version);

            var folder = $"{_translationFolderVersion}\\{_project.ScriptsToTranslate[0].SourcePath}";
            var lines = Services.ScriptTranslationService.LoadBulkScriptTranslations(folder);
            SearchResults = lines.SelectMany(s => s.Items).ToList();

            SearchPatterns = new ObservableCollection<SearchPattern>(_config.SearchPatterns);
        }

        public void SelectSearchResult(ScriptTranslationItem item)
        {
            SelectedItem = item;
            ScratchZone = string.Empty;

            //Todo: Perform translation
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
            throw new NotImplementedException();
        }

        public void QuickReplace()
        {
            ScratchZone = ScratchZone.Replace(QuickFindTerm, QuickReplaceTerm);
        }

        public void AddSearchPattern()
        {
            var lastPattern = SearchPatterns.Last();

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
            _config.SearchPatterns = SearchPatterns
                .Where(p => p.Find != string.Empty)
                .ToList();

            _config.WriteConfig();
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
            ShowSearchPanel = false;
        }

        public void SearchWithPatterns()
        {
            Console.WriteLine("Searching");

            WriteSearchPatterns();
            ShowSearchPanel = false;
        }
    }
}
