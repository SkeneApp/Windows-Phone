using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Maps.Controls;
using WhisprBeta.Common;
using WhisprBeta.Map;
using WhisprBeta.Services;

namespace WhisprBeta
{
    public partial class WhisprMap
    {
        private const int NumLatestWhisprsOnMap = 200;
        private MapOverlay userLocationOverlay;
        private MapOverlay feedLocationOverlay;
        private MapOverlay feedRadiusOverlay;
        private MapOverlay[] latestWhisprsOverlays;
        private Ellipse feedRadiusCircle;
        private const double DefaultZoomLevel = 14;
        private bool firstUserLocation = true;

        public double ZoomLevel
        {
            get { return map.ZoomLevel; }
            set { map.ZoomLevel = value; }
        }

        public GeoCoordinate Center
        {
            get { return map.Center; }
            set { map.Center = value; }
        }

        public bool LockedOnUserLocation { get; private set; }

        public double ControlHeight
        {
            get { return map.ActualHeight; }
            set { map.Heading = value; }
        }

        public WhisprMap()
        {
            InitializeComponent();
            LockedOnUserLocation = true;
        }
        public void InitializeMapGraphics()
        {
            // Geometric shapes
            feedRadiusCircle = new Ellipse()
            {
                Fill = new SolidColorBrush(Colors.Green),
                Opacity = 0.1
            };
            // Overlays
            userLocationOverlay = new MapOverlay()
            {
                PositionOrigin = new Point(0.5, 0.5),
                Content = new Ellipse()
                {
                    Fill = new SolidColorBrush(Colors.Blue),
                    Opacity = 0.5,
                    Height = 20,
                    Width = 20
                }
            };
            feedRadiusOverlay = new MapOverlay()
            {
                PositionOrigin = new Point(0.5, 0.5),
                Content = feedRadiusCircle
            };
            feedLocationOverlay = new MapOverlay()
            {
                PositionOrigin = new Point(0.5, 0.5),
                Content = new Ellipse()
                {
                    Fill = new SolidColorBrush(Colors.Green),
                    Opacity = 0.5,
                    Height = 20,
                    Width = 20
                }
            };
            latestWhisprsOverlays = new MapOverlay[NumLatestWhisprsOnMap];
            for (int i = 0; i < latestWhisprsOverlays.Length; i++)
            {
                latestWhisprsOverlays[i] = new MapOverlay()
                {
                    PositionOrigin = new Point(0.5, 0.5)
                };
            }
            // Layers
            MapLayer userLocationLayer = new MapLayer();
            userLocationLayer.Add(userLocationOverlay);
            MapLayer latestWhisprsLayer = new MapLayer();
            foreach (MapOverlay overlay in latestWhisprsOverlays)
            {
                latestWhisprsLayer.Add(overlay);
            }
            MapLayer feedRadiusLayer = new MapLayer();
            feedRadiusLayer.Add(feedRadiusOverlay);
            MapLayer feedLocationLayer = new MapLayer();
            feedLocationLayer.Add(feedLocationOverlay);
            // Map
            map.Layers.Add(latestWhisprsLayer);
            map.Layers.Add(feedRadiusLayer);
            map.Layers.Add(feedLocationLayer);
            map.Layers.Add(userLocationLayer);
            //DrawUserLocation();
            //DrawFeedLocation();
        }

        public void GoToUserLocation(double mapVisibleHeight)
        {
            if (App.Location.UserLocation != null)
            {
                LockedOnUserLocation = true;
                map.TransformCenter = new Point(0.5, GetMapDisplayedCenterY(mapVisibleHeight));
                // Now for map transform center to work, we need to apply new Center.
                // But if the Center coordinate hasn't changed, the map will not update.
                // So if center is the same, change it a little bit and change back immediately
                // to firce the map to refresh.
                if (Math.Round(map.Center.Latitude, 4) == Math.Round(App.Location.UserLocation.Latitude, 4)
                    && Math.Round(map.Center.Longitude, 4) == Math.Round(App.Location.UserLocation.Longitude, 4))
                {
                    map.Center = new GeoCoordinate(App.Location.UserLocation.Latitude - 1, App.Location.UserLocation.Longitude);
                }
                map.Center = App.Location.UserLocation;
                map.ZoomLevel = DefaultZoomLevel;
            }
        }

