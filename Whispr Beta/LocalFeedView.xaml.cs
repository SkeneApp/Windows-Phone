using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using WhisprBeta.Common;

namespace WhisprBeta
{
    public partial class LocalFeedView
    {
        public delegate void MapButtonClickedEventHandler();
        public event MapButtonClickedEventHandler MapButtonClicked;
        private readonly List<Message> unsentPosts;

        public LocalFeedView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            unsentPosts = new List<Message>();
            PendingMessages.Initialize();
            LocalFeed.Initialize();
            RadiusSliderControl.Value = App.Location.FeedRadius;
            RadiusSliderControl.ValueChanged += RadiusSliderControl_ValueChanged;
            Toolbar.MapButtonClicked += Toolbar_MapButtonClicked;
            Toolbar.PublishButtonClicked += Toolbar_PublishButtonClicked;
          //  Toolbar.PublishDelayChanged += Toolbar_PublishDelayChanged;
            LocalFeed.Tap += LocalFeed_Tap;
        }

        public void Show()
        {
            OnNavigatedTo();
        }

        public void Hide()
        {
            OnNavigatedFrom();
        }

        private void OnNavigatedTo()
        {
            App.Backend.LocalWhisprsUpdateEnabled = true;
            App.Backend.LocalWhisprsUpdated += Backend_LocalWhisprsUpdated;
            App.Location.RadiusChanged += Location_RadiusChanged;
            App.Location.UserLocationChanged += Location_UserLocationChanged;
       //     Toolbar.PublishDelay = App.Backend.PublishDelay;
        }
        private void OnNavigatedFrom()
        {
            App.Backend.LocalWhisprsUpdated -= Backend_LocalWhisprsUpdated;
            App.Location.RadiusChanged -= Location_RadiusChanged;
            App.Location.UserLocationChanged -= Location_UserLocationChanged;
            App.Backend.LocalWhisprsUpdateEnabled = false;
        }

        private void CheckIfAnyMessages()
        {
            if (LocalFeed.Messages.Count == 0)
            {
                StatusOverlay.Show("No Whisprs in this area!");
            }
            else
            {
                StatusOverlay.Hide();
            }
        }

        private void LocalFeed_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Toolbar.HideNewMessagePanel();
        }

        private void Toolbar_PublishDelayChanged(TimeSpan delay)
        {
            App.Backend.PublishDelay = delay;
        }

        private void Toolbar_PublishButtonClicked(string messageText)
        {
            Message newMessage = new Message(App.Location.UserLocation, messageText, (int)App.Backend.PublishDelay.TotalSeconds);
            if (App.Location.UserLocation != null)
            {
                App.Backend.PostWhispr(newMessage);
            }
            else
            {
                unsentPosts.Add(newMessage);
            }
            if (App.Backend.PublishDelay.Minutes == 0)
            {
                // Add message to immediate list
                LocalFeed.ImmediateListAdd(newMessage);
            }
            else
            {
                // Add message to pending list
                PendingMessages.Add(newMessage);
            }
        }

        private void Toolbar_MapButtonClicked()
        {
            if (MapButtonClicked != null)
            {
                MapButtonClicked();
            }
            Hide();
        }

        private void Location_RadiusChanged()
        {
            App.Backend.GetLocalWhisprs();
        }

        private void Backend_LocalWhisprsUpdated(List<Message> localWhisprs)
        {
            LocalFeed.UpdateFeed(localWhisprs);
            CheckIfAnyMessages();
        }

        private void Location_UserLocationChanged()
        {
            // Get whisprs from this new location
            App.Backend.GetLocalWhisprs();

            // Check if there are any posts that were not sent because location was unknown
            if (unsentPosts.Count > 0) {
                // There are unsent posts. Set their location and send them
                foreach (Message post in unsentPosts) {
                    post.latitude = App.Location.UserLocation.Latitude;
                    post.longitude = App.Location.UserLocation.Longitude;
                    App.Backend.PostWhispr(post);
                }
                unsentPosts.Clear();
            }
        }

        private void Status_StatusChanged()
        {
           
            if (App.Status.StatusClear) {
                StatusOverlay.Hide();
            } else {
                StatusOverlay.Show(App.Status.CurrentStatus);
            }
        }

        private void StatusOverlay_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Toolbar.HideNewMessagePanel();
        }

        private void RadiusSliderControl_ValueChanged(int value)
        {
            LocalFeed.OutdateFeed();
            App.Location.FeedRadius = value;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            App.Status.StatusChanged += Status_StatusChanged;
            if (App.Status.StatusClear)
            {
                StatusOverlay.Hide();
            }
            else
            {
                StatusOverlay.Show(App.Status.CurrentStatus);
            }
            OnNavigatedTo();
        }

        private void roomList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            roomList.Visibility = System.Windows.Visibility.Collapsed;
            lockIcon.Visibility = System.Windows.Visibility.Collapsed;
            refreshIcon.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void localRoom_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            roomList.Visibility = System.Windows.Visibility.Visible;
            lockIcon.Visibility = System.Windows.Visibility.Visible;
            refreshIcon.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
