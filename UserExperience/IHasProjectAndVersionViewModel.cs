using FanslationStudio.Domain;

namespace FanslationStudio.UserExperience
{
    public interface IHasProjectAndVersionViewModel
    {
        Config Config { get; set; }
        Project Project { get; set; }
        ProjectVersion Version { get; set; }
    }
}
