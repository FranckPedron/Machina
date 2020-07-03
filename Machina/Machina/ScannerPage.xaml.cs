using Machina.model;
using Machina.service;
using Plugin.Media.Abstractions;
using Plugin.SimpleAudioPlayer;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Machina
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScannerPage : ContentPage
    {
        bool processing = true;
        FaceDetectResult faceDetectResult = null;
        SpeechOptions speechOptions = null;

        public ScannerPage(MediaFile file)
        {
            InitializeComponent();

            infoLayout.IsVisible = false;

            NavigationPage.SetHasNavigationBar(this, false);

            faceImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStreamWithImageRotatedForExternalStorage();
                return stream;
            });

            LaserAnimationWithSoundAndDisplayResults();
            StartDetection(file);
        }

        //laserImage

        private async Task LaserAnimationWithSoundAndDisplayResults()
        {
            laserImage.Opacity = 0;
            await Task.Delay(500);
            await laserImage.FadeTo(1, 500);

            PlaySound("scan.wav");
            await laserImage.TranslateTo(0, 360, 1800);
            double y = 0;
            while (processing)
            {
                PlayCurrentSound();
                await laserImage.TranslateTo(0, y, 1800);
                y = (y == 0) ? 360 : 0;
            }

            laserImage.IsVisible = false;
            PlaySound("result.wav");
            await DisplayResults();
            await ResultsSpeech();
        }

        private async Task StartDetection(MediaFile file)
        {
            faceDetectResult = await CognitiveService.FaceDetect(file.GetStreamWithImageRotatedForExternalStorage());
            //await Task.Delay(5000);
            processing = false;
        }

        private async Task DisplayResults()
        {
            statusLabel.Text = "Analyse terminée";

            if (faceDetectResult == null)
            {
                //faceLabel.Text = "Pas de détection";
                await DisplayAlert("Erreur", "L'analyse n'a pas fonctionné", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                // On a récupéré les infos du visage
                ageLabel.Text = faceDetectResult.faceAttributes.age.ToString();
                genderLabel.Text = faceDetectResult.faceAttributes.gender.Substring(0, 1).ToUpper();
                infoLayout.IsVisible = true;
                continueButton.Opacity = 1;
            }
        }

        private void ContinueButtonClicked(object sender, EventArgs eventArgs)
        {
            Navigation.PopAsync();
        }

        private void PlaySound(string soundName)
        {
            ISimpleAudioPlayer player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            player.Load(GetStreamFromFile(soundName));
            player.Play();
        }

        private void PlayCurrentSound()
        {
            ISimpleAudioPlayer player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            player.Stop();
            player.Play();
        }

        private Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;

            var names = assembly.GetManifestResourceNames();
            Console.WriteLine("RESOURCES NAMES : " + String.Join(", ", names));

            var stream = assembly.GetManifestResourceStream("Machina." + filename);
            return stream;
        }

        private async Task InitSpeak()
        {
            var locales = await TextToSpeech.GetLocalesAsync();

            // Grab the first locale
            var locale = locales.Where(o => o.Language.ToLower() == "fr").FirstOrDefault();

            speechOptions = new SpeechOptions()
            {
                Volume = .75f,
                Pitch = 0.1f,
                Locale = locale
            };
        }

        private async Task Speak(string text)
        {
            if (speechOptions == null)
            {
                await InitSpeak();
            }
            await TextToSpeech.SpeakAsync(text, speechOptions);
        }

        private async Task ResultsSpeech()
        {
            if (faceDetectResult == null)
            {
                await Speak("Humain non détecté");
            }
            else
            {
                await Speak("Humain détecté");
                if (faceDetectResult.faceAttributes.gender.ToLower() == "male")
                {
                    await Speak("Sexe masculin");
                }
                else
                {
                    await Speak("Sexe féminin");
                }
                await Speak("âge " + faceDetectResult.faceAttributes.age.ToString() + " ans");
            }
        }
    }
}