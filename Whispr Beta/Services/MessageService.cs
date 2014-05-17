using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WhisprBeta.Common;

namespace WhisprBeta.Services
{
    public class MessageService : IMessageService
    {
        private long? serverTimeDiffSeconds;
        private const string UrlBase = "http://whispr.outi.me/api/{0}";
        private const string GetParameters = "?json=1&random={0}&timestamp=0";

        public async Task<IEnumerable<Message>> Get(GeoCoordinate location, int radiusMeters)
        {
            IEnumerable<Message> result = new List<Message>();
            try
            {
                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
                var area = WBoundingBox.FromRadius(location, radiusMeters);
                var locationStr = string.Format("&min_lat={0}&min_long={1}&max_lat={2}&max_long={3}",
                    area.Min.Latitude.ToString(CultureInfo.InvariantCulture),
                    area.Min.Longitude.ToString(CultureInfo.InvariantCulture),
                    area.Max.Latitude.ToString(CultureInfo.InvariantCulture),
                    area.Max.Longitude.ToString(CultureInfo.InvariantCulture));
                var rand = Utils.DateTimeToUnixTimestamp(DateTime.Now);
                var getParameters = string.Format(GetParameters, rand);
                var uri = new Uri(string.Format(UrlBase, "get_local_whispers") + getParameters + locationStr);
                var json = await client.GetStringAsync(uri);
                result = JsonConvert.DeserializeObject<List<Message>>(json);
                ConvertToLocalTime(result);
            }
            catch (Exception ex)
            {
                // TODO: Handle this better. Display some error message or something
                Debugger.Break();
            }
            return result;
        }

        public async Task<IEnumerable<Message>> GetMapData()
        {
            IEnumerable<Message> result = new List<Message>();
            try
            {
                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
                var rand = Utils.DateTimeToUnixTimestamp(DateTime.Now);
                var getParameters = string.Format(GetParameters, rand);
                var uri = new Uri(string.Format(UrlBase, "get_latest") + getParameters);
                var json = await client.GetStringAsync(uri);
                result = JsonConvert.DeserializeObject<List<Message>>(json);
                ConvertToLocalTime(result);
            }
            catch (Exception ex)
            {
                // TODO: Handle this better. Display some error message or something
                Debugger.Break();
            }
            return result;
        }

        public async Task<string> Post(Message newMessage)
        {
	        var client = new HttpClient();
            var uri = new Uri(string.Format(UrlBase, "add_json"));
            var json = JsonConvert.SerializeObject(newMessage);
            try
            {
                var content = new StringContent(json);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception)
            {
                return "0";
            }
        }

        private async void GetServerTimeDiff()
        {
            var client = new HttpClient();
            var uri = new Uri(string.Format(UrlBase, "get_servertime"));
            try
            {
                var unixTimeStampStr = await client.GetStringAsync(uri);
                var serverTimeStamp = long.Parse(unixTimeStampStr);
                // This is not accurate by any means, considering the network latency,
                // but it should be good enough for this application.
                var localTimeStamp = Utils.DateTimeToUnixTimestamp(DateTime.Now);
                serverTimeDiffSeconds = localTimeStamp - serverTimeStamp;
            }
            catch (Exception)
            {
                // TODO: Handle the error so that we won't end up with messed up time stamps on messages
                Debugger.Break();
            }
        }

        private void ConvertToLocalTime(IEnumerable<Message> messages)
        {
            if (serverTimeDiffSeconds == null)
            {
                GetServerTimeDiff();
            }
            if (serverTimeDiffSeconds != null)
            {
                // Change server timestamp to local time stamp
                foreach (var msg in messages)
                {
                    msg.PublishTime += (long)serverTimeDiffSeconds;
                }
            }
        } 
    }
}
