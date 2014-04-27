using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Threading;
using WhisprBeta.Common;

namespace WhisprBeta.Services
{
    public delegate void ServerTimeUpdatedEventHandler();
    public delegate void LatestWhisprsUpdatedEventHandler(List<Message> latestWhisprs);
    public delegate void LocalWhisprsUpdatedEventHandler(List<Message> localWhisprs);
    public delegate void OtherWhisprsUpdatedEventHandler(List<Message> otherWhisprs);
    public class Backend
    {
        public event ServerTimeUpdatedEventHandler ServerTimeUpdated;
        public event LatestWhisprsUpdatedEventHandler LatestWhisprsUpdated;
        public event LocalWhisprsUpdatedEventHandler LocalWhisprsUpdated;
        public event OtherWhisprsUpdatedEventHandler OtherWhisprsUpdated;

        public TimeSpan ServerTimeDiff { get; set; }
        private bool localWhisprsUpdateEnabled;
        public bool LocalWhisprsUpdateEnabled
        {
            get {
                return localWhisprsUpdateEnabled;
            }
            set {
                if (value != localWhisprsUpdateEnabled) {
                    localWhisprsUpdateEnabled = value;
                    if (value) {
                        GetLocalWhisprs();
                        localWhisprsUpdateTimer.Start();
                    } else {
                        localWhisprsUpdateTimer.Stop();
                    }
                }
            }
        }
        private bool latestWhisprsUpdateEnabled;
        public bool LatestWhisprsUpdateEnabled
        {
            get
            {
                return latestWhisprsUpdateEnabled;
            }
            set
            {
                if (value != latestWhisprsUpdateEnabled) {
                    latestWhisprsUpdateEnabled = value;
                    if (value) {
                        GetLatestWhisprs();
                        latestWhisprsUpdateTimer.Start();
                    } else {
                        latestWhisprsUpdateTimer.Stop();
                    }
                }
            }
        }
        private bool otherWhisprsUpdateEnabled;
        public bool OtherWhisprsUpdateEnabled
        {
            get
            {
                return otherWhisprsUpdateEnabled;
            }
            set
            {
                if (value != otherWhisprsUpdateEnabled) {
                    otherWhisprsUpdateEnabled = value;
                    if (value) {
                        GetOtherWhisprs();
                        otherWhisprsUpdateTimer.Start();
                    } else {
                        otherWhisprsUpdateTimer.Stop();
                    }
                }
            }
        }
        public TimeSpan PublishDelay = TimeSpan.FromMinutes(0);

        private const int LatestWhisprsRefreshIntervalSec = 30;
        private const int LocalWhisprsRefreshIntervalSec = 5;
        private const int OtherWhisprsRefreshIntervalSec = 5;
        private const string BackendBaseUrl = "http://whispr.outi.me/api/";
        private const string GetLocalUrl = BackendBaseUrl + "get_local_whispers";
        private const string GetServertimeUrl = BackendBaseUrl + "get_servertime";
        private const string GetLatestUrl = BackendBaseUrl + "get_latest";
        private const string PostUrl = BackendBaseUrl + "add";
        private readonly DispatcherTimer latestWhisprsUpdateTimer = new DispatcherTimer();
        private readonly DispatcherTimer localWhisprsUpdateTimer = new DispatcherTimer();
        private readonly DispatcherTimer otherWhisprsUpdateTimer = new DispatcherTimer();
        private bool serverTimeSynchronized;
        private ulong latestRequestId;
        private ulong localRequestId;
        private ulong otherRequestId;

        public Backend()
        {
            ServerTimeDiff = new TimeSpan(0);
            LocalWhisprsUpdateEnabled = false;
            LatestWhisprsUpdateEnabled = false;
            OtherWhisprsUpdateEnabled = false;
            InitializeRefreshTimers();
            GetServerTime();
        }

        #region Get server time
        public void GetServerTime()
        {
            if (!HasInternetConnection()) {
                return;
            }
            Network.GetRequest(GetServertimeUrl, GetServerTimeProcessResponse, GetServerTimeProcessError);
        }
        private void GetServerTimeProcessResponse(string response)
        {
            DateTime serverTime = Utils.UnixTimeStampToDateTime(double.Parse(response));
            ServerTimeDiff = DateTime.Now - serverTime;
            serverTimeSynchronized = true;
            if (ServerTimeUpdated != null) {
                ServerTimeUpdated();
            }
            if (LocalWhisprsUpdateEnabled) {
                GetLocalWhisprs();
            }
            if (OtherWhisprsUpdateEnabled) {
                GetOtherWhisprs();
            }
        }
        private void GetServerTimeProcessError(Exception e)
        {
            System.Diagnostics.Debug.WriteLine("GetServerTimeProcessError(): Getting server time error: " + e.Message);
            GetServerTime();
        }
        #endregion

