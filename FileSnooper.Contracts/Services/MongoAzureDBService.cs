
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
            var db = Client.GetDatabase(_config.GetValue<string>("MongoDBName"));
            var collection = db.GetCollection<T>(_config.GetValue<string>("MongoDBCollection"));
           
            await collection.InsertOneAsync(data);
        }

        public async Task<List<T>> GetListData<T>(FilterDefinition<BsonDocument> fieldFilterDefinition)
        {
            var collection = Client.GetDatabase(_config.GetValue<string>("MongoDBName")).GetCollection<BsonDocument>(_config.GetValue<string>("MongoDBCollection"));         
            var docList = await collection.Find(fieldFilterDefinition).ToListAsync();

            var documentListTemp = await collection.FindAsync<T>(fieldFilterDefinition);
            var heartBeatList = documentListTemp?.ToList();

            return heartBeatList;        
        }

    }
}
