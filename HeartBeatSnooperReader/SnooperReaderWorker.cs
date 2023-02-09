using HeartBeatSnooperReader.Services;

namespace HeartBeatSnooperReader
{
    public class SnooperReaderWorker : BackgroundService
    {
        private readonly ILogger<SnooperReaderWorker> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _workerDelayInMilliseconds;
        private readonly int _minutesToFilterBack;
        public SnooperReaderWorker(
            ILogger<SnooperReaderWorker> logger,
            IConfiguration config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
            
            _workerDelayInMilliseconds = _config.GetValue<int>("WorkerServiceDelayInMilliseconds");
            _minutesToFilterBack = _config.GetValue<int>("MinutesToFilterBack");

            if(_workerDelayInMilliseconds == 0)
            {
                _logger.LogDebug("Failure retrieving config for Worker Delay!");
                _workerDelayInMilliseconds = 20000;
            }

            _logger.LogDebug("Worker delay is currently: {workerDelay}", _workerDelayInMilliseconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(_workerDelayInMilliseconds, stoppingToken);

                await GetPingData();
            }
        }

        private async Task GetPingData()
        {
            var dateTimeIntervalToFilter = DateTime.UtcNow.AddMinutes(-_minutesToFilterBack);
            
            using IServiceScope scope = _serviceProvider.CreateScope();
            ISnooperHeartbeatCompareService snooperHeartbeatCompareService= scope.ServiceProvider.GetRequiredService<ISnooperHeartbeatCompareService>();

            await snooperHeartbeatCompareService.GetLatestPingDataByInterval(dateTimeIntervalToFilter);
        }
    }
}