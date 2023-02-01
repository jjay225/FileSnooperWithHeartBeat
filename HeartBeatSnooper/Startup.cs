using FileSnooper.Contracts.Services;
using HeartBeatSnooper.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(HeartBeatSnooper.Startup))]
namespace HeartBeatSnooper
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IAzureCosmosDBService, MongoAzureDBService>();
        }
    }
}
