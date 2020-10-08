using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Sessions;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GeoCoding;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.Sessions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IValidate, Validate>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();

            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<IGetSessionHttpTriggerService, GetSessionHttpTriggerService>();
            builder.Services.AddTransient<IGetSessionByIdHttpTriggerService, GetSessionByIdHttpTriggerService>();
            builder.Services.AddTransient<IPostSessionHttpTriggerService, PostSessionHttpTriggerService>();
            builder.Services.AddTransient<IPatchSessionHttpTriggerService, PatchSessionHttpTriggerService>();
            builder.Services.AddTransient<ISessionPatchService, SessionPatchService>();

            builder.Services.AddScoped<IGeoCodingService, GeoCodingService>();
            builder.Services.AddScoped<IAzureMapService, AzureMapService>();
        }
    }
}
