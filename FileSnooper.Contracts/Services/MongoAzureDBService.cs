
using FileSnooper.Contracts.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileSnooper.Contracts.Services
{
    public class MongoAzureDBService : IAzureCosmosDBService
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
        public async Task Create<T>(T data)
        {
            var collection = Client.GetDatabase(_config.GetValue<string>("MongoDBName")).GetCollection<T>(_config.GetValue<string>("MongoDBCollection"));
          
            await collection.InsertOneAsync(data);
        }

        public async Task<List<T>> GetListData<T>(FilterDefinition<T> fieldFilterDefinition)
        {          
            var collection = Client.GetDatabase(_config.GetValue<string>("MongoDBName")).GetCollection<T>(_config.GetValue<string>("MongoDBCollection"));
            var results = collection.Find<T>(fieldFilterDefinition).ToList();

            return results;        
        }

    }
}
