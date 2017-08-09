using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechSynthesis;
using Windows.Media.SpeechRecognition;
using System.Collections.ObjectModel;

namespace EnglishLocalizer
{
    /// <summary>
    /// Only one page for this application.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<SpeechRecognitionResult> results = new ObservableCollection<SpeechRecognitionResult>();
        public MainPage()
        {
            this.InitializeComponent();
            initVoices();
        }

        private void initVoices()
        {
            var nonEnglishVoices = SpeechSynthesizer.AllVoices.Where(v => !v.Language.Contains("en-")).OrderBy(v => v.Language);
            if (nonEnglishVoices.Count() > 0)
            {
                voices.ItemsSource = nonEnglishVoices;
                voices.SelectedItem = nonEnglishVoices.First(v => v.Id.Equals(SpeechSynthesizer.DefaultVoice.Id));
            }
            else
            {
                openLanguageSettings(); // TODO: should show dialog
            }
        }

        private async void readText(string text)
        {
            var synthesizer = new SpeechSynthesizer();
            synthesizer.Voice = (VoiceInformation)voices.SelectedItem;
            var ssml = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{0}'><prosody rate='{1}'>{2}</prosody></speak>";
            var lang = synthesizer.Voice.Language;
            var rate = ((ComboBoxItem) speed.SelectedItem).Content;
            var content = lang == "ja-JP " ? text.Replace(" a ", " ア ").Replace(" am ", " アム ") : text;
            content = wordByWord.IsOn ? content.Replace(" ", "<break strength='weak' />") : content;
            var stream = await synthesizer.SynthesizeSsmlToStreamAsync(string.Format(ssml, lang, rate, content));
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        private async void openLanguageSettings()
        {
            bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:regionlanguage"));
        }

        private async void listenButton_Click(object sender, RoutedEventArgs e)
        {
            var language = new Windows.Globalization.Language("en-US");
            var recognizer = new SpeechRecognizer(language);
            await recognizer.CompileConstraintsAsync();
            recognizer.UIOptions.IsReadBackEnabled = false;
            recognizer.UIOptions.ShowConfirmation = false;
            recognizer.UIOptions.AudiblePrompt = "Say something";
            SpeechRecognitionResult result = await recognizer.RecognizeWithUIAsync();
            //SpeechRecognitionResult result = await recognizer.RecognizeAsync();
            if(result.Status == SpeechRecognitionResultStatus.Success && result.Text.Length > 0)
            {
                results.Add(result);
                readText(result.Text);
            }
        }

        private void resultList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var result = (SpeechRecognitionResult) e.ClickedItem;
            readText(result.Text);
        }

        private void openSettings_Click(object sender, RoutedEventArgs e)
        {
            openLanguageSettings();
        }
    }
}
