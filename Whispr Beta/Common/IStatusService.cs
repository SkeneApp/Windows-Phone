namespace WhisprBeta.Common
{
    public enum StatusType
    {
        None,
        NoInternet,
        NoUserLocation,
        LoadingWhisprs,
        LocationDisabled
    }
    public interface IStatusService
    {
        event StatushangedEventHandler StatusChanged;
        string CurrentStatus { get; set; }
        bool StatusClear { get; set; }
        void Set(StatusType statusType);
        void Unset(StatusType statusType);
    }
}