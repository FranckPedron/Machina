using Machina.model;
using Machina.service;
using Plugin.Media.Abstractions;
using Plugin.SimpleAudioPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Machina
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScannerPage : ContentPage
    {
        bool processing = true;
        FaceDetectResult faceDetectResult = null;

        public ScannerPage(MediaFile file)
        {
            InitializeComponent();
            
            NavigationPage.SetHasNavigationBar(this, false);

            faceImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStreamWithImageRotatedForExternalStorage() ;
                return stream;
            });
            StartLaserAnimation();
            StartDetection(file);

        }

        private async Task StartLaserAnimation()
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
                y = (Y == 0) ? 360 : 0;
            }

            laserImage.IsVisible = false;
            PlaySound("result.wav");
            await DisplayResults();
            }

        private async Task StartDetection(MediaFile file)
        {
            faceDetectResult = await CognitiveService.FaceDetect(file.GetStreamWithImageRotatedForExternalStorage());
            processing = false;
            
        }
        
        private async Task DisplayResults()
        {
            statusLabel.Text = "Analyse terminée";

            if (faceDetectResult == null)
            {
                await DisplayAlert("Erreur", "L'analyse n'a pas fonctionné", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                ageLabel.Text = faceDetectResult.faceAttributes.age.ToString();
                genderLabel.Text = faceDetectResult.faceAttributes.gender.Substring(0, 1).ToUpper();
                infoLayout.IsVisible = true;
                continueButton.Opacity = 1;
            }

        }

        private void ContinueButtonClicked(object sender, EventArgs e)
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
    }
}