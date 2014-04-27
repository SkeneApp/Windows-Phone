using System;
using System.Collections.Generic;
using System.Device.Location;
using Newtonsoft.Json;

namespace WhisprBeta.Common
{
    public class Message
    {
        public ulong id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string text { get; set; }
        public long pubTime { get; set; }
        public int pubDelaySec { get; set; }

        public Message()
        {
        }

        public Message(GeoCoordinate pos, string text, int pubDelaySec)
        {
            if (pos != null) {
                this.latitude = pos.Latitude;
                this.longitude = pos.Longitude;
            }
            this.text = text;
            this.pubDelaySec = pubDelaySec;
        }

        public static List<Message> Deserialize(string jsonString)
        {
            List<Message> posts = null;
            try
            {
                posts = JsonConvert.DeserializeObject<List<Message>>(jsonString);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Deserialize(): Exception: " + e.Message);
            }
            return posts;
        }
    }
}
