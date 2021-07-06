using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience.Events
{
    public class GenerateOutputFilesEvent
    {
        public ProjectVersion SelectedVersion { get; set; }
    }
}
