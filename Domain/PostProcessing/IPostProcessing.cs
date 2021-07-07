using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.Domain.PostProcessing
{
    public interface IPostProcessing
    {
        string ProcessLine(string line);
    }
}
