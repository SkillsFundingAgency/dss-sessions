using System.Net;
using System.Net.Http;
using System.Reflection;
using DFC.Functions.DI.Standard.Attributes;
using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Sessions.APIDefinition
{
    public class GenerateSessionSwaggerDoc
    {
        public const string ApiTitle = "Sessions";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "To support the Data Collections integration with DSS SubcontractorId has been added as an attribute.";
        public const string ApiVersion = "2.0.0";
        private ISwaggerDocumentGenerator _swaggerDocumentGenerator;
        private ILogger _logger;

        public GenerateSessionSwaggerDoc(ISwaggerDocumentGenerator swaggerDocumentGenerator, ILogger logger)
        {
            _swaggerDocumentGenerator = swaggerDocumentGenerator;
            _logger = logger;
        }

        [FunctionName(ApiDefinitionName)]
        public HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiDefRoute)]HttpRequest req)
        {
            var swagger = _swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription,
                ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
            {
                _logger.LogWarning("GenerateSessionSwaggerDoc HttpStatusCode.NoContent");
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swagger)
            };
        }
    }
}