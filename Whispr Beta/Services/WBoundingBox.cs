using System;
using System.Device.Location;
using WhisprBeta.Common;

namespace WhisprBeta.Services
{
    public class WBoundingBox
    {
        public GeoCoordinate Min { get; set; }
        public GeoCoordinate Max { get; set; }

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
            return new WBoundingBox
            {
                Min = new GeoCoordinate(minLat, minLong),
                Max = new GeoCoordinate(maxLat, maxLong)
            };
        }
    }
}