        #region Get latest Whisprs
        public void GetLatestWhisprs()
        {
            if (!HasInternetConnection()) {
                return;
            }
            latestRequestId++;
            string requestStr = GenerateGetRequestString(GetLatestUrl, latestRequestId);
            Network.GetRequest(requestStr, GetLatestWhisprsProcessResponse, GetLatestWhisprsProcessError);
        }
        private void GetLatestWhisprsProcessResponse(string response)
        {
            ulong reqId = ParseRequestId(ref response);
            if (reqId == latestRequestId) {
                // This response is fresh - use it
                List<Message> resultList = Message.Deserialize(response);
                if (resultList != null && latestWhisprsUpdateEnabled) {
                    if (LatestWhisprsUpdated != null) {
                        LatestWhisprsUpdated(resultList);
                    }
                }
            } else {
                // This response is not fresh - ignore it.
            }
        }
        private void GetLatestWhisprsProcessError(Exception e)
        {
            System.Diagnostics.Debug.WriteLine("GetLatestWhisprs(): Failed to send or receive request. Error: " + e.Message);
        }
        #endregion

        #region Get local Whisprs
        public void GetLocalWhisprs()
        {
            if (!HasInternetConnection()) {
                return;
            }
            if (serverTimeSynchronized == false) {
                System.Diagnostics.Debug.WriteLine("GetLocalWhisprs(): Server time not synchronized");
                return;
            }
            if (App.Location.UserLocation == null) {
                System.Diagnostics.Debug.WriteLine("GetLocalWhisprs(): User location not known");
                return;
            }
            localRequestId++;
            WBoundingBox area = WBoundingBox.FromRadius(App.Location.UserLocation, App.Location.FeedRadius);
            string requestStr = GenerateGetLocalRequestString(GetLocalUrl, area, localRequestId);
            Network.GetRequest(requestStr, GetLocalWhisprsProcessResponse, GetLocalWhisprsProcessError);
        }
        private void GetLocalWhisprsProcessResponse(string response)
        {
            ulong reqId = ParseRequestId(ref response);
            if (reqId == localRequestId) {
                // This response is fresh - use it
                List<Message> resultList = Message.Deserialize(response);
                if (resultList != null && localWhisprsUpdateEnabled) {
                    App.Status.Unset(Status.StatusType.LoadingWhisprs);
                    if (LocalWhisprsUpdated != null) {
                        LocalWhisprsUpdated(resultList);
                    }
                }
            } else {
                // This response is not fresh - ignore it.
            }
        }
        private void GetLocalWhisprsProcessError(Exception e)
        {
            System.Diagnostics.Debug.WriteLine("GetLocalWhisprs(): Failed to send or receive request. Error: " + e.Message);
        }
        #endregion

        #region Get other Whisprs
        public void GetOtherWhisprs()
        {
            if (!HasInternetConnection()) {
                return;
            }
            if (serverTimeSynchronized == false) {
                System.Diagnostics.Debug.WriteLine("GetOtherWhisprs(): Server time not synchronized");
                return;
            }
            if (App.Location.FeedLocation == null) {
                System.Diagnostics.Debug.WriteLine("GetOtherWhisprs(): Feed location not known");
                return;
            }
            otherRequestId++;
            WBoundingBox area = WBoundingBox.FromRadius(App.Location.FeedLocation, App.Location.FeedRadius);
            string requestStr = GenerateGetLocalRequestString(GetLocalUrl, area, otherRequestId);
            Network.GetRequest(requestStr, GetOtherWhisprsProcessResponse, GetOtherWhisprsProcessError);
        }
        private void GetOtherWhisprsProcessResponse(string response)
        {
            ulong reqId = ParseRequestId(ref response);
            if (reqId == otherRequestId) {
                // This response is fresh - use it
                List<Message> resultList = Message.Deserialize(response);
                if (resultList != null && otherWhisprsUpdateEnabled) {
                    if (OtherWhisprsUpdated != null) {
                        OtherWhisprsUpdated(resultList);
                    }
                }
            } else {
                // This response is not fresh - ignore it.
            }
        }
        private void GetOtherWhisprsProcessError(Exception e)
        {
            System.Diagnostics.Debug.WriteLine("GetOtherWhisprs(): Failed to send or receive request. Error: " + e.Message);
        }
        #endregion

