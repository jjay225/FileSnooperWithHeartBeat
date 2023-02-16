using FileSnooper.Contracts.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FileSnooper.Services
{
    public class AzureHeartBeatService : IAzureHeartBeatService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AzureHeartBeatService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _identifier;
        private readonly string _azureHeartBeatServiceFunctionUrlPath;
        //private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //};
        public const string ClientName = "AzureHeartBeat";

        public AzureHeartBeatService(
            IConfiguration config,
            ILogger<AzureHeartBeatService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            _identifier = _config.GetValue<string>("Identifier");
            _azureHeartBeatServiceFunctionUrlPath = _config.GetValue<string>("AzureHeartBeatServiceFunctionUrlPath");
        }
        public async Task Pulse()
        {
            try
            {
                _logger.LogDebug("Sending pulse to azure heart beat service at: {time} ", DateTime.Now);
                var client = _httpClientFactory.CreateClient(ClientName);

                var pingData = new FileSnooperPingData
                {
                    Identifier = _identifier,
                    TimeSent = DateTime.Now
                };

                var serializedContent = JsonSerializer.Serialize(pingData);
                _logger.LogDebug("Ping Data to send: {pingData}", serializedContent);

                var clientResponse = await client.PostAsJsonAsync(requestUri: _azureHeartBeatServiceFunctionUrlPath, value: pingData);
                var stringResponse = await clientResponse.Content.ReadAsStringAsync();//do more with this later maybe
                if (clientResponse.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully sent pulse to heart beat service");
                }
                else
                {
                    _logger.LogError("Failed to send pulse to heart beat service! String Response: {response}", stringResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception sending heartbeat pulse to Azure! Error details: {errorDetail}", ex);
            }           
        }
    }
}
