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

---

The third project is **_HeartBeatSnooperReader_** is also a Worker service which runs in docker. Didn't have to be docker but just felt like experimenting running this in Azure as a Container Instance. It's config is as follows:

```JSON
{
  "WorkerServiceDelayInMinutes": 2,
  "MinutesToFilterBack": 30,
  "MongoConnectionString": "",
  "MongoDBName": "",
  "MongoDBCollection": "",
  "SendGridApiKey": "",
  "SendGridEmailAddress": "",
  "SendGridEmailAddresses": "",
  "SendGridEmailName": ""
}

```

- *MinutesToFilterBack* : So what this setting does is u specify how far to query back in time on the cloud DB for retrieving heartbeat data linked to intervals you would like to query.
- *WorkerServiceDelayInMinutes* : This is used in conjunction with *MinutesToFilterBack*. So say you send a Heartbeat every 5 minutes from the *FileSnooper* project, you can then set this WorkerServiceDelayInMinutes to fire off every 12 minutes for instance and search back say 12 minutes with the *MinutesToFilterBack* and then you would find two records in the cloud DB. If you don't find two records that could mean the FileSnooper service is down because it should have sent two Heartbeats from whatever pc it is running on in a 10 minute window because it sends every 5 minutes.
- *SendGrid* : SendGrid is a useful service to send emails which is used for the alerts here, works well, easy to setup.

---

## Architecture notes

#### FileSnooper
This project uses the FileSystemWatcher class to monitor changes in files and folders you would have specified and the appsettings.json file. Everytime it detects a change it adds the file and path to a cache, specifically _Microsoft.Extensions.Caching.Memory_. Instead of automatically copying this file while you could be working on it the save is currently happening, it waits for the worker interval in your appsettings.json to trigger a copy.

It also caters for the way windows saves files, so windows actually saves file by creating a .tmp file and saving it to that and then renaming it. So the cache system won't go add a bunch of .tmp files to it's store and then try upload those and they won't actually exist. It currently only caters for 3 FileSystemWatchers and paths

### HeartBeatSnooper
This is an Azure Function with a Http Trigger. It's use for the _FileSnooper_ project to send Heartbeats to, that in turn saves the Heartbeat data Azure CosmosDB using the MongoDB API. That API can swapped out for others too. This function is slightly different from the generic project template because it adds a Startup class for dependency injection and changes the main function class to non static and injects some stuff we need.

### HeartBeatSnooperReader
This is also a worker service project but containerised with Docker, doesn't have to be but just felt like trying it out. It's containerised to run in Azure as a Container Instance. This reads from the Azure Cosmos DB to check that a certain amount of Heartbeats were received and if they weren't it could mean that the _FileSnooper_ service is down.


