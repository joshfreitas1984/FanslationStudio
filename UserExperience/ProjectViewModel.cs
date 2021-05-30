using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.ScriptToTranslate;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.UserExperience
{
    public class ProjectViewModel: Screen, IHasProjectAndVersionViewModel
    {

        #region IHasProjectAndVersionViewModel

        private Config _config;
        private Project _project;
        private ProjectVersion _version;

        public Config Config
        {
            get
            {
                return _config;
            }
            set
            {
                _config = value;
                NotifyOfPropertyChange(() => Config);
            }
        }

        public Project Project
        {
            get
            {
                return _project;
            }
            set
            {
                _project = value;
                NotifyOfPropertyChange(() => Project);
                NotifyOfPropertyChange(() => ProjectFile);
            }
        }

        public ProjectVersion Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                NotifyOfPropertyChange(() => Version);
            }
        }

        #endregion

        public string ProjectName
        {
            get
            {
                return _project.Name;
            }
            set
            {
                _project.Name = value;
                NotifyOfPropertyChange(() => ProjectName);
            }
        }
        public string ProjectFile
        {
            get
            {
                return _project.ProjectFile;
            }
        }

        public void VersionSelectionChanged(ProjectVersion version)
        {
        }

        public void DeleteVersion(ProjectVersion version)
        {
        }

        public void ScriptSelectionChanged(IScriptToTranslate script)
        {
        }

        public void DeleteScript(IScriptToTranslate script)
        {
        }
    }
}
