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
        public ScannerPage(MediaFile file)
        {
            InitializeComponent();
            
            NavigationPage.SetHasNavigationBar(this, false);

            faceImage.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStreamWithImageRotatedForExternalStorage() ;
                return stream;
            });

            startDetection(file);

        }

       
        private async Task startDetection(MediaFile file)
        {
            var faceDetectResult = await CognitiveService.FaceDetect(file.GetStreamWithImageRotatedForExternalStorage());
            
            statusLabel.Text = "Analyse terminée";

            if (faceDetectResult == null)
            {
                await DisplayAlert("Erreur","L'analyse n'a pas fonctionné","OK");
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