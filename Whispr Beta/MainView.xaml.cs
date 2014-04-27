using System.Windows;

namespace WhisprBeta
{
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            ShowLoadingScreen();
            LocalFeedView.MapButtonClicked += LocalFeedView_OnMapButtonClicked;
            MapView.FeedButtonClicked += MapView_OnFeedButtonClicked;
        }

        private void ShowLoadingScreen()
        {
            var appVersion = System.Xml.Linq.XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;
            LoadingScreen.VersionText = "Beta " + appVersion;
            LoadingScreen.StartAnimation();
        }

        private void LocalFeedView_OnMapButtonClicked()
        {
            MapView.Show();
        }

        private void MapView_OnFeedButtonClicked()
        {
            LocalFeedView.Show();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MapView.IsVisible)
            {
                MapView.Hide();
                LocalFeedView.Show();
                e.Cancel = true;
            }
        }
    }
}