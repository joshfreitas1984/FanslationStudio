using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience.Events
{
    public class SelectProjectEvent
    {
        public Project SelectedProject { get; set; }
    }
}
