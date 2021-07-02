using FanslationStudio.Domain;

namespace FanslationStudio.UserExperience.Events
{
    public class SelectProjectVersionEvent
    {
        public ProjectVersion SelectedVersion { get; set; }
    }
}
