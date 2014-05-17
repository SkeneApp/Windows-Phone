using System.Collections.Generic;

namespace WhisprBeta.Common
{
    public delegate void StatushangedEventHandler();
    public class StatusService : IStatusService
    {
        public event StatushangedEventHandler StatusChanged;

        public string CurrentStatus { get; set; }
        public bool StatusClear { get; set; }

        private static List<StatusType> _activeStatuses;

        public StatusService()
        {
            _activeStatuses = new List<StatusType>();
            CurrentStatus = string.Empty;
            StatusClear = true;
        }

        public void Set(StatusType statusType)
        {
            if (!_activeStatuses.Contains(statusType)) {
                _activeStatuses.Add(statusType);
                SetCurrentStatus();
            }
        }

        public void Unset(StatusType statusType)
        {
            if (_activeStatuses.Contains(statusType)) {
                _activeStatuses.Remove(statusType);
                SetCurrentStatus();
            }
        }

        private void SetCurrentStatus()
        {
            StatusClear = false;
            if (_activeStatuses.Contains(StatusType.LocationDisabled))
            {
                CurrentStatus = "Location services disabled. Please enable location services in your phone's settings.";
            }
            else if (_activeStatuses.Contains(StatusType.NoInternet))
            {
                CurrentStatus = "No internet connection!";
                System.Diagnostics.Debug.WriteLine("Status: No internet connection");
            }
            else if (_activeStatuses.Contains(StatusType.NoUserLocation))
            {
                CurrentStatus = "Waiting for GPS location...";
                System.Diagnostics.Debug.WriteLine("Status: Waiting for GPS location");
            }
            else if (_activeStatuses.Contains(StatusType.LoadingWhisprs))
            {
                CurrentStatus = "Loading Whisprs...";
                System.Diagnostics.Debug.WriteLine("Status: Loading Whisprs");
            }
            else
            {
                CurrentStatus = string.Empty;
                StatusClear = true;
            }
            if (StatusChanged != null)
            {
                StatusChanged();
            }
        }
    }
}
