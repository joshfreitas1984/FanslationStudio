using Caliburn.Micro;
//using CefSharp;
//using CefSharp.Wpf;
using FanslationStudio.UserExperience;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FanslationStudio.MigratedCatTool
{
    public class CaliburnBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container = new SimpleContainer();

        public CaliburnBootstrapper()
        {
            Initialize();

            //var settings = new CefSettings();
            ////settings.CefCommandLineArgs.Add("allow-no-sandbox-job", "1");
            //Cef.Initialize(settings);
        }

        protected override void Configure()
        {
            base.Configure();

            _container.Singleton<IEventAggregator, EventAggregator>();

            //Register base Caliburn stuff
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<CatView>();
            _container.Singleton<CatViewModel>();
            _container.Singleton<ShellView>();
            _container.Singleton<ShellViewModel>();
            _container.Singleton<ProjectView>();
            _container.Singleton<ProjectViewModel>();
            _container.Singleton<ManualTranslateView>();
            _container.Singleton<ManualTranslateViewModel>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
}
