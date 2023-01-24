using HeartBeatSnooper.Workers;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartBeatSnooper.Services
{
    internal class MongoAzureDBService : IAzureCosmosDBService
    {
        private readonly IConfiguration _config;
        private readonly MongoClient _client;

        public MongoAzureDBService(IConfiguration config)
        {
            _config = config;//future change this to the IOptions pattern for config
            var connString = _config.GetValue<string>("MongoConnectionString");
            _client = new MongoClient(connString);
        }
        public void Create<T>(T data)
        {
            var db = _client.GetDatabase(_config.GetValue<string>("MongoDBName"));
            var collection = db.GetCollection<T>(_config.GetValue<string>("MongoDBCollection"));

            collection.InsertOne(data);
        }
    }
}
