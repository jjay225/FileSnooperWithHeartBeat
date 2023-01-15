using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HeartBeatSnooper.contracts;

namespace HeartBeatSnooper
{
    public static class HeartBeatChecker
    {
        [FunctionName("HeartBeatChecker")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("HeartBeatChecker received a ping action");
            

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var fileSnooperPing = JsonConvert.DeserializeObject<FileSnooperPingData>(requestBody);
            log.LogInformation("Identifier: {identifier}, time sent: {timeSent}", fileSnooperPing.Identifier, fileSnooperPing.TimeSent);
          

            return new OkResult();
        }
    }
}
