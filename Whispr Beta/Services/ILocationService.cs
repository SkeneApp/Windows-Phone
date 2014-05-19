using System.Device.Location;

namespace WhisprBeta.Services
{
    public interface ILocationService
    {
        event UserLocationChangedEventHandler UserLocationChanged;
        GeoCoordinate UserLocation { get; set; }
    }
}