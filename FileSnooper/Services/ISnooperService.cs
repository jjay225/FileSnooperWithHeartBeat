using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileSnooper.Services
{
    public interface ISnooperService
    {
        void Ping();

        Task InsertCacheItem<T>(string key, T value);

        Task UploadFilesInCache();
    }
}