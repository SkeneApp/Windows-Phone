using System;
using System.Device.Location;
using System.Windows;
using Windows.Devices.Geolocation;
using WhisprBeta.Common;

namespace WhisprBeta.Services
{
    public delegate void UserLocationChangedEventHandler();
    public delegate void FeedLocationChangedEventHandler();
    public delegate void RadiusChangedEventHandler();
    public class Location
    {
        public event UserLocationChangedEventHandler UserLocationChanged;
        public event FeedLocationChangedEventHandler FeedLocationChanged;
        public event RadiusChangedEventHandler RadiusChanged;
        private int feedRadius = 100;
        private Geolocator geolocator;
        public int FeedRadius
        {
            get
            {
                return feedRadius;
            }
            set
            {
                if (value != feedRadius) {
                    feedRadius = value;
                    if (RadiusChanged != null)
                    {
                        RadiusChanged();
                    }
                }
            }
        }
        private GeoCoordinate feedLocation;
        public GeoCoordinate FeedLocation
        {
            get
            {
                return feedLocation;
            }
            set
            {
                if (value != feedLocation) {
                    feedLocation = value;
                    if (FeedLocationChanged != null)
                    {
                        FeedLocationChanged();
                    }
                }
            }
        }
        private GeoCoordinate userLocation;
        public GeoCoordinate UserLocation
        {
            get
            {
                return userLocation;
            }
            set
            {
                if (value != userLocation) {
                    userLocation = value;
                    if (FeedLocation == null) {
                        FeedLocation = value;
                    }
                    if (UserLocationChanged != null) {
                        UserLocationChanged();
                    }
                }
            }
        }

        public Location()
        {
            App.Status.Set(Status.StatusType.NoUserLocation);
            StartTracking();
        }


        private void StartTracking()
        {
            geolocator = new Geolocator {DesiredAccuracy = PositionAccuracy.High, MovementThreshold = 50};
            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;
            System.Diagnostics.Debug.WriteLine("Geolocation: Started tracking");
        }

        // NOTE: geolocator_StatusChanged is currently not used for anything.
        // One parctical use for it would be detecting if location has been disabled in
        // phone's settings (PositionStatus.Disabled), so the app would ask user to go
        // to settings and enable it.
        private void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            switch (args.Status) {
                case PositionStatus.Disabled:
                    // The application does not have the right capability or the location master switch is off
                    App.Status.Set(Status.StatusType.LocationDisabled);
                    break;
                case PositionStatus.Initializing:
                    // The geolocator started the tracking operation
                    Deployment.Current.Dispatcher.BeginInvoke(() => App.Status.Unset(Status.StatusType.LocationDisabled));
                    break;
                case PositionStatus.NoData:
                    // The location service was not able to acquire the location
                    break;
                case PositionStatus.Ready:
                    // The location service is generating geopositions as specified by the tracking parameters
                    App.Status.Unset(Status.StatusType.LocationDisabled);
                    break;
                case PositionStatus.NotAvailable:
                    // Not used in WindowsPhone, Windows desktop uses this value to signal that there is no hardware capable to acquire location information
                    break;
                case PositionStatus.NotInitialized:
                    // The initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state
                    break;
            }
        }

        private void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Geolocation: New position");
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                UserLocation = new GeoCoordinate(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                App.Status.Unset(Status.StatusType.NoUserLocation);
            });
        }
    }
    public class WBoundingBox
    {
        public GeoCoordinate Min { get; set; }
        public GeoCoordinate Max { get; set; }
        public WBoundingBox()
        {
            this.Min = new GeoCoordinate();
            this.Max = new GeoCoordinate();
        }
        public WBoundingBox(WBoundingBox box)
        {
            this.Min = new GeoCoordinate(box.Min.Latitude, box.Min.Longitude);
            this.Max = new GeoCoordinate(box.Max.Latitude, box.Max.Longitude);
        }
        public WBoundingBox(GeoCoordinate min, GeoCoordinate max)
        {
            this.Min = new GeoCoordinate(min.Latitude, min.Longitude);
            this.Max = new GeoCoordinate(max.Latitude, max.Longitude);
        }
        public WBoundingBox(double minLat, double minLng, double maxLat, double maxLng)
        {
            this.Min = new GeoCoordinate(minLat, minLng);
            this.Max = new GeoCoordinate(maxLat, maxLng);
        }
        public static WBoundingBox FromRadius(GeoCoordinate pos, int radiusMeters)
        {
            double latRadian = Utils.ToRadians(pos.Latitude);
        	const double degLatKm = 110.574235; 
        	double degLongKm = 110.572833 * Math.Cos(latRadian);
            double deltaLat = radiusMeters / 1000.0 / degLatKm;
            double deltaLong = radiusMeters / 1000.0 / degLongKm;
            double minLat = pos.Latitude - deltaLat;
            double minLong = pos.Longitude - deltaLong;
            double maxLat = pos.Latitude + deltaLat;
            double maxLong = pos.Longitude + deltaLong;
            return new WBoundingBox(minLat, minLong, maxLat, maxLong);
        }
    }
}
