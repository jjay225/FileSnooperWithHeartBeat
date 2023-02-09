using FileSnooper.Contracts.Classes;
using MongoDB.Bson;
using MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using FileSnooper.Contracts.Services;

namespace HeartBeatSnooperReader.Services
{
    public class SnooperHeartbeatCompareService : ISnooperHeartbeatCompareService
    {
        private readonly ILogger<SnooperHeartbeatCompareService> _logger;
        private readonly IAzureCosmosDBService _azureCosmosDBService;
        private readonly ISendGridService _sendGridService;

        public SnooperHeartbeatCompareService(
            ILogger<SnooperHeartbeatCompareService> logger,
            IAzureCosmosDBService azureCosmosDBService,
            ISendGridService sendGridService)
        {
            _logger = logger;
            _azureCosmosDBService = azureCosmosDBService;
            _sendGridService = sendGridService;
        }
        public async Task GetLatestPingDataByInterval(DateTime filter)
        {
            var filterDef = Builders<FileSnooperPingData>.Filter.Gte("TimeSent", filter) & Builders<FileSnooperPingData>.Filter.Lt("TimeSent", DateTime.UtcNow);
            var results = await _azureCosmosDBService.GetListData(filterDef);
           
            if(results.Count < 2) 
            {
                await _sendGridService.SendEmailTest();
            }
        }
    }
}
