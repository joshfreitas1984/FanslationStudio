using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience.Events
{
    public class MergeVersionEvent
    {
        public Project Project { get; set; }
        public ProjectVersion VersionToMerge { get; set; }
        public ProjectVersion TargetVersion { get; set; }
    }
}
