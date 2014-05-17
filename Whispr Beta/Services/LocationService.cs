using System.Device.Location;
using System.Windows;
using Windows.Devices.Geolocation;
using WhisprBeta.Common;

namespace WhisprBeta.Services
{
    public delegate void UserLocationChangedEventHandler();

    public class LocationService : ILocationService
    {
        public event UserLocationChangedEventHandler UserLocationChanged;
        private Geolocator geolocator;
        private readonly IStatusService statusService;

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
                    if (UserLocationChanged != null) {
                        UserLocationChanged();
                    }
                }
            }
        }

        public LocationService()
        {
            statusService = App.StatusService;
            statusService.Set(StatusType.NoUserLocation);
            StartTracking();
        }


        private void StartTracking()
        {
            geolocator = new Geolocator {DesiredAccuracy = PositionAccuracy.High, MovementThreshold = 50};
            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;
        }

        private void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            switch (args.Status) {
                case PositionStatus.Disabled:
                    // The application does not have the right capability or the location master switch is off
                    Deployment.Current.Dispatcher.BeginInvoke(() => statusService.Set(StatusType.LocationDisabled));
                    break;
                case PositionStatus.Initializing:
                    // The geolocator started the tracking operation
                    Deployment.Current.Dispatcher.BeginInvoke(() => statusService.Unset(StatusType.LocationDisabled));
                    break;
                case PositionStatus.NoData:
                    // The location service was not able to acquire the location
                    break;
                case PositionStatus.Ready:
                    // The location service is generating geopositions as specified by the tracking parameters
                    Deployment.Current.Dispatcher.BeginInvoke(() => statusService.Unset(StatusType.LocationDisabled));
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
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                UserLocation = new GeoCoordinate(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                statusService.Unset(StatusType.NoUserLocation);
            });
        }
    }
}
