# FileSnooperWithHeartBeat
A worker service that runs to monitor folders to back up your files and added azure heartbeat functionality to alert when the service goes down. The core project is automatic detection of file changes for backup to cloud. The heartbeat stuff was just added later for fun :smiley:

## How does it all work?

The main project is *FileSnooper*, this uses a worker service that runs at an interval of your choosing. When it starts up it looks at your config in *appsettings* like so: 

```JSON
{
 "FileMonitorPaths": [

    {
      "Source": "c:\\temp",
      "Target": "C:\\CopyOfTemp"
    },
    {
      "Source": "C:\\Dev",
      "Target": "C:\\Users\\Test"
    }

  ],
  "FileTypeToMonitor": "*.cs",  
  "SnoopDelayInMinutes": 2,
  "UseHeartService": false,
  "AzureHeartBeatServiceHostBase": "localhost:7262",
  "Identifier": "Snooper1",
  "AzureHeartBeatServiceFunctionUrlPath": "/api/HeartBeatChecker"
}
```

- *FileTypeToMonitor* : This can be a wild card or "*.docx", whichever file type you want to be monitored for changes etc. 
- *SnoopDelayInMinutes* : The delay for the worker service *SnooperWorker.cs*. So every say 10 minutes it will execute actions which I will discuss further down
- *UseHeartService*: Wether or not to use the Heartbeat service checker functionality which will be described in more detail below.
- *AzureHeartBeatServiceHostBase* : The base path of your Azure function in the project *HeartBeatSnooper*.
- *UseHeartService* : Identifies which File Snooper instance is sending a heartbeat/pulse to the Azure function
- *AzureHeartBeatServiceFunctionUrlPath* : API path for the Azure function.

