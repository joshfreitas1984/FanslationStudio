using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Text;

namespace FanslationStudio.Domain
{
    public class ScriptSearchResult : PropertyChangedBase
    {
        private string _sourcePath;
        private ScriptTranslation _script;
        private ScriptTranslationItem _item;
        private string _find;
        private string _replace;

        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                NotifyOfPropertyChange(() => SourcePath);
            }
        }

        public ScriptTranslation Script
        {
            get => _script; set
            {
                _script = value;
                NotifyOfPropertyChange(() => SourcePath);
            }
        }
        public ScriptTranslationItem Item
        {
            get => _item; set
            {
                _item = value;
                NotifyOfPropertyChange(() => SourcePath);
            }
        }
        public string Find
        {
            get => _find; set
            {
                _find = value;
                NotifyOfPropertyChange(() => SourcePath);
            }
        }
        public string Replace
        {
            get => _replace; set
            {
                _replace = value;
                NotifyOfPropertyChange(() => SourcePath);
            }
        }
    }
}
