using HeartBeatSnooperReader.Services;

namespace HeartBeatSnooperReader
{
    public class SnooperReaderWorker : BackgroundService
    {
        private readonly ILogger<SnooperReaderWorker> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _workerDelayInMinutes;
        private readonly int _minutesToFilterBack;
        public SnooperReaderWorker(
            ILogger<SnooperReaderWorker> logger,
            IConfiguration config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
            
            _workerDelayInMinutes = _config.GetValue<int>("WorkerServiceDelayInMinutes");
            _minutesToFilterBack = _config.GetValue<int>("MinutesToFilterBack");

            if(_workerDelayInMinutes == 0)
            {
                _logger.LogDebug("Failure retrieving config for Worker Delay!");
                _workerDelayInMinutes = 10;
            }

            _logger.LogDebug("Worker delay is currently: {workerDelay}", _workerDelayInMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromMinutes(_workerDelayInMinutes), stoppingToken);

                await GetHeartBeatData();
            }
        }

        private async Task GetHeartBeatData()
        {
            _logger.LogDebug("Current time is: ", DateTime.Now);
            if(DateTime.Now.Hour < 8 && DateTime.Now.Hour < 22) { return; }

            var dateTimeIntervalToFilter = DateTime.UtcNow.AddMinutes(-_minutesToFilterBack);
            
            using IServiceScope scope = _serviceProvider.CreateScope();
            ISnooperHeartbeatCompareService snooperHeartbeatCompareService= scope.ServiceProvider.GetRequiredService<ISnooperHeartbeatCompareService>();

            await snooperHeartbeatCompareService.GetLatestHeartDataByInterval(dateTimeIntervalToFilter);
        }
    }
}