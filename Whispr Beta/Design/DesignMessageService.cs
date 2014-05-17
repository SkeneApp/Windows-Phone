using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Threading.Tasks;
using WhisprBeta.Common;
using WhisprBeta.Services;

namespace WhisprBeta.Design
{
    /// <summary>
    /// This class contains design time data for Blend or Visual Studio designer
    /// </summary>
    public class DesignMessageService : IMessageService
    {
        private List<Message> fakeMessages; 
        public Task<IEnumerable<Message>> Get(GeoCoordinate location, int radiusMeters)
        {
            if (fakeMessages == null) { GenerateFakeMessages(); }
            return Task.FromResult((IEnumerable<Message>)fakeMessages);
        }

        public Task<string> Post(Message newMessage)
        {
            // Post is unlikely to be called from a designer
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Message>> GetMapData()
        {
            if (fakeMessages == null) { GenerateFakeMessages(); }
            return Task.FromResult((IEnumerable<Message>)fakeMessages);
        }

        private void GenerateFakeMessages()
        {
            fakeMessages = new List<Message>
            {
                new Message {Latitude = 60.4532, Longitude = 22.2866},
                new Message {Latitude = 60.4527, Longitude = 22.2847},
                new Message {Latitude = 60.4558, Longitude = 22.2906},
                new Message {Latitude = 60.4612, Longitude = 22.2864},
                new Message {Latitude = 60.1644, Longitude = 24.9113},
                new Message {Latitude = 60.1644, Longitude = 24.9113},
                new Message {Latitude = 60.1644, Longitude = 24.9113},
                new Message {Latitude = 60.4547, Longitude = 22.29},
                new Message {Latitude = 60.4549, Longitude = 22.2919},
                new Message {Latitude = 60.4504, Longitude = 22.2911},
                new Message {Latitude = 60.455, Longitude = 22.2821},
                new Message {Latitude = 60.4549, Longitude = 22.2735},
                new Message {Latitude = 60.4511, Longitude = 22.2692},
                new Message {Latitude = 60.4511, Longitude = 22.2692}
            };
            int msgCount = 0;
            foreach (Message msg in fakeMessages)
            {
                msg.PublishTime = Utils.DateTimeToUnixTimestamp(DateTime.Now - TimeSpan.FromHours(msgCount*5));
                msg.Text = "Single-origin coffee raw denim aesthetic, retro semiotics roof party try-hard salvia. Flannel keytar photo booth American Apparel, Pitchfork umami slow-carb.";
                msgCount++;
            }
        }
    }
}

