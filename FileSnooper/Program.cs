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

namespace FileSnooper
{
    public class Program
    {
        private static string _contentRoute;
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.File(@"c:\Logs\FileSnooper\Startup.txt")
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {

                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                _contentRoute = Path.GetDirectoryName(pathToExe);

                Log.Logger.Information("Starting up the snooper from path {startUpPath}", _contentRoute);
                CreateHostBuilder(args).Build().Run();
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

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
        

            return Host.CreateDefaultBuilder(args)
             .UseWindowsService()
             .UseContentRoot(_contentRoute)
             .UseSerilog((context, services, config) => config
                 .ReadFrom.Configuration(context.Configuration)
                 .Enrich.FromLogContext()
             )
             .ConfigureAppConfiguration((hostContext, config) =>
                    config
                    .AddJsonFile($"{_contentRoute}apppsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"{_contentRoute}appsettings.json.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                )
             .ConfigureServices((hostContext, services) =>
             {
                 services.AddHostedService<SnooperWorker>()
                 .AddMemoryCache()
                 .AddSingleton<ISnooperService, SnooperService>()
                 .AddSingleton<ICacheService, CacheService>();
             });
        }
    }
}