using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using WhisprBeta.Common;

namespace WhisprBeta.LocalFeed
{
    public partial class PendingMessagesControl
    {
        public PendingMessagesControl()
        {
            InitializeComponent();
        }

        private DispatcherTimer timer;

        public List<Message> List { get; set; }

        public void Initialize()
        {
            List = new List<Message>();
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 5) };
            timer.Tick += TimerTick;
            UpdateStatus();
        }

        public void Add(Message message)
        {
            if (message.PublishTime == 0)
            {
                message.PublishTime = Utils.UnixTimeNow() + message.PublishDelaySec;
            }
            List.Add(message);
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
            UpdateStatus();
        }

        public void Check()
        {
            lock (List)
            {
                long timeNow = Utils.UnixTimeNow();
                List<Message> postsToRemove = new List<Message>();
                foreach (Message pendingPost in List)
                {
                    // If message should have been posted already, remove it from the pending list.
                    // Give 3 sec extra, just because there is always some deley of sending message to server.
                    if (timeNow > (pendingPost.PublishTime + 3))
                    {
                        // Post's time has passed, remove it from the pending list
                        postsToRemove.Add(pendingPost);
                    }
                    else
                    {
                        // Post's time didn't pass yet
                    }
                }
                foreach (Message postToRemove in postsToRemove)
                {
                    List.Remove(postToRemove);
                }
                // If there are now no more messages pending, disable pending timer
                if (List.Count == 0)
                {
                    timer.Stop();
                }
                // Update status if anything has changed
                if (postsToRemove.Count > 0)
                {
                    UpdateStatus();
                }
            }
        }

        private void UpdateStatus()
        {
            string pendingMessage = string.Empty;
            bool statusVisible = false;
            if (List.Count > 0)
            {
                // There is something on the list - show status
                statusVisible = true;
                // Generate message in singular or plural
                if (List.Count == 1)
                {
                    pendingMessage = "1 whispr pending";
                }
                else
                {
                    pendingMessage = List.Count + " whisprs pending";
                }
            }
            else
            {
                // Nothing on the list - status will be hidden
                statusVisible = false;
                pendingMessage = string.Empty;
            }
            // Set the status
            SetStatus(pendingMessage, statusVisible);
        }

        private void SetStatus(string message, bool isVisible)
        {
            TextBlockMessage.Text = message;
            if (isVisible)
            {
                Border.Visibility = Visibility.Visible;
                TextBlockMessage.Visibility = Visibility.Visible;
            }
            else
            {
                Border.Visibility = Visibility.Collapsed;
                TextBlockMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Check();
        }
    }
}
