using System;
using System.Net;

namespace WhisprBeta.Services
{
    public static class Network
    {
        public static void GetRequest(string uri, Action<string> callback, Action<Exception> errorCallback)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e) {
                if (e.Error == null) {
                    callback(e.Result);
                } else {
                    errorCallback(e.Error);
                }
            };
            webClient.DownloadStringAsync(new Uri(uri));
        }

        public static void PostRequest(string uri, Action<string> callback, Action<Exception> errorCallback, string data)
        {
            WebClient webClient = new WebClient();
            webClient.UploadStringCompleted += delegate(object sender, UploadStringCompletedEventArgs e) {
                if (e.Error == null) {
                    callback(e.Result);
                } else {
                    errorCallback(e.Error);
                }
            };
            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            webClient.UploadStringAsync(new Uri(uri), "POST", data);
        }
    }
}
