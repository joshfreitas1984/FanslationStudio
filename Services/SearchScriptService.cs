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
            bool isSearchingRaw,
            bool searchWhereHasMergeChanges,
            bool searchWhereUntranslated,
            bool searchMismatches,
            string quickSearchTerm)
        {
            var foundResults = new ConcurrentBag<ScriptSearchResult>();

            Parallel.ForEach(_scripts, scriptEntry =>
            {              
                if (!isSearchingRaw) //Id Search
                {
                    var foundScripts = scriptEntry.Value
                        .SelectMany(s => s.Items, (script, item) => new { script, item })
                        .Where(s => s.item.RequiresTranslation == true
                            && s.script.LineId.Contains(quickSearchTerm ?? string.Empty));

                    foreach (var foundScript in foundScripts)
                    {
                        foundResults.Add(new ScriptSearchResult()
                        {
                            SourcePath = scriptEntry.Key,
                            Script = foundScript.script,
                            Item = foundScript.item,
                        });
                    }
                }
                else
                {
                    //Bulk Searches
                    if (searchMismatches)
                    {
                        var scriptParts = scriptEntry.Value
                            .SelectMany(s => s.Items, (script, item) => new { script, item })
                            .Where(s => s.item.RequiresTranslation == true);

                        var multiParts =
                            (from s in scriptParts
                             group s by s.item.Raw into g
                             where g.Count() > 1
                             select new { Raw = g.Key, MinTrans = g.Min(i => i.item.ResultingTranslation), MaxTrans = g.Max(i => i.item.ResultingTranslation), Count = g.Count() })
                            .ToList();

                        var foundSegments = multiParts
                            .Where(x => x.MinTrans != x.MaxTrans)
                            .Select(s => s.Raw)
                            .ToList();

                        var foundItems =
                            (from p in scriptParts
                             join s in foundSegments
                             on p.item.Raw equals s
                             select p)
                            .ToList();

                        foreach (var i in foundItems)
                        {
                            foundResults.Add(new ScriptSearchResult()
                            {
                                SourcePath = scriptEntry.Key,
                                Script = i.script,
                                Item = i.item,
                            });
                        }
                    }

                    foreach (var script in scriptEntry.Value)
                    {
                        foreach (var scriptItem in script.Items)
                        {
                            if (!scriptItem.RequiresTranslation)
                                continue;

                            string resultingTranslation = scriptItem.ResultingTranslation;
                            //if (resultingTranslation == scriptItem.Raw)
                            //    resultingTranslation = string.Empty;

                            //Add Merge Changes
                            if (searchWhereHasMergeChanges && scriptItem.MergeHadRawChanges)
                                foundResults.Add(new ScriptSearchResult()
                                {
                                    SourcePath = scriptEntry.Key,
                                    Script = script,
                                    Item = scriptItem,
                                });

                            //Add Untranslated
                            if (searchWhereUntranslated && !string.IsNullOrEmpty(scriptItem.Raw) && (string.IsNullOrEmpty(resultingTranslation) || resultingTranslation == scriptItem.CleanedUpLine))
                                foundResults.Add(new ScriptSearchResult()
                                {
                                    SourcePath = scriptEntry.Key,
                                    Script = script,
                                    Item = scriptItem,
                                });

                            //Add found terms
                            if (!string.IsNullOrEmpty(quickSearchTerm) && FindPattern(resultingTranslation, new SearchPattern() { CaseSensitive = false, Find = quickSearchTerm }))
                                foundResults.Add(new ScriptSearchResult()
                                {
                                    SourcePath = scriptEntry.Key,
                                    Script = script,
                                    Item = scriptItem,
                                    Find = quickSearchTerm,
                                });
                        }
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
                    found = lineContent.Contains(pattern.Find, StringComparison.InvariantCulture);
                else
                    found = lineContent.Contains(pattern.Find, StringComparison.InvariantCultureIgnoreCase);
            }

            return found;
        }
    }
}
