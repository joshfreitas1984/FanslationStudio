using Caliburn.Micro;
using FanslationStudio.MigratedCatTool.Config;
using FanslationStudio.MigratedCatTool.Events;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FanslationStudio.MigratedCatTool
{
    public class CatViewModel : PropertyChangedBase, IHandle<DeeplTransEvent>
    {
        //Clipboard Hooks
        //private static SharpClipboard _clipboard = new SharpClipboard();
        //private string _lastClipboardContent;

        //Event Aggregator for talking to View
        private IEventAggregator _eventAggregator;

        //Regex Patterns
        private const string ChineseCharacterPattern = @"[\u4E00-\u9FCC]";

        //Directories
        private string _rawFolder = @"../../../../../GameTranslator/GameTranslator/Hlts/splitoutput/";
        private string _initialTranslationFolder = @"../../../../../GameTranslator/GameTranslator/Hlts/combinedtrans/";
        private string _cleanedupFolder = @"../../../../../GameTranslator/GameTranslator/Hlts/cleanedup/";

        //Search Patterns
        private const string _searchPatternFile = "SearchPatterns.config";
        private string _newRegex;
        private string _newReplacement;
        private bool _caseSensitive;
        private ObservableCollection<SearchPattern> _searchPatterns;
        private SearchPattern _selectedSearchPattern;

        //Item Details
        //private ObservableCollection<FoundLine> _foundLines;
        //private FoundLine _selectedFoundLine;
        private string _foundItemsDisplay;
        private string _rawLine;
        private string _currentTransLine;
        private string _newTransLine;
        private string _scatchLine;
        private string _findValue;
        private string _replaceValue;

        //File Lines
        //private Dictionary<string, FileWithLines> _rawLines;
        //private Dictionary<string, FileWithLines> _initialTranslationLines;
        //private Dictionary<string, FileWithLines> _cleanedUpLines;

        public string NewRegex
        {
            get { return _newRegex; }
            set
            {
                _newRegex = value;
                NotifyOfPropertyChange(() => NewRegex);
            }
        }
        public string NewReplacement
        {
            get { return _newReplacement; }
            set
            {
                _newReplacement = value;
                NotifyOfPropertyChange(() => NewReplacement);
            }
        }
        public bool NewCaseSensitive
        {
            get { return _caseSensitive; }
            set
            {
                _caseSensitive = value;
                NotifyOfPropertyChange(() => NewCaseSensitive);
            }
        }
        public SearchPattern SelectedSearchPattern
        {
            get { return _selectedSearchPattern; }
            set
            {
                _selectedSearchPattern = value;
                NotifyOfPropertyChange(() => _selectedSearchPattern);
            }
        }
        public ObservableCollection<SearchPattern> SearchPatterns
        {
            get { return _searchPatterns; }
            set
            {
                _searchPatterns = value;
                NotifyOfPropertyChange(() => SearchPatterns);
            }
        }
        //public ObservableCollection<FoundLine> FoundLines
        //{
        //    get { return _foundLines; }
        //    set
        //    {
        //        _foundLines = value;
        //        NotifyOfPropertyChange(() => FoundLines);
        //    }
        //}
        //public FoundLine SelectedFoundLine
        //{
        //    get { return _selectedFoundLine; }
        //    set
        //    {
        //        _selectedFoundLine = value;
        //        NotifyOfPropertyChange(() => SelectedFoundLine);
        //        SelectFoundLine();
        //    }
        //}
        public string FoundItemsDisplay
        {
            get { return _foundItemsDisplay ?? "Found Items:"; }
            set
            {
                _foundItemsDisplay = value;
                NotifyOfPropertyChange(() => FoundItemsDisplay);
            }
        }
        public string RawLine
        {
            get { return _rawLine; }
            set
            {
                _rawLine = value;
                NotifyOfPropertyChange(() => RawLine);
            }
        }
        public string CurrentTransLine
        {
            get { return _currentTransLine; }
            set
            {
                _currentTransLine = value;
                NotifyOfPropertyChange(() => CurrentTransLine);
            }
        }
        public string NewTransLine
        {
            get { return _newTransLine; }
            set
            {
                _newTransLine = value;
                NotifyOfPropertyChange(() => NewTransLine);
            }
        }
        public string ScratchLine
        {
            get { return _scatchLine; }
            set
            {
                _scatchLine = value;
                NotifyOfPropertyChange(() => ScratchLine);
            }
        }
        public string FindValue
        {
            get { return _findValue; }
            set
            {
                _findValue = value;
                NotifyOfPropertyChange(() => FindValue);
            }
        }
        public string ReplaceValue
        {
            get { return _replaceValue; }
            set
            {
                _replaceValue = value;
                NotifyOfPropertyChange(() => ReplaceValue);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public CatViewModel(IEventAggregator eventAggregator)
        {
            //_clipboard.ClipboardChanged += ClipboardHasChanged;
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            //_foundLines = new ObservableCollection<FoundLine>();
            NewRegex = "New";
            NewReplacement = "Relacement";


            LoadSearchPatterns();

            //LoadAllLines();
        }

        #region Actions

        public void AddSearchPattern()
        {
            SearchPatterns.Add(new SearchPattern(NewRegex, NewReplacement, NewCaseSensitive));
            WriteSearchPatterns();
        }

        public void RemoveSearchPattern()
        {
            SearchPatterns.Remove(SelectedSearchPattern);
            WriteSearchPatterns();
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

        //public void SearchInitialFiles()
        //{
        //    SearchForLines(_initialTranslationLines, true);
        //}

        //public void SearchCleanFiles()
        //{
        //    SearchForLines(_cleanedUpLines, false);
        //}

        //public void SearchOutputFiles()
        //{
        //    SearchForLines(_rawLines, false);
        //}

        public void CopyFromCurrentToScratch()
        {
            ScratchLine = CurrentTransLine;
        }

        public void CopyFromNewToScratch()
        {
            ScratchLine = NewTransLine;
        }

        //public void ReplaceAllInScratch()
        //{
        //    ScratchLine = Regex.Replace(ScratchLine, FindValue, ReplaceValue,
        //        _selectedFoundLine.Pattern.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
        //}

        //public void NewAddToCleanUp()
        //{
        //    if (_selectedFoundLine is null)
        //        return;

        //    WriteCleanupFileWithNewLine(_newTransLine);
        //    MoveToNextFoundLine();
        //}

        //public void ScratchAddToCleanUp()
        //{
        //    if (_selectedFoundLine is null)
        //        return;

        //    WriteCleanupFileWithNewLine(_scatchLine);
        //    MoveToNextFoundLine();
        //}

        #endregion

        #region Private Methods 

        //private void SelectFoundLine()
        //{
        //    NewTransLine = string.Empty;
        //    ScratchLine = string.Empty;
        //    FindValue = string.Empty;
        //    ReplaceValue = string.Empty;

        //    if (_selectedFoundLine is null)
        //    {
        //        RawLine = string.Empty;
        //        CurrentTransLine = string.Empty;
        //    }
        //    else
        //    {
        //        var file = _selectedFoundLine.FileName;
        //        var lineId = _selectedFoundLine.Info.ToNewLineId();

        //        RawLine = _rawLines[file].Lines[lineId].LineContent;
        //        CurrentTransLine = _initialTranslationLines[file].Lines[lineId].LineContent;
        //        FindValue = _selectedFoundLine.Pattern.Regex;
        //        ReplaceValue = _selectedFoundLine.Pattern.Replacement;

        //        //Override with clean up if its already been cleaned up
        //        if (_cleanedUpLines.ContainsKey(file) && _cleanedUpLines[file].Lines.ContainsKey(lineId))
        //        {
        //            CurrentTransLine = _cleanedUpLines[file].Lines[lineId].LineContent;
        //            NewTransLine = CurrentTransLine;
        //        }

        //        _eventAggregator.PublishOnUIThread(new RawLineCopiedEvent(RawLine));
        //    }
        //}

        //private void ClipboardHasChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        //{
        //    //Only care if it comes from this app
        //    if (e.SourceApplication.Name != "LashCAT.exe")
        //        return;

        //    //Only care about text
        //    if (e.ContentType != SharpClipboard.ContentTypes.Text)
        //        return;

        //    //If we've been called twice (dont know why it does this)
        //    string content = e.Content.ToString();
        //    if (content == _lastClipboardContent)
        //        return;

        //    //If not chinese assume its from the browser windows and put it in new trans to save time
        //    if (!Regex.Match(content, ChineseCharacterPattern).Success)           
        //    {
        //        NewTransLine = content;
        //    }

        //    _lastClipboardContent = content;
        //}

        /// <summary>
        /// Search for lines in our collection
        /// </summary>
        /// <param name="filesWithLines">Collection with files with lines</param>
        //private void SearchForLines(Dictionary<string, FileWithLines> filesWithLines, bool excludeLinesAlreadyCleaned)
        //{
        //    var stopWatch = Stopwatch.StartNew();
        //    var patterns = _searchPatterns.ToArray(); //Small speed improvement

        //    var foundLines = new List<FoundLine>();
        //    _foundLines.Clear();

        //    //Search for matches in Initial translation
        //    foreach (var file in filesWithLines)
        //    {
        //        foreach (var line in file.Value.Lines)
        //        {
        //            var lineContent = line.Value.LineContent;

        //            //Go through all the patterns
        //            foreach (var pattern in patterns)
        //            {
        //                bool found = FindPattern(lineContent, pattern);

        //                if (excludeLinesAlreadyCleaned)
        //                {
        //                    if (_cleanedUpLines.ContainsKey(file.Key)
        //                        && _cleanedUpLines[file.Key].Lines.ContainsKey(line.Key))
        //                    {
        //                        continue;
        //                    }
        //                }

        //                if (found)
        //                {
        //                    if (foundLines.Any(l => l.FileName == file.Key && l.Info.LineId == line.Value.LineId))
        //                        continue;

        //                    foundLines.Add(new FoundLine(file.Key, line.Value, pattern));

        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    foreach (var found in foundLines.OrderBy(f => f.Pattern.Replacement).ThenBy(f => f.Pattern.Regex))
        //    {
        //        _foundLines.Add(found);
        //    }

        //    stopWatch.Stop();
        //    FoundItemsDisplay = $"Found Items {_foundLines.Count} Time: {stopWatch.ElapsedMilliseconds} ms";
        //}

        private static bool FindPattern(string lineContent, SearchPattern pattern)
        {
            bool found;

            //Regex matching is quite slow so lets just index of stuff that isnt a regex
            if (pattern.IsRegex)
            {
                if (pattern.CaseSensitive)
                    found = Regex.Match(lineContent, pattern.Regex).Success;
                else
                    found = Regex.Match(lineContent, pattern.Regex, RegexOptions.IgnoreCase).Success;
            }
            else
            {
                if (pattern.CaseSensitive)
                    found = lineContent.IndexOf(pattern.Regex, StringComparison.InvariantCulture) != -1;
                else
                    found = lineContent.IndexOf(pattern.Regex, StringComparison.InvariantCultureIgnoreCase) != -1;
            }

            return found;
        }

        /// <summary>
        /// Cache Lines in Memory
        /// </summary>
        //private void LoadAllLines()
        //{
        //    _rawLines = LoadLinesForFolder(_rawFolder);
        //    _initialTranslationLines = LoadLinesForFolder(_initialTranslationFolder);
        //    _cleanedUpLines = LoadLinesForFolder(_cleanedupFolder);
        //}

        //private Dictionary<string, FileWithLines> LoadLinesForFolder(string folder)
        //{
        //    var response = new Dictionary<string, FileWithLines>();

        //    //Files in Folder ordered numerically if they have a number
        //    var files = Directory.GetFiles(folder)
        //        .OrderBy(f =>
        //        {
        //            var matches = Regex.Match(f, @"(\d+)"); //Any digit
        //            if (matches.Length == 0)
        //                return 0;
        //            else
        //                return Convert.ToInt32(matches.Value);
        //        });

        //    foreach (var file in files)
        //    {
        //        var lines = File.ReadAllLines(file).ToArray();
        //        var loadedFile = new FileWithLines(file);

        //        //Read Lines and extract content
        //        foreach (var line in lines)
        //        {
        //            var info = LineInfo.ExtractLineInfo(line, null);
        //            loadedFile.Lines.Add(info.ToNewLineId(), info);
        //        }

        //        response.Add(loadedFile.FileNameWithoutExtension, loadedFile);
        //    }

        //    return response;
        //}

        private void LoadSearchPatterns()
        {
            _searchPatterns = new ObservableCollection<SearchPattern>();

            if (File.Exists(_searchPatternFile))
            {
                string contents = File.ReadAllText(_searchPatternFile);
                _searchPatterns = JsonSerializer.Deserialize<ObservableCollection<SearchPattern>>(contents);
            }
        }

        private void WriteSearchPatterns()
        {
            var jsonString = JsonSerializer.Serialize(_searchPatterns, new JsonSerializerOptions() { WriteIndented = true });

            if (File.Exists(_searchPatternFile))
                File.Delete(_searchPatternFile);

            File.WriteAllText(_searchPatternFile, jsonString);
        }

        //private void MoveToNextFoundLine()
        //{
        //    var newIndex = _foundLines.IndexOf(_selectedFoundLine) + 1;
        //    if (newIndex < _foundLines.Count)
        //    {
        //        SelectedFoundLine = _foundLines[newIndex];
        //    }
        //}

        //private void WriteCleanupFileWithNewLine(string line)
        //{
        //    var file = _selectedFoundLine.FileName;
        //    var lineId = _selectedFoundLine.Info.ToNewLineId();

        //    string cleanedUpFileName = $"{_cleanedupFolder}{file}.txt";
        //    FileWithLines fileWithLines;

        //    //Sort out Memory collection
        //    if (_cleanedUpLines.ContainsKey(file) && _cleanedUpLines[file].Lines.ContainsKey(lineId))
        //    {
        //        fileWithLines = _cleanedUpLines[file];
        //        fileWithLines.Lines[lineId].LineContent = line;
        //    }
        //    else
        //    {
        //        //Load FileWithLines
        //        if (!_cleanedUpLines.ContainsKey(file))
        //        {
        //            fileWithLines = new FileWithLines(cleanedUpFileName);
        //        }
        //        else
        //            fileWithLines = _cleanedUpLines[file];

        //        //Create Info
        //        var info = new LineInfo()
        //        {
        //            LineContent = line,
        //            LineId = lineId,
        //            LineNumber = _selectedFoundLine.Info.LineNumber,
        //            LineSplitIndex = _selectedFoundLine.Info.LineSplitIndex,
        //        };

        //        fileWithLines.Lines.Add(lineId, info);
        //    }

        //    //Write to new file first
        //    string backupFolder = $"{_cleanedupFolder}/backup/";
        //    if (!Directory.Exists(backupFolder))
        //        Directory.CreateDirectory(backupFolder);

        //    //Backup it up
        //    string backupFileName = $"{backupFolder}/{file}.txt";
        //    //Only delete it if we have a new one
        //    if (File.Exists(backupFileName) && File.Exists(cleanedUpFileName))
        //        File.Delete(backupFileName);

        //    if (File.Exists(cleanedUpFileName))
        //    {
        //        var originalLines = File.ReadAllLines(cleanedUpFileName).ToArray();
        //        Debug.WriteLine($"{file}:LineCount:{originalLines.Length}");
        //        File.Move(cleanedUpFileName, backupFileName);
        //    }

        //    //Write the new one
        //    var cleanLines = fileWithLines.ToLineList();
        //    Debug.WriteLine($"{file}:NewLineCount:{cleanLines.Length}");
        //    File.WriteAllLines(cleanedUpFileName, cleanLines);
        //}

        #endregion

        public Task HandleAsync(DeeplTransEvent message, CancellationToken cancellationToken)
        {
            NewTransLine = message.TransLine;

            return Task.CompletedTask;
        }
    }
}
