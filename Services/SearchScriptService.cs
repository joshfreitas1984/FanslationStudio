using FanslationStudio.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FanslationStudio.Services
{
    public static class SearchScriptService
    {
        public static List<ScriptSearchResult> QuickSearch(Dictionary<string, List<ScriptTranslation>> _scripts,
            bool searchWhereHasMergeChanges, bool searchWhereUntranslated, string quickSearchTerm)
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

                        //Add Merge Changes
                        if (searchWhereHasMergeChanges && scriptItem.MergeHadRawChanges)
                            foundResults.Add(new ScriptSearchResult()
                            {
                                SourcePath = scriptEntry.Key,
                                Script = script,
                                Item = scriptItem,
                            });
                        //Add Untranslated
                        else if (searchWhereUntranslated && !string.IsNullOrEmpty(scriptItem.Raw) && string.IsNullOrEmpty(resultingTranslation))
                            foundResults.Add(new ScriptSearchResult()
                            {
                                SourcePath = scriptEntry.Key,
                                Script = script,
                                Item = scriptItem,
                            });
                        //Add found terms
                        else if (!string.IsNullOrEmpty(quickSearchTerm) && resultingTranslation.Contains(quickSearchTerm))
                            foundResults.Add(new ScriptSearchResult()
                            {
                                SourcePath = scriptEntry.Key,
                                Script = script,
                                Item = scriptItem,
                                Find = quickSearchTerm,
                            });
                    }
                }
            });

           return foundResults.ToList();
        }

        public static List<ScriptSearchResult> SearchWithPatterns(Dictionary<string, List<ScriptTranslation>> scripts,
            List<SearchPattern> searchPatterns)
        {
            var foundResults = new ConcurrentBag<ScriptSearchResult>();

            Parallel.ForEach(scripts, scriptEntry =>
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

                        foreach (var pattern in searchPatterns)
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

            return foundResults.ToList();
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
    }
}
