using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool _searchUntranslated;
        private bool _searchMerge;

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

        public ManualTranslateViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
            
            Activated += OnActivate;
        }

        private void OnActivate(object sender, ActivationEventArgs e)
        {
            _projectFolder = Services.ProjectFolderService.CalculateProjectFolder(_config.WorkshopFolder, _project.Name);
            _translationVersionFolder = Services.ProjectFolderService.CalculateTranslationVersionFolder(_projectFolder, _version);

            SearchPatterns = new ObservableCollection<SearchPattern>(_config.SearchPatterns);
        }

        public async void SelectSearchResult(ScriptSearchResult item)
        {
            SelectedItem = item;
            QuickFindTerm = item?.Find;
            QuickReplaceTerm = item?.Replace;
            ScratchZone = string.Empty;

            await _eventAggregator.PublishOnUIThreadAsync(new Events.RawLineCopiedEvent(item?.Item.Raw));
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
            var foundResults = new ConcurrentBag<ScriptSearchResult>();

            Parallel.ForEach(_scripts, scriptEntry =>
            {
                foreach (var script in scriptEntry.Value)
                {
                    foreach(var scriptItem in script.Items)
                    {
                        if (!scriptItem.RequiresTranslation)
                            continue;

                        string resultingTranslation = scriptItem.ResultingTranslation;
                        if (resultingTranslation == scriptItem.Raw)
                            resultingTranslation = string.Empty;

                        //Add Merge Changes
                        if (SearchMerge && scriptItem.MergeHadRawChanges)
                            foundResults.Add(new ScriptSearchResult()
                            {
                                SourcePath = scriptEntry.Key,
                                Script = script,
                                Item = scriptItem,
                            });
                        //Add Untranslated
                        else if (SearchUntranslated && !string.IsNullOrEmpty(scriptItem.Raw) && string.IsNullOrEmpty(resultingTranslation))
                            foundResults.Add(new ScriptSearchResult()
                            {
                                SourcePath = scriptEntry.Key,
                                Script = script,
                                Item = scriptItem,
                            });
                        //Add found terms
                        else if (!string.IsNullOrEmpty(_quickFindTerm) && resultingTranslation.Contains(_quickFindTerm))
                            foundResults.Add(new ScriptSearchResult()
                            {
                                SourcePath = scriptEntry.Key,
                                Script = script,
                                Item = scriptItem,
                                Find = QuickFindTerm,
                            });
                    }
                }
            });

            SearchResults = new ObservableCollection<ScriptSearchResult>(foundResults);
            ShowSearchPanel = false;                     
        }

        public void SearchWithPatterns()
        {
            var foundResults = new ConcurrentBag<ScriptSearchResult>();

            Parallel.ForEach(_scripts, scriptEntry =>
            {
                foreach (var script in scriptEntry.Value)
                {
                    foreach (var scriptItem in script.Items)
                    {
                        if (!scriptItem.RequiresTranslation)
                            continue;

                        string resultingTranslation = scriptItem.ResultingTranslation;
                        if (resultingTranslation == scriptItem.Raw)
                            resultingTranslation = string.Empty;

                        foreach (var pattern in SearchPatterns)
                        {
                            if (FindPattern(resultingTranslation, pattern))
                                foundResults.Add(new ScriptSearchResult()
                                {
                                    SourcePath = scriptEntry.Key,
                                    Script = script,
                                    Item = scriptItem,
                                    Find = pattern.Find,
                                    Replace = pattern.Replacement,
                                });
                        }
                    }
                }
            });

            SearchResults = new ObservableCollection<ScriptSearchResult>(foundResults);

            WriteSearchPatterns();
            ShowSearchPanel = false;
        }

        private static bool FindPattern(string lineContent, SearchPattern pattern)
        {
            bool found;

            //Regex matching is quite slow so lets just index of stuff that isnt a regex
            if (pattern.IsRegex)
            {
                if (pattern.CaseSensitive)
                    found = Regex.Match(lineContent, pattern.Find).Success;
                else
                    found = Regex.Match(lineContent, pattern.Find, RegexOptions.IgnoreCase).Success;
            }
            else
            {
                if (pattern.CaseSensitive)
                    found = lineContent.IndexOf(pattern.Find, StringComparison.InvariantCulture) != -1;
                else
                    found = lineContent.IndexOf(pattern.Find, StringComparison.InvariantCultureIgnoreCase) != -1;
            }

            return found;
        }

        public Task HandleAsync(Events.DeeplTransEvent message, CancellationToken cancellationToken)
        {
            ScratchZone = message.TransLine;

            return Task.CompletedTask;
        }
    }



    public class ScriptSearchResult
    {
        public string SourcePath { get; set; }
        public ScriptTranslation Script { get; set; }
        public ScriptTranslationItem Item { get; set; }
        public string Find { get; set; }
        public string Replace { get; set; }
    }
}
