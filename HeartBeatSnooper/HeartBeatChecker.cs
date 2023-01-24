using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using HeartBeatSnooper.contracts;
using System.Text.Json;
using HeartBeatSnooper.Services;
using System.Web.Http;
using Microsoft.Extensions.Configuration;

namespace HeartBeatSnooper
{
    public class HeartBeatChecker
    {
        private readonly IConfiguration _config;

        public HeartBeatChecker(IConfiguration config)
        {
            _config = config;           
        }

        [FunctionName("HeartBeatChecker")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("HeartBeatChecker received a heartbeat action");
            var test = _config.GetValue<string>("MongoDBName");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var fileSnooperPing = JsonSerializer.Deserialize<FileSnooperPingData>(requestBody);
                log.LogInformation("Identifier: {identifier}, time sent: {timeSent}", fileSnooperPing.Identifier, fileSnooperPing.TimeSent);

                MongoDBService.Create(fileSnooperPing);
            }
            catch (Exception ex)
            {
                log.LogError("Exception in HeartBeatChecker function! Details: {error}", ex.Message);
                return new ExceptionResult(ex, false);
            }

           

            return new OkResult();
        }
    }
}
