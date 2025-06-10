using Azure.Messaging.ServiceBus;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
using NCS.DSS.Sessions.ServiceBus;
using NCS.DSS.Sessions.Validation;
namespace NCS.DSS.Sessions
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.SetBasePath(Environment.CurrentDirectory)
                        .AddJsonFile("local.settings.json", optional: true,
                            reloadOnChange: false)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<SessionsConfigurationSettings>()
                        .Bind(configuration);
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddLogging();
                    services.AddSingleton<IValidate, Validate>();
                    services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
                    services.AddSingleton<ICosmosDBProvider, CosmosDBProvider>();
                    services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddTransient<IGetSessionHttpTriggerService, GetSessionHttpTriggerService>();
                    services.AddTransient<IGetSessionByIdHttpTriggerService, GetSessionByIdHttpTriggerService>();
                    services.AddTransient<IPostSessionHttpTriggerService, PostSessionHttpTriggerService>();
                    services.AddTransient<IPatchSessionHttpTriggerService, PatchSessionHttpTriggerService>();
                    services.AddSingleton<IJsonHelper, JsonHelper>();
                    services.AddTransient<ISessionPatchService, SessionPatchService>();
                    services.AddScoped<IAzureMapService, AzureMapService>();
                    services.AddScoped<IGeoCodingService, GeoCodingService>();
                    services.AddSingleton(sp =>
                    {
                        var settings = sp.GetRequiredService<IOptions<SessionsConfigurationSettings>>().Value;
                        var options = new CosmosClientOptions()
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };
                        return new CosmosClient(settings.SessionConnectionString, options);
                    });
                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();
                    services.AddScoped<ISessionsServiceBusClient, SessionsServiceBusClient>();
                    services.AddSingleton(serviceProvider =>
                    {
                        var settings = serviceProvider.GetRequiredService<IOptions<SessionsConfigurationSettings>>().Value;
                        return new ServiceBusClient(settings.ServiceBusConnectionString);
                    });
                })
                .Build();

            await host.RunAsync();

        }
    }
}
