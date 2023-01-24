using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartBeatSnooper.Services
{
    interface IAzureCosmosDBService
    {
        public void Create<T>(T data);
    }
}