        #region Request helper methods
        private string GenerateGetLocalRequestString(string url, WBoundingBox area, ulong req_id)
        {
            string requestStr = GenerateGetRequestString(url, req_id);
            requestStr += "&timestamp=" + 0;
            requestStr += "&min_lat=" + area.Min.Latitude.ToString(CultureInfo.InvariantCulture) + "&min_long=" + area.Min.Longitude.ToString(CultureInfo.InvariantCulture);
            requestStr += "&max_lat=" + area.Max.Latitude.ToString(CultureInfo.InvariantCulture) + "&max_long=" + area.Max.Longitude.ToString(CultureInfo.InvariantCulture);
            return requestStr;
        }
        private string GenerateGetRequestString(string url, ulong req_id)
        {
            string requestStr = url;
            requestStr += "?json=1";
            requestStr += "&random=" + Utils.DateTimeToUnixTimestamp(DateTime.Now);
            requestStr += "&req_id=" + req_id;
            return requestStr;
        }
        private ulong ParseRequestId(ref string data)
        {
            ulong reqId;
            string reqIdTagStart = "{\"req_id\":\"";
            string reqIdTagEnd = "\"}";
            int idxOfReqIdTagStart = data.IndexOf(reqIdTagStart);
            int idxOfReqIdStart = idxOfReqIdTagStart + reqIdTagStart.Length;
            int idxOfReqIdEnd = data.IndexOf(reqIdTagEnd, idxOfReqIdStart);
            int lenOfReqId = idxOfReqIdEnd - idxOfReqIdStart;
            string reqIdStr = data.Substring(idxOfReqIdStart, lenOfReqId);
            reqId = ulong.Parse(reqIdStr);
            // Remove the request id from data
            int lenOfReqIdTag = idxOfReqIdEnd - idxOfReqIdTagStart + reqIdTagEnd.Length;
            data = data.Remove(idxOfReqIdTagStart, lenOfReqIdTag);
            return reqId;
        }
        #endregion

        #region Post Whispr
        public void PostWhispr(Message message)
        {
            if (!HasInternetConnection()) {
                return;
            }
            if (serverTimeSynchronized == false) {
                System.Diagnostics.Debug.WriteLine("PostWhispr(): Server time not synchronized");
                return;
            }
            string postData = "lat=" + message.latitude.ToString(CultureInfo.InvariantCulture);
            postData += "&long=" + message.longitude.ToString(CultureInfo.InvariantCulture);
            postData += "&text=" + System.Net.HttpUtility.UrlEncode(message.text);
            postData += "&pubt=" + message.pubDelaySec;
            Network.PostRequest(PostUrl, PostWhisprProcessResponse, PostWhisprProcessError, postData);
        }
        private void PostWhisprProcessResponse(string response)
        {
            GetLocalWhisprs();
        }
        private void PostWhisprProcessError(Exception e)
        {
            System.Diagnostics.Debug.WriteLine("PostWhispr(): Error posting Whispr, message: " + e.Message);
        }
        #endregion

        private bool HasInternetConnection()
        {
            bool hasConnection = NetworkInterface.GetIsNetworkAvailable();

            if (hasConnection) {
                App.Status.Unset(Status.StatusType.NoInternet);
            } else {
                App.Status.Set(Status.StatusType.NoInternet);
            }
            return hasConnection;
        }

        #region Refresh timer functions
        private void InitializeRefreshTimers()
        {
            latestWhisprsUpdateTimer.Interval = TimeSpan.FromSeconds(LatestWhisprsRefreshIntervalSec);
            latestWhisprsUpdateTimer.Tick += latestWhisprUpdateTimer_Tick;
            localWhisprsUpdateTimer.Interval = TimeSpan.FromSeconds(LocalWhisprsRefreshIntervalSec);
            localWhisprsUpdateTimer.Tick += localWhisprUpdateTimer_Tick;
            otherWhisprsUpdateTimer.Interval = TimeSpan.FromSeconds(OtherWhisprsRefreshIntervalSec);
            otherWhisprsUpdateTimer.Tick += otherWhisprUpdateTimer_Tick;
        }
        private void localWhisprUpdateTimer_Tick(object sender, EventArgs e)
        {
            GetLocalWhisprs();
        }

        private void latestWhisprUpdateTimer_Tick(object sender, EventArgs e)
        {
            GetLatestWhisprs();
        }

        private void otherWhisprUpdateTimer_Tick(object sender, EventArgs e)
        {
            GetOtherWhisprs();
        }
        #endregion
    }
}
