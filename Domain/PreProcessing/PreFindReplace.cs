using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FanslationStudio.Domain.PreProcessing
{
    public class PreFindReplace : SearchPattern, IPreProcessing
    {
        public string ProcessLine(string line)
        {
            if (!Enabled)
                return line;

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
