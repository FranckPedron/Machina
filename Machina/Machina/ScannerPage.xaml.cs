using Machina.model;
using Machina.service;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
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
            startLaserAnimation();
            startDetection(file);

        }

        private async Task startLaserAnimation()
            {
            laserImage.Opacity = 0;
            await Task.Delay(500);
            await laserImage.FadeTo(1, 500);
               
            await laserImage.TranslateTo(0, 360, 1800);
            double y = 0;

            while (processing)
            {
                await laserImage.TranslateTo(0, y, 1800);
                y = (Y == 0) ? 360 : 0;
            }

            laserImage.IsVisible = false;
            await DisplayResults();
            }

        private async Task startDetection(MediaFile file)
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
    }
}