using System;
using System.Windows;
using System.Windows.Threading;

namespace WhisprBeta
{
    public partial class LoadingScreenView
    {
        private bool fadingIn;
        private bool fadingOut;

        public string VersionText
        {
            set
            {
                XamlVersionText.Text = value;
            }
        }

        public LoadingScreenView()
        {
            InitializeComponent();
            // Set the initial opacity of loading screen elements to zero,
            // so that they could be faded in
            XamlVersionText.Opacity = 0;
            LogoImage.Opacity = 0;
        }

        public void StartAnimation()
        {
            // Init loading screen animation and hiding timer
            var animationStepTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 750) };
            animationStepTimer.Tick += animationStepTimer_Tick;
            animationStepTimer.Start();
        }

        private void animationStepTimer_Tick(object sender, EventArgs e)
        {
            if (fadingIn == false)
            {
                AnimLogoFadeIn.Begin();
                AnimVersionFadeIn.Begin();
                fadingIn = true;
                ((DispatcherTimer)sender).Interval = new TimeSpan(0, 0, 0, 3, 0);
            }
            else if (fadingOut == false)
            {
                // Begin loading screen logo fade out animation
                AnimLogoFadeOut.Begin();
                AnimVersionFadeOut.Begin();
                fadingOut = true;
                ((DispatcherTimer)sender).Interval = new TimeSpan(0, 0, 0, 2, 0);
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }

        }
    }
}
