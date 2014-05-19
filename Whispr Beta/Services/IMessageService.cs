using System.Collections.Generic;
using System.Device.Location;
using System.Threading.Tasks;
using WhisprBeta.Common;

namespace WhisprBeta.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> Get(GeoCoordinate location, int radiusMeters);
        Task<string> Post(Message newMessage);
        Task<IEnumerable<Message>> GetMapData();
    }
}
