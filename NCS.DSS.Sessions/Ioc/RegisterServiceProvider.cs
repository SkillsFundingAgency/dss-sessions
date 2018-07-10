using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Sessions.Cosmos.Helper;
using NCS.DSS.Sessions.Helpers;
using NCS.DSS.Sessions.PostSessionHttpTrigger.Function;
using NCS.DSS.Sessions.Validation;


namespace NCS.DSS.Sessions.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IPostSessionHttpTriggerService, PostSessionHttpTriggerService>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            return services.BuildServiceProvider(true);
        }
    }
}
