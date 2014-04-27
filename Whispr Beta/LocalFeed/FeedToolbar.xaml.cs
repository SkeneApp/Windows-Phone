using System;
using System.Windows;
using System.Windows.Controls;

namespace WhisprBeta.LocalFeed
{
    public partial class FeedToolbar
    {
        public delegate void MapButtonClickedEventHandler();
        public delegate void PublishButtonClickedEventHandler(string messageText);
        public delegate void PublishDelayChangedEventHandler(TimeSpan delay);

        public event MapButtonClickedEventHandler MapButtonClicked;
        public event PublishButtonClickedEventHandler PublishButtonClicked;
        public event PublishDelayChangedEventHandler PublishDelayChanged;

        private bool placeholderTextVisible = false;
        private const string PlaceholderText = "Tap to whispr";
        private const int MaximumWhisprCharacters = 160;
        private bool newMessagePanelVisible = false;

        public string MessageText
        {
            get
            {
                if (placeholderTextVisible)
                {
                    return string.Empty;
                }
                return textBoxInput.Text;
            }
            set { textBoxInput.Text = value; }
        }

        public TimeSpan PublishDelay
        {
            get { return TimeSpan.FromMinutes(DelaySliderControl.Value); }
            set { DelaySliderControl.Value = value.Minutes; }
        }

        public FeedToolbar()
        {
            InitializeComponent();
            textBoxInput.MaxLength = MaximumWhisprCharacters;
            DelaySliderControl.ValueChanged += DelaySliderControl_ValueChanged;
            ShowPlaceholderText();
        }

        public void ShowPlaceholderText()
        {
            MessageText = PlaceholderText;
            placeholderTextVisible = true;
        }

        public void HidePlaceholderText()
        {
            MessageText = "";
            placeholderTextVisible = false;
        }

        public void ShowNewMessagePanel()
        {
            if (newMessagePanelVisible) return;

            gridPublishTools.Visibility = Visibility.Visible;
            DelaySliderControl.Visibility = Visibility.Visible;
            //Grid.SetRowSpan(scrollViewer, 1);
            Grid.SetColumnSpan(textBoxInput, 1);
            newMessagePanelVisible = true;
        }

        public void HideNewMessagePanel()
        {
            if (!newMessagePanelVisible) return;

            if (MessageText.Trim() == "")
            {
                ShowPlaceholderText();
            }
            gridPublishTools.Visibility = Visibility.Collapsed;
            DelaySliderControl.Visibility = Visibility.Collapsed;
            //Grid.SetRowSpan(scrollViewer, 2);
            Grid.SetColumnSpan(textBoxInput, 2);
            newMessagePanelVisible = false;
        }

        private void buttonPublish_Click(object sender, RoutedEventArgs e)
        {
            if (placeholderTextVisible || MessageText.Length <= 0) return;

            if (PublishButtonClicked != null)
            {
                PublishButtonClicked(MessageText);
            }
            ShowPlaceholderText();
        }

        private void buttonMap_Click(object sender, RoutedEventArgs e)
        {
            if (MapButtonClicked != null)
            {
                MapButtonClicked();
            }
        }

        private void textBoxInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (placeholderTextVisible)
            {
                HidePlaceholderText();
            }
            ShowNewMessagePanel();
        }

        private void textBoxInput_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (textBoxInput.RenderSize.Height > 100)
            {
                textBlockNumChars.Visibility = Visibility.Visible;
            }
            else
            {
                textBlockNumChars.Visibility = Visibility.Collapsed;
            }
        }

        private void textBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetNumCharsText(((TextBox)sender).Text.Length);
        }

        private void DelaySliderControl_ValueChanged(int value)
        {
            if (PublishDelayChanged != null)
            {
                PublishDelayChanged(PublishDelay);
            }
        }

        public void SetNumCharsText(int charactersEntered)
        {
            textBlockNumChars.Text = charactersEntered + "/" + MaximumWhisprCharacters;
        }
    }
}
