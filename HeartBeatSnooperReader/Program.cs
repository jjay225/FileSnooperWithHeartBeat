using FileSnooper.Contracts.Services;
using HeartBeatSnooperReader;
using HeartBeatSnooperReader.Services;
using SendGrid.Extensions.DependencyInjection;
using Serilog;

var executablePath = Environment.CurrentDirectory + "\\";
var exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
var dirFullName = Path.GetDirectoryName(exeFullName);
var sendGridApiKey = "";

executablePath = dirFullName + "/";

IHost host = Host.CreateDefaultBuilder(args)
     .ConfigureAppConfiguration((hostContext, config) =>
     {
         var configBuilder = config
         .AddJsonFile($"{executablePath}appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
         .Build();

         sendGridApiKey = configBuilder.GetValue<string>("SendGridApiKey");

         Log.Logger = new LoggerConfiguration()
                       .ReadFrom.Configuration(configBuilder)
                       .CreateLogger();
     })
    .ConfigureServices(services =>
    {
        services.AddHostedService<SnooperReaderWorker>()
        .AddScoped<IAzureCosmosDBService, MongoAzureDBService>()
        .AddScoped<ISnooperHeartbeatCompareService, SnooperHeartbeatCompareService>()
        .AddScoped<ISendGridService, SendGridService>()
        .AddSendGrid(options => 
        { 
            options.ApiKey = sendGridApiKey;
        });
    })   
    .UseSerilog()
    .Build();

await host.RunAsync();
