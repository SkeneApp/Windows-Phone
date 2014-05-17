using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhisprBeta.Services;

namespace WhisprBeta.Design
{
    /// <summary>
    /// This class contains design time data for Blend or Visual Studio designer
    /// </summary>
    class DesignLocationService : ILocationService
    {
        public event UserLocationChangedEventHandler UserLocationChanged;
        public GeoCoordinate UserLocation { get; set; }

        public DesignLocationService()
        {
            UserLocation = new GeoCoordinate(60.4532, 22.2866);
            if (UserLocationChanged != null)
            {
                UserLocationChanged();
            }
        }
    }
}
