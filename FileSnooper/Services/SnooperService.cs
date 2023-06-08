using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using FileSnooper.Helpers;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Options;
using FileSnooper.Contracts.Classes;
using System.Xml.Linq;

namespace FileSnooper.Services
{
    public class SnooperService : ISnooperService
    {
        private readonly ILogger<SnooperService> _logger;
        private readonly IConfiguration _config;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _pauseBetweenFailures = TimeSpan.FromMilliseconds(10000);
        private readonly int _pollyMaxRetryAttempts = 3;
        private readonly List<FileMonitorPaths> _fileMonitorPaths;

        private static AsyncRetryPolicy RetryPolicyAsync { get; set; }
        private string CurrentFileName { get; set; }

        private int RetryCount { get; set; }

        public SnooperService(
            ILogger<SnooperService> logger,
            IConfiguration config,
            ICacheService cacheService,
            IOptions<List<FileMonitorPaths>> fileMonitorPaths)
        {
            _logger = logger;
            _config = config;
            _cacheService = cacheService;
            _fileMonitorPaths = fileMonitorPaths.Value;
            
            SetupRetryPolicy();
        }

        public async Task InsertCacheItem<T>(string key, T value)
        {
            try
            {
                await _cacheService.InsertCacheItem<T>(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while inserting a cache item in method: {methodName}. Details: {error}", nameof(InsertCacheItem), ex.Message);
            }
        }

        public async Task UploadFilesInCache()
        {
            //Attempt to move files. Execute it in Polly Retry policy
            try
            {
                _logger.LogDebug("Proceeding to move files!");
                RetryCount = 0;
                await RetryPolicyAsync.ExecuteAsync(() => MoveFiles());
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while attempting to copy {fileName}, Error details: {error}", CurrentFileName, ex.Message);
            }
        }

        public void Ping()
        {
            Console.WriteLine("Hello!");
        }

        private async Task MoveFiles()
        {
            var listOfCacheEntries = await _cacheService.GetCacheAllCacheItems();
            string FileTargetPath = null;
            var pathCount = listOfCacheEntries?.Count;
            _logger.LogDebug("Found  {cacheEntries} cache entries to upload", pathCount);

            if (listOfCacheEntries?.Count > 0)
            {
                foreach (var cacheEntry in listOfCacheEntries)
                {
                    var CurrentFileName = cacheEntry.Key.ToString();
                    var CurrentFilePath = cacheEntry.Value.ToString();

                    if(!File.Exists(CurrentFilePath))
                    {
                        _logger.LogWarning("File {fileName} does not exist or is NOT an actual file. Removing it from cache.", CurrentFileName);
                        await _cacheService.RemoveCacheItem(CurrentFileName);
                    }
                    else
                    {

                        foreach (var item in _fileMonitorPaths)
                        {
                            if(PathComparer.HasSameRootFolder(filePath: CurrentFilePath, rootFolderPath: item.Source))
                            {
                                var monitoredRootPath = Path.GetFullPath(item.Source);
                                var relativePath = Path.GetRelativePath(monitoredRootPath, CurrentFilePath);

                                var destinationFilePath = Path.Combine(item.Target, relativePath);
                                var destinationDirectoryPath = Path.GetDirectoryName(destinationFilePath);

                                if (!Directory.Exists(destinationDirectoryPath))
                                {
                                    Directory.CreateDirectory(destinationDirectoryPath);
                                }

                                File.Copy(CurrentFilePath, destinationFilePath, overwrite: true);

                                _logger.LogInformation("File copied from {sourcePath} to {destinationPath}", CurrentFilePath, destinationFilePath);
                                await _cacheService.RemoveCacheItem(CurrentFileName);


                            }
                        }
                    }
                   
                }
            }
        }     

        private void SetupRetryPolicy()
        {
            RetryPolicyAsync = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(_pollyMaxRetryAttempts, i => _pauseBetweenFailures, onRetry: (ex, time) =>
                {
                    RetryCount++;
                    _logger.LogError(
                        "IO Exception! Details: {error}. Will Retry {maxRetryAttempt} times(s). Currently on number {retryCount} of retries. ",
                        ex.Message,
                        _pollyMaxRetryAttempts,
                        RetryCount);
                });
        }
    }
}