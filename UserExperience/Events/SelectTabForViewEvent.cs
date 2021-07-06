using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience.Events
{
    public class SelectTabForViewEvent
    {
        public string TabName { get; set; }

        public SelectTabForViewEvent(string name)
        {
            TabName = name;
        }
    }
}
