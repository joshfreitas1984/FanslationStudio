using Caliburn.Micro;
using CefSharp;
using FanslationStudio.UserExperience.Events;
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
    public partial class ManualTranslateView : UserControl, IHandle<RawLineCopiedEvent>, IHandle<RequestDeeplResult>, IHandle<MoveToNextGridItemEvent>
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
            GoogleTranslate.SetZoomLevel(PercentageToToZoomLevel(50));
        }

        private async void DeepLTranslate_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            DeepLTranslate.SetZoomLevel(PercentageToToZoomLevel(50));

            //Wait for the Page to finish loading
            if (e.IsLoading == false)
            {
                //Translator1.ExecuteScriptAsync("alert('All Resources Have Loaded');");
                await EvaluateDeepLTranslation();
            }
        }

        private void DeepLTranslate_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Debug.WriteLine(e.Message.ToString());
        }

        public async Task EvaluateDeepLTranslation()
        {
            var frame = DeepLTranslate.GetMainFrame();

            var task = frame.EvaluateScriptAsync(
                "(function() { return document.getElementsByClassName('lmt__textarea lmt__target_textarea lmt__textarea_base_style')[0].value; })();", null);

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

            await Task.CompletedTask;
        }

        public Task HandleAsync(RawLineCopiedEvent message, CancellationToken cancellationToken)
        {
            string encoded = UrlEncoder.Create().Encode(message.RawLine ?? string.Empty);
            string sourceLanguageCode = message.SourceLanguageCode; //Might need to map these in future
            string targetLanguageCode = message.TargetLanguageCode;

            DeepLTranslate.Address = $"https://www.deepl.com/translator#{sourceLanguageCode}/{targetLanguageCode}/{encoded}";
            GoogleTranslate.Address = $"https://translate.google.com/#view=home&op=translate&sl={sourceLanguageCode}&tl={targetLanguageCode}&text={encoded}";
            return Task.CompletedTask;
        }

        public async Task HandleAsync(RequestDeeplResult message, CancellationToken cancellationToken)
        {
            //Try now and hit the browser
            await EvaluateDeepLTranslation();
        }

        public Task HandleAsync(MoveToNextGridItemEvent message, CancellationToken cancellationToken)
        {
            //Move to next item
            if (SearchGrid.SelectedIndex < SearchGrid.Items.Count - 1)
            {
                SearchGrid.SelectedIndex++;
                SearchGrid.UpdateLayout();
                SearchGrid.ScrollIntoView(SearchGrid.SelectedItem);
            }

            return Task.CompletedTask;
        }
    }
}
