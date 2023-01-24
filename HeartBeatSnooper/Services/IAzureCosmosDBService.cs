using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartBeatSnooper.Services
{
    public interface IAzureCosmosDBService
    {
        void Create<T>(T data);
    }
}
