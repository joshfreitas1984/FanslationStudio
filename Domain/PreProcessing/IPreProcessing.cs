using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.Domain.PreProcessing
{
    public interface IPreProcessing
    {
        string ProcessLine(string line);
    }
}
