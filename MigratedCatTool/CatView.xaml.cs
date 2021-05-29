using Caliburn.Micro;
using CefSharp;
using CefSharp.Wpf;
using FanslationStudio.MigratedCatTool.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
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

namespace FanslationStudio.MigratedCatTool
{
    /// <summary>
    /// Interaction logic for CatView.xaml
    /// </summary>
    public partial class CatView : Window, IHandle<RawLineCopiedEvent>
    {
        private string _lastValidTranslation;
        private IEventAggregator _eventAggregator;

        public CatView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            Translator1.ConsoleMessage += Translator1_ConsoleMessage;
            Translator1.LoadingStateChanged += Translator1_LoadingStateChanged;
        }

        private void Translator1_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            //Wait for the Page to finish loading
            if (e.IsLoading == false)
            {
                //Translator1.ExecuteScriptAsync("alert('All Resources Have Loaded');");
                EvaluateDeepLTranslation();
            }
        }

        private void Translator1_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Debug.WriteLine(e.Message.ToString());
        }

        public async void EvaluateDeepLTranslation()
        {
            var frame = Translator1.GetMainFrame();

            var task = frame.EvaluateScriptAsync(
                "(function() { " +
                "   return document.getElementsByClassName('lmt__textarea lmt__target_textarea lmt__textarea_base_style')[0].value; " +
                "})();", null);

            await task.ContinueWith(async t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    var result = response.Success && response.Result != null ? 
                        (response.Result.ToString() ?? "null") 
                        : response.Message;

                    if (!string.IsNullOrEmpty(result) || result != _lastValidTranslation)
                    {
                        await _eventAggregator.PublishOnUIThreadAsync(new DeeplTransEvent(result));
                        _lastValidTranslation = result;
                    }
                }                
            });
        }

        public Task HandleAsync(RawLineCopiedEvent message, CancellationToken cancellationToken)
        {
            string encoded = UrlEncoder.Create().Encode(message.RawLine);
            Translator1.Address = $"https://www.deepl.com/translator#zh/en/{encoded}";
            Translator2.Address = $"https://translate.google.com/#view=home&op=translate&sl=zh-CN&tl=en&text={encoded}";
            return Task.CompletedTask;
        }
    }
}
