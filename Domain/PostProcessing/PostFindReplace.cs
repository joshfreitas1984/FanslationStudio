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
            if (IsRegex)
            {
                line = Regex.Replace(line, Find, Replacement);
            }
            else
                line = line.Replace(Find, Replacement);

            return line;
        }
    }
}
