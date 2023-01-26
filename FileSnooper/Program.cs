using FileSnooper.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Polly;
using Polly.Contrib.WaitAndRetry;
using FileSnooper.Contracts.Classes;

namespace FileSnooper
{
    public class Program
    {
        private static string _contentRoute;
        private static string _azureHeartBeatServiceBase;
        public static void Main(string[] args)
        {
            var executablePath = Environment.CurrentDirectory + "\\";
            var exeFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var dirFullName = System.IO.Path.GetDirectoryName(exeFullName);
            _contentRoute = executablePath;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.File(@"c:\Logs\FileSnooper\Startup.txt")
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Logger.Information("Starting up the snooper from path {startUpPath}", _contentRoute);
                CreateHostBuilder(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "There was an error starting up the Snooper service");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void CreateHostBuilder(string[] args)
        {
        

            Host.CreateDefaultBuilder(args)
             .UseWindowsService()
             .UseContentRoot(_contentRoute)
             .UseSerilog((context, services, config) => config
                 .ReadFrom.Configuration(context.Configuration)
                 .Enrich.FromLogContext()
             )
             .ConfigureAppConfiguration((hostContext, config) =>
             {
                 var configBuilder =
                   config
                   .SetBasePath(_contentRoute)
                   .AddJsonFile($"{_contentRoute}apppsettings.json", optional: true, reloadOnChange: true)
                   .AddJsonFile($"{_contentRoute}appsettings.json.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true).Build();

                 
                 _azureHeartBeatServiceBase = configBuilder.GetValue<string>("AzureHeartBeatServiceHostBase");

             })           
             .ConfigureServices((hostContext, services) =>
             {
                 IConfiguration configuration = hostContext.Configuration;

                 services.Configure<List<FileMonitorPaths>>(configuration.GetSection("FileMonitorPaths"));

                 services.AddHostedService<SnooperWorker>()
                 .AddMemoryCache()
                 .AddSingleton<ISnooperService, SnooperService>()
                 .AddSingleton<ICacheService, CacheService>()
                 .AddSingleton<IAzureHeartBeatService, AzureHeartBeatService>()
                 .AddHttpClient(AzureHeartBeatService.ClientName, client =>
                 {
                     //if testing locally, change this to http if needed
                     client.BaseAddress = new Uri($"http://{_azureHeartBeatServiceBase}");
                 })
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3)));
             }).Build().Run();
        }
    }
}