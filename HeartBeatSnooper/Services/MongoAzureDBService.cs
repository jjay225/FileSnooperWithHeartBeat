using HeartBeatSnooper.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private MongoClient _client;
        private MongoClient Client
        {
            get => _client ??= new MongoClient(_connString);
            
            set => _client = value;            
        }
         
        private readonly string _connString;

        public MongoAzureDBService(IConfiguration config)
        {
            _config = config;//future change this to the IOptions pattern for config
            _connString = _config.GetValue<string>("MongoConnectionString");
            
        }
        public void Create<T>(T data)
        {    
            var db = Client.GetDatabase(_config.GetValue<string>("MongoDBName"));
            var collection = db.GetCollection<T>(_config.GetValue<string>("MongoDBCollection"));

            collection.InsertOne(data);
        }
    }
}
