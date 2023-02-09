using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSnooper.Contracts.Services
{
    public interface IAzureCosmosDBService
    {
        Task Create<T>(T data);

        Task<List<T>> GetListData<T>(FilterDefinition<T> fieldFilterDefinition);
    }
}
