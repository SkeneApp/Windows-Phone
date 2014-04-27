using System.Windows;

namespace WhisprBeta.Common
{
    public partial class StatusOverlayControl
    {
        public StatusOverlayControl()
        {
            InitializeComponent();
        }

        public void Show(string text)
        {
            MessageTextBlock.Text = text;
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
            MessageTextBlock.Text = string.Empty;
        }
    }
}
