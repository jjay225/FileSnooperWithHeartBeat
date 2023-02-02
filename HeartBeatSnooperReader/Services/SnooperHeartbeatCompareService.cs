using FileSnooper.Contracts.Classes;
using MongoDB.Bson;
using MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace HeartBeatSnooperReader.Services
{
    public class SnooperHeartbeatCompareService : ISnooperHeartbeatCompareService
    {
        public Task<List<FileSnooperPingData>> GetLatestPingDataByInterval(DateTime filter)
        {
            var dateTest = BsonValue.Create(filter);
            var filterDef = Builders<BsonDocument>.Filter.Gt("TimeSent", dateTest);
            filterDef &= Builders<BsonDocument>.Filter.Lt("TimeSent", dateTest);
            //OR Builders<BsonDocument>.Filter.Gt("TimeSent", dateTest);

            throw new NotImplementedException();
        }
    }
}
