using Caliburn.Micro;
using FanslationStudio.Domain;
using FanslationStudio.Domain.ScriptToTranslate;
using FanslationStudio.Services;
using FanslationStudio.UserExperience.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FanslationStudio.UserExperience
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ShellView : Window, IHandle<SelectTabForViewEvent>
    {
        public IEventAggregator _eventAggregator;

        public ShellView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnUIThread(this);
        }

        public Task HandleAsync(SelectTabForViewEvent message, CancellationToken cancellationToken)
        {
            //Handling when we use VM to navigate
            foreach(var tab in ShellTabs.Items.Cast<TabItem>())
            {
                if (tab.IsSelected == false && tab.Name == message.TabName)
                    tab.IsSelected = true;
            }

            return Task.CompletedTask;
        }
    }
}
