//Colin O'Dwanny
using System;

namespace ground_and_go.Pages.Workout
{
    public partial class VideoPlayer : ContentPage
    {
        public VideoPlayer()
        {
            InitializeComponent();
        }


        //Stops video when you press the back button
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            VideoWebView.Source = "about:blank";

        }
    }
}