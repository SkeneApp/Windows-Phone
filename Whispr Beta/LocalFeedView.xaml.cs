using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WhisprBeta.Common;
using WhisprBeta.Services;

namespace WhisprBeta
{
    public partial class LocalFeedView
    {
        public delegate void MapButtonClickedEventHandler();
        public event MapButtonClickedEventHandler MapButtonClicked;
        private readonly IMessageService messageService;
        private readonly ILocationService locationService;
        private readonly DispatcherTimer messageUpdateTimer;

        public LocalFeedView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            messageService = App.MessageService;
            locationService = App.LocationService;
            PendingMessages.Initialize();
            LocalFeed.Initialize();
            //TODO: Save UI values somewhere else // RadiusSliderControl.Value = locationService.FeedRadius;
            RadiusSliderControl.ValueChanged += RadiusSliderControl_ValueChanged;
            Toolbar.MapButtonClicked += Toolbar_MapButtonClicked;
            Toolbar.PublishButtonClicked += Toolbar_PublishButtonClicked;
            LocalFeed.Tap += LocalFeed_Tap;
            messageUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            messageUpdateTimer.Tick += (sender, e) => UpdateLocalMessages();
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
            locationService.UserLocationChanged += Location_UserLocationChanged;
            // TODO: Save publish delay somewhere else //Toolbar.PublishDelay = App.Backend.PublishDelay;
            if (locationService.UserLocation != null)
            {
                 UpdateLocalMessages();
            }
            messageUpdateTimer.Start();
        }
        private void OnNavigatedFrom()
        {
            locationService.UserLocationChanged -= Location_UserLocationChanged;
            messageUpdateTimer.Stop();
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

        private async void Toolbar_PublishButtonClicked(string messageText)
        {
            var newMessage = new Message
            {
                Latitude = locationService.UserLocation.Latitude,
                Longitude = locationService.UserLocation.Longitude,
                Text = messageText,
                PublishDelaySec = (int)Toolbar.PublishDelay.TotalSeconds
            };
            string returnedId = await messageService.Post(newMessage);
            if (Toolbar.PublishDelay.Minutes == 0)
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

        private async void UpdateLocalMessages()
        {
            if (locationService.UserLocation == null) return;
            var localWhisprs = (await messageService.Get(locationService.UserLocation, RadiusSliderControl.Value)).ToList();
            LocalFeed.UpdateFeed(localWhisprs);
            CheckIfAnyMessages();
        }

        private void Location_UserLocationChanged()
        {
            // Get whisprs from this new location
            UpdateLocalMessages();
            Toolbar.PostingEnabled = true;
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
            UpdateLocalMessages();
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
    }
}
