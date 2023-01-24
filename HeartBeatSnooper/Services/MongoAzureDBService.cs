using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartBeatSnooper.Services
{
    internal class MongoAzureDBService : IAzureCosmosDBService
    {
        public MongoAzureDBService()
        {
            
        }
        public void Create<T>(T data)
        {
            throw new NotImplementedException();
        }
    }
}
