using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WhisprBeta.Services;

namespace WhisprBeta
{
    public partial class MapView
    {
        public delegate void FeedButtonClickedEventHandler();

        public event FeedButtonClickedEventHandler FeedButtonClicked;
        private readonly MessageService messageService;
        private readonly DispatcherTimer messageUpdateTimer;
        private readonly DispatcherTimer mapDataUpdateTimer;

        public bool IsVisible { get; set; }

        private double totalMapHeight;
        private Storyboard storyboard;
        private DoubleAnimation animation;
        public MapView()
        {
            IsVisible = false;
            InitializeComponent();
            LayoutRootTransform.X = 480;
            RemoteFeed.Initialize();
            InitAnimation();
            messageService = App.MessageService;
            RadiusSliderControl.ValueChanged += RadiusSliderControl_OnValueChanged;
            RadiusSliderControl.InstantChangeThreshold = 10.0;
            RadiusSliderControl.InstantValueChanged += RadiusSliderControl_InstantValueChanged;
            Toolbar.FeedButtonClicked += Toolbar_FeedButtonClicked;
            Toolbar.MyLocationButtonClicked += Toolbar_MyLocationButtonClicked;
            messageUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            mapDataUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            messageUpdateTimer.Tick += (sender, e) => UpdateRemoteMessages();
            mapDataUpdateTimer.Tick += (sender, e) => UpdateMapData();
        }

        public void Show()
        {
            LayoutRootTransform.X = 0;
            OnNavigatedTo();
            if (App.Location.UserLocation != null)
            {
                Map.DrawUserLocation(GetMapVisibleHeight());
            }
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
            OnNavigatedFrom();
            LayoutRootTransform.X = 480;
        }

        private void OnNavigatedTo()
        {
            App.Location.UserLocationChanged += Location_UserLocationChanged;
            App.Location.FeedLocationChanged += Location_FeedLocationChanged;
            UpdateRemoteMessages();
            messageUpdateTimer.Start();
            mapDataUpdateTimer.Start();
        }

        private void OnNavigatedFrom()
        {
            messageUpdateTimer.Stop();
            mapDataUpdateTimer.Stop();
            App.Location.UserLocationChanged -= Location_UserLocationChanged;
            App.Location.FeedLocationChanged -= Location_FeedLocationChanged;
        }

        /// <summary>
        /// Gets the height of the visible part of the map
        /// </summary>
        /// <returns></returns>
        private double GetMapVisibleHeight()
        {
            double visibleMapHeight = totalMapHeight - LayoutTransform.Y;
            return visibleMapHeight;
        }

        /// <summary>
        /// Runs initialization that can only be done after the page layout
        /// has been calcualted. Don't call this before the page layout has
        /// loaded.
        /// Best place to call this is on Page.Loaded event.
        /// </summary>
        private void InitLayout()
        {
            totalMapHeight = ContentGrid.ActualHeight - Grip.ActualHeight;
            LayoutTransform.Y = totalMapHeight/2;
            RadiusSliderControl.Value = App.Location.FeedRadius;
            Map.InitializeMapGraphics();
            Map.LoadMapData(GetMapVisibleHeight());
        }

        private void InitAnimation()
        {
            storyboard = new Storyboard();
            animation = new DoubleAnimation();
            Storyboard.SetTarget(animation, LayoutTransform);
            Storyboard.SetTargetProperty(animation, new PropertyPath(TranslateTransform.YProperty));
            storyboard.Children.Add(animation);
        }

        /// <summary>
        /// Stops the automatic map sliding when user touches the slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (storyboard != null)
            {
                storyboard.Pause();
            }
        }

        /// <summary>
        /// Updates map position on the screen as user drags the slider up or down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grip_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var nextY = LayoutTransform.Y + e.DeltaManipulation.Translation.Y;
            if (nextY >= 0 && nextY <= totalMapHeight)
            {
                LayoutTransform.Y += e.DeltaManipulation.Translation.Y;
            }
        }

        /// <summary>
        /// Starts automatic sliding animation after user has
        /// lifted finger off the screen after dragging slider up or down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grip_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var velocityDiPsMs = e.FinalVelocities.LinearVelocity.Y;
            // The velocityThreshold constant sets how fast the slider has to be dragged for automatic sliding to begin.
            const int velocityThreshold = 200;
            if (velocityDiPsMs < -velocityThreshold || velocityDiPsMs > velocityThreshold)
            {
                // Scale velocity
                var velocityPxMs = Math.Abs(velocityDiPsMs) / 1000;
                double durationMs;
                double destination;
                if (velocityDiPsMs > 0)
                {
                    // Sliding down
                    var distanceLeft = totalMapHeight - LayoutTransform.Y;
                    durationMs = Math.Abs(distanceLeft) / velocityPxMs;
                    destination = totalMapHeight;
                }
                else
                {
                    // Sliding up
                    var distanceLeft = LayoutTransform.Y;
                    durationMs = Math.Abs(distanceLeft) / velocityPxMs;
                    destination = 0;
                }
                animation.Duration = TimeSpan.FromMilliseconds(durationMs);
                animation.From = LayoutTransform.Y;
                animation.To = destination;
                storyboard.Begin();
            }
        }

        private void Location_FeedLocationChanged()
        {
            Map.DrawFeedLocation();
            Map.DrawFeedRadius();
            UpdateRemoteMessages();
            if (!IsVisible)
            {
                // No longer follow feed location changes if this is hidden.
                // This is probably the first time the location was aquired.
                App.Location.UserLocationChanged -= Location_UserLocationChanged;
            }
        }

        private async void UpdateRemoteMessages()
        {
            if (App.Location.FeedLocation == null) return;
            var whisprs = (await messageService.Get(App.Location.FeedLocation, RadiusSliderControl.Value)).ToList();
            RemoteFeed.UpdateFeed(whisprs);
        }

        private async void UpdateMapData()
        {
            var whisprs = (await messageService.GetMapData()).ToList();
            Map.DrawLatestWhisprs(whisprs);
        }

        private void Location_UserLocationChanged()
        {
            Map.DrawUserLocation(GetMapVisibleHeight());
            if (!IsVisible)
            {
                // No longer follow user location changes if this is hidden.
                // This is probably the first time the location was aquired.
                App.Location.UserLocationChanged -= Location_UserLocationChanged;
            }
        }

        private void Toolbar_MyLocationButtonClicked()
        {
            Map.GoToUserLocation(GetMapVisibleHeight());
            if (App.Location.UserLocation != null)
            {
                App.Location.FeedLocation = App.Location.UserLocation;
            }
        }

        private void Toolbar_FeedButtonClicked()
        {
            if (FeedButtonClicked != null)
            {
                FeedButtonClicked();
            }
            Hide();
        }

        private void RadiusSliderControl_OnValueChanged(int value)
        {
            RemoteFeed.OutdateFeed();
            UpdateRemoteMessages();
        }

        private void RadiusSliderControl_InstantValueChanged(int value)
        {
            Map.DrawFeedRadius(value);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitLayout();
            // Enabled some service events just to get initial data.
            // These should be disabled later if map view is not visible.
            App.Location.UserLocationChanged += Location_UserLocationChanged;
            App.Location.FeedLocationChanged += Location_FeedLocationChanged;
            UpdateRemoteMessages();
            UpdateMapData();
        }
    }
}
