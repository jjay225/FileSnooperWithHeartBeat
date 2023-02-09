using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using HeartBeatSnooper.Services;
using System.Web.Http;
using Microsoft.Extensions.Configuration;
using FileSnooper.Contracts.Classes;
using FileSnooper.Contracts.Services;
using MongoDB.Bson;
using System.Linq;

namespace HeartBeatSnooper
{
    public class HeartBeatChecker
    {
        private readonly IConfiguration _config;
        private readonly IAzureCosmosDBService _azureMongoDBService;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public HeartBeatChecker(
            IConfiguration config,
            IAzureCosmosDBService azureMongoDBService)
        {
            _config = config;
            _azureMongoDBService = azureMongoDBService;
        }

        [FunctionName("HeartBeatChecker")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("HeartBeatChecker received a heartbeat action");
            
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var fileSnooperPing = new FileSnooperPingData();
                var isPostman = false;
                var userAgent = req.Headers.Where(x => x.Key == "User-Agent")?.First().Value.ToString();
                if(userAgent != null)
                {
                    if(userAgent.ToLower().Contains("postman"))
                    {
                        isPostman = true;
                    }
                }

                if(isPostman)
                {
                    fileSnooperPing = JsonSerializer.Deserialize<FileSnooperPingData>(requestBody);
                }
                else
                {
                   fileSnooperPing = JsonSerializer.Deserialize<FileSnooperPingData>(requestBody, _jsonSerializerOptions);
                }
                
                log.LogInformation("Identifier: {identifier}, time sent: {timeSent}", fileSnooperPing.Identifier, fileSnooperPing.TimeSent);
                fileSnooperPing.TimeSent = DateTime.UtcNow;
                await _azureMongoDBService.Create(fileSnooperPing);

            }
            catch (Exception ex)
            {
                log.LogError("Exception in HeartBeatChecker function! Details: {error}", ex.Message);
                return new ExceptionResult(ex, true);
            }           

            return new OkResult();
        }
    }
}
