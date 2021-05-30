using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience.Events
{
    public class SelectProjectEvent
    {
        Project SelectedProject { get; set; }
    }

    public class SelectProjectVersionEvent
    {
        ProjectVersion SelectedVersion { get; set; }
    }
}
