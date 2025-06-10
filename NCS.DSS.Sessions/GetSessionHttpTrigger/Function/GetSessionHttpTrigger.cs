using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.GetSessionHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger.Function
{
    public class GetSessionHttpTrigger
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IGetSessionHttpTriggerService _sessionGetService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly ILogger<GetSessionHttpTrigger> _logger;

        public GetSessionHttpTrigger(
            ICosmosDBProvider cosmosDBProvider,
            IGetSessionHttpTriggerService sessionGetService,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            ILogger<GetSessionHttpTrigger> logger)
        {
            _cosmosDbProvider = cosmosDBProvider;
            _sessionGetService = sessionGetService;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            this._logger = logger;
        }

        [Function("GET")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Sessions Retrieved", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Get request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [ProducesResponseType(typeof(Models.Session), 200)]
        [Display(Name = "Get", Description = "Ability to get a session object for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/interactions/{interactionId}/sessions")] HttpRequest req, string customerId, string interactionId)
        {
            var functionName = nameof(GetSessionHttpTrigger);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to locate 'TouchpointId' in request header",correlationId,response.StatusCode);
                return response;
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}", correlationId, response.StatusCode,customerId);
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = new BadRequestObjectResult(interactionGuid);
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Unable to parse 'interactionId' to a Guid: {interactionId}", correlationId, response.StatusCode,interactionId);
                return response;
            }
            _logger.LogInformation("{CorrelationId} Input validation has succeeded.", correlationId);

            _logger.LogInformation("{CorrelationId} Attempting to see if customer exists {customerGuid}", correlationId, customerGuid);
            var doesCustomerExist = await _cosmosDbProvider.DoesCustomerResourceExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Customer does not exist {customerGuid}", correlationId, response.StatusCode,customerGuid);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Customer record found in Cosmos DB {customerGuid}", correlationId, customerGuid);
            }

            _logger.LogInformation("{CorrelationId} Attempting to see if interaction exists {interactionGuid}", correlationId, interactionGuid);
            var doesInteractionExist = await _cosmosDbProvider.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Interaction does not exist {interactionGuid}", correlationId, response.StatusCode,interactionGuid);
                return response;
            }
            else
            {
                _logger.LogInformation("{CorrelationId} Interaction record with {interactionGuid} found in Cosmos DB for Customer {customerGuid}", correlationId, interactionGuid, customerGuid);
            }

            _logger.LogInformation("{CorrelationId} Attempting to get sessions for customer {customerGuid}", correlationId, customerGuid);
            var sessions = await _sessionGetService.GetSessionsAsync(customerGuid);

            if (sessions == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning("{CorrelationId} Response Status Code: {StatusCode}. Sessions do not exist {interactionGuid}", correlationId, response.StatusCode,interactionGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
            else
            {
                var response = (sessions.Count == 1) ? new JsonResult(sessions[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                } : new JsonResult(sessions, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
                _logger.LogInformation("{CorrelationId} Response Status Code: {StatusCode}. Get sessions succeeded for customer {customerGuid}", correlationId,response.StatusCode, customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
                return response;
            }
        }

    }
}
