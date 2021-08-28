using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FanslationStudio.Domain.ScriptToTranslate
{
    public class SplitTextFileToTranslate : IScriptToTranslate
    {
        public string SourcePath { get; set; }
        public List<int> SplitIndexes { get; set; }
        public string SplitCharacters { get; set; }

        //If false use line id
        public bool UseIndexForLineId { get; set; }
        public int LineIdIndex { get; set; }

        //Should this be here - or should we use some injectable set of pre/post processing
        public int OverrideFontSize { get; set; }

        public List<ScriptTranslation> GetTranslationLines(string rawFolder, Project project)
        {
            var translations = new List<ScriptTranslation>();
            string rawFile = $"{rawFolder}\\{SourcePath}";

            //Force full read not stream
            var lines = File.ReadAllLines(rawFile).ToArray();
            string lineId;

            int lineNum = 0;
            foreach (var line in lines)
            {
                //Skip empties
                if (string.IsNullOrEmpty(line))
                    continue;

                var lineSplits = line.Split(SplitCharacters);
                lineNum++;

                //Calculate the Line Id
                if (UseIndexForLineId)
                    lineId = lineSplits[LineIdIndex];
                else
                    lineId = lineNum.ToString();

                //Create Raw Translation
                var translation = new ScriptTranslation()
                {
                    LineId = lineId,
                    RawLine = line,
                    Items = new List<ScriptTranslationItem>(),
                };

                for (int i = 0; i < lineSplits.Length; i++)
                {
                    var item = new ScriptTranslationItem()
                    {
                        Raw = lineSplits[i],
                        ItemSequence = i,
                    };

                    //If we are a split that requires translating
                    if (SplitIndexes.Contains(i))
                    {
                        item.RequiresTranslation = true;
                        item.CleanedUpLine = PreProcessLine(lineSplits[i], project.PreProcessingItems);
                    }

                    translation.Items.Add(item);
                }

                translations.Add(translation);
            }

            return translations;
        }

        public void OutputLines(List<ScriptTranslation> scripts, string outputFolder, Project project)
        {
            List<string> lines = new List<string>();

            foreach (var script in scripts)
            {
                var line = new StringBuilder();

                foreach (var item in script.Items)
                {
                    var itemToAppend = item.ResultingTranslation;

                    if (string.IsNullOrEmpty(itemToAppend))
                        itemToAppend = item.Raw;

                    //Add Post Processing
                    if (item.RequiresTranslation)
                    {
                        //itemToAppend = CleanupTranslatedFile(itemToAppend);
                        itemToAppend = PostProcessLine(itemToAppend, project.PostProcessingItems);
                        itemToAppend = OverrideFontSizeOnLine(itemToAppend);
                    }

                    line.Append(itemToAppend);
                    line.Append(SplitCharacters);
                }

                if (line.ToString().EndsWith(SplitCharacters))
                {
                    line.Remove(line.Length - SplitCharacters.Length, SplitCharacters.Length);
                }

                lines.Add(line.ToString());
            }

            string file = $"{outputFolder}\\{SourcePath}";
            string folder = Path.GetDirectoryName(file);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllLines(file, lines.ToArray());
        }

        private const string _wierdOpen = "【";
        private const string _wierdClose = "】";
        private const string _wideComma = "，";
        private const string _wideBullets = "…";

        public string PreProcessLine(string line, List<PreProcessing.IPreProcessing> preProcessings)
        {
            foreach (var p in preProcessings)
                line = p.ProcessLine(line);

            return line;
        }

        public string PostProcessLine(string line, List<PostProcessing.IPostProcessing> preProcessings)
        {
            line = line.Trim(); //Trim it first because we might put in legit spaces after

            foreach (var p in preProcessings)
                line = p.ProcessLine(line);

            return line;
        }

        [Obsolete]
        public string CleanUpLine(string line)
        {
            //DeepL doesnt really like dealing with markup so we are going to encase funky characters and mark up blocks with <>'s so it thinks they are XML

            //Things that will make it easier to translate
            //Remove Any <html> tags
            //Replace these '…' with '..'            
            //Encase /n and /t with <>        
            var removePatterns = new[] {
                @"<[^>]*>",
                @"{color[^}]*}", @"{/color[^}]*}", @"{\\color[^}]*}",
                @"{size[^}]*}", @"{/size[^}]*}", @"{\\size[^}]*}",
                @"{back[^}]*}", @"{/back[^}]*}", @"{\\back[^}]*}",
                @"{vpunch[^}]*}", @"{/vpunch[^}]*}", @"{\\vpunch[^}]*}",
                @"{punch[^}]*}", @"{/punch[^}]*}", @"{\\punch[^}]*}",
                @"{shake[^}]*}", @"{/shake[^}]*}", @"{\\shake[^}]*}",
                @"{w[^}]*}", @"{/w[^}]*}", @"{\\w[^}]*}", //needs to be last
                @"{s[^}]*}", @"{/s[^}]*}", @"{\\s[^}]*}", //needs to be last
            };

            foreach (var pattern in removePatterns)
            {
                var splits = Regex.Matches(line, pattern);

                //if (splits.Count > 0)
                //    Debug.WriteLine($"File: {format.FileName} Pattern: {pattern}");

                foreach (var split in splits)
                {
                    //if (split.ToString() != "<br>")
                    line = line.Replace(split.ToString(), "");
                }
            }

            line = line
                .Replace(@"\n", @"<\n>")
                .Replace(@"\t", @"<\t>")
                .Replace(@"{p}", "")
                .Replace(@"{/p}", "")
                .Replace(@"{\p}", "")

                .Replace("{", "<")
                .Replace("}", ">")
                .Replace(_wideComma, ",")
                .Replace(_wideBullets, "...")
                .Replace(_wierdOpen, @"<[>")
                .Replace(_wierdClose, @"<]>");

            //Consider
            //- Convert \n to actual line feeds so we can tran
            //- this could be better or worse because line feeds might have been used to trim text not take it outta context

            return line;
        }

        [Obsolete]
        public string CleanupTranslatedFile(string line)
        {
            //Do a little bit of content clean up so that we dont pick it up in stuff we need to manually do
            var response = line
                .Replace("0 >", "0>")
                .Replace("< 0", "<0")
                .Replace("<0>", "{0}")

                .Replace("1 >", "1>")
                .Replace("< 1", "<1")
                .Replace("<1>", "{1}")

                .Replace("2 >", "2>")
                .Replace("< 2", "<2")
                .Replace("<2>", "{2}")

                .Replace("3 >", "3>")
                .Replace("< 3", "<3")
                .Replace("<3>", "{3}")
                .Replace("<<", "<")
                .Replace(">>", ">")

                //Brackets
                .Replace("[ >", "[>")
                .Replace("< [", "<[")
                .Replace(@"<[>", "[")
                .Replace("] >", "]>")
                .Replace("< ]", "<]")
                .Replace(@"<]>", "]")
                .Replace(@"[>", "[")
                .Replace(@"<]", "]")
                .Replace(@">]", "]")

                //Messed up numbers
                .Replace("{0th", "{0")
                .Replace("{1st", "{1")
                .Replace("{2nd", "{2")
                .Replace("{0 ", "{0")
                .Replace("{1 ", "{1")
                .Replace("{2 ", "{2")
                .Replace("{0: ", "{0:")
                .Replace("{1: ", "{1:")
                .Replace("{2: ", "{2:")

                //Name tags
                .Replace(@"name_1 >", @"name_1>")
                .Replace(@"<name_1", @"<name_1")
                .Replace(@"<name_1>", @"{name_1}")
                .Replace(@"name_2 >", @"name_2>")
                .Replace(@"<name_2", @"<name_2")
                .Replace(@"<name_2>", @"{name_2}")

                //...s
                .Replace(@".........", "....")
                .Replace(@"........", "....")
                .Replace(@".......", "....")
                .Replace(@".....", "....")

                //Add in manual Spaces
                .Replace("<space>", " ")

                .Replace(@"<\n>", @"\n")
                .Replace(@"<\t>", @"\t")

                .Trim();

            //Shouldnt have any html tags left
            var matches = Regex.Matches(response, @"<[^>]*>");

            if (matches.Count() > 0)
                Console.WriteLine(response);

            //Put spaces after name tags
            var nameWithoutSpace = Regex.Matches(response, @"({name_\d}[A-z])");

            if (nameWithoutSpace.Count > 0)
            {
                foreach (var match in nameWithoutSpace.AsEnumerable())
                {
                    var start = match.Value.Substring(0, match.Length - 1);
                    var end = match.Value.Substring(match.Length - 1, 1);
                    string newReplace = $"{start} {end}";
                    response = response.Replace(match.Value, newReplace);
                }
            }

            response = OverrideFontSizeOnLine(response);

            return response;
        }

        private string OverrideFontSizeOnLine(string line)
        {
            //Add in Size override 
            if (OverrideFontSize != 0)
            {
                //TODO: Move to config
                var exclusions = new List<string>
                {
                    "Ade Ake",
                    "Li Tan",
                    "Qi Xiaoer",
                    "Wang Zhe",
                    "He Yuqing",
                    "Duan Siping",
                    "Duan Siliang",
                    "He Ziwan",
                    "Huang Shang",
                    "Fan Xiangdie",
                    "Bai Sun",
                    "Fan Xiangdie",
                    "Luo Yuanyu",
                    "Zhang Junbao",
                    "Yan Yushu",
                    "Cheng Yanhua",
                    "Situ Jing",
                    "Zhuan Sun Ning",
                    "Shi Hongtu"
                };

                if (!exclusions.Contains(line) && !line.StartsWith("<size="))
                {
                    line = $"<size={OverrideFontSize}>{line}</size>";
                }
            }

            return line;
        }

        public void WriteProgress(double lineCount, double currentLine)
        {
            double progress = currentLine / lineCount * 100.0;
            Console.WriteLine($"Writen to: {currentLine} ~~ ({progress} %)");
        }
    }
}
