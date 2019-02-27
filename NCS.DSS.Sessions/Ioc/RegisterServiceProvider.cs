using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionByIdHttpTrigger.Service;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.PatchSessionHttpTrigger.Service;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Service;
using NCS.DSS.Sessions.Validation;


namespace NCS.DSS.Sessions.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddTransient<IGetSessionHttpTriggerService, GetSessionHttpTriggerService>();
            services.AddTransient<IGetSessionByIdHttpTriggerService, GetSessionByIdHttpTriggerService>();
            services.AddTransient<IPostSessionHttpTriggerService, PostSessionHttpTriggerService>();
            services.AddTransient<IPatchSessionHttpTriggerService, PatchSessionHttpTriggerService>();
            services.AddTransient<ISessionPatchService, SessionPatchService>();

            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();

            return services.BuildServiceProvider(true);
        }
    }
}
