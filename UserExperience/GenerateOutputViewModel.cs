using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FanslationStudio.UserExperience
{
    public class GenerateOutputViewModel : Screen, IHasProjectAndVersionViewModel
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

        private Dictionary<string, List<ScriptTranslation>> _scripts;
        public Dictionary<string, List<ScriptTranslation>> Scripts
        {
            get
            {
                return _scripts;
            }
            set
            {
                _scripts = value;
            }
        }

        private bool _isOutputing { get; set; }
        private bool _isOutputed { get; set; }

        public bool IsOutputing
        {
            get
            {
                return _isOutputing;
            }
            set
            {
                _isOutputing = value;
                NotifyOfPropertyChange(() => IsOutputing);
            }
        }

        public bool IsOutputed
        {
            get
            {
                return _isOutputed;
            }
            set
            {
                _isOutputed = value;
                NotifyOfPropertyChange(() => IsOutputed);
            }
        }

        public GenerateOutputViewModel()
        {
            Activated += OnActivate;
        }

        private void OnActivate(object sender, ActivationEventArgs e)
        {
            IsOutputing = false;
            IsOutputed = false;
        }

        public void OuputFiles()
        {            
            IsOutputing = true;
            IsOutputed = false;

            Task.Run(() =>
            {
                _version.GenerateOutput(_project);

                IsOutputed = true;
                IsOutputing = false;
            });
        }
    }
}
