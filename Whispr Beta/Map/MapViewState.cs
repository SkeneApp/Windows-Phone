using System.Device.Location;
using Microsoft.Phone.Maps.Controls;

namespace WhisprBeta.Map
{
    public class MapViewState
    {
        public double MapZoomLevel { get; set; }
        public MapLayer FeedLocationLayer { get; set; }
        public MapLayer FeedRadiusLayer { get; set; }
        public MapLayer LatestWhisprsLayer { get; set; }
        public MapLayer UserLocationLayer { get; set; }
        public GeoCoordinate MapCenter { get; set; }
        public bool FirstUserLocation { get; set; }
        public bool Initialized { get; set; }
        public double LayoutTransformY { get; set; }
        public bool FeedLocationLockedOnUserLocation { get; set; }

        public MapViewState()
        {
            FirstUserLocation = true;
            Initialized = false;
        }
    }
}
