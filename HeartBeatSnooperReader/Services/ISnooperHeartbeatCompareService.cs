using FileSnooper.Contracts.Classes;

namespace HeartBeatSnooperReader.Services
{
    public interface ISnooperHeartbeatCompareService
    {
        Task GetLatestHeartDataByInterval(DateTime filter);
    }
}