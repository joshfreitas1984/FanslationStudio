using FanslationStudio.Domain;

namespace FanslationStudio.UserExperience.Events
{
    public class ImportProjectVersionEvent
    {
        public ProjectVersion SelectedVersion { get; set; }
    }
}
