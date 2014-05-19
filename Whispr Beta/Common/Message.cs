using Newtonsoft.Json;

namespace WhisprBeta.Common
{
    public class Message
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("pubTime")]
        public long PublishTime { get; set; }

        [JsonProperty("pubDelaySec")]
        public int PublishDelaySec { get; set; }
    }
}
