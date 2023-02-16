namespace HeartBeatSnooperReader.Services
{
    public interface ISendGridService
    {
        Task SendEmailTest();
        Task SendAlertEmail();
    }
}