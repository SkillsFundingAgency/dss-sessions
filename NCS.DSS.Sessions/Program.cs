using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddSingleton<IResourceHelper, ResourceHelper>();
        services.AddSingleton<IValidate, Validate>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
        services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
        services.AddSingleton<IJsonHelper, JsonHelper>();
        services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();

        services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddTransient<IGetSessionHttpTriggerService, GetSessionHttpTriggerService>();
        services.AddTransient<IGetSessionByIdHttpTriggerService, GetSessionByIdHttpTriggerService>();
        services.AddTransient<IPostSessionHttpTriggerService, PostSessionHttpTriggerService>();
        services.AddTransient<IPatchSessionHttpTriggerService, PatchSessionHttpTriggerService>();
        services.AddTransient<ISessionPatchService, SessionPatchService>();
        services.AddScoped<IAzureMapService, AzureMapService>();
        services.AddScoped<IGeoCodingService, GeoCodingService>();

        services.AddSingleton<IDynamicHelper, DynamicHelper>();
    })
    .Build();

host.Run();
