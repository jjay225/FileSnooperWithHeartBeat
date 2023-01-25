using System.Threading.Tasks;

namespace FileSnooper.Services
{
    public interface IAzureHeartBeatService
    {
        Task Pulse();
    }
}