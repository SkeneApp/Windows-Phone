using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WhisprBeta.Common;

namespace WhisprBeta
{
    public partial class MapView
    {
        public delegate void FeedButtonClickedEventHandler();

        public event FeedButtonClickedEventHandler FeedButtonClicked;

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
            RadiusSliderControl.ValueChanged += RadiusSliderControl_OnValueChanged;
            RadiusSliderControl.InstantChangeThreshold = 10.0;
            RadiusSliderControl.InstantValueChanged += RadiusSliderControl_InstantValueChanged;
            Toolbar.FeedButtonClicked += Toolbar_FeedButtonClicked;
            Toolbar.MyLocationButtonClicked += Toolbar_MyLocationButtonClicked;
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
            App.Backend.LatestWhisprsUpdateEnabled = true;
            App.Backend.OtherWhisprsUpdateEnabled = true;
            App.Backend.LatestWhisprsUpdated += Backend_LatestWhisprsUpdated;
            App.Backend.OtherWhisprsUpdated += Backend_OtherWhisprsUpdated;
            App.Location.UserLocationChanged += Location_UserLocationChanged;
            App.Location.RadiusChanged += Location_RadiusChanged;
            App.Location.FeedLocationChanged += Location_FeedLocationChanged;
        }

        private void OnNavigatedFrom()
        {
            App.Backend.LatestWhisprsUpdated -= Backend_LatestWhisprsUpdated;
            App.Backend.OtherWhisprsUpdated -= Backend_OtherWhisprsUpdated;
            App.Location.UserLocationChanged -= Location_UserLocationChanged;
            App.Location.RadiusChanged -= Location_RadiusChanged;
            App.Location.FeedLocationChanged -= Location_FeedLocationChanged;
            App.Backend.LatestWhisprsUpdateEnabled = false;
            App.Backend.OtherWhisprsUpdateEnabled = false;
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
            App.Backend.GetOtherWhisprs();
        }

        private void Location_RadiusChanged()
        {
            App.Backend.GetOtherWhisprs();
        }

        private void Backend_OtherWhisprsUpdated(List<Message> whisprs)
        {
            RemoteFeed.UpdateFeed(whisprs);
            if (!IsVisible)
            {
                // No longer follow latest whisprs changes if this is hidden.
                // This is probably the first time the latest whisprs were updated.
                App.Backend.OtherWhisprsUpdated -= Backend_OtherWhisprsUpdated;
                App.Backend.OtherWhisprsUpdateEnabled = false;
            }
        }

        private void Backend_LatestWhisprsUpdated(List<Message> latestWhisprs)
        {
            Map.DrawLatestWhisprs(latestWhisprs);
            if (!IsVisible)
            {
                // No longer follow latest whisprs changes if this is hidden.
                // This is probably the first time the latest whisprs were updated.
                App.Backend.LatestWhisprsUpdated -= Backend_LatestWhisprsUpdated;
                App.Backend.LatestWhisprsUpdateEnabled = false;
            }
        }

        private void Location_UserLocationChanged()
        {
            Map.DrawUserLocation(GetMapVisibleHeight());
            if (!IsVisible)
            {
                // No longer follow user location changes if this is hidden.
                // This is probably the first time the location was aquired.
                Map.DrawFeedLocation();
                Map.DrawFeedRadius();
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
            App.Location.FeedRadius = value;
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
            App.Backend.LatestWhisprsUpdateEnabled = true;
            App.Backend.OtherWhisprsUpdateEnabled = true;
            App.Backend.LatestWhisprsUpdated += Backend_LatestWhisprsUpdated;
            App.Backend.OtherWhisprsUpdated += Backend_OtherWhisprsUpdated;
            App.Location.UserLocationChanged += Location_UserLocationChanged;
        }
    }
}
