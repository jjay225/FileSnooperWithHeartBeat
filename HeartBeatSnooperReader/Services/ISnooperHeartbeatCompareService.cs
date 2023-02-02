using FileSnooper.Contracts.Classes;

namespace HeartBeatSnooperReader.Services
{
    public interface ISnooperHeartbeatCompareService
    {
        Task<List<FileSnooperPingData>> GetLatestPingDataByInterval(DateTime filter);
    }
}