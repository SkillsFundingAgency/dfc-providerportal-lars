using Dfc.ProviderPortal.Lars.Common.DependencyInjection;
using Dfc.ProviderPortal.Lars.Functions;
using Dfc.ProviderPortal.Lars.Functions.FindAndExtract;
using Dfc.ProviderPortal.Lars.Functions.ImportCsv;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "A Web Jobs Extension Sample")]
namespace Dfc.ProviderPortal.Lars.Functions
{
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.Configure<FindAndExtractSettings>(configuration.GetSection(nameof(FindAndExtractSettings)));
            builder.Services.Configure<LarsCosmosDbCollectionSettings>(configuration.GetSection(nameof(LarsCosmosDbCollectionSettings)));
        }
    }
}
