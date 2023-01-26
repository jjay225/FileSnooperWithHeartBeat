using FileSnooper.Contracts.Classes;
using FileSnooper.Helpers;
using FileSnooper.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSnooper
{
    public class SnooperWorker : BackgroundService
    {
        private readonly ILogger<SnooperWorker> _logger;
        private readonly ISnooperService _snooperService;
        private readonly IConfiguration _config;
        private readonly IAzureHeartBeatService _azureHeartBeatService;
        private readonly int _snoopDelayInMinutes;
        private readonly List<FileMonitorPaths> _fileMonitorPaths;
        private FileSystemWatcher FileSnooper1 { get; set; }
        private FileSystemWatcher FileSnooper2 { get; set; }
        private FileSystemWatcher FileSnooper3 { get; set; }
        private string FileTypeToMonitor { get; set; }
        private int PathCount { get; set; }

        public SnooperWorker(
            ILogger<SnooperWorker> logger,
            ISnooperService snooperService,
            IConfiguration config,
            IAzureHeartBeatService azureHeartBeatService,
            IOptions<List<FileMonitorPaths>> fileMonitorPaths)
        {
            _logger = logger;
            _snooperService = snooperService;
            _config = config;
            _azureHeartBeatService = azureHeartBeatService;
            
            _snoopDelayInMinutes = _config.GetValue<int>("SnoopDelayInMinutes");
            _fileMonitorPaths = fileMonitorPaths.Value;            

            FileTypeToMonitor = _config.GetValue<string>("FileTypeToMonitor");            
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service startup.");

            PathCount = _fileMonitorPaths.Count;

            _logger.LogInformation("Snooping delay is {delay} minute(s), we are monitoring {pathCount} path(s)", _snoopDelayInMinutes, PathCount);

            if (_snoopDelayInMinutes == 0 && PathCount == 0)
            {
                _logger.LogError("Wooah! No settings from AppSettings have been set. Probable path issue on startup!");
                throw new Exception("Path exception! No settings loaded");
            }

            if (PathCount > 3)
            {
                _logger.LogError("Only 3 File System Watchers are allowed, you have {watcherCount} watcher(s)", PathCount);
                throw new Exception("Cannot proceed with start, to many file paths specified");
            }

            LoopThroughPathsAndSetupWatchers(_fileMonitorPaths.Select(x => x.Source).ToList());

            SetupChangedEvents();
            SetupCreatedEvents();
            //SetupDeletedEvents();//keeping for later maybe
            SetupRenamedEvents();

            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");

            await base.StopAsync(cancellationToken);
        }

        private void SetupCreatedEvents()
        {
            if (FileSnooper1 != null)
            {
                FileSnooper1.Created += FileSnooper_Created;
            }

            if (FileSnooper2 != null)
            {
                FileSnooper2.Created += FileSnooper_Created;
            }

            if (FileSnooper3 != null)
            {
                FileSnooper3.Created += FileSnooper_Created;
            }
        }

        private void SetupChangedEvents()
        {
            if (FileSnooper1 != null)
            {
                FileSnooper1.Changed += FileSnooper_Changed;
            }

            if (FileSnooper2 != null)
            {
                FileSnooper2.Changed += FileSnooper_Changed;
            }

            if (FileSnooper3 != null)
            {
                FileSnooper3.Changed += FileSnooper_Changed;
            }
        }

        private void SetupDeletedEvents()
        {
            if (FileSnooper1 != null)
            {
                FileSnooper1.Deleted += FileSnooper_Deleted;
            }

            if (FileSnooper2 != null)
            {
                FileSnooper2.Deleted += FileSnooper_Deleted;
            }

            if (FileSnooper3 != null)
            {
                FileSnooper3.Deleted += FileSnooper_Deleted;
            }
        }

        private void SetupRenamedEvents()
        {
            if (FileSnooper1 != null)
            {
                FileSnooper1.Renamed += FileSnooper_Renamed;
            }

            if (FileSnooper2 != null)
            {
                FileSnooper2.Renamed += FileSnooper_Renamed;
            }

            if (FileSnooper3 != null)
            {
                FileSnooper3.Renamed += FileSnooper_Renamed;
            }
        }

        private async void FileSnooper_Changed(object sender, FileSystemEventArgs e)
        {
            if (CheckIfTmpFile(e.FullPath)) return;
            
            _logger.LogDebug("Changed event fired for {path}", e.FullPath);
            
            var key = Path.GetFileName(e.FullPath);

            await _snooperService.InsertCacheItem(key, e.FullPath);
        }

        private async void FileSnooper_Created(object sender, FileSystemEventArgs e)
        {
            if (CheckIfTmpFile(e.FullPath)) return;
            
            _logger.LogDebug("Created event fired for {path}", e.FullPath);
            
            var key = Path.GetFileName(e.FullPath);
            await _snooperService.InsertCacheItem(key, e.FullPath);
        }

        private async void FileSnooper_Deleted(object sender, FileSystemEventArgs e)
        {
            if (CheckIfTmpFile(e.FullPath)) return;
            
            _logger.LogDebug("Deleted event fired for {path}", e.FullPath);
            
            var key = Path.GetFileName(e.FullPath);
            await _snooperService.InsertCacheItem(key, e.FullPath);
        }

        private async void FileSnooper_Renamed(object sender, FileSystemEventArgs e)
        {
            if (CheckIfTmpFile(e.FullPath)) return;
            
            _logger.LogDebug("Renamed event fired for {path}", e.FullPath);
            
            var key = Path.GetFileName(e.FullPath);
            await _snooperService.InsertCacheItem(key, e.FullPath);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(_snoopDelayInMinutes), stoppingToken);                
                await _snooperService.UploadFilesInCache();
                await _azureHeartBeatService.Pulse();
            }
        }

        private void LoopThroughPathsAndSetupWatchers(List<string> paths)
        {
            for (int i = 0; i < PathCount; i++)
            {
                switch (i)
                {
                    case 0:
                        FileSnooper1 = FileSnooperFactory.BuildFileSystemWatchAndSetupPaths(paths[i], fileTypes: FileTypeToMonitor);
                        break;

                    case 1:
                        FileSnooper2 = FileSnooperFactory.BuildFileSystemWatchAndSetupPaths(paths[i], fileTypes: FileTypeToMonitor);
                        break;

                    case 2:
                        FileSnooper3 = FileSnooperFactory.BuildFileSystemWatchAndSetupPaths(paths[i], fileTypes: FileTypeToMonitor);
                        break;

                    default:
                        break;
                }
            }
        }

        private static bool CheckIfTmpFile(string fullPath)
        {
            var ext = Path.GetExtension(fullPath);
            if (ext.ToLower() == ".tmp")
            {
                return true;
            }

            return false;
        }
    }
}