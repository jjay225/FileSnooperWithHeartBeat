namespace HeartBeatSnooperReader
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly int _workerDelayInMilliseconds;
        public Worker(
            ILogger<Worker> logger,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _workerDelayInMilliseconds = _config.GetValue<int>("WorkerServiceDelayInMilliseconds");
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
            }
        }
    }
}