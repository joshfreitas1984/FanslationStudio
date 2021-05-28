using FanslationStudio.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.Services
{
    public class ProjectFolderService
    {
        public static string CalculateProjectFolder(string workshopFolder, string name)
        {
            return $"{workshopFolder}\\{name}";
        }

        public static string CalculateRawFolder(string projectFolder)
        {
            return $"{projectFolder}\\Raw";
        }

        public static string CalculateRawVersionFolder(string projectFolder, ProjectVersion version)
        {
            return $"{projectFolder}\\Raw\\{version.Version}";
        }

        public static string CalculateTranslationFolder(string projectFolder)
        {
            return $"{projectFolder}\\Translation";
        }

        public static string CalculateTranslationVersionFolder(string projectFolder, ProjectVersion version)
        {
            return $"{projectFolder}\\Translation\\{version.Version}";
        }

        public static string CalculateOutputFolder(string projectFolder)
        {
            return $"{projectFolder}\\Output";
        }

        public static string CalculateOutputVersionFolder(string projectFolder, ProjectVersion version)
        {
            return $"{projectFolder}\\Output\\{version.Version}";
        }
    }
}
