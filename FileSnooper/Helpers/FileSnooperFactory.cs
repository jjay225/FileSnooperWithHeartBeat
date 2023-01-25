using FileSnooper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileSnooper.Helpers
{
    internal class FileSnooperFactory
    {           

        internal static FileSystemWatcher BuildFileSystemWatchAndSetupPaths(string Path, string fileTypes)
        {
            return new FileSystemWatcher(Path)
            {
                IncludeSubdirectories = true,
                Filter = fileTypes,
                EnableRaisingEvents = true
            };
        }
    }
}