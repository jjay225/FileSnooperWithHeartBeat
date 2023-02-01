using HeartBeatSnooperReader;
using Serilog;

var executablePath = Environment.CurrentDirectory + "\\";
var exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
var dirFullName = Path.GetDirectoryName(exeFullName);

executablePath = dirFullName + "/";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<SnooperReaderWorker>();
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        var configBuilder = config
        .AddJsonFile($"{executablePath}appsettings.json", optional: true, reloadOnChange: true)
        .Build();

        Log.Logger = new LoggerConfiguration()
                      .ReadFrom.Configuration(configBuilder)
                      .CreateLogger();
    })
    .UseSerilog()
    .Build();

await host.RunAsync();
