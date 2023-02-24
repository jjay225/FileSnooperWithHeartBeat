# FileSnooperWithHeartBeat
A worker service that runs to monitor folders to back up your files and added azure heartbeat functionality to alert when the service goes down. The core project is automatic detection of file changes for backup to cloud. The heartbeat stuff was just added later for fun :smiley:

## How does it all fit together?

The main project is **_FileSnooper_**, this uses FileSytemWatchers and a Worker Service that runs at an interval of your choosing. It monitors for file system changes, stores them in a cache and uploads or copies them to a folder of your choosing, more on this later below. When it starts up it looks at your config in *appsettings* like so: 

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
- *FileMonitorPaths* : An array of Source path to monitor and a Target path to copy the changed/created file to
- *FileTypeToMonitor* : This can be a wild card or "*.docx", whichever file type you want to be monitored for changes etc. 
- *SnoopDelayInMinutes* : The delay for the worker service *SnooperWorker.cs*. So every say 10 minutes it will execute actions which I will discuss further down
- *UseHeartService*: Whether or not to use the Heartbeat service checker functionality which will be described in more detail below.
- *AzureHeartBeatServiceHostBase* : The base path of your Azure function in the project *HeartBeatSnooper*.
- *UseHeartService* : Identifies which File Snooper instance is sending a heartbeat/pulse to the Azure function
- *AzureHeartBeatServiceFunctionUrlPath* : API path for the Azure function.

---

The second project is **_HeartBeatSnooper_** and this is an Azure Function with a Http Trigger. It receive a *Heartbeat*/*Pulse*/*Ping* from the FileSnooper project and sends that to a cloud db for storage. Currently using a MongoDB API on CosmosDB in Azure.

