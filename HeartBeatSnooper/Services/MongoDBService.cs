using HeartBeatSnooper.Workers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartBeatSnooper.Services
{
    internal class MongoDBService
    {
        private static readonly MongoClient _client;
        static MongoDBService()
        {
            _client = new MongoClient(SettingsWorker.GetEnvironmentVariable("MongoConnectionString"));
        }

        internal static MongoClient GetClient()
        {
            return _client;
        }

        internal static void Create<T>(T data)
        {
            var db = GetClient().GetDatabase(SettingsWorker.GetEnvironmentVariable("MongoDBName"));
            var collection = db.GetCollection<T>("HeartBeatDetails");

            collection.InsertOne(data);
        }
    }
}
