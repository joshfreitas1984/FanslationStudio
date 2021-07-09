using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FanslationStudio.Domain.PostProcessing
{
    public class PostFindReplace: SearchPattern, IPostProcessing
    {
        public string ProcessLine(string line)
        {
            if (!Enabled)
                return line;

            if (IsRegex)
            {
                bool something = (Replacement.Contains("{") && Regex.IsMatch(line, Find)); //TODO: Still needs a valid test to make sure it works

                if (something)
                    Console.WriteLine($"Original Line: {line}");

                line = Regex.Replace(line, Find, Replacement);

                if (something)
                    Console.WriteLine($"New Line: {line}");
            }
            else
                line = line.Replace(Find, Replacement);

            return line;
        }
    }
}
