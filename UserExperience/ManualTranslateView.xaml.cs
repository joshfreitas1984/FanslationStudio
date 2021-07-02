using Caliburn.Micro;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace FanslationStudio.UserExperience
{
    /// <summary>
    /// Interaction logic for ManualTranslateView.xaml
    /// </summary>
    public partial class ManualTranslateView : UserControl, IHandle<Events.RawLineCopiedEvent>
    {
        private string _lastValidTranslation;
        private IEventAggregator _eventAggregator;

        public ManualTranslateView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            DeepLTranslate.ConsoleMessage += DeepLTranslate_ConsoleMessage;
            DeepLTranslate.LoadingStateChanged += DeepLTranslate_LoadingStateChanged;
            GoogleTranslate.LoadingStateChanged += GoogleTranslate_LoadingStateChanged;
        }

        double PercentageToToZoomLevel(int percent)
        {
            return (percent - 100) / 25.0;
        }

        private void GoogleTranslate_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            GoogleTranslate.SetZoomLevel(PercentageToToZoomLevel(80));
        }

        public Task HandleAsync(Events.RawLineCopiedEvent message, CancellationToken cancellationToken)
        {
            string encoded = UrlEncoder.Create().Encode(message.RawLine);
            DeepLTranslate.Address = $"https://www.deepl.com/translator#zh/en/{encoded}";
            GoogleTranslate.Address = $"https://translate.google.com/#view=home&op=translate&sl=zh-CN&tl=en&text={encoded}";
            return Task.CompletedTask;
        }

        private void DeepLTranslate_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            DeepLTranslate.SetZoomLevel(PercentageToToZoomLevel(80));

            //Wait for the Page to finish loading
            if (e.IsLoading == false)
            {
                //Translator1.ExecuteScriptAsync("alert('All Resources Have Loaded');");
                EvaluateDeepLTranslation();
            }
        }

        private void DeepLTranslate_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Debug.WriteLine(e.Message.ToString());
        }

        public async void EvaluateDeepLTranslation()
        {
            var frame = DeepLTranslate.GetMainFrame();

            var task = frame.EvaluateScriptAsync(
                "(function() { " +
                "   return document.getElementsByClassName('lmt__textarea lmt__target_textarea lmt__textarea_base_style')[0].value; " +
                "})();");

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
                        await _eventAggregator.PublishOnUIThreadAsync(new Events.DeeplTransEvent(result));
                        _lastValidTranslation = result;
                    }
                }
            });
        }
    }
}