        public void LoadMapData(double mapVisibleHeight)
        {
            map.TransformCenter = new Point(0.5, GetMapDisplayedCenterY(mapVisibleHeight));
            map.ZoomLevel = DefaultZoomLevel;
            DrawUserLocation(mapVisibleHeight);
            DrawFeedLocation();
            DrawFeedRadius();
        }

        public void DrawUserLocation(double mapVisibleHeight)
        {
            if (App.Location.UserLocation != null && userLocationOverlay != null)
            {
                if (LockedOnUserLocation)
                {
                    App.Location.FeedLocation = App.Location.UserLocation;
                }
                userLocationOverlay.GeoCoordinate = App.Location.UserLocation;
                if (firstUserLocation)
                {
                    firstUserLocation = false;
                    GoToUserLocation(mapVisibleHeight);
                }
            }
        }
        public void DrawFeedLocation()
        {
            if (App.Location.FeedLocation != null && feedLocationOverlay != null)
            {
                feedRadiusOverlay.GeoCoordinate = App.Location.FeedLocation;
                feedLocationOverlay.GeoCoordinate = App.Location.FeedLocation;
            }
        }
        public void DrawFeedRadius()
        {
            DrawFeedRadius(App.Location.FeedRadius);
        }
        public void DrawFeedRadius(int radius)
        {
            if (App.Location.FeedLocation != null && feedRadiusCircle != null)
            {
                double circleRadius = MetersToCircleRadius(App.Location.FeedLocation, radius);
                feedRadiusCircle.Height = circleRadius * 2;
                feedRadiusCircle.Width = circleRadius * 2;
            }
        }
        public void DrawLatestWhisprs(List<Message> whisprPosts)
        {
            if (latestWhisprsOverlays != null)
            {
                int i = 0;
                foreach (Message post in whisprPosts)
                {
                    if (i < latestWhisprsOverlays.Length)
                    {
                        latestWhisprsOverlays[i].GeoCoordinate = new GeoCoordinate(post.latitude, post.longitude);
                        latestWhisprsOverlays[i].Content = new Ellipse()
                        {
                            Fill = new SolidColorBrush(Colors.Red),
                            Opacity = 0.1,
                            Height = 30,
                            Width = 30
                        };
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        private double MetersToCircleRadius(GeoCoordinate pos, int meters)
        {
            WBoundingBox box = WBoundingBox.FromRadius(pos, meters);
            GeoCoordinate c1 = new GeoCoordinate(pos.Latitude, box.Min.Longitude);
            GeoCoordinate c2 = new GeoCoordinate(pos.Latitude, box.Max.Longitude);
            Point p1 = map.ConvertGeoCoordinateToViewportPoint(c1);
            Point p2 = map.ConvertGeoCoordinateToViewportPoint(c2);
            double dist = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            return dist / 2;
        }

        /// <summary>
        /// Calculates the center point of visible portion of the map.
        /// This function calcualtes only the Y coordinate, because
        /// X coordinate is always going to be 0.5.
        /// 
        /// This is used at least when running GoToUserLocation() function,
        /// that centers map to current user location.
        /// </summary>
        /// <returns>Returned value range between 0.0 and 1.0</returns>
        private double GetMapDisplayedCenterY(double mapVisibleHeight)
        {
            double mapCenterY = 1 / map.Height * (mapVisibleHeight / 2);
            return mapCenterY;
        }


        private void map_ZoomLevelChanged(object sender, MapZoomLevelChangedEventArgs e)
        {
            DrawFeedRadius();
        }

        private void map_Tap(object sender, GestureEventArgs e)
        {
            LockedOnUserLocation = false;
            GeoCoordinate position = map.ConvertViewportPointToGeoCoordinate(e.GetPosition(map));
            App.Location.FeedLocation = position;

        }
    }
}
