using System;
using System.Windows.Media;

namespace WhisprBeta.Common
{
    public partial class MessageControl
    {
        private readonly Color normalTextColor = Colors.White;
        private readonly Color normalDateColor = Color.FromArgb(255, 93, 93, 93);
        private readonly Color normalBackgroundColor = Color.FromArgb(255, 21, 21, 21);
        private readonly Color normalBorderColor = Color.FromArgb(255, 100, 100, 100);

        private readonly Color immediateTextColor = Colors.White;
        private readonly Color immediateDateColor = Color.FromArgb(255, 93, 93, 93);
        private readonly Color immediateBackgroundColor = Color.FromArgb(255, 21, 21, 21);
        private readonly Color immediateBorderColor = Color.FromArgb(255, 100, 100, 100);

        private readonly Color outdatedTextColor = Colors.Gray;
        private readonly Color outdatedDateColor = Color.FromArgb(255, 93, 93, 93);
        private readonly Color outdatedBackgroundColor = Color.FromArgb(255, 10, 10, 10);
        private readonly Color outdatedBorderColor = Color.FromArgb(255, 50, 50, 50);

        public string Text
        {
            get
            {
                return textBlockMessage.Text;
            }
            set
            {
                textBlockMessage.Text = value;
                Show();
            }
        }
        private DateTime publishDate;
        public DateTime PublishDate
        {
            get
            {
                return publishDate;
            }
            set
            {
                if (publishDate != value) {
                    publishDate = value;
                    textBlockDate.Text = Utils.ToTimeSinceString(value);
                    unixTimeStamp = Utils.DateTimeToUnixTimestamp(value);
                    Show();
                }
            }
        }
        private long unixTimeStamp;
        public long UnixTimeStamp
        {
            get
            {
                return unixTimeStamp;
            }
            set
            {
                if (unixTimeStamp != value) {
                    unixTimeStamp = value;
                    publishDate = Utils.UnixTimeStampToDateTime(unixTimeStamp);
                    Show();
                }
            }
        }
        public ulong Id { get; set; }

        public bool IsImmediate
        {
            set
            {
                if (value) {
                    textBlockMessage.Foreground = new SolidColorBrush(immediateTextColor);
                    textBlockDate.Foreground = new SolidColorBrush(immediateDateColor);
                    borderBottom.Background = new SolidColorBrush(immediateBackgroundColor);
                    borderBottom.BorderBrush = new SolidColorBrush(immediateBorderColor);
                    textBlockDate.Text = "Right now";
                } else {
                    textBlockMessage.Foreground = new SolidColorBrush(normalTextColor);
                    textBlockDate.Foreground = new SolidColorBrush(normalDateColor);
                    borderBottom.Background = new SolidColorBrush(normalBackgroundColor);
                    borderBottom.BorderBrush = new SolidColorBrush(normalBorderColor);
                }
            }
        }

        public bool IsOutdated
        {
            set
            {
                if (value) {
                    textBlockMessage.Foreground = new SolidColorBrush(outdatedTextColor);
                    textBlockDate.Foreground = new SolidColorBrush(outdatedDateColor);
                    borderBottom.Background = new SolidColorBrush(outdatedBackgroundColor);
                    borderBottom.BorderBrush = new SolidColorBrush(outdatedBorderColor);
                } else {
                    textBlockMessage.Foreground = new SolidColorBrush(normalTextColor);
                    textBlockDate.Foreground = new SolidColorBrush(normalDateColor);
                    borderBottom.Background = new SolidColorBrush(normalBackgroundColor);
                    borderBottom.BorderBrush = new SolidColorBrush(normalBorderColor);
                }
            }
        }

        private bool isHidden;

        public MessageControl()
        {
            isHidden = false;
            InitializeComponent();
            Hide();
        }
        public MessageControl(Message message)
        {
            isHidden = false;
            InitializeComponent();
            Text = message.Text;
            PublishDate = Utils.UnixTimeStampToDateTime(message.PublishTime);
            Id = message.Id;
        }

        public void Hide()
        {
            if (!isHidden) {
                textBlockMessage.Visibility = System.Windows.Visibility.Collapsed;
                textBlockDate.Visibility = System.Windows.Visibility.Collapsed;
                borderBottom.Visibility = System.Windows.Visibility.Collapsed;
                isHidden = true;
            }
        }

        public void Show()
        {
            if (isHidden) {
                textBlockMessage.Visibility = System.Windows.Visibility.Visible;
                textBlockDate.Visibility = System.Windows.Visibility.Visible;
                borderBottom.Visibility = System.Windows.Visibility.Visible;
                isHidden = false;
            }
        }

        public void UpdatePublishTimeText()
        {
            textBlockDate.Text = Utils.ToTimeSinceString(PublishDate);
        }
    }
}
