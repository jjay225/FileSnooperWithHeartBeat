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
        private readonly IAzureCosmosDBService _azureCosmosDBService;

        public SnooperHeartbeatCompareService(IAzureCosmosDBService azureCosmosDBService)
        {
            _azureCosmosDBService = azureCosmosDBService;
        }
        public async Task<List<FileSnooperPingData>> GetLatestPingDataByInterval(DateTime filter)
        {
            var filterDef = Builders<FileSnooperPingData>.Filter.Gte("TimeSent", filter) & Builders<FileSnooperPingData>.Filter.Lte("TimeSent", DateTime.UtcNow);
            var results = await _azureCosmosDBService.GetListData<FileSnooperPingData>(filterDef);
            foreach (var result in results)
            {
                await Console.Out.WriteLineAsync($"Date Sent: {result.TimeSent}");
            }


            throw new NotImplementedException();
        }
    }
}
