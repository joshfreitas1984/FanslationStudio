using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FanslationStudio.ScriptToTranslate
{
    public class SplitTextFileToTranslate : IScriptToTranslate
    {
        public string SourcePath { get; set; }
        public int[] SplitIndexes { get; set; }
        public string SplitCharacters { get; set; }

        //If false use line id
        public bool UseIndexForLineId { get; set; }
        public int LineIdIndex { get; set; }

        //Should this be here - or should we use some injectable set of pre/post processing
        public int OverrideFontSize { get; set; }

        public List<ScriptTranslation> GetTranslationLines(string rawFolder)
        {
            var translations = new List<ScriptTranslation>();
            string rawFile = $"{rawFolder}\\{SourcePath}";

            //Force full read not stream
            var lines = File.ReadAllLines(rawFile).ToArray();
            string lineId; 

            int lineNum = 0;
            foreach (var line in lines)
            {
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

                for(int i = 0; i < lineSplits.Length; i++)
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
                        item.CleanedUpLine = CleanUpLine(lineSplits[i]);
                    }

                    translation.Items.Add(item);
                }

                translations.Add(translation);
            }           

            return translations;
        }

        public void OutputLines(List<ScriptTranslation> scripts, string outputFolder)
        {
            List<string> lines = new List<string>();

            foreach (var script in scripts)
            {
                var line = new StringBuilder();

                foreach (var item in script.Items)
                {
                    var itemToAppend = item.ResultingTranslation;
                    //Add Post Processing
                    if (OverrideFontSize > 0 && item.RequiresTranslation)
                    {
                        if(!itemToAppend.StartsWith("<size="))
                        {
                            itemToAppend = $"<size={OverrideFontSize}>{itemToAppend}</size>";
                        }
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

        public string CleanUpLine(string line)
        {
            //DeepL doesnt really like dealing with markup so we are going to encase funky characters and mark up blocks with <>'s so it thinks they are XML

            //Things that will make it easier to translate
            //Remove Any <html> tags
            //Replace these '…' with '..'            
            //Encase /n and /t with <>        
            var removePatterns = new[] {
                @"<[^>]*>",
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

        public void WriteProgress(double lineCount, double currentLine)
        {
            double progress = currentLine / lineCount * 100.0;
            Console.WriteLine($"Writen to: {currentLine} ~~ ({progress} %)");
        }
    }
}
