# FileSnooperWithHeartBeat
A worker service that runs to monitor folders to back up your files and added azure heartbeat functionality to alert when the service goes down. The heartbeat stuff was just for fun :smiley:

## How does it all work?

The main project is *FileSnooper*, this uses a worker service that runs at an interval of your choosing. When it starts up it looks at your config in *appsettings*